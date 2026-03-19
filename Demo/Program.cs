using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiHome.Net.Dto;
using MiHome.Net.Extensions;
using MiHome.Net.Miio;
using MiHome.Net.Service;
using QRCoder;

namespace Demo
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder();
            //添加小米米家的驱动服务
            hostBuilder.ConfigureServices(it => it.AddMiHomeDriver());
            hostBuilder.ConfigureServices(sc => sc.AddMemoryCache());
            hostBuilder.ConfigureServices(sc => sc.AddSingleton<IMiAuthStateProvider, FileAuthStateProvider>());
            var host = hostBuilder.Build();

            var miHomeDriver = host.Services.GetRequiredService<IMiHomeDriver>();
            var miAuth = host.Services.GetRequiredService<IMiAuth>();
            var authState = host.Services.GetRequiredService<IMiAuthStateProvider>();

            if (!await authState.StateValid())
            {
                var (loginUrl, pollUrl) = await miAuth.RequestLogin();

                {
                    var qrCode = QRCodeGenerator.GenerateQrCode(loginUrl, QRCodeGenerator.ECCLevel.L);
                    var qrStr = new AsciiQRCode(qrCode);
                    Console.WriteLine(qrStr.GetGraphic(1));
                }

                await miAuth.FinishLogin(pollUrl);
            }

            //列出所有家庭
            var homeList = await miHomeDriver.Cloud.GetHomeListAsync();
            var homeId = homeList.First().Id;

            //获取耗材列表
            var consumableItems = await miHomeDriver.Cloud.GetConsumableItemsAsync(homeId);

            //列出所有场景
            var sceneList = await miHomeDriver.Cloud.GetSceneListAsync(homeId);
            
            // 列出所有设备
            var deviceList = await miHomeDriver.Cloud.GetDeviceListAsync();
            
            // 选择测试设备
            var device = deviceList.FirstOrDefault(x => x.Name == "乌蒙变压器");
            if (device != null)
            {
                var spec = await miHomeDriver.Cloud.GetDeviceSpec(device.Model);
                var powerStateReqData = new GetPropertyDto
                {
                    Did = device.Did,
                    Siid = 2,
                    Piid = 1
                };
                var state = await miHomeDriver.Cloud.GetPropertyAsync(powerStateReqData);

                var actionReqData = new CallActionInputDto
                {
                    Did = device.Did,
                    SiId = 2,
                    AiId = 1,
                    In = [],
                };
                await miHomeDriver.Cloud.CallActionAsync(actionReqData);
                Console.Read();
            }

            // return;

            //执行场景,参数为场景id
            //   var executeResult = await miHomeDriver.Cloud.RunSceneAsync(sceneList.First().SceneId);
            //   //列出家庭里所有的智能家居设备
            //   var deviceList = await miHomeDriver.Cloud.GetDeviceListAsync();
            //   //通过米家app里自己设置的智能家居名称找出自己想要操作的智能家居设备
            //   var moonLight = deviceList.FirstOrDefault(it => it.Name == "月球灯");
            //   var xiaoAi = deviceList.FirstOrDefault(it => it.Name == "小爱音箱Play增强版");
            //   var cp5pro = deviceList.FirstOrDefault(it => it.Name == "Gosund智能排插CP5 Pro");
            //
            //   //通过设备型号获取设备规格
            //   var result = await miHomeDriver.Cloud.GetDeviceSpec(moonLight.Model);
            //   var result2 = await miHomeDriver.Cloud.GetDeviceSpec(xiaoAi.Model);
            //   var result3 = await miHomeDriver.Cloud.GetDeviceSpec(cp5pro.Model);
            //
            //   //使用云端方式调用Gosund智能排插CP5 Pro中4个开关中第3个开关的toggle方法
            //   var r11 = await miHomeDriver.Cloud.CallActionAsync(new CallActionInputDto()
            //   {
            //       Did = cp5pro.Did,
            //       AiId = 1,
            //       SiId = 5,
            //       In = new List<string>() { }
            //   });
            //
            //   //使用本地方式调用Gosund智能排插CP5 Pro中4个开关中第3个开关的toggle方法
            //   var r10 = await miHomeDriver.Local.CallActionAsync(cp5pro.LocalIp, cp5pro.Token, new CallActionPayload()
            //   {
            //       SiId = 5,
            //       AiId = 1,
            //       In = new List<string>() { }
            //   });
            //
            //   //使用小爱音箱Play增强版播放我们的自定义文字
            //   var r9 = await miHomeDriver.Cloud.CallActionAsync(new CallActionInputDto()
            //   {
            //       Did = xiaoAi.Did,
            //       AiId = 3,
            //       SiId = 5,
            //       In = new List<string>() { "门前大桥下，游过一群鸭" }
            //   });
            //
            //   //通过本地方式获取属性值
            //   var r1 = await miHomeDriver.Local.GetPropertyAsync(moonLight.LocalIp, moonLight.Token, new GetPropertyPayload()
            //   {
            //       SiId = 2,
            //       PiId = 1
            //   });
            //   //通过云端方式获取属性值
            //   var r7 = await miHomeDriver.Cloud.GetPropertyAsync(new GetPropertyDto()
            //   {
            //       Did = moonLight.Did,
            //       Siid = 2,
            //       Piid = 1
            //   });
            //
            //   //通过本地方式设置属性值
            //   var r2 = await miHomeDriver.Local.SetPropertyAsync(moonLight.LocalIp, moonLight.Token, new SetPropertyPayload()
            //   {
            //       SiId = 2,
            //       PiId = 1,
            //       Value = true
            //   });
            //
            //   //通过云端方式设置属性值
            //   var r8 = await miHomeDriver.Cloud.SetPropertyAsync(new SetPropertyDto()
            //   {
            //       Did = moonLight.Did,
            //       SiId = 2,
            //       PiId = 1,
            //       Value = false
            //   });
            //
            //   //通过本地方式批量获取属性值
            //   var r3 = await miHomeDriver.Local.GetPropertiesAsync(moonLight.LocalIp, moonLight.Token, new List<GetPropertyPayload>(){new GetPropertyPayload()
            //   {
            //       SiId = 2,
            //       PiId = 1
            //   }});
            //
            //   //通过云端方式批量获取属性值
            //   var r5 = await miHomeDriver.Cloud.GetPropertiesAsync(new List<GetPropertyDto>()
            //   {
            //       new GetPropertyDto()
            //       {
            //           Did = moonLight.Did,
            //           Siid = 2,
            //           Piid = 1
            //       }
            //   });
            //
            //   //通过本地方式批量设置属性值
            //   var r4 = await miHomeDriver.Local.SetPropertiesAsync(moonLight.LocalIp, moonLight.Token, new List<SetPropertyPayload>(){new SetPropertyPayload()
            //   {
            //       SiId = 2,
            //       PiId = 1,
            //       Value =true
            //   }});
            //
            //
            //   //通过云端方式批量设置属性值
            //   var r6 = await miHomeDriver.Cloud.SetPropertiesAsync(new List<SetPropertyDto>()
            // {
            //     new SetPropertyDto()
            //     {
            //         Did = moonLight.Did,
            //         SiId = 2,
            //         PiId = 1,
            //         Value = true
            //     }
            // });

            //退出登录
            // await miHomeDriver.Cloud.Logout();
        }
    }
}