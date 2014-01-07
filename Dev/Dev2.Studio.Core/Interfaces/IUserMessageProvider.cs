
namespace Dev2.Studio.Core.Interfaces
{
    public interface IUserMessageProvider
    {
        void ShowUserMessage(string message, string title = "");
        void ShowUserErrorMessage(string message, string title = "");
        void ShowUserWarningMessage(string message, string title = "");
    }
}
