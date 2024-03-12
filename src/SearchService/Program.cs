using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuctionServiceUrl"]);

}).AddPolicyHandler(GetPolicy());


var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// This ensures that the application is ready before connecting to the database
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception error)
    {
        Console.WriteLine("Error: ", error);
    }
});

app.Run();

// keep trying to connect to the auction service every 3 seconds, until there is a sucessful connection
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

