#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using Microsoft.SharePoint.Client;
using System.Security.Policy;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.Exchange.WebServices.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
#if NETFRAMEWORK
using File = Microsoft.SharePoint.Client.File;
#endif

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
#if NETFRAMEWORK
                ctx.Credentials = new SharePointOnlineCredentials(UserName, secureString);
#else
                ctx.Credentials = new SharePointOnlineCredentials(UserName, secureString.ToString());
#endif
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
#if NETFRAMEWORK
                context.ExecuteQuery();
#else
                context.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

                file.CopyTo(copyPath, overWrite);
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

                file.MoveTo(movePath, moveOption);
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

                file.DeleteObject();
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
                string filepath = localPath;
                FileCreationInformation newfile = new FileCreationInformation();
                newfile.Url = System.IO.Path.GetFileName(filepath);
                newfile.Content = System.IO.File.ReadAllBytes(filepath);
                var fullPath = GetSharePointRootFolder(serverPath, ctx);
                var folder = ctx.Web.GetFolderByServerRelativeUrl(fullPath);
                Microsoft.SharePoint.Client.File uploadFile = folder.Files.Add(newfile);
                ctx.Load(folder, f => f.Files.Include(c => c.Name, c => c.ServerRelativeUrl), f => f.ParentFolder.ServerRelativeUrl, f => f.ServerRelativeUrl, f => f.Folders);
                ctx.ExecuteQueryAsync().Wait();
                ctx.Load(uploadFile);
                ctx.ExecuteQueryAsync().Wait();
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

            if (fileName == null || localPath == null)
            {
                return "Failed";
            }

            CreateFolderIfNotExist(localPath);

            using (var ctx = GetContext())
            {
                var fullPath = GetSharePointRootFolder(serverPath, ctx);

                var file = ctx.Web.GetFileByServerRelativeUrl(fullPath);

                ctx.Load(file);
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif

                Microsoft.SharePoint.Client.ClientResult<Stream> mstream = file.OpenBinaryStream();
                ctx.ExecuteQueryAsync().Wait();
              
                var newPath = Path.Combine(localPath, fileName);

                using (var fileStream = new System.IO.FileStream(newPath, System.IO.FileMode.Create))
                {
                    mstream.Value.CopyTo(fileStream);
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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
#if NETFRAMEWORK
                ctx.ExecuteQuery();
#else
                ctx.ExecuteQueryAsync().Wait();
#endif
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