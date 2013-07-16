using System;
using Caliburn.Micro;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;

namespace Dev2.Studio.Diagnostics
{
    public class AppExceptionHandler : AppExceptionHandlerAbstract
    {
        readonly IEventAggregator _aggregator;
        readonly IApp _current;
        readonly IMainViewModel _mainViewModel;

        #region CTOR

        public AppExceptionHandler(IEventAggregator aggregator, IApp current, IMainViewModel mainViewModel)
        {
            if(aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }
            if(current == null)
            {
                throw new ArgumentNullException("current");
            }
            if(mainViewModel == null)
            {
                throw new ArgumentNullException("mainViewModel");
            }
            _aggregator = aggregator;
            _current = current;
            _mainViewModel = mainViewModel;
        }

        #endregion

        #region CreatePopupController

        protected override IAppExceptionPopupController CreatePopupController()
        {
            return new AppExceptionPopupController(_mainViewModel.ActiveEnvironment);
        }

        #endregion

        #region ShutdownApp

        protected override void ShutdownApp()
        {
            _aggregator.Publish(new SaveAllOpenTabsMessage());
            _current.ShouldRestart = false;
            _current.Shutdown();
        }

        #endregion

        #region RestartApp

        protected override void RestartApp()
        {
            _aggregator.Publish(new SaveAllOpenTabsMessage());
            _current.ShouldRestart = true;
            _current.Shutdown();
        }

        #endregion
    }
}
