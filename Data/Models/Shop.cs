namespace Data.Models;

public class Shop : BaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    
    public ICollection<Product> Products { get; set; }
}