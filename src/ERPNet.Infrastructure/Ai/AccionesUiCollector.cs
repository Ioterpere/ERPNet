using ERPNet.Application.Ai;
using ERPNet.Application.Ai.DTOs;

namespace ERPNet.Infrastructure.Ai;

public class AccionesUiCollector : IAccionesUiCollector
{
    private AccionUi? _accion;

    public void Guardar(AccionUi accion) => _accion = accion;
    public AccionUi? Obtener() => _accion;
}
