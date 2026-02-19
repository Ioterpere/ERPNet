namespace ERPNet.Contracts.FileStorage;

public record ArchivoDescarga(Func<Stream, CancellationToken, Task> EscribirContenido, string ContentType, string NombreArchivo);
