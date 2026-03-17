using System.Text.Json.Serialization;

namespace ERPNet.Application.Ai.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoAccionUi
{
    RellenarFormulario,
    AbrirRegistro,
    ElegirOpcion
}

public record AccionUi
{
    public TipoAccionUi Tipo { get; init; }
    public required string TipoDato { get; init; }
    public string? Ruta { get; init; }
    public object? Datos { get; init; }
    public List<ItemSeleccionable>? Opciones { get; init; }
}
