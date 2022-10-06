namespace API.Controllers;

using API.ContentNegotiation;
using API.Extensions;
using API.Models.Hateoas;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Models.Shop;
using Core.Contracts.Services;
using Data.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Net.Http.Headers;
using Utilities;

[ApiController]
[Route("/shops")]
[WithVendorSupport(VendorMediaTypes.ShopFull, VendorMediaTypes.ShopMinified)]
public class ShopController : ControllerBase
{
    private readonly IService<Shop> _shopService;
    private readonly IContentFormatManager<Shop> _contentFormatManager;
    private readonly IMapper _mapper;
    private readonly IValidator<ShopInputModel> _validator;

    public ShopController(IService<Shop> shopService, IContentFormatManager<Shop> contentFormatManager, IMapper mapper, IValidator<ShopInputModel> validator = null)
    {
        this._shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        this._contentFormatManager = contentFormatManager ?? throw new ArgumentNullException(nameof(contentFormatManager));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this._validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShopInputModel shopInputModel, CancellationToken cancellationToken)
    {
        var validationResult = await this.ValidateAsync(shopInputModel, cancellationToken);
        if (validationResult is { IsValid: false }) return this.ValidationError(validationResult);

        return await this.CreateInternallyAsync(shopInputModel, id: null, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ShopInputModel shopInputModel, CancellationToken cancellationToken)
    {
        var validationResult = await this.ValidateAsync(shopInputModel, cancellationToken);
        if (validationResult is { IsValid: false }) return this.ValidationError(validationResult);

        var getExistingShop = await this._shopService.GetAsync(id, cancellationToken);
        if (!getExistingShop.IsSuccessful) return this.Error(getExistingShop);

        var shop = getExistingShop.Data;
        if (shop is null) return await this.CreateInternallyAsync(shopInputModel, id, cancellationToken);

        this._mapper.Map(shopInputModel, shop);
        var update = await this._shopService.UpdateAsync(shop, cancellationToken);
        if (!update.IsSuccessful) return this.Error(update);

        return this.Ok(this.ToViewModel(shop)); // We can also return "204 No content" here so the consumer of our API should decide whether or not to call the GetById endpoint explicitly after update.
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var getAllShops = await this._shopService.GetManyAsync(cancellationToken).ConfigureAwait(false);
        if (!getAllShops.IsSuccessful) return this.Error(getAllShops);

        var viewModels = getAllShops.Data.OrEmptyIfNull().IgnoreNullValues().Select(this.ToViewModel);
        return this.Ok(viewModels);
    }

    [HttpGet("{id:guid}")]
    [HttpHead("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var getShop = await this._shopService.GetAsync(id, cancellationToken);
        if (!getShop.IsSuccessful) return this.Error(getShop);

        var shop = getShop.Data;
        if (shop is null) return this.NotFound();

        return this.Ok(this.ToViewModel(shop));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var getShop = await this._shopService.GetAsync(id, cancellationToken);
        if (!getShop.IsSuccessful) return this.Error(getShop);

        var shop = getShop.Data;
        if (shop is null) return this.NotFound();

        var deleteShop = await this._shopService.DeleteAsync(shop, cancellationToken);
        if (!deleteShop.IsSuccessful) return this.Error(deleteShop);

        return this.NoContent();
    }

    private async Task<ValidationResult> ValidateAsync(ShopInputModel inputModel, CancellationToken cancellationToken)
    {
        if (this._validator is null) return null;
        return await this._validator.ValidateAsync(inputModel, cancellationToken);
    }

    private async Task<IActionResult> CreateInternallyAsync(ShopInputModel inputModel, Guid? id, CancellationToken cancellationToken)
    {
        var shop = this._mapper.Map<Shop>(inputModel);
        if (id.HasValue) shop.Id = id.Value;

        var create = await this._shopService.CreateAsync(shop, cancellationToken);
        if (!create.IsSuccessful) return this.Error(create);

        return this.CreatedAtAction("GetById", new { create.Data.Id }, this.ToViewModel(create.Data));
    }

    private object ToViewModel(Shop shop)
    {
        ContentFormatDescriptor formatDescriptor = null;
        if (this.Request.Headers.TryGetValue(HeaderNames.Accept, out var acceptedMediaType)) formatDescriptor = this._contentFormatManager.GetContentFormat(acceptedMediaType);
        if (formatDescriptor is null) formatDescriptor = this._contentFormatManager.GetDefaultContentFormat();
        
        return this._mapper.Map(shop, typeof(Shop), formatDescriptor.OutputType, ConfigureMappingProcess);

        void ConfigureMappingProcess(IMappingOperationOptions<object, object> options)
        {
            if (options?.Items is null) throw new InvalidOperationException("Invalid mapping options were provided.");
            if (formatDescriptor.WithHateoasLinks) options.Items["links"] = this.GetLinks(shop);
        }
    }

    private IEnumerable<HateoasLink> GetLinks(Shop shop)
    {
        if (shop is null) return Enumerable.Empty<HateoasLink>();

        var links = new List<HateoasLink>(capacity: 2)
        {
            new() { Url = this.AbsoluteActionUrl("GetById", "Shop", new { shop.Id }), Rel = "self", Method = HttpMethods.Get },
            new() { Url = this.AbsoluteActionUrl("Delete", "Shop", new { shop.Id }), Rel = "delete", Method = HttpMethods.Delete }
        };

        return links;
    }
}