using Dev2.Common;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Utils
{
    public class StudioCompileMessageRepo
    {
        public static string GetCompileMessagesFromServer(IContextualResourceModel resourceModel)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FetchDependantCompileMessagesService";
            dataObj.ServiceID = resourceModel.ID;
            var workspaceID = resourceModel.Environment.Connection.WorkspaceID;
            dataObj.WorkspaceID = workspaceID;
            dataObj.FilterList = "";
            string result = resourceModel.Environment.Connection.ExecuteCommand(dataObj.XmlString, workspaceID,
                GlobalConstants.NullDataListID);
            return result;
        }
    }
}
