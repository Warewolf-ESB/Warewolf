using Caliburn.Micro;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Network;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    public static class Dev2MockFactory
    {
        private static Mock<IMainViewModel> _mockMainViewModel;
        private static Mock<IEnvironmentModel> _mockEnvironmentModel;
        private static Mock<IContextualResourceModel> _mockResourceModel;
        private static Mock<IStudioClientContext> _mockFrameworkDataChannel;
        private static Mock<IFilePersistenceProvider> _mockFilePersistenceProvider;



        /*
         * Travis.Frisinger : 04-04-2012
         * 
         * The stuff for ILayoutObjectViewModel is ugly, but given that I need to return from a callback???
         * It was the easiest method I would find to correctly result the LayoutGrid without taking another day to get it right
         * http://stackoverflow.com/questions/2494930/settings-variable-values-in-a-moq-callback-call
         * sites a method of returning inline, but it does not take paramers into the callback method and I could not get the ordering right
         * hence this not-so-nice workaround
         * 
         */
        // required for IsSelected Property of LayoutGridObjectViewModel
        private static bool[,] isSelected = new bool[,] { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } };
        private static ILayoutObjectViewModel _sillyMoqResult;
        private static ILayoutGridViewModel _model;


        public static void printIsSelected()
        {

            for (int i = 0; i < 4; i++)
            {
                for (int q = 0; q < 4; q++)
                {
                    Console.WriteLine(i + "," + q + " " + isSelected[i, q]);
                }
            }
        }

        public static ILayoutObjectViewModel FetchCallbackResult()
        {
            return _sillyMoqResult;
        }

        public static ILayoutObjectViewModel LayoutObjectFromMock(ILayoutGridViewModel grid, int row, int col)
        {
            _sillyMoqResult = LayoutObject(grid, row, col).Object;
            _model = grid;
            return _sillyMoqResult;
        }

        public static Mock<ILayoutObjectViewModel> LayoutObject(ILayoutGridViewModel grid, int row, int col)
        {
            Mock<ILayoutObjectViewModel> result = new Mock<ILayoutObjectViewModel>();

            result.SetupAllProperties();

            // Setup IsSelected Properties for LayoutGridObjectViewModel
            result.SetupGet(c => c.IsSelected).Returns(() => isSelected[row, col]);
            result.SetupSet(c => c.IsSelected = It.IsAny<bool>()).Callback<bool>(value => isSelected[row, col] = value);

            result.Setup(c => c.AddColumnLeftCommand);
            result.Setup(c => c.LayoutObjectGrid).Returns(grid);

            result.Setup(c => c.GridRow).Returns(row);
            result.Setup(c => c.GridColumn).Returns(col);
            result.Setup(c => c.LayoutObjectGrid).Returns(grid);
            result.Setup(c => c.CopyCommand);
            result.Setup(c => c.ClearAllCommand);

            return result;
        }

        static public Mock<IEnvironmentModel> EnvironmentModel
        {
            get
            {
                if (_mockEnvironmentModel == null)
                {
                    _mockEnvironmentModel = SetupEnvironmentModel();
                    return _mockEnvironmentModel;
                }
                else
                    return _mockEnvironmentModel;
            }
            set
            {
                if (_mockEnvironmentModel == null)
                {
                    _mockEnvironmentModel = value;
                }
            }
        }

        static public Mock<IMainViewModel> MainViewModel
        {
            get
            {
                if (_mockMainViewModel == null)
                {
                    _mockMainViewModel = SetupMainViewModel();
                    return _mockMainViewModel;
                }
                else
                {
                    return _mockMainViewModel;
                }
            }
            set
            {
                _mockMainViewModel = value;
            }
        }

        static public Mock<IFilePersistenceProvider> FilePersistenceProvider
        {
            get
            {
                if (_mockFilePersistenceProvider == null)
                {
                    _mockFilePersistenceProvider = SetupFilePersistenceProviderMock();
                    return _mockFilePersistenceProvider;
                }
                else
                {
                    return _mockFilePersistenceProvider;
                }
            }
            set
            {
                _mockFilePersistenceProvider = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModel
        {
            get
            {
                if (_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModelWithOnlyInputs
        {
            get
            {
                if (_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelWithOnlyInputsMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModelWithOnlyOutputs
        {
            get
            {
                if (_mockResourceModel == null)
                {
                    _mockResourceModel = SetupResourceModelWithOnlyOuputsMock();
                    return _mockResourceModel;
                }
                else
                {
                    return _mockResourceModel;
                }
            }
            set
            {
                _mockResourceModel = value;
            }
        }


        static public Mock<IMainViewModel> SetupMainViewModel()
        {
            // MainViewModel Setup
            // Dependancies on the EnvironmentRepository and the Data Channel
            _mockMainViewModel = new Mock<IMainViewModel>();
            //5559 Check if the removal of the below lines impacted any tests
            //_mockMainViewModel.Setup(mainVM => mainVM.EnvironmentRepository.Save(SetupEnvironmentModel().Object)).Verifiable();
            //_mockMainViewModel.Setup(mainVM => mainVM.DsfChannel).Returns(SetupIFrameworkDataChannel().Object);

            return _mockMainViewModel;

        }

        public static Mock<IStudioClientContext> SetupIFrameworkDataChannel_EmptyReturn()
        {
            Mock<IStudioClientContext> mockDataChannel = new Mock<IStudioClientContext>();
            mockDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand("", Guid.Empty, Guid.Empty)).Returns("");

            return mockDataChannel;
        }

        static public Mock<IStudioClientContext> SetupIFrameworkDataChannel()
        {
            _mockFrameworkDataChannel = new Mock<IStudioClientContext>();
            _mockFrameworkDataChannel.Setup(dataChannel => dataChannel.ExecuteCommand("<x>String</x>", Guid.Empty, Guid.Empty)).Returns("success");

            return _mockFrameworkDataChannel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel()
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Resources).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.WebServerAddress).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfAddress).Returns(new Uri(StringResources.Uri_WebServer));
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel).Returns(SetupIFrameworkDataChannel_EmptyReturn().Object);

            return _mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Resources).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, resourceRepositoryFakeBacker).Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel.ExecuteCommand("", Guid.Empty, Guid.Empty)).Returns("");
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.WebServerAddress).Returns(new Uri(StringResources.Uri_WebServer));

            return _mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var _mockEnvironmentModel = new Mock<IEnvironmentModel>();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.Resources).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, returnResources, resourceRepositoryFakeBacker).Object);
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel.ExecuteCommand("", Guid.Empty, Guid.Empty)).Returns("");
            _mockEnvironmentModel.Setup(environmentModel => environmentModel.WebServerAddress).Returns(new Uri(StringResources.Uri_WebServer));

            return _mockEnvironmentModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock()
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>());
            //mockResourceModel.Setup(resource => resource.Save().Verifiable();
            //mockResourceModel.Setup(resource => resource.ResourceType).Returns(enResourceType.Service);

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(returnResource.Object);
            mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(r => resourceRepositoryFakeBacker.Add(r));
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Returns(new List<IResourceModel>() { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>() { returnResource.Object });

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(returnResource.Object);
            mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(r => resourceRepositoryFakeBacker.Add(r));
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<string>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>())).Returns(new List<IResourceModel>() { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(returnResources);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelWithOnlyInputsMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataListInputOnly);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResourcesTest.xmlServiceDefinitionWithInputsOnly);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelWithOnlyOuputsMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataListOutputOnly);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResourcesTest.xmlServiceDefinitionWithOutputsOnly);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType, string resourceName, bool returnSelf = true)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");

            if (returnSelf)
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);
            }
            else
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel().Object);
            }

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType, string resourceName, Mock<IContextualResourceModel> findSingleReturn)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(findSingleReturn, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.ServiceDefinition).Returns(StringResources.xmlServiceDefinition);
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, resourceRepositoryFakeBacker).Object);

            return mockResourceModel;
        }

        static public Mock<IFilePersistenceProvider> SetupFilePersistenceProviderMock()
        {
            var mockFilePersistenceProvider = new Mock<IFilePersistenceProvider>();
            mockFilePersistenceProvider.Setup(filepro => filepro.Write(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath), "<xmlData/>")).Verifiable();
            mockFilePersistenceProvider.Setup(filepro => filepro.Read(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath))).Returns("<xmlData/>").Verifiable();
            mockFilePersistenceProvider.Setup(filepro => filepro.Delete(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, StringResources.DebugData_FilePath))).Verifiable();

            return mockFilePersistenceProvider;
        }

        public static Mock<IWebActivity> SetupWebActivityMock()
        {
            var _mockWebActivity = new Mock<IWebActivity>();

            _mockWebActivity.SetupAllProperties();
            _mockWebActivity.Setup(activity => activity.XMLConfiguration).Returns(StringResourcesTest.WebActivity_XmlConfig);
            _mockWebActivity.Object.SavedInputMapping = StringResourcesTest.WebActivity_SavedInputMapping;
            _mockWebActivity.Object.SavedOutputMapping = StringResourcesTest.WebActivity_SavedOutputMapping;
            _mockWebActivity.Object.LiveInputMapping = StringResourcesTest.WebActivity_LiveInputMapping;
            _mockWebActivity.Object.LiveOutputMapping = StringResourcesTest.WebActivity_LiveOutputMapping;
            _mockWebActivity.Object.ServiceName = "MyTestActivity";
            _mockWebActivity.Object.ResourceModel = SetupResourceModelMock().Object;
            return _mockWebActivity;
        }

        public static Mock<IDataListViewModel> SetupDataListViewModel()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            return mockDataListViewModel;
        }

        public static Mock<IDataListItemModel> SetupDataListItemModel()
        {
            var mockDataListItemViewModel = new Mock<IDataListItemModel>();
            mockDataListItemViewModel.Setup(itemVM => itemVM.Name).Returns("UnitTestDataListItem");
            ObservableCollection<IDataListItemModel> children = new ObservableCollection<IDataListItemModel>();
            return mockDataListItemViewModel;
        }

        public static Mock<IExecutionStatusCallbackDispatcher> SetupExecutionStatusCallbackDispatcher(bool addResult = true, bool removeResult = true)
        {
            Mock<IExecutionStatusCallbackDispatcher> mockExecutionStatusCallbackDispatcher = new Mock<IExecutionStatusCallbackDispatcher>();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Returns(addResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Returns(removeResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.RemoveRange(It.IsAny<IList<Guid>>())).Verifiable();

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();

            return mockExecutionStatusCallbackDispatcher;
        }

        public static Mock<INetworkMessageBroker> SetupNetworkMessageBroker<T>(bool sendThrowsException = false) where T : INetworkMessage, new()
        {
            Mock<INetworkMessageBroker> mockNetworkMessageBroker = new Mock<INetworkMessageBroker>();
            
            if (sendThrowsException)
            {
                mockNetworkMessageBroker.Setup(e => e.Send<T>(It.IsAny<T>(), It.IsAny<INetworkOperator>())).Throws(new Exception());
            }
            else
            {
                mockNetworkMessageBroker.Setup(e => e.Send<ExecutionStatusCallbackMessage>(It.IsAny<ExecutionStatusCallbackMessage>(), It.IsAny<INetworkOperator>())).Verifiable();
            }

            return mockNetworkMessageBroker;
        }

        public static Mock<IStudioNetworkChannelContext> SetupStudioNetworkChannelContext()
        {
            Mock<IStudioNetworkChannelContext> mockSetupServerNetworkChannelContext = new Mock<IStudioNetworkChannelContext>();
            mockSetupServerNetworkChannelContext.Setup(s => s.Account).Returns(Guid.Empty);
            mockSetupServerNetworkChannelContext.Setup(s => s.Server).Returns(Guid.Empty);

            return mockSetupServerNetworkChannelContext;
        }

        public static Mock<IStudioNetworkMessageAggregator> SetupStudioNetworkMessageAggregator()
        {
            Mock<IStudioNetworkMessageAggregator> mockStudioNetworkMessageAggregator = new Mock<IStudioNetworkMessageAggregator>();

            return mockStudioNetworkMessageAggregator;
        }

        public static Mock<IDataListItemModel> SetupDataListItemViewModel()
        {
            var mockDataListItemViewModel = new Mock<IDataListItemModel>();      
            mockDataListItemViewModel.Setup(itemVM => itemVM.Name).Returns("UnitTestDataListItem");
            ObservableCollection<IDataListItemModel> children = new ObservableCollection<IDataListItemModel>();
            return mockDataListItemViewModel;
        }

        public static Mock<IDev2Definition> SetupIDev2Definition(string name, string value, string recordSetName, string rawValue, string mapsTo, bool isRequired, bool isRecordSet, bool isEvaluated, string defaultValue)
        {
            Mock<IDev2Definition> _mockDev2Def = new Mock<IDev2Definition>();
            _mockDev2Def.Setup(devDef => devDef.Name).Returns(name);
            _mockDev2Def.Setup(devDef => devDef.Value).Returns(value);
            _mockDev2Def.Setup(devDef => devDef.RecordSetName).Returns(recordSetName);
            _mockDev2Def.Setup(devDef => devDef.RawValue).Returns(rawValue);
            _mockDev2Def.Setup(devDef => devDef.MapsTo).Returns(mapsTo);
            _mockDev2Def.Setup(devDef => devDef.IsRequired).Returns(isRequired);
            _mockDev2Def.Setup(devDef => devDef.IsRecordSet).Returns(isRecordSet);
            _mockDev2Def.Setup(devDef => devDef.IsEvaluated).Returns(isEvaluated);
            _mockDev2Def.Setup(devDef => devDef.DefaultValue).Returns(defaultValue);
            return _mockDev2Def;
        }

        public static Mock<IInputOutputViewModel> SetupIInputOutputViewModel(string name, string value, string recordSetName, bool isSelected, string mapsTo, bool isRequired, string displayName, string defaultValue)
        {
            Mock<IInputOutputViewModel> _mockInOut = new Mock<IInputOutputViewModel>();
            _mockInOut.SetupAllProperties();
            _mockInOut.Setup(devDef => devDef.Name).Returns(name);
            _mockInOut.Setup(devDef => devDef.Value).Returns(value);
            _mockInOut.Setup(devDef => devDef.RecordSetName).Returns(recordSetName);
            _mockInOut.Setup(devDef => devDef.IsSelected).Returns(isSelected);
            _mockInOut.Setup(devDef => devDef.MapsTo).Returns(mapsTo);
            _mockInOut.Setup(devDef => devDef.Required).Returns(isRequired);
            _mockInOut.Setup(devDef => devDef.DisplayName).Returns(displayName);
            _mockInOut.Setup(devDef => devDef.DefaultValue).Returns(defaultValue);
            ObservableCollection<IDataListItemModel> dataList = new ObservableCollection<IDataListItemModel>();
            dataList.Add(SetupDataListItemViewModel().Object);
            //_mockInOut.Setup(inOut => inOut.DataList).Returns(dataList);

            return _mockInOut;
        }

        public static Mock<IDataMappingViewModel> SetupIDataMappingViewModel()
        {
            Mock<IDataMappingViewModel> _mockDataMappingViewModel = new Mock<IDataMappingViewModel>();

            Mock<IMainViewModel> _mockMainViewModel = new Mock<IMainViewModel>();
            //Mock<IDataMappingListFactory> _mockDataMappingListFactory = new Mock<IDataMappingListFactory>();
            //Mock<IDataListViewModelFactory> _mockDataListViewModelFactory = new Mock<IDataListViewModelFactory>();
            Mock<IWebActivity> _mockWebActivity = SetupWebActivityMock();
            Mock<IContextualResourceModel> _mockresource = SetupResourceModelMock();
            Mock<IDataListViewModel> _mockDataListViewModel = new Mock<IDataListViewModel>();
            Mock<IDataListFactory> _mockDataListFactory = new Mock<IDataListFactory>();
            Mock<IDataListItemModel> _mockDataListItemViewModel = new Mock<IDataListItemModel>();
            IList<IDev2Definition> inputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefIn1 = SetupIDev2Definition("reg", "", "", "", "", true, false, true, "NUD2347");
            Mock<IDev2Definition> _mockDev2DefIn2 = SetupIDev2Definition("asdfsad", "registration223", "", "", "registration223", true, false, true, "w3rt24324");
            Mock<IDev2Definition> _mockDev2DefIn3 = SetupIDev2Definition("number", "", "", "", "", false, false, true, "");

            inputDev2defList.Add(_mockDev2DefIn1.Object);
            inputDev2defList.Add(_mockDev2DefIn2.Object);
            inputDev2defList.Add(_mockDev2DefIn3.Object);

            IList<IDev2Definition> outputDev2defList = new List<IDev2Definition>();

            Mock<IDev2Definition> _mockDev2DefOut1 = SetupIDev2Definition("vehicleVin", "", "", "", "VIN", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut2 = SetupIDev2Definition("vehicleColor", "", "", "", "vehicleColor", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut3 = SetupIDev2Definition("Fines", "", "", "", "", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut4 = SetupIDev2Definition("speed", "", "Fines", "", "speed", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut5 = SetupIDev2Definition("date", "Fines.Date", "", "", "date", false, false, true, "");
            Mock<IDev2Definition> _mockDev2DefOut6 = SetupIDev2Definition("location", "", "Fines", "", "location", false, false, true, "");

            outputDev2defList.Add(_mockDev2DefOut1.Object);
            outputDev2defList.Add(_mockDev2DefOut2.Object);
            outputDev2defList.Add(_mockDev2DefOut3.Object);
            outputDev2defList.Add(_mockDev2DefOut4.Object);
            outputDev2defList.Add(_mockDev2DefOut5.Object);
            outputDev2defList.Add(_mockDev2DefOut6.Object);

            //_mockDataMappingListFactory.Setup(dmlf => dmlf.CreateListOutputMapping(It.IsAny<String>())).Returns(outputDev2defList);
            //_mockDataMappingListFactory.Setup(dmlf => dmlf.CreateListInputMapping(It.IsAny<String>())).Returns(inputDev2defList);

            IList<IInputOutputViewModel> inputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm1 = SetupIInputOutputViewModel("reg", "", "", false, "", true, "reg", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm2 = SetupIInputOutputViewModel("asdfsad", "registration223", "", false, "registration223", true, "asdfsad", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm3 = SetupIInputOutputViewModel("number", "", "", false, "", false, "number", "");
            inputInOutList.Add(_mockInOutVm1.Object);
            inputInOutList.Add(_mockInOutVm2.Object);
            inputInOutList.Add(_mockInOutVm3.Object);

            IList<IInputOutputViewModel> outputInOutList = new List<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> _mockInOutVm4 = SetupIInputOutputViewModel("vehicleVin", "", "", false, "VIN", false, "vehicleVin", "");
            Mock<IInputOutputViewModel> _mockInOutVm5 = SetupIInputOutputViewModel("vehicleColor", "", "", false, "", true, "vehicleColor", "");
            Mock<IInputOutputViewModel> _mockInOutVm6 = SetupIInputOutputViewModel("Fines", "", "", false, "", false, "Fines", "");
            Mock<IInputOutputViewModel> _mockInOutVm7 = SetupIInputOutputViewModel("speed", "", "Fines", false, "", false, "speed", "NUD2347");
            Mock<IInputOutputViewModel> _mockInOutVm8 = SetupIInputOutputViewModel("date", "Fines.Date", "Fines", false, "registration223", true, "date", "w3rt24324");
            Mock<IInputOutputViewModel> _mockInOutVm9 = SetupIInputOutputViewModel("location", "", "Fines", false, "", false, "location", "");
            outputInOutList.Add(_mockInOutVm4.Object);
            outputInOutList.Add(_mockInOutVm5.Object);
            outputInOutList.Add(_mockInOutVm6.Object);
            outputInOutList.Add(_mockInOutVm7.Object);
            outputInOutList.Add(_mockInOutVm8.Object);
            outputInOutList.Add(_mockInOutVm9.Object);

            //_mockDataMappingListFactory.Setup(dmlf => dmlf.CreateListToDisplayOutputs(outputDev2defList)).Returns(outputInOutList);
            //_mockDataMappingListFactory.Setup(dmlf => dmlf.CreateListToDisplayInputs(inputDev2defList)).Returns(inputInOutList);
            //_mockDataMappingListFactory.Setup(dlf => dlf.GenerateMapping(It.IsAny<IList<IDev2Definition>>(), enDev2ArgumentType.Output)).Returns("PassTestOut");
            //_mockDataMappingListFactory.Setup(dlf => dlf.GenerateMapping(It.IsAny<IList<IDev2Definition>>(), enDev2ArgumentType.Input)).Returns("PassTestIn");

            _mockDataListFactory.Setup(dlf => dlf.GenerateMappingFromWebpage(StringResourcesTest.WebActivity_XmlConfig, enDev2ArgumentType.Output)).Returns("FIX THIS");
            //_mockDataListFactory.Setup(dlf => dlf.GenerateMappingFromDataList(_mockDataListViewModel.Object.RootDataListItem.ToDataListXml(), enDev2ArgumentType.Input)).Returns("FIX THIS");
            //_mockDataListFactory.Setup(dlf => dlf.GenerateMappingFromDataList(_mockDataListViewModel.Object.RootDataListItem.ToDataListXml(), enDev2ArgumentType.Output)).Returns("FIX THIS");

            //_mockDataListItemViewModel.Setup(dlivm => dlivm.ToDataListXml()).Returns("FIX THIS");

            _mockDataListViewModel.Setup(dlvm => dlvm.Resource).Returns(_mockresource.Object);
            //_mockDataListViewModel.Setup(dlvm => dlvm.RootDataListItem).Returns(_mockDataListItemViewModel.Object);

            //_mockDataListViewModelFactory.Setup(dlvmf => dlvmf.CreateDataListViewModel(_mockresource.Object)).Returns(_mockDataListViewModel.Object);

            _mockWebActivity.Setup(activity => activity.UnderlyingWebActivityObjectType).Returns(typeof(DsfWebPageActivity));
            _mockDataMappingViewModel.Setup(dmvm => dmvm.Activity).Returns(_mockWebActivity.Object);
            _mockDataMappingViewModel.Setup(dmvm => dmvm.MainViewModel).Returns(_mockMainViewModel.Object);
            //_mockDataMappingViewModel.Setup(dmvm => dmvm.DataListFactory).Returns(_mockDataListFactory.Object);
            //_mockDataMappingViewModel.Setup(dmvm => dmvm.DataMappingListfactory).Returns(_mockDataMappingListFactory.Object);
            //_mockDataMappingViewModel.Setup(dmvm => dmvm.DataListViewModelFactory).Returns(_mockDataListViewModelFactory.Object);
            _mockDataMappingViewModel.Setup(dmvm => dmvm.Inputs).Returns(inputInOutList.ToObservableCollection());
            _mockDataMappingViewModel.Setup(dmvm => dmvm.Outputs).Returns(outputInOutList.ToObservableCollection());



            return _mockDataMappingViewModel;
        }


        public static Mock<IDev2ConfigurationProvider> CreateConfigurationProvider(string[] key, string[] val)
        {
            Mock<IDev2ConfigurationProvider> result = new Mock<IDev2ConfigurationProvider>();
            IDictionary d = new Dictionary<string, string>();
            int pos = 0;

            foreach (string k in key)
            {
                d.Add(k, val[pos]);
            }

            // result.SetupSet(c => c.IsSelected = It.IsAny<bool>()).Callback<bool>(value => isSelected[row,col] = value);
            result.Setup(c => c.ReadKey(It.IsAny<string>())).Returns<string>((string kk) => (string)d[kk]);

            return result;
        }

        public static Mock<IPopUp> CreateIPopup(MessageBoxResult returningResult)
        {
            Mock<IPopUp> result = new Mock<IPopUp>();
            result.Setup(moq => moq.Show()).Returns(returningResult).Verifiable();

            return result;
        }

        public static Mock<IEventAggregator> SetupMockEventAggregator()
        {
            Mock<IEventAggregator> mock = new Mock<IEventAggregator>();
            mock.Setup(m => m.Publish(It.IsAny<IMessage>())).Verifiable();
            return mock;
        }
    }
}
