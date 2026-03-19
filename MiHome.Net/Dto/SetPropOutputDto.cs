using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

/// <summary>
/// 设置属性返回值的dto
/// </summary>
public class SetPropOutputDto
{
    /// <summary>
    /// 返回码，为0则正常
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = "";

    public List<SetPropOutputItemDto> Result { get; set; }
}

public class SetPropOutputItemDto
{
    /// <summary>
    /// 设备id
    /// </summary>
    public string Did { get; set; } = "";

    /// <summary>
    /// 二级控制大类id
    /// </summary>
    [JsonPropertyName("piid")]
    public int PiId { get; set; }

    /// <summary>
    /// 一级控制大类id
    /// </summary>
    [JsonPropertyName("siid")]
    public int SiId { get; set; }

    public string Code    { get; set; } = "";
    public long   ExeTime { get; set; }
}