using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class RefreshToken : ISoftDeletable
{
    public int Id { get; set; }
    public string Token { get; set; } = null!; // SHA-256 hash
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaRevocacion { get; set; }
    public string? ReemplazadoPor { get; set; } // Hash del token reemplazo

    public Usuario Usuario { get; set; } = null!;

    public bool IsExpirado => DateTime.UtcNow >= FechaExpiracion;
    public bool IsRevocado => FechaRevocacion is not null;
    public bool IsActivo => !IsExpirado && !IsRevocado;
    public bool IsDeleted { get; set; }
}
