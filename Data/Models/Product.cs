namespace Data.Models;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Distributor { get; set; }
    
    public Guid ShopId { get; set; }
    public Shop Shop { get; set; }
}