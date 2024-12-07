using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to log to a file
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: @"C:\VS Code Projects\OpenApiWithAuth\Logging\Logs\Log-.txt", // Add a "-" for daily rolling
        rollingInterval: RollingInterval.Day
       // retainedFileCountLimit: 30 // Optional: Keep logs for 30 days
    )
    .CreateLogger();

// Add services to the container.
builder.Services.AddControllers();
builder.Logging.AddConsole();

// Add Swagger services with Basic Authentication
builder.Services.AddSwaggerGen(options =>
{
    // Define the Basic Authentication scheme
    options.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication with username and password"
    });

    // Apply the Basic Authentication to all API operations
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "BasicAuth"
                }
            },
            new string[] { }
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "A sample API for testing"
    });
});
// Add logging to the app
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
var app = builder.Build();
// Log the application's URLs
var urls = app.Urls; // Get configured URLs (if any)
Console.WriteLine($"Listening on: http://localhost:5238 (Default)");
// Use Swagger in the request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger in development environment
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty;  // Swagger UI at the root (optional)

        // Disable the "Try it out" button for unauthorized users
        c.OAuthClientId("your-client-id"); // Configure OAuth client (if using OAuth)
        c.OAuthAppName("Swagger API");
        c.DisplayRequestDuration();  // Show the request duration in Swagger UI

        // This will disable the "Try it out" button for the PUT method
        c.DocumentTitle = "API Documentation (Basic Auth Required)";
    });
}

// Add the BasicAuthMiddleware before UseAuthorization
app.UseMiddleware<BasicAuthMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
