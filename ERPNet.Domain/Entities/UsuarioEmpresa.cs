using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class UsuarioEmpresa : ISoftDeletable
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public bool IsDeleted { get; set; }
}
