using System;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Diagnostics
{
    interface IExceptionPopupController
    {
        void ShowPopup(Exception e, ErrorSeverity severity, IEnvironmentModel environment, AppExceptionHandler handler);
    }
}
