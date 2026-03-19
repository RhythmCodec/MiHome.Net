using System.Text.Json;
using MiHome.Net.Dto;
using MiHome.Net.Service;

namespace Demo;

public class FileAuthStateProvider : IMiAuthStateProvider
{
    private LoginInfoDto? _state;

    /// <inheritdoc />
    public async Task<LoginInfoDto?> GetLoginInfo()
    {
        if (_state == null && File.Exists("auth.json"))
        {
            await using var fs = File.OpenRead("auth.json");
            _state = await JsonSerializer.DeserializeAsync<LoginInfoDto>(fs);
        }

        return _state;
    }
    /// <inheritdoc />
    public async Task UpdateLoginInfo(LoginInfoDto loginInfo)
    {
        await using var fs = File.OpenWrite("auth.json");
        await JsonSerializer.SerializeAsync(fs, loginInfo);
    }
    /// <inheritdoc />
    public Task Expire()
    {
        _state = null;
        if (File.Exists("auth.json"))
            File.Delete("auth.json");
        return Task.CompletedTask;
    }
}