using System.Text;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log request details
        var request = context.Request;
        request.EnableBuffering();

        var requestBody = string.Empty;
        if (request.ContentLength > 0)
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset the stream position for the next middleware
            }
        }

        var requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        // // Log to ILogger (writes to log file)
         _logger.LogInformation($"Request URL: {requestUrl}{requestBody}");

        // Intercept and log the response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Log response details
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation($"Response: {context.Response.StatusCode} | Body: {responseBodyText}");
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
