using System.Text.Json.Serialization;

namespace ERPNet.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecursoCodigo
{
    /// <summary>
    /// Administración Interna: Usuarios, Roles, Menus, Logs...
    /// </summary>
    Aplicacion = 1,
    Empleados = 2,
    Vacaciones = 3,
    Turnos = 4,
    Marcajes = 5,
    Maquinaria = 6,
    Mantenimiento = 7,
    OrdenesFabrica = 8,
    Clientes = 9,
    Facturas = 10,
    Empresas = 11,
    AsistenteIa = 12,
    Articulos = 13
}
