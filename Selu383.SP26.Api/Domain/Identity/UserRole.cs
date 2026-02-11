using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Domain.Identity;

public class UserRole : IdentityUserRole<int>
{
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
