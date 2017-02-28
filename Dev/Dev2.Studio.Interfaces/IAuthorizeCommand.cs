using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Services.Security;

namespace Dev2.Studio.Interfaces
{
    public interface IAuthorizeCommand : ICommand
    {
        void UpdateContext(IEnvironmentModel environment, IContextualResourceModel resourceModel = null);
        AuthorizationContext AuthorizationContext { get; set; }
        IAuthorizationService AuthorizationService { get;}
    }

    public interface IAuthorizeCommand<T> : IAuthorizeCommand
    {
    }
}