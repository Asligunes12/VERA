using Microsoft.AspNetCore.Http;
using VERA.Application.DTOs.Common;
using VERA.Application.Exceptions;

namespace VERA.API.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
{
    await _next(context);
}
    catch (AppException ex)
{
    context.Response.StatusCode = ex.StatusCode;
    context.Response.ContentType = "application/json";

    var response = new ErrorResponse
    {
        Message = ex.Message,
        Code = ex.Code
    };

    await context.Response.WriteAsJsonAsync(response);
}
   catch (Exception)
   {
    context.Response.StatusCode =
        StatusCodes.Status500InternalServerError;

    context.Response.ContentType = "application/json";

    var response = new ErrorResponse
    {
        Message = "Beklenmeyen bir hata oluştu.",
        Code = "INTERNAL_SERVER_ERROR"
    };

    await context.Response.WriteAsJsonAsync(response);
    }
    }
}