namespace API.ContentNegotiation;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WithVendorSupportAttribute : ProducesAttribute
{
    public WithVendorSupportAttribute(params string[] supportedVendorMediaTypes)
        : base (MediaTypeNames.Application.Json)
    {
        foreach (var t in supportedVendorMediaTypes)
        {
            this.ContentTypes.Add(VendorMediaTypes.WithSuffix(t, "json"));
            this.ContentTypes.Add($"{VendorMediaTypes.WithSuffix(t, "json")}; hateoas=true");
        }
    }
}