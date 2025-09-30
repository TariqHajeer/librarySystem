using System.Net;
using System.Text.Json;
using LibrarySystem.DataAccess.Exceptions;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.Presentation.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ConflictException ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                error = ex.Message
            };

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (SqlException ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                error = "No database connection"
            };

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.GetType());
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unknown exception happened connect with the site owner"
            };

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
