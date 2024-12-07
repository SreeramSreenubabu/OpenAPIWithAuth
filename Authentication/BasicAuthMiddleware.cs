using System.Net;
using System.Text;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if Authorization header exists
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;  // 401 Unauthorized
            await context.Response.WriteAsync("Authorization header missing");
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Basic ".Length).Trim();
            try
            {
                // Decode credentials from the token
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(token)).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                // Validate credentials (this is your hardcoded check for demo purposes)
                if (ValidateCredentials(username, password))
                {
                    await _next(context);  // Credentials are valid, proceed to the next middleware
                    return;
                }
            }
            catch (FormatException)  // Catch any issues in decoding the credentials
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;  // 401 Unauthorized
                await context.Response.WriteAsync("Invalid Authorization header format");
                return;
            }
        }

        // If no valid authorization or wrong credentials, return Unauthorized
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    // Method to validate username and password
    private bool ValidateCredentials(string username, string password)
    {
        // Replace this with your actual validation logic (e.g., check against a database)
        return username == "admin" && password == "password";  // Example check
    }
}
