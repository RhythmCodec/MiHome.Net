namespace MiHome.Net.Dto;

/// <summary>
/// 获取家庭列表dto
/// </summary>
public class GetHomeInputDto
{
    public bool Fg            { get; set; }
    public bool FetchShare    { get; set; }
    public bool FetchShareDev { get; set; }
    public int  Limit         { get; set; }
    public int  AppVer        { get; set; }
}