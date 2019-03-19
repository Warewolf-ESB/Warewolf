#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Search;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Search;
using Dev2.ViewModels;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Studio.Tests.ViewModels
{
    [TestClass]
    public class SearchModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SearchModel))]
        public void SearchModel_Constructor_Validate()
        {
            const string expectedDisplayName = "SearchViewDisplayName";
            var mockEventAggregator = new Mock<IEventAggregator>();
            var searchViewModel = new MockSearchViewModel
            {
                DisplayName = expectedDisplayName
            };
            var mockView = new Mock<IView>();
            var mockAuthorizeCommand = new Mock<IAuthorizeCommand>();

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.SaveCommand).Returns(mockAuthorizeCommand.Object);

            SearchModel searchModel = null;
            using (searchModel = new SearchModel(mockEventAggregator.Object, searchViewModel, mockView.Object, mockShellViewModel.Object))
            {
                searchModel.HelpText = "test";

                Assert.IsNotNull(searchModel.ViewModel);
                Assert.IsNotNull(searchModel.View);
                Assert.IsFalse(searchModel.HasVariables);
                Assert.IsFalse(searchModel.HasDebugOutput);
                Assert.IsFalse(searchModel.IsDirty);
                Assert.AreEqual(searchModel.View, searchModel.GetView());
                Assert.AreEqual(expectedDisplayName, searchModel.DisplayName);
                Assert.AreEqual("Search", searchModel.ResourceType);
                Assert.AreEqual("test", searchModel.HelpText);

                var doDeactivateCalled = searchModel.DoDeactivate(false);
                Assert.IsTrue(doDeactivateCalled);
                Assert.IsTrue(searchViewModel.HelpCalled);

                searchViewModel.TestPropertyChangedEvent("DisplayName");
                mockShellViewModel.Verify(shellViewModel => shellViewModel.SaveCommand, Times.Once);
            }
            mockEventAggregator.Verify(o => o.Unsubscribe(searchModel), Times.AtLeastOnce);
        }

        class MockSearchViewModel : ISearchViewModel
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public event SelectedExplorerItemChanged SelectedItemChanged;

            public event ServerState ServerStateChanged;

            public bool AllowDrag { get; set; }

            public ICommand ClearSearchTextCommand { get; set; }

            public IConnectControlViewModel ConnectControlViewModel { get; set; }

            public ICommand CreateFolderCommand { get; set; }
            public string DisplayName { get; set; }
            public ObservableCollection<IEnvironmentViewModel> Environments { get; set; }

            public string ExplorerClearSearchTooltip { get; set; }

            public bool HelpCalled { get; set; }
            public bool IsFromActivityDrop { get; set; }
            public bool IsRefreshing { get; set; }
            public ICommand OpenResourceCommand { get; set; }
            public ICommand RefreshCommand { get; set; }
            public Func<Task> RefreshEnvironmentAsyncImpl { get; set; }

            public Func<Guid, Task> RefreshEnvironmentImpl { get; set; }

            public string RefreshToolTip { get; set; }
            public ISearch Search { get; set; }
            public ICommand SearchInputCommand { get; set; }
            public ObservableCollection<ISearchResult> SearchResults { get; set; }

            public string SearchText { get; set; }

            public string SearchToolTip { get; set; }
            public object[] SelectedDataItems { get; set; }
            public IEnvironmentViewModel SelectedEnvironment { get; set; }
            public IExplorerTreeItem SelectedItem { get; set; }

            public IServer SelectedServer { get; set; }

            public Action<Guid> SelectItemImpl { get; set; }
            public bool ShowConnectControl { get; set; }

            public void Dispose() { }
            public void Filter(string filter) { }
            public Task RefreshEnvironment(Guid environmentId) { return RefreshEnvironmentImpl?.Invoke(environmentId); }
            public Task RefreshSelectedEnvironmentAsync() { return RefreshEnvironmentAsyncImpl?.Invoke(); }
            public void SelectItem(Guid id) { SelectItemImpl?.Invoke(id); }
            public void TestPropertyChangedEvent(string propertyName)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            public void UpdateHelpDescriptor(string helpText) { HelpCalled = true; }
        }
    }
}
