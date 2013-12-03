
namespace Dev2.Runtime.WebServer.Security
{
    public interface IAuthorizationService
    {
        bool IsAuthorized(IAuthorizationRequest request);
    }
}