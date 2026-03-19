namespace MiHome.Net.Dto;

public class LoginInfoDto
{
    public required long UserId       { get; set; }
    public required string ServiceToken { get; set; }
    public required string DeviceId     { get; set; }
    public required string Ssecurity    { get; set; }
    
    public required string PassO { get; set; }

    public required string PassToken { get; set; }

    public required string CUserId { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset? ExpireTime { get; set; }
}