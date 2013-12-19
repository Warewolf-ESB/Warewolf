using Dev2.Controller;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Utils
{
    public class StudioCompileMessageRepo
    {
        public CompileMessageList GetCompileMessagesFromServer(IContextualResourceModel resourceModel)
        {
            var comsController = new CommunicationController { ServiceName = "FetchDependantCompileMessagesService" };

            var workspaceID = resourceModel.Environment.Connection.WorkspaceID;

            comsController.AddPayloadArgument("ServiceID", resourceModel.ID.ToString());
            comsController.AddPayloadArgument("WorkspaceID", workspaceID.ToString());
            var con = resourceModel.Environment.Connection;
            var result = comsController.ExecuteCommand<CompileMessageList>(con, con.WorkspaceID);

            return result;
        }
    }
}
