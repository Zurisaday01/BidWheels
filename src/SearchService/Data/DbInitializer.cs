
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        try
        {
            // get connection string
            var connectionString = app.Configuration.GetConnectionString("MongoDbConnection");

            await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(connectionString));
            // Index item - the things are used to search for it
            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();


            // check items in database
            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

            // get the items from the auction service through  httpClient communication
            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(items.Count + " returned from the Auction service");

            // if there is data in the auction service store it in the search service
            if (items.Count > 0)
            {
                await
                 DB.SaveAsync(items);
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during initialization
            Console.WriteLine("An error occurred during database initialization: " + ex.Message);
        }

    }
}