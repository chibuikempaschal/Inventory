using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationException = StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);
        else
            _logger.LogWarning(exception, "Handled exception ({Status}) for {Path}", statusCode, httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= 500
                ? "Something went wrong. Please try again later."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        switch (exception)
        {
            case ApplicationException appEx:      // any of your exceptions — uses its own StatusCode
                return (appEx.StatusCode, "Application error");

            case UnauthorizedAccessException:
                return (StatusCodes.Status401Unauthorized, "Unauthorized");

            case KeyNotFoundException:
                return (StatusCodes.Status404NotFound, "Not found");

            case ArgumentException:
                return (StatusCodes.Status400BadRequest, "Invalid argument");

            default:
                return (StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }
}