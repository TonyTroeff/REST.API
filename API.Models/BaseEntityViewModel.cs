namespace API.Models;

using API.Models.HATEOAS;

public class BaseEntityViewModel
{
    public Guid Id { get; set; }
    public long LastModified { get; set; }
    
    public IEnumerable<HateoasLink> Links { get; set; }
}