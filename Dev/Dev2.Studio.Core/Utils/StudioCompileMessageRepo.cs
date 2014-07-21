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
