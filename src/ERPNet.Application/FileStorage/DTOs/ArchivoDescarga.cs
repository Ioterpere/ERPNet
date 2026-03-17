namespace ERPNet.Application.FileStorage.DTOs;

public record ArchivoDescarga(Func<Stream, CancellationToken, Task> EscribirContenido, string ContentType, string NombreArchivo);
