using CoreRCON;
using CoreRCON.Parsers.Standard;
using System.Net;
using System.Timers;
using Timer = System.Timers.Timer;
using RconSharp;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var host = app.Configuration.GetValue<string?>("RconHost");
var password = app.Configuration.GetValue<string?>("RconPassword");
var RconPort = app.Configuration.GetValue<int>("RconPort");
var port = app.Configuration.GetValue<int?>("Port") ?? 8888;

var client = RconClient.Create(host, RconPort);
await client.ConnectAsync();
var connected = await client.AuthenticateAsync(password);
if (!connected)
{
    Console.WriteLine("Unable to connect");
    return 1;
}

var playersConnected = false;

var timer = new Timer(5000);
timer.Start();
timer.Enabled = true;
timer.Elapsed += async (sender, eventArgs) =>
{
    Console.WriteLine("Checking for connected players");
    var playerlist = await client.ExecuteCommandAsync("listplayers");
    var foundPlayers = playerlist.Contains(',');

    if (playersConnected == foundPlayers) return;

    playersConnected = foundPlayers;
    var scale = foundPlayers ? 1 : 0.01;
    _ = client.ExecuteCommandAsync($"slomo {scale}");
    Console.WriteLine($"Set slomo to {scale}");
};

app.MapGet("/pause", async () =>
{
    if (await Slomo(0.01f))
        return new Response("Ok");
    return new Response("Error");
});

app.MapGet("/play", async () =>
{
    if (await Slomo(1))
        return new Response("Ok");
    return new Response("Error");
});

async Task<bool> Slomo(float scale)
{
    var client = RconClient.Create(host, RconPort);
    await client.ConnectAsync();
    var connected = await client.AuthenticateAsync(password);
    if (!connected) return false;
    await client.ExecuteCommandAsync($"slomo {scale}");
    return true;
}


app.Run($"http://localhost:{port}");

return 0;

record Response(string status);
