using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Enums;

namespace ERPNet.Application.Ai.ToolConfigs;

public class CreateUsuarioFormulario : FormularioToolConfig<CreateUsuarioRequest>
{
    public override RecursoCodigo Recurso => RecursoCodigo.Aplicacion;
    public override string Nombre => "RellenarFormularioUsuario";
    public override string Descripcion =>
        "Muestra el formulario de creación de usuario y lo rellena con los datos indicados. " +
        "Llama primero a BuscarRutaEnMenu para obtener la ruta. " +
        "Usa BuscarEmpleados para resolver el empleadoId.";

    protected override void Configurar()
    {
        Describir(x => x.Email, "Email real del usuario tal como lo dijo el usuario.");
        Describir(x => x.EmpleadoId, "ID numérico del empleado vinculado, obtenido con BuscarEmpleados.");
    }
}
