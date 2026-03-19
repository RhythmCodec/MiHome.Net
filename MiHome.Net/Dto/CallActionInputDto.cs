using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class CallActionInputDto
{
    /// <summary>
    /// 设备id
    /// </summary>
    public required string Did { get; set; }

    [JsonPropertyName("aiid")]
    public int AiId { get; set; }

    [JsonPropertyName("siid")]
    public int SiId { get; set; }

    public required List<string> In { get; set; }
}