namespace MiHome.Net.Dto;

public class GetPropPostData
{
    public          string?              AccessKey { get; set; }
    public required List<GetPropertyDto> Params    { get; set; }
}