using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Ai.ToolConfigs;

public class CreateEmpleadoFormulario : FormularioToolConfig<CreateEmpleadoRequest>
{
    public override RecursoCodigo Recurso => RecursoCodigo.Empleados;
    public override string Nombre => "RellenarFormularioEmpleado";
    public override string Descripcion =>
        "Muestra el formulario de creación de empleado y lo rellena con los datos indicados. " +
        "Llama primero a BuscarRutaEnMenu para obtener la ruta. " +
        "Usa BuscarSecciones para resolver el seccionId y BuscarEmpleados para el encargadoId.";

    protected override void Configurar()
    {
        Describir(x => x.Nombre, "Nombre real del empleado tal como lo dijo el usuario.");
        Describir(x => x.Apellidos, "Apellidos reales del empleado tal como los dijo el usuario.");
        Describir(x => x.Dni, "DNI real del empleado tal como lo dijo el usuario.");
        Describir(x => x.SeccionId, "ID numérico de la sección, obtenido con BuscarSecciones.");
        Describir(x => x.EncargadoId, "ID numérico del encargado, obtenido con BuscarEmpleados. Null si no se ha indicado.");
    }
}
