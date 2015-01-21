using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyExplorerViewModel:ExplorerViewModel
    {
        public DummyExplorerViewModel(IShellViewModel shellViewModel)
        {
            Environments = CreateEnvironments(shellViewModel);
        }

        static List<IEnvironmentViewModel> CreateEnvironments(IShellViewModel shellViewModel)
        {
            var oneLevelDeep = new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "One Level Deep",
            };
            oneLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "Resource One Level Deep"
            });
            var multiLevelDeep = new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "Multi Level Deep"
            };
            multiLevelDeep.Children.Add(new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "No children"
            });
            var childHasChildren = new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "Has One Chid"
            };
            multiLevelDeep.Children.Add(childHasChildren);
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel)
            {
                ResourceName = "Is child of child"
            });
            childHasChildren.Children.Add(new ExplorerItemViewModel(shellViewModel)
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
                        new ExplorerItemViewModel(shellViewModel)
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

    internal class DummyServer : IServer
    {
        #region Implementation of IServer

        public bool Connect()
        {
            return true;
        }

        public IList<IResource> Load()
        {
            return new List<IResource>();
        }

        public IList<IServer> GetServerConnections()
        {
            return new List<IServer>();
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

        #endregion



    }
}
