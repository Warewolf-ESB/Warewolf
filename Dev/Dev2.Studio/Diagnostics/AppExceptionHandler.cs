
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.Studio.Diagnostics
// ReSharper restore CheckNamespace
{
    public class AppExceptionHandler : AppExceptionHandlerAbstract
    {
        readonly IApp _current;
        readonly IMainViewModel _mainViewModel;

        #region CTOR

        public AppExceptionHandler(IApp current, IMainViewModel mainViewModel)
        {
            if(current == null)
            {
                throw new ArgumentNullException("current");
            }
            if(mainViewModel == null)
            {
                throw new ArgumentNullException("mainViewModel");
            }
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
            _current.ShouldRestart = false;
            _current.Shutdown();
        }

        #endregion

        #region RestartApp

        protected override void RestartApp()
        {
            _current.ShouldRestart = true;
            _current.Shutdown();
        }

        #endregion
    }
}
