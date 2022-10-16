namespace API.Validation;

using API.Models.Product;
using FluentValidation;

public class ProductValidator : AbstractValidator<ProductInputModel>
{
    public ProductValidator()
    {
        this.RuleFor(p => p.Name).NotEmpty();
        this.RuleFor(p => p.Distributor).NotEmpty();
    }
}