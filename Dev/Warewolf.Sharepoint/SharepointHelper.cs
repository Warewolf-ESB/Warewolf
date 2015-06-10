using System;
using System.Collections.Generic;
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

        public List<ISharepointFieldTo> LoadFieldsForList(string listName)
        {
            var fields = new List<ISharepointFieldTo>();
            using(var ctx = GetContext())
            {
                List list = ctx.Web.Lists.GetByTitle(listName);
                ctx.Load(list.Fields);
                ctx.ExecuteQuery();
                var fieldCollection = list.Fields;
                foreach(var field in fieldCollection)
                {
                    fields.Add(new SharepointFieldTo{Name = field.Title,InternalName = field.InternalName});
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
    }
}
