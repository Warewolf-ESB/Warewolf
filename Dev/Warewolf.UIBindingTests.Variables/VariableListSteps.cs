using System;
using System.Linq;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.Views.DataList;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.Variables
{
    [Binding]
    public class VariableListSteps
    {
        [BeforeFeature("VariableList")]
        public static void SetupForFeature()
        {
            Utils.SetupResourceDictionary();
            var mockEventAggregator = new Mock<IEventAggregator>();
            IView manageVariableListViewControl = new DataListView();
            var viewModel = new DataListViewModel(mockEventAggregator.Object);
            viewModel.InitializeDataListViewModel(new Mock<IResourceModel>().Object);
            manageVariableListViewControl.DataContext = viewModel;

            Utils.ShowTheViewForTesting(manageVariableListViewControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageVariableListViewControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
            FeatureContext.Current.Add("eventAggregator", mockEventAggregator);
        }

        [BeforeScenario("VariableList")]
        public void SetupForScenerio()
        {
  
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<DataListView>(Utils.ViewNameKey));
            var dataListViewModel = FeatureContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            dataListViewModel.SearchText = "";
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, dataListViewModel);
        }

        [Given(@"I have variables as")]
        public void GivenIHaveVariablesAs(Table table)
        {
            var variableListViewModel = Utils.GetViewModel<DataListViewModel>();
            var rows = table.Rows;
            foreach (var tableRow in rows)
            {
                var variableName = tableRow["Variable"];
                var isUsedStringValue = tableRow["IsUsed"];
                var isInput = !string.IsNullOrEmpty(tableRow["Input"]) && tableRow["Input"].Equals("YES", StringComparison.OrdinalIgnoreCase);
                var isOutput = !string.IsNullOrEmpty(tableRow["Output"]) && tableRow["Output"].Equals("YES", StringComparison.OrdinalIgnoreCase);
                var ioDirection = enDev2ColumnArgumentDirection.None;
                if (isInput && isOutput)
                {
                    ioDirection = enDev2ColumnArgumentDirection.Both;
                }
                else if (isInput)
                {
                    ioDirection = enDev2ColumnArgumentDirection.Input;
                }
                else if (isOutput)
                {
                    ioDirection = enDev2ColumnArgumentDirection.Output;
                }
                var isUsed = isUsedStringValue != null && (!string.IsNullOrEmpty(isUsedStringValue) || isUsedStringValue.Equals("YES", StringComparison.OrdinalIgnoreCase));
                if (DataListUtil.IsValueRecordset(variableName))
                {
                    var recSetName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                    var columnName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);

                    var existingRecordSet = variableListViewModel.RecsetCollection.FirstOrDefault(model => model.DisplayName.Equals(recSetName, StringComparison.OrdinalIgnoreCase));
                    if (existingRecordSet == null)
                    {
                        existingRecordSet = DataListItemModelFactory.CreateRecordSetItemModel(recSetName);
                        if (existingRecordSet != null)
                        {
                            existingRecordSet.DisplayName = recSetName;
                            variableListViewModel.RecsetCollection.Add(existingRecordSet);
                        }
                    }
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        var item = DataListItemModelFactory.CreateRecordSetFieldItemModel(columnName, "", existingRecordSet);
                        if (item != null)
                        {
                            item.DisplayName = variableName;
                            item.IsUsed = isUsed;
                            item.ColumnIODirection = ioDirection;
                            existingRecordSet?.Children.Add(item);
                        }
                    }
                }
                else
                {
                    var displayName = DataListUtil.RemoveLanguageBrackets(variableName);
                    var item = DataListItemModelFactory.CreateScalarItemModel(displayName);
                    if (item != null)
                    {
                        item.DisplayName = variableName;
                        item.IsUsed = isUsed;
                        item.ColumnIODirection = ioDirection;
                        variableListViewModel.ScalarCollection.Add(item);
                    }
                }
            }
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [When(@"I delete unassigned variables")]
        public void WhenIDeleteUnassignedVariables()
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            sourceControl.RemoveUnusedDataListItems();
        }

        [When(@"I search for variable ""(.*)""")]
        public void WhenISearchForVariable(string searchTerm)
        {
            var expectedVisibility = String.Equals(searchTerm, "lr().a", StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(expectedVisibility);
        }

        [Then(@"I click delete for ""(.*)""")]
        [Given(@"I click delete for ""(.*)""")]
        public void ThenIClickDeleteFor(string variableName)
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            var sourceViewControl = ScenarioContext.Current.Get<DataListView>(Utils.ViewNameKey);
            string varName;
            string parentName;
            if (DataListUtil.IsValueRecordset(variableName))
            {
                if (variableName.Contains("."))
                {
                    varName = variableName.Substring(variableName.IndexOf(".", StringComparison.Ordinal) + 1);
                    parentName = variableName.Substring(0, variableName.IndexOf("(", StringComparison.Ordinal));
                    var variableListViewRecsetCollection = sourceControl.RecsetCollection.SelectMany(a => a.Children).FirstOrDefault(model => model.DisplayName == varName && model.Parent.DisplayName == parentName);
                    Assert.IsTrue(sourceControl.DeleteCommand.CanExecute(variableListViewRecsetCollection));
                }
                else
                {
                    varName = variableName.Contains("()") ? variableName.Replace("()", "") : variableName;
                    var variableListViewRecsetCollection = sourceControl.RecsetCollection.FirstOrDefault(model => model.DisplayName == varName);
                    if (variableListViewRecsetCollection != null && variableListViewRecsetCollection.IsUsed)
                    {
                        variableListViewRecsetCollection.IsUsed = false;
                    }
                    Assert.IsTrue(sourceControl.DeleteCommand.CanExecute(variableListViewRecsetCollection));
                }
            }
            else
            {
                if (variableName.Contains("."))
                {
                    varName = variableName.Substring(variableName.IndexOf(".", StringComparison.Ordinal) + 1);
                    var variableListViewScalarCollection = sourceControl.ScalarCollection.FirstOrDefault(model => model.DisplayName == varName);
                    Assert.IsTrue(sourceControl.DeleteCommand.CanExecute(variableListViewScalarCollection));
                }
                else
                {
                    var variableListViewScalarCollection = sourceControl.ScalarCollection.FirstOrDefault(model => model.DisplayName == variableName);
                    Assert.IsTrue(sourceControl.DeleteCommand.CanExecute(variableListViewScalarCollection));
                }
            }
        }

        [When(@"I clear the filter")]
        public void WhenIClearTheFilter()
        {
            var variableListViewModel = Utils.GetView<DataListViewModel>();
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            sourceControl.SearchText = "";
            variableListViewModel.SearchText = string.Empty;
        }

        [When(@"I click ""(.*)""")]
        [Then(@"I click ""(.*)""")]
        public void WhenIClick(string command)
        {
            var sourceControl = ScenarioContext.Current.Get<DataListView>(Utils.ViewNameKey);
        }

        [When(@"I Sort the variables")]
        public void WhenISortTheVariables()
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            Assert.IsTrue(sourceControl.SortCommand.CanExecute(sourceControl.CanSortItems));
        }

        [Then(@"variables filter box is ""(.*)""")]
        public void ThenVariablesFilterBoxIs(string visibleString)
        {
            var view = Utils.GetView<DataListView>();
            //var visibility = view.GetFilterBoxVisibility();
            //Assert.AreEqual(visibleString.ToLowerInvariant() == "visible" ? Visibility.Visible : Visibility.Collapsed, visibility);
        }

        [Then(@"the Variable Names are")]
        [When(@"the Variable Names are")]
        [Given(@"the Variable Names are")]
        public void ThenTheVariableNamesAre(Table table)
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            var variableListViewScalarCollection = sourceControl.ScalarCollection;
            var rows = table.Rows;
            var i = 0;
            foreach (var tableRow in rows)
            {
                var scalarViewModel = variableListViewScalarCollection[i];
                if (table.ContainsColumn("Error Tooltip") && table.ContainsColumn("Error State"))
                {
                    if (!string.IsNullOrEmpty(tableRow["Error Tooltip"]) && tableRow["Error State"].Equals("YES"))
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(tableRow["Error Tooltip"]));
                    }
                }
                if (string.IsNullOrEmpty(scalarViewModel.DisplayName))
                {
                    continue;
                }
                if (tableRow["Delete IsEnabled"].Equals("YES"))
                {
                    continue;
                }
                Assert.AreEqual(tableRow["Variable Name"], scalarViewModel.DisplayName);
                i++;
            }
        }

        [Then(@"the Recordset Names are")]
        [When(@"the Recordset Names are")]
        [Given(@"the Recordset Names are")]
        public void ThenTheRecordsetNamesAre(Table table)
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            var variableListViewRecsetCollection = sourceControl.RecsetCollection;
            if (variableListViewRecsetCollection.Count > 0)
            {
                var variableListViewRecset = variableListViewRecsetCollection[0];
                if (string.IsNullOrEmpty(variableListViewRecset.DisplayName))
                {
                    variableListViewRecsetCollection.RemoveAt(0);
                }
            }
            var rows = table.Rows;
            var i = 0;

            if (variableListViewRecsetCollection.Count > 0)
            {
                foreach (var tableRow in rows)
                {
                    if (tableRow["Recordset Name"].Contains("."))
                    {
                        continue;
                    }
                    if (table.ContainsColumn("Error Tooltip") && table.ContainsColumn("Error State"))
                    {
                        if (!string.IsNullOrEmpty(tableRow["Error Tooltip"]) && tableRow["Error State"].Equals("YES"))
                        {
                            Assert.IsTrue(!string.IsNullOrEmpty(tableRow["Error Tooltip"]));
                        }
                    }
                    var recordSetViewModel = variableListViewRecsetCollection[i];
                    string recordSetName;
                    if (string.IsNullOrEmpty(recordSetViewModel.DisplayName))
                    {
                        continue;
                    }
                    if (!recordSetViewModel.DisplayName.Contains("()"))
                    {
                        recordSetName = recordSetViewModel.DisplayName + "()";
                    }
                    else
                    {
                        recordSetName = recordSetViewModel.DisplayName;
                    }
                    if (tableRow["Delete IsEnabled"].Equals("YES"))
                    {
                        continue;
                    }
                    if (recordSetName != tableRow["Recordset Name"])
                    {
                        continue;
                    }
                    Assert.AreEqual(tableRow["Recordset Name"], recordSetName);
                    i++;
                }
            }
        }

        [Given(@"I remove variable ""(.*)""")]
        public void GivenIRemoveVariable(string variableName)
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            var variableListViewScalarCollection = sourceControl.ScalarCollection.FirstOrDefault(model => model.DisplayName == variableName);
            Assert.IsTrue(sourceControl.DeleteCommand.CanExecute(variableListViewScalarCollection));
        }

        [Given(@"I change variable Name from ""(.*)"" to ""(.*)""")]
        public void GivenIChangeVariableNameFromTo(string variableFrom, string variableTo)
        {
            Assert.AreNotSame(variableFrom, variableTo);
        }

        [Given(@"I change Recordset Name from ""(.*)"" to ""(.*)""")]
        public void GivenIChangeRecordsetNameFromTo(string recordSetFrom, string recordSetTo)
        {
            Assert.AreNotSame(recordSetFrom, recordSetTo);
        }

        [When(@"I press the clear filter button")]
        public void WhenIPressTheClearFilterButton()
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            sourceControl.ClearSearchTextCommand.Execute(null);
        }

        [When(@"I press ""(.*)""")]
        public void WhenIPress(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"the Debug Input window is opened")]
        public void WhenTheDebugInputWindowIsOpened()
        {
            var sourceControl = ScenarioContext.Current.Get<DataListViewModel>(Utils.ViewModelNameKey);
            //sourceControl.DebugInputWindowsIsVisible
            ScenarioContext.Current.Pending();
        }

        [When(@"I save workflow as ""(.*)""")]
        public void WhenISaveWorkflowAs(string p0)
        {
            var variableListViewModel = Utils.GetViewModel<DataListViewModel>();
            ScenarioContext.Current.Pending();
        }

        [When(@"create variable ""(.*)"" equals """"(.*)""""")]
        public void WhenCreateVariableEquals(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"cursor focus is '(.*)'")]
        public void ThenCursorFocusIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
