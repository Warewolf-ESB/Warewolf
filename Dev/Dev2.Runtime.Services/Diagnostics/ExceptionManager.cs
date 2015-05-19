
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
using Dev2.Common;

namespace Dev2.Runtime.Diagnostics
{
    public abstract class ExceptionManager
    {
        public bool HasErrors { get; private set; }
        public string Error { get; private set; }

        protected ExceptionManager()
        {
            ClearError();
        }

        protected void ClearError()
        {
            HasErrors = false;
            Error = string.Empty;
        }

        protected void RaiseError(Exception ex)
        {
            RaiseError(ex.Message);
            Dev2Logger.Log.Info(ex.Message + " Stacktrace : " + ex.Message);
        }

        protected void RaiseError(string error)
        {
            HasErrors = true;
            Error = error;

            Dev2Logger.Log.Info(error);
        }
    }
}
