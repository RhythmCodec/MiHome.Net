namespace MiHome.Net.Dto;

public class QrCodeLoginOutPutDto
{
    public string LoginUrl     { get; set; } = "";
    public string Qr           { get; set; } = "";
    public string QrTips       { get; set; } = "";
    public string Lp           { get; set; } = "";
    public string Sl           { get; set; } = "";
    public int    Timeout      { get; set; }
    public int    TimeInterval { get; set; }
    public int    Code         { get; set; }
    public string Result       { get; set; } = "";
    public string Desc         { get; set; } = "";
    public string Description  { get; set; } = "";
}
