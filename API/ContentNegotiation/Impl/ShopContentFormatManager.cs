namespace API.ContentNegotiation.Impl;

using API.Models.Shop;
using Data.Models;
using Microsoft.EntityFrameworkCore;

public class ShopContentFormatManager : BaseContentFormatManager<Shop>
{
    public ShopContentFormatManager()
    {
        this.AddVendor(VendorMediaTypes.ShopFull, typeof(ShopFullViewModel), IncludeProducts);
        this.AddVendor(VendorMediaTypes.ShopMinified, typeof(ShopMinifiedViewModel));
    }

    private static IQueryable<Shop> IncludeProducts(IQueryable<Shop> queryable)
    {
        if (queryable is null) throw new ArgumentNullException(nameof(queryable));
        return queryable.Include(s => s.Products);
    }
}