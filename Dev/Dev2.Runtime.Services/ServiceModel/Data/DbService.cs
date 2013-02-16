using Dev2.Common;
using Dev2.DynamicServices;
using System;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbService : Service
    {
        #region CTOR

        public DbService()
        {
        }

        public DbService(XElement xml)
            : base(xml)
        {
            Source = new DbSource(xml.Element("Source"));
        }

        #endregion

        public DbSource Source { get; set; }

        #region Save

        public override void Save(Guid workspaceID, Guid dataListID)
        {
            var xml = ToXml();
            Resources.Save(workspaceID, GlobalConstants.ServicesDirectory, ResourceName, xml.ToString());
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            //
            // Recreate output description
            //
            IOutputDescription outputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            outputDescription.DataSourceShapes.Add(dataSourceShape);

            foreach (var field in Recordset.Fields)
            {
                var path = field.Path;

                if (path != null)
                {
                    path.OutputExpression = field.Alias;
                    dataSourceShape.Paths.Add(path);
                }
            }

            IOutputDescriptionSerializationService outputDescriptionSerializationService =
                OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            string serializedOutputDescription = outputDescriptionSerializationService.Serialize(outputDescription);

            //
            // Create service action XML
            //
            var inputsElement = new XElement("Inputs");

            foreach(var parameter in Method.Parameters)
            {
                inputsElement.Add(new XElement("Input",
                    new XAttribute("Name", parameter.Name ?? string.Empty),
                    new XAttribute("Source", parameter.Name ?? string.Empty),
                    new XAttribute("DefaultValue", parameter.DefaultValue ?? string.Empty)
                    ));

                if (parameter.IsRequired)
                {
                    inputsElement.Add(new XElement("Validator",
                        new XAttribute("Type", "Required")));
                }
            }

            return new XElement("Service",
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ServerID", Guid.Empty.ToString()),
                new XElement("Actions",
                    new XElement("Action",
                        new XAttribute("Name", ResourceName ?? string.Empty),
                        new XAttribute("Type", enActionType.InvokeStoredProc),
                        new XAttribute("SourceName", Source.ResourceName ?? string.Empty),
                        new XAttribute("SourceMethod", Method.Name ?? string.Empty),
                        inputsElement,
                        new XElement("Outputs"),
                        new XElement("OutputDescription",
                            new XCData(serializedOutputDescription)))),
                new XElement("AuthorRoles"),
                new XElement("Comment"),
                new XElement("Category", ResourcePath ?? string.Empty),
                new XElement("Tags"),
                new XElement("HelpLink"),
                new XElement("UnitTestTargetWorkflowService"),
                new XElement("BizRule"),
                new XElement("WorkflowActivityDef"),
                new XElement("XamlDefinition"),
                new XElement("DisplayName"),
                new XElement("DataList"),
                new XElement("TypeOf", ResourceType)
                );


            //ServiceAction serviceAction = new ServiceAction()
            //{ 
            //    Name = ResourceName, 
            //    ActionType = enActionType.InvokeStoredProc, 
            //    SourceMethod = Method.Name, 
            //    SourceName = Source.ResourceName,
            //    OutputDescription = serializedOutputDescription,
            //};

            //UnlimitedObject u = new UnlimitedObject();


            //string s = u.GetXmlDataFromObject(serviceAction);
            ////s = u.XmlString;
            //string s1 = s;

            ////var workspaceItemInput = new ServiceActionInput { Name = "ItemXml", Source = "ItemXml" };
            ////workspaceItemInput.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            ////workspaceItemAction.ServiceActionInputs.Add(workspaceItemInput);

            ////workspaceItemInput = new ServiceActionInput { Name = "Roles", Source = "Roles" };
            ////workspaceItemInput.Validators.Add(new Validator { ValidatorType = enValidationType.Required });
            ////workspaceItemAction.ServiceActionInputs.Add(workspaceItemInput);

            ////var workspaceItemService = new DynamicService { Name = "UpdateWorkspaceItemService" };
            ////workspaceItemService.Actions.Add(workspaceItemAction);


            var result = base.ToXml();
            result.Add(Source.ToXml());

            return result;
        }

        #endregion

        #region CreateEmpty

        public static DbService Create()
        {
            return new DbService
            {
                ResourceID = Guid.Empty,
                ResourceType = enSourceType.SqlDatabase,
                Source = new DbSource { ResourceID = Guid.Empty, ResourceType = enSourceType.SqlDatabase }
            };
        }

        #endregion


    }
}