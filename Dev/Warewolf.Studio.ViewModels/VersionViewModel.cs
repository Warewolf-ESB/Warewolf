using System;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Dev2;
using System.Xml.Linq;
using Dev2.Studio.Core.Models;
using Dev2.Services.Events;
using System.Text;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;

namespace Warewolf.Studio.ViewModels
{
    public class VersionViewModel : ExplorerItemViewModel
    {
        public VersionViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
            : base(server, parent, selectAction, shellViewModel, popupController)
        {
        }
    }

    public static class VersionViewModelExtenstions
    {
        public static IContextualResourceModel ToContextualResourceModel(this IVersionInfo versionViewModel, IServer server, Guid? Id)
        {
            VerifyArgument.IsNotNull(nameof(versionViewModel), versionViewModel);
            var workflowXaml = server?.ProxyLayer?.GetVersion(versionViewModel, versionViewModel.ResourceId);
            if (workflowXaml != null)
            {
                var resourceModel = server?.ResourceRepository.LoadContextualResourceModel(versionViewModel.ResourceId);
                var xamlElement = XElement.Parse(workflowXaml.ToString());
                var dataList = xamlElement.Element(@"DataList");
                var dataListString = string.Empty;
                if (dataList != null)
                {
                    dataListString = dataList.ToString();
                }
                var action = xamlElement.Element(@"Action");

                var xamlString = string.Empty;
                var xaml = action?.Element(@"XamlDefinition");
                if (xaml != null)
                {
                    xamlString = xaml.Value;
                }
                var resourceVersion = new ResourceModel(server, EventPublishers.Aggregator)
                {
                    ResourceType = resourceModel.ResourceType,
                    ResourceName = resourceModel.ResourceName,
                    WorkflowXaml = new StringBuilder(xamlString),
                    UserPermissions = Permissions.Contribute,
                    DataList = dataListString,
                    IsVersionResource = true,
                    ID = Id ?? Guid.NewGuid()
                };

                return resourceVersion;
            }
            return default(IContextualResourceModel);
        }
    }
}