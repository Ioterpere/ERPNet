using ERPNet.Domain.Filters;

namespace ERPNet.Application.Ai.ToolConfigs;

public class UsuarioConsulta : ConsultaToolConfig<PaginacionFilter>
{
    public override string Entidad => "usuarios";
    public override string Nombre => "BuscarUsuarios";
    public override string Descripcion => "Busca usuarios del sistema por email.";

    protected override void Configurar()
    {
        Describir(x => x.Busqueda, "Texto libre (email)");
        Describir(x => x.Pagina, "Página (desde 0)");
        Describir(x => x.PorPagina, "Registros por página (máx 50)");
    }
}
