using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MiHome.Net.Dto;
using MiHome.Net.Service;
using MiHome.Net.Utils;

namespace MiHome.Net.Middlewares;

internal class CryptoHandler : DelegatingHandler
{
    private readonly IMiAuth _auth;

    public CryptoHandler(IMiAuth auth)
    {
        _auth = auth;
    }

    private static readonly JsonSerializerOptions JsonOptions = Constants.JsonSerializerOption;

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // 若没有传递dto参数，不进行加密
        if (!request.Options.TryGetValue(new HttpRequestOptionsKey<object>("dto"), out var dto))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // 尝试获取登录凭据
        LoginInfoDto loginInfo;
        try
        {
            loginInfo = await _auth.GetLoginInfoAsync();
        }
        catch
        {
#if DEBUG
            throw;
#endif
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        // 加密请求
        var method = request.Method.Method;
        var queryStartIndex = request.RequestUri!.AbsoluteUri.IndexOf('?');
        var requestUri = queryStartIndex != -1
            ? request.RequestUri!.AbsoluteUri[..queryStartIndex]
            : request.RequestUri!.AbsoluteUri;

        var param = GetRc4Params(method, requestUri, dto, loginInfo.Ssecurity, out var signedNonce);

        // 包装为表单内容
        var requestContent = new FormUrlEncodedContent(param);

        // 替换原始请求内容
        request.Content = requestContent;

        // 发送请求
        var response = await base.SendAsync(request, cancellationToken);

        // 请求失败，无法解密，直接返回BadRequest
        if (!response.IsSuccessStatusCode)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // 解密响应
        var respStr = await response.Content.ReadAsStringAsync(cancellationToken);
        var decrypted = DecryptData(signedNonce, respStr);

        // 包装为json内容
        var responseContent = new StringContent(decrypted, MediaTypeHeaderValue.Parse("application/json"));

        // 替换响应内容
        response.Content = responseContent;
        return response;
    }

    /// <summary>
    /// 对参数添加rc4加密
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="ssecurity"></param>
    /// <param name="signedNonce"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetRc4Params(string method, string url, object data, string ssecurity,
        out byte[] signedNonce)
    {
        var dat = new Dictionary<string, string>
        {
            ["data"] = JsonSerializer.Serialize(data, JsonOptions)
        };

        var nonce = CalculateNonce();
        signedNonce = SignedNonce(ssecurity, nonce);
        var signedNonceString = Convert.ToBase64String(signedNonce);
        dat["rc4_hash__"] = Sha1Sign(url, dat, signedNonceString, method);
        foreach (var pair in dat)
        {
            dat[pair.Key] = EncryptData(signedNonce, pair.Value);
        }

        dat["signature"] = Sha1Sign(url, dat, signedNonceString, method);
        dat["_nonce"] = Convert.ToBase64String(nonce);
        dat["ssecurity"] = ssecurity;
        dat["signedNonce"] = signedNonceString;
        return dat;
    }

    /// <summary>
    /// 对数据使用sha1进行加密
    /// </summary>
    /// <param name="url"></param>
    /// <param name="dat"></param>
    /// <param name="nonce"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static string Sha1Sign(string url, Dictionary<string, string> dat, string nonce, string method = "POST")
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        if (path.Length > 5 && path[..5] == "/app/")
        {
            path = path[4..];
        }

        var arr = new List<string>
        {
            method.ToUpper(),
            path
        };
        arr.AddRange(dat.Select(pair => $"{pair.Key}={pair.Value}"));

        arr.Add(nonce);
        var signStr = string.Join("&", arr);
        var sign = SHA1.HashData(Encoding.UTF8.GetBytes(signStr));
        var result = Convert.ToBase64String(sign);
        return result;
    }

    /// <summary>
    /// 加密消息体
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string EncryptData(byte[] key, string data)
    {
        var dataBytes = Encoding.UTF8.GetBytes(data);
        new Rc4(key).Crypt(dataBytes);
        var encoded = Convert.ToBase64String(dataBytes);
        return encoded;
    }

    /// <summary>
    /// 解密消息体
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string DecryptData(byte[] key, string data)
    {
        var decoded = Convert.FromBase64String(data);
        new Rc4(key).Crypt(decoded);
        return Encoding.UTF8.GetString(decoded);
    }

    /// <summary>
    /// 生成校验码
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private static byte[] SignedNonce(string secret, byte[] nonce)
    {
        var secretBytes = Convert.FromBase64String(secret);
        var finalBytes = secretBytes.Concat(nonce).ToArray();
        var finalResult = SHA256.HashData(finalBytes);
        return finalResult;
    }

    /// <summary>
    /// 获取nonce一次性随机数
    /// </summary>
    /// <returns></returns>
    private static byte[] CalculateNonce()
    {
        //Allocate a buffer
        var buf = new byte[12];
        //Generate a cryptographically random set of bytes
        RandomNumberGenerator.Fill(buf.AsSpan()[..8]);

        BinaryPrimitives.WriteInt32BigEndian(buf.AsSpan()[8..], DateTimeOffset.UtcNow.Second);

        return buf;
    }
}