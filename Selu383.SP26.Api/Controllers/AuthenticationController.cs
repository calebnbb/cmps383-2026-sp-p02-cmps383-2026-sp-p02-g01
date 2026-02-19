using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController(SignInManager<User> signInManager, UserManager<User> userManager) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            return BadRequest();

        var result = await signInManager.PasswordSignInAsync(user, dto.Password, false, false);
        if (!result.Succeeded)
            return BadRequest();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName!, Roles = roles.ToArray() });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto { Id = user.Id, UserName = user.UserName!, Roles = roles.ToArray() });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }
}