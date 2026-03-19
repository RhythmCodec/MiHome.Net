namespace MiHome.Net.Middlewares;

internal class RemoveResponsePrefixHandler : DelegatingHandler
{
    private static readonly byte[] PrefixBytes = "&&&START&&&"u8.ToArray();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var originalStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // 包装一个新流，跳过前缀
        var filteredStream = new PrefixStrippedStream(originalStream, PrefixBytes);

        var newContent = new StreamContent(filteredStream);
        foreach (var header in response.Content.Headers)
        {
            newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        response.Content = newContent;
        return response;
    }
}

/// <summary>
/// 一个包装流，在第一次读取时跳过指定前缀
/// </summary>
file class PrefixStrippedStream : Stream
{
    private readonly Stream        _inner;
    private readonly byte[]        _prefix;
    private          bool          _prefixChecked;
    private          MemoryStream? _bufferedPrefix;

    public PrefixStrippedStream(Stream inner, byte[] prefix)
    {
        _inner = inner;
        _prefix = prefix;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!_prefixChecked)
        {
            // 先尝试读取前缀长度的字节
            var temp = new byte[_prefix.Length];
            var read = _inner.Read(temp, 0, temp.Length);

            if (read == _prefix.Length && temp.SequenceEqual(_prefix))
            {
                // 前缀完全匹配，跳过
            }
            else
            {
                // 前缀不匹配，把已读数据缓存起来
                _bufferedPrefix = new MemoryStream(read);
                _bufferedPrefix.Write(temp, 0, read);
                _bufferedPrefix.Position = 0;
            }

            _prefixChecked = true;
        }

        if (_bufferedPrefix != null && _bufferedPrefix.Position < _bufferedPrefix.Length)
        {
            return _bufferedPrefix.Read(buffer, offset, count);
        }

        return _inner.Read(buffer, offset, count);
    }

    // 其他必要的 Stream 方法代理给 _inner
    public override bool CanRead  => _inner.CanRead;
    public override bool CanSeek  => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length   => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
}