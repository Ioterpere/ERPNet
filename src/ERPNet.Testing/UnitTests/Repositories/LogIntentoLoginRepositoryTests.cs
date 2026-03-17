using ERPNet.Domain.Entities;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class LogIntentoLoginRepositoryTests : RepositoryTestBase
{
    private readonly LogIntentoLoginRepository _sut;

    public LogIntentoLoginRepositoryTests()
    {
        _sut = new LogIntentoLoginRepository(Context);
    }

    private static LogIntentoLogin CrearIntento(long id, string email, string ip, bool exitoso, DateTime fecha) => new()
    {
        Id = id,
        NombreUsuario = email,
        DireccionIp = ip,
        Exitoso = exitoso,
        FechaIntento = fecha
    };

    #region CountRecentFailedByEmailAsync

    [Fact(DisplayName = "CountByEmail: cuenta solo fallidos recientes del email")]
    public async Task CountByEmail_CuentaFallidosRecientes()
    {
        var ahora = DateTime.UtcNow;
        Context.IntentosLogin.AddRange(
            CrearIntento(1, "user@t.com", "1.1.1.1", false, ahora.AddMinutes(-5)),
            CrearIntento(2, "user@t.com", "1.1.1.1", false, ahora.AddMinutes(-2)),
            CrearIntento(3, "user@t.com", "1.1.1.1", true, ahora.AddMinutes(-1)),   // exitoso
            CrearIntento(4, "otro@t.com", "1.1.1.1", false, ahora.AddMinutes(-1)),   // otro email
            CrearIntento(5, "user@t.com", "1.1.1.1", false, ahora.AddMinutes(-20))); // fuera de ventana
        await SaveAndClearAsync();

        var result = await _sut.CountRecentFailedByEmailAsync("user@t.com", ahora.AddMinutes(-10));

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "CountByEmail: sin intentos devuelve 0")]
    public async Task CountByEmail_SinIntentos_Cero()
    {
        var result = await _sut.CountRecentFailedByEmailAsync("nada@t.com", DateTime.UtcNow.AddMinutes(-10));

        Assert.Equal(0, result);
    }

    #endregion

    #region CountRecentFailedByIpAsync

    [Fact(DisplayName = "CountByIp: cuenta solo fallidos recientes de la IP")]
    public async Task CountByIp_CuentaFallidosRecientes()
    {
        var ahora = DateTime.UtcNow;
        Context.IntentosLogin.AddRange(
            CrearIntento(1, "a@t.com", "10.0.0.1", false, ahora.AddMinutes(-5)),
            CrearIntento(2, "b@t.com", "10.0.0.1", false, ahora.AddMinutes(-2)),
            CrearIntento(3, "c@t.com", "10.0.0.1", true, ahora.AddMinutes(-1)),    // exitoso
            CrearIntento(4, "d@t.com", "10.0.0.2", false, ahora.AddMinutes(-1)),    // otra IP
            CrearIntento(5, "e@t.com", "10.0.0.1", false, ahora.AddMinutes(-20)));  // fuera de ventana
        await SaveAndClearAsync();

        var result = await _sut.CountRecentFailedByIpAsync("10.0.0.1", ahora.AddMinutes(-10));

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "CountByIp: sin intentos devuelve 0")]
    public async Task CountByIp_SinIntentos_Cero()
    {
        var result = await _sut.CountRecentFailedByIpAsync("192.168.0.1", DateTime.UtcNow.AddMinutes(-10));

        Assert.Equal(0, result);
    }

    #endregion

    #region AddAsync

    [Fact(DisplayName = "Add: persiste intento en BD")]
    public async Task Add_PersisteIntento()
    {
        var intento = CrearIntento(0, "test@t.com", "127.0.0.1", false, DateTime.UtcNow);

        await _sut.AddAsync(intento);
        await SaveAndClearAsync();

        var result = await _sut.CountRecentFailedByEmailAsync("test@t.com", DateTime.UtcNow.AddMinutes(-1));

        Assert.Equal(1, result);
    }

    #endregion
}
