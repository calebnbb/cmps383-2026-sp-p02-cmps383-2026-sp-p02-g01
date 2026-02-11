using Microsoft.AspNetCore.Identity;
using Selu383.SP26.Api.Domain.Identity;

namespace Selu383.SP26.Api.Domain.Identity;

public class Role : IdentityRole<int>
{
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}
