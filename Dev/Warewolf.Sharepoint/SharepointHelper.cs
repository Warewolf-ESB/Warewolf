using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.SharePoint.Client;

namespace Warewolf.Sharepoint
{
    public class SharepointHelper
    {
        string Server { get; set; }
        string UserName { get; set; }
        string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointHelper(string server,string userName,string password,bool isSharepointOnline)
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
            if(string.IsNullOrEmpty(UserName) && String.IsNullOrEmpty(Password))
            {
                ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                ctx.Credentials = new NetworkCredential(UserName,Password);
            }
            return ctx;
        }

        ClientContext GetContextWithOnlineCredentials()
        {
            var ctx = new ClientContext(Server);
            if(string.IsNullOrEmpty(UserName) && String.IsNullOrEmpty(Password))
            {
                ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                var secureString = new SecureString();
                foreach(var c in Password)
                {
                    secureString.AppendChar(c);
                }
                ctx.Credentials = new SharePointOnlineCredentials(UserName,secureString);
            }
            return ctx;
        }

        public List<SharepointListTo> LoadLists()
        {
            var lists = new List<SharepointListTo>();
            using(var context = GetContext())
            {
                var listCollection = context.Web.Lists;
                context.Load(listCollection);
                context.ExecuteQuery();
                lists.AddRange(listCollection.Select(list => new SharepointListTo { FullName = list.Title }));
            }
            return lists;
        }

        public List<ISharepointFieldTo> LoadFieldsForList(string listName,bool editableFieldsOnly)
        {
            var fields = new List<ISharepointFieldTo>();
            using(var ctx = GetContext())
            {
                var list = LoadFieldsForList(listName, ctx, editableFieldsOnly);
                ctx.ExecuteQuery();
                var fieldCollection = list.Fields;
                fields.AddRange(fieldCollection.Select(field => CreateSharepointFieldToFromSharepointField(field)));
            }
            return fields;
        }

        static SharepointFieldTo CreateSharepointFieldToFromSharepointField(Field field)
        {
            var sharepointFieldTo = new SharepointFieldTo { Name = field.Title, InternalName = field.InternalName, IsRequired = field.Required,IsEditable = !field.ReadOnlyField};
            switch(field.FieldTypeKind)
            {
                case FieldType.Invalid:
                    break;
                case FieldType.Integer:
                case FieldType.Counter:
                    sharepointFieldTo.Type = SharepointFieldType.Integer;
                    var intField = field as FieldNumber;
                    if(intField != null)
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
                    if(textField != null)
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
                    if(numberField != null)
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
                using(var ctx = GetContext())
                {
                    Web web = ctx.Web;
                    ctx.Load(web);
                    ctx.ExecuteQuery();
                }
            }
            catch(Exception)
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
            if(editableFieldsOnly)
            {
                ctx.Load(list.Fields, collection => collection.Where(field => field.Hidden == false && field.ReadOnlyField==false));
            }
            else
            {
                ctx.Load(list.Fields, collection => collection.Where(field => field.Hidden == false));
            }
            return list;
        }

    }
}
