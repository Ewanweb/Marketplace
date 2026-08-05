using Microsoft.AspNetCore.Authorization;

namespace Marketplace.API.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "HasPermission_";

    public HasPermissionAttribute(string permission) : base(PolicyPrefix + permission)
    {
    }
}
