using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class GetPropOutputDto
{
    /// <summary>
    /// 
    /// </summary>
    public long Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = "";

    public List<GetPropOutputItemDto> Result { get; set; } = [];
}

public class GetPropOutputItemDto
{
    /// <summary>
    /// 设备id
    /// </summary>
    public string Did { get; set; } = "";

    public string Iid { get; set; }

    /// <summary>
    /// 二级控制大类id
    /// </summary>
    [JsonPropertyName("Piid")]
    public int PiId { get; set; }

    /// <summary>
    /// 一级控制大类id
    /// </summary>
    [JsonPropertyName("Siid")]
    public int SiId { get; set; }

    /// <summary>
    /// 具体的值
    /// </summary>
    public object? Value { get; set; }

    public int Code { get; set; }

    [JsonPropertyName("updateTime")]
    public long UpdateTime { get; set; }
    public long ExeTime { get;    set; }
}