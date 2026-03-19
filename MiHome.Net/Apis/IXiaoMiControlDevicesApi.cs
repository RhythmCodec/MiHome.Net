using MiHome.Net.Dto;
using Refit;

namespace MiHome.Net.Apis;

[Headers(
    "User-Agent: APP/com.xiaomi.mihome APPV/6.0.103 iosPassportSDK/3.9.0 iOS/14.4 miHSTS",
    "X-XIAOMI-PROTOCAL-FLAG-CLI: PROTOCAL-HTTP2",
    "MIOT-ENCRYPT-ALGORITHM: ENCRYPT-RC4")]
public interface IXiaoMiControlDevicesApi
{
    /// <summary>
    /// 获取设备列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/home/device_list")]
    Task<GetDeviceListOutputResult> GetDeviceList([Property("dto")] GetDeviceInputDto dto);

    /// <summary>
    /// 获取家庭列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/v2/homeroom/gethome")]
    Task<GetHomeOutputResultDto> GetHomeList([Property("dto")] GetHomeInputDto dto);

    /// <summary>
    /// 获取场景列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/appgateway/miot/appsceneservice/AppSceneService/GetSceneList")]
    Task<GetSceneOutputResultDto> GetSceneList([Property("dto")] GetSceneInputDto dto);

    /// <summary>
    /// 执行场景
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/appgateway/miot/appsceneservice/AppSceneService/RunScene")]
    Task<RunSceneOutputResultDto> RunScene([Property("dto")] RunSceneInputDto dto);

    /// <summary>
    /// 获取耗材列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/v2/home/standard_consumable_items")]
    Task<GetConsumableItemsOutputResultDto> GetConsumableItems([Property("dto")] GetConsumableItemsInputDto dto);

    /// <summary>
    /// 设置属性
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/miotspec/prop/set")]
    Task<SetPropOutputDto> PropSet([Property("dto")] SetPropPostData dto);

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/miotspec/prop/get")]
    Task<GetPropOutputDto> PropGet([Property("dto")] GetPropPostData dto);

    /// <summary>
    /// 调用方法
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Post("/miotspec/action")]
    Task<CallActionOutputDto> ActionCall([Property("dto")] DoActionPostData dto);

    [Post("/user/get_user_device_data")]
    Task<string> GetUserDeviceData([Property("dto")] Dictionary<string, string> dto);
}