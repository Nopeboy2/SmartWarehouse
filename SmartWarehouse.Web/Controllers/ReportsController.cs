using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin,Supervisor")]
public sealed class ReportsController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await operations.GetDashboardAsync(cancellationToken));
}

