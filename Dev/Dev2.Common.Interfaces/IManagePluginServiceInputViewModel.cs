/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebServices;
using Warewolf.Options;

namespace Dev2.Common.Interfaces
{
    public interface IManageComPluginServiceInputViewModel : IToolRegion, IManageServiceInputViewModel<IComPluginService>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        IGenerateOutputArea OutputArea { get; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; }
        bool PasteResponseVisible { get; set; }
        bool PasteResponseAvailable { get; }
        bool OutputCountExpandAllowed { get; set; }
        bool InputCountExpandAllowed { get; set; }
        bool IsGenerateInputsEmptyRows { get; set; }
    }

    public interface IManageWebServiceInputViewModel : IToolRegion, IManageServiceInputViewModel<IWebService>
    {
        string TestResults { get; set; }
        bool OkSelected { get; set; }
        ICommand PasteResponseCommand { get; }
        IGenerateOutputArea OutputArea { get; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; }
        bool PasteResponseVisible { get; set; }
        bool PasteResponseAvailable { get; }
        bool OutputCountExpandAllowed { get; set; }
        bool InputCountExpandAllowed { get; set; }
        bool IsGenerateInputsEmptyRows { get; set; }
    }

    public interface IManageWebInputViewModel : IManageWebServiceInputViewModel
    {
        bool IsFormDataChecked { get; set; }
        IOptionsWithNotifier ConditionExpressionOptions { get; set; }

        void LoadConditionExpressionOptions(IList<IOption> conditionExpressionOptions);
    }
}