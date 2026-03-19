namespace MiHome.Net.Dto;

public class GetUserDeviceDataInputDto
{
    public required string Did       { get; set; }
    public required string Key       { get; set; }
    public required string Type      { get; set; }
    public          long   TimeStart { get; set; }

    public long TimeEnd { get; set; }
    public int  Limit   { get; set; }

    public required string Uid { get; set; }
}