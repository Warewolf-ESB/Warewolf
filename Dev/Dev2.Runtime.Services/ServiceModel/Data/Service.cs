#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph;


namespace Dev2.Runtime.ServiceModel.Data
{
    // DO NOT override ToXml() here!
    public class Service : Resource
    {
        IOutputDescription _outputDescription;

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
        public IOutputDescription OutputDescription
        {
            get
            {
                return _outputDescription;
            }
            set
            {
                _outputDescription = value;
            }
        }
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
            VerifyArgument.IsNotNull("source", source);
            var action = new XElement("Action",
                new XAttribute("Name", actionName),
                new XAttribute("Type", actionType),
                new XAttribute("SourceID", source.ResourceID),
                new XAttribute("SourceName", source.ResourceName ?? string.Empty),
                new XAttribute("ExecuteAction", Method.ExecuteAction ?? string.Empty),
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

            Guid.TryParse(action.AttributeSafe("SourceID"), out Guid sourceId);
            var result = new T
            {
                ResourceID = sourceId,
                ResourceName = action.AttributeSafe("SourceName")
            };
            return result;
        }

        #endregion

        #region CreateInputsMethod

        public ServiceMethod CreateInputsMethod(XElement action)
        {
            var result = new ServiceMethod { Name = action.AttributeSafe("SourceMethod"), Parameters = new List<MethodParameter>(), ExecuteAction = String.IsNullOrEmpty(action.AttributeSafe("ExecuteAction")) ? action.AttributeSafe("SourceMethod") : action.AttributeSafe("ExecuteAction") };
            foreach(var input in action.Descendants("Input"))
            {
                if(!input.HasAttributes && input.IsEmpty)
                {
                    continue;
                }
                var typeName = input.AttributeSafe("NativeType", true);

                Type tmpType = string.IsNullOrEmpty(typeName) ? typeof(object) : TypeExtensions.GetTypeFromSimpleName(typeName);

                XElement validator = input.Element("Validator");
                // NOTE : Inlining causes debug issues, please avoid ;)
                result.Parameters.Add(new MethodParameter
                {
                    Name = input.AttributeSafe("Name"),
                    EmptyToNull = bool.TryParse(input.AttributeSafe("EmptyToNull"), out bool emptyToNull) && emptyToNull,
                    IsRequired = validator != null && validator.AttributeSafe("Type").Equals("Required", StringComparison.InvariantCultureIgnoreCase),
                    DefaultValue = input.AttributeSafe("DefaultValue"),
                    TypeName = tmpType.FullName
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
                    outputDescriptionStr =
                        outputDescriptionStr.Replace("<JSON />", "")
                                            .Replace("</Dev2XMLResult>", "")
                                            .Replace("</InterrogationResult>", "")
                                            .Replace("<InterrogationResult>", "")
                                            .Replace("<Dev2XMLResult>", "");

                    description = outputDescriptionSerializationService.Deserialize(outputDescriptionStr);
                }

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
                var rsAlias = output.AttributeSafe("Recordset");
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
        
        public IEnumerable<XElement> CreateOutputsXml(IEnumerable<Recordset> recordsets)
        {
            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            var outputs = new XElement("Outputs");

            if(recordsets != null)
            {
                outputs = CreateOutputsXml(recordsets, dataSourceShape, outputs);
            }

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            var description = new XElement("OutputDescription", new XCData(serializedOutputDescription));

            return new[] { outputs, description };
        }

        static XElement CreateOutputsXml(IEnumerable<Recordset> recordsets, IDataSourceShape dataSourceShape, XElement outputs)
        {
            foreach (var recordset in recordsets)
            {
                var rsName = string.IsNullOrEmpty(recordset.Name) ? "" : recordset.Name.Replace("()", "");

                foreach (var field in recordset.Fields)
                {
                    if (String.IsNullOrEmpty(field.Name))
                    {
                        continue;
                    }
                    var path = field.Path;
                    var rsAlias = string.IsNullOrEmpty(field.RecordsetAlias) ? "" : field.RecordsetAlias.Replace("()", "");

                    var value = string.Empty;
                    if (!string.IsNullOrEmpty(field.Alias))
                    {
                        value = string.IsNullOrEmpty(rsAlias)
                                    ? string.Format("[[{0}]]", field.Alias)
                                    : string.Format("[[{0}().{1}]]", rsAlias, field.Alias);
                    }

                    if (path != null)
                    {
                        path.OutputExpression = value;
                        dataSourceShape.Paths.Add(path);
                    }

                    // MapsTo MUST NOT contain recordset name
                    var mapsTo = field.Alias ?? string.Empty;
                    var idx = mapsTo.IndexOf("().", StringComparison.InvariantCultureIgnoreCase);
                    if (idx != -1)
                    {
                        mapsTo = mapsTo.Substring(idx + 3);
                    }


                    var output = new XElement("Output",
                        new XAttribute("OriginalName", field.Name),
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
            return outputs;
        }

        #endregion
    }
}
