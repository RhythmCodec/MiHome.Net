using System.Text.Json.Serialization;

namespace MiHome.Net.Miio;

public class GetDeviceInfoResult
{
    public int                     Id      { get; set; }
    public GetDeviceInfoResultItem Result  { get; set; } = new();
    public int                     ExeTime { get; set; }
}

public class GetDeviceInfoResultItem
{
    public int    Life  { get; set; }
    public long   Uid   { get; set; }
    public string Model { get; set; } = "";
    public string Token { get; set; } = "";
    [JsonPropertyName("ipflag")]
    public int IpFlag { get; set; }

    [JsonPropertyName("fw_ver")]
    public string FrimewareVersion { get; set; } = "";
    public string MiioVer { get;          set; } = "";
    [JsonPropertyName("hw_ver")]
    public string HardwareVersion { get; set; } = "";
    public int    Mmfree { get;          set; }
    public string Mac    { get;          set; } = "";

    [JsonPropertyName("wifi_fw_ver")]
    public string WifiFrimewareVersion { get; set; } = "";
    public Ap    Ap    { get;                 set; } = new();
    public Netif Netif { get;                 set; } = new();
}

public class Ap
{
    public string Ssid    { get; set; } = "";
    public string Bssid   { get; set; } = "";
    public int    Rssi    { get; set; }
    public int    Primary { get; set; }
}

public class Netif
{
    public string LocalIp { get; set; } = "";
    public string Mask    { get; set; } = "";
    public string Gw      { get; set; } = "";
}