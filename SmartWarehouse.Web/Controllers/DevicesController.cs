using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;

namespace SmartWarehouse.Web.Controllers;

public sealed class DevicesController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetDevicesAsync(cancellationToken));
}
