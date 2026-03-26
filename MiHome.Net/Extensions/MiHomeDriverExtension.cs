using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using MiHome.Net.Apis;
using MiHome.Net.Middlewares;
using MiHome.Net.Service;
using Refit;

namespace MiHome.Net.Extensions;

public static class MiHomeDriverExtension
{
    public static IServiceCollection AddMiHomeDriver(this IServiceCollection services)
    {
        // Middlewares
        services.AddTransient<CryptoHandler>();
        services.AddTransient<DeviceControlCookiesHandler>();
#if DEBUG
        services.AddTransient<LoggingHandler>();
#endif
        services.AddTransient<RemoveResponsePrefixHandler>();

        // Services
        services.AddScoped<IMiAuth, MiAuth>();
        services.AddKeyedSingleton<ICookieContainer, MiAuthCookie>(Constants.MI_LOGIN_NAME);
        services.AddKeyedSingleton<ICookieContainer, MiDeviceControlCookie>(Constants.MI_CONTROL_DEVICE_NAME);
        services.AddScoped<IMiHomeDriver, MiHomeDriver>();
        services.AddScoped<IMiotCloud, MIotCloud>();
        services.AddScoped<IMiotLocal, MiotLocal>();

        // Refit clients
        services.AddRefitClient<IMiotCloudApi>(httpClientName: Constants.MI_CLOUD_API_NAME)
            .ConfigureHttpClient(c => { c.BaseAddress = new Uri(Constants.MI_CLOUD_API_URL); })
#if DEBUG
            .AddHttpMessageHandler<LoggingHandler>()
#endif
            ;

        // Setup JsonSerializer options
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(Constants.JsonSerializerOption)
        };

        services.AddRefitClient<IXiaoMiControlDevicesApi>(httpClientName: Constants.MI_CONTROL_DEVICE_NAME,
                settings: refitSettings)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var cookie = sp.GetRequiredKeyedService<ICookieContainer>(Constants.MI_CONTROL_DEVICE_NAME);
                return new HttpClientHandler
                {
                    CookieContainer = cookie.CookieContainer
                };
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(Constants.MI_CONTROL_DEVICE_URL);
                c.DefaultRequestVersion = HttpVersion.Version20;
                c.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));
                c.DefaultRequestHeaders.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("identity"));
            })
#if DEBUG
            .AddHttpMessageHandler<LoggingHandler>()
#endif
            .AddHttpMessageHandler<CryptoHandler>()
            .AddHttpMessageHandler<DeviceControlCookiesHandler>();

        services.AddRefitClient<IXiaoMiLoginApi>(httpClientName: Constants.MI_LOGIN_NAME)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var cookie = sp.GetRequiredKeyedService<ICookieContainer>(Constants.MI_LOGIN_NAME);
                return new HttpClientHandler
                {
                    CookieContainer = cookie.CookieContainer
                };
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(Constants.MI_LOGIN_URL);
                c.Timeout = TimeSpan.FromSeconds(120);
            })
#if DEBUG
            .AddHttpMessageHandler<LoggingHandler>()
#endif
            .AddHttpMessageHandler<RemoveResponsePrefixHandler>();

        return services;
    }
}