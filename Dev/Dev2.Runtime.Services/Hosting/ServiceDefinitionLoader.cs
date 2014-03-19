using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;

namespace Dev2.Runtime.Hosting
{
    #region Service MetaData

    public class ServiceMetaData
    {
        /// <summary>
        /// Extracts the meta data.
        /// </summary>
        /// <param name="xe">The executable.</param>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ServiceMetaData ExtractMetaData(XElement xe, ref DynamicServiceObjectBase obj)
        {
            ServiceMetaData result = new ServiceMetaData();

            var tmp = ExtractValue(xe, "Category");
            obj.Category = tmp;

            tmp = ExtractValue(xe, "DisplayName");
            obj.DisplayName = tmp;

            tmp = ExtractValue(xe, "Comment");
            obj.Comment = tmp;

            tmp = ExtractValue(xe, "IconPath");
            obj.IconPath = tmp;

            tmp = ExtractValue(xe, "HelpLink");
            obj.HelpLink = tmp;

            tmp = ExtractValue(xe, "DataList");
            obj.DataListSpecification = tmp;

            obj.Name = xe.AttributeSafe("Name");

            return result;
        }

        /// <summary>
        /// Extracts the value.
        /// </summary>
        /// <param name="xe">The executable.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns></returns>
        public static string ExtractValue(XElement xe, string elementName)
        {
            var tmp = xe.Element(elementName);

            if(tmp != null)
            {
                var extractValue = tmp.Value;
                return extractValue;
            }

            return string.Empty;
        }

        public static Guid SetID(ref XElement xe)
        {
            Guid id = new Guid();

            var tmpID = xe.AttributeSafe("ID");

            if(!string.IsNullOrEmpty(tmpID))
            {
                Guid.TryParse(tmpID, out id);
            }
            else
            {
                xe.Add(new XAttribute("ID", id.ToString()));
            }

            return id;

        }
    }

    #endregion

    public class ServiceDefinitionLoader
    {
        public List<DynamicServiceObjectBase> GenerateServiceGraph(StringBuilder serviceData)
        {
            if(serviceData == null || serviceData.Length == 0)
            {
                throw new ArgumentException("serviceData");
            }

            List<DynamicServiceObjectBase> result = new List<DynamicServiceObjectBase>();
            var xe = serviceData.ToXElement();

            if(IsSource(serviceData))
            {
                Source src = new Source();
                var tmp = src as DynamicServiceObjectBase;
                ServiceMetaData.ExtractMetaData(xe, ref tmp);

                var typeOf = xe.AttributeSafe("ResourceType");

                enSourceType sourceType;
                if(!Enum.TryParse(typeOf, out sourceType))
                {
                    src.Type = enSourceType.Unknown;
                }
                else
                {
                    src.Type = sourceType;
                }

                src.ConnectionString = xe.AttributeSafe("ConnectionString");
                var tmpUri = xe.AttributeSafe("Uri");
                if(!string.IsNullOrEmpty(tmpUri))
                {
                    src.WebServiceUri = new Uri(tmpUri);
                }

                src.AssemblyName = xe.AttributeSafe("AssemblyName");
                src.AssemblyLocation = xe.AttributeSafe("AssemblyLocation");

                // PBI 6597: TWR - added source ID check
                var id = ServiceMetaData.SetID(ref xe);
                src.ID = id;
                src.ResourceDefinition = serviceData;

                result.Add(src);

            }
            else
            {
                DynamicService ds = new DynamicService();
                var tmp = ds as DynamicServiceObjectBase;
                ServiceMetaData.ExtractMetaData(xe, ref tmp);

                // set the resource def ;)
                ds.ResourceDefinition = serviceData;

                var actions = xe.Element("Actions");
                XElement action;
                if(actions != null)
                {
                    action = actions.Element("Action");
                }
                else
                {
                    action = xe.Element("Action");
                }

                if(action != null)
                {
                    ServiceAction sa = new ServiceAction { Name = action.AttributeSafe("Name"), ResourceDefinition = serviceData };

                    // Set service action ;)
                    enActionType actionType;
                    var typeOf = action.AttributeSafe("Type");
                    if(Enum.TryParse(typeOf, out actionType))
                    {
                        sa.ActionType = actionType;
                    }

                    var element = action.Element("Outputs");
                    if(element != null)
                    {
                        sa.OutputSpecification = element.Value;
                    }

                    // set name and id ;)
                    sa.ServiceName = ds.Name;
                    var id = ServiceMetaData.SetID(ref xe);
                    ds.ID = id;

                    if(IsWorkflow(serviceData))
                    {
                        // Convert to StringBuilder
                        var xElement = action.Element("XamlDefinition");
                        if(xElement != null)
                        {
                            var def = xElement.ToStringBuilder();
                            def = def.Replace("<XamlDefinition>", "").Replace("</XamlDefinition>", "");
                            sa.XamlDefinition = def.Unescape();
                        }

                        var dataList = xe.Element("DataList");
                        if(dataList != null)
                        {
                            ds.DataListSpecification = dataList.ToString();
                        }
                    }
                    else
                    {
                        if(sa.ActionType == enActionType.InvokeStoredProc)
                        {
                            int timeout;
                            Int32.TryParse(action.AttributeSafe("CommandTimeout"), out timeout);
                            sa.CommandTimeout = timeout;
                        }

                        var xElement = action.Element("OutputDescription");
                        if(xElement != null)
                        {
                            sa.OutputDescription = xElement.Value;
                        }

                        // process inputs and outputs ;)
                        var inputs = action.Element("Inputs");

                        if(inputs != null)
                        {
                            var inputCollection = inputs.Elements("Input");

                            foreach(var inputItem in inputCollection)
                            {
                                bool emptyToNull;
                                bool.TryParse(inputItem.AttributeSafe("EmptyToNull"), out emptyToNull);

                                ServiceActionInput sai = new ServiceActionInput
                                {
                                    Name = inputItem.AttributeSafe("Name"),
                                    Source = inputItem.AttributeSafe("Source"),
                                    DefaultValue = inputItem.AttributeSafe("DefaultValue"),
                                    EmptyToNull = emptyToNull,
                                    NativeType = inputItem.AttributeSafe("NativeType")
                                };

                                if(string.IsNullOrEmpty(sai.NativeType))
                                {
                                    sai.NativeType = "object";
                                }

                                // handle validators ;)
                                var validators = inputItem.Elements("Validator");
                                foreach(var validator in validators)
                                {
                                    Validator v = new Validator();

                                    enValidationType validatorType;
                                    if(!Enum.TryParse(validator.AttributeSafe("Type"), out validatorType))
                                    {
                                        v.ValidatorType = enValidationType.Required;
                                    }
                                    else
                                    {
                                        v.ValidatorType = validatorType;
                                    }

                                    sai.Validators.Add(v);
                                }

                                sa.ServiceActionInputs.Add(sai);
                            }
                        }
                    }

                    // add the action
                    ds.Actions.Add(sa);
                    result.Add(ds);
                }

            }

            return result;
        }

        bool IsSource(StringBuilder serviceData)
        {
            return (serviceData.IndexOf("<Source ", 0, false) == 0);
        }

        bool IsWorkflow(StringBuilder serviceData)
        {

            var startIdx = serviceData.IndexOf("<XamlDefinition>", 0, false);
            if(startIdx >= 0)
            {
                var endIdx = serviceData.IndexOf("</XamlDefinition>", startIdx, false);
                var dif = endIdx - startIdx;

                // we know a blank wf is larger then our max string size ;)
                return (startIdx > 0 && dif > GlobalConstants.MAX_SIZE_FOR_STRING);
            }

            return false;
        }
    }
}
