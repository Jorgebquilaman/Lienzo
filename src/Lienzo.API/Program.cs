using Lienzo.API.Extensions;
using Lienzo.API.Hubs;
using Lienzo.API.Middleware;
using Lienzo.API.Services;
using Lienzo.Application;
using Lienzo.Application.Interfaces;
using Lienzo.Infrastructure;
using Lienzo.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSwaggerWithAuth();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IRealTimeNotifier, RealTimeNotifier>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        Directory.CreateDirectory(env.WebRootPath);
        await SeedData.SeedAsync(services);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "An error occurred while seeding the database");
    }
}

app.Run();
