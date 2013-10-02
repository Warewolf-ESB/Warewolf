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
    // NOTE : Static instance causing memory leaks
    public class DynamicObjectHelper
    {

        #region Generate an object graph from the domain specific language string for the DSF
        /// <summary>
        /// Generates the object graph from string.
        /// </summary>
        /// <param name="serviceDefinitionsXml">The service definitions XML.</param>
        /// <returns></returns>
        public List<DynamicServiceObjectBase> GenerateObjectGraphFromString(string serviceDefinitionsXml)
        {
            Exceptions.ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString("serviceDefinitionXml", serviceDefinitionsXml);

            //This will store the return data of this method
            //which represents the services that were successfully loaded
            List<DynamicServiceObjectBase> objectsLoaded = new List<DynamicServiceObjectBase>();

            dynamic dslObject = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(serviceDefinitionsXml);

            #region Create MetatData about this resource
            string authorRoles = string.Empty;
            string comment = string.Empty;
            string helpLink = null;
            string unitTestTarget = string.Empty;
            string category = string.Empty;
            string tags = string.Empty;
            string dataList = string.Empty;

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
                        ServerLogger.LogError(ex);
                    }

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

                    if (Actions is List<UnlimitedObject>)
                    {

                        foreach (dynamic action in Actions)
                        {
                            dynamic sa = AddServiceAction(action, ds);

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

        public ServiceAction AddServiceAction(dynamic action, DynamicService ds)
        {
            ServiceAction sa = new ServiceAction();
            if(action.Type is string)
            {
                DynamicServices.enActionType actionType;

                if(Enum.TryParse<DynamicServices.enActionType>(action.Type, out actionType))
                {
                    sa.ActionType = actionType;

                    if(action.Name is string)
                    {
                        sa.Name = action.Name;
                    }

                    if(action.OutputDescription is string)
                    {
                        sa.OutputDescription = action.OutputDescription;
                    }

                    // Attach ServiceAction outputs and inputs
                    if(action.Outputs is UnlimitedObject)
                    {
                        var outputs = action.Outputs;
                        if(!(outputs is string))
                        {
                            sa.OutputSpecification = outputs.XmlString;
                        }
                    }

                    sa.Parent = action.Parent;

                    if(action.SourceName is string)
                    {
                        sa.SourceName = action.SourceName;
                        sa.PluginDomain = AppDomain.CreateDomain(action.SourceName);
                    }

                    if(action.SourceMethod is string)
                    {
                        sa.SourceMethod = action.SourceMethod;
                    }


                    switch(sa.ActionType)
                    {


                        case DynamicServices.enActionType.InvokeDynamicService:
                        case DynamicServices.enActionType.InvokeManagementDynamicService:
                        case DynamicServices.enActionType.InvokeServiceMethod:
                        case DynamicServices.enActionType.InvokeWebService:
                        case DynamicServices.enActionType.Plugin:
                            break;

                        case DynamicServices.enActionType.InvokeStoredProc:
                            if(action.CommandTimeout is string)
                            {
                                int timeout = 30;
                                int.TryParse(action.CommandTimeout, out timeout);

                                sa.CommandTimeout = timeout;
                            }
                            break;

                        case DynamicServices.enActionType.Workflow:
                            if(action.XamlDefinition is string)
                            {
                                sa.XamlDefinition = action.XamlDefinition;
                                sa.ServiceName = ds.Name;
                            }
                            break;

                        default:
                            break;
                    }

                    if(action.ResultsToClient is string)
                    {
                        sa.ResultsToClient = bool.Parse(action.ResultsToClient);
                    }

                    if(action.ServiceName is string)
                    {
                        sa.ServiceName = action.ServiceName;
                    }

                    if(action.TerminateServiceOnFault is string)
                    {
                        sa.TerminateServiceOnFault = bool.Parse(action.TerminateServiceOnFault);
                    }

                    ds.Actions.Add(sa);
                }
            }
            return sa;
        }

        #endregion

        #region SetID

        void SetID(IDynamicServiceObject dso, dynamic resource)
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
