using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class AppExceptionPopupController : AppExceptionPopupControllerAbstract
    {
        readonly IEnvironmentModel _environment;

        public AppExceptionPopupController(IEnvironmentModel environment)
        {
            _environment = environment;
        }

        protected override IExceptionViewModel CreateExceptionViewModel(Exception ex, ErrorSeverity severity)
        {
            return ExceptionFactory.CreateViewModel(ex, _environment, severity);
        }
    }
}
