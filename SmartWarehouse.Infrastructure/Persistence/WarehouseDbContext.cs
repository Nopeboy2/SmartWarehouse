using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SmartWarehouse.Infrastructure.Identity;
using SmartWarehouse.Domain.Entities;
using SmartWarehouse.Domain.Enums;

namespace SmartWarehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : IdentityDbContext<WarehouseUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RfidTag> RfidTags => Set<RfidTag>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<InboundOrder> InboundOrders => Set<InboundOrder>();
    public DbSet<InboundOrderDetail> InboundOrderDetails => Set<InboundOrderDetail>();
    public DbSet<OutboundOrder> OutboundOrders => Set<OutboundOrder>();
    public DbSet<OutboundOrderDetail> OutboundOrderDetails => Set<OutboundOrderDetail>();
    public DbSet<RfidReadEvent> RfidReadEvents => Set<RfidReadEvent>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<ConveyorTask> ConveyorTasks => Set<ConveyorTask>();
    public DbSet<AgvTask> AgvTasks => Set<AgvTask>();
    public DbSet<DeviceAlert> DeviceAlerts => Set<DeviceAlert>();
    public DbSet<DeviceEvent> DeviceEvents => Set<DeviceEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Sku).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RfidTag>(entity =>
        {
            entity.ToTable("RfidTags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Epc).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.Epc).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.RfidTags).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<WarehouseZone>(entity =>
        {
            entity.ToTable("WarehouseZones");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();
            entity.HasOne(x => x.Warehouse).WithMany(x => x.Zones).HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WarehouseLocation>(entity =>
        {
            entity.ToTable("WarehouseLocations", table => table.HasCheckConstraint("CK_WarehouseLocations_CapacityUnits", "[CapacityUnits] > 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.ZoneId, x.Code }).IsUnique();
            entity.HasOne(x => x.Zone).WithMany(x => x.Locations).HasForeignKey(x => x.ZoneId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventory", table =>
            {
                table.HasCheckConstraint("CK_Inventory_Quantities", "[QuantityOnHand] >= 0 AND [QuantityReserved] >= 0 AND [QuantityInTransit] >= 0");
                table.HasCheckConstraint("CK_Inventory_Reserved", "[QuantityReserved] <= [QuantityOnHand]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.ProductId, x.LocationId }).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.Inventory).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Location).WithMany(x => x.Inventory).HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("InventoryMovements", table => table.HasCheckConstraint("CK_InventoryMovements_Quantity", "[Quantity] > 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => new { x.ProductId, x.OccurredAt });
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.FromLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.ToLocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InboundOrder>(entity =>
        {
            entity.ToTable("InboundOrders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Number).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.HasMany(x => x.Details).WithOne(x => x.InboundOrder).HasForeignKey(x => x.InboundOrderId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<InboundOrderDetail>(entity =>
        {
            entity.ToTable("InboundOrderDetails", table => table.HasCheckConstraint("CK_InboundOrderDetails_Quantities", "[ExpectedQuantity] > 0 AND [ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [ExpectedQuantity]"));
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OutboundOrder>(entity =>
        {
            entity.ToTable("OutboundOrders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Number).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.HasMany(x => x.Details).WithOne(x => x.OutboundOrder).HasForeignKey(x => x.OutboundOrderId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OutboundOrderDetail>(entity =>
        {
            entity.ToTable("OutboundOrderDetails", table => table.HasCheckConstraint("CK_OutboundOrderDetails_Quantities", "[Quantity] > 0 AND [PickedQuantity] >= 0 AND [PickedQuantity] <= [Quantity]"));
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RfidReadEvent>(entity =>
        {
            entity.ToTable("RFIDReadEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Epc).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReaderCode).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ReadPoint).HasMaxLength(40).IsRequired();
            entity.Property(x => x.RejectionReason).HasMaxLength(240);
            entity.HasIndex(x => x.EventKey).IsUnique();
            entity.HasIndex(x => new { x.Epc, x.ReadAt });
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });
        modelBuilder.Entity<ConveyorTask>(entity =>
        {
            entity.ToTable("ConveyorTasks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TaskNumber).HasMaxLength(48).IsRequired();
            entity.HasIndex(x => x.TaskNumber).IsUnique();
        });
        modelBuilder.Entity<AgvTask>(entity =>
        {
            entity.ToTable("AGVTasks", table => table.HasCheckConstraint("CK_AGVTasks_BatteryPercent", "[BatteryPercent] BETWEEN 0 AND 100"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TaskNumber).HasMaxLength(48).IsRequired();
            entity.HasIndex(x => x.TaskNumber).IsUnique();
        });
        modelBuilder.Entity<DeviceAlert>(entity =>
        {
            entity.ToTable("DeviceAlerts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DeviceCode).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(400).IsRequired();
            entity.HasIndex(x => new { x.ResolvedAt, x.CreatedAt });
        });

        modelBuilder.Entity<DeviceEvent>(entity =>
        {
            entity.ToTable("DeviceEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DeviceCode).HasMaxLength(40).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(400).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(64);
            entity.HasIndex(x => new { x.DeviceCode, x.OccurredAt });
        });
        SeedDemoData(modelBuilder);
    }

    private static void SeedDemoData(ModelBuilder modelBuilder)
    {
        var categoryId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var productId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var warehouseId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var zoneId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var locationId = Guid.Parse("50000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Category>().HasData(new Category { Id = categoryId, Code = "GENERAL", Name = "Hàng hóa chung", IsActive = true });
        modelBuilder.Entity<Product>().HasData(new Product { Id = productId, Sku = "DEMO-001", Name = "Thùng hàng mẫu", UnitOfMeasure = "THÙNG", CategoryId = categoryId, IsActive = true });
        modelBuilder.Entity<RfidTag>().HasData(new RfidTag { Id = Guid.Parse("80000000-0000-0000-0000-000000000001"), Epc = "E2806894A000000000000001", ProductId = productId, Status = RfidTagStatus.Active });
        modelBuilder.Entity<Warehouse>().HasData(new Warehouse { Id = warehouseId, Code = "WH-MAIN", Name = "Kho chính", Address = "Khu vực demo" });
        modelBuilder.Entity<WarehouseZone>().HasData(new WarehouseZone { Id = zoneId, WarehouseId = warehouseId, Code = "A", Name = "Khu kệ A" });
        modelBuilder.Entity<WarehouseLocation>().HasData(new WarehouseLocation { Id = locationId, ZoneId = zoneId, Code = "A-01-01", Name = "Ô kệ A-01-01", CapacityUnits = 100, IsActive = true });
        modelBuilder.Entity<Inventory>().HasData(new { Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), ProductId = productId, LocationId = locationId, QuantityOnHand = 24, QuantityReserved = 0, QuantityInTransit = 0 });
        modelBuilder.Entity<Device>().HasData(
            new Device { Id = Guid.Parse("70000000-0000-0000-0000-000000000001"), Code = "RFID-IN-01", Name = "Cổng RFID nhập", Type = DeviceType.RfidReader, Status = DeviceStatus.Idle },
            new Device { Id = Guid.Parse("70000000-0000-0000-0000-000000000002"), Code = "CONV-01", Name = "Băng tải chính", Type = DeviceType.Conveyor, Status = DeviceStatus.Idle },
            new Device { Id = Guid.Parse("70000000-0000-0000-0000-000000000003"), Code = "AGV-01", Name = "Xe tự hành 01", Type = DeviceType.Agv, Status = DeviceStatus.Idle },
            new Device { Id = Guid.Parse("70000000-0000-0000-0000-000000000004"), Code = "RFID-OUT-01", Name = "Cổng RFID xuất", Type = DeviceType.RfidReader, Status = DeviceStatus.Idle });
    }
}





