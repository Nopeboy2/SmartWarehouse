using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Web.Simulation;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin,WarehouseOperator")]
public sealed class SimulationController(ISimulationRunner simulation) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Devices() => View("Index");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StartInbound(CancellationToken cancellationToken)
    {
        var started = await simulation.QueueAsync("inbound", cancellationToken);
        TempData[started ? "Success" : "Error"] = started ? "Quy trình nhập kho đã được đưa vào hàng đợi." : "Một quy trình mô phỏng khác đang chạy.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StartOutbound(CancellationToken cancellationToken)
    {
        var started = await simulation.QueueAsync("outbound", cancellationToken);
        TempData[started ? "Success" : "Error"] = started ? "Quy trình xuất kho đã được đưa vào hàng đợi." : "Một quy trình mô phỏng khác đang chạy.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        try
        {
            await simulation.ResetAsync(cancellationToken);
            TempData["Success"] = "Thiết bị mô phỏng đã được đặt lại.";
        }
        catch (InvalidOperationException exception) { TempData["Error"] = exception.Message; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Fault(CancellationToken cancellationToken)
    {
        try
        {
            await simulation.InjectFaultAsync(cancellationToken);
            TempData["Info"] = "Đã ghi nhận cảnh báo mô phỏng.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Không ghi được cảnh báo. Hãy kiểm tra database.";
        }
        return RedirectToAction(nameof(Index));
    }

}





