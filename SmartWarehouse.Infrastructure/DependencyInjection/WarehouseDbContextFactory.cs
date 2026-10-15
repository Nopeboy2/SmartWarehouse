using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartWarehouse.Infrastructure.Persistence;

namespace SmartWarehouse.Infrastructure.DependencyInjection;

public sealed class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTWAREHOUSE_CONNECTION")
            ?? "Server=.\\SQLEXPRESS;Database=SmartWarehouseDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new WarehouseDbContext(options);
    }
}


