using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Domain.Entities;
using SmartWarehouse.Domain.Enums;
using SmartWarehouse.Infrastructure.Persistence;

namespace SmartWarehouse.Infrastructure.Services;

public sealed class WarehouseOperations(WarehouseDbContext db) : IWarehouseOperations
{
    public async Task<DashboardSummary> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var productCount = await db.Products.CountAsync(x => x.IsActive, cancellationToken);
        var stockUnits = await db.Inventory.SumAsync(x => (int?)x.QuantityOnHand, cancellationToken) ?? 0;
        var openAlerts = await db.DeviceAlerts.CountAsync(x => x.ResolvedAt == null, cancellationToken);
        var activeTasks = await db.AgvTasks.CountAsync(x => x.Status == DeviceTaskStatus.Queued || x.Status == DeviceTaskStatus.Assigned || x.Status == DeviceTaskStatus.InProgress, cancellationToken);
        var movementRows = await db.InventoryMovements.AsNoTracking()
            .OrderByDescending(x => x.OccurredAt).Take(8)
            .Join(db.Products.AsNoTracking(), m => m.ProductId, p => p.Id,
                (m, p) => new { m.OccurredAt, p.Sku, m.Type, m.Quantity, m.ReferenceNumber })
            .ToListAsync(cancellationToken);
        var recent = movementRows.Select(x => new MovementRow(x.OccurredAt, x.Sku, x.Type.ToString(), x.Quantity, x.ReferenceNumber)).ToList();
        return new DashboardSummary(productCount, stockUnits, openAlerts, activeTasks, recent);
    }

    public async Task<IReadOnlyList<CategoryRow>> GetCategoryRowsAsync(CancellationToken cancellationToken = default) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CategoryRow(x.Id, x.Code, x.Name, x.Products.Count, x.IsActive)).ToListAsync(cancellationToken);

    public async Task CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        if (code.Length is < 1 or > 32 || name.Length is < 1 or > 120) throw new ArgumentException("Mã hoặc tên danh mục không hợp lệ.");
        if (await db.Categories.AnyAsync(x => x.Code == code, cancellationToken)) throw new InvalidOperationException("Mã danh mục đã tồn tại.");
        db.Categories.Add(new Category { Id = Guid.NewGuid(), Code = code, Name = name, IsActive = true });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WarehouseZoneRow>> GetWarehouseZonesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.WarehouseZones.AsNoTracking().Include(x => x.Warehouse).OrderBy(x => x.Warehouse.Code).ThenBy(x => x.Code).ToListAsync(cancellationToken);
        return rows.Select(x => new WarehouseZoneRow(x.Id, x.Warehouse.Code, x.Code, x.Name)).ToList();
    }

    public async Task<IReadOnlyList<WarehouseLocationRow>> GetWarehouseLocationsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.WarehouseLocations.AsNoTracking().Include(x => x.Zone).ThenInclude(x => x.Warehouse).OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return rows.Select(x => new WarehouseLocationRow(x.Zone.Warehouse.Code, x.Zone.Code, x.Code, x.Name, x.CapacityUnits, x.IsActive)).ToList();
    }

    public async Task CreateWarehouseLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        if (request.CapacityUnits <= 0 || code.Length is < 1 or > 40 || name.Length is < 1 or > 120)
            throw new ArgumentException("Thông tin vị trí lưu trữ không hợp lệ.");
        if (!await db.WarehouseZones.AnyAsync(x => x.Id == request.ZoneId, cancellationToken)) throw new ArgumentException("Khu vực kho không tồn tại.");
        if (await db.WarehouseLocations.AnyAsync(x => x.ZoneId == request.ZoneId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("Mã vị trí đã tồn tại trong khu vực.");
        db.WarehouseLocations.Add(new WarehouseLocation { Id = Guid.NewGuid(), ZoneId = request.ZoneId, Code = code, Name = name, CapacityUnits = request.CapacityUnits, IsActive = true });
        await db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<RfidTagRow>> GetRfidTagsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.RfidTags.AsNoTracking().Include(x => x.Product).OrderBy(x => x.Epc).ToListAsync(cancellationToken);
        return rows.Select(x => new RfidTagRow(x.Epc, x.Product.Sku, x.Product.Name, x.Status.ToString(), x.LastSeenAt)).ToList();
    }

    public async Task<IReadOnlyList<ActivityRow>> GetActivityAsync(CancellationToken cancellationToken = default)
    {
        var movements = await db.InventoryMovements.AsNoTracking().Include(x => x.Product).OrderByDescending(x => x.OccurredAt).Take(100).ToListAsync(cancellationToken);
        var reads = await db.RfidReadEvents.AsNoTracking().OrderByDescending(x => x.ReadAt).Take(100).ToListAsync(cancellationToken);
        var deviceEvents = await db.DeviceEvents.AsNoTracking().OrderByDescending(x => x.OccurredAt).Take(100).ToListAsync(cancellationToken);
        return movements.Select(x => new ActivityRow(x.OccurredAt, "WMS", x.Type.ToString(), $"{x.Product.Sku} · {x.Quantity} · {x.ReferenceNumber}", "Đã ghi sổ"))
            .Concat(reads.Select(x => new ActivityRow(x.ReadAt, x.ReaderCode, "Đọc RFID", $"{x.Epc} · {x.ReadPoint}", x.IsAccepted ? "Hợp lệ" : x.RejectionReason ?? "Từ chối")))
            .Concat(deviceEvents.Select(x => new ActivityRow(x.OccurredAt, x.DeviceCode, x.EventType, x.Message, "Đã ghi nhận")))
            .OrderByDescending(x => x.OccurredAt).Take(100).ToList();
    }

    public async Task<IReadOnlyList<AlertRow>> GetAlertsAsync(CancellationToken cancellationToken = default) =>
        await db.DeviceAlerts.AsNoTracking().OrderByDescending(x => x.CreatedAt)
            .Select(x => new AlertRow(x.Id, x.DeviceCode, x.Message, x.CreatedAt, x.ResolvedAt)).ToListAsync(cancellationToken);

    public async Task ResolveAlertAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var alert = await db.DeviceAlerts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ArgumentException("Không tìm thấy cảnh báo.");
        if (alert.ResolvedAt is null) { alert.ResolvedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken); }
    }
    public async Task<IReadOnlyList<DeviceRow>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.Devices.AsNoTracking().OrderBy(x => x.Type).ThenBy(x => x.Code).ToListAsync(cancellationToken);
        return rows.Select(x => new DeviceRow(x.Code, x.Name, x.Type.ToString(), x.Status.ToString(), x.LastSeenAt)).ToList();
    }

    public async Task CreateRfidTagAsync(CreateRfidTagRequest request, CancellationToken cancellationToken = default)
    {
        var epc = request.Epc.Trim().ToUpperInvariant();
        if (epc.Length is not (24 or 28 or 32 or 48 or 64) || !epc.All(Uri.IsHexDigit))
            throw new ArgumentException("EPC phải có 24, 28, 32, 48 hoặc 64 ký tự hex.");
        var sku = NormalizeSku(request.Sku);
        var product = await db.Products.SingleOrDefaultAsync(x => x.Sku == sku && x.IsActive, cancellationToken)
            ?? throw new ArgumentException("Không tìm thấy SKU đang hoạt động.");
        if (await db.RfidTags.AnyAsync(x => x.Epc == epc, cancellationToken))
            throw new InvalidOperationException("EPC này đã được gán cho một sản phẩm khác.");
        db.RfidTags.Add(new RfidTag { Id = Guid.NewGuid(), Epc = epc, ProductId = product.Id, Status = RfidTagStatus.Active });
        await db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<ProductRow>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        await db.Products.AsNoTracking().OrderBy(x => x.Sku)
            .Select(x => new ProductRow(x.Id, x.Sku, x.Name, x.Category.Name, x.UnitOfMeasure, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(Guid Id, string Code, string Name)>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name }).ToListAsync(cancellationToken);
        return rows.Select(x => (x.Id, x.Code, x.Name)).ToList();
    }

    public async Task CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var sku = request.Sku.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var unit = request.UnitOfMeasure.Trim().ToUpperInvariant();
        if (sku.Length is < 1 or > 64 || name.Length is < 1 or > 200 || unit.Length is < 1 or > 16)
            throw new ArgumentException("SKU, tên sản phẩm hoặc đơn vị tính không hợp lệ.");
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken))
            throw new ArgumentException("Danh mục không tồn tại hoặc đã ngừng hoạt động.");
        if (await db.Products.AnyAsync(x => x.Sku == sku, cancellationToken))
            throw new InvalidOperationException($"SKU '{sku}' đã tồn tại.");
        db.Products.Add(new Product { Id = Guid.NewGuid(), Sku = sku, Name = name, CategoryId = request.CategoryId,
            UnitOfMeasure = unit, Description = request.Description?.Trim(), IsActive = true });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryRow>> GetInventoryAsync(CancellationToken cancellationToken = default) =>
        await db.Inventory.AsNoTracking().OrderBy(x => x.Product.Sku).ThenBy(x => x.Location.Code)
            .Select(x => new InventoryRow(x.Product.Sku, x.Product.Name, x.Location.Code,
                x.QuantityOnHand, x.QuantityReserved, x.QuantityOnHand - x.QuantityReserved, x.QuantityInTransit))
            .ToListAsync(cancellationToken);

    public async Task<RfidReadResult> ProcessRfidReadAsync(string epc, string readerCode, string readPoint, string eventKey, CancellationToken cancellationToken = default)
    {
        epc = epc.Trim().ToUpperInvariant();
        readerCode = readerCode.Trim();
        readPoint = readPoint.Trim();
        eventKey = eventKey.Trim();
        if (epc.Length is < 1 or > 64 || readerCode.Length is < 1 or > 40 || readPoint.Length is < 1 or > 40 || eventKey.Length is < 1 or > 128)
            throw new ArgumentException("Dữ liệu sự kiện RFID không hợp lệ.");

        var existing = await db.RfidReadEvents.AsNoTracking().SingleOrDefaultAsync(x => x.EventKey == eventKey, cancellationToken);
        if (existing is not null)
        {
            var existingSku = await db.RfidTags.Where(x => x.Epc == existing.Epc).Select(x => x.Product.Sku).SingleOrDefaultAsync(cancellationToken);
            return new RfidReadResult(existing.IsAccepted, true, existingSku, existing.RejectionReason);
        }

        var tag = await db.RfidTags.Include(x => x.Product).SingleOrDefaultAsync(x => x.Epc == epc && x.Status == RfidTagStatus.Active, cancellationToken);
        var read = new RfidReadEvent
        {
            Id = Guid.NewGuid(), EventKey = eventKey, Epc = epc, ReaderCode = readerCode, ReadPoint = readPoint,
            ReadAt = DateTimeOffset.UtcNow, IsAccepted = tag is not null,
            RejectionReason = tag is null ? "EPC không tồn tại hoặc thẻ đã ngừng hoạt động." : null
        };
        db.RfidReadEvents.Add(read);
        await db.SaveChangesAsync(cancellationToken);
        return new RfidReadResult(read.IsAccepted, false, tag?.Product.Sku, read.RejectionReason);
    }
    public async Task<bool> ReceiveStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        if (await db.InventoryMovements.AnyAsync(x => x.IdempotencyKey == request.IdempotencyKey.Trim(), cancellationToken)) return false;
        var product = await db.Products.SingleOrDefaultAsync(x => x.Sku == NormalizeSku(request.Sku) && x.IsActive, cancellationToken)
            ?? throw new ArgumentException("Không tìm thấy SKU đang hoạt động.");
        var location = await db.WarehouseLocations.SingleOrDefaultAsync(x => x.Code == request.LocationCode.Trim() && x.IsActive, cancellationToken)
            ?? throw new ArgumentException("Không tìm thấy vị trí kho đang hoạt động.");
        var inventory = await db.Inventory.SingleOrDefaultAsync(x => x.ProductId == product.Id && x.LocationId == location.Id, cancellationToken);
        if (inventory is null)
        {
            inventory = new Inventory { Id = Guid.NewGuid(), ProductId = product.Id, LocationId = location.Id };
            db.Inventory.Add(inventory);
        }
        inventory.QuantityOnHand = checked(inventory.QuantityOnHand + request.Quantity);
        db.InventoryMovements.Add(CreateMovement(product.Id, null, location.Id, request, MovementType.Receipt));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReserveStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        if (await db.InventoryMovements.AnyAsync(x => x.IdempotencyKey == request.IdempotencyKey.Trim(), cancellationToken)) return false;
        var row = await FindInventoryAsync(request, cancellationToken);
        if (row.QuantityOnHand - row.QuantityReserved < request.Quantity) throw new InvalidOperationException("Tồn khả dụng không đủ để giữ chỗ.");
        row.QuantityReserved += request.Quantity;
        db.InventoryMovements.Add(CreateMovement(row.ProductId, null, row.LocationId, request, MovementType.Reservation));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DispatchStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        if (await db.InventoryMovements.AnyAsync(x => x.IdempotencyKey == request.IdempotencyKey.Trim(), cancellationToken)) return false;
        var row = await FindInventoryAsync(request, cancellationToken);
        if (row.QuantityReserved < request.Quantity || row.QuantityOnHand < request.Quantity)
            throw new InvalidOperationException("Không đủ hàng đã giữ chỗ để xác nhận xuất.");
        row.QuantityReserved -= request.Quantity;
        row.QuantityOnHand -= request.Quantity;
        db.InventoryMovements.Add(CreateMovement(row.ProductId, row.LocationId, null, request, MovementType.Dispatch));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Inventory> FindInventoryAsync(StockOperationRequest request, CancellationToken cancellationToken)
    {
        var sku = NormalizeSku(request.Sku);
        var locationCode = request.LocationCode.Trim();
        return await db.Inventory.Include(x => x.Product).Include(x => x.Location)
            .SingleOrDefaultAsync(x => x.Product.Sku == sku && x.Location.Code == locationCode, cancellationToken)
            ?? throw new ArgumentException("Không tìm thấy tồn kho cho SKU và vị trí đã chọn.");
    }

    private static InventoryMovement CreateMovement(Guid productId, Guid? from, Guid? to, StockOperationRequest request, MovementType type) => new()
    {
        Id = Guid.NewGuid(), ProductId = productId, FromLocationId = from, ToLocationId = to,
        Quantity = request.Quantity, Type = type, ReferenceNumber = request.ReferenceNumber.Trim(),
        IdempotencyKey = request.IdempotencyKey.Trim(), OccurredAt = DateTimeOffset.UtcNow
    };

    private static void ValidateRequest(StockOperationRequest request)
    {
        if (request.Quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
        if (string.IsNullOrWhiteSpace(request.Sku) || string.IsNullOrWhiteSpace(request.LocationCode)
            || string.IsNullOrWhiteSpace(request.ReferenceNumber) || string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("SKU, vị trí, mã tham chiếu và khóa idempotency là bắt buộc.");
        if (request.ReferenceNumber.Trim().Length > 64 || request.IdempotencyKey.Trim().Length > 128)
            throw new ArgumentException("Mã tham chiếu hoặc khóa idempotency quá dài.");
    }

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();
}






