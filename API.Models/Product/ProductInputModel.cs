namespace API.Models.Product;

public class ProductInputModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Distributor { get; set; }
}