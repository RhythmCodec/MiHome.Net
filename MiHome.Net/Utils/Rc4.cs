using System.Diagnostics.CodeAnalysis;

namespace MiHome.Net.Utils;

public class Rc4
{
    private int    _idx, _jdx;
    private byte[] _ksa;

    public Rc4(byte[] key)
    {
        Ksa(key);
        Init1024();
    }

    public void Crypt(Span<byte> data)
    {
        var i = _idx;
        var j = _jdx;

        foreach (ref var byt in data)
        {
            i = (i + 1) & 255;
            j = (j + _ksa[i]) & 255;
            (_ksa[i], _ksa[j]) = (_ksa[j], _ksa[i]);
            var r = (byte)(byt ^ _ksa[(_ksa[i] + _ksa[j]) & 255]);
            byt = r;
        }

        _idx = i;
        _jdx = j;
    }

    [MemberNotNull(nameof(_ksa))]
    private void Ksa(ReadOnlySpan<byte> key)
    {
        var cnt = key.Length;
        var tempKsa = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();


        var j = 0;
        for (var i = 0; i < 256; i++)
        {
            j = (j + tempKsa[i] + key[i % cnt]) & 255;
            (tempKsa[i], tempKsa[j]) = (tempKsa[j], tempKsa[i]);
        }

        _ksa = tempKsa;
        _idx = 0;
        _jdx = 0;
    }

    private void Init1024()
    {
        var i = _idx;
        var j = _jdx;
        for (var k = 0; k != 1024; k++)
        {
            i = (i + 1) & 255;
            j = (j + _ksa[i]) & 255;
            (_ksa[i], _ksa[j]) = (_ksa[j], _ksa[i]);
        }

        _idx = i;
        _jdx = j;
    }
}