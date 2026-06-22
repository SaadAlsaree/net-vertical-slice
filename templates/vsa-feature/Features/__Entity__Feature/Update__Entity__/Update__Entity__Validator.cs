using __RootNamespace__.Data;
using __RootNamespace__.Data.Entities.__Category__;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace __RootNamespace__.Features.__Entity__Feature.Update__Entity__;

public class Update__Entity__Validator : AbstractValidator<Update__Entity__Command>
{
    private readonly __DbContext__ _context;

    public Update__Entity__Validator(__DbContext__ context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters")
            .MustAsync(BeUniqueName).WithMessage("Name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
    }

    private async Task<bool> BeUniqueName(Update__Entity__Command command, string name, CancellationToken cancellationToken)
    {
        return !await _context.Set<__Entity__>().AnyAsync(
            x => x.Name == name && x.Id != command.Id, cancellationToken);
    }
}
