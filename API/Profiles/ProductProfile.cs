namespace API.Profiles;

using API.Models;
using API.Models.Product;
using AutoMapper;
using Data.Models;
using JetBrains.Annotations;

[UsedImplicitly]
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        this.CreateMap<ProductInputModel, Product>().ForMember(p => p.ShopId,
            conf =>
            {
                conf.PreCondition((_,_,context) => context.Items.ContainsKey("shop_id"));
                conf.MapFrom((_,_,_,context) => context.Items["shop_id"]);
            });
        this.CreateMap<Product, ProductFullViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
        this.CreateMap<Product, ProductMinifiedViewModel>().IncludeBase<BaseEntity, BaseEntityViewModel>();
    }
}