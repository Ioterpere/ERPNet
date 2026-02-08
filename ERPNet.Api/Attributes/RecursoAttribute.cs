namespace ERPNet.Api.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RecursoAttribute(string codigo) : Attribute
{
    public string Codigo { get; } = codigo;
}
