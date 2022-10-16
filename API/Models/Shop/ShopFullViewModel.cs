namespace API.Models.Shop;

using API.Models.Product;

public class ShopFullViewModel : BaseEntityViewModel
{
    public string Name { get; set; }
    public string Address { get; set; }
    public IEnumerable<ProductFullViewModel> Products { get; set; }
}