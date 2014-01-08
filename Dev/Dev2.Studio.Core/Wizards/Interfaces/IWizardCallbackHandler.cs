
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Wizards.Interfaces
{
    public interface IWizardCallbackHandler
    {
        /// <summary>
        /// Completes the callback.
        /// </summary>
        void CompleteCallback();
        /// <summary>
        /// Cancels the callback.
        /// </summary>
        void CancelCallback();
    }
}
