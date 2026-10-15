using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin,Supervisor,WarehouseOperator")]
public sealed class AlertsController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await operations.GetAlertsAsync(cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken cancellationToken)
    {
        await operations.ResolveAlertAsync(id, cancellationToken);
        TempData["Success"] = "Cảnh báo đã được đánh dấu xử lý.";
        return RedirectToAction(nameof(Index));
    }
}

