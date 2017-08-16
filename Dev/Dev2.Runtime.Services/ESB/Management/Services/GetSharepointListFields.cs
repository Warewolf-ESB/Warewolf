using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class GetSharepointListFields : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetSharepointListFields";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if(values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            string serializedSource = null;
            string listName = null;
            string editableOnly = null;
            StringBuilder tmp;
            values.TryGetValue("SharepointServer", out tmp);
            if(tmp != null)
            {
                serializedSource = tmp.ToString();
            }
            values.TryGetValue("ListName", out tmp);
            if(tmp != null)
            {
                listName = tmp.ToString();
            }
            values.TryGetValue("OnlyEditable", out tmp);
            if (tmp != null)
            {
                editableOnly = tmp.ToString();
            }
           
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            if(string.IsNullOrEmpty(serializedSource))
            {
                var res = new ExecuteMessage();
                res.HasError = true;
                res.SetMessage(ErrorResource.NoSharepointServerSet);
                Dev2Logger.Debug(ErrorResource.NoSharepointServerSet, "Warewolf Debug");
                return serializer.SerializeToBuilder(res);
            }
            if(string.IsNullOrEmpty(listName))
            {
                var res = new ExecuteMessage();
                res.HasError = true;
                res.SetMessage(ErrorResource.NoSharepointListNameSet);
                Dev2Logger.Debug(ErrorResource.NoSharepointListNameSet, "Warewolf Debug");
                return serializer.SerializeToBuilder(res);
            }
            var editableFieldsOnly = false;
            if(!string.IsNullOrEmpty(editableOnly))
            {
                editableFieldsOnly = serializer.Deserialize<bool>(editableOnly);
            }
            try
            {
                listName = serializer.Deserialize<string>(listName);
                var sharepointSource = serializer.Deserialize<SharepointSource>(serializedSource);
                var source = ResourceCatalog.Instance.GetResource<SharepointSource>(theWorkspace.ID, sharepointSource.ResourceID);
                if (source == null)
                {
                    var contents = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, sharepointSource.ResourceID);
                    source = new SharepointSource(contents.ToXElement());
                }
                List<ISharepointFieldTo> fields = source.LoadFieldsForList(listName, editableFieldsOnly);
                return serializer.SerializeToBuilder(fields);
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex, "Warewolf Error");
                var res = new DbColumnList(ex);
                return serializer.SerializeToBuilder(res);
            }
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;
        }

        #endregion

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}