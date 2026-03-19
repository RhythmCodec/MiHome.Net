using System.Text.Json;

namespace MiHome.Net;

internal static class Constants
{
    public const string MI_CLOUD_API_NAME      = "MiotCloud";
    public const string MI_CONTROL_DEVICE_NAME = "MiControl";
    public const string MI_LOGIN_NAME          = "MiLogin";

    public const string MI_CLOUD_API_URL      = "https://miot-spec.org/miot-spec-v2";
    public const string MI_CONTROL_DEVICE_URL = "https://api.io.mi.com/app";
    public const string MI_LOGIN_URL          = "https://account.xiaomi.com";

    public static readonly JsonSerializerOptions JsonSerializerOption = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}