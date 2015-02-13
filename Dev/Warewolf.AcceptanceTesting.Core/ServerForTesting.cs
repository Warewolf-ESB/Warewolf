using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Moq;

namespace Warewolf.AcceptanceTesting.Core
{
    public class ServerForTesting : Resource, IServer
    {
        public ServerForTesting(Mock<IExplorerRepository> explorerRepository)
        {
            MockExplorerRepo = explorerRepository;
            _explorerProxy = explorerRepository.Object;
            ResourceName = "localhost";
        }

        private IExplorerRepository _explorerProxy;
        Mock<IExplorerRepository> _mockExplorerRepo;

        public ServerForTesting(IResource copy) : base(copy)
        {
        }

        public ServerForTesting(XElement xml) : base(xml)
        {
        }

        public Task<bool> Connect()
        {
            return Task.FromResult(true);
        }

        public List<IResource> Load()
        {
            return CreateResources();
        }

        private List<IResource> CreateResources()
        {
            return new List<IResource>();
        }

        public Task<IExplorerItem> LoadExplorer()
        {
            return Task.FromResult(CreateExplorerItems());
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

        private IEnumerable<IExplorerItem> CreateFolders(string[] names)
        {
            var folders = new List<IExplorerItem>();
            foreach (var name in names)
            {
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns(ResourceType.Folder);
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(name);
                mockIExplorerItem.Setup(item => item.Children).Returns(new List<IExplorerItem>());
                folders.Add(mockIExplorerItem.Object);
            }
            CreateChildrenForFolder(folders[1], new[] { "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1", "Child 1" });
            return folders;
        }

        private void CreateChildrenForFolder(IExplorerItem explorerItem, string[] childNames)
        {
            int i = 1;
            var resourceType = ResourceType.EmailSource;
            foreach (var name in childNames)
            {
                if (i % 2 == 0)
                {
                    resourceType = ResourceType.WorkflowService;
                }
                if (i % 3 == 0)
                {
                    resourceType = ResourceType.DbService;
                }
                if (i % 4 == 0)
                {
                    resourceType = ResourceType.WebSource;
                }
                var mockIExplorerItem = new Mock<IExplorerItem>();
                mockIExplorerItem.Setup(item => item.ResourceType).Returns(resourceType);
                mockIExplorerItem.Setup(item => item.DisplayName).Returns(explorerItem.DisplayName + " " + name);
                mockIExplorerItem.Setup(item => item.ResourceId).Returns(Guid.NewGuid());
                explorerItem.Children.Add(mockIExplorerItem.Object);
                i++;
            }
        }

        public IList<IServer> GetServerConnections()
        {
            return null;
        }

        public IList<IToolDescriptor> LoadTools()
        {
            throw new NotImplementedException();
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

        public bool IsConnected()
        {
            return true;
        }

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
                return new List<IWindowsGroupPermission>{new WindowsGroupPermission
                {
                    Administrator = true,
                    IsServer = true,
                    ResourceID = Guid.Empty
                
                }};
            }
        }
        
        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;

        public IStudioUpdateManager UpdateRepository
        {
            get { throw new NotImplementedException(); }
        }
        public Mock<IExplorerRepository> MockExplorerRepo
        {
            get
            {
                return _mockExplorerRepo;
            }
            set
            {
                _mockExplorerRepo = value;
            }
        }

        public string GetServerVersion()
        {
            throw new NotImplementedException();
        }
    }
}