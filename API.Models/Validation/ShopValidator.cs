namespace API.Models.Validation;

using API.Models.Shop;
using FluentValidation;
using JetBrains.Annotations;

[UsedImplicitly]
public class ShopValidator : AbstractValidator<ShopInputModel>
{
    public ShopValidator()
    {
        this.RuleFor(s => s.Name).NotEmpty();
        this.RuleFor(s => s.Address).NotEmpty();
    }
}