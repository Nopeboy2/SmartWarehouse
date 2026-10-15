using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWarehouse.Infrastructure.Persistence;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Infrastructure.Services;

namespace SmartWarehouse.Infrastructure.DependencyInjection;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WarehouseDb")
            ?? throw new InvalidOperationException("Connection string 'WarehouseDb' is missing.");

        services.AddScoped<IWarehouseOperations, WarehouseOperations>();
        services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

        return services;
    }
}


