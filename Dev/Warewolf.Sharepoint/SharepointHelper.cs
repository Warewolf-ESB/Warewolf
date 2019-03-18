#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using File = Microsoft.SharePoint.Client.File;

namespace Warewolf.Sharepoint
{
    public class SharepointHelperFactory : ISharepointHelperFactory
    {
        public ISharepointHelper New(string server, string userName, string password, bool isSharepointOnline)
        {
            return new SharepointHelper(server, userName, password, isSharepointOnline);
        }
    }

    public class SharepointHelper : ISharepointHelper
    {
        string Server { get; set; }
        string UserName { get; set; }
        string Password { get; set; }

        public SharepointHelper(string server)
            : this(server, "", "", false)
        {
        }
        
        public SharepointHelper(string server, string userName, string password, bool isSharepointOnline)
        {
            Server = server;
            UserName = userName;
            Password = password;
            IsSharepointOnline = isSharepointOnline;
        }

        bool IsSharepointOnline { get; set; }

        public ClientContext GetContext()
        {
            if (IsSharepointOnline)
            {
                return GetContextWithOnlineCredentials();
            }
            var ctx = new ClientContext(Server);
            if (string.IsNullOrEmpty(UserName) && String.IsNullOrEmpty(Password))
            {
                ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                ctx.Credentials = new NetworkCredential(UserName, Password);
            }
            return ctx;
        }

        ClientContext GetContextWithOnlineCredentials()
        {
            var ctx = new ClientContext(Server);
            if (string.IsNullOrEmpty(UserName) && String.IsNullOrEmpty(Password))
            {
                ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                var secureString = new SecureString();
                foreach (var c in Password)
                {
                    secureString.AppendChar(c);
                }
                ctx.Credentials = new SharePointOnlineCredentials(UserName, secureString);
            }
            return ctx;
        }

        public List<ISharepointListTo> LoadLists()
        {
            var lists = new List<ISharepointListTo>();
            using (var context = GetContext())
            {
                var listCollection = context.Web.Lists;
                context.Load(listCollection);
                context.ExecuteQuery();
                lists.AddRange(listCollection.Select(list => new SharepointListTo { FullName = list.Title }));
            }
            return lists;
        }

        public List<ISharepointFieldTo> LoadFieldsForList(string listName, bool editableFieldsOnly)
        {
            var fields = new List<ISharepointFieldTo>();
            using (var ctx = GetContext())
            {
                var list = LoadFieldsForList(listName, ctx, editableFieldsOnly);
                ctx.ExecuteQuery();
                var fieldCollection = list.Fields;
                fields.AddRange(fieldCollection.Select(field => CreateSharepointFieldToFromSharepointField(field)));
            }

            return fields;
        }

        public List<string> LoadFiles(string folderUrl)
        {
            var fields = new List<string>();

            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(folderUrl, ctx);
                var folder = ctx.Web.GetFolderByServerRelativeUrl(fullPath);
                ctx.Load(folder, f => f.Files.Include(c => c.Name, c => c.ServerRelativeUrl), f => f.ParentFolder.ServerRelativeUrl, f => f.ServerRelativeUrl, f => f.Folders);
                ctx.ExecuteQuery();

                fields.AddRange(folder.Files.Select(file => file.ServerRelativeUrl));
            }

            return fields;
        }

        public List<string> LoadFolders(string folderUrl)
        {
            var folders = new List<string>();

            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(folderUrl, ctx);
                var folder = ctx.Web.GetFolderByServerRelativeUrl(fullPath);
                ctx.Load(folder, f => f.Files.Include(c => c.Name, c => c.ServerRelativeUrl), f => f.ParentFolder.ServerRelativeUrl, f => f.ServerRelativeUrl, f => f.Folders);
                ctx.ExecuteQuery();

                folders.AddRange(folder.Folders.Select(file => file.ServerRelativeUrl));
            }
            return folders;
        }

        public string CopyFile(string serverPathFrom, string serverPathTo, bool overWrite)
        {
            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(serverPathFrom, ctx);
                var copyPath = GetSharePointRootFolder(serverPathTo, ctx);
                var file = ctx.Web.GetFileByServerRelativeUrl(fullPath);

                ctx.Load(file);
                ctx.ExecuteQuery();

                file.CopyTo(copyPath, overWrite);
                ctx.ExecuteQuery();
            }

            return "Success";
        }

        public string MoveFile(string serverPathFrom, string serverPathTo, bool overWrite)
        {
            var moveOption = MoveOperations.None;

            if (overWrite)
            {
                moveOption = MoveOperations.Overwrite;
            }

            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(serverPathFrom, ctx);
                var movePath = GetSharePointRootFolder(serverPathTo, ctx);
                var file = ctx.Web.GetFileByServerRelativeUrl(fullPath);

                ctx.Load(file);
                ctx.ExecuteQuery();

                file.MoveTo(movePath, moveOption);
                ctx.ExecuteQuery();
            }

            return "Success";
        }

        public string Delete(string serverPath)
        {
            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(serverPath, ctx);
                var file = ctx.Web.GetFileByServerRelativeUrl(fullPath);

                ctx.Load(file);
                ctx.ExecuteQuery();

                file.DeleteObject();
                ctx.ExecuteQuery();
            }

            return "Success";
        }

        public string UploadFile(string serverPath, string localPath)
        {
            var fileName = string.Empty;

            if (!string.IsNullOrEmpty(serverPath))
            {
                var extention = GetFileExtention(serverPath);

                if (!string.IsNullOrEmpty(extention))
                {
                    fileName = GetFileName(serverPath);
                    serverPath = Path.GetDirectoryName(serverPath);
                }
            }

            using (var ctx = GetContext())
            {
                using (var fs = new FileStream(localPath, FileMode.Open))
                {
                    var localFile = new FileInfo(localPath);

                    var fullPath = GetSharePointRootFolder(serverPath, ctx);

                    var folder = ctx.Web.GetFolderByServerRelativeUrl(fullPath);

                    ctx.Load(folder, f => f.Files.Include(c => c.Name, c => c.ServerRelativeUrl), f => f.ParentFolder.ServerRelativeUrl, f => f.ServerRelativeUrl, f => f.Folders);
                    ctx.ExecuteQuery();

                    var newfileName = !string.IsNullOrEmpty(fileName) ? fileName : localFile.Name;

                    var fileUrl = string.Format("{0}/{1}", folder.ServerRelativeUrl, newfileName);

                    File.SaveBinaryDirect(ctx, fileUrl, fs, true);
                }
            }

            return "Success";
        }

        public string DownLoadFile(string serverPath, string localPath) => DownLoadFile(serverPath, localPath, false);
        public string DownLoadFile(string serverPath, string localPath, bool overwrite)
        {
            var fileName = Path.GetFileName(localPath);

            if (string.IsNullOrEmpty(GetFileExtention(localPath)))
            {
                fileName = GetFileName(serverPath);
            }
            else
            {
                localPath = Path.GetDirectoryName(localPath);
            }

            if (!overwrite && !string.IsNullOrEmpty(localPath) && !string.IsNullOrEmpty(fileName) && CheckIfFileExist(Path.Combine(localPath, fileName)))
            {
                return "Success";
            }


            CreateFolderIfNotExist(localPath);

            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(serverPath, ctx);

                var file = ctx.Web.GetFileByServerRelativeUrl(fullPath);

                ctx.Load(file);
                ctx.ExecuteQuery();

                var fileRef = file.ServerRelativeUrl;
                var fileInfo = File.OpenBinaryDirect(ctx, fileRef);

                if (fileName == null || localPath == null)
                {
                    return "Failed";
                }

                var newPath = Path.Combine(localPath, fileName);

                using (var fileStream = System.IO.File.Create(newPath))
                {
                    fileInfo.Stream.CopyTo(fileStream);
                }
            }

            return "Success";
        }

        string GetFileName(string serverPath) => Path.GetFileName(serverPath);

        string GetFileExtention(string localPath) => Path.GetExtension(localPath);

        bool CheckIfFileExist(string localPath) => System.IO.File.Exists(localPath);

        void CreateFolderIfNotExist(string localPath)
        {
            if (localPath != null && !Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
        }

        string GetSharePointRootFolder(string folderUrl, ClientContext ctx)
        {
            var list = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(list.RootFolder);
            ctx.ExecuteQuery();
            var serverRelativeUrl = list.RootFolder.ServerRelativeUrl;
            var fullPath = folderUrl;
            if (!folderUrl.StartsWith(serverRelativeUrl))
            {
                fullPath = serverRelativeUrl + "/" + folderUrl;
            }
            return fullPath;
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        static SharepointFieldTo CreateSharepointFieldToFromSharepointField(Field field)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var sharepointFieldTo = new SharepointFieldTo { Name = field.Title, InternalName = field.InternalName, IsRequired = field.Required, IsEditable = !field.ReadOnlyField };
            switch (field.FieldTypeKind)
            {
                case FieldType.Invalid:
                    break;

                case FieldType.Integer:
                case FieldType.Counter:
                    sharepointFieldTo.Type = SharepointFieldType.Integer;
                    var intField = field as FieldNumber;
                    if (intField != null)
                    {
                        sharepointFieldTo.MaxValue = intField.MaximumValue;
                        sharepointFieldTo.MinValue = intField.MinimumValue;
                    }
                    break;

                case FieldType.Currency:
                    sharepointFieldTo.Type = SharepointFieldType.Currency;
                    break;

                case FieldType.Text:
                case FieldType.Choice:
                    sharepointFieldTo.Type = SharepointFieldType.Text;
                    var textField = field as FieldText;
                    if (textField != null)
                    {
                        sharepointFieldTo.MaxLength = textField.MaxLength;
                    }
                    break;

                case FieldType.Note:
                    sharepointFieldTo.Type = SharepointFieldType.Note;
                    break;

                case FieldType.DateTime:
                    sharepointFieldTo.Type = SharepointFieldType.DateTime;
                    break;

                case FieldType.Boolean:
                    sharepointFieldTo.Type = SharepointFieldType.Boolean;
                    break;

                case FieldType.Number:
                    var numberField = field as FieldNumber;
                    sharepointFieldTo.Type = SharepointFieldType.Number;
                    if (numberField != null)
                    {
                        sharepointFieldTo.MaxValue = numberField.MaximumValue;
                        sharepointFieldTo.MinValue = numberField.MinimumValue;
                    }
                    break;
                case FieldType.Lookup:
                    break;
                case FieldType.URL:
                    break;
                case FieldType.Computed:
                    break;
                case FieldType.Threading:
                    break;
                case FieldType.Guid:
                    break;
                case FieldType.MultiChoice:
                    break;
                case FieldType.GridChoice:
                    break;
                case FieldType.Calculated:
                    break;
                case FieldType.File:
                    break;
                case FieldType.Attachments:
                    break;
                case FieldType.User:
                    break;
                case FieldType.Recurrence:
                    break;
                case FieldType.CrossProjectLink:
                    break;
                case FieldType.ModStat:
                    break;
                case FieldType.Error:
                    break;
                case FieldType.ContentTypeId:
                    break;
                case FieldType.PageSeparator:
                    break;
                case FieldType.ThreadIndex:
                    break;
                case FieldType.WorkflowStatus:
                    break;
                case FieldType.AllDayEvent:
                    break;
                case FieldType.WorkflowEventType:
                    break;
                case FieldType.Geolocation:
                    break;
                case FieldType.OutcomeChoice:
                    break;
                case FieldType.MaxItems:
                    break;
                default:
                    sharepointFieldTo.Type = SharepointFieldType.Text;
                    break;
            }
            return sharepointFieldTo;
        }

        public string TestConnection(out bool isSharepointOnline)
        {
            var result = "Test Successful";
            isSharepointOnline = false;
            try
            {
                using (var ctx = GetContext())
                {
                    var web = ctx.Web;
                    ctx.Load(web);
                    ctx.ExecuteQuery();
                }
            }
            catch (Exception)
            {
                try
                {
                    using (var ctx = GetContextWithOnlineCredentials())
                    {
                        var web = ctx.Web;
                        ctx.Load(web);
                        ctx.ExecuteQuery();
                        isSharepointOnline = true;
                    }
                }
                catch (Exception ex)
                {
                    result = "Test Failed: " + ex.Message;
                }
            }
            return result;
        }

        public List LoadFieldsForList(string listName, ClientContext ctx, bool editableFieldsOnly)
        {
            var list = ctx.Web.Lists.GetByTitle(listName);
            if (editableFieldsOnly)
            {
                ctx.Load(list.Fields, collection => collection.Where(field => !field.Hidden && !field.ReadOnlyField));
            }
            else
            {
                ctx.Load(list.Fields, collection => collection.Where(field => !field.Hidden));
            }
            return list;
        }
    }
}