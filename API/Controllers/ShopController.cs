namespace API.Controllers;

using API.ContentNegotiation;
using API.Extensions;
using API.Models;
using API.Models.Hateoas;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Models.Shop;
using Core.Contracts.Services;
using Core.Options;
using Data.Models;
using FluentValidation;
using FluentValidation.Results;
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

    [HttpPut("{shopId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid shopId, [FromBody] ShopInputModel shopInputModel, CancellationToken cancellationToken)
    {
        var validationResult = await this.ValidateAsync(shopInputModel, cancellationToken);
        if (validationResult is { IsValid: false }) return this.ValidationError(validationResult);

        var getExistingShop = await this._shopService.GetAsync(shopId, cancellationToken);
        if (!getExistingShop.IsSuccessful) return this.Error(getExistingShop);

        var shop = getExistingShop.Data;
        if (shop is null) return await this.CreateInternallyAsync(shopInputModel, shopId, cancellationToken);

        this.MaterializeInputModel(shopInputModel, shop);
        var update = await this._shopService.UpdateAsync(shop, cancellationToken);
        if (!update.IsSuccessful) return this.Error(update);
        
        var getRepresentation = await this.GetRepresentationAsync(shopId, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);

        return this.Ok(getRepresentation.Data); // We can also return "204 No content" here so the consumer of our API should decide whether or not to call the GetById endpoint explicitly after update.
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var contentFormatDescriptor = this.GetContentFormatDescriptor(this._contentFormatManager);

        var getOptions = new QueryEntityOptions<Shop>().WithContentFormatSpecifics(contentFormatDescriptor);
        var getAllShops = await this._shopService.GetManyAsync(cancellationToken, getOptions);
        if (!getAllShops.IsSuccessful) return this.Error(getAllShops);

        var viewModels = getAllShops.Data.OrEmptyIfNull().IgnoreNullValues().Select(x => this.ToViewModel(x, contentFormatDescriptor));
        return this.Ok(viewModels);
    }

    [HttpGet("{shopId:guid}")]
    [HttpHead("{shopId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid shopId, CancellationToken cancellationToken)
    {
        var getRepresentation = await this.GetRepresentationAsync(shopId, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);

        var representation = getRepresentation.Data;
        if (representation is null) return this.NotFound();
        
        return this.Ok(representation);
    }

    [HttpDelete("{shopId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid shopId, CancellationToken cancellationToken)
    {
        var getShop = await this._shopService.GetAsync(shopId, cancellationToken);
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
        var shop = this.MaterializeInputModel(inputModel);
        if (id.HasValue) shop.Id = id.Value;

        var create = await this._shopService.CreateAsync(shop, cancellationToken);
        if (!create.IsSuccessful) return this.Error(create);

        var getRepresentation = await this.GetRepresentationAsync(create.Data.Id, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);
        
        return this.CreatedAtAction("GetById", new { ShopId = create.Data.Id }, getRepresentation.Data);
    }

    private async Task<OperationResult<object>> GetRepresentationAsync(Guid id, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<object>();
        
        var contentFormatDescriptor = this.GetContentFormatDescriptor(this._contentFormatManager);

        var getOptions = new QueryEntityOptions<Shop>().WithContentFormatSpecifics(contentFormatDescriptor);
        var getShop = await this._shopService.GetAsync(id, cancellationToken, getOptions);
        if (!getShop.IsSuccessful) return operationResult.AppendErrors(getShop);

        var shop = getShop.Data;
        if (shop is not null) operationResult.Data = this.ToViewModel(shop, contentFormatDescriptor);

        return operationResult;
    }

    private object ToViewModel(Shop shop, ContentFormatDescriptor<Shop> formatDescriptor)
    {
        var viewModel = this._mapper.Map(shop, typeof(Shop), formatDescriptor.OutputType);
        if (formatDescriptor.WithHateoasLinks && viewModel is BaseEntityViewModel baseEntityViewModel) baseEntityViewModel.Links = this.GetLinks(shop);

        return viewModel;
    }   
    
    private Shop MaterializeInputModel(ShopInputModel inputModel, Shop existingProduct = null) => this._mapper.Map(inputModel, existingProduct);

    private IEnumerable<HateoasLink> GetLinks(Shop shop)
    {
        if (shop is null) return Enumerable.Empty<HateoasLink>();

        var links = new List<HateoasLink>(capacity: 2)
        {
            new() { Url = this.AbsoluteActionUrl("GetById", "Shop", new { ShopId = shop.Id }), Rel = "self", Method = HttpMethods.Get },
            new() { Url = this.AbsoluteActionUrl("Delete", "Shop", new { ShopId = shop.Id }), Rel = "delete", Method = HttpMethods.Delete }
        };

        return links;
    }
}