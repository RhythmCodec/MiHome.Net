using System.Net;

namespace MiHome.Net.Service;

internal interface ICookieContainer
{
    public CookieContainer CookieContainer { get; set; }
}

internal class MiAuthCookie : ICookieContainer
{
    /// <inheritdoc />
    public CookieContainer CookieContainer { get; set; } = new();
}

internal class MiDeviceControlCookie : ICookieContainer
{
    /// <inheritdoc />
    public CookieContainer CookieContainer { get; set; } = new();
}