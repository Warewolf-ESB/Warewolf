#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Wrappers;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Warewolf.OS;

namespace Dev2.Common
{
    public class ExternalProcessExecutor : IExternalProcessExecutor
    {
        private readonly IProcessFactory _processWrapper;

        public ExternalProcessExecutor(IProcessFactory processWrapper)
        {
            _processWrapper = processWrapper;
        }

        [ExcludeFromCodeCoverage]
        public ExternalProcessExecutor()
            :this(new ProcessWrapperFactory())
        {

        }

        public void OpenInBrowser(Uri url)
        {
            IProcess start = null;
            try
            {
                start = _processWrapper.Start(url.ToString()) ;
            }
            catch (TimeoutException)
            {
                //TODO: Remove this code as it is never used
                start?.Kill();
                start?.Dispose();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                //TODO: Remove this code as it is never used
                start?.Kill();
                start?.Dispose();
            }
        }

        public Process Start(ProcessStartInfo startInfo) => _processWrapper.Start(startInfo).Unwrap();
    }
}