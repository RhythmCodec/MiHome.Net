using System.Collections.Immutable;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;
using MiHome.Net.Dto;
using MiHome.Net.FeignService;
using MiHome.Net.Utils;
using Newtonsoft.Json;
using SummerBoot.Core;
using SummerBoot.Feign;

namespace MiHome.Net.Service;

/// <summary>
/// 米家云端接口
/// </summary>
public interface IMiotCloud
{
    /// <summary>
    /// Get device specification information获取设备规格信息
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<MiotSpec> GetDeviceSpec(string model);

    /// <summary>
    /// Get a list of home devices获取家庭设备列表
    /// </summary>
    /// <returns></returns>
    Task<List<XiaoMiDeviceInfo>> GetDeviceListAsync();

    /// <summary>
    /// Get scene list
    /// 获取场景列表。
    /// </summary>
    /// <returns></returns>
    Task<List<SceneDto>> GetSceneListAsync(string homeId);

    /// <summary>
    /// Execution scenario
    /// 执行场景
    /// </summary>
    /// <param name="sceneId">场景id</param>
    /// <returns></returns>
    Task<bool> RunSceneAsync(string sceneId);

    /// <summary>
    /// 获取耗材列表
    /// </summary>
    /// <param name="homeId">家庭id</param>
    /// <returns></returns>
    Task<List<GetConsumableItemsOutputDto>> GetConsumableItemsAsync(string homeId);

    /// <summary>
    /// Get a list of families
    /// 获取家庭列表
    /// </summary>
    /// <returns></returns>
    Task<List<HomeDto>> GetHomeListAsync();

    /// <summary>
    /// Get properties in batches批量获取属性
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    Task<List<GetPropOutputItemDto>> GetPropertiesAsync(List<GetPropertyDto> properties);

    /// <summary>
    /// get property获取属性 
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    Task<GetPropOutputItemDto> GetPropertyAsync(GetPropertyDto property);

    /// <summary>
    /// set property设置属性
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    Task<SetPropOutputItemDto> SetPropertyAsync(SetPropertyDto property);

    /// <summary>
    /// Set properties in batches 批量设置属性
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    Task<List<SetPropOutputItemDto>> SetPropertiesAsync(List<SetPropertyDto> properties);

    /// <summary>
    /// Call device method调用设备方法
    /// </summary>
    /// <param name="callActionParam"></param>
    /// <returns></returns>
    Task<string> CallActionAsync(CallActionInputDto callActionParam);

    /// <summary>
    /// 登录
    /// </summary>
    /// <returns></returns>
    [Obsolete($"使用{nameof(RequestLogin)}和{nameof(FinishLogin)}进行登录", error: true)]
    Task LoginAsync();

    /// <summary>
    /// 退出
    /// </summary>
    /// <returns></returns>
    [Obsolete($"使用{nameof(Logout)}退出登录", error: true)]
    Task LogOutAsync();

    /// <summary>
    /// 请求登录链接进行登录
    /// </summary>
    /// <returns>返回登录链接与检查链接</returns>
    Task<(string, string)> RequestLogin();

    /// <summary>
    /// 查询登录结果，完成登录
    /// </summary>
    /// <param name="url">用来查询的url</param>
    /// <returns>成功时返回<see cref="LoginState.Success"/>, 超时返回<see cref="LoginState.Timeout"/></returns>
    Task<LoginState> FinishLogin(string url);

    /// <summary>
    /// 退出登录
    /// </summary>
    /// <returns></returns>
    Task Logout();
}

[AutoRegister(typeof(IMiotCloud))]
public class MIotCloud : IMiotCloud
{
    private readonly IMiotCloudService            miotCloudService;
    private readonly IXiaoMiLoginService          xiaoMiLoginService;
    private readonly IXiaoMiControlDevicesService xiaoMiControlDevicesService;
    private readonly IFeignUnitOfWork             fegiFeignUnitOfWork;
    private readonly ILogger<MiHomeDriver>        logger;
    private readonly IMiAuthStateProvider         authStateProvider;

    public MIotCloud(IMiotCloudService miotCloudService, IXiaoMiLoginService xiaoMiLoginService,
        IXiaoMiControlDevicesService xiaoMiControlDevicesService, IFeignUnitOfWork fegiFeignUnitOfWork,
        ILogger<MiHomeDriver> logger, IMiAuthStateProvider authStateProvider)
    {
        this.miotCloudService = miotCloudService;
        this.xiaoMiLoginService = xiaoMiLoginService;
        this.xiaoMiControlDevicesService = xiaoMiControlDevicesService;
        this.fegiFeignUnitOfWork = fegiFeignUnitOfWork;
        this.logger = logger;
        this.authStateProvider = authStateProvider;
    }

    public async Task<MiotSpec> GetDeviceSpec(string model)
    {
        var modelSchema = await GetModelSchema(model);
        return modelSchema;
    }

    private async Task<MiotSpec> GetModelSchema(string model)
    {
        var allInstances = await GetAllInstancesAsync();
        var modelInfo = allInstances.Instances.Where(it => it.Model == model).MaxBy(it => it.Version);
        if (modelInfo == null)
        {
            throw new Exception($"Device(model:{model} ) information not found");
        }

        var miotSpec = await GetSpecByDeviceType(modelInfo.Type, model);
        return miotSpec;
    }

    private async Task<GetAllInstanceResult> GetAllInstancesAsync()
    {
        var cacheFilePath = Path.Combine(AppContext.BaseDirectory, "allInstance.json");
        var fi = new FileInfo(cacheFilePath);
        if (fi.Exists && (DateTime.Now - fi.CreationTime).TotalHours < 24)
        {
            using var sw = new StreamReader(fi.OpenRead());
            var allInstancesTxt = await sw.ReadToEndAsync();
            var result = JsonConvert.DeserializeObject<GetAllInstanceResult>(allInstancesTxt);
            return result;
        }
        else
        {
            if (fi.Exists)
            {
                fi.Delete();
                await Task.Delay(50);
            }

            var instances = await miotCloudService.GetAllInstancesAsync();
            await using var sw = new StreamWriter(fi.OpenWrite());
            await sw.WriteAsync(instances.ToJson());
            return instances;
        }
    }

    private async Task<MiotSpec> GetSpecByDeviceType(string deviceType, string model)
    {
        var cacheFilePath = Path.Combine(AppContext.BaseDirectory, model + ".json");
        var fi = new FileInfo(cacheFilePath);
        if (fi.Exists && (DateTime.Now - fi.CreationTime).TotalHours < 24)
        {
            using var sw = new StreamReader(fi.OpenRead());
            var miotSpecTxt = await sw.ReadToEndAsync();
            var result = JsonConvert.DeserializeObject<MiotSpec>(miotSpecTxt);
            return result;
        }
        else
        {
            if (fi.Exists)
            {
                fi.Delete();
                await Task.Delay(50);
            }

            var miotSpec = await miotCloudService.GetSpecByDeviceType(deviceType);
            await using var sw = new StreamWriter(fi.OpenWrite());
            await sw.WriteAsync(miotSpec.ToJson());
            return miotSpec;
        }
    }

    /// <summary>
    ///关闭控制设备的cookie
    /// </summary>
    /// <returns></returns>
    private Task StopControlDeviceCookieAsync()
    {
        fegiFeignUnitOfWork.StopCookie();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 开启控制设备的cookie
    /// </summary>
    /// <returns></returns>
    private async Task BeginControlDeviceCookieAsync()
    {
        var loginInfoDto = await GetLoginInfoAsync();

        fegiFeignUnitOfWork.BeginCookie();
        var apiUrl = "https://api.io.mi.com/app/";
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("userId", loginInfoDto.UserId));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("serviceToken", loginInfoDto.ServiceToken));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("yetAnotherServiceToken", loginInfoDto.ServiceToken));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("is_daylight", "0"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("channel", "MI_APP_STORE"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("dst_offset", "0"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("locale", "zh_CN"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("timezone", "GMT+08:00"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("sdkVersion", "3.9"));
        fegiFeignUnitOfWork.AddCookie(apiUrl, new Cookie("deviceId", loginInfoDto.DeviceId));
    }

    /// <summary>
    /// 获取登录信息，从缓存或者真正登录获取
    /// </summary>
    /// <returns></returns>
    private async Task<LoginInfoDto> GetLoginInfoAsync()
    {
        var loginInfoDto = await authStateProvider.GetLoginInfo();

        if (loginInfoDto != null)
        {
            var expireTime = loginInfoDto.ExpireTime ?? DateTimeOffset.UtcNow;
            var timeleft = expireTime - DateTimeOffset.UtcNow;

            // 可用时间大于7天时返回
            if (timeleft.Days >= 7)
                return loginInfoDto;

            // 可用时间不足7天，且大于0时，尝试刷新Token
            if (timeleft.Minutes >= 5)
                await RefreshToken(loginInfoDto);

            // 递归调用，尝试获取新的Token
            return await GetLoginInfoAsync();
        }

        await Logout();
        throw new Exception("登录信息已过期，请重新登录");
    }

    private async Task RefreshToken(LoginInfoDto loginInfo)
    {
        fegiFeignUnitOfWork.BeginCookie();

        var clientId = GetClientId();
        var url = "https://account.xiaomi.com/";
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("sdkVersion", "3.9"));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("deviceId", clientId));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("pass_o", loginInfo.PassO));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("passToken", loginInfo.PassToken));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("userId", loginInfo.UserId));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("cUserId", loginInfo.CUserId));

        var result = await xiaoMiLoginService.ServiceLogin(clientId);
        var startValue = "&&&START&&&";

        if (result.HasText() && result.StartsWith(startValue))
        {
            result = result.Replace(startValue, "");
            var resultObj = JsonConvert.DeserializeObject<ServiceLoginResultDto>(result);
            if (resultObj == null)
            {
                throw new Exception("刷新失败,原因：第一步");
            }

            if (resultObj.code != 0)
            {
                throw new Exception("刷新失败，原因：第一步 code != 0");
            }

            var location = resultObj.location;

            var loginResponse = await xiaoMiLoginService.Login(location);
            loginResponse.EnsureSuccessStatusCode();

            var cookies = loginResponse.Headers.Where(x => x.Key == "Set-Cookie")
                .SelectMany(x => x.Value)
                .Select(x => x.Split(';')[0])
                .ToImmutableList();

            var serviceToken = cookies.FirstOrDefault(it => it.StartsWith("serviceToken"))
                ?.Replace("serviceToken=", "");
            fegiFeignUnitOfWork.StopCookie();

            loginInfo.ServiceToken = serviceToken ?? throw new Exception("serviceToken == null");
            loginInfo.CUserId = resultObj.cUserId ?? throw new Exception("cUserId == null");
            loginInfo.Ssecurity = resultObj.ssecurity ?? throw new Exception("ssecurity == null");
            loginInfo.PassToken = resultObj.passToken ?? throw new Exception("passToken == null");
            loginInfo.ExpireTime = DateTime.UtcNow;

            await authStateProvider.UpdateLoginInfo(loginInfo);
        }
    }
    

    private static string GetClientId()
    {
        return Random.Shared.GetString("ABCDEF", 13);
    }

    /// <summary>
    /// 对参数添加rc4加密
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="ssecurity"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetRc4Params(string method, string url, object data, string ssecurity)
    {
        var dat = new Dictionary<string, string>
        {
            ["data"] = data.ToJson()
        };

        var nonce = CalculateNonce();
        var signedNonce = SignedNonce(ssecurity, nonce);
        dat["rc4_hash__"] = Sha1Sign(url, dat, signedNonce, method);
        foreach (var pair in dat)
        {
            dat[pair.Key] = EncryptData(signedNonce, pair.Value);
        }

        dat["signature"] = Sha1Sign(url, dat, signedNonce, method);
        dat["_nonce"] = nonce;
        dat["ssecurity"] = ssecurity;
        dat["signedNonce"] = signedNonce;
        return dat;
    }

    /// <summary>
    /// 对数据使用sha1进行加密
    /// </summary>
    /// <param name="url"></param>
    /// <param name="dat"></param>
    /// <param name="nonce"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static string Sha1Sign(string url, Dictionary<string, string> dat, string nonce, string method = "POST")
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        if (path.Length > 5 && path[..5] == "/app/")
        {
            path = path[4..];
        }

        var arr = new List<string>
        {
            method.ToUpper(),
            path
        };
        arr.AddRange(dat.Select(pair => $"{pair.Key}={pair.Value}"));

        arr.Add(nonce);
        var sign = string.Join("&", arr);
        var result = sign.ToSha1ThenBase64();
        return result;
    }

    /// <summary>
    /// 加密消息体
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string EncryptData(string key, string data)
    {
        var p = new Rc4(key).Init1024().Crypt(data.GetBytes()).ToBase64();
        return p;
    }

    /// <summary>
    /// 解密消息体
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string DecryptData(string key, string data)
    {
        var baseData = Convert.FromBase64String(data);
        var p = new Rc4(key).Init1024().Crypt(baseData);
        return Encoding.UTF8.GetString(p);
    }

    /// <summary>
    /// 生成校验码
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private static string SignedNonce(string secret, string nonce)
    {
        var secretBytes = Convert.FromBase64String(secret);
        var nonceBytes = Convert.FromBase64String(nonce);
        var finalBytes = secretBytes.Concat(nonceBytes).ToArray();
        var finalResult = finalBytes.ToSha256ThenBase64();
        return finalResult;
    }

    /// <summary>
    /// 获取nonce一次性随机数
    /// </summary>
    /// <returns></returns>
    private static string CalculateNonce()
    {
        //Allocate a buffer
        Span<byte> buf = stackalloc byte[12];
        //Generate a cryptographically random set of bytes
        using (var rnd = RandomNumberGenerator.Create())
        {
            rnd.GetBytes(buf[..8]);
        }

        MemoryMarshal.Cast<byte, int>(buf[8..])[0] = DateTimeOffset.UtcNow.Second;
        //Base64 encode and then return
        return Convert.ToBase64String(buf);
    }

    public async Task<List<XiaoMiDeviceInfo>> GetDeviceListAsync()
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();
        var inputDto = new GetDeviceInputDto()
        {
            GetVirtualModel = true,
            GetHuamiDevices = 1,
            Get_split_device = false,
            Support_smart_home = true
        };
        var param = GetRc4Params("POST", "https://api.io.mi.com/app/home/device_list", inputDto,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var deviceListResultString = await xiaoMiControlDevicesService.GetDeviceList(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, deviceListResultString);
        var result = JsonConvert.DeserializeObject<GetDeviceListOutputResult>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result.List;
        }

        var errorMsg = $"get device List error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<List<GetConsumableItemsOutputDto>> GetConsumableItemsAsync(string homeId)
    {
        if (!decimal.TryParse(homeId, out _))
        {
            throw new NotSupportedException("homeId必须为数字");
        }

        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();

        var inputDto = new GetConsumableItemsInputDto()
        {
            home_id = long.Parse(homeId),
            owner_id = long.Parse(loginInfoDto.UserId),
            //dids = new List<string>(),
            //accessKey = "REMOVED",
            //filter_ignore = true
        };
        var param = GetRc4Params("POST", "https://api.io.mi.com/app/v2/home/standard_consumable_items", inputDto,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var deviceListResultString = await xiaoMiControlDevicesService.GetConsumableItems(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, deviceListResultString);
        var result = JsonConvert.DeserializeObject<GetConsumableItemsOutputResultDto>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result?.items ?? [];
        }

        var errorMsg = $"Get ConsumableItems error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<List<HomeDto>> GetHomeListAsync()
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();
        //data = {"fg": False, "fetch_share": True, "fetch_share_dev": True, "limit": 300, "app_ver": 7}
        var inputDto = new GetHomeInputDto()
        {
            fg = false,
            fetch_share = true,
            fetch_share_dev = true,
            limit = 300,
            app_ver = 7
        };
        var param = GetRc4Params("POST", "https://api.io.mi.com/app/v2/homeroom/gethome", inputDto,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var deviceListResultString = await xiaoMiControlDevicesService.GetHomeList(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, deviceListResultString);
        var result = JsonConvert.DeserializeObject<GetHomeOutputResultDto>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result.HomeList;
        }

        var errorMsg = $"get Home List error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<List<SceneDto>> GetSceneListAsync(string homeId)
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();

        var inputDto = new GetSceneInputDto()
        {
            home_id = homeId
        };
        var param = GetRc4Params("POST",
            "https://api.io.mi.com/app/appgateway/miot/appsceneservice/AppSceneService/GetSceneList", inputDto,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var deviceListResultString = await xiaoMiControlDevicesService.GetSceneList(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, deviceListResultString);
        var result = JsonConvert.DeserializeObject<GetSceneOutputResultDto>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result.SceneList;
        }

        var errorMsg = $"get Scene List error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<bool> RunSceneAsync(string sceneId)
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();

        var inputDto = new RunSceneInputDto()
        {
            scene_id = sceneId,
            trigger_key = "user.click"
        };
        var param = GetRc4Params("POST",
            "https://api.io.mi.com/app/appgateway/miot/appsceneservice/AppSceneService/RunScene", inputDto,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var deviceListResultString = await xiaoMiControlDevicesService.RunScene(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, deviceListResultString);
        var result = JsonConvert.DeserializeObject<RunSceneOutputResultDto>(decryptData);
        if (result?.Code == 0)
        {
            return true;
        }

        var errorMsg = $"get Scene List error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<List<GetPropOutputItemDto>> GetPropertiesAsync(List<GetPropertyDto> properties)
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();
        var postData = new GetPropPostData()
        {
            Params = properties
        };
        var param = GetRc4Params("POST", "https://api.io.mi.com/app/miotspec/prop/get", postData,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var resultString = await xiaoMiControlDevicesService.PropGet(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, resultString);
        var result = JsonConvert.DeserializeObject<GetPropOutputDto>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result;
        }

        var errorMsg = $"propGet error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<GetPropOutputItemDto> GetPropertyAsync(GetPropertyDto property)
    {
        var result = await this.GetPropertiesAsync([property]);
        return result.First();
    }

    public async Task<string> CallActionAsync(CallActionInputDto callActionParam)
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();
        var postData = new DoActionPostData()
        {
            AccessKey = "",
            Params = callActionParam
        };
        var param = GetRc4Params("POST", "https://api.io.mi.com/app/miotspec/action", postData,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var resultString = await xiaoMiControlDevicesService.ActionCall(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, resultString);
        var result = JsonConvert.DeserializeObject<CallActionOutputDto>(decryptData);
        if (result?.Code == 0)
        {
            return "";
        }

        var errorMsg = $"propGet error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<List<SetPropOutputItemDto>> SetPropertiesAsync(List<SetPropertyDto> properties)
    {
        await BeginControlDeviceCookieAsync();
        var loginInfoDto = await GetLoginInfoAsync();
        var postData = new SetPropPostData()
        {
            Params = properties
        };

        var param = GetRc4Params("POST", "https://api.io.mi.com/app/miotspec/prop/set", postData,
            loginInfoDto.Ssecurity);
        var signedNonce = param["signedNonce"];
        var resultString = await xiaoMiControlDevicesService.PropSet(param);
        await StopControlDeviceCookieAsync();
        var decryptData = DecryptData(signedNonce, resultString);
        var result = JsonConvert.DeserializeObject<SetPropOutputDto>(decryptData);
        if (result?.Code == 0)
        {
            return result.Result;
        }

        var errorMsg = $"propGet error,reason:{result?.Message}";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<SetPropOutputItemDto> SetPropertyAsync(SetPropertyDto property)
    {
        var result = await this.SetPropertiesAsync([property]);
        return result.First();
    }

    public Task LoginAsync()
    {
        return Task.FromException(new NotSupportedException());
    }

    public Task LogOutAsync()
    {
        return Task.FromException(new NotSupportedException());
    }

    public async Task<(string, string)> RequestLogin()
    {
        fegiFeignUnitOfWork.BeginCookie();

        var clientId = GetClientId();
        var url = "https://account.xiaomi.com/";
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("sdkVersion", "3.9"));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("deviceId", clientId));

        var result = await xiaoMiLoginService.ServiceLogin(clientId);
        var startValue = "&&&START&&&";

        if (result.HasText() && result.StartsWith(startValue))
        {
            result = result.Replace(startValue, "");
            var resultObj = JsonConvert.DeserializeObject<ServiceLoginResultDto>(result);
            if (resultObj == null)
            {
                throw new Exception("登录失败,原因：第一步");
            }

            var millis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var dc2 = millis.ToString();

            var location = resultObj.location;
            var uri = new Uri(location);
            var query = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(query);
            var serviceParam = queryDict["serviceParam"] ?? "";

            var qrCodeParam = new QrCodeLoginInputDto()
            {
                qs = resultObj.qs,
                callback = resultObj.callback,
                serviceParam = serviceParam,
                _sign = resultObj._sign,
                _dc = dc2
            };
            var loginUrlResultString = await xiaoMiLoginService.LoginUrl(qrCodeParam);
            if (loginUrlResultString.HasText() && loginUrlResultString.StartsWith(startValue))
            {
                loginUrlResultString = loginUrlResultString.Replace(startValue, "");
                var loginUrlResult = JsonConvert.DeserializeObject<QrCodeLoginOutPutDto>(loginUrlResultString);
                if (loginUrlResult == null)
                {
                    throw new Exception("登录失败,原因：第2步,获取二维码信息失败");
                }

                if (loginUrlResult.code != 0)
                {
                    throw new Exception("登录失败,原因：第2步,获取二维码信息失败," + loginUrlResultString);
                }

                fegiFeignUnitOfWork.StopCookie();

                return (loginUrlResult.loginUrl, loginUrlResult.lp);
            }
        }

        const string errorMsg = "login fail";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task<LoginState> FinishLogin(string checkUrl)
    {
        fegiFeignUnitOfWork.BeginCookie();

        var clientId = GetClientId();
        var url = "https://account.xiaomi.com/";
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("sdkVersion", "3.9"));
        fegiFeignUnitOfWork.AddCookie(url, new Cookie("deviceId", clientId));

        var startValue = "&&&START&&&";
        string qrCodeLoginResultString;
        try
        {
            qrCodeLoginResultString = await xiaoMiLoginService.QrCodeLogin(checkUrl);
        }
        catch (TimeoutException)
        {
            return LoginState.Timeout;
        }
        catch (TaskCanceledException)
        {
            return LoginState.Timeout;
        }

        if (qrCodeLoginResultString.HasText() && qrCodeLoginResultString.StartsWith(startValue))
        {
            qrCodeLoginResultString = qrCodeLoginResultString.Replace(startValue, "");
            var qrCodeLoginResult =
                JsonConvert.DeserializeObject<QrCodeLogin2OutputDto>(qrCodeLoginResultString);
            if (qrCodeLoginResult == null)
            {
                throw new Exception("登录失败,原因：第4步解析失败");
            }

            if (qrCodeLoginResult.code != 0)
            {
                throw new Exception("登录失败,原因：第4步" + qrCodeLoginResult.desc);
            }

            var result3 = await xiaoMiLoginService.Login(qrCodeLoginResult.location);
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
            fegiFeignUnitOfWork.StopCookie();
            var loginInfo = new LoginInfoDto()
            {
                DeviceId = clientId,
                ServiceToken = serviceToken ?? throw new Exception("serviceToken == null"),
                Ssecurity = qrCodeLoginResult.ssecurity,
                UserId = qrCodeLoginResult.userId,
                PassO = passO,
                PassToken = qrCodeLoginResult.passToken,
                CUserId = qrCodeLoginResult.cUserId,
            };

            await authStateProvider.UpdateLoginInfo(loginInfo);
            return LoginState.Success;
        }

        const string errorMsg = "login fail";
        logger?.LogError(errorMsg);
        throw new Exception(errorMsg);
    }

    public async Task Logout()
    {
        await authStateProvider.Expire();
    }
}