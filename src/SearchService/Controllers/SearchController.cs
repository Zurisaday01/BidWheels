using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/Search")]
public class SearchController : ControllerBase
{
    // [HttpGet]
    // public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    // {
    //     var items = DB.PagedSearch<Item, Item>();

    //     // if the users searches for an spefic item
    //     if (!string.IsNullOrEmpty(searchParams.Query))
    //     {
    //         // check match
    //         items = items.Match(Search.Full, searchParams.Query).SortByTextScore();
    //     }

    //     // sorting
    //     // check if the value of order by is "make" or "new", sort correspondigly
    //     // if it does not match "make" or "new", then (_) sort by AuctionEnd
    //     items = searchParams.OrderBy switch
    //     {
    //         "make" => items.Sort(x => x.Ascending(a => a.Make)),
    //         "new" => items.Sort(x => x.Ascending(a => a.CreatedAt)),
    //         _ => items.Sort(x => x.Ascending(a => a.AuctionEnd))
    //     };

    //     // filtering
    //     items = searchParams.FilterBy switch
    //     {
    //         "finished" => items.Match(x => x.AuctionEnd < DateTime.UtcNow),
    //         "endingSoon" => items.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow), // ending in 6 hours
    //         _ => items.Match(x => x.AuctionEnd > DateTime.UtcNow)
    //     };

    //     // check if the user searches for the seller or winner
    //     if (!string.IsNullOrEmpty(searchParams.Seller))
    //     {
    //         items = items.Match(x => x.Seller == searchParams.Seller);
    //     }

    //     if (!string.IsNullOrEmpty(searchParams.Winner))
    //     {
    //         items = items.Match(x => x.Winner == searchParams.Winner);
    //     }

    //     // implementing pagination
    //     items.PageNumber(searchParams.PageNumber);
    //     items.PageSize(searchParams.PageSize);

    //     var result = await items.ExecuteAsync();


    //     // return an anonymous object
    //     return Ok(new
    //     {
    //         results = result.Results,
    //         pageCount = result.PageCount, // number of pages available
    //         totalCount = result.TotalCount // number of items that match the item
    //     });
    // }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.Search))
        {
            query = query.Match(Search.Full, searchParams.Search).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make))
                .Sort(x => x.Ascending(a => a.Model)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query = query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query = query.Match(x => x.Winner == searchParams.Winner);
        }

        query = query.PageNumber(searchParams.PageNumber);
        query = query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}

