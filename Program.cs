using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
   
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Logging.AddConsole();

// Step 2: Configure Services
builder.Services.AddControllers(); // Add Controllers for the API
builder.Services.AddEndpointsApiExplorer(); // Enable Endpoints API Explorer
builder.Services.AddSwaggerGen(options =>
{
    // Define Basic Authentication scheme
    options.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication with username and password"
    });

    // Apply Basic Authentication to all API operations
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

    // Configure Swagger document
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "A sample API for testing"
    });
});

var app = builder.Build();

// Step 3: Configure Middleware and Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed error pages in development
    app.UseSwagger(); // Enable Swagger for API documentation
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at the root
        c.DocumentTitle = "API Documentation (Basic Auth Required)";
        c.DisplayRequestDuration(); // Show request duration in Swagger UI
    });
}

app.UseHttpsRedirection(); // Enforce HTTPS
app.UseMiddleware<BasicAuthMiddleware>(); // Add Basic Authentication Middleware
app.UseMiddleware<OpenAPIWithAuth.Logging.RequestResponseLoggingMiddleware>(); // Log Request/Response Middleware
app.UseAuthorization(); // Add Authorization Middleware

// Step 4: Map Controllers
app.MapControllers(); // Map API Controllers

// Step 5: Run the Application
app.Run();
