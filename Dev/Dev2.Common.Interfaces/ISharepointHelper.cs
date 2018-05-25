using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.SharePoint.Client;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointHelper
    {
        string CopyFile(string serverPathFrom, string serverPathTo, bool overWrite);
        string Delete(string serverPath);
        string DownLoadFile(string serverPath, string localPath);
        string DownLoadFile(string serverPath, string localPath, bool overwrite);
        ClientContext GetContext();
        List<ISharepointFieldTo> LoadFieldsForList(string listName, bool editableFieldsOnly);
        List LoadFieldsForList(string listName, ClientContext ctx, bool editableFieldsOnly);
        List<string> LoadFiles(string folderUrl);
        List<string> LoadFolders(string folderUrl);
        List<ISharepointListTo> LoadLists();
        string MoveFile(string serverPathFrom, string serverPathTo, bool overWrite);
        string TestConnection(out bool isSharepointOnline);
        string UploadFile(string serverPath, string localPath);
    }
}