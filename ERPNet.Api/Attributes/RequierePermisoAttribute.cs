namespace ERPNet.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequierePermisoAttribute : Attribute
{
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
