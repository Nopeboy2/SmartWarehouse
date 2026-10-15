using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.Infrastructure.Identity;
using SmartWarehouse.Infrastructure.Persistence;

namespace SmartWarehouse.Web.Services;

public sealed class DevelopmentSeedService(
    IServiceScopeFactory scopeFactory,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<DevelopmentSeedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return;
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            await db.Database.MigrateAsync(cancellationToken);
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var roleName in new[] { "Admin", "WarehouseOperator", "Supervisor" })
                if (!await roles.RoleExistsAsync(roleName))
                    await roles.CreateAsync(new IdentityRole<Guid>(roleName));

            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Development admin seed skipped. Set Seed:AdminEmail and Seed:AdminPassword to create an admin.");
                return;
            }

            var users = scope.ServiceProvider.GetRequiredService<UserManager<WarehouseUser>>();
            var user = await users.FindByEmailAsync(email);
            if (user is null)
            {
                user = new WarehouseUser { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true, DisplayName = "Quản trị viên" };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
            if (!await users.IsInRoleAsync(user, "Admin"))
                await users.AddToRoleAsync(user, "Admin");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Development database migration or seed failed. The app will continue; apply migrations and restart.");
        }
    }
}


