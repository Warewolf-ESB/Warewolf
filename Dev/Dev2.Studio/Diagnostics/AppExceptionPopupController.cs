
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
