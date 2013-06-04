using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class PluginService : Service
    {
        // BUG 9500 - 2013.05.31 - TWR : removed Recordset property
        public RecordsetList Recordsets { get; set; }
        public PluginSource Source { get; set; }

        // BUG 9500 - 2013.05.31 - TWR : added
        public string Namespace { get; set; }

        #region CTOR

        public PluginService()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.PluginService;
            Source = new PluginSource();
            Recordsets = new RecordsetList();
            Method = new ServiceMethod();
        }

        public PluginService(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.PluginService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            // BUG 9500 - 2013.05.31 - TWR : added
            Namespace = action.AttributeSafe("Namespace");

            #region Parse Source

            Guid sourceID;
            Guid.TryParse(action.AttributeSafe("SourceID"), out sourceID);
            Source = new PluginSource
            {
                ResourceID = sourceID,
                ResourceName = action.AttributeSafe("SourceName")
            };

            #endregion

            #region Parse Method

            Method = new ServiceMethod { Name = action.AttributeSafe("SourceMethod"), Parameters = new List<MethodParameter>() };
            foreach(var input in action.Descendants("Input"))
            {
                XElement validator;
                bool emptyToNull;
                Method.Parameters.Add(new MethodParameter
                {
                    Name = input.AttributeSafe("Name"),
                    EmptyToNull = bool.TryParse(input.AttributeSafe("EmptyToNull"), out emptyToNull) && emptyToNull,
                    IsRequired = (validator = input.Element("Validator")) != null && validator.AttributeSafe("Type").Equals("Required", StringComparison.InvariantCultureIgnoreCase),
                    DefaultValue = input.AttributeSafe("DefaultValue")
                });
            }

            #endregion

            var outputDescriptionStr = action.ElementSafe("OutputDescription");
            var paths = new List<IPath>();
            if(!string.IsNullOrEmpty(outputDescriptionStr))
            {
                var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                var description = outputDescriptionSerializationService.Deserialize(outputDescriptionStr);
                if(description.DataSourceShapes.Count > 0)
                {
                    paths = description.DataSourceShapes[0].Paths;
                }
            }

            // BUG 9500 - 2013.05.31 - TWR : replaced
            #region Parse Recordsets

            Recordsets = new RecordsetList();
            foreach(var output in action.Descendants("Output"))
            {
                var name = output.AttributeSafe("Recordset");

                var recordset = Recordsets.FirstOrDefault(r => r.Name == name);
                if(recordset == null)
                {
                    recordset = new Recordset { Name = name };
                    Recordsets.Add(recordset);
                }

                recordset.Fields.Add(new RecordsetField
                {
                    Name = output.AttributeSafe("Name"),
                    Alias = output.AttributeSafe("MapsTo"),
                    Path = paths.FirstOrDefault(p => output.AttributeSafe("Value").Equals(p.OutputExpression, StringComparison.InvariantCultureIgnoreCase))
                });
            }

            #endregion
        }

        #endregion

        #region ToXml

        // BUG 9500 - 2013.05.31 - TWR : refactored
        public override XElement ToXml()
        {
            var inputs = new XElement("Inputs");
            var outputs = new XElement("Outputs");

            #region Create output description and add recordset fields to outputs

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            if(Recordsets != null)
            {
                foreach(var recordset in Recordsets)
                {
                    foreach(var field in recordset.Fields)
                    {
                        var path = field.Path;

                        if(path != null)
                        {
                            path.OutputExpression = string.Format("[[{0}]]", field.Alias);
                            dataSourceShape.Paths.Add(path);
                        }

                        var output = new XElement("Output",
                            new XAttribute("Name", field.Name ?? string.Empty),
                            new XAttribute("MapsTo", field.Alias ?? string.Empty),
                            new XAttribute("Value", string.IsNullOrEmpty(field.Alias) ? "" : "[[" + field.Alias + "]]"),
                            new XAttribute("Recordset", string.IsNullOrEmpty(recordset.Name) ? "" : recordset.Name.Replace("()", ""))
                            );
                        outputs.Add(output);
                    }
                }
            }

            #endregion

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            #region Add method parameters to inputs

            foreach(var parameter in Method.Parameters)
            {
                var input = new XElement("Input",
                    new XAttribute("Name", parameter.Name ?? string.Empty),
                    new XAttribute("Source", parameter.Name ?? string.Empty),
                    new XAttribute("EmptyToNull", parameter.EmptyToNull),
                    new XAttribute("DefaultValue", parameter.DefaultValue ?? string.Empty)
                    );

                if(parameter.IsRequired)
                {
                    input.Add(new XElement("Validator", new XAttribute("Type", "Required")));
                }
                inputs.Add(input);
            }

            #endregion

            const enActionType ActionType = enActionType.Plugin;

            var result = base.ToXml();
            result.AddFirst(
                new XElement("Actions",
                    new XElement("Action",
                        new XAttribute("Name", ResourceName ?? string.Empty),
                        new XAttribute("Type", ActionType),
                        new XAttribute("SourceID", Source.ResourceID),
                        new XAttribute("SourceName", Source.ResourceName ?? string.Empty),
                        new XAttribute("SourceMethod", Method.Name ?? (ResourceName ?? string.Empty)), // Required for legacy!!
                        new XAttribute("Namespace", Namespace ?? string.Empty),
                        inputs,
                        outputs,
                        new XElement("OutputDescription", new XCData(serializedOutputDescription)))),
                new XElement("AuthorRoles"),
                new XElement("Comment"),
                new XElement("Tags"),
                new XElement("HelpLink"),
                new XElement("UnitTestTargetWorkflowService"),
                new XElement("BizRule"),
                new XElement("WorkflowActivityDef"),
                new XElement("XamlDefinition"),
                new XElement("DataList"),
                new XElement("TypeOf", ActionType)
                );

            return result;
        }

        #endregion
    }
}