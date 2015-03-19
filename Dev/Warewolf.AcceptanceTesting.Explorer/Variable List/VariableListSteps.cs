using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels.VariableList;

namespace Warewolf.AcceptanceTesting.Explorer.Variable_List
{
    [Binding]
    public class VariableListSteps
    {
        [BeforeFeature("VariableList")]
        public static void SetupForFeature()
        {
            var bootStrapper = new UnityBootstrapperForDatabaseSourceConnectorTesting();
            bootStrapper.Run();
            var databaseSourceControlView = new Mock<IVariableListView>();
            var manageDatabaseSourceControlViewModel = new Mock<IVariableListViewModel>();
            databaseSourceControlView.Object.DataContext = manageDatabaseSourceControlViewModel;
            Utils.ShowTheViewForTesting(databaseSourceControlView.Object);
            FeatureContext.Current.Add(Utils.ViewNameKey, databaseSourceControlView.Object);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageDatabaseSourceControlViewModel.Object);
        }

        [BeforeScenario("VariableList")]
        public void SetupForScenerio()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<IVariableListView>(Utils.ViewNameKey));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<IVariableListViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I have variables as")]
        public void GivenIHaveVariablesAs(Table table)
        {
            var variableListViewModel = Utils.GetViewModel<IVariableListViewModel>();
            var rows = table.Rows;
            foreach(var tableRow in rows)
            {
                if (tableRow["Variable"].Contains(")."))
                {
                    variableListViewModel.AddRecordSet(new VariableListViewRecordSetViewModel("",new List<IVariableListViewColumnViewModel>(),variableListViewModel,variableListViewModel.RecordSets));
                }
            }
            var variableListView = Utils.GetView<IVariableListView>();
            variableListView.GetAllScalarVariables();
            variableListView.GetAllRecordSetVariables();
        }

        [When(@"I delete unassigned variables")]
        public void WhenIDeleteUnassignedVariables()
        {
            var view = Utils.GetView<IVariableListView>();
            view.DeleteUnusedVariables();
        }

        [When(@"I search for variable ""(.*)""")]
        public void WhenISearchForVariable(string searchTerm)
        {
            var view = Utils.GetView<IVariableListView>();
            view.Search(searchTerm);
        }

        [When(@"I clear the filter")]
        public void WhenIClearTheFilter()
        {
            var view = Utils.GetView<IVariableListView>();
            view.ClearFilter();
        }

        [When(@"I Sort the variables")]
        public void WhenISortTheVariables()
        {
            var view = Utils.GetView<IVariableListView>();
            view.Sort();
        }

        [Then(@"variables filter box is ""(.*)""")]
        public void ThenVariablesFilterBoxIs(string visibleString)
        {
            var view = Utils.GetView<IVariableListView>();
            var visibility = view.GetFilterBoxVisibility();
            Assert.AreEqual(visibleString.ToLowerInvariant() == "visible" ? Visibility.Visible : Visibility.Collapsed, visibility);
        }
      
        [Then(@"the Variable Names are")]
        public void ThenTheVariableNamesAre(Table table)
        {
            var variableListView = Utils.GetView<IVariableListView>();
            var variableListViewScalarViewModels = variableListView.GetAllScalarVariables();
            var rows = table.Rows;
            var i = 0;
            foreach (var tableRow in rows)
            {
                var scalarViewModel = variableListViewScalarViewModels[i];
                Assert.AreEqual(tableRow["Variable Name"],scalarViewModel.Name);
                i++;
            }
        }

        [Then(@"the Recordset Names are")]
        public void ThenTheRecordsetNamesAre(Table table)
        {
            var variableListView = Utils.GetView<IVariableListView>();
            var variablelistViewRecordSetViewModels = variableListView.GetAllRecordSetVariables();
            var rows = table.Rows;
            var i = 0;
            foreach (var tableRow in rows)
            {
                var recordSetViewModel = variablelistViewRecordSetViewModels[i];
                Assert.AreEqual(tableRow["Variable Name"], recordSetViewModel.Name);
                i++;
            }
        }
    }
}
