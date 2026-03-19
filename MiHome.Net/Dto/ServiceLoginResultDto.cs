using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class ServiceLoginResultDto
{
    public string? Qs       { get; set; }
    public string? Sid      { get; set; }
    public string? Callback { get; set; }
    [JsonPropertyName("_sign")]
    public string? Sign    { get; set; }

    public int?    Code      { get; set; }
    public string? PassToken { get; set; }
    public string? Ssecurity { get; set; }
    public string? CUserId   { get; set; }

    public string Location { get; set; }
}