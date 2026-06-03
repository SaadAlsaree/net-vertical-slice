using VsaTemplate.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Features.SampleFeature.CreateSample;

public class CreateSampleValidator : AbstractValidator<CreateSampleCommand>
{
    private readonly AppDbContext _context;

    public CreateSampleValidator(AppDbContext context)
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
        return !await _context.Samples.AnyAsync(x => x.Name == name, cancellationToken);
    }
}
