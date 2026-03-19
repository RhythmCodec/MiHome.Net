using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MiHome.Net.Miio;

/// <summary>
/// 命令
/// </summary>
public class Command
{
    private static readonly JsonSerializerOptions SerializerOptions = Constants.JsonSerializerOption;

    public static Command Parse(ReadOnlySpan<byte> bytes, byte[] token)
    {
        var command = new Command();

        var magicNumber = BinaryPrimitives.ReadInt16BigEndian(bytes);
        command.MagicNumber = magicNumber;
        var length = BinaryPrimitives.ReadInt16BigEndian(bytes[2..]);
        command.Length = length;
        var unknown = BinaryPrimitives.ReadInt32BigEndian(bytes[4..]);
        command.Unknown = unknown;
        var deviceId = BinaryPrimitives.ReadInt32BigEndian(bytes[8..]);
        command.DeviceId = deviceId;
        var ts = DateTimeOffset.FromUnixTimeSeconds(BinaryPrimitives.ReadInt32BigEndian(bytes[12..]));
        command.Ts = ts;

        bytes[..2].CopyTo(command.MagicNumberBytes);
        bytes[2..4].CopyTo(command.LengthBytes);
        bytes[4..8].CopyTo(command.UnknownBytes);
        bytes[8..12].CopyTo(command.DeviceIdBytes);
        bytes[12..16].CopyTo(command.TsBytes);
        bytes[16..32].CopyTo(command.CheckSum);

        Debug.Assert(token.Length == command.TokenBytes.Length);
        token.CopyTo(command.TokenBytes);
        if (length > 0)
        {
            var dataBytes = bytes[32..length].ToArray();
            var decryptDataBytes = command.Decrypt(dataBytes);
            if (decryptDataBytes.Length > 0 && decryptDataBytes[^1] == 0)
                command.DataBytes = decryptDataBytes.AsSpan()[..^1].ToArray();
        }
        return command;
    }

    public byte[] Build()
    {
        var buffer = new byte[Length];
        var pBuf = buffer.AsSpan();
        MagicNumberBytes.CopyTo(pBuf);
        LengthBytes.CopyTo(pBuf[2..]);
        UnknownBytes.CopyTo(pBuf[4..]);
        DeviceIdBytes.CopyTo(pBuf[8..]);
        TsBytes.CopyTo(pBuf[12..]);
        TokenBytes.CopyTo(pBuf[16..]);
        DataBytes.CopyTo(pBuf[32..]);
        var checkSum = MD5.HashData(pBuf);
        checkSum.CopyTo(pBuf[16..]);

        return buffer;
    }

    private byte[] Encrypt(byte[] body)
    {
        var key = MD5.HashData(TokenBytes);
        var iv = key.Concat(TokenBytes).ToArray();
        iv = MD5.HashData(iv);
        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var crypt = aes.CreateEncryptor();
        var cipherText = crypt.TransformFinalBlock(body, 0, body.Length);
        return cipherText;
    }

    private byte[] Decrypt(byte[] body)
    {
        var key = MD5.HashData(TokenBytes);
        var iv = key.Concat(TokenBytes).ToArray();
        iv = MD5.HashData(iv);
        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;
        using var crypt = aes.CreateDecryptor();

        var cipherText = crypt.TransformFinalBlock(body, 0, body.Length);
        return cipherText;
    }

    public int MagicNumber { get; set; }
    /// <summary>
    /// 魔术数
    /// </summary>
    public byte[] MagicNumberBytes { get; set; } = [33, 49];
    /// <summary>
    /// 数据长度
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// 数据长度
    /// </summary>
    public byte[] LengthBytes { get; set; } = [0, 0];

    public int Unknown { get; set; }
    /// <summary>
    /// 分割
    /// </summary>
    public byte[] UnknownBytes { get; set; } = [0, 0, 0, 0];
    /// <summary>
    /// 设备id
    /// </summary>
    public int DeviceId { get; set; }
    /// <summary>
    /// 设备id
    /// </summary>
    public byte[] DeviceIdBytes { get; set; } = [0, 0, 0, 0];
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTimeOffset Ts { get; set; }

    public byte[] TsBytes { get; set; } = [0, 0, 0, 0];

    /// <summary>
    /// token值
    /// </summary>
    public byte[] TokenBytes { get; set; } = new byte[16];
    /// <summary>
    /// 数据
    /// </summary>
    public byte[]? DataBytes { get; private set; }

    public void SetData(CommandPayload commandPayload)
    {
        var methodCallDtos = JsonSerializer.Serialize(commandPayload, SerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(methodCallDtos).ToList();
        bytes.Add(0);
        var result = Encrypt(bytes.ToArray());
        DataBytes = result;
        Length = result.Length + 32;
        BinaryPrimitives.WriteInt16BigEndian(LengthBytes, (short)Length);
    }
    /// <summary>
    /// 检查和，用来进行校验
    /// </summary>
    public byte[] CheckSum { get; set; } = new byte[16];
}