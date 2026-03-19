namespace MiHome.Net.Dto;

public class ServiceLoginAuth2OutputDto
{
    public string? Ssecurity { get; set; } = "";
    public string? UserId    { get; set; } = "";

    public string? CUserId   { get; set; } = "";
    public string? PassToken { get; set; } = "";

    public string Location { get; set; } = "";

    public string Code  { get; set; } = "";
    public string Nonce { get; set; } = "";

    /// <summary>
    /// 通知url，如果不为空，则需要校验手机
    /// </summary>
    public string? NotificationUrl { get; set; } = "";
}