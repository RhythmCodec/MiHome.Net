using MiHome.Net.Dto;
using MiHome.Net.Service;

namespace Demo;

public class NoOpMiAuthStateProvider : IMiAuthStateProvider
{
    /// <inheritdoc />
    public Task<LoginInfoDto?> GetLoginInfo()
    {
        return Task.FromResult<LoginInfoDto?>(null);
    }

    /// <inheritdoc />
    public Task UpdateLoginInfo(LoginInfoDto loginInfo)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Expire()
    {
        return Task.CompletedTask;
    }
}