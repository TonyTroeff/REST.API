namespace API.Profiles;

using AutoMapper;
using API.Models;
using API.Models.Shop;
using Data.Models;

public class ShopProfile : Profile
{
    public ShopProfile()
    {
        this.CreateMap<ShopInputModel, Shop>();
        this.CreateMap<Shop, ShopViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
    }
}