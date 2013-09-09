using System;

namespace Dev2.Studio.Diagnostics
{
    public interface IAppExceptionPopupController
    {
        void ShowPopup(Exception ex, ErrorSeverity severity);
    }
}
