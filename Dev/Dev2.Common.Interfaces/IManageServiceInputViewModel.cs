using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces
{
    public interface IManageServiceInputViewModel<T>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        Action OkAction { get; set; }
        T Model { get; set; }
        void ShowView();
    }

    public interface IManageDatabaseInputViewModel : IManageServiceInputViewModel<IDatabaseService>
    {
        DataTable TestResults { get; set; }
        bool OkSelected { get; set; }
    }
}