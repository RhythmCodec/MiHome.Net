using System.Text.Json.Serialization;

namespace MiHome.Net.Miio;

public class SetPropertiesResult
{
    public int                           Id      { get; set; }
    public List<SetPropertiesResultItem> Result  { get; set; } = [];
    public int                           ExeTime { get; set; }
}

public class SetPropertiesResultItem
{
    public string Did { get; set; } = "";
    [JsonPropertyName("siid")]
    public int SiId { get; set; }
    [JsonPropertyName("piid")]
    public int PiId { get; set; }
    public int Code { get; set; }
}