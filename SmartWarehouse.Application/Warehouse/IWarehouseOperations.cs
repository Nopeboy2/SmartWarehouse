namespace SmartWarehouse.Application.Warehouse;

public sealed record ProductRow(Guid Id, string Sku, string Name, string Category, string UnitOfMeasure, bool IsActive);
public sealed record InventoryRow(string Sku, string ProductName, string LocationCode, int OnHand, int Reserved, int Available, int InTransit);
public sealed record MovementRow(DateTimeOffset OccurredAt, string Sku, string Type, int Quantity, string ReferenceNumber);
public sealed record DashboardSummary(int ProductCount, int StockUnits, int OpenAlerts, int ActiveTasks, IReadOnlyList<MovementRow> RecentMovements);
public sealed record CreateProductRequest(string Sku, string Name, Guid CategoryId, string UnitOfMeasure, string? Description);
public sealed record CategoryRow(Guid Id, string Code, string Name, int ProductCount, bool IsActive);
public sealed record WarehouseZoneRow(Guid Id, string WarehouseCode, string Code, string Name);
public sealed record WarehouseLocationRow(string WarehouseCode, string ZoneCode, string Code, string Name, int CapacityUnits, bool IsActive);
public sealed record CreateCategoryRequest(string Code, string Name);
public sealed record CreateLocationRequest(Guid ZoneId, string Code, string Name, int CapacityUnits);
public sealed record RfidTagRow(string Epc, string Sku, string ProductName, string Status, DateTimeOffset? LastSeenAt);
public sealed record ActivityRow(DateTimeOffset OccurredAt, string Source, string Title, string Detail, string Status);
public sealed record AlertRow(Guid Id, string DeviceCode, string Message, DateTimeOffset CreatedAt, DateTimeOffset? ResolvedAt);
public sealed record DeviceRow(string Code, string Name, string Type, string Status, DateTimeOffset? LastSeenAt);
public sealed record CreateRfidTagRequest(string Epc, string Sku);
public sealed record RfidReadResult(bool IsAccepted, bool IsDuplicate, string? Sku, string? RejectionReason);
public sealed record StockOperationRequest(string Sku, string LocationCode, int Quantity, string ReferenceNumber, string IdempotencyKey);

public interface IWarehouseOperations
{
    Task<DashboardSummary> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryRow>> GetCategoryRowsAsync(CancellationToken cancellationToken = default);
    Task CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseZoneRow>> GetWarehouseZonesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseLocationRow>> GetWarehouseLocationsAsync(CancellationToken cancellationToken = default);
    Task CreateWarehouseLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RfidTagRow>> GetRfidTagsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityRow>> GetActivityAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertRow>> GetAlertsAsync(CancellationToken cancellationToken = default);
    Task ResolveAlertAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceRow>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task CreateRfidTagAsync(CreateRfidTagRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductRow>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid Id, string Code, string Name)>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryRow>> GetInventoryAsync(CancellationToken cancellationToken = default);
    Task<RfidReadResult> ProcessRfidReadAsync(string epc, string readerCode, string readPoint, string eventKey, CancellationToken cancellationToken = default);
    Task<bool> ReceiveStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default);
    Task<bool> ReserveStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DispatchStockAsync(StockOperationRequest request, CancellationToken cancellationToken = default);
}




