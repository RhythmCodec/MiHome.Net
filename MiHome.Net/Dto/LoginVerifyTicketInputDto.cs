using Refit;

namespace MiHome.Net.Dto;

public class LoginVerifyTicketInputDto
{
    [AliasAs("_flag")]
    public int Flag { get; set; }

    [AliasAs("ticket")]
    public required string Ticket { get; set; }

    [AliasAs("trust")]
    public bool Trust { get; set; }

    [AliasAs("_json")]
    public bool Json { get; set; }
}