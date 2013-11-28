
namespace Dev2.Runtime.WebServer.Security
{
    public interface IAuthorizationProvider
    {
        bool IsAuthorized(IAuthorizationRequest request);
    }
}