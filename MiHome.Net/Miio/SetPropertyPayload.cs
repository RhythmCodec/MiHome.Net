using System.Text.Json.Serialization;

namespace MiHome.Net.Miio;

public class SetPropertyPayload
{
    //public string Did { get; set; }
    public string Did => $"set-{SiId}-{PiId}";
    [JsonPropertyName("siid")]
    public int SiId { get; set; }
    [JsonPropertyName("piid")]
    public int PiId { get; set; }

    public object? Value { get; set; }
}