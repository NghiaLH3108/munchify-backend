using Munchify.DAL.DTOs.Users;

namespace Munchify.DAL.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
