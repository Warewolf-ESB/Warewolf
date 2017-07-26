using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.DataList;
using Moq;
using System.Collections.ObjectModel;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.Database
{
    [Binding]
    class InitDatabaseToolsDataList
    {
        [BeforeScenario("@OpeningSavedWorkflowWithPostgresServerTool", "@ChangeTheSourceOnExistingPostgresql", "@ChangeTheActionOnExistingPostgresql", "@ChangeTheRecordsetOnExistingPostgresqlTool", "@ChangingSqlServerFunctions", "@CreatingOracleToolInstance", "@ChangingOracleActions")]
        public void InitChangingFunction()
        {
            var mock = new Mock<IDataListViewModel>();
            mock.Setup(model => model.ScalarCollection).Returns(new ObservableCollection<IScalarItemModel>());
            if (DataListSingleton.ActiveDataList == null)
            {
                DataListSingleton.SetDataList(mock.Object);
            }
        }
    }
}
