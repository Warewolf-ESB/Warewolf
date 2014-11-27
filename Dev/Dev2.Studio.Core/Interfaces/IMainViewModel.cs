
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Input;
using Dev2.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Interfaces
{
    public interface IMainViewModel
    {
        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        AuthorizeCommand<string> NewResourceCommand { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }
        IContextualResourceModel DeployResource { get; set; }
        void SetActiveEnvironment(IEnvironmentModel activeEnvironment);
        bool IsWorkFlowOpened(IContextualResourceModel resource);
        void UpdateWorkflowLink(IContextualResourceModel resource, string newPath, string oldPath);
        void ClearToolboxSelection();
        void UpdatePane(IContextualResourceModel model);

        void AddWorkSurfaceContext(IContextualResourceModel resourceModel);
    }
}
