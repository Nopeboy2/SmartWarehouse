using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Application.Warehouse;
using SmartWarehouse.Web.ViewModels;

namespace SmartWarehouse.Web.Controllers;

[Authorize(Roles = "Admin,WarehouseOperator")]
public sealed class InventoryController(IWarehouseOperations operations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await operations.GetInventoryAsync(cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Receive(StockOperationViewModel model, CancellationToken cancellationToken) =>
        ExecuteAsync(model, operations.ReceiveStockAsync, "Nhập kho đã được ghi nhận.", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reserve(StockOperationViewModel model, CancellationToken cancellationToken) =>
        ExecuteAsync(model, operations.ReserveStockAsync, "Hàng đã được giữ chỗ.", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Dispatch(StockOperationViewModel model, CancellationToken cancellationToken) =>
        ExecuteAsync(model, operations.DispatchStockAsync, "Xuất kho đã được xác nhận.", cancellationToken);

    private async Task<IActionResult> ExecuteAsync(StockOperationViewModel model,
        Func<StockOperationRequest, CancellationToken, Task<bool>> operation, string successMessage,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng kiểm tra dữ liệu thao tác.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var processed = await operation(new StockOperationRequest(model.Sku, model.LocationCode,
                model.Quantity, model.ReferenceNumber, model.IdempotencyKey), cancellationToken);
            TempData[processed ? "Success" : "Info"] = processed ? successMessage : "Sự kiện này đã được xử lý trước đó.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}

