namespace API.Extensions;

using API.ContentNegotiation;
using Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

public static class ContentNegotiationExtensions
{
    public static ContentFormatDescriptor<T> GetContentFormatDescriptor<T>(this ControllerBase controller, IContentFormatManager<T> contentFormatManager)
    {
        if (controller is null) throw new ArgumentNullException(nameof(controller));
        if (contentFormatManager is null) throw new ArgumentNullException(nameof(contentFormatManager));
        
        ContentFormatDescriptor<T> formatDescriptor = null;
        if (controller.Request.Headers.TryGetValue(HeaderNames.Accept, out var acceptedMediaType)) formatDescriptor = contentFormatManager.GetContentFormat(acceptedMediaType);
        if (formatDescriptor is null) formatDescriptor = contentFormatManager.GetDefaultContentFormat();
        return formatDescriptor;
    }

    public static QueryEntityOptions<T> WithContentFormatSpecifics<T>(this QueryEntityOptions<T> options, ContentFormatDescriptor<T> contentFormatDescriptor)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (contentFormatDescriptor is null) throw new ArgumentNullException(nameof(contentFormatDescriptor));

        foreach (var transform in contentFormatDescriptor.Transforms) options.AddTransform(transform);
        return options;
    }
}