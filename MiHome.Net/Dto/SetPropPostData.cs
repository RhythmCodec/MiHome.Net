namespace MiHome.Net.Dto;

public class SetPropPostData
{
    public string? AccessKey { get; set; }
    public required List<SetPropertyDto> Params { get; set; }
}