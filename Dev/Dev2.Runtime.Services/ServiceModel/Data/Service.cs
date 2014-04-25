using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    // DO NOT override ToXml() here!
    public class Service : Resource
    {
        #region CTOR

        public Service()
        {
        }

        public Service(XElement xml)
            : base(xml)
        {
        }

        #endregion

        public ServiceMethod Method { get; set; }
        [JsonIgnore]
        public IOutputDescription OutputDescription { get; set; }
        [JsonIgnore]
        public string OutputSpecification { get; set; }
        public Resource Source { get; set; }

        #region CreateXml

        protected XElement CreateXml(enActionType actionType, Resource source, RecordsetList recordsets, params object[] actionContent)
        {
            return CreateXml(actionType, ResourceName ?? string.Empty, source, recordsets, actionContent);
        }

        protected XElement CreateXml(enActionType actionType, string actionName, Resource source, RecordsetList recordsets, params object[] actionContent)
        {
            var action = new XElement("Action",
                new XAttribute("Name", actionName),
                new XAttribute("Type", actionType),
                new XAttribute("SourceID", source.ResourceID),
                new XAttribute("SourceName", source.ResourceName ?? string.Empty),
                new XAttribute("SourceMethod", Method.Name ?? (ResourceName ?? string.Empty)) // Required for legacy!!
                );

            if(actionContent != null)
            {
                action.Add(actionContent);
            }

            var inputs = CreateInputsXml(Method);
            var outputs = CreateOutputsXml(recordsets);
            action.Add(inputs);
            action.Add(outputs);

            var result = base.ToXml();
            result.AddFirst(
                new XElement("Actions", action),
                new XElement("AuthorRoles"),
                new XElement("Comment"),
                new XElement("Tags"),
                new XElement("HelpLink"),
                new XElement("UnitTestTargetWorkflowService"),
                new XElement("BizRule"),
                new XElement("WorkflowActivityDef"),
                new XElement("XamlDefinition"),
                new XElement("DataList"),
                new XElement("TypeOf", actionType)
                );

            return result;
        }

        #endregion

        #region CreateSource

        protected static T CreateSource<T>(XElement action)
            where T : IResource, new()
        {

            Guid sourceID;
            Guid.TryParse(action.AttributeSafe("SourceID"), out sourceID);
            var result = new T
            {
                ResourceID = sourceID,
                ResourceName = action.AttributeSafe("SourceName")
            };
            return result;
        }

        #endregion

        #region CreateInputsMethod

        // BUG 9626 - 2013.06.11 - TWR : refactored
        // BUG 10532 - Removed static and made public for testing ;)
        public ServiceMethod CreateInputsMethod(XElement action)
        {
            var result = new ServiceMethod { Name = action.AttributeSafe("SourceMethod"), Parameters = new List<MethodParameter>() };
            foreach(var input in action.Descendants("Input"))
            {
                if(!input.HasAttributes && input.IsEmpty)
                {
                    continue;
                }
                XElement validator;
                bool emptyToNull;
                var typeName = input.AttributeSafe("NativeType", true);

                Type tmpType;

                if(string.IsNullOrEmpty(typeName))
                {
                    tmpType = typeof(object);
                }
                else
                {
                    tmpType = TypeExtensions.GetTypeFromSimpleName(typeName);
                }

                // NOTE : Inlining causes debug issues, please avoid ;)
                result.Parameters.Add(new MethodParameter
                {
                    Name = input.AttributeSafe("Name"),
                    EmptyToNull = bool.TryParse(input.AttributeSafe("EmptyToNull"), out emptyToNull) && emptyToNull,
                    IsRequired = (validator = input.Element("Validator")) != null && validator.AttributeSafe("Type").Equals("Required", StringComparison.InvariantCultureIgnoreCase),
                    DefaultValue = input.AttributeSafe("DefaultValue"),
                    Type = tmpType
                });
            }
            return result;
        }

        #endregion

        #region CreateInputsXml

        // BUG 9626 - 2013.06.11 - TWR : refactored
        static XElement CreateInputsXml(ServiceMethod method)
        {
            var inputs = new XElement("Inputs");

            foreach(var parameter in method.Parameters)
            {
                var input = new XElement("Input",
                    new XAttribute("Name", parameter.Name ?? string.Empty),
                    new XAttribute("Source", parameter.Name ?? string.Empty),
                    new XAttribute("EmptyToNull", parameter.EmptyToNull),
                    new XAttribute("DefaultValue", parameter.DefaultValue ?? string.Empty),
                    new XAttribute("NativeType", parameter.TypeName ?? string.Empty)
                    );

                if(parameter.IsRequired)
                {
                    input.Add(new XElement("Validator", new XAttribute("Type", "Required")));
                }
                inputs.Add(input);
            }
            return inputs;
        }

        #endregion

        #region CreateOutputsRecordsetList

        // BUG 9626 - 2013.06.11 - TWR : refactored
        protected RecordsetList CreateOutputsRecordsetList(XElement action)
        {
            var result = new RecordsetList();

            var outputDescriptionStr = action.ElementSafe("OutputDescription");
            var paths = new List<IPath>();
            if(!string.IsNullOrEmpty(outputDescriptionStr))
            {
                var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                var description = outputDescriptionSerializationService.Deserialize(outputDescriptionStr);

                if(description == null)
                {
                    // we need to handle old plugins ;)
                    outputDescriptionStr =
                        outputDescriptionStr.Replace("<JSON />", "")
                                            .Replace("</Dev2XMLResult>", "")
                                            .Replace("</InterrogationResult>", "")
                                            .Replace("<InterrogationResult>", "")
                                            .Replace("<Dev2XMLResult>", "");

                    description = outputDescriptionSerializationService.Deserialize(outputDescriptionStr);
                }

                // TODO : Get Result Coming Back ;)

                OutputDescription = description;

                if(description != null && description.DataSourceShapes.Count > 0)
                {
                    paths = description.DataSourceShapes[0].Paths;
                }
            }
            var xElement = action.Element("Outputs");
            if(xElement != null)
            {
                OutputSpecification = xElement.ToString();
            }
            foreach(var output in action.Descendants("Output"))
            {
                var rsName = output.AttributeSafe("RecordsetName");
                var rsAlias = output.AttributeSafe("Recordset");  // legacy - should be RecordsetAlias
                var fieldName = output.AttributeSafe("OriginalName");
                var fieldAlias = output.AttributeSafe("MapsTo");

                var path = paths.FirstOrDefault(p => output.AttributeSafe("Value").Equals(p.OutputExpression, StringComparison.InvariantCultureIgnoreCase));
                if(path != null)
                {
                    var names = RecordsetListHelper.SplitRecordsetAndFieldNames(path);
                    if(string.IsNullOrEmpty(rsName))
                    {
                        rsName = names.Item1;
                    }
                    if(string.IsNullOrEmpty(fieldName))
                    {
                        fieldName = names.Item2;
                    }
                }

                var recordset = result.FirstOrDefault(r => r.Name == rsName);
                if(recordset == null)
                {
                    recordset = new Recordset { Name = rsName };
                    result.Add(recordset);
                }

                recordset.Fields.Add(new RecordsetField
                {
                    Name = fieldName,
                    Alias = fieldAlias,
                    RecordsetAlias = rsAlias,
                    Path = path
                });
            }

            return result;
        }

        #endregion

        #region CreateOutputsXml

        // BUG 9626 - 2013.06.11 - TWR : refactored
        static IEnumerable<XElement> CreateOutputsXml(IEnumerable<Recordset> recordsets)
        {
            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputs = new XElement("Outputs");

            if(recordsets != null)
            {
                foreach(var recordset in recordsets)
                {
                    var rsName = string.IsNullOrEmpty(recordset.Name) ? "" : recordset.Name.Replace("()", "");

                    foreach(var field in recordset.Fields)
                    {
                        if(String.IsNullOrEmpty(field.Name))
                        {
                            continue;
                        }
                        var path = field.Path;
                        var rsAlias = string.IsNullOrEmpty(field.RecordsetAlias) ? "" : field.RecordsetAlias.Replace("()", "");

                        var value = string.Empty;
                        if(!string.IsNullOrEmpty(field.Alias))
                        {
                            value = string.IsNullOrEmpty(rsAlias)
                                        ? string.Format("[[{0}]]", field.Alias)
                                        : string.Format("[[{0}().{1}]]", rsAlias, field.Alias);
                        }

                        if(path != null)
                        {
                            path.OutputExpression = value;
                            dataSourceShape.Paths.Add(path);
                        }

                        // MapsTo MUST NOT contain recordset name
                        var mapsTo = field.Alias ?? string.Empty;
                        var idx = mapsTo.IndexOf("().", StringComparison.InvariantCultureIgnoreCase);
                        if(idx != -1)
                        {
                            mapsTo = mapsTo.Substring(idx + 3);
                        }


                        var output = new XElement("Output",
                            new XAttribute("OriginalName", field.Name ?? string.Empty),
                            new XAttribute("Name", mapsTo),  // Name MUST be same as MapsTo 
                            new XAttribute("MapsTo", mapsTo),
                            new XAttribute("Value", value),
                            new XAttribute("RecordsetName", rsName),
                            new XAttribute("RecordsetAlias", rsAlias),
                            new XAttribute("Recordset", rsAlias)  // legacy - used by LanguageParser._recordSetAttribute and hard-coded in our tests
                            );
                        outputs.Add(output);
                    }
                }
            }

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            var description = new XElement("OutputDescription", new XCData(serializedOutputDescription));

            return new[] { outputs, description };
        }

        public string GetOutputString(IEnumerable<Recordset> recordsets)
        {
            IEnumerable<XElement> outputsXml = CreateOutputsXml(recordsets);
            return outputsXml.ToList()[0].ToString();
        }

        #endregion


    }
}
