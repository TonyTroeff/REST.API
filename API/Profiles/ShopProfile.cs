namespace API.Profiles;

using API.Models;
using API.Models.Shop;
using AutoMapper;
using Data.Models;

public class ShopProfile : Profile
{
    public ShopProfile()
    {
        this.CreateMap<ShopInputModel, Shop>();
        this.CreateMap<Shop, ShopFullViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
        this.CreateMap<Shop, ShopMinifiedViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
    }
}