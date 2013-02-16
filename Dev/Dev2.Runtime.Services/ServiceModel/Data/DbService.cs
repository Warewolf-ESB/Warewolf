using System;
using System.Xml.Linq;
using Dev2.DynamicServices;
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
            //var action = xml.Element("Action");
            //Source = new DbSource(xml.Element("Action"));
            //Method = new ServiceMethod();
            //ResourceName = xml.AttributeSafe("Name");
        }

        #endregion

        public DbSource Source { get; set; }

        #region ToXml

        public override XElement ToXml()
        {
            #region Create output description

            var outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            foreach(var field in Recordset.Fields)
            {
                var path = field.Path;

                if(path != null)
                {
                    path.OutputExpression = field.Alias;
                    dataSourceShape.Paths.Add(path);
                }
            }

            #endregion

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            var inputsElement = new XElement("Inputs");

            #region Add inputs

            foreach(var parameter in Method.Parameters)
            {
                inputsElement.Add(new XElement("Input",
                    new XAttribute("Name", parameter.Name ?? string.Empty),
                    new XAttribute("Source", parameter.Name ?? string.Empty),
                    new XAttribute("DefaultValue", parameter.DefaultValue ?? string.Empty)
                    ));

                if(parameter.IsRequired)
                {
                    inputsElement.Add(new XElement("Validator",
                        new XAttribute("Type", "Required")));
                }
            }

            #endregion

            const enActionType ActionType = enActionType.InvokeStoredProc;
            var result = base.ToXml();
            result.AddFirst(
                new XElement("Actions",
                    new XElement("Action",
                        new XAttribute("Name", ResourceName ?? string.Empty),
                        new XAttribute("Type", ActionType),
                        new XAttribute("SourceID", Source.ResourceID),
                        new XAttribute("SourceName", Source.ResourceName ?? string.Empty),
                        new XAttribute("SourceMethod", Method.Name ?? string.Empty),
                        inputsElement,
                        new XElement("Outputs"),
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