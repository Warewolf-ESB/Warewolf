using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbService : Service
    {
        #region CTOR

        public DbService()
        {
            ResourceType = ResourceType.DbService;
        }

        public DbService(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.DbService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            Guid sourceID;
            Guid.TryParse(action.AttributeSafe("SourceID"), out sourceID);
            Source = new DbSource
            {
                ResourceID = sourceID,
                ResourceName = action.AttributeSafe("SourceName")
            };

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

            Recordset = new Recordset { Name = action.AttributeSafe("Name") };
            foreach(var output in action.Descendants("Output"))
            {
                Recordset.Fields.Add(new RecordsetField
                {
                    Name = output.AttributeSafe("Name"),
                    Alias = output.AttributeSafe("MapsTo")
                });
            }
            if(Recordset.Name == ResourceName)
            {
                Recordset.Name = Method.Name;
            }
        }

        #endregion

        public DbSource Source { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            var isRecordset = !string.IsNullOrEmpty(Recordset.Name);

            #region Create output description

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            foreach(var field in Recordset.Fields)
            {
                var path = field.Path;

                if(path != null)
                {
                    string expressionFormat;
                    if (isRecordset)
                    {
                        expressionFormat = "[[{0}().{1}]]";
                    }
                    else
                    {
                        expressionFormat = "[[{1}]]";
                    }
                    path.OutputExpression = string.Format(expressionFormat, Recordset.Name, field.Alias);
                    dataSourceShape.Paths.Add(path);
                }
            }

            #endregion

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            var inputs = new XElement("Inputs");

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

            var outputs = new XElement("Outputs");

            #region Add recordset fields to outputs

            
            foreach(var field in Recordset.Fields)
            {
                if(isRecordset)
                {
                    var output = new XElement("Output",
                        new XAttribute("Name", field.Name ?? string.Empty),
                        new XAttribute("MapsTo", field.Alias ?? string.Empty),
                        new XAttribute("Value", "[[" + Recordset.Name + "()." + field.Alias + "]]"),
                        new XAttribute("Recordset", Recordset.Name)
                        );
                    outputs.Add(output);
                }
                else
                {
                    var output = new XElement("Output",
                        new XAttribute("Name", field.Name ?? string.Empty),
                        new XAttribute("MapsTo", field.Alias ?? string.Empty),
                        new XAttribute("Value", "[[" + field.Alias + "]]")
                        );
                    outputs.Add(output);
                }
            }

            #endregion

            const enActionType ActionType = enActionType.InvokeStoredProc;

            var result = base.ToXml();
            result.AddFirst(
                new XElement("Actions",
                    new XElement("Action",
                        new XAttribute("Name", Recordset.Name ?? string.Empty),
                        new XAttribute("Type", ActionType),
                        new XAttribute("SourceID", Source.ResourceID),
                        new XAttribute("SourceName", Source.ResourceName ?? string.Empty),
                        new XAttribute("SourceMethod", Method.Name ?? string.Empty),
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

        #region CreateEmpty

        public static DbService Create()
        {
            return new DbService
            {
                ResourceID = Guid.Empty,
                ResourceType = ResourceType.DbService,
                Source = new DbSource { ResourceID = Guid.Empty, ResourceType = ResourceType.DbSource }
            };
        }

        #endregion


    }
}