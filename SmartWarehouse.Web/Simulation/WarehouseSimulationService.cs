using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Domain.Entities;
using SmartWarehouse.Domain.Enums;
using SmartWarehouse.Infrastructure.Persistence;
using SmartWarehouse.Web.Hubs;

namespace SmartWarehouse.Web.Simulation;

public sealed class WarehouseSimulationService(
    IHubContext<WarehouseHub> hub,
    IServiceScopeFactory scopeFactory,
    ILogger<WarehouseSimulationService> logger) : BackgroundService, ISimulationRunner
{
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private int _busy;

    public async Task<bool> QueueAsync(string flow, CancellationToken cancellationToken = default)
    {
        if (flow is not ("inbound" or "outbound") || Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return false;
        try { await _queue.Writer.WriteAsync(flow, cancellationToken); return true; }
        catch { Volatile.Write(ref _busy, 0); throw; }
    }

    public async Task InjectFaultAsync(CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref _busy) != 0) throw new InvalidOperationException("Hãy chờ quy trình đang chạy kết thúc trước khi mô phỏng lỗi.");
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var now = DateTimeOffset.UtcNow;
        var conveyor = await db.Devices.SingleOrDefaultAsync(x => x.Code == "CONV-01", cancellationToken);
        if (conveyor is not null) { conveyor.Status = DeviceStatus.Error; conveyor.LastSeenAt = now; }
        db.DeviceAlerts.Add(new DeviceAlert { Id = Guid.NewGuid(), DeviceCode = "CONV-01", Message = "Mô phỏng băng tải bị kẹt — cần kiểm tra cảm biến.", CreatedAt = now });
        db.DeviceEvents.Add(new DeviceEvent { Id = Guid.NewGuid(), DeviceCode = "CONV-01", EventType = "SimulatedFault", Message = "Băng tải bị kẹt tại cảm biến chuyển hướng.", OccurredAt = now });
        await db.SaveChangesAsync(cancellationToken);
        await PublishAsync(new SimulationUpdate("fault", "error", 0, "Băng tải bị kẹt. Hệ thống đã tạo cảnh báo; chức năng an toàn phải dừng tại bộ điều khiển phần cứng.", null, null, DateTimeOffset.UtcNow), cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref _busy) != 0) throw new InvalidOperationException("Không thể đặt lại thiết bị khi quy trình đang chạy.");
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var now = DateTimeOffset.UtcNow;
        var devices = await db.Devices.Where(x => x.Status == DeviceStatus.Error || x.Status == DeviceStatus.EmergencyStop).ToListAsync(cancellationToken);
        foreach (var device in devices) { device.Status = DeviceStatus.Idle; device.LastSeenAt = now; }
        db.DeviceEvents.Add(new DeviceEvent { Id = Guid.NewGuid(), DeviceCode = "SYSTEM", EventType = "SimulationReset", Message = "Đặt lại trạng thái thiết bị mô phỏng.", OccurredAt = now });
        await db.SaveChangesAsync(cancellationToken);
        await PublishAsync(new SimulationUpdate("system", "reset", 0, "Thiết bị mô phỏng đã được đặt về trạng thái chờ.", null, null, now), cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var flow in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try { await RunFlowAsync(flow, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception, "Simulation flow {Flow} failed", flow);
                await PublishAsync(new SimulationUpdate(flow, "error", 0, "Quy trình mô phỏng thất bại. Hãy kiểm tra kết nối database và nhật ký ứng dụng.", null, null, DateTimeOffset.UtcNow), stoppingToken);
            }
            finally { Volatile.Write(ref _busy, 0); }
        }
    }

    private async Task RunFlowAsync(string flow, CancellationToken cancellationToken)
    {
        var inbound = flow == "inbound";
        var steps = inbound
            ? new[] { ("receiving", "Kiện hàng đến khu tiếp nhận", "box-rfid"), ("rfid", "Đầu đọc RFID nhận diện EPC", "box-rfid"), ("conveyor", "Băng tải chuyển kiện hàng", "box-transfer"), ("agv", "AGV nhận nhiệm vụ vận chuyển", "box-agv"), ("storage", "AGV bàn giao tại vị trí A-01-01", "box-storage") }
            : new[] { ("order", "Kiểm tra đơn và tồn khả dụng", "box-storage"), ("agv", "AGV nhận nhiệm vụ lấy hàng", "box-agv"), ("conveyor", "Băng tải đưa hàng đến cổng xuất", "box-transfer"), ("rfid", "RFID xác nhận EPC xuất kho", "box-rfid"), ("dispatch", "Hoàn tất xuất kho", "box-outbound") };
        var epc = "E2806894A000000000000001";
        var taskNumber = $"AGV-{DateTime.UtcNow:HHmmss}-{Guid.NewGuid():N}"[..20];
        using var scope = scopeFactory.CreateScope();
        var operations = scope.ServiceProvider.GetRequiredService<IWarehouseOperations>();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var agvTask = new AgvTask { Id = Guid.NewGuid(), TaskNumber = taskNumber, Status = DeviceTaskStatus.Queued, PickupPoint = inbound ? "RECEIVING" : "A-01-01", DropoffPoint = inbound ? "A-01-01" : "DISPATCH", BatteryPercent = 96, CreatedAt = DateTimeOffset.UtcNow };
        db.AgvTasks.Add(agvTask);
        await db.SaveChangesAsync(cancellationToken);
        var readEventKey = $"sim-rfid-{flow}-{Guid.NewGuid():N}";
        await PublishAsync(new SimulationUpdate(flow, "running", 0, "Quy trình đã bắt đầu.", epc, taskNumber, DateTimeOffset.UtcNow), cancellationToken);
        for (var i = 0; i < steps.Length; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1100), cancellationToken);
            var step = steps[i];
            var now = DateTimeOffset.UtcNow;
            if (step.Item1 == "conveyor")
            {
                db.ConveyorTasks.Add(new ConveyorTask { Id = Guid.NewGuid(), TaskNumber = $"CV-{Guid.NewGuid():N}"[..20], Status = DeviceTaskStatus.InProgress, SourcePoint = inbound ? "RECEIVING" : "A-01-01", DestinationPoint = inbound ? "A-01-01" : "DISPATCH", CreatedAt = now });
                var conveyor = await db.Devices.SingleOrDefaultAsync(x => x.Code == "CONV-01", cancellationToken);
                if (conveyor is not null) { conveyor.Status = DeviceStatus.Running; conveyor.LastSeenAt = now; }
            }
            if (step.Item1 == "agv")
            {
                agvTask.Status = DeviceTaskStatus.InProgress;
                var agv = await db.Devices.SingleOrDefaultAsync(x => x.Code == "AGV-01", cancellationToken);
                if (agv is not null) { agv.Status = DeviceStatus.Running; agv.LastSeenAt = now; }
            }
            if (step.Item1 == "rfid")
            {
                var reader = await db.Devices.SingleOrDefaultAsync(x => x.Code == (inbound ? "RFID-IN-01" : "RFID-OUT-01"), cancellationToken);
                if (reader is not null) { reader.Status = DeviceStatus.Running; reader.LastSeenAt = now; }
                var read = await operations.ProcessRfidReadAsync(epc, inbound ? "RFID-IN-01" : "RFID-OUT-01",
                    inbound ? "GATE-IN" : "GATE-OUT", readEventKey, cancellationToken);
                if (!read.IsAccepted || read.Sku != "DEMO-001")
                    throw new InvalidOperationException(read.RejectionReason ?? "EPC không khớp với hàng hóa trong luồng.");
            }
            db.DeviceEvents.Add(new DeviceEvent { Id = Guid.NewGuid(), DeviceCode = step.Item1 == "rfid" ? (inbound ? "RFID-IN-01" : "RFID-OUT-01") : step.Item1 == "conveyor" ? "CONV-01" : "AGV-01", EventType = step.Item1, Message = step.Item2, CorrelationId = taskNumber, OccurredAt = now });
            await db.SaveChangesAsync(cancellationToken);
            await PublishAsync(new SimulationUpdate(flow, step.Item1, (i + 1) * 100 / steps.Length, step.Item2, epc, taskNumber, now), cancellationToken);
        }

        agvTask.Status = DeviceTaskStatus.Completed;
        var completedAt = DateTimeOffset.UtcNow;
        foreach (var device in await db.Devices.Where(x => x.Code == "AGV-01" || x.Code == "CONV-01" || x.Code == (inbound ? "RFID-IN-01" : "RFID-OUT-01")).ToListAsync(cancellationToken))
        {
            device.Status = DeviceStatus.Idle;
            device.LastSeenAt = completedAt;
        }
        foreach (var task in await db.ConveyorTasks.Where(x => x.Status == DeviceTaskStatus.InProgress).ToListAsync(cancellationToken)) task.Status = DeviceTaskStatus.Completed;
        await db.SaveChangesAsync(cancellationToken);
        var key = $"sim-{flow}-{Guid.NewGuid():N}";
        var request = new StockOperationRequest("DEMO-001", "A-01-01", 1,
            $"SIM-{(inbound ? "IN" : "OUT")}-{DateTime.UtcNow:yyyyMMddHHmmss}", key);
        if (inbound)
            await operations.ReceiveStockAsync(request, cancellationToken);
        else
        {
            await operations.ReserveStockAsync(request, cancellationToken);
            await operations.DispatchStockAsync(request with { IdempotencyKey = $"{key}-dispatch" }, cancellationToken);
        }
        await PublishAsync(new SimulationUpdate(flow, "completed", 100, inbound ? "Nhập kho hoàn tất. Tồn kho đã được cập nhật." : "Xuất kho hoàn tất. Tồn kho đã được cập nhật.", epc, taskNumber, DateTimeOffset.UtcNow), cancellationToken);
    }

    private Task PublishAsync(SimulationUpdate update, CancellationToken cancellationToken) =>
        hub.Clients.All.SendAsync("SimulationStep", update, cancellationToken);
}







