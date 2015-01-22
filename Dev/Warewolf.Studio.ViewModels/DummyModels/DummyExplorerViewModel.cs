using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Runtime.ServiceModel.Data;
using Moq;

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
            var oneLevelDeep = new ExplorerItemViewModel(shellViewModel,new DummyServer(),new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "One Level Deep",
            };
            oneLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel, new DummyServer(), new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "Resource One Level Deep"
            });
            var multiLevelDeep = new ExplorerItemViewModel(shellViewModel, new DummyServer(),new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "Multi Level Deep"
            };
            multiLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel, new DummyServer(), new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "No children"
            });
            var childHasChildren = new ExplorerItemViewModel(shellViewModel, new DummyServer(), new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "Has One Chid"
            };
            multiLevelDeep.Children.Add(childHasChildren);
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel, new DummyServer(), new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "Is child of child"
            });
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel, new DummyServer(), new Mock<IExplorerHelpDescriptorBuilder>().Object)
            {
                ResourceName = "Is Another child of a child"
            });
            return new List<IEnvironmentViewModel>
            {
                new EnvironmentViewModel(new DummyServer(),shellViewModel)
                {
                    DisplayName = "Test1",
                    ExplorerItemViewModels = new List<IExplorerItemViewModel>
                    {
                        new ExplorerItemViewModel(shellViewModel,new DummyServer(),new Mock<IExplorerHelpDescriptorBuilder>().Object)
                        {
                            ResourceName = "SingleLevel"
                        },
                        oneLevelDeep,
                        multiLevelDeep
                    }
                }, 
                new EnvironmentViewModel(new DummyServer(),shellViewModel)
                {
                    DisplayName = "Test4",
                    ExplorerItemViewModels = new List<IExplorerItemViewModel>
                    {
                        multiLevelDeep
                    }
                }
            };
        }
    }

    internal class DummyServer : Resource,IServer
    {
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

        public event PermissionsChanged PermissionsChanged;

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
