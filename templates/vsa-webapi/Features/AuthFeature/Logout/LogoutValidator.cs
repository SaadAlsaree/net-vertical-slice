#if AUTH_IDENTITY
using FluentValidation;

namespace VsaTemplate.Features.AuthFeature.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
#endif
