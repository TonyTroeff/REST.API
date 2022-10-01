namespace API.Models.Shop;

using System.ComponentModel.DataAnnotations;

public class ShopInputModel
{
    [Required] public string Name { get; set; }
    [Required] public string Address { get; set; }
}