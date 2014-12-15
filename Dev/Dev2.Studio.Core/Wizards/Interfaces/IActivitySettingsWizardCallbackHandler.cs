
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
using System.Activities.Presentation.Model;
using Dev2.DataList.Contract;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Wizards.Interfaces
{
    public interface IActivitySettingsWizardCallbackHandler : IWizardCallbackHandler
    {
        //IDataListCompiler Compiler { get; set; }
        Func<IDataListCompiler> CreateCompiler { get; set; }
        ModelItem Activity { get; set; }
        Guid DatalistID { get; set; }
    }
}
