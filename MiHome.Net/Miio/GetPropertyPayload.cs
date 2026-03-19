using System.Text.Json.Serialization;

namespace MiHome.Net.Miio;

public class GetPropertyPayload
{
    //public string Did { get; set; }
    public string Did => $"{SiId}-{PiId}";
    [JsonPropertyName("siid")]
    public int SiId { get; set; }
    [JsonPropertyName("piid")]
    public int PiId { get; set; }
}