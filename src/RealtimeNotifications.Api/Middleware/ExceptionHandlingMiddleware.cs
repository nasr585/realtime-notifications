using System.Net;
using System.Text.Json;
using FluentValidation;
using RealtimeNotifications.Application.Common;
using RealtimeNotifications.Domain.Common;

namespace RealtimeNotifications.Api.Middleware;

/// <summary>
/// Maps domain and validation exceptions to HTTP problem responses,
/// so controllers and handlers stay free of try/catch plumbing.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteProblem(context, HttpStatusCode.BadRequest, "Validation failed", errors);
        }
        catch (DomainException exception)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (NotFoundException exception)
        {
            await WriteProblem(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(
        HttpContext context,
        HttpStatusCode status,
        string title,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
