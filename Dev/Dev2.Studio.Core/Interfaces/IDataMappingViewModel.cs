/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IDataMappingViewModel
    {
        IWebActivity Activity { get; set; }
        bool IsInitialLoad { get; set; }
        ObservableCollection<IInputOutputViewModel> Outputs { get; }
        ObservableCollection<IInputOutputViewModel> Inputs { get; }
        string XmlOutput { get; set; }

        string GetInputString(IList<IInputOutputViewModel> inputData);
        string GetOutputString(IList<IInputOutputViewModel> outputData);
    }
}
