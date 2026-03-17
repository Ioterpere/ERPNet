using System.Text.Json.Serialization;

namespace ERPNet.Application.Ai.DTOs;

public record ArchivoAdjunto
{
    public required string Nombre { get; init; }
    public required string ContentType { get; init; }
    public required string DatosBase64 { get; init; }
}

public record NuevoMensajeRequest
{
    public required string Mensaje { get; init; }
    public IReadOnlyList<ArchivoAdjunto>? Adjuntos { get; init; }
    /// <summary>Ruta de la página activa en el cliente (ej: /empleados). Permite al AI deducir contexto sin buscar en el menú.</summary>
    public string? PaginaActual { get; init; }
}

public record CrearSesionResponse
{
    public required string SessionId { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoChatStreamEvent { Herramienta, Fin }

public record ChatStreamEvent
{
    public TipoChatStreamEvent Tipo { get; init; }
    /// <summary>Nombre del tool en ejecución. Solo presente en Herramienta.</summary>
    public string? Contenido { get; init; }
    /// <summary>Texto final del asistente. Solo presente en Fin.</summary>
    public string? Texto { get; init; }
    /// <summary>Acción UI resultante. Solo presente en Fin.</summary>
    public AccionUi? Accion { get; init; }
}
