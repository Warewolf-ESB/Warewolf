using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IManageServiceInputViewModel<T>
    {
     
        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ImageSource TestIconImageSource { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        Action OkAction { get; set; }
        Action CloseAction { get; set; }
        T Model { get; set; }
        string TestHeader { get; set; }

        void ShowView();
        void CloseView();
    }

    public interface IManageDatabaseInputViewModel : IManageServiceInputViewModel<IDatabaseService>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        DataTable TestResults { get; set; }
        bool OkSelected { get; set; }
    }
}