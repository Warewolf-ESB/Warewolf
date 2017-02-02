using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Moq;
using Newtonsoft.Json;

namespace Warewolf.Testing
{
    public class ServerForTesting : Resource, IServer
    {
        public ServerForTesting(Mock<IExplorerRepository> explorerRepository)
        {
            MockExplorerRepo = explorerRepository;
            _explorerProxy = explorerRepository.Object;
            ResourceName = "localhost";
            DisplayName = "localhost";
            ServerID = Guid.Empty;
            _updateManager = new Mock<IStudioUpdateManager>().Object;
        }

        public ServerForTesting(Mock<IExplorerRepository> explorerRepository,Mock<IQueryManager> mockQueryManager)
        {
            MockExplorerRepo = explorerRepository;
            _explorerProxy = explorerRepository.Object;
            ResourceName = "localhost";
            DisplayName = "localhost";
            ServerID = Guid.Empty;
            _updateManager = new Mock<IStudioUpdateManager>().Object;
            _queryManager = mockQueryManager.Object;
        }

        public ServerForTesting(Mock<IExplorerRepository> explorerRepository, WindowsGroupPermission permission)
        {
            MockExplorerRepo = explorerRepository;
            _explorerProxy = explorerRepository.Object;
            ResourceName = "localhost";
            DisplayName = "localhost";
            ServerID = Guid.Empty;
            _updateManager = new Mock<IStudioUpdateManager>().Object;
            Permissions = new List<IWindowsGroupPermission>{ permission };
        }

        private readonly IExplorerRepository _explorerProxy;
#pragma warning disable 0649
        bool _hasLoaded;
#pragma warning restore 0649
        private IStudioUpdateManager _updateManager;
        private List<IWindowsGroupPermission> _permissions;

        public ServerForTesting(IResource copy) : base(copy)
        {
        }

        public ServerForTesting(XElement xml) : base(xml)
        {
        }

        public void Connect()
        {
        }

        public Task<bool> ConnectAsync()
        {
            var task = new Task<bool>(()=>true);
            task.Start();
            task.Wait();
            return task;
        }

        public string DisplayName { get;  set; }
        public bool HasLoaded => _hasLoaded;

        public bool CanDeployTo => _canDeployTo;

        public bool CanDeployFrom => _canDeployFrom;
        public IExplorerRepository ProxyLayer { get; }

        public IServer Clone()
        {
            return null;
        }

        public string GetMinSupportedVersion()
        {
            return null;
        }
        
        public Task<List<string>> LoadExplorerDuplicates()
        {
            return null;
        }

        public Permissions GetPermissions(Guid resourceID)
        {
            return Dev2.Common.Interfaces.Security.Permissions.None;
        }

        public List<IResource> Load()
        {
            return CreateResources();
        }

        private List<IResource> CreateResources()
        {
            return new List<IResource>();
        }

        public Task<IExplorerItem> LoadExplorer(bool reloadCatalogue=false)
        {
            return Task.FromResult(CreateExplorerItems());
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return null;
        }

        private IExplorerItem CreateExplorerItems()
        {
            var mockExplorerItem = new Mock<IExplorerItem>();
            mockExplorerItem.Setup(item => item.DisplayName).Returns("Level 0");
            var children = new List<IExplorerItem>();
            children.AddRange(CreateFolders(new[] { "Folder 1", "Folder 2", "Folder 3", "Folder 4", "Folder 5" }));
            mockExplorerItem.Setup(item => item.Children).Returns(children);
            return mockExplorerItem.Object;
        }

        private IEnumerable<IExplorerItem> CreateFolders(IEnumerable<string> names)
        {
            var folders = new List<IExplorerItem>();
            foreach (var name in names)
            {
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns("Folder");
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(name);
                mockIExplorerItem.Setup(item => item.Children).Returns(new List<IExplorerItem>());
                mockIExplorerItem.Setup(item => item.ResourcePath).Returns(name);
                folders.Add(mockIExplorerItem.Object);
            }
            CreateChildrenForFolder(folders[1], new[] { "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1" });
            return folders;
        }
        int i = 1;
#pragma warning disable 0649
        private bool _canDeployTo;
        private bool _canDeployFrom;
#pragma warning restore 0649
        private IQueryManager _queryManager;

        private void CreateChildrenForFolder(IExplorerItem explorerItem, IEnumerable<string> childNames)
        {

            var resourceType = "EmailSource";
            foreach (var name in childNames)
            {
                if (i % 2 == 0)
                {
                    resourceType = "WorkflowService";
                }
                if (i % 3 == 0)
                {
                    resourceType = "DbService";
                }
                if (i % 4 == 0)
                {
                    resourceType = "WebSource";
                }
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns(resourceType);
                var resourceName = explorerItem.DisplayName + " " + name;
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(resourceName);
                mockIExplorerItem.Setup(item => item.ResourceId).Returns(Guid.NewGuid());
                mockIExplorerItem.Setup(item => item.Parent).Returns(explorerItem);
                mockIExplorerItem.Setup(item => item.ResourcePath).Returns(explorerItem.ResourcePath + "\\" + resourceName);
                explorerItem.Children.Add(mockIExplorerItem.Object);
                i++;
            }
        }

        public IList<IServer> GetServerConnections()
        {
            return null;
        }

        public IList<IServer> GetAllServerConnections()
        {
            return null;
        }

        public IExplorerRepository ExplorerRepository
        {
            get
            {
                if (_explorerProxy != null)
                {
                    return _explorerProxy;
                }
                return new Mock<IExplorerRepository>().Object;
            }
        }

        [JsonIgnore]
        public IQueryManager QueryProxy
        {
            get
            {
                if (_queryManager != null)
                {
                    return _queryManager;
                }
                return new Mock<IQueryManager>().Object;
            }
        }

        public bool IsConnected => true;
        public bool AllowEdit { get; set; }

        public void ReloadTools()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Edit()
        {
            throw new NotImplementedException();
        }

        public List<IWindowsGroupPermission> Permissions
        {
            get
            {
                return _permissions ?? new List<IWindowsGroupPermission>{new WindowsGroupPermission
                {
                    Administrator = true,
                    IsServer = true,
                    ResourceID = Guid.Empty
                
                }};
            }
            set
            {
                _permissions = value;
            }
        }
        public Guid EnvironmentID { get; set; }

        public Guid? ServerID { get; private set; }
#pragma warning disable 0067
        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
        public event ItemAddedEvent ItemAddedEvent;
#pragma warning restore 0067

        [JsonIgnore]
        public IStudioUpdateManager UpdateRepository => _updateManager;

        public Mock<IExplorerRepository> MockExplorerRepo { get; set; }

        public string GetServerVersion()
        {
            throw new NotImplementedException();
        }

        public string GetServerInformationalVersion()
        {
            throw new NotImplementedException();
        }
        

        public Dictionary<string, string> GetServerInformation()
        {
            throw new NotImplementedException();
        }

        public IServer FetchServer(Guid savedServerID)
        {
            throw new NotImplementedException();
        }
    }
}