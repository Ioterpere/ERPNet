using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Turno : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public int ToleranciaMinutos { get; set; }

    public TimeOnly? LunesEntrada { get; set; }
    public TimeOnly? LunesSalida { get; set; }
    public TimeOnly? MartesEntrada { get; set; }
    public TimeOnly? MartesSalida { get; set; }
    public TimeOnly? MiercolesEntrada { get; set; }
    public TimeOnly? MiercolesSalida { get; set; }
    public TimeOnly? JuevesEntrada { get; set; }
    public TimeOnly? JuevesSalida { get; set; }
    public TimeOnly? ViernesEntrada { get; set; }
    public TimeOnly? ViernesSalida { get; set; }
    public TimeOnly? SabadoEntrada { get; set; }
    public TimeOnly? SabadoSalida { get; set; }
    public TimeOnly? DomingoEntrada { get; set; }
    public TimeOnly? DomingoSalida { get; set; }

    public ICollection<AsignacionTurno> AsignacionesTurno { get; set; } = [];
}
