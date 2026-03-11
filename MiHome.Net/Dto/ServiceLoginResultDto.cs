namespace MiHome.Net.Dto;

public class ServiceLoginResultDto
{
    public string? qs       { get; set; }
    public string? sid      { get; set; }
    public string? callback { get; set; }
    public string? _sign    { get; set; }

    public int?    code      { get; set; }
    public string? passToken { get; set; }
    public string? ssecurity { get; set; }
    public string? cUserId   { get; set; }

    public string location { get; set; }
}