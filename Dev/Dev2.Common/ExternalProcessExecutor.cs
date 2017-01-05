/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using System;
using System.Diagnostics;

namespace Dev2.Common
{
    public class ExternalProcessExecutor : IExternalProcessExecutor
    {
        #region Implementation of IExternalProcessExecutor

        public void OpenInBrowser(Uri url)
        {
            Process start = null;
            try
            {
                start = Process.Start(url.ToString());
            }
            catch (TimeoutException)
            {
                start?.Kill();
                start?.Dispose();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                start?.Kill();
                start?.Dispose();
            }
        }

        #endregion Implementation of IExternalProcessExecutor
    }
}