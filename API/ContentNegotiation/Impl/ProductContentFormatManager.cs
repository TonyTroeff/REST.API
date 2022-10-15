namespace API.ContentNegotiation.Impl;

using API.Models.Product;
using Data.Models;

public class ProductContentFormatManager : BaseContentFormatManager<Product>
{
    public ProductContentFormatManager()
    {
        this.AddVendor(VendorMediaTypes.ProductFull, typeof(ProductFullViewModel));
        this.AddVendor(VendorMediaTypes.ProductMinified, typeof(ProductMinifiedViewModel));
    }
}