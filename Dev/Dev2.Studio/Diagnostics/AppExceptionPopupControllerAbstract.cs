using System;
using Dev2.Studio.ViewModels.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public abstract class AppExceptionPopupControllerAbstract : IAppExceptionPopupController
    {
        public void ShowPopup(Exception ex, ErrorSeverity severity)
        {
            var exceptionViewModel = CreateExceptionViewModel(ex, severity);
            if(exceptionViewModel != null)
            {
                exceptionViewModel.Show();
            }
        }

        protected abstract IExceptionViewModel CreateExceptionViewModel(Exception exception, ErrorSeverity severity);
    }
}
