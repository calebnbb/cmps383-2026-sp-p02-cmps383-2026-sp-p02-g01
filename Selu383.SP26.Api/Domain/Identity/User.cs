using Microsoft.AspNetCore.Identity;
using Selu383.SP26.Api.Domain.Identity;

namespace Selu383.SP26.Api.Domain.Identity;

public class User : IdentityUser<int>
{
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}
