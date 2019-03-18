#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
