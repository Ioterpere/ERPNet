using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class RolUsuario : ISoftDeletable
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public bool IsDeleted { get; set; }
}
