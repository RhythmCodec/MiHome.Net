namespace MiHome.Net.Dto;

public class DoActionPostData
{
    public required string             AccessKey { get; set; }
    public required CallActionInputDto Params    { get; set; }
}