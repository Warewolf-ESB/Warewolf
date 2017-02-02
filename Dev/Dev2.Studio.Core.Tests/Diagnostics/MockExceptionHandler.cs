/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Interfaces;
using Dev2.Studio;
using Dev2.Studio.Diagnostics;

namespace Dev2.Core.Tests.Diagnostics
{
    public class MockExceptionHandler : AppExceptionHandler
    {
        public MockExceptionHandler(IApp current, IMainViewModel mainViewModel)
            : base(current, mainViewModel)
        {
        }

        #region ShutdownApp

        public void TestShutdownApp()
        {
            ShutdownApp();
        }

        #endregion

        #region RestartApp

        public void TestRestartApp()
        {
            RestartApp();
        }

        #endregion
    }
}
