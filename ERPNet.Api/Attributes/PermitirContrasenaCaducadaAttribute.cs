namespace ERPNet.Api.Attributes;

/// <summary>
/// Permite acceder al endpoint aunque la contraseña del usuario haya caducado.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PermitirContrasenaCaducadaAttribute : Attribute;
