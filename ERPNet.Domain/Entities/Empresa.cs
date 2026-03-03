using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Empresa : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public string? Cif { get; set; }
    public bool Activo { get; set; }

    public ICollection<UsuarioEmpresa> UsuarioEmpresas { get; set; } = [];
}
