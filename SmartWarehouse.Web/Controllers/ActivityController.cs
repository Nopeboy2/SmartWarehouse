using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;

namespace SmartWarehouse.Web.Controllers;

public sealed class ActivityController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetActivityAsync(cancellationToken));
}
