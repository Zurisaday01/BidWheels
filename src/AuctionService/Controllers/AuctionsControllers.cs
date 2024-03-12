using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsControllers : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsControllers(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        // you can continue to build upon it with additional LINQ operations before executing the query against the database.
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    //     var query = _context.Auctions
    //                     .OrderBy(x => x.Item.Make)
    //                     .AsQueryable();

    // // Filter auctions for a specific make
    // query = query.Where(a => a.Item.Make == "SomeMake");

    // // Further filter auctions based on another condition
    // query = query.Where(a => a.StartTime > DateTime.Now);

    // // Select only specific properties
    // var result = query.Select(a => new 
    //                                 {
    //                                     AuctionId = a.Id,
    //                                     ItemName = a.Item.Name,
    //                                     StartTime = a.StartTime
    //                                 })
    //                     .ToList();

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();


        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auction)
    {
        // pass the auctionDto structure to action structure
        var newAuction = _mapper.Map<Auction>(auction);
        newAuction.Seller = "Test";

        // save the new auction in memory
        _context.Auctions.Add(newAuction);

        // return the number of row changes from database
        // 1 row > 0 = true == A change was performed
        // 0 > 0 false == something went wrong
        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");

        // result created, where can you find it
        return CreatedAtAction(nameof(GetAuctionById), new { newAuction.Id }, _mapper.Map<AuctionDto>(newAuction));
        // the headers of the response will contain the location of the newly resource
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auction)
    {
        var updatedAuction = await _context.Auctions.Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        // add default values to the optional properties
        updatedAuction.Item.Make = auction.Make ?? updatedAuction.Item.Make;
        updatedAuction.Item.Model = auction.Model ?? updatedAuction.Item.Model;
        updatedAuction.Item.Color = auction.Color ?? updatedAuction.Item.Color;
        updatedAuction.Item.Mileage = auction.Mileage ?? updatedAuction.Item.Mileage;
        updatedAuction.Item.Year = auction.Year ?? updatedAuction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes");

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auctionToDelete = await _context.Auctions.FindAsync(id);

        if (auctionToDelete == null) return NotFound();

        // TODO: check seller == username

        _context.Auctions.Remove(auctionToDelete);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not remove auction");

        return Ok();
    }

}

