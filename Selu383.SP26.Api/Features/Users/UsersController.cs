using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Authentication;

namespace Selu383.SP26.Api.Features.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public UsersController(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        if (dto.Roles is null || dto.Roles.Length == 0)
            return BadRequest();

        // Ensure roles exist
        foreach (var role in dto.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest();
        }

        // Unique username
        var existing = await _userManager.FindByNameAsync(dto.UserName);
        if (existing is not null)
            return BadRequest();

        var user = new User { UserName = dto.UserName };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return BadRequest();

        var addRolesResult = await _userManager.AddToRolesAsync(user, dto.Roles);
        if (!addRolesResult.Succeeded)
            return BadRequest();

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Roles = dto.Roles
        });
    }
}
