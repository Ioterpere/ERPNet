using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace ERPNet.Web.Blazor.Auth;

public class BffTokenTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }

    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            var tokenCookieService = transformContext.HttpContext.RequestServices.GetRequiredService<ITokenCookieService>();
            var accessToken = await tokenCookieService.GetValidAccessTokenAsync(transformContext.HttpContext);

            if (accessToken is not null)
            {
                transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            // Forward client IP for rate limiting / logging
            var clientIp = transformContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if (clientIp is not null)
            {
                transformContext.ProxyRequest.Headers.Remove("X-Forwarded-For");
                transformContext.ProxyRequest.Headers.Add("X-Forwarded-For", clientIp);
            }
        });
    }
}
