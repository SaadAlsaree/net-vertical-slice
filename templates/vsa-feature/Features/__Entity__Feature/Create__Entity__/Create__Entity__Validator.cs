using FluentValidation;

namespace __RootNamespace__.Features.__Entity__Feature.Create__Entity__;

public class Create__Entity__Validator : AbstractValidator<Create__Entity__Command>
{
    // TODO(__Entity__): add validation rules and inject DbContext if uniqueness checks are needed.
    // Example (uncomment and adapt):
    //   private readonly __DbContext__ _context;
    //
    //   public Create__Entity__Validator(__DbContext__ context)
    //   {
    //       _context = context;
    //
    //       RuleFor(x => x.NameAr)
    //           .NotEmpty().WithMessage("NameAr is required")
    //           .MaximumLength(50).WithMessage("NameAr must be less than 50 characters")
    //           .MustAsync(BeUniqueNameAr).WithMessage("NameAr already exists");
    //   }
    //
    //   private async Task<bool> BeUniqueNameAr(string nameAr, CancellationToken cancellationToken)
    //       => !await _context.Set<__Entity__>().AnyAsync(x => x.NameAr == nameAr, cancellationToken);
    public Create__Entity__Validator() { }
}
