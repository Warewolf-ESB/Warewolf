/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;


namespace Dev2.Studio.Diagnostics
{
    public class AppExceptionPopupController : AppExceptionPopupControllerAbstract
    {
        readonly IServer _environment;

        public AppExceptionPopupController(IServer environment)
        {
            _environment = environment;
        }

        protected override Task<IExceptionViewModel> CreateExceptionViewModel(Exception ex, ErrorSeverity severity)
        {
            return ExceptionFactory.CreateViewModel(ex, _environment, severity);
        }
    }
}
