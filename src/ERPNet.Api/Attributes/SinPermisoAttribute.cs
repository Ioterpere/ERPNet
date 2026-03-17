namespace ERPNet.Api.Attributes;

/// <summary>
/// Permite evitar el control de acceso por roles para recursos en endpoints concretos
/// </summary>

[AttributeUsage(AttributeTargets.Method)]
public class SinPermisoAttribute : Attribute;
