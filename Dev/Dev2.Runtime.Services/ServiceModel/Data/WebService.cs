using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class WebService : Service
    {
        #region Properties

        public WebSource Source { get; set; }
        public string RequestUrl { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WebRequestMethod RequestMethod { get; set; }

        public string RequestHeaders { get; set; }
        public string RequestBody { get; set; }
        public string RequestResponse { get; set; }

        public RecordsetList Recordsets { get; set; }

        #endregion

        #region CTOR

        public WebService()
        {
            ResourceID = Guid.Empty;
            ResourceType = ResourceType.WebService;
            Source = new WebSource();
            Recordsets = new RecordsetList();
            Method = new ServiceMethod();
        }

        public WebService(XElement xml)
            : base(xml)
        {
            ResourceType = ResourceType.WebService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            RequestUrl = action.AttributeSafe("RequestUrl");
            WebRequestMethod requestMethod;
            RequestMethod = Enum.TryParse(action.AttributeSafe("RequestMethod"), true, out requestMethod) ? requestMethod : WebRequestMethod.Get;
            RequestHeaders = action.ElementSafe("RequestHeaders");
            RequestBody = action.ElementSafe("RequestBody");

            #region Parse Source

            Guid sourceID;
            Guid.TryParse(action.AttributeSafe("SourceID"), out sourceID);
            Source = new WebSource
            {
                ResourceID = sourceID,
                ResourceName = action.AttributeSafe("SourceName")
            };

            #endregion

            #region Parse Method

            Method = new ServiceMethod { Name = "", Parameters = new List<MethodParameter>() };
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
                            new XAttribute("Value", "[[" + field.Alias + "]]"),
                            new XAttribute("Recordset", recordset.Name)
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

            const enActionType ActionType = enActionType.InvokeWebService;

            var result = base.ToXml();
            result.AddFirst(
                new XElement("Actions",
                    new XElement("Action",
                        new XAttribute("Name", ResourceName ?? string.Empty),
                        new XAttribute("Type", ActionType),
                        new XAttribute("SourceID", Source.ResourceID),
                        new XAttribute("SourceName", Source.ResourceName ?? string.Empty),
                        new XAttribute("RequestUrl", RequestUrl ?? string.Empty),
                        new XAttribute("RequestMethod", RequestMethod.ToString()),
                        new XElement("RequestHeaders", new XCData(RequestHeaders ?? string.Empty)),
                        new XElement("RequestBody", new XCData(RequestBody ?? string.Empty)),
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

        #region GetOutputDescription

        public IOutputDescription GetOutputDescription()
        {
            IOutputDescription result = null;
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

            var requestResponse = RequestResponse;

            // HACK: Remove strings that cause issues with the data mapper

            var regex = new Regex("\\sxmlns[^\"]+\"[^\"]+\""); // e.g. xmlns="http://www.webservice.net"
            requestResponse = regex.Replace(requestResponse, "");

            regex = new Regex("(<\\?).*(\\?>)"); // e.g. <?xml version="1.0" encoding="utf-8"?>
            requestResponse = regex.Replace(requestResponse, "");

            try
            {
                result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                result.DataSourceShapes.Add(dataSourceShape);
                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                dataSourceShape.Paths.AddRange(dataBrowser.Map(requestResponse));

            }
            catch(Exception ex)
            {
                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                XElement errorResult = new XElement("Error");
                errorResult.Add(ex);
                var data = errorResult.ToString();
                dataSourceShape.Paths.AddRange(dataBrowser.Map(data));
            }
            return result;
        }

        #endregion

    }
}