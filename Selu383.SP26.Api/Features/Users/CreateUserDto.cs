namespace Selu383.SP26.Api.Features.Users;

public class CreateUserDto
{
    public string UserName { get; set; } = default!;
    public string[] Roles { get; set; } = default!;
    public string Password { get; set; } = default!;
}
