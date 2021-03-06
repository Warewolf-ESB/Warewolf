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

using System;
using System.Text;
using Dev2.Common;


namespace Dev2.Studio.Diagnostics
{
    public abstract class AppExceptionHandlerAbstract : IAppExceptionHandler
    {
        string _lastExceptionSignature;
        Exception _exception;
        bool _busy;

        #region Handle

        public bool Handle(Exception e)
        {
            if(e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (_busy || e.Message.ToLowerInvariant().Equals("The remote name could not be resolved: 'warewolf.io'".ToLowerInvariant()) || (e.Source!=null && (e.Source.Contains("InfragisticsWPF4.DragDrop") || e.Source.Contains("PresentationFramework"))))
            {
                return true;
            }
            try
            {
                Dev2Logger.Error("Unhandled Exception" ,e, "Warewolf Error");
                _exception = e;
                _busy = true;                
                var popupController = CreatePopupController();
                var exceptionString = ToErrorString(_exception);
                var lastExceptionSignature = _lastExceptionSignature;
                _lastExceptionSignature = exceptionString;

                // Exception is critical if it is the same as the last one
                if(lastExceptionSignature != null && exceptionString == lastExceptionSignature)
                {
                    popupController.ShowPopup(_exception, ErrorSeverity.Critical);
                    RestartApp();
                }
                else
                {
                    popupController.ShowPopup(_exception, ErrorSeverity.Default);
                }
                return true;
            }
            catch (Exception ex)
            {
                ShutdownApp();
                return false;
            }
            finally
            {
                _busy = false;
            }
        }

        #endregion

        #region ToErrorString

        protected string ToErrorString(Exception ex)
        {
            var errors = new StringBuilder();
            while(ex != null)
            {
                errors.Append(ex.GetType());
                errors.Append(ex.Message);
                ex = ex.InnerException;
            }
            return errors.ToString();
        }

        #endregion

        protected abstract IAppExceptionPopupController CreatePopupController();

        protected abstract void ShutdownApp();

        protected abstract void RestartApp();
    }
}
