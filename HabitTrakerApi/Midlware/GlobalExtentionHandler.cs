using Microsoft.AspNetCore.Mvc;

namespace HabitTrakerApi.Midlware;
using Microsoft.AspNetCore.Diagnostics;


public class GlobalExtentionHandler : IExceptionHandler
{
    
    private readonly ILogger <GlobalExtentionHandler> _logger;

    public GlobalExtentionHandler(ILogger<GlobalExtentionHandler> logger)
    {
        _logger = logger;
    }
    
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError($"Ощибка {exception.Message}");

        var problemDeteils = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Произошла непредвиденная ошибка. Пожалуйста, попробуйте позже.",
            Title = "Ощибка со стороны сервера"
        };

        httpContext.Response.StatusCode = problemDeteils.Status.Value;
        
        _logger.Log((LogLevel)300, 1 , problemDeteils.Detail.ToString() , exception.Message.ToString());
        
        return new ValueTask<bool>(false);
        
    }
}