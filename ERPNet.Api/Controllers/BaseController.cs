using ERPNet.Application.Auth;
using ERPNet.Common;
using ERPNet.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected UsuarioContext UsuarioActual =>
        HttpContext.Items["UsuarioContext"] as UsuarioContext
        ?? throw new InvalidOperationException("UsuarioContext no disponible. Verifique que el middleware está configurado.");

    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return BuildErrorResponse(result);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return BuildErrorResponse(result);
    }

    protected IActionResult CreatedFromResult<T>(Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return BuildErrorResponse(result);
    }

    private IActionResult BuildErrorResponse(Result result)
    {
        var statusCode = result.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.InternalError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        if (result.ErrorType == ErrorType.Validation && result.Errors is { Count: > 0 })
        {
            var validationProblem = new ValidationProblemDetails
            {
                Status = statusCode,
                Title = "Error de validacion."
            };

            validationProblem.Errors.Add("General", result.Errors.ToArray());

            return new ObjectResult(validationProblem) { StatusCode = statusCode };
        }

        return Problem(
            detail: result.Error,
            statusCode: statusCode,
            title: GetTitle(result.ErrorType));
    }

    private static string GetTitle(ErrorType? errorType) => errorType switch
    {
        ErrorType.Validation => "Error de validacion.",
        ErrorType.NotFound => "Recurso no encontrado.",
        ErrorType.Conflict => "Conflicto.",
        ErrorType.Unauthorized => "No autorizado.",
        ErrorType.InternalError => "Error interno del servidor.",
        _ => "Error."
    };
}
