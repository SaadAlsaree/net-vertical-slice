#if AUTH_IDENTITY
using Microsoft.AspNetCore.Identity;

namespace VsaTemplate.Data.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
}
#endif
