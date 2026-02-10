using ERPNet.Domain.Enums;

namespace ERPNet.Api.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RecursoAttribute(RecursoCodigo codigo) : Attribute
{
    public RecursoCodigo Codigo { get; } = codigo;
}
