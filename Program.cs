using Microsoft.AspNetCore.RateLimiting;
using NotificationApplication.Services;
using NotificationApplication.Services.Interfaces;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("discord-limit", config =>
    {
        config.PermitLimit = 10;                       // allow 10 requests
        config.Window = TimeSpan.FromMinutes(1);        // 10 requests per minute
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;                          // no queuing — reject immediately
    });

    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Rate limit exceeded. Max 10 messages per minute.", token);
    };
});

builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddHttpClient<IDiscordService, DiscordService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRateLimiter(); 

app.MapControllers();
app.Run();
