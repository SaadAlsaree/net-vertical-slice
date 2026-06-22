using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.Create__Entity__;

public class Create__Entity__Validator : AbstractValidator<Create__Entity__Command>
{
    private readonly __DbContext__ _context;

    public Create__Entity__Validator(__DbContext__ context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters")
            .MustAsync(BeUniqueName).WithMessage("Name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _context.Set<__Entity__>().AnyAsync(x => x.Name == name, cancellationToken);
    }
}
