namespace API.Controllers;

using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Models.Shop;
using Core.Contracts.Services;
using Data.Models;
using Utilities;

[ApiController]
[Route("/shops")]
public class ShopController : ControllerBase
{
    private readonly IService<Shop> _shopService;
    private readonly IMapper _mapper;

    public ShopController(IService<Shop> shopService, IMapper mapper)
    {
        this._shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpPost]
    public async Task<ActionResult<ShopViewModel>> CreateAsync([FromBody] ShopInputModel shopInputModel, CancellationToken cancellationToken)
    {
        var shop = this._mapper.Map<Shop>(shopInputModel);
        var create = await this._shopService.CreateAsync(shop, cancellationToken);
        if (!create.IsSuccessful) return this.Error(create);

        var viewModel = this._mapper.Map<ShopViewModel>(create.Data);
        return this.CreatedAtAction("GetById", new { id = create.Data.Id }, viewModel);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShopViewModel>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var getAllShops = await this._shopService.GetManyAsync(cancellationToken).ConfigureAwait(false);
        if (!getAllShops.IsSuccessful) return this.Error(getAllShops);

        var viewModels = this._mapper.Map<IEnumerable<ShopViewModel>>(getAllShops.Data.OrEmptyIfNull().IgnoreNullValues());
        return this.Ok(viewModels);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShopViewModel>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var getShop = await this._shopService.GetAsync(id, cancellationToken);
        if (!getShop.IsSuccessful) return this.Error(getShop);

        var shop = getShop.Data;
        if (shop is null) return this.NotFound();

        var viewModel = this._mapper.Map<ShopViewModel>(shop);
        return this.Ok(viewModel);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var getShop = await this._shopService.GetAsync(id, cancellationToken);
        if (!getShop.IsSuccessful) return this.Error(getShop);

        var shop = getShop.Data;
        if (shop is null) return this.NotFound();

        var deleteShop = await this._shopService.DeleteAsync(shop, cancellationToken);
        if (!deleteShop.IsSuccessful) return this.Error(deleteShop);

        return this.NoContent();
    }
}