using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class SetPropertyDto
{
    /// <summary>
    /// 设备id
    /// </summary>
    public required string Did { get; set; }

    [JsonPropertyName("piid")]
    public int PiId { get; set; }
    [JsonPropertyName("siid")]
    public int SiId { get; set; }
    public object? Value { get; set; }
}