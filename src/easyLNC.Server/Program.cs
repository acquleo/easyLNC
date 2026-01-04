using easyLNC.Abstract;
using easyLNC.Core;
using easyLNC.Screen.InfoHandler;
using easyLNC.ScreenCapture.WGC;
using easyLNC.ScreenControl.WinV1;
using easyLNC.ScreenStream.OMT;
using easyLNC.Server.Dummy;
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
builder.Services.AddSingleton<IScreenControlHandler, KeyboardMouseInputWinV1>();
builder.Services.AddSingleton<IServerTransportHandler, DummyServerTransportHandler>();
builder.Services.AddSingleton<CoreEasyLNC>();

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
        var coreEasyLNC = app.Services.GetRequiredService<CoreEasyLNC>();

        while (true)
        {

            screenCaptureHandler.Begin(screenInfoHandler.GetScreens().First(), out var streamCapture);

            streamHandler.Attach(streamCapture, out var streamInfo);

            break;

            Thread.Sleep(0);

            screenCaptureHandler?.End(streamCapture);

            Thread.Sleep(10);

        }

    }
    catch (Exception ex)
    {
        app.Lifetime.StopApplication();
    }
});

app.Run();
