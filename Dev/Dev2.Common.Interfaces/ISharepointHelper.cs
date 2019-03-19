#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Microsoft.SharePoint.Client;

namespace Dev2.Common.Interfaces
{
    public interface ISharepointHelperFactory
    {
        ISharepointHelper New(string server, string userName, string password, bool isSharepointOnline);
    }
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