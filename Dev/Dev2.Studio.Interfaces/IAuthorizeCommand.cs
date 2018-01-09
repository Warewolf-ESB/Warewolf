using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Services.Security;

namespace Dev2.Studio.Interfaces
{
    public interface IAuthorizeCommand : ICommand
    {
        void UpdateContext(IServer environment);
        void UpdateContext(IServer environment, IContextualResourceModel resourceModel);
        AuthorizationContext AuthorizationContext { get; set; }
        IAuthorizationService AuthorizationService { get;}
    }

    public interface IAuthorizeCommand<T> : IAuthorizeCommand
    {
    }
}