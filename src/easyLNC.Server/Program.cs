using easyLNC.Abstract;
using easyLNC.Screen.InfoHandler;
using easyLNC.ScreenCapture.WGC;
using easyLNC.ScreenStream.OMT;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IScreenCaptureHandler, WindowsCaptureSessionHandler>();
builder.Services.AddSingleton<IScreenStreamHandler, OmtScreenStreamHandler>();
builder.Services.AddSingleton<IScreenInfoHandler, ScreenInfoHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStopping.Register(() =>
{
    var screenCaptureHandler = app.Services.GetRequiredService<IScreenCaptureHandler>();
    foreach(var streamHandler in screenCaptureHandler.Get())
    {
        screenCaptureHandler.End(streamHandler);
    }
});
app.Lifetime.ApplicationStopped.Register(() =>
{
    var screenCaptureHandler = app.Services.GetRequiredService<IScreenCaptureHandler>();
    foreach (var streamHandler in screenCaptureHandler.Get())
    {
        screenCaptureHandler.End(streamHandler);
    }
});
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var screenCaptureHandler = app.Services.GetRequiredService<IScreenCaptureHandler>();
        var streamHandler = app.Services.GetRequiredService<IScreenStreamHandler>();
        var screenInfoHandler = app.Services.GetRequiredService<IScreenInfoHandler>();

        while (true)
        {
            
            var captureSession = screenCaptureHandler.Begin(screenInfoHandler.GetScreens().First());

            streamHandler.Attach(captureSession);

            break;

            Thread.Sleep(0);

            screenCaptureHandler?.End(captureSession);

            Thread.Sleep(10);

        }

    }
    catch (Exception ex)
    {
        app.Lifetime.StopApplication();
    }
});

app.Run();
