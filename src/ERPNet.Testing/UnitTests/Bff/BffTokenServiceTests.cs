using ERPNet.Web.Blazor.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ERPNet.Testing.UnitTests.Bff;

sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    : HttpMessageHandler
{
    private int _callCount;
    public int CallCount => _callCount;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        return await handler(request);
    }
}

public class BffTokenServiceTests
{
    private const string SessionKey = "test-session-abc";

    private static readonly string RefreshResponseJson = JsonSerializer.Serialize(new
    {
        accessToken = "new-access-token",
        refreshToken = "new-refresh-token",
        expiration = DateTime.UtcNow.AddMinutes(30),
        requiereCambioContrasena = false
    });

    private static IHttpContextAccessor BuildContextAccessor()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var context = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("session_key", SessionKey)]));

        context.User.Returns(user);
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    private static IDistributedCache BuildCache(BffTokenData? initial = null)
    {
        var cache = Substitute.For<IDistributedCache>();
        var store = new Dictionary<string, byte[]>();

        if (initial is not null)
            store[$"bff-token:{SessionKey}"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initial));

        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(ci => store.TryGetValue(ci.Arg<string>(), out var v) ? v : null);

        cache.SetAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>())
             .Returns(ci =>
             {
                 store[ci.ArgAt<string>(0)] = ci.ArgAt<byte[]>(1);
                 return Task.CompletedTask;
             });

        cache.RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(ci => { store.Remove(ci.Arg<string>()); return Task.CompletedTask; });

        return cache;
    }

    private static (BffTokenService sut, FakeHttpMessageHandler fakeHandler) BuildSut(
        BffTokenData? tokenInCache = null,
        Func<HttpRequestMessage, Task<HttpResponseMessage>>? refreshResponse = null)
    {
        var fakeHandler = new FakeHttpMessageHandler(
            refreshResponse ?? (_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RefreshResponseJson, Encoding.UTF8, "application/json")
            })));

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("ErpNetApi")
               .Returns(new HttpClient(fakeHandler) { BaseAddress = new Uri("http://api-test") });

        return (new BffTokenService(factory, BuildCache(tokenInCache), BuildContextAccessor()), fakeHandler);
    }

    [Fact(DisplayName = "Token fresco: devuelve el access token sin llamar a la API")]
    public async Task GetAccessToken_TokenFresco_NoLlamaAPI()
    {
        var tokenData = new BffTokenData
        {
            AccessToken = "access-vigente",
            RefreshToken = "refresh-vigente",
            Expiration = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        var (sut, fakeHandler) = BuildSut(tokenData);

        var result = await sut.GetAccessTokenAsync();

        Assert.Equal("access-vigente", result);
        Assert.Equal(0, fakeHandler.CallCount);
    }

    [Fact(DisplayName = "Token expirado: llama a la API y devuelve el token nuevo")]
    public async Task GetAccessToken_TokenExpirado_RefrescaYDevuelveNuevo()
    {
        var tokenData = new BffTokenData
        {
            AccessToken = "access-viejo",
            RefreshToken = "refresh-viejo",
            Expiration = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var (sut, fakeHandler) = BuildSut(tokenData);

        var result = await sut.GetAccessTokenAsync();

        Assert.Equal("new-access-token", result);
        Assert.Equal(1, fakeHandler.CallCount);
    }

    [Fact(DisplayName = "Race condition: 5 peticiones concurrentes con token expirado solo refrescan una vez")]
    public async Task GetAccessToken_Concurrente_SoloRefresca1Vez()
    {
        var tokenData = new BffTokenData
        {
            AccessToken = "access-viejo",
            RefreshToken = "refresh-viejo",
            Expiration = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var (sut, fakeHandler) = BuildSut(tokenData, async _ =>
        {
            await Task.Delay(30); // delay para maximizar la contención entre tareas
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(RefreshResponseJson, Encoding.UTF8, "application/json")
            };
        });

        var results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => sut.GetAccessTokenAsync()));

        Assert.Equal(1, fakeHandler.CallCount);
        Assert.All(results, token => Assert.Equal("new-access-token", token));
    }

    // Demuestra el problema original: sin lock, N peticiones concurrentes harían N llamadas a la API,
    // rotando el refresh token N veces. La segunda llamada ya encontraría el token revocado → sesión cerrada.
    [Fact(DisplayName = "Sin lock: peticiones concurrentes llamarían a la API N veces")]
    public async Task SinLock_DemuestraRaceCondition()
    {
        int callCount = 0;
        var tokenExpirado = DateTimeOffset.UtcNow.AddMinutes(-5);

        async Task<string> RefrescarSinLockAsync()
        {
            if (tokenExpirado <= DateTimeOffset.UtcNow.AddMinutes(2))
            {
                await Task.Delay(30);
                Interlocked.Increment(ref callCount);
            }
            return "token";
        }

        await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => RefrescarSinLockAsync()));

        Assert.Equal(5, callCount);
    }
}
