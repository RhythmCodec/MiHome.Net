using MiHome.Net.Dto;
using Refit;

namespace MiHome.Net.Apis;

public interface IMiotCloudApi
{
    [Get("/instances?status=released")]
    Task<GetAllInstanceResult> GetAllInstancesAsync();

    /// <summary>
    /// 通过设备类型获取设备规格，包括服务，属性，方法，事件等
    /// </summary>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    [Get("/instance")]
    Task<MiotSpec> GetSpecByDeviceType([AliasAs("type")] string deviceType);
}