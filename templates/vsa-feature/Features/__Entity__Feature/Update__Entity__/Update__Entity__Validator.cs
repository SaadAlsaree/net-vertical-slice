using FluentValidation;

namespace __RootNamespace__.Features.__Entity__Feature.Update__Entity__;

public class Update__Entity__Validator : AbstractValidator<Update__Entity__Command>
{
    // TODO(__Entity__): add validation rules and inject DbContext if uniqueness checks are needed.
    // Example (uncomment and adapt):
    //   private readonly __DbContext__ _context;
    //
    //   public Update__Entity__Validator(__DbContext__ context)
    //   {
    //       _context = context;
    //
    //       RuleFor(x => x.NameAr)
    //           .NotEmpty().WithMessage("NameAr is required")
    //           .MaximumLength(50).WithMessage("NameAr must be less than 50 characters")
    //           .MustAsync(BeUniqueNameAr).WithMessage("NameAr already exists");
    //   }
    //
    //   private async Task<bool> BeUniqueNameAr(Update__Entity__Command command, string nameAr, CancellationToken cancellationToken)
    //       => !await _context.Set<__Entity__>().AnyAsync(x => x.NameAr == nameAr && x.Id != command.Id, cancellationToken);
    public Update__Entity__Validator() { }
}
