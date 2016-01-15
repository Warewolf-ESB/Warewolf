using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IManageServiceInputViewModel
    {
        ICollection<IServiceInput> Inputs { get; set; }
        DataTable TestResults { get; set; }
        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        IDatabaseService Model { get; set; }
        Action OkAction { get; set; }

        void ShowView();
    }
}