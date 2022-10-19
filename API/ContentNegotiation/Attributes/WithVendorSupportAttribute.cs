namespace API.ContentNegotiation.Attributes;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WithVendorSupportAttribute : ProducesAttribute
{
    public WithVendorSupportAttribute(params string[] supportedVendorMediaTypes)
        : base(MediaTypeNames.Application.Json)
    {
        foreach (var t in supportedVendorMediaTypes)
        {
            foreach (var s in VendorMediaTypes.SupportedSuffixes)
            {
                this.ContentTypes.Add(VendorMediaTypes.WithSuffix(t, s));
                this.ContentTypes.Add($"{VendorMediaTypes.WithSuffix(t, s)}; hateoas=true");
            }
        }
    }
}