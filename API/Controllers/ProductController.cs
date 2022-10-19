namespace API.Controllers;

using API.ContentNegotiation;
using API.ContentNegotiation.Attributes;
using API.ContentNegotiation.Contracts;
using API.Extensions;
using API.Models;
using API.Models.Hateoas;
using API.Models.Product;
using AutoMapper;
using Core.Contracts.Services;
using Core.Options;
using Data.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Utilities;

[ApiController]
[Route("/shops/{shopId:guid}/products")]
[WithVendorSupport(VendorMediaTypes.ProductFull, VendorMediaTypes.ProductMinified)]
public class ProductController : ControllerBase
{
    private readonly IService<Shop> _shopService;
    private readonly IService<Product> _productService;
    private readonly IContentFormatManager<Product> _contentFormatManager;
    private readonly IMapper _mapper;
    private readonly IValidator<ProductInputModel> _validator;

    public ProductController(IService<Shop> shopService, IService<Product> productService, IContentFormatManager<Product> contentFormatManager, IMapper mapper, IValidator<ProductInputModel> validator = null)
    {
        this._shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        this._productService = productService ?? throw new ArgumentNullException(nameof(productService));
        this._contentFormatManager = contentFormatManager ?? throw new ArgumentNullException(nameof(contentFormatManager));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this._validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] Guid shopId, [FromBody] ProductInputModel productInputModel, CancellationToken cancellationToken)
    {
        var shopIsAccessible = await this._shopService.ExistsAsync(shopId, cancellationToken);
        if (!shopIsAccessible.IsSuccessful) return this.Error(shopIsAccessible);
        if (!shopIsAccessible.Data) return this.NotFound();
        
        var validationResult = await this.ValidateAsync(productInputModel, cancellationToken);
        if (validationResult is { IsValid: false }) return this.ValidationError(validationResult);

        return await this.CreateInternallyAsync(shopId, productInputModel, id: null, cancellationToken);
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid shopId, [FromRoute] Guid productId, [FromBody] ProductInputModel productInputModel, CancellationToken cancellationToken)
    {
        var shopIsAccessible = await this._shopService.ExistsAsync(shopId, cancellationToken);
        if (!shopIsAccessible.IsSuccessful) return this.Error(shopIsAccessible);
        if (!shopIsAccessible.Data) return this.NotFound();
        
        var validationResult = await this.ValidateAsync(productInputModel, cancellationToken);
        if (validationResult is { IsValid: false }) return this.ValidationError(validationResult);

        var getExistingProduct = await this._productService.GetAsync(productId, cancellationToken);
        if (!getExistingProduct.IsSuccessful) return this.Error(getExistingProduct);

        var product = getExistingProduct.Data;
        if (product is null) return await this.CreateInternallyAsync(shopId, productInputModel, productId, cancellationToken);

        this.MaterializeInputModel(productInputModel, shopId, product);
        var update = await this._productService.UpdateAsync(product, cancellationToken);
        if (!update.IsSuccessful) return this.Error(update);
        
        var getRepresentation = await this.GetRepresentationAsync(productId, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);

        return this.Ok(getRepresentation.Data); // We can also return "204 No content" here so the consumer of our API should decide whether or not to call the GetById endpoint explicitly after update.
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] Guid shopId, CancellationToken cancellationToken)
    {
        var shopIsAccessible = await this._shopService.ExistsAsync(shopId, cancellationToken);
        if (!shopIsAccessible.IsSuccessful) return this.Error(shopIsAccessible);
        if (!shopIsAccessible.Data) return this.NotFound();
        
        var contentFormatDescriptor = this.GetContentFormatDescriptor(this._contentFormatManager);

        var getOptions = new QueryEntityOptions<Product>().WithContentFormatSpecifics(contentFormatDescriptor);
        var getAllProducts = await this._productService.GetManyAsync(cancellationToken, getOptions).ConfigureAwait(false);
        if (!getAllProducts.IsSuccessful) return this.Error(getAllProducts);

        var viewModels = getAllProducts.Data.OrEmptyIfNull().IgnoreNullValues().Select(x => this.ToViewModel(x, contentFormatDescriptor));
        return this.Ok(viewModels);
    }

    [HttpGet("{productId:guid}")]
    [HttpHead("{productId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid shopId, [FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var shopIsAccessible = await this._shopService.ExistsAsync(shopId, cancellationToken);
        if (!shopIsAccessible.IsSuccessful) return this.Error(shopIsAccessible);
        if (!shopIsAccessible.Data) return this.NotFound();

        var getRepresentation = await this.GetRepresentationAsync(productId, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);

        var product = getRepresentation.Data;
        if (product is null) return this.NotFound();

        return this.Ok(product);
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid shopId, [FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var shopIsAccessible = await this._shopService.ExistsAsync(shopId, cancellationToken);
        if (!shopIsAccessible.IsSuccessful) return this.Error(shopIsAccessible);
        if (!shopIsAccessible.Data) return this.NotFound();

        var getProduct = await this._productService.GetAsync(productId, cancellationToken);
        if (!getProduct.IsSuccessful) return this.Error(getProduct);

        var product = getProduct.Data;
        if (product is null) return this.NotFound();

        var deleteProduct = await this._productService.DeleteAsync(product, cancellationToken);
        if (!deleteProduct.IsSuccessful) return this.Error(deleteProduct);

        return this.NoContent();
    }

    private async Task<ValidationResult> ValidateAsync(ProductInputModel inputModel, CancellationToken cancellationToken)
    {
        if (this._validator is null) return null;
        return await this._validator.ValidateAsync(inputModel, cancellationToken);
    }

    private async Task<IActionResult> CreateInternallyAsync(Guid shopId, ProductInputModel inputModel, Guid? id, CancellationToken cancellationToken)
    {
        var product = this.MaterializeInputModel(inputModel, shopId);
        if (id.HasValue) product.Id = id.Value;

        var create = await this._productService.CreateAsync(product, cancellationToken);
        if (!create.IsSuccessful) return this.Error(create);

        var getRepresentation = await this.GetRepresentationAsync(create.Data.Id, cancellationToken);
        if (!getRepresentation.IsSuccessful) return this.Error(getRepresentation);
        
        return this.CreatedAtAction("GetById", new { create.Data.ShopId, ProductId = create.Data.Id }, getRepresentation.Data);
    }

    private async Task<OperationResult<object>> GetRepresentationAsync(Guid id, CancellationToken cancellationToken)
    {
        var operationResult = new OperationResult<object>();
        
        var contentFormatDescriptor = this.GetContentFormatDescriptor(this._contentFormatManager);

        var getOptions = new QueryEntityOptions<Product>().WithContentFormatSpecifics(contentFormatDescriptor);
        var getProduct = await this._productService.GetAsync(id, cancellationToken, getOptions);
        if (!getProduct.IsSuccessful) return operationResult.AppendErrors(getProduct);

        var product = getProduct.Data;
        if (product is not null) operationResult.Data = this.ToViewModel(product, contentFormatDescriptor);

        return operationResult;
    }

    private object ToViewModel(Product product, ContentFormatDescriptor<Product> formatDescriptor)
    {
        var viewModel = this._mapper.Map(product, typeof(Product), formatDescriptor.OutputType);
        if (formatDescriptor.WithHateoasLinks && viewModel is BaseEntityViewModel baseEntityViewModel) baseEntityViewModel.Links = this.GetLinks(product);

        return viewModel;
    }

    private Product MaterializeInputModel(ProductInputModel inputModel, Guid shopId, Product existingProduct = null) => this._mapper.Map(inputModel, existingProduct, options => options.Items["shop_id"] = shopId);

    private IEnumerable<HateoasLink> GetLinks(Product product)
    {
        if (product is null) return Enumerable.Empty<HateoasLink>();

        var links = new List<HateoasLink>(capacity: 2)
        {
            new() { Url = this.AbsoluteActionUrl("GetById", "Product", new { product.ShopId, ProductId = product.Id }), Rel = "self", Method = HttpMethods.Get },
            new() { Url = this.AbsoluteActionUrl("Update", "Product", new { product.ShopId, ProductId = product.Id }), Rel = "update", Method = HttpMethods.Put },
            new() { Url = this.AbsoluteActionUrl("Delete", "Product", new { product.ShopId, ProductId = product.Id }), Rel = "delete", Method = HttpMethods.Delete },
        };

        return links;
    }
}