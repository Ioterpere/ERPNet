using ERPNet.Application.Mailing;
using ERPNet.Application.Mailing.Models;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Handlers;

public class GlobalExceptionHandler(
    IHostEnvironment env,
    IServiceScopeFactory scopeFactory)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string codigoError = exception.GetHashCode().ToString();

        var modelo = new ExcepcionEmailModel(
            Mensaje: exception.Message,
            Codigo: codigoError,
            StackTrace: exception.StackTrace,
            Origen: exception.TargetSite?.DeclaringType?.FullName,
            InnerExceptionMensaje: exception.InnerException?.Message,
            InnerExceptionStackTrace: exception.InnerException?.StackTrace,
            MetodoHttp: httpContext.Request.Method,
            Ruta: httpContext.Request.Path,
            QueryString: httpContext.Request.QueryString.Value,
            UsuarioEmail: httpContext.User.Identity?.Name,
            DireccionIp: httpContext.Connection.RemoteIpAddress?.ToString(),
            Fecha: DateTime.UtcNow);

        using (var scope = scopeFactory.CreateScope())
        {
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            await logService.ErrorAsync(exception, codigoError, cancellationToken);

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

            var emails = await usuarioRepository.GetEmailsByRolAsync("Administrador");
            foreach (var email in emails)
            {
                await emailService.EnviarAsync(new MensajeEmail(
                    email,
                    $"[ERPNet] Excepcion: {exception.GetType().Name}",
                    PlantillaEmail.Excepcion,
                    modelo));
            }
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error interno del servidor.",
            Detail = env.IsDevelopment() ? exception.ToString() : $"Error inesperado. Código: {codigoError}"
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
