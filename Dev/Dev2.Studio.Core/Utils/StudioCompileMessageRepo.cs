
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Controller;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Utils
{
    public interface IStudioCompileMessageRepo
    {
        CompileMessageList GetCompileMessagesFromServer(IContextualResourceModel resourceModel);
    }
    public interface IStudioCompileMessageRepoFactory
    {
        IStudioCompileMessageRepo Create();
    }

    public class StudioCompileMessageRepoFactory : IStudioCompileMessageRepoFactory
    {
        #region Implementation of IStudioCompileMessageRepoFactory

        public IStudioCompileMessageRepo Create()
        {
            return new StudioCompileMessageRepo();
        }

        #endregion
    }

    public class StudioCompileMessageRepo : IStudioCompileMessageRepo
    {
        public CompileMessageList GetCompileMessagesFromServer(IContextualResourceModel resourceModel)
        {
            var comsController = new CommunicationController { ServiceName = "FetchDependantCompileMessagesService" };

            var workspaceID = GlobalConstants.ServerWorkspaceID;

            comsController.AddPayloadArgument("ServiceID", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("WorkspaceID", workspaceID.ToString());
            var con = resourceModel.Environment.Connection;
            var result = comsController.ExecuteCommand<CompileMessageList>(con, GlobalConstants.ServerWorkspaceID);

            return result;
        }
    }
}
