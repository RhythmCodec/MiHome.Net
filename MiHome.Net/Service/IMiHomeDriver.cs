namespace MiHome.Net.Service;

/// <summary>
/// 小米智能家居sdk
/// </summary>
public interface IMiHomeDriver
{
    IMiotCloud Cloud {  get; }

    IMiotLocal Local { get; }

}

internal class MiHomeDriver : IMiHomeDriver
{

    public  IMiotCloud Cloud { get; }

    public  IMiotLocal Local { get; }

    public MiHomeDriver(IMiotCloud cloud, IMiotLocal local)
    {
        Cloud = cloud;
        Local = local;
    }
}