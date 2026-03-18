using System.Security.Claims;
using System.Text.Json;
using ERPNet.Api.Attributes;
using ERPNet.Application.Ai;
using ERPNet.Application.Ai.DTOs;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ERPNet.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Recurso(RecursoCodigo.AsistenteIa)]
public class AiController(
    IAiChatService chatService,
    IOptions<AiSettings> settings) : ControllerBase
{
    /// <summary>Comprueba si el asistente IA está habilitado para el usuario actual.</summary>
    [HttpGet("acceso")]
    [SinPermiso]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Acceso() =>
        settings.Value.Habilitado ? Ok() : StatusCode(503);

    /// <summary>Devuelve los nombres de las herramientas disponibles para el usuario actual.</summary>
    [HttpGet("herramientas")]
    [SinPermiso]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public IActionResult Herramientas() => Ok(chatService.ObtenerNombresHerramientas());

    /// <summary>Crea una sesión de chat y devuelve su ID.</summary>
    [HttpPost("sesiones")]
    [EnableRateLimiting("ai-sesiones")]
    [ProducesResponseType<CrearSesionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CrearSesion()
    {
        if (!settings.Value.Habilitado) return StatusCode(503);
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var sessionId = await chatService.CrearSesionAsync(usuarioId);
        return Ok(new CrearSesionResponse { SessionId = sessionId });
    }

    /// <summary>Envía un mensaje y recibe la respuesta como stream SSE (text/event-stream).</summary>
    [HttpPost("sesiones/{sessionId}/mensajes")]
    [EnableRateLimiting("ai-stream")]
    [ProducesResponseType<ChatStreamEvent>(StatusCodes.Status200OK)]
    public async Task MensajeStream(string sessionId, [FromBody] NuevoMensajeRequest request, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        await foreach (var evento in chatService.ChatStreamAsync(sessionId, usuarioId, request, ct))
        {
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(evento, _jsonOpts)}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
