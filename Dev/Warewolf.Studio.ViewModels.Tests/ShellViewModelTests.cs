using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.ErrorHandling;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Util;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Models.Help;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ShellViewModelTests
    {
        [TestInitialize]
        public void Init()
        {
            AppSettings.LocalHost = "http://myserver";
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]

        // ReSharper disable InconsistentNaming
        public void ShellViewModel_Constructor_NullContainer_NullExceptionThrown()

        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ShellViewModel(null, new Mock<IRegionManager>().Object,new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShellViewModel_Constructor_NullRegionManager_NullExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ShellViewModel(new Mock<IUnityContainer>().Object, null,new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Initialize")]
        public void ShellViewModel_Initialize_Should_AddViewsToRegions()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            testContainer.RegisterInstance<IExceptionHandler>(new WarewolfExceptionHandler(new Dictionary<Type, Action>()));
            testContainer.RegisterInstance<IPopupController>(new Mock<IPopupController>().Object);
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Explorer",new Region());
            testRegionManager.Regions.Add("Toolbox",new Region());
            testRegionManager.Regions.Add("Menu",new Region());
            testRegionManager.Regions.Add("Variables", new Region());
            SetupContainer(testContainer);
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            //------------Execute Test---------------------------
            shellViewModel.Initialize();
            //------------Assert Results-------------------------
            Assert.IsNotNull(shellViewModel);
            Assert.IsTrue(shellViewModel.RegionHasView("Explorer"));
            Assert.IsTrue(shellViewModel.RegionHasView("Toolbox"));
            Assert.IsTrue(shellViewModel.RegionHasView("Menu"));
        }

        static void SetupContainer(UnityContainer testContainer)
        {
            testContainer.RegisterInstance<IExplorerView>(new Mock<IExplorerView>().Object);
            testContainer.RegisterInstance<IToolboxView>(new Mock<IToolboxView>().Object);
            testContainer.RegisterInstance<IMenuView>(new Mock<IMenuView>().Object);
            testContainer.RegisterInstance<IVariableListView>(new Mock<IVariableListView>().Object);

            testContainer.RegisterInstance<IExplorerViewModel>(new Mock<IExplorerViewModel>().Object);
            testContainer.RegisterInstance<IToolboxViewModel>(new Mock<IToolboxViewModel>().Object);
            testContainer.RegisterInstance<IMenuViewModel>(new Mock<IMenuViewModel>().Object);
            testContainer.RegisterInstance<Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel>(new Mock<Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel>().Object);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Initialize")]
        public void ShellViewModel_Initialize_Should_SetViewModelAsDataContextForRegionViews()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Explorer",new Region());
            testRegionManager.Regions.Add("Toolbox",new Region());
            testRegionManager.Regions.Add("Menu",new Region());
            testRegionManager.Regions.Add("Variables", new Region());
            testContainer.RegisterInstance<IExplorerViewModel>(new Mock<IExplorerViewModel>().Object);
            testContainer.RegisterInstance<IToolboxViewModel>(new Mock<IToolboxViewModel>().Object);
            testContainer.RegisterInstance<IMenuViewModel>(new Mock<IMenuViewModel>().Object);
            testContainer.RegisterInstance<Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel>(new Mock<Dev2.Common.Interfaces.DataList.DatalistView.IVariableListViewModel>().Object);
            testContainer.RegisterInstance<IExceptionHandler>(new WarewolfExceptionHandler(new Dictionary<Type, Action>()));
            testContainer.RegisterInstance<IPopupController>(new Mock<IPopupController>().Object);
            var mockExplorerView = new Mock<IExplorerView>();
            mockExplorerView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IExplorerView>(mockExplorerView.Object);
            var mockToolBoxView = new Mock<IToolboxView>();
            mockToolBoxView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IToolboxView>(mockToolBoxView.Object);
            var mockMenuView = new Mock<IMenuView>();
            mockMenuView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IMenuView>(mockMenuView.Object);
            var mockVariableListView = new Mock<IVariableListView>();
            mockMenuView.SetupProperty(view => view.DataContext);
            testContainer.RegisterInstance<IVariableListView>(mockVariableListView.Object);
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            //------------Execute Test---------------------------
            shellViewModel.Initialize();
            //------------Assert Results-------------------------
            Assert.IsNotNull(shellViewModel);
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Explorer"));
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Toolbox"));
            Assert.IsTrue(shellViewModel.RegionViewHasDataContext("Menu"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_AddService")]
        public void ShellViewModel_AddService_WhenNotInTab_ShouldAddServiceViewModelToTab()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServiceDesignerViewModel, MockServiceDesignerViewModel>();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            //------------Execute Test---------------------------
            shellViewModel.AddService(new Mock<IResource>().Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shellViewModel.RegionHasView("Workspace"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_AddService")]
        public void ShellViewModel_AddService_SameResource_ShouldNotAddAnotherTab()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServiceDesignerViewModel, MockServiceDesignerViewModel>();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            var resource = new Mock<IResource>().Object;
            shellViewModel.AddService(resource);
            //------------Execute Test---------------------------
            shellViewModel.AddService(resource);
            //------------Assert Results-------------------------
            var viewsCollection = shellViewModel.GetRegionViews("Workspace");
            Assert.AreEqual(1,viewsCollection.Count());
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_AddService")]
        public void ShellViewModel_AddService_DifferentResource_ShouldAddAnotherTab()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServiceDesignerViewModel, MockServiceDesignerViewModel>();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            shellViewModel.AddService(new Mock<IResource>().Object);
            //------------Execute Test---------------------------
            shellViewModel.AddService(new Mock<IResource>().Object);
            //------------Assert Results-------------------------
            var viewsCollection = shellViewModel.GetRegionViews("Workspace");
            Assert.AreEqual(2,viewsCollection.Count());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_AddService")]
        public void ShellViewModel_AddService_WorkflowResource_ShouldAddIWorkflowServiceDesignerViewModel()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            testContainer.RegisterType<IServiceDesignerViewModel, MockServiceDesignerViewModel>();
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceType).Returns(ResourceType.WorkflowService);
            //------------Execute Test---------------------------
            shellViewModel.AddService(mockResource.Object);
            //------------Assert Results-------------------------
            var viewsCollection = shellViewModel.GetRegionViews("Workspace");
            Assert.AreEqual(1,viewsCollection.Count());
            var workflowView = viewsCollection.FirstOrDefault();
            Assert.IsNotNull(workflowView);
            Assert.IsInstanceOfType(workflowView,typeof(IWorkflowServiceDesignerViewModel));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ShellViewModel_DeployItem")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShellViewModel_DeployItem_NullItem_ExpectError()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            testContainer.RegisterType<IServiceDesignerViewModel, MockServiceDesignerViewModel>();
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);
            
            //------------Execute Test---------------------------
            shellViewModel.DeployService(null);
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ShellViewModel_DeployItem")]
        public void ShellViewModel_DeployItem_ValidItem_DeployNotOpened_ExpectSet()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var dep = new Mock<IDeployViewModel>();
            testContainer.RegisterInstance<IDeployViewModel>(dep.Object);
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            var region = new SingleActiveRegion();
            region.Add(dep.Object,"Deploy");
            testRegionManager.Regions.Add("Workspace",region );
            

            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            var shell = new ExplorerItemViewModel(shellViewModel, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null);
            shellViewModel.DeployService(shell);
            //------------Assert Results-------------------------
            dep.Verify(a=>a.SelectSourceItem(shell));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ShellViewModel_DeployItem")]
        public void ShellViewModel_DeployItem_ValidItem_DeployOpened_ExpectSet()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var dep = new Mock<IDeployViewModel>();
            testContainer.RegisterInstance<IDeployViewModel>(dep.Object);
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager,new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            var shell = new ExplorerItemViewModel(shellViewModel, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null);
            shellViewModel.DeployService(shell);
            //------------Assert Results-------------------------
            dep.Verify(a => a.SelectSourceItem(shell));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ShellViewModel_UpdateHelpDescriptor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShellViewModel_UpdateHelpDescriptor_NullExpectException()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var dep = new Mock<IDeployViewModel>();
            testContainer.RegisterInstance<IDeployViewModel>(dep.Object);
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager, new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            new ExplorerItemViewModel(shellViewModel, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null);
            shellViewModel.UpdateHelpDescriptor(null);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ShellViewModel_UpdateHelpDescriptor")]
        public void ShellViewModel_UpdateHelpDescriptor_Valid_FiresEventAggregator()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var dep = new Mock<IDeployViewModel>();
            testContainer.RegisterInstance<IDeployViewModel>(dep.Object);
            testContainer.RegisterType<IWorkflowServiceDesignerViewModel, MockWorkflowServiceDesignerViewModel>();
            var testRegionManager = new RegionManager();
            testRegionManager.Regions.Add("Workspace", new SingleActiveRegion());
            var aggregator =new Mock<IEventAggregator>();
            aggregator.Setup(a=>a.GetEvent<HelpChangedEvent>()).Returns(new HelpChangedEvent());
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager, aggregator.Object);

            //------------Execute Test---------------------------
            new ExplorerItemViewModel(shellViewModel, new Mock<IServer>().Object, new Mock<IExplorerHelpDescriptorBuilder>().Object, null);
            shellViewModel.UpdateHelpDescriptor(new HelpDescriptor("bob","the",new DrawingImage()));
            aggregator.Verify(a=>a.GetEvent<HelpChangedEvent>());

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        public void ShellViewModel_Constructor_ShouldSetLocalHostServerToActiveServer()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            var aggregator = new Mock<IEventAggregator>();
            //------------Execute Test---------------------------
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager, aggregator.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(shellViewModel.ActiveServer);
            Assert.AreEqual(shellViewModel.LocalhostServer,shellViewModel.ActiveServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        public void ShellViewModel_ActiveServer_Set_ShouldFireActiveServerChangedEvent()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            var aggregator = new Mock<IEventAggregator>();
            var anotherMockServer = new Mock<IServer>();
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager, aggregator.Object);
            var activeServerChanged = false;
            shellViewModel.ActiveServerChanged += () => activeServerChanged = true; 
            //------------Execute Test---------------------------
            shellViewModel.ActiveServer = anotherMockServer.Object;
            //------------Assert Results-------------------------
            Assert.IsTrue(activeServerChanged);
            Assert.IsNotNull(shellViewModel.ActiveServer);
            Assert.AreNotEqual(shellViewModel.LocalhostServer,shellViewModel.ActiveServer);
            Assert.AreEqual(anotherMockServer.Object,shellViewModel.ActiveServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShellViewModel_Constructor")]
        public void ShellViewModel_ActiveServer_SetToSameServer_ShouldNotFireActiveServerChangedEvent()
        {
            //------------Setup for test--------------------------
            var testContainer = new UnityContainer();
            testContainer.RegisterType<IServer, MockServer>();
            var testRegionManager = new RegionManager();
            var aggregator = new Mock<IEventAggregator>();
            var anotherMockServer = new Mock<IServer>();
            anotherMockServer.Setup(server => server.Equals(It.IsAny<IResource>())).Returns(true);
            var shellViewModel = new ShellViewModel(testContainer, testRegionManager, aggregator.Object);
            var activeServerChanged = false;
            shellViewModel.ActiveServer = anotherMockServer.Object;
            shellViewModel.ActiveServerChanged += () => activeServerChanged = true; 
            //------------Execute Test---------------------------
            shellViewModel.ActiveServer = anotherMockServer.Object;
            //------------Assert Results-------------------------
            Assert.IsFalse(activeServerChanged);
        }




        // ReSharper restore InconsistentNaming
    }

    public class MockServer:IServer
    {
        public MockServer()
        {
            ExplorerRepository = new Mock<IExplorerRepository>().Object;
            UpdateRepository = new Mock<IStudioUpdateManager>().Object;
            Permissions = new List<IWindowsGroupPermission>();
        }

        #region Implementation of IEquatable<IResource>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IResource other)
        {
            if (other == null)
            {
                return false;
            }
            if (other.ResourceID == ResourceID)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Implementation of IResource

        /// <summary>
        ///     The resource ID that uniquely identifies the resource.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public Guid ResourceID { get; set; }
        /// <summary>
        ///     The version that uniquely identifies the resource.
        /// </summary>
        // Version Version { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        /// <summary>
        ///     The display name of the resource.
        /// </summary>
        public string ResourceName { get; set; }
        /// <summary>
        ///     Gets or sets the type of the resource.
        /// </summary>
        public ResourceType ResourceType { get; set; }
        /// <summary>
        ///     Gets or sets the category of the resource.
        /// </summary>
        public string ResourcePath { get; set; }
        /// <summary>
        ///     Gets or sets the file path of the resource.
        ///     <remarks>
        ///         Must only be used by the catalog!
        ///     </remarks>
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        ///     Gets or sets the author roles.
        /// </summary>
        public string AuthorRoles { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is upgraded.
        /// </summary>
        public bool IsUpgraded { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating whether [is new resource].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [is new resource]; otherwise, <c>false</c>.
        /// </value>
        public bool IsNewResource { get; set; }
        public IList<IResourceForTree> Dependencies { get; set; }
        public bool IsValid { get; set; }
        public List<IErrorInfo> Errors { get; set; }
        public StringBuilder DataList { get; set; }
        public string Inputs { get; set; }
        public string Outputs { get; set; }
        public Permissions UserPermissions { get; set; }
        public IList<IResource> Children { get; set; }
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Gets the XML representation of this resource.
        /// </summary>
        /// <returns>The XML representation of this resource.</returns>
        public XElement ToXml()
        {
            return default(XElement);
        }

        /// <summary>
        ///     Gets the XML representation of this resource.
        /// </summary>
        /// <returns>The XML representation of this resource.</returns>
        XElement IResource.ToXml()
        {
            return ToXml();
        }

        /// <summary>
        ///     Gets the string builder for this resource.
        /// </summary>
        /// <returns></returns>
        public StringBuilder ToStringBuilder()
        {
            return null;
        }

        /// <summary>
        ///     Determines whether the given user roles are in the <see cref="IResource.AuthorRoles" />.
        /// </summary>
        /// <param name="userRoles">The user roles to be queried.</param>
        /// <returns>
        ///     <c>true</c> if the user roles are in the <see cref="IResource.AuthorRoles" />; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUserInAuthorRoles(string userRoles)
        {
            return false;
        }

        public void UpdateErrorsBasedOnXML(XElement xml)
        {
        }

        public void SetIsNew(XElement xml)
        {
        }

        public void GetInputsOutputs(XElement xml)
        {
        }

        public void ReadDataList(XElement xml)
        {
        }

        /// <summary>
        ///     If this instance <see cref="IResource.IsUpgraded" /> then sets the ID, Version, Name and ResourceType attributes on the given
        ///     XML.
        /// </summary>
        /// <param name="xml">The XML to be upgraded.</param>
        /// <param name="resource"></param>
        /// <returns>The XML with the additional attributes set.</returns>
        public XElement UpgradeXml(XElement xml, IResource resource)
        {
            return default(XElement);
        }

        #endregion

        #region Implementation of IServer

        public Task<bool> Connect()
        {
            return null;
        }

        public List<IResource> Load()
        {
            return null;
        }

        public Task<IExplorerItem> LoadExplorer()
        {
            return null;
        }

        public IList<IServer> GetServerConnections()
        {
            return null;
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return null;
        }

        public IExplorerRepository ExplorerRepository { get; private set; }

        public bool IsConnected()
        {
            return false;
        }

        public void ReloadTools()
        {
        }

        public void Disconnect()
        {
        }

        public void Edit()
        {
        }

        public List<IWindowsGroupPermission> Permissions { get; private set; }

        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;

        public IStudioUpdateManager UpdateRepository { get; private set; }

        public string GetServerVersion()
        {
            return "0";
        }

        #endregion
    }

    internal class MockServiceDesignerViewModel : IServiceDesignerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MockServiceDesignerViewModel(IResource resource)
        {
            Resource = resource;
        }

        #region Implementation of IServiceDesignerViewModel

        public IResource Resource
        {
            get;
            set;
        }
        /// <summary>
        /// Should the hyperlink to execute the service in browser
        /// </summary>
        public bool IsServiceLinkVisible
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Command to execute when the hyperlink is clicked
        /// </summary>
        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The hyperlink text shown
        /// </summary>
        public string DisplayWorkflowLink
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
    
    internal class MockWorkflowServiceDesignerViewModel : IWorkflowServiceDesignerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MockWorkflowServiceDesignerViewModel(IResource resource)
        {
            Resource = resource;
        }

        #region Implementation of IServiceDesignerViewModel

        public IResource Resource
        {
            get;
            set;
        }
        /// <summary>
        /// Should the hyperlink to execute the service in browser
        /// </summary>
        public bool IsServiceLinkVisible
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Command to execute when the hyperlink is clicked
        /// </summary>
        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The hyperlink text shown
        /// </summary>
        public string DisplayWorkflowLink
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The designer for the resource
        /// </summary>
        public UIElement DesignerView
        {
            get
            {
                return null;
            }
        }
        public bool IsNewWorkflow
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        #endregion
    }
}
