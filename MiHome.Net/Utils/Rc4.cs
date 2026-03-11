namespace MiHome.Net.Utils;

public class Rc4
{
    private int        idx, jdx;
    private List<byte> ksa;

    public Rc4(string key)
    {
        var pwd = Convert.FromBase64String(key);
        var cnt = pwd.Length;
        var tempKsa = new List<byte>();
        for (int i = 0; i < 256; i++)
        {
            tempKsa.Add((byte)i);
        }

        var j = 0;
        for (int i = 0; i < 256; i++)
        {
            j = (j + tempKsa[i] + pwd[i % cnt]) & 255;
            (tempKsa[i], tempKsa[j]) = (tempKsa[j], tempKsa[i]);
        }

        ksa = tempKsa;
        idx = 0;
        jdx = 0;
    }

    public byte[] Crypt(byte[] data)
    {
        var ksa = this.ksa;
        var i = idx;
        var j = jdx;
        var outList = new List<byte>();
        foreach (byte byt in data)
        {
            i = (i + 1) & 255;
            j = (j + ksa[i]) & 255;
            (ksa[i], ksa[j]) = (ksa[j], ksa[i]);
            var r = (byte)(byt ^ ksa[(ksa[i] + ksa[j]) & 255]);
            outList.Add(r);
        }

        idx = i;
        jdx = j;
        this.ksa = ksa;
        return outList.ToArray();
    }

    public Rc4 Init1024()
    {
        var ksa = this.ksa;
        var i = idx;
        var j = jdx;
        for (var k = 0; k != 1024; k++)
        {
            i = (i + 1) & 255;
            j = (j + ksa[i]) & 255;
            (ksa[i], ksa[j]) = (ksa[j], ksa[i]);
        }

        idx = i;
        jdx = j;
        this.ksa = ksa;
        return this;
    }
}