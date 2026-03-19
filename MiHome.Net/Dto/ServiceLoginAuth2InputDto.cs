using Refit;

namespace MiHome.Net.Dto;

public class ServiceLoginAuth2InputDto
{
    //public string _json { get; set; } = "true";
    [AliasAs("user")]
    public required string User { get; set; }
    [AliasAs("hash")]
    public required string Hash { get; set; }
    [AliasAs("callback")]
    public required string Callback { get; set; }
    [AliasAs("sid")]
    public required string Sid { get; set; }
    [AliasAs("qs")]
    public required string Qs { get; set; }
    [AliasAs("_sign")]
    public required string Sign { get; set; }
}