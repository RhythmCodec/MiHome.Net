using MiHome.Net.Dto;
using Refit;

namespace MiHome.Net.Apis;

//,
//"Cookie:sdkVersion=3.9;deviceId={{deviceId}}")
[Headers("User-Agent: APP/com.xiaomi.mihome APPV/6.0.103 iosPassportSDK/3.9.0 iOS/14.4 miHSTS")]
public interface IXiaoMiLoginApi
{
    [Get("/pass/serviceLogin?sid=xiaomiio&_json=true")]
    Task<ServiceLoginResultDto> ServiceLogin(string deviceId);

    [Get("/longPolling/loginUrl")]
    Task<QrCodeLoginOutPutDto> LoginUrl([Query] QrCodeLoginInputDto dto);
}