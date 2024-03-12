using MongoDB.Entities;

namespace SearchService.Services;

//  Adding Http communication to get the data
public class AuctionServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {

        // get the updated data from database
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();
        //Project specify that only the UpdatedAt field should be included in the result set

        // return data in JSON format, connection to the other service
        return await _httpClient.GetFromJsonAsync<List<Item>>("/api/auctions?date=" + lastUpdated);
    }
}