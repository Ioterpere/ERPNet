using ERPNet.Application.Ai.DTOs;

namespace ERPNet.Application.Ai;

public interface IAccionesUiCollector
{
    void Guardar(AccionUi accion);
    AccionUi? Obtener();
}
