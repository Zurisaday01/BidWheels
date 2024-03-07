using AuctionService.DTOs;
using AuctionService.Model;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // mapp from one format to anothers
        // from db to user
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        // from user to db
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(desination => desination.Item, options => options.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();

    }
}