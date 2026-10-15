using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SmartWarehouse.Web.ErrorHandling;

public sealed class WarehouseExceptionHandler(ILogger<WarehouseExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted) return false;
        var (status, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Không thể thực hiện thao tác"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Dữ liệu vừa được thay đổi"),
            _ => (StatusCodes.Status500InternalServerError, "Đã xảy ra lỗi hệ thống")
        };
        if (status >= 500) logger.LogError(exception, "Request failed with trace {TraceId}", httpContext.TraceIdentifier);
        else logger.LogWarning(exception, "Request rejected with {StatusCode}", status);

        httpContext.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 ? "Yêu cầu chưa thể hoàn tất. Hãy thử lại hoặc liên hệ quản trị viên." : exception.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
