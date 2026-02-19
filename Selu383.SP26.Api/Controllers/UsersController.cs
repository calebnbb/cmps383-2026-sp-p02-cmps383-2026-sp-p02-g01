using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController(UserManager<User> userManager, RoleManager<Role> roleManager) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        if (dto.Roles == null || dto.Roles.Length == 0)
            return BadRequest();

        foreach (var role in dto.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                return BadRequest();
        }

        var existing = await userManager.FindByNameAsync(dto.UserName);
        if (existing != null)
            return BadRequest();

        var user = new User { UserName = dto.UserName };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest();

        await userManager.AddToRolesAsync(user, dto.Roles);
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName!, Roles = dto.Roles });
    }
}