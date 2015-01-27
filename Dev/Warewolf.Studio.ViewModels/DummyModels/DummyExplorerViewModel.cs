using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Runtime.ServiceModel.Data;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyExplorerViewModel:ExplorerViewModel
    {
        public DummyExplorerViewModel(IShellViewModel shellViewModel)
            : base(shellViewModel)
        {
            Environments = CreateEnvironments(shellViewModel);
        }

        static List<IEnvironmentViewModel> CreateEnvironments(IShellViewModel shellViewModel)
        {
            var server = new Server(new Uri( @"http://localhost:3142"));
            var oneLevelDeep = new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "One Level Deep",
            };
            oneLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "Resource One Level Deep",
                ResourceId = Guid.Parse("0bdc3207-ff6b-4c01-a5eb-c7060222f75d")
            });
            var multiLevelDeep = new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "Multi Level Deep",
                ResourceId = Guid.Parse("0bdc3207-ff6b-4c01-a5eb-c7060222f75d")
            };
            multiLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "No children",
                ResourceId = Guid.Parse("0bdc3207-ff6b-4c01-a5eb-c7060222f75d")
            });
            var childHasChildren = new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "Has One Chid",
                ResourceId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1")
               
            };
            multiLevelDeep.Children.Add(childHasChildren);
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "Is child of child"
            });
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object, null)
            {
                ResourceName = "Is Another child of a child"
            });
            return new List<IEnvironmentViewModel>
            {
                new EnvironmentViewModel(new DummyServer(),shellViewModel)
                {
                    DisplayName = "Test1",
                    Children = new List<IExplorerItemViewModel>
                    {
                        new ExplorerItemViewModel(shellViewModel,server,new Mock<IExplorerHelpDescriptorBuilder>().Object,null)
                        {
                            ResourceName = "SingleLevel"

                        },
                        oneLevelDeep,
                        multiLevelDeep
                    }
                }, 
                new EnvironmentViewModel(server,shellViewModel)
                {
                    DisplayName = "Test4",
                    Children = new List<IExplorerItemViewModel>
                    {
                        multiLevelDeep
                    }
                }
            };
        }
    }

    internal class DummyServer : Resource,IServer
    {
        IExplorerRepository _explorerRepository;

        public DummyServer()
        {
            Permissions = new List<IWindowsGroupPermission>();
        }

        #region Implementation of IServer

        public Task<bool> Connect()
        {
            return new Task<bool>(() => true);
        }

        public List<IResource> Load()
        {
            return null;
        }

        public Task<IExplorerItem> LoadExplorer()
        {
            return new Task<IExplorerItem>(() => null);
        }

        public IList<IServer> GetServerConnections()
        {
            return new List<IServer>{new DummyServer{ResourceName = "Localhost"},new DummyServer{ResourceName = "Remote Server"}};
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return null;
        }

        public IExplorerRepository ExplorerRepository
        {
            get
            {
                return _explorerRepository;
            }
        }

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

        #endregion

        #region Overrides of Resource

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return ResourceName;
        }

        #endregion
    }
}
