using AuctionService.Model;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    // Table names (plural)
    public DbSet<Auction> Auctions { get; set; }

}