using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class GetConsumableItemsOutputResultDto
{
    public int                                    Code    { get; set; }
    public string                                 Message { get; set; } = "";
    public GetConsumableItemsOutputResultItemDto? Result  { get; set; }
}

public class GetConsumableItemsOutputResultItemDto
{
    public List<GetConsumableItemsOutputDto> Items { get; set; } = [];
}

public class GetConsumableItemsOutputDto
{
    /// <summary>
    /// 耗材状态，1：正常，3：已耗尽
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 该状态下耗材的数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 忽略数量
    /// </summary>
    public int IgnoreCount { get; set; }

    /// <summary>
    /// 消耗数据列表
    /// </summary>
    public List<ConsumesData> ConsumesData { get; set; } = [];
}

public class ConsumesData
{
    /// <summary>
    /// 消耗数据详情列表
    /// </summary>
    public List<ConsumesDataDetail> Details { get; set; } = [];

    /// <summary>
    /// 是否忽略
    /// </summary>
    public bool IsIgnore { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 设备id
    /// </summary>
    public string Did { get; set; } = "";

    /// <summary>
    /// 设备型号
    /// </summary>
    public string Model { get; set; } = "";

    /// <summary>
    /// 子类
    /// </summary>
    public string SubClass { get; set; } = "";

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("time_stamp")]
    public int Timestamp { get; set; }

    /// <summary>
    /// 房间id
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 房间创建时间
    /// </summary>
    public int RoomCreateTime { get; set; }

    /// <summary>
    /// 是否跳过rpc
    /// </summary>
    public bool SkipRpc { get; set; }

    /// <summary>
    /// 是否为蓝牙网关
    /// </summary>
    public bool BleGateway { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public int StartTime { get; set; }
}

/// <summary>
/// 消耗数据详情
/// </summary>
public class ConsumesDataDetail
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// 更新时间
    /// </summary>
    public int UpdateTime { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int State { get; set; }

    public string Inadeq  { get; set; } = "";
    public string Exhaust { get; set; } = "";

    /// <summary>
    /// 额外url
    /// </summary>
    public string ExtraUrl { get; set; } = "";

    /// <summary>
    /// 寿命时间
    /// </summary>
    public string LeftTime { get; set; } = "";

    /// <summary>
    /// 总寿命
    /// </summary>
    public string TotalLife { get; set; } = "";

    /// <summary>
    /// 介绍
    /// </summary>
    public string Intro { get; set; } = "";

    /// <summary>
    /// 属性值
    /// </summary>
    public string Prop { get; set; } = "";

    /// <summary>
    /// 消耗类型
    /// </summary>
    public string ConsumableType { get; set; } = "";

    /// <summary>
    /// 图片地址列表
    /// </summary>

    public List<string> PicUrls { get; set; } = [];

    /// <summary>
    /// 重置方法
    /// </summary>

    public string ResetMethod { get; set; } = "";

    /// <summary>
    /// 变更介绍
    /// </summary>

    public List<object> ChangeInstruction { get; set; } = [];

    /// <summary>
    /// 重置状态
    /// </summary>

    public int ResetState { get; set; }

    /// <summary>
    /// 消耗类型名称
    /// </summary>
    public string TypeName { get; set; } = "";

    public string? WechatPath { get; set; }
    public int     LinkType   { get; set; }
}