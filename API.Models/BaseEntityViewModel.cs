namespace API.Models;

using System.Text.Json.Serialization;
using API.Models.Hateoas;

public class BaseEntityViewModel
{
    public Guid Id { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<HateoasLink> Links { get; set; }
}