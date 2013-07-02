using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;

namespace Dev2.Studio.Diagnostics
{
    public class AppExceptionPopupController : IExceptionPopupController
    {
        public bool ViewModelInitializing = false;

        public virtual void ShowPopup(Exception e, ErrorSeverity severity, IEnvironmentModel environment, AppExceptionHandler handler)
        {
            bool isCritical = (severity == ErrorSeverity.Critical);

            ViewModelInitializing = true;
            // PBI 9598 - 2013.06.10 - TWR : added environmentModel parameter
            IExceptionViewModel vm = ExceptionFactory.CreateViewModel(e, environment, isCritical);
            ViewModelInitializing = false;

            if(vm != null)
            {
                vm.Show();
            }

            handler.ShutdownApp(isCritical);
        }
    }
}
