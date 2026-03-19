namespace MiHome.Net.Dto;

public class QrCodeLogin2OutputDto
{
    public string  Psecurity       { get; set; } = "";
    public long    Nonce           { get; set; }
    public string  Ssecurity       { get; set; } = "";
    public string  PassToken       { get; set; } = "";
    public long     UserId          { get; set; }
    public string  CUserId         { get; set; } = "";
    public int     SecurityStatus  { get; set; }
    public string  NotificationUrl { get; set; } = "";
    public int     Pwd             { get; set; }
    public int     Child           { get; set; }
    public int     Code            { get; set; }
    public string  Result          { get; set; } = "";
    public string  Desc            { get; set; } = "";
    public string  Description     { get; set; } = "";
    public string  Location        { get; set; } = "";
    public object? CaptchaUrl      { get; set; }
}