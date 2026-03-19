namespace MiHome.Net.Dto;

public class GetDeviceInputDto
{
    public bool GetVirtualModel  { get; set; }
    public int  GetHuamiDevices  { get; set; }
    public bool GetSplitDevice   { get; set; }
    public bool SupportSmartHome { get; set; }
}