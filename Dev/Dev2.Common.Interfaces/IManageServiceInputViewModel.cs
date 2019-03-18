#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;
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