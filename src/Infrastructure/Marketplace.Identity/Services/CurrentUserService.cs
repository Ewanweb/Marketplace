using System.Security.Claims;
using Marketplace.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Marketplace.Identity.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) 
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            return user.IsInRole("Admin") || 
                   user.IsInRole("SuperAdmin") || 
                   user.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "Admin" || c.Value == "SuperAdmin"));
        }
    }
}
