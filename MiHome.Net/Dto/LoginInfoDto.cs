namespace MiHome.Net.Dto;

public class LoginInfoDto
{
    public string UserId { get; set; }
    public string ServiceToken { get; set; }
    public string DeviceId { get; set; }
    public string Ssecurity { get; set; }
    
    public string PassO { get; set; }

    public string PassToken { get; set; }

    public string CUserId { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }
}