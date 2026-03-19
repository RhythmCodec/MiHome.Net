using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class Extra
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isSetPincode")]
    public long IsSetPincode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pincodeType")]
    public long PinCodeType { get; set; }

    public string Platform { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fw_version")]
    public string? FrimewareVersion { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public long NeedVerifyCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long IsPasswordEncrypt { get; set; }
    
    public string? McuVersion { get; set; }
}

public class Owner
{
    public int     Userid   { get; set; }
    public string  Nickname { get; set; }
    public string? Icon     { get; set; }
}

public class XiaoMiDeviceInfo
{
    /// <summary>
    /// 设备id
    /// </summary>
    public string Did { get; set; } = "";

    /// <summary>
    /// 设备token值
    /// </summary>
    public string Token { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Longitude { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Latitude { get; set; } = "";

    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Pid { get; set; } = "";

    /// <summary>
    /// 局域网ip
    /// </summary>
    [JsonPropertyName("localip")]
    public string LocalIp { get; set; } = "";

    /// <summary>
    /// 设备mac地址
    /// </summary>
    public string Mac { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Ssid { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Bssid { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string ParentId { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string ParentModel { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public long ShowMode { get; set; }

    /// <summary>
    /// Model型号
    /// </summary>
    public string Model { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("adminFlag")]
    public long AdminFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shareFlag")]
    public long ShareFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("permitLevel")]
    public long PermitLevel { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 设备在线 
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public required Extra Extra { get; set; }

    /// <summary>
    /// 家庭id
    /// </summary>
    public long Uid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long PdId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("p2p_id")]
    public string P2PId { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public long Rssi { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long FamilyId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long ResetFlag { get; set; }
    
    public string InternetIp { get; set; }
    public Owner  Owner       { get; set; }
}

public class VirtualModelsItem
{
    /// <summary>
    /// 
    /// </summary>
    public string Model { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public long State { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Url { get; set; } = "";
}

public class GetDeviceListOutputResultItem
{
    /// <summary>
    /// 
    /// </summary>
    public List<XiaoMiDeviceInfo> List { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    public List<VirtualModelsItem> VirtualModels { get; set; } = [];
}

public class GetDeviceListOutputResult
{
    /// <summary>
    /// 
    /// </summary>
    public long Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public GetDeviceListOutputResultItem? Result { get; set; }
}