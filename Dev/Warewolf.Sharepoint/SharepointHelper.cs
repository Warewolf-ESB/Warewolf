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
    public class SharepointHelper
    {
        private string Server { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointHelper(string server, string userName="", string password="", bool isSharepointOnline=false)
        {
            Server = server;
            UserName = userName;
            Password = password;
            IsSharepointOnline = isSharepointOnline;
        }

        private bool IsSharepointOnline { get; set; }

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

        private ClientContext GetContextWithOnlineCredentials()
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

        public List<SharepointListTo> LoadLists()
        {
            var lists = new List<SharepointListTo>();
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

        public string DownLoadFile(string serverPath, string localPath, bool overwrite = false)
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

            if (!overwrite && !string.IsNullOrEmpty(localPath) && !string.IsNullOrEmpty(fileName))
            {
                if (CheckIfFileExist(Path.Combine(localPath, fileName))) return "Success";
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

                if (fileName == null || localPath == null) return "Failed";

                var newPath = Path.Combine(localPath, fileName);

                using (var fileStream = System.IO.File.Create(newPath))
                {
                    fileInfo.Stream.CopyTo(fileStream);
                }
            }

            return "Success";
        }

        private string GetFileName(string serverPath)
        {
            return Path.GetFileName(serverPath);
        }

        private string GetFileExtention(string localPath)
        {
            return Path.GetExtension(localPath);
        }

        private bool CheckIfFileExist(string localPath)
        {
            return System.IO.File.Exists(localPath);
        }

        private void CreateFolderIfNotExist(string localPath)
        {
            if (localPath != null && !Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
        }

        private string GetSharePointRootFolder(string folderUrl, ClientContext ctx)
        {
            var list = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(list.RootFolder);
            ctx.ExecuteQuery();
            var serverRelativeUrl = list.RootFolder.ServerRelativeUrl;
            var fullPath = folderUrl;
            if(!folderUrl.StartsWith(serverRelativeUrl))
            {
                fullPath = serverRelativeUrl + "/" + folderUrl;
            }
            return fullPath;
        }

        private static SharepointFieldTo CreateSharepointFieldToFromSharepointField(Field field)
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
                    Web web = ctx.Web;
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
                        Web web = ctx.Web;
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
            List list = ctx.Web.Lists.GetByTitle(listName);
            if (editableFieldsOnly)
            {
                ctx.Load(list.Fields, collection => collection.Where(field => field.Hidden == false && field.ReadOnlyField == false));
            }
            else
            {
                ctx.Load(list.Fields, collection => collection.Where(field => field.Hidden == false));
            }
            return list;
        }
    }
}