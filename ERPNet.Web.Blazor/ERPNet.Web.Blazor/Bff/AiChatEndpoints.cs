using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace ERPNet.Web.Blazor.Bff;

/// <summary>
/// Endpoint BFF que actúa de proxy entre el cliente Blazor y la API de OpenAI.
/// Añade la API key (que nunca llega al browser), el modelo configurado y un
/// system prompt corporativo; reenvía el resto del payload tal cual.
/// </summary>
public static class AiChatEndpoints
{
    private const string SystemPrompt =
        "Eres un asistente del ERP ERPNet. Ayudas a los usuarios a gestionar empleados, " +
        "usuarios del sistema y otros recursos de la empresa. " +
        "Responde siempre en español de forma concisa y amigable. " +
        "REGLAS IMPORTANTES: " +
        "1) Cuando el usuario mencione a una persona por nombre, SIEMPRE usa buscar_empleados primero. " +
        "Si la búsqueda devuelve más de un resultado, DEBES presentar las opciones al usuario y preguntarle cuál es el correcto antes de continuar. " +
        "NUNCA asumas ni inventes un ID. " +
        "2) Cuando el usuario pida crear algo, usa la herramienta disponible para precargar el formulario " +
        "y confirma que ya puede revisarlo y pulsar el botón para guardar.";

    public static IEndpointConventionBuilder MapAiChat(this WebApplication app)
    {
        return app.MapPost("/bff/ai/chat", Handle)
            .RequireAuthorization()
            .DisableAntiforgery();
    }

    private static async Task Handle(
        HttpContext ctx,
        IConfiguration config,
        IHttpClientFactory factory)
    {
        var apiKey = config["OpenAi:ApiKey"];
        var modelo = config["OpenAi:Modelo"] ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"error\":\"OpenAI API key no configurada.\"}");
            return;
        }

        var payload = await JsonNode.ParseAsync(ctx.Request.Body);
        if (payload is not JsonObject body)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Añadir modelo desde configuración
        body["model"] = modelo;

        // Insertar system prompt al inicio de los mensajes
        if (body["messages"] is JsonArray messages)
        {
            messages.Insert(0, new JsonObject
            {
                ["role"] = "system",
                ["content"] = SystemPrompt
            });
        }

        var client = factory.CreateClient("OpenAi");
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        ctx.Response.StatusCode = (int)response.StatusCode;
        ctx.Response.ContentType = "application/json";
        await response.Content.CopyToAsync(ctx.Response.Body);
    }
}
