using System.Security.Claims;

namespace Munchify.API;

/// <summary>Hằng số role dùng cho [Authorize(Roles = ...)] (khớp enum UserRole: ADMIN | STAFF).</summary>
public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Staff = "STAFF";
    public const string AdminOrStaff = Admin + "," + Staff;
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>Lấy UserId từ JWT claim (NameIdentifier / sub). Trả null nếu không hợp lệ.</summary>
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out var id) && id > 0 ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(AppRoles.Admin);
}
