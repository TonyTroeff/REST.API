namespace API.ContentNegotiation.Impl;

using API.Models.Shop;
using Data.Models;

public class ShopContentFormatManager : BaseContentFormatManager<Shop>
{
    protected override ContentFormatDescriptor Default { get; } = new (typeof(ShopFullViewModel), true);
    protected override IDictionary<string, Type> SupportedVendors { get; } = new Dictionary<string, Type> { { VendorMediaTypes.ShopFull, typeof(ShopFullViewModel) }, { VendorMediaTypes.ShopMinified, typeof(ShopMinifiedViewModel) } };
}