namespace Selu383.SP26.Api.Features.Authentication;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string[] Roles { get; set; } = default!;
}
