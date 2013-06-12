using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel
{
    public class WebRequestPoco
    {
        public string ResourceType { get; set; }
        public string ResourceID { get; set; }
    }
    public class Services : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public Services()
            : this(ResourceCatalog.Instance)
        {
        }

        public Services(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion

        #region Get

        // POST: Service/Services/Get
        public Service Get(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var webRequestPoco = JsonConvert.DeserializeObject<WebRequestPoco>(args);
                //dynamic argsObj = JObject.Parse(args);
                //string resourceTypeStr = argsObj.resourceType.Value;
                string resourceTypeStr = webRequestPoco.ResourceType;
                var resourceType = Resources.ParseResourceType(resourceTypeStr);
                //string resourceID = argsObj.resourceID.Value;
                string resourceID = webRequestPoco.ResourceID;
                var xmlStr = Resources.ReadXml(workspaceID, resourceType, resourceID);
                var xml = string.IsNullOrEmpty(xmlStr) ? null : XElement.Parse(xmlStr);
                return DeserializeService(xml, resourceType);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return DbService.Create();
        }

        #endregion

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                Service service = DeserializeService(args);
                _resourceCatalog.SaveResource(workspaceID, service);
                if(workspaceID != GlobalConstants.ServerWorkspaceID)
                {
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, service);
                }

                return service.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region DbMethods

        // POST: Service/Services/DbMethods
        public ServiceMethodList DbMethods(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ServiceMethodList();
            if(!string.IsNullOrEmpty(args))
            {
                try
                {
                    var source = JsonConvert.DeserializeObject<DbSource>(args);
                    var serviceMethods = FetchMethods(source);
                    result.AddRange(serviceMethods);
                }
                catch(Exception ex)
                {
                    RaiseError(ex);
                    result.Add(new ServiceMethod(ex.Message, ex.StackTrace));
                }
            }
            return result;
        }

        #endregion

        #region DbTest

        // POST: Service/Services/DbTest
        public Recordset DbTest(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<DbService>(args);

                if(string.IsNullOrEmpty(service.Recordset.Name))
                {
                    service.Recordset.Name = service.Method.Name;
                }

                var addFields = service.Recordset.Fields.Count == 0;
                if(addFields)
                {
                    service.Recordset.Fields.Clear();
                }
                service.Recordset.Records.Clear();

                return FetchRecordset(service, addFields);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new Recordset { HasErrors = true, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region FetchRecordset

        public virtual Recordset FetchRecordset(DbService dbService, bool addFields)
        {
            if(dbService == null)
            {
                throw new ArgumentNullException("dbService");
            }

            var broker = new MsSqlBroker();
            var outputDescription = broker.TestService(dbService);

            if(outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            // Clear out the Recordset.Fields list because the sequence and
            // number of fields may have changed since the last invocation.
            //
            // Create a copy of the Recordset.Fields list before clearing it
            // so that we don't lose the user-defined aliases.
            //
            var rsFields = new List<RecordsetField>(dbService.Recordset.Fields);
            dbService.Recordset.Fields.Clear();
            dbService.Recordset.Name = dbService.Recordset.Name.Replace(".", "_");

            for(var i = 0; i < outputDescription.DataSourceShapes[0].Paths.Count; i++)
            {
                var path = outputDescription.DataSourceShapes[0].Paths[i];
                //if(string.IsNullOrEmpty(path.SampleData))
                //{
                //    continue;
                //}

                // Remove bogus names and dots
                var name = path.DisplayPath.Replace("NewDataSet", "").Replace(".Table.", "").Replace(".", "");

                #region Remove recordset name if present

                var idx = name.IndexOf("()", StringComparison.InvariantCultureIgnoreCase);
                if(idx >= 0)
                {
                    name = name.Remove(0, idx + 2);
                }

                #endregion

                var field = new RecordsetField { Name = name, Alias = string.IsNullOrEmpty(path.OutputExpression) ? name : path.OutputExpression, Path = path };

                RecordsetField rsField;
                if(!addFields && (rsField = rsFields.FirstOrDefault(f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }
                dbService.Recordset.Fields.Add(field);

                var data = path.SampleData.Split(',');
                for(var recordIndex = 0; recordIndex < data.Length; recordIndex++)
                {
                    dbService.Recordset.SetValue(recordIndex, i, data[recordIndex]);
                }
            }

            return dbService.Recordset;
        }

        public virtual RecordsetList FetchRecordset(PluginService pluginService, bool addFields)
        {
            if(pluginService == null)
            {
                throw new ArgumentNullException("pluginService");
            }
            var broker = new PluginBroker();
            var outputDescription = broker.TestPlugin(pluginService);
            return CreateRecordsets(pluginService.Recordsets, addFields, outputDescription);
        }

        public virtual RecordsetList FetchRecordset(WebService webService, bool addFields)
        {
            if(webService == null)
            {
                throw new ArgumentNullException("webService");
            }

            var outputDescription = webService.GetOutputDescription();
            return CreateRecordsets(webService.Recordsets, addFields, outputDescription);
        }

        #endregion

        #region FetchMethods

        public virtual ServiceMethodList FetchMethods(DbSource dbSource)
        {
            var broker = new MsSqlBroker();
            return broker.GetServiceMethods(dbSource);
        }

        #endregion

        #region DeserializeService

        protected virtual Service DeserializeService(string args)
        {
            var service = JsonConvert.DeserializeObject<Service>(args);
            switch(service.ResourceType)
            {
                case ResourceType.DbService:
                    return JsonConvert.DeserializeObject<DbService>(args);
            }
            return service;
        }

        protected virtual Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            if(xml != null)
            {
                switch(resourceType)
                {
                    case ResourceType.DbService:
                        return new DbService(xml);
                }
            }
            else
            {
                switch(resourceType)
                {
                    case ResourceType.DbService:
                        return DbService.Create();
                }
            }
            return null;
        }

        #endregion

        #region BuildNodesBasedOnPath

        static void BuildNodesBasedOnPath(string path, Node root, IPath thePath)
        {
            var actualPath = path;
            if(actualPath.Contains('.'))
            {
                var indexOf = actualPath.IndexOf('.');
                var subString = actualPath.Substring(indexOf + 1);
                var parentBit = actualPath.Substring(0, indexOf);
                if(subString.Contains('.'))
                {
                    BuildNodesBasedOnPath(subString, root, thePath);
                }
                else
                {
                    var firstOrDefault = root.ChildNodes.FirstOrDefault(node => node.Name == parentBit);
                    if(firstOrDefault == null)
                    {
                        var item = new Node { Name = parentBit };
                        item.MyProps.Add(subString, thePath);
                        root.ChildNodes.Add(item);
                    }
                    else
                    {
                        firstOrDefault.MyProps.Add(subString, thePath);
                    }
                }
            }
            else
            {
                root.MyProps.Add(actualPath, thePath);
            }

        }

        #endregion

        #region BuildRecordset

        static void BuildRecordset(Recordset recordset, Node root, bool addFields = true, List<RecordsetField> rsFields = null)
        {
            var index = 0;
            foreach(var prop in root.MyProps)
            {
                var name = prop.Key;
                var aliasValue = prop.Key;
                if(!String.IsNullOrEmpty(root.Name))
                {
                    aliasValue = root.Name.Contains("()") ? root.Name + "." + prop.Key : root.Name + prop.Key;
                }
                var path = prop.Value;
                //Alias = string.IsNullOrEmpty(path.DisplayPath) ? name : path.DisplayPath
                var field = new RecordsetField { Name = name, Alias = aliasValue, Path = path };
                RecordsetField rsField;
                if(!addFields && rsFields != null && (rsField = rsFields.FirstOrDefault(f => f.Path != null ? f.Path.ActualPath == path.ActualPath : f.Name == field.Name)) != null)
                {
                    field.Alias = rsField.Alias;
                }
                recordset.Fields.Add(field);
                var data = path.SampleData.Split(',');
                for(var recordIndex = 0; recordIndex < data.Length; recordIndex++)
                {
                    recordset.SetValue(recordIndex, index, data[recordIndex]);
                }
                index++;
            }
        }

        #endregion

        #region CreateRecordsets

        static RecordsetList CreateRecordsets(RecordsetList serviceRecordsets, bool addFields, IOutputDescription outputDescription)
        {
            if(outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            var result = serviceRecordsets ?? new RecordsetList();

            var rsFields = new List<RecordsetField>();

            if(result.Count == 0)
            {
                result.Add(new Recordset());
            }
            else
            {
                //
                // Create a copy of the Recordset.Fields list before clearing it
                // so that we don't lose the user-defined aliases.
                //
                foreach(var rs in result)
                {
                    rsFields.AddRange(rs.Fields);
                    rs.Fields.Clear();
                    if(!String.IsNullOrEmpty(rs.Name))
                    {
                        rs.Name = rs.Name.Replace(".", "_");
                    }
                }
            }

            var dataSourceShape = outputDescription.DataSourceShapes[0];

            var paths = dataSourceShape.Paths;
            var root = new Node();
            paths.ForEach(path => BuildNodesBasedOnPath(path.ActualPath, root, path));

            var recordset = result.FirstOrDefault(rs => String.IsNullOrEmpty(rs.Name));
            if(recordset != null)
            {
                BuildRecordset(recordset, root, addFields, rsFields);
            }

            root.ChildNodes.ForEach(node =>
            {
                var name = node.Name;
                recordset = result.FirstOrDefault(rs => rs.Name == name);
                if(recordset == null)
                {
                    recordset = new Recordset { Name = name };
                    result.Add(recordset);
                }
                BuildRecordset(recordset, node, addFields, rsFields);
            });
            return result;
        }

        #endregion

    }

    internal class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Node()
        {
            MyProps = new Dictionary<string, IPath>();
            ChildNodes = new List<Node>();
        }

        public string Name { get; set; }
        public Dictionary<string, IPath> MyProps { get; set; }
        public List<Node> ChildNodes { get; set; }
    }

}
