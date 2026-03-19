using Refit;

namespace MiHome.Net.Dto;

/// <summary>
/// 二维码登录参数
/// </summary>
public class QrCodeLoginInputDto
{
    [AliasAs("_qrsize")]
    public string? QrSize { get; set; }

    [AliasAs("qs")]
    public required string Qs { get; set; }

    [AliasAs("bizDeviceType")]

    public string BusinessDeviceType { get; set; } = "";

    [AliasAs("callback")]
    public required string Callback { get; set; }

    [AliasAs("_json")]

    public bool Json { get; set; } = true;

    [AliasAs("theme")]
    public string Theme { get; set; } = "";

    [AliasAs("sid")]
    public string Sid { get; set; } = "xiaomiio";

    [AliasAs("needTheme")]
    public bool NeedTheme { get; set; } = false;

    [AliasAs("showActiveX")]
    public bool ShowActiveX { get; set; } = false;

    [AliasAs("_local")]
    public string Local { get; set; } = "zh_CN";

    [AliasAs("_sign")]
    public required string Sign { get; set; }

    [AliasAs("_dc")]
    public required string Dc { get; set; }

    [AliasAs("serviceParam")]
    public required string ServiceParam { get; set; }
}