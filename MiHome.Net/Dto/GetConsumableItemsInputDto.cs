namespace MiHome.Net.Dto;

public class GetConsumableItemsInputDto
{
    public long HomeId { get; set; }

    public long OwnerId { get; set; }

    /// <summary>
    /// 设备id列表
    /// </summary>
    //public List<string> dids { get; set; }

    //public string accessKey { get; set; }

    //public bool filter_ignore { get; set; }
}