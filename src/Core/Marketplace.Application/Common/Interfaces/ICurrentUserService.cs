namespace Marketplace.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsSuperAdmin { get; }
}
