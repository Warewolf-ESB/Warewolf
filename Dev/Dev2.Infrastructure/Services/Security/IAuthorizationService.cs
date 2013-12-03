
namespace Dev2.Services.Security
{
    public interface IAuthorizationService
    {
        bool IsAuthorized(AuthorizationContext context, string resource);
    }
}