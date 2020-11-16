#pragma warning disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces
{
    public interface IManageServiceInputViewModel
    {

        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        Action OkAction { get; set; }
        Action CloseAction { get; set; }
        IDatabaseService Model { get; set; }
        string TestHeader { get; set; }
        ICollection<IServiceInput> Inputs { get; set; }
        DataTable TestResults { get; set; }
    }

    public interface IManageServiceInputViewModel<T>
    {
     
        Action TestAction { get; set; }
        ICommand TestCommand { get; }
        bool TestResultsAvailable { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        bool IsTesting { get; set; }
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
        Action OkAction { get; set; }
        Action CloseAction { get; set; }
        T Model { get; set; }
        string TestHeader { get; set; }
    }

    public interface IManageDatabaseInputViewModel : IToolRegion, IManageServiceInputViewModel<IDatabaseService>
    {
        ICollection<IServiceInput> Inputs { get; set; }
        DataTable TestResults { get; set; }
        bool OkSelected { get; set; }
        IGenerateOutputArea OutputArea { get; }
        IOutputDescription Description { get; set; }
        IGenerateInputArea InputArea { get; }
        bool OutputCountExpandAllowed { get; set; }
        bool InputCountExpandAllowed { get; set; }
        bool IsGenerateInputsEmptyRows { get; set; }
    }
	public interface IManageSqliteInputViewModel : IToolRegion, IManageServiceInputViewModel<ISqliteService>
	{
		ICollection<IServiceInput> Inputs { get; set; }
		IGenerateOutputArea OutputArea { get; }
		IOutputDescription Description { get; set; }
		IGenerateInputArea InputArea { get; }
		string SqlQuery { get; set; }
		bool OutputCountExpandAllowed { get; set; }
		bool InputCountExpandAllowed { get; set; }
		bool IsGenerateInputsEmptyRows { get; set; }
	}
}