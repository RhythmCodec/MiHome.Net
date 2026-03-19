using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace MiHome.Net.Miio;

public class MiioProtocol
{
    private static readonly JsonSerializerOptions SerializerOptions = Constants.JsonSerializerOption;

    /// <summary>
    /// token值的16进制
    /// </summary>
    private readonly byte[] _tokenBytes;

    private readonly IPAddress _ipAddress;
    private          Command?  _requestCommand;
    private          int       _id;

    /// <summary>
    /// 是否有发现
    /// </summary>
    [MemberNotNull(nameof(_requestCommand))]
    private bool IsDiscovered { get; set; }

    public MiioProtocol(string ip, string token)
    {
        _ipAddress = IPAddress.Parse(ip);
        _tokenBytes = Convert.FromHexString(token);
        _id = 0;
    }

    /// <summary>
    /// 批量获取属性
    /// </summary>
    /// <param name="propertiesPayloads"></param>
    /// <returns></returns>
    public async Task<GetPropertiesResult?> GetPropertiesAsync(List<GetPropertyPayload> propertiesPayloads)
    {
        await SendAsync("get_properties", propertiesPayloads);
        var result = JsonSerializer.Deserialize<GetPropertiesResult>(_requestCommand.DataBytes, SerializerOptions);
        return result;
    }

    /// <summary>
    /// 批量设置属性
    /// </summary>
    /// <param name="propertiesPayloads"></param>
    /// <returns></returns>
    public async Task<SetPropertiesResult?> SetPropertiesAsync(List<SetPropertyPayload> propertiesPayloads)
    {
        await SendAsync("set_properties", propertiesPayloads);
        var result = JsonSerializer.Deserialize<SetPropertiesResult>(_requestCommand.DataBytes, SerializerOptions);
        return result;
    }

    /// <summary>
    /// 调用方法
    /// </summary>
    /// <param name="callActionPayload"></param>
    /// <returns></returns>
    public async Task<CallActionResult?> CallActionAsync(CallActionPayload callActionPayload)
    {
        await SendAsync("action", callActionPayload);
        var result = JsonSerializer.Deserialize<CallActionResult>(_requestCommand.DataBytes, SerializerOptions);
        return result;
    }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    public async Task<GetDeviceInfoResult?> GetDeviceInfoAsync()
    {
        await SendAsync("miIO.info");
        var result = JsonSerializer.Deserialize<GetDeviceInfoResult>(_requestCommand.DataBytes, SerializerOptions);
        return result;
    }

    private async Task SendAsync(string command, object? parameter = null)
    {
        if (!IsDiscovered)
        {
            await SendHandshake();
            IsDiscovered = true;
        }
        var body = CreateBody(command, parameter);
        var sendCommand = new Command()
        {
            DeviceIdBytes = _requestCommand.DeviceIdBytes,
            UnknownBytes = _requestCommand.UnknownBytes,
            Ts = _requestCommand.Ts.AddSeconds(1),
            TokenBytes = _tokenBytes
        };
        sendCommand.SetData(body);

        var sendBytes = sendCommand.Build();
        using var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        serverSocket.ReceiveTimeout = 5000;
        var endPoint = new IPEndPoint(_ipAddress, 54321);
        serverSocket.SendTo(sendBytes, endPoint);

        var buffer = new byte[4096];
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var len = await serverSocket.ReceiveAsync(buffer, cts.Token);
            if (len > 0)
            {
                _requestCommand = Command.Parse(buffer, _tokenBytes);
            }
        }
        catch (OperationCanceledException)
        { }
    }

    private CommandPayload CreateBody(string command, object? parameter = null)
    {
        _id++;
        if (_id >= 9999)
        {
            _id = 1;
        }
        var methodCallDto = new CommandPayload()
        {
            Id = _id,
            Method = command,
            Params = parameter
        };

        return methodCallDto;
    }

    /// <summary>
    /// 发送握手包
    /// </summary>
    /// <returns></returns>
    private async Task SendHandshake()
    {
        using var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        var endPoint = new IPEndPoint(_ipAddress, 54321);
        ReadOnlySpan<byte> helloBytes =
        [
            0x21, 0x31, 0x00, 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        ];
        serverSocket.SendTo(helloBytes, endPoint);

        var buffer = new byte[1024];
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var len = await serverSocket.ReceiveAsync(buffer, cts.Token);
            if (len > 0)
            {
                _requestCommand = Command.Parse(buffer, _tokenBytes);
            }
        }
        catch (OperationCanceledException)
        { }
    }
}