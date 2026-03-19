using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class GetAllInstanceResult
{
    public List<GetAllInstanceItem> Instances { get; set; } = [];
}

public class GetAllInstanceItem
{
    public string Model   { get; set; } = "";
    public int Version { get; set; }

    public string Type { get; set; } = "";

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}