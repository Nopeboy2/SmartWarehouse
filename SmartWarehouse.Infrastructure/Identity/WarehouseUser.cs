using Microsoft.AspNetCore.Identity;

namespace SmartWarehouse.Infrastructure.Identity;

public sealed class WarehouseUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
