using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using MiHome.Net.Apis;
using MiHome.Net.Dto;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace MiHome.Net.Service;

public interface IMiAuth
{
    /// <summary>
    /// 请求登录链接进行登录
    /// </summary>
    /// <returns>返回登录链接与检查链接</returns>
    /// <exception cref="Refit.ApiException">请求失败</exception>
    /// <exception cref="Exception">请求返回异常</exception>
    Task<(string, string)> RequestLogin();

    /// <summary>
    /// 查询登录结果，完成登录
    /// </summary>
    /// <param name="url">用来查询的url</param>
    /// <returns>成功时返回<see cref="LoginState.Success"/>, 超时返回<see cref="LoginState.Timeout"/></returns>
    /// <exception cref="Refit.ApiException">请求失败</exception>
    /// <exception cref="Exception">请求返回异常</exception>
    Task<LoginState> FinishLogin(string url);

    /// <summary>
    /// 退出登录
    /// </summary>
    /// <returns></returns>
    Task Logout();

    /// <summary>
    /// 获取登录信息，从缓存或者真正登录获取
    /// </summary>
    /// <returns></returns>
    internal Task<LoginInfoDto> GetLoginInfoAsync();

    internal Task RefreshToken(LoginInfoDto loginInfo);
}

internal class MiAuth : IMiAuth
{
    private readonly ILogger<MiAuth>      _logger;
    private readonly IXiaoMiLoginApi      _xiaoMiLoginApi;
    private readonly IMiAuthStateProvider _authStateProvider;
    private readonly ICookieContainer        _cookie;
    private readonly IHttpClientFactory   _factory;

    public MiAuth(ILogger<MiAuth> logger, IXiaoMiLoginApi xiaoMiLoginApi, IMiAuthStateProvider authStateProvider,
        [FromKeyedServices(Constants.MI_LOGIN_NAME)]ICookieContainer cookie, IHttpClientFactory factory)
    {
        _logger = logger;
        _xiaoMiLoginApi = xiaoMiLoginApi;
        _authStateProvider = authStateProvider;
        _cookie = cookie;
        _factory = factory;
    }


    /// <inheritdoc />
    public async Task<(string, string)> RequestLogin()
    {
        var clientId = GetClientId();
        var url = new Uri(Constants.MI_LOGIN_URL);
        var cookie = _cookie.CookieContainer;
        cookie.Add(url, new Cookie("sdkVersion", "3.9"));
        cookie.Add(url, new Cookie("deviceId", clientId));

        var result = await _xiaoMiLoginApi.ServiceLogin(clientId);

        if (result.Code == 0)
        {
            throw new Exception("已经登陆，重复请求登录");
        }

        var millis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dc2 = millis.ToString();

        var location = result.Location;
        var uri = new Uri(location);
        var query = uri.Query;
        var queryDict = HttpUtility.ParseQueryString(query);
        var serviceParam = queryDict["serviceParam"] ?? "";

        var qrCodeParam = new QrCodeLoginInputDto
        {
            Qs = result.Qs!,
            Callback = result.Callback!,
            ServiceParam = serviceParam,
            Sign = result.Sign!,
            Dc = dc2
        };
        var loginUrlResult = await _xiaoMiLoginApi.LoginUrl(qrCodeParam);

        if (loginUrlResult.Code != 0)
        {
            throw new Exception("登录失败,原因：第2步,获取二维码信息失败," + JsonSerializer.Serialize(loginUrlResult));
        }

        return (loginUrlResult.LoginUrl, loginUrlResult.Lp);
    }

    /// <inheritdoc />
    public async Task<LoginState> FinishLogin(string checkUrl)
    {
        var clientId = GetClientId();
        var url = new Uri(Constants.MI_LOGIN_URL);
        var cookie = _cookie.CookieContainer;
        cookie.Add(url, new Cookie("sdkVersion", "3.9"));
        cookie.Add(url, new Cookie("deviceId", clientId));

        QrCodeLogin2OutputDto? qrCodeLoginResult;
        var client = _factory.CreateClient("MiLogin");
        client.BaseAddress = null;
        try
        {
            qrCodeLoginResult =
                await client.GetFromJsonAsync<QrCodeLogin2OutputDto>(checkUrl, JsonSerializerOptions.Web);
        }
        catch (OperationCanceledException)
        {
            return LoginState.Timeout;
        }

        if (qrCodeLoginResult == null)
        {
            throw new Exception("登录失败,原因：第4步解析失败");
        }

        if (qrCodeLoginResult.Code != 0)
        {
            throw new Exception("登录失败,原因：第4步" + qrCodeLoginResult.Desc);
        }

        var result3 = await client.GetAsync(qrCodeLoginResult.Location);
        result3.EnsureSuccessStatusCode();
        var cookies = result3.Headers.Where(it => it.Key == "Set-Cookie")
            .SelectMany(it => it.Value)
            .Select(it => it.Split(";")[0])
            .ToList();
        if (cookies.Count == 0)
        {
            throw new Exception("登录失败,原因：第5步，Get ServiceToken Error");
        }

        var serviceToken = cookies.FirstOrDefault(it => it.StartsWith("serviceToken"))
            ?.Replace("serviceToken=", "");
        var passO = cookies.FirstOrDefault(x => x.StartsWith("pass_o"))?.Replace("pass_o=", "") ??
                    Random.Shared.GetHexString(16, true);
        var loginInfo = new LoginInfoDto
        {
            DeviceId = clientId,
            ServiceToken = serviceToken ?? throw new Exception("serviceToken == null"),
            Ssecurity = qrCodeLoginResult.Ssecurity,
            UserId = qrCodeLoginResult.UserId,
            PassO = passO,
            PassToken = qrCodeLoginResult.PassToken,
            CUserId = qrCodeLoginResult.CUserId,
            ExpireTime = DateTimeOffset.UtcNow.AddDays(28),
        };

        await _authStateProvider.UpdateLoginInfo(loginInfo);
        return LoginState.Success;
    }

    /// <inheritdoc />
    public Task Logout()
    {
        return _authStateProvider.Expire();
    }

    /// <inheritdoc />
    public async Task<LoginInfoDto> GetLoginInfoAsync()
    {
        var loginInfoDto = await _authStateProvider.GetLoginInfo();

        if (loginInfoDto != null)
        {
            var expireTime = loginInfoDto.ExpireTime ?? DateTimeOffset.UtcNow;
            var timeleft = expireTime - DateTimeOffset.UtcNow;

            // 可用时间大于7天时返回
            if (timeleft.Days >= 7)
                return loginInfoDto;

            // 可用时间不足7天，且大于0时，尝试刷新Token
            try
            {
                if (timeleft.Minutes >= 5)
                    await RefreshToken(loginInfoDto);
            }
            catch
            {
                await Logout();
                throw;
            }

            // 尝试获取新的Token
            loginInfoDto = await _authStateProvider.GetLoginInfo();
            return loginInfoDto ?? throw new Exception("无法刷新Token");
        }

        await Logout();
        throw new Exception("登录信息已过期，请重新登录");
    }

    /// <inheritdoc />
    public async Task RefreshToken(LoginInfoDto loginInfo)
    {
        var clientId = GetClientId();
        var url = new Uri(Constants.MI_LOGIN_URL);
        var cookie = _cookie.CookieContainer;
        cookie.Add(url, new Cookie("sdkVersion", "3.9"));
        cookie.Add(url, new Cookie("deviceId", clientId));
        cookie.Add(url, new Cookie("pass_o", loginInfo.PassO));
        cookie.Add(url, new Cookie("passToken", loginInfo.PassToken));
        cookie.Add(url, new Cookie("userId", loginInfo.UserId.ToString()));
        cookie.Add(url, new Cookie("cUserId", loginInfo.CUserId));

        var result = await _xiaoMiLoginApi.ServiceLogin(clientId);

        if (result.Code != 0)
        {
            throw new Exception("刷新失败，原因：第一步 code != 0");
        }

        var location = result.Location;

        var client = _factory.CreateClient("MiLogin");
        client.BaseAddress = null;
        var loginResponse = await client.GetAsync(location);
        loginResponse.EnsureSuccessStatusCode();

        var cookies = cookie.GetAllCookies();

        var serviceToken = cookies.FirstOrDefault(it => it.Name == "serviceToken")?.Value;

        loginInfo.ServiceToken = serviceToken ?? throw new Exception("serviceToken == null");
        loginInfo.CUserId = result.CUserId ?? throw new Exception("cUserId == null");
        loginInfo.Ssecurity = result.Ssecurity ?? throw new Exception("ssecurity == null");
        loginInfo.PassToken = result.PassToken ?? throw new Exception("passToken == null");
        loginInfo.ExpireTime = DateTimeOffset.UtcNow.AddDays(28);

        await _authStateProvider.UpdateLoginInfo(loginInfo);
    }

    private static string GetClientId()
    {
        return Random.Shared.GetString("ABCDEF", 13);
    }
}