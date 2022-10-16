namespace API.ContentNegotiation;

using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Net.Http.Headers;
using Utilities;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class SupportedContentTypesAttribute : Attribute, IActionConstraint
{
    private readonly HashSet<MediaTypeHeaderValue> _supportedContentTypes = new ();

    public SupportedContentTypesAttribute(params string[] supportedContentTypes)
    {
        foreach (var contentType in supportedContentTypes.OrEmptyIfNull().IgnoreNullOrWhitespaceValues())
        {
            foreach (var supportedSuffix in VendorMediaTypes.SupportedSuffixes)
            {
                var suffixSpecificContentType = VendorMediaTypes.WithSuffix(contentType, supportedSuffix);
                this._supportedContentTypes.Add(MediaTypeHeaderValue.Parse(suffixSpecificContentType));
            }
        }
    }

    public int Order => 0;

    public bool Accept(ActionConstraintContext context)
    {
        if (context.RouteContext.HttpContext.Request.Headers.TryGetValue(HeaderNames.ContentType, out var contentTypesInHeader) == false) return false;

        foreach (var contentTypeValue in contentTypesInHeader)
        {
            if (MediaTypeHeaderValue.TryParse(contentTypeValue, out var parsedContentTypeValue) && this._supportedContentTypes.Contains(parsedContentTypeValue))
                return true;
        }

        return false;
    }
}