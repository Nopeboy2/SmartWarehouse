using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Domain.Entities;
using SmartWarehouse.Domain.Enums;
using SmartWarehouse.Infrastructure.Persistence;
using SmartWarehouse.Infrastructure.Services;

namespace SmartWarehouse.Tests;

public sealed class WarehouseOperationsTests
{
    [Fact]
    public async Task ReceiveStock_UpdatesInventoryAndWritesLedger()
    {
        await using var fixture = await CreateFixtureAsync();
        var processed = await fixture.Operations.ReceiveStockAsync(Request("receive-1", 3));

        Assert.True(processed);
        Assert.Equal(27, (await fixture.Db.Inventory.SingleAsync()).QuantityOnHand);
        var movement = await fixture.Db.InventoryMovements.SingleAsync();
        Assert.Equal(MovementType.Receipt, movement.Type);
        Assert.Equal("receive-1", movement.IdempotencyKey);
    }

    [Fact]
    public async Task ReceiveStock_RepeatedIdempotencyKeyDoesNotAddStockTwice()
    {
        await using var fixture = await CreateFixtureAsync();
        Assert.True(await fixture.Operations.ReceiveStockAsync(Request("same-event", 2)));
        Assert.False(await fixture.Operations.ReceiveStockAsync(Request("same-event", 2)));

        Assert.Equal(26, (await fixture.Db.Inventory.SingleAsync()).QuantityOnHand);
        Assert.Single(await fixture.Db.InventoryMovements.ToListAsync());
    }

    [Fact]
    public async Task OutboundReservationAndDispatch_UseReservedQuantity()
    {
        await using var fixture = await CreateFixtureAsync();
        Assert.True(await fixture.Operations.ReserveStockAsync(Request("reserve-1", 4)));
        var reserved = await fixture.Db.Inventory.SingleAsync();
        Assert.Equal(24, reserved.QuantityOnHand);
        Assert.Equal(4, reserved.QuantityReserved);
        Assert.Equal(20, reserved.QuantityAvailable);

        Assert.True(await fixture.Operations.DispatchStockAsync(Request("dispatch-1", 4)));
        var dispatched = await fixture.Db.Inventory.SingleAsync();
        Assert.Equal(20, dispatched.QuantityOnHand);
        Assert.Equal(0, dispatched.QuantityReserved);
        Assert.Equal(2, await fixture.Db.InventoryMovements.CountAsync());
    }

    [Fact]
    public async Task ReserveStock_RejectsQuantityAboveAvailable()
    {
        await using var fixture = await CreateFixtureAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Operations.ReserveStockAsync(Request("too-many", 25)));
        Assert.Equal(0, (await fixture.Db.Inventory.SingleAsync()).QuantityReserved);
    }

    [Fact]
    public async Task RfidRead_AcceptsKnownTagAndRejectsUnknownTag()
    {
        await using var fixture = await CreateFixtureAsync();
        var accepted = await fixture.Operations.ProcessRfidReadAsync("E2806894A000000000000001", "RFID-IN-01", "GATE-IN", "read-known");
        var rejected = await fixture.Operations.ProcessRfidReadAsync("AAAAAAAAAAAAAAAAAAAAAAAA", "RFID-IN-01", "GATE-IN", "read-unknown");

        Assert.True(accepted.IsAccepted);
        Assert.Equal("DEMO-001", accepted.Sku);
        Assert.False(rejected.IsAccepted);
        Assert.NotNull(rejected.RejectionReason);
        Assert.Equal(2, await fixture.Db.RfidReadEvents.CountAsync());
    }

    [Fact]
    public async Task RfidRead_ReplayOfSameEventKeyIsIdempotent()
    {
        await using var fixture = await CreateFixtureAsync();
        await fixture.Operations.ProcessRfidReadAsync("E2806894A000000000000001", "RFID-IN-01", "GATE-IN", "read-replay");
        var duplicate = await fixture.Operations.ProcessRfidReadAsync("E2806894A000000000000001", "RFID-IN-01", "GATE-IN", "read-replay");

        Assert.True(duplicate.IsAccepted);
        Assert.True(duplicate.IsDuplicate);
        Assert.Single(await fixture.Db.RfidReadEvents.ToListAsync());
    }

    private static StockOperationRequest Request(string key, int quantity) =>
        new("DEMO-001", "A-01-01", quantity, "TEST-ORDER", key);

    private static async Task<Fixture> CreateFixtureAsync()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase($"warehouse-tests-{Guid.NewGuid():N}")
            .Options;
        var db = new WarehouseDbContext(options);
        var category = new Category { Id = Guid.NewGuid(), Code = "GENERAL", Name = "General" };
        var product = new Product { Id = Guid.NewGuid(), Sku = "DEMO-001", Name = "Demo carton", UnitOfMeasure = "BOX", CategoryId = category.Id, Category = category };
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Code = "WH-MAIN", Name = "Main" };
        var zone = new WarehouseZone { Id = Guid.NewGuid(), WarehouseId = warehouse.Id, Warehouse = warehouse, Code = "A", Name = "Aisle A" };
        var location = new WarehouseLocation { Id = Guid.NewGuid(), ZoneId = zone.Id, Zone = zone, Code = "A-01-01", Name = "Shelf A-01-01", CapacityUnits = 100 };
        db.AddRange(category, product, warehouse, zone, location,
            new Inventory { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, LocationId = location.Id, Location = location, QuantityOnHand = 24 },
            new RfidTag { Id = Guid.NewGuid(), Epc = "E2806894A000000000000001", ProductId = product.Id, Product = product, Status = RfidTagStatus.Active });
        await db.SaveChangesAsync();
        return new Fixture(db, new WarehouseOperations(db));
    }

    private sealed class Fixture(WarehouseDbContext db, WarehouseOperations operations) : IAsyncDisposable
    {
        public WarehouseDbContext Db { get; } = db;
        public WarehouseOperations Operations { get; } = operations;
        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }
}
