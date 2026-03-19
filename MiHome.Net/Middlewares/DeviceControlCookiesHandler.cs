using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MiHome.Net.Dto;
using MiHome.Net.Service;

namespace MiHome.Net.Middlewares;

internal class DeviceControlCookiesHandler : DelegatingHandler
{
    private readonly ICookieContainer _cookie;
    private readonly IMiAuth          _auth;

    public DeviceControlCookiesHandler([FromKeyedServices(Constants.MI_CONTROL_DEVICE_NAME)] ICookieContainer cookie,
        IMiAuth auth)
    {
        _cookie = cookie;
        _auth = auth;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var cookie = _cookie.CookieContainer;
        var url = new Uri(Constants.MI_CONTROL_DEVICE_URL);

        // 尝试获取登录信息
        LoginInfoDto loginInfo;
        try
        {
            loginInfo = await _auth.GetLoginInfoAsync();
        }
        catch
        {
#if DEBUG
            throw;
#endif
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        // 设置认证cookie
        cookie.Add(url, new Cookie("userId", loginInfo.UserId.ToString()));
        cookie.Add(url, new Cookie("serviceToken", loginInfo.ServiceToken));
        cookie.Add(url, new Cookie("yetAnotherServiceToken", loginInfo.ServiceToken));
        cookie.Add(url, new Cookie("is_daylight", "0"));
        cookie.Add(url, new Cookie("channel", "MI_APP_STORE"));
        cookie.Add(url, new Cookie("dst_offset", "0"));
        cookie.Add(url, new Cookie("locale", "zh_CN"));
        cookie.Add(url, new Cookie("timezone", "GMT+08:00"));
        cookie.Add(url, new Cookie("sdkVersion", "3.9"));
        cookie.Add(url, new Cookie("deviceId", loginInfo.DeviceId));

        return await base.SendAsync(request, cancellationToken);
    }
}