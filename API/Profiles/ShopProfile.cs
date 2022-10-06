namespace API.Profiles;

using AutoMapper;
using API.Models;
using API.Models.Shop;
using Data.Models;
using JetBrains.Annotations;

[UsedImplicitly]
public class ShopProfile : Profile
{
    public ShopProfile()
    {
        this.CreateMap<ShopInputModel, Shop>();
        this.CreateMap<Shop, ShopFullViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
        this.CreateMap<Shop, ShopMinifiedViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
    }
}