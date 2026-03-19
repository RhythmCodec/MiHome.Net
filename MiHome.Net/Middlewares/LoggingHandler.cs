using Microsoft.Extensions.Logging;

namespace MiHome.Net.Middlewares;

internal class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 请求日志
        _logger.LogInformation("HTTP {Method} {Url}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
            _logger.LogInformation("Request Body: {Body}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // 响应日志
        _logger.LogInformation("Response {StatusCode}", response.StatusCode);

        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Body: {Body}", responseBody);
        }

        return response;
    }
}