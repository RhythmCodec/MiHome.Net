using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MiHome.Net.Apis;
using MiHome.Net.Dto;

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
    Task CallActionAsync(CallActionInputDto callActionParam);
}

internal class MIotCloud : IMiotCloud
{
    private readonly IMiotCloudApi            _miotCloudApi;
    private readonly IXiaoMiControlDevicesApi _xiaoMiControlDevicesApi;
    private readonly ILogger<MiHomeDriver>    _logger;
    private readonly IMemoryCache             _memoryCache;
    private readonly IMiAuth                  _miAuth;

    public MIotCloud(IMiotCloudApi miotCloudApi, IXiaoMiControlDevicesApi xiaoMiControlDevicesApi,
        ILogger<MiHomeDriver> logger, IMemoryCache memoryCache, IMiAuth miAuth)
    {
        _miotCloudApi = miotCloudApi;
        _xiaoMiControlDevicesApi = xiaoMiControlDevicesApi;
        _logger = logger;
        _memoryCache = memoryCache;
        _miAuth = miAuth;
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

    private Task<GetAllInstanceResult> GetAllInstancesAsync()
    {
        return GetOrCreateCachedAsync("MiHomeNet:AllInstances", "allInstance.json",
            () => _miotCloudApi.GetAllInstancesAsync());
    }

    private Task<MiotSpec> GetSpecByDeviceType(string deviceType, string model)
    {
        return GetOrCreateCachedAsync($"MiHomeNet:DeviceSpecs:{model}", $"{model}.json",
            () => _miotCloudApi.GetSpecByDeviceType(deviceType));
    }

    private async Task<T> GetOrCreateCachedAsync<T>(
        string memoryCacheKey,
        string cacheFileName,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        expiration ??= TimeSpan.FromHours(24);

        // 1. 优先检查内存缓存
        if (_memoryCache.TryGetValue(memoryCacheKey, out T? cachedResult))
        {
            return cachedResult!;
        }

        var cacheFilePath = Path.Combine(Path.GetTempPath(), "MiHomeNet", cacheFileName);
        var fi = new FileInfo(cacheFilePath);

        // 2. 检查文件缓存
        if (fi.Exists && (DateTime.Now - fi.CreationTime) < expiration)
        {
            await using var fs = fi.OpenRead();
            var result = await JsonSerializer.DeserializeAsync<T>(fs);

            _memoryCache.Set(memoryCacheKey, result!, expiration.Value);
            return result!;
        }
        else
        {
            if (fi.Exists)
            {
                fi.Delete();
                await Task.Delay(50);
            }

            // 3. 调用服务获取数据
            var result = await factory();

            Directory.CreateDirectory(fi.DirectoryName!);
            await using var fs = fi.OpenWrite();
            await JsonSerializer.SerializeAsync(fs, result);

            _memoryCache.Set(memoryCacheKey, result, expiration.Value);

            return result;
        }
    }

    public async Task<List<XiaoMiDeviceInfo>> GetDeviceListAsync()
    {
        var inputDto = new GetDeviceInputDto
        {
            GetVirtualModel = true,
            GetHuamiDevices = 1,
            GetSplitDevice = false,
            SupportSmartHome = true
        };
        var result = await _xiaoMiControlDevicesApi.GetDeviceList(inputDto);
        if (result.Code == 0)
        {
            return result.Result!.List;
        }

        _logger.LogError("get device List error,reason:{message}", result.Message);
        throw new Exception($"get device List error,reason:{result.Message}");
    }

    public async Task<List<GetConsumableItemsOutputDto>> GetConsumableItemsAsync(string homeId)
    {
        if (!decimal.TryParse(homeId, out _))
        {
            throw new NotSupportedException("homeId必须为数字");
        }

        var loginInfo = await _miAuth.GetLoginInfoAsync();

        var inputDto = new GetConsumableItemsInputDto
        {
            HomeId = long.Parse(homeId),
            OwnerId = loginInfo.UserId,
        };
        var result = await _xiaoMiControlDevicesApi.GetConsumableItems(inputDto);
        if (result.Code == 0)
        {
            return result.Result?.Items ?? [];
        }

        _logger.LogError("Get ConsumableItems error,reason:{message}", result.Message);
        throw new Exception($"Get ConsumableItems error,reason:{result.Message}");
    }

    public async Task<List<HomeDto>> GetHomeListAsync()
    {
        //data = {"fg": False, "fetch_share": True, "fetch_share_dev": True, "limit": 300, "app_ver": 7}
        var inputDto = new GetHomeInputDto
        {
            Fg = false,
            FetchShare = true,
            FetchShareDev = true,
            Limit = 300,
            AppVer = 7
        };
        var result = await _xiaoMiControlDevicesApi.GetHomeList(inputDto);
        if (result.Code == 0)
        {
            return result.Result!.HomeList;
        }

        _logger.LogError("get Home List error,reason:{message}", result.Message);
        throw new Exception($"get Home List error,reason:{result.Message}");
    }

    public async Task<List<SceneDto>> GetSceneListAsync(string homeId)
    {
        var inputDto = new GetSceneInputDto
        {
            HomeId = homeId
        };
        var result = await _xiaoMiControlDevicesApi.GetSceneList(inputDto);
        if (result.Code == 0)
        {
            return result.Result?.SceneInfoList ?? [];
        }

        _logger.LogError("get Scene List error,reason:{message}", result.Message);
        throw new Exception($"get Scene List error,reason:{result.Message}");
    }

    public async Task<bool> RunSceneAsync(string sceneId)
    {
        var inputDto = new RunSceneInputDto
        {
            SceneId = sceneId,
            TriggerKey = "user.click"
        };
        var result = await _xiaoMiControlDevicesApi.RunScene(inputDto);
        if (result.Code == 0)
        {
            return true;
        }

        _logger.LogError("get Scene List error,reason:{message}", result.Message);
        throw new Exception($"get Scene List error,reason:{result.Message}");
    }

    public async Task<List<GetPropOutputItemDto>> GetPropertiesAsync(List<GetPropertyDto> properties)
    {
        var postData = new GetPropPostData
        {
            Params = properties
        };
        var result = await _xiaoMiControlDevicesApi.PropGet(postData);
        if (result.Code == 0)
        {
            return result.Result;
        }

        _logger.LogError("propGet error,reason:{message}", result.Message);
        throw new Exception($"propGet error,reason:{result.Message}");
    }

    public async Task<GetPropOutputItemDto> GetPropertyAsync(GetPropertyDto property)
    {
        var result = await this.GetPropertiesAsync([property]);
        return result.First();
    }

    public async Task CallActionAsync(CallActionInputDto callActionParam)
    {
        var postData = new DoActionPostData
        {
            AccessKey = "",
            Params = callActionParam
        };
        var result = await _xiaoMiControlDevicesApi.ActionCall(postData);
        if (result.Code == 0)
        {
            return;
        }

        _logger.LogError("propGet error,reason:{message}", result.Message);
        throw new Exception($"propGet error,reason:{result.Message}");
    }

    public async Task<List<SetPropOutputItemDto>> SetPropertiesAsync(List<SetPropertyDto> properties)
    {
        var postData = new SetPropPostData
        {
            Params = properties
        };

        var result = await _xiaoMiControlDevicesApi.PropSet(postData);
        if (result.Code == 0)
        {
            return result.Result;
        }

        _logger.LogError("propGet error,reason:{message}", result.Message);
        throw new Exception($"propGet error,reason:{result.Message}");
    }

    public async Task<SetPropOutputItemDto> SetPropertyAsync(SetPropertyDto property)
    {
        var result = await this.SetPropertiesAsync([property]);
        return result.First();
    }
}