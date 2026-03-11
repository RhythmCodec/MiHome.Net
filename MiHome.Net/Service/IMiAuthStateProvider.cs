using MiHome.Net.Dto;

namespace MiHome.Net.Service;

/// <summary>
/// 从缓存或者数据库等来源加载MiHome登录信息
/// </summary>
public interface IMiAuthStateProvider
{
    /// <summary>
    /// 获取登录信息。
    /// </summary>
    /// <returns>成功获取时返回<see cref="LoginInfoDto"/>, 否则返回null</returns>
    Task<LoginInfoDto?> GetLoginInfo();

    /// <summary>
    /// 更新登录信息。
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <returns></returns>
    Task UpdateLoginInfo(LoginInfoDto loginInfo);

    /// <summary>
    /// 使登录信息缓存过期。
    /// </summary>
    /// <returns></returns>
    Task Expire();

    /// <summary>
    /// 检查状态是否过期，默认有效时间小于5分钟为过期。
    /// </summary>
    /// <returns></returns>
    async Task<bool> StateValid()
    {
        var state = await GetLoginInfo();
        if (state == null) return false;

        var timeleft = (state.ExpireTime ?? DateTimeOffset.UtcNow) - DateTimeOffset.UtcNow;
        return timeleft.Minutes >= 5;
    }
}