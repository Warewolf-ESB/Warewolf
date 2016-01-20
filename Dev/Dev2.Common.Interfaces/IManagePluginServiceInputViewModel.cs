using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IManagePluginServiceInputViewModel
    {
        ICollection<IServiceInput> Inputs { get; set; }
        string TestResults { get; set; }
        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        IPluginService Model { get; set; }
        Action OkAction { get; set; }
        List<IServiceOutputMapping> OutputMappings { get; set; }
        IOutputDescription Description { get; set; }

        void ShowView();
    }
}