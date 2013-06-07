using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Unlimited.Framework;

namespace Dev2.Runtime.Hosting
{
    public static class DynamicObjectHelper
    {

        #region Generate an object graph from the domain specific language string for the DSF
        /// <summary>
        /// Generates and object graph for each type contained in the domain specific xml language
        /// </summary>
        /// <param name="serviceDefinitionsXml">The string containing the domain specific language code</param>
        /// <returns>List<ServiceObjectBase> containing all object graphs that were built </returns>
        public static List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("serviceDefinitionXml", serviceDefinitionsXml);

            //This will store the return data of this method
            //which represents the services that were successfully loaded
            List<DynamicServiceObjectBase> objectsLoaded = new List<DynamicServiceObjectBase>();

            dynamic dslObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(serviceDefinitionsXml);

            #region Create MetatData about this resource
            string authorRoles = string.Empty;
            string comment = string.Empty;
            string helpLink = null;
            string unitTestTarget = string.Empty;
            string category = string.Empty;
            string tags = string.Empty;
            string dataList = string.Empty;
            string inputMapping = string.Empty;
            string outputMapping = string.Empty;

            if (dslObject.AuthorRoles is string)
            {
                authorRoles = dslObject.AuthorRoles;
            }

            if (dslObject.Comment is string)
            {
                comment = dslObject.Comment;
            }

            if (dslObject.Category is string)
            {
                category = dslObject.Category;
            }

            if (dslObject.Tags is string)
            {
                tags = dslObject.Tags;
            }

            if (dslObject.UnitTestTargetWorkflowService is string)
            {
                unitTestTarget = dslObject.UnitTestTargetWorkflowService;
            }

            if (dslObject.HelpLink is string)
            {
                if (!string.IsNullOrEmpty(dslObject.HelpLink))
                {
                    if (Uri.IsWellFormedUriString(dslObject.HelpLink, UriKind.RelativeOrAbsolute))
                    {
                        helpLink = dslObject.HelpLink;
                    }
                }
            }

            // Travis Added for Data List
            if (dslObject.DataList != null)
            {
                // Try..catch refactored out by Michael (Verified by Travis)
                if ((dslObject.DataList).GetType() == typeof(UnlimitedObject))
                {
                    dataList = dslObject.DataList.XmlString;
                }
                else
                {
                    dataList = "<ADL></ADL>";
                }
                /*
                try
                {
                    dataList = dslObject.DataList.XmlString;
                }
                catch (Exception)
                {
                    // nothing, init it as such
                    dataList = "<ADL></ADL>";
                }
                 */
            }


            #endregion

            #region Create and Hydrate BizRules then add them to the service directory
            //Retrieve a list of UnlimitedObjects that 
            //each contain an individual BizRule node from the
            //Service Definition file
            dynamic BizRules = dslObject.BizRule;
            //We check if a list is being returned as this
            //will be the case when loading complex xml 
            //elements that have attributes
            //as in the case of the service definition file
            //All classes are hydrated in this way so these
            //comments will not be repeated later on in this source file
            if (BizRules is List<UnlimitedObject>)
            {
                //Iterate the bizrule collection of UnlimitedObjects and 
                //Hydrate an instance of the BizRule class each time
                foreach (dynamic bizrule in BizRules)
                {
                    BizRule br = new BizRule();
                    br.Name = bizrule.Name;
                    br.ServiceName = bizrule.ServiceName;
                    br.Expression = bizrule.Expression;
                    br.ExpressionColumns = bizrule.ExpressionColumns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //Add the newly instantiated and Hydrated bizrule class
                    //to the BizRules list of the DynamicServices service directory
                    //this.BizRules.Add(br);
                    objectsLoaded.Add(br);
                    ServerLogger.LogTrace(string.Format("successfully parsed biz rule '{0}'", br.Name));
                }
            }
            #endregion

            #region Create and Hydrate Workflow ActivityMetaData
            dynamic activities = dslObject.WorkflowActivityDef;
            if (activities is List<UnlimitedObject>)
            {
                foreach (dynamic item in activities)
                {
                    WorkflowActivityDef wd = new WorkflowActivityDef();
                    wd.AuthorRoles = authorRoles;
                    wd.Comment = comment;
                    wd.Tags = tags;
                    wd.Category = category;
                    wd.HelpLink = helpLink;
                    wd.ResourceDefinition = item.XmlString;
                    wd.DataListSpecification = dataList;

                    if (item.ServiceName is string)
                    {
                        wd.ServiceName = item.ServiceName;
                    }
                    if (item.Name is string)
                    {
                        wd.Name = item.Name;
                    }
                    if (item.IconPath is string)
                    {
                        wd.IconPath = item.IconPath;
                    }
                    if (item.DataTags is string)
                    {
                        wd.DataTags = item.DataTags;
                    }
                    if (item.DeferExecution is string)
                    {
                        bool defer = false;
                        bool.TryParse(item.DeferExecution, out defer);
                        wd.DeferExecution = defer;
                    }
                    if (item.ResultValidationExpression is string)
                    {
                        wd.ResultValidationExpression = item.ResultValidationExpression;
                    }
                    if (item.ResultValidationRequiredTags is string)
                    {
                        wd.ResultValidationRequiredTags = item.ResultValidationRequiredTags;
                    }
                    if (item.AuthorRoles is string)
                    {
                        wd.AuthorRoles = item.AuthorRoles;
                    }
                    if (item.AdminRoles is string)
                    {
                        wd.AdminRoles = item.AdminRoles;
                    }

                    objectsLoaded.Add(wd);
                }

            }

            #endregion

            #region Create and Hydrate Sources then add them to the service directory
            //Retrieve a list of UnlimitedObjects that 
            //each contain in individual Source node from the
            //Service Definition file
            dynamic sources = dslObject.Source;
            if (sources is List<UnlimitedObject>)
            {
                foreach (dynamic source in sources)
                {
                    bool dlCheck = false;

                    try
                    {
                        //XmlDocument xDoc = new XmlDocument();
                        //xDoc.LoadXml("<x>" + (source as UnlimitedObject).XmlString + "</x>");
                        //XmlNodeList nl = xDoc.GetElementsByTagName("Action");
                        //if (nl.Count > 0) {
                        //dlCheck = true;
                        //}
                        if (source.Type is string)
                        {

                            enSourceType sourceType;
                            if (!Enum.TryParse<enSourceType>(source.Type, out sourceType))
                            {
                                dlCheck = false;
                            }
                            else
                            {
                                dlCheck = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        ServerLogger.LogError(ex);
                    }

                    //(source as UnlimitedObject).xmlData.HasElements;

                    if (dlCheck)
                    { // Travis : filter out the Source in DataList issue
                        Source src = new Source();
                        src.AuthorRoles = authorRoles;
                        src.Tags = tags;
                        src.Comment = comment;
                        src.Category = category;
                        src.HelpLink = helpLink;
                        src.ResourceDefinition = source.XmlString;
                        src.DataListSpecification = dataList;

                        if (source.Name is string)
                        {
                            src.Name = source.Name;
                        }

                        if (source.Type is string)
                        {
                            enSourceType sourceType;
                            if (!Enum.TryParse<enSourceType>(source.Type, out sourceType))
                            {
                                src.Type = enSourceType.Unknown;
                            }
                            else
                            {
                                src.Type = sourceType;
                            }
                        }

                        if (source.ConnectionString is string)
                        {
                            if (!string.IsNullOrEmpty(source.ConnectionString))
                            {
                                src.ConnectionString = source.ConnectionString;
                            }
                        }

                        if (source.Uri is string)
                        {
                            if (!string.IsNullOrEmpty(source.Uri))
                            {
                                src.WebServiceUri = new Uri(source.Uri);
                            }
                        }

                        if (source.AssemblyName is string)
                        {
                            if (!string.IsNullOrEmpty(source.AssemblyName))
                            {
                                src.AssemblyName = source.AssemblyName;
                            }
                        }

                        if (source.AssemblyLocation is string)
                        {
                            if (!string.IsNullOrEmpty(source.AssemblyLocation))
                            {
                                src.AssemblyLocation = source.AssemblyLocation;
                            }
                        }

                        // PBI 6597: TWR - added source ID check
                        SetID(src, source);

                        objectsLoaded.Add(src);
                    }
                }
            }
            #endregion

            #region Build an object graph for each service in the domain specific language string
            dynamic services = dslObject.Service;
            if (services is List<UnlimitedObject>)
            {
                foreach (dynamic service in services)
                {
                    DynamicService ds = new DynamicService();
                    ds.AuthorRoles = authorRoles;
                    ds.Category = category;
                    ds.Tags = tags;
                    ds.Comment = comment;
                    ds.HelpLink = helpLink;
                    ds.ResourceDefinition = service.XmlString;
                    ds.UnitTestTargetWorkflowService = unitTestTarget;
                    ds.DataListSpecification = dataList;

                    if (service.IconPath is string)
                    {
                        ds.IconPath = service.IconPath;
                    }

                    if (service.DisplayName is string)
                    {
                        ds.DisplayName = service.DisplayName;
                    }

                    if (service.Name is string)
                    {
                        ds.Name = service.Name;
                    }
                    else
                    {
                        // Travis : we have a DataList clash between this "dynamic" property and the DataList XML when Name is an element
                        UnlimitedObject tmpObj = (service as UnlimitedObject);

                        try
                        {
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(tmpObj.XmlString);
                            XmlNode n = xDoc.SelectSingleNode("Service");
                            ds.Name = n.Attributes["Name"].Value;
                        }
                        catch (Exception ex)
                        {
                            ServerLogger.LogError(ex);
                        }
                    }

                    #region Build Actions
                    dynamic Actions = service.Action;
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                    if (Actions is List<UnlimitedObject>)
                    {

                        foreach (dynamic action in Actions)
                        {
                            #region Process Actions
                            ServiceAction sa = new ServiceAction();

                            if (action.Name is string)
                            {
                                sa.Name = action.Name;
                            }

                            if (action.OutputDescription is string)
                            {
                                sa.OutputDescription = action.OutputDescription;
                            }

                            // Attach ServiceAction outputs and inputs
                            if (action.Outputs is UnlimitedObject)
                            {
                                var outputs = action.Outputs;
                                if (!(outputs is string))
                                {
                                    sa.OutputSpecification = outputs.XmlString;
                                }

                            }

                            // TODO : Build the correct service action inputs ;)

                            //if (action.Input is string)
                            //{
                            //    sa.ServiceActionInputs = 
                            //}

                            sa.Parent = action.Parent;

                            if (action.Type is string)
                            {
                                DynamicServices.enActionType actionType;

                                if (!Enum.TryParse<DynamicServices.enActionType>(action.Type, out actionType))
                                {
                                    sa.ActionType = DynamicServices.enActionType.Unknown;
                                }
                                else
                                {
                                    sa.ActionType = actionType;
                                }
                            }

                            if (action.SourceName is string)
                            {
                                sa.SourceName = action.SourceName;
                                sa.PluginDomain = AppDomain.CreateDomain(action.SourceName);
                            }

                            if (action.SourceMethod is string)
                            {
                                sa.SourceMethod = action.SourceMethod;
                            }

                            //sa.ActionType = Enum.Parse(typeof(enActionType), action.Type);
                            //Biz Rules are special actions 
                            //so we need to treat everything else differently

                            switch (sa.ActionType)
                            {
                                case DynamicServices.enActionType.BizRule:
                                    //sa.BizRuleName = action.BizRuleName;
                                    //var BizRule = from c in this.BizRules
                                    //              where c.Name == sa.BizRuleName
                                    //              select c;

                                    //if (BizRule.Count() > 0) {
                                    //    sa.BizRule = BizRule.First();
                                    //}
                                    break;

                                case DynamicServices.enActionType.InvokeDynamicService:
                                case DynamicServices.enActionType.InvokeManagementDynamicService:
                                case DynamicServices.enActionType.InvokeServiceMethod:
                                case DynamicServices.enActionType.InvokeWebService:
                                case DynamicServices.enActionType.Plugin:
                                    break;

                                case DynamicServices.enActionType.InvokeStoredProc:
                                    if (action.CommandTimeout is string)
                                    {
                                        int timeout = 30;
                                        int.TryParse(action.CommandTimeout, out timeout);

                                        sa.CommandTimeout = timeout;


                                    }
                                    break;

                                case DynamicServices.enActionType.Workflow:
                                    if (action.XamlDefinition is string)
                                    {
                                        sa.XamlDefinition = action.XamlDefinition;
                                        sa.ServiceName = ds.Name;
                                    }
                                    break;

                                default:
                                    break;

                            }

                            if (action.ResultsToClient is string)
                            {
                                sa.ResultsToClient = bool.Parse(action.ResultsToClient);
                            }

                            if (action.ServiceName is string)
                            {
                                sa.ServiceName = action.ServiceName;
                            }



                            if (action.TerminateServiceOnFault is string)
                            {
                                sa.TerminateServiceOnFault = bool.Parse(action.TerminateServiceOnFault);
                            }

                            #endregion

                            ds.Actions.Add(sa);

                            #region Process Inputs for Action
                            dynamic Inputs = action.Input;
                            if (Inputs is List<UnlimitedObject>)
                            {
                                foreach (dynamic input in Inputs)
                                {
                                    ServiceActionInput sai = new ServiceActionInput();
                                    if (input.Name is string)
                                    {
                                        sai.Name = input.Name;
                                    }
                                    if (input.Source is string)
                                    {
                                        sai.Source = input.Source;
                                    }

                                    if (input.DefaultValue is string)
                                    {
                                        sai.DefaultValue = input.DefaultValue;

                                    }

                                    // 16.10.2012 - Travis.Frisinger  : EmptyToNull amendments
                                    if (input.EmptyToNull is string)
                                    {
                                        bool result = false;
                                        Boolean.TryParse(input.EmptyToNull, out result);
                                        sai.EmptyToNull = result;
                                    }
                                    else
                                    {
                                        sai.EmptyToNull = false;
                                    }

                                    // 16.10.2012 - Travis.Frisinger  : EmptyToNull amendments
                                    if (input.NativeType is string)
                                    {
                                        sai.NativeType = input.NativeType;
                                    }
                                    else
                                    {
                                        sai.NativeType = "object";
                                    }

                                    dynamic Validators = input.Validator;

                                    if (Validators is List<UnlimitedObject>)
                                    {

                                        foreach (dynamic validator in Validators)
                                        {
                                            Validator v = new Validator();

                                            if (validator.Type is string)
                                            {
                                                enValidationType validatorType;
                                                if (!Enum.TryParse<enValidationType>(validator.Type, out validatorType))
                                                {
                                                    v.ValidatorType = enValidationType.Required;
                                                }
                                                else
                                                {
                                                    v.ValidatorType = validatorType;
                                                }

                                            }

                                            sai.Validators.Add(v);
                                        }
                                    }
                                    sa.ServiceActionInputs.Add(sai);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion Process Actions


                    // PBI: 801: TWR - added ID check
                    SetID(ds, service);

                    objectsLoaded.Add(ds);
                }
            }
            #endregion

            return objectsLoaded;
        }

        #endregion

        #region SetID

        static void SetID(IDynamicServiceObject dso, dynamic resource)
        {
            Guid id = new Guid();
            UnlimitedObject unlimitedObject = resource as UnlimitedObject;
            if (unlimitedObject != null)
            {
                string xmlString = unlimitedObject.XmlString;
                XElement element = XElement.Parse(xmlString);
                XAttribute idAttribute = element.Attribute("ID");
                if (idAttribute != null)
                {
                    id = Guid.Parse(idAttribute.Value);
                }
            }

            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
                var xml = XElement.Parse(dso.ResourceDefinition);
                xml.Add(new XAttribute("ID", id.ToString()));
                dso.ResourceDefinition = xml.ToString(SaveOptions.DisableFormatting);
            }

            //// TODO: Add ID property to IDynamicServiceObject
            var source = dso as Source;
            if (source != null)
            {
                source.ID = id;
            }
            else
            {
                var ds = dso as DynamicService;
                if (ds != null)
                {
                    ds.ID = id;
                }
            }
        }

        #endregion

    }
}
