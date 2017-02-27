using System.Windows.Input;

namespace Dev2.Studio.Interfaces
{
    public interface IAuthorizeCommand : ICommand
    {
        void UpdateContext(IEnvironmentModel environment, IContextualResourceModel resourceModel = null);
    }

    public interface IAuthorizeCommand<T> : IAuthorizeCommand
    {
    }
}