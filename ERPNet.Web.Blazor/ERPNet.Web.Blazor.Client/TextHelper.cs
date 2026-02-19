using System.Globalization;
using System.Text;

namespace ERPNet.Web.Blazor.Client;

/// <summary>
/// Utilidades de texto para búsquedas en la UI.
/// </summary>
public static class TextHelper
{
    /// <summary>
    /// Convierte el texto a minúsculas y elimina los diacríticos (tildes, ü, ñ…)
    /// para que las comparaciones sean case-insensitive e insensibles a acentos.
    /// </summary>
    public static string Normalizar(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    /// <summary>
    /// Devuelve true si <paramref name="texto"/> contiene <paramref name="query"/>
    /// ignorando mayúsculas/minúsculas y acentos.
    /// </summary>
    public static bool ContieneBusqueda(string? texto, string query)
        => texto is not null && Normalizar(texto).Contains(query);
}
