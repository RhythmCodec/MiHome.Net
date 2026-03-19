using System.Text.Json.Serialization;

namespace MiHome.Net.Miio;

public class CallActionPayload
{
    public string Did => $"call-{SiId}-{AiId}";
    /// <summary>
    /// service id 服务id
    /// </summary>
    [JsonPropertyName("siid")]
    public int SiId { get; init; }
    /// <summary>
    /// action id 方法id
    /// </summary>
    [JsonPropertyName("aiid")]
    public int AiId { get; init; }
    /// <summary>
    /// 入参
    /// </summary>
    public List<string> In { get; set; } = [];
}