namespace MiHome.Net.Dto;

public class CheckIdentityLisResult
{
    public int    Code                    { get; set; }
    public int    Flag                    { get; set; }
    public int    Option                  { get; set; }
    public int[]  Options                 { get; set; } = [];
    public string Version                 { get; set; } = "";
    public bool   ShowFastUpdateEmailLink { get; set; }
    public string ExternalId              { get; set; } = "";
    public string RetrieveType            { get; set; } = "";
    public bool   TrustCheckBox           { get; set; }
    public bool   TrustCheckBoxSelected   { get; set; }
    public bool   DirectVerify            { get; set; }
}