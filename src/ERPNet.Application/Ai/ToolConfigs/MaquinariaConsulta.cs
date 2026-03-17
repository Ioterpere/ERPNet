using ERPNet.Domain.Filters;

namespace ERPNet.Application.Ai.ToolConfigs;

public class MaquinariaConsulta : ConsultaToolConfig<PaginacionFilter>
{
    public override string Entidad => "maquinas";
    public override string Nombre => "BuscarMaquinaria";
    public override string Descripcion => "Busca maquinaria por nombre o referencia.";

    protected override void Configurar()
    {
        Describir(x => x.Busqueda, "Texto libre (nombre, referencia)");
        Describir(x => x.Pagina, "Página (desde 0)");
        Describir(x => x.PorPagina, "Registros por página (máx 50)");
    }
}
