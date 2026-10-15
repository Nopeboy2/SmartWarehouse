using SmartWarehouse.Domain.Enums;

namespace SmartWarehouse.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public sealed class Product
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = "PCS";
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<RfidTag> RfidTags { get; set; } = new List<RfidTag>();
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}

public sealed class RfidTag
{
    public Guid Id { get; set; }
    public string Epc { get; set; } = "";
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public RfidTagStatus Status { get; set; } = RfidTagStatus.Active;
    public DateTimeOffset? LastSeenAt { get; set; }
}

public sealed class Warehouse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
}

public sealed class WarehouseZone
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
}

public sealed class WarehouseLocation
{
    public Guid Id { get; set; }
    public Guid ZoneId { get; set; }
    public WarehouseZone Zone { get; set; } = null!;
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int CapacityUnits { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}

public sealed class Inventory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid LocationId { get; set; }
    public WarehouseLocation Location { get; set; } = null!;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityInTransit { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
}

public sealed class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public string ReferenceNumber { get; set; } = "";
    public string IdempotencyKey { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class InboundOrder
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<InboundOrderDetail> Details { get; set; } = new List<InboundOrderDetail>();
}

public sealed class InboundOrderDetail
{
    public Guid Id { get; set; }
    public Guid InboundOrderId { get; set; }
    public InboundOrder InboundOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int ExpectedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
}

public sealed class OutboundOrder
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<OutboundOrderDetail> Details { get; set; } = new List<OutboundOrderDetail>();
}

public sealed class OutboundOrderDetail
{
    public Guid Id { get; set; }
    public Guid OutboundOrderId { get; set; }
    public OutboundOrder OutboundOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? LocationId { get; set; }
    public int Quantity { get; set; }
    public int PickedQuantity { get; set; }
}

public sealed class RfidReadEvent
{
    public Guid Id { get; set; }
    public string EventKey { get; set; } = "";
    public string Epc { get; set; } = "";
    public string ReaderCode { get; set; } = "";
    public string ReadPoint { get; set; } = "";
    public DateTimeOffset ReadAt { get; set; }
    public bool IsAccepted { get; set; }
    public string? RejectionReason { get; set; }
}

public sealed class Device
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DeviceType Type { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public DateTimeOffset? LastSeenAt { get; set; }
}

public sealed class ConveyorTask
{
    public Guid Id { get; set; }
    public string TaskNumber { get; set; } = "";
    public DeviceTaskStatus Status { get; set; } = DeviceTaskStatus.Queued;
    public string? SourcePoint { get; set; }
    public string? DestinationPoint { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AgvTask
{
    public Guid Id { get; set; }
    public string TaskNumber { get; set; } = "";
    public DeviceTaskStatus Status { get; set; } = DeviceTaskStatus.Queued;
    public string? PickupPoint { get; set; }
    public string? DropoffPoint { get; set; }
    public int BatteryPercent { get; set; } = 100;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class DeviceAlert
{
    public Guid Id { get; set; }
    public string DeviceCode { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}


public sealed class DeviceEvent
{
    public Guid Id { get; set; }
    public string DeviceCode { get; set; } = "";
    public string EventType { get; set; } = "";
    public string Message { get; set; } = "";
    public string? CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}


