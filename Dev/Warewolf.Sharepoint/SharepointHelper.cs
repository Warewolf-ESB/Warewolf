using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.SharePoint.Client;

namespace Warewolf.Sharepoint
{
    public class SharepointHelper
    {
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointHelper(string server,string userName,string password)
        {
            Server = server;
            UserName = userName;
            Password = password;
        }

        public ClientContext GetContext()
        {
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

        public List<SharepointListTo> LoadLists()
        {
            var lists = new List<SharepointListTo>();
            using(var context = GetContext())
            {
                var listCollection = context.Web.Lists;
                context.Load(listCollection);
                context.ExecuteQuery();
                foreach(var list in listCollection)
                {
                    lists.Add(new SharepointListTo{FullName = list.Title});
                }
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
                foreach(var field in fieldCollection)
                {
                    var sharepointFieldTo = new SharepointFieldTo{Name = field.Title,InternalName = field.InternalName,IsRequired = field.Required};
                    switch(field.FieldTypeKind)
                    {
                        case FieldType.Invalid:
                            break;
                        case FieldType.Integer:
                            sharepointFieldTo.Type = SharepointFieldType.Integer;
                            var intField = field as FieldNumber;
                            if (intField != null)
                            {
                                sharepointFieldTo.MaxValue = intField.MaximumValue;
                                sharepointFieldTo.MinValue = intField.MinimumValue;
                            }
                            break;
                        case FieldType.Text:
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
                        case FieldType.Counter:
                            break;
                        case FieldType.Choice:
                            break;
                        case FieldType.Lookup:
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
                        case FieldType.Currency:
                            sharepointFieldTo.Type = SharepointFieldType.Currency;
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
                    }
                    fields.Add(sharepointFieldTo);
                }
            }
            return fields;
        }
        
        public string TestConnection()
        {
            var result = "Test Successful";
            try
            {
                using(var ctx = GetContext())
                {
                    Web web = ctx.Web;
                    ctx.Load(web);
                    ctx.ExecuteQuery();
                }
            }
            catch(Exception e)
            {
                result = "Test Failed: " + e.Message;
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
