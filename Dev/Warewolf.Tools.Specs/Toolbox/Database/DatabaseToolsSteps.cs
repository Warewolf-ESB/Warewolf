using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TechTalk.SpecFlow;
using Dev2.Common.Interfaces.DB;

namespace Warewolf.Tools.Specs.Toolbox.Database
{
    [Binding]
    class DatabaseToolsSteps
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

        public static void AssertAgainstServiceInputs(Table table, ICollection<IServiceInput> inputs)
        {
            var rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue, serviceInput.Name);
                Assert.AreEqual(value, serviceInput.Value);
                rowNum++;
            }
        }
    }
}
