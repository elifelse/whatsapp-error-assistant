using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using WhatsAppErrorAssistant.Models;
using WhatsAppErrorAssistant.Services;

// .env dosyasını oku (varsa ortam değişkenlerine yükle)
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
    }
}

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------------------------
// Services
// --------------------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "WhatsApp Error Assistant API",
        Description = """
            WhatsApp hata mesajlarını ve log çıktılarını **Claude Haiku AI** ile analiz eden REST API.

            - `POST /analyze` — Hata mesajını gönder, analiz al.
            - Rate limit: **10 istek / dakika** (IP başına)
            """,
        Version = "v1",
        Contact = new() { Name = "WhatsApp Error Assistant" }
    });
});

builder.Services.AddSingleton<ClaudeService>();

// --------------------------------------------------------------------------
// Rate Limiting — 10 requests / minute per IP
// --------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("analyze-policy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit aşıldı. Dakikada en fazla 10 istek gönderebilirsiniz.",
            retryAfterSeconds = 60
        }, cancellationToken);
    };
});

// --------------------------------------------------------------------------
// App
// --------------------------------------------------------------------------
var app = builder.Build();

// Swagger UI — root'ta açılır (/)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WhatsApp Error Assistant v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "WhatsApp Error Assistant";
    options.DisplayRequestDuration();
});

app.UseRateLimiter();

// --------------------------------------------------------------------------
// Endpoints
// --------------------------------------------------------------------------

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("HealthCheck")
    .WithSummary("Health Check")
    .WithDescription("Servisin çalışıp çalışmadığını kontrol eder.")
    .WithTags("System")
    .Produces<object>(200);

// Analyze endpoint
app.MapPost("/analyze", async (AnalyzeRequest request, ClaudeService claudeService) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest(new { error = "Message alanı boş olamaz." });

    if (request.Message.Length > 4000)
        return Results.BadRequest(new { error = "Message en fazla 4000 karakter olabilir." });

    var result = await claudeService.AnalyzeAsync(request);
    return Results.Ok(result);
})
.RequireRateLimiting("analyze-policy")
.WithName("AnalyzeError")
.WithSummary("WhatsApp Hatası Analiz Et")
.WithDescription("Gönderilen WhatsApp hata mesajını Claude Haiku ile analiz eder ve olası nedenleri ile çözüm önerilerini döner.")
.WithTags("Analysis")
.Produces<AnalyzeResponse>(200)
.Produces<object>(400)
.Produces<object>(429)
.WithOpenApi();

app.Run();
