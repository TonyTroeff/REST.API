namespace API.Models.HATEOAS;

public class HateoasLink
{
    public string Url { get; set; }
    public string Rel { get; set; }
    public string Method { get; set; }
}