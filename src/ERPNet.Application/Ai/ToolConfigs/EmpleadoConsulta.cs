using ERPNet.Domain.Filters;

namespace ERPNet.Application.Ai.ToolConfigs;

public class EmpleadoConsulta : ConsultaToolConfig<EmpleadoFilter>
{
    public override string Entidad => "empleados";
    public override string Nombre => "BuscarEmpleados";
    public override string Descripcion => "Busca empleados por nombre, apellidos o DNI.";

    protected override void Configurar()
    {
        Describir(x => x.Busqueda, "Texto libre (nombre, apellidos o DNI)");
        Describir(x => x.SeccionId, "Filtrar por ID de sección");
        Describir(x => x.Activo, "true = solo activos, false = solo inactivos, null = todos");
        Describir(x => x.Pagina, "Página (desde 0)");
        Describir(x => x.PorPagina, "Registros por página (máx 50)");
    }
}
