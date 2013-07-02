using System;
using System.IO;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;

namespace Dev2.Studio.Diagnostics
{
    public class AppExceptionHandler : IExceptionHandler
    {
        private string _lastExceptionThrown;
        private readonly AppExceptionPopupController _popupController;
        readonly IApp _current;
        readonly IEnvironmentModel _environment;
        readonly EventAggregator _aggregator;

        #region Public Methods

        public AppExceptionHandler(AppExceptionPopupController popupController, IApp current, IEnvironmentModel environment, EventAggregator aggregator)
        {
            _lastExceptionThrown = null;
            _popupController = popupController;
            _current = current;
            if(_environment == null)
            {
                _environment = environment;
            }
            _aggregator = aggregator;
        }

        public bool Handle(Exception e)
        {
            try
            {
                if (!_popupController.ViewModelInitializing)
                {
                    // 2013.06.20 - TWR - added AutoConnectHelper by-pass because it's so annoying!
                    if (IsAutoConnectHelperError(e))
                    {
                        return true;
                    }

                    var exceptionString = GetErrorToString(e);

                    ErrorSeverity severity;
                    if (_lastExceptionThrown != null && exceptionString == _lastExceptionThrown)
                    {
                        severity = ErrorSeverity.Critical;
                    }
                    else
                    {
                        severity = ErrorSeverity.Default;
                    }
                    _lastExceptionThrown = exceptionString;
                    _popupController.ShowPopup(e, severity, _environment, this);//pass 'this' for when ShutdownApp is called
                }
                return true;
            }
            catch
            {
                _current.Shutdown();
                return false;
            }
        }

        public string GetErrorToString(Exception exception, StringBuilder builder = null)
        {
            if (builder == null)
            {
                builder = new StringBuilder();
            }
            if (exception != null)
            {
                builder.Append(exception.GetType());
                builder.Append(exception.Message);
                if (exception.InnerException != null)
                {
                    builder.Append(GetErrorToString(exception.InnerException));
                }
            }
            return builder.ToString();
        }

        public virtual void ShutdownApp(bool isCritical)
        {
            if (isCritical)
            {
                //Init shutdown with the possibility of a restart after
                _aggregator.Publish(new SaveAllOpenTabsMessage());
                _current.ShouldRestart = true;
                _current.Shutdown();
            }
        }
        
        #endregion

        #region Private Methods

        static bool IsAutoConnectHelperError(Exception e)
        {
            return e is NullReferenceException
                   && e.Source == "System.Activities.Presentation"
                   && e.StackTrace.Contains("AutoConnectHelper");
        }
        
        #endregion
    }
}
