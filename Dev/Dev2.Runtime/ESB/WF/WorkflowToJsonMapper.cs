using Dev2.Activities.WF;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Utilities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Text;
using System.Xaml;

namespace Dev2.Runtime.ESB.WF
{

    public class WorkflowToJsonMapper
    {
        public static void Process(EsbExecuteRequest request)
        {
            var data = request.ExecuteResult;
            var finalresult = new ExecuteMessage();

            if (data != null)
            {
                var executionResult = JsonConvert.DeserializeObject<ExecuteMessage>(data.ToString());

                if (null != executionResult)
                {
                    var requestInfo = JsonConvert.DeserializeObject<Common.X6.X6RequestInfo>(executionResult.Message.ToString());
                    if (requestInfo != null)
                    {
                        var json = MapToJson(requestInfo);

                        finalresult.Message = new StringBuilder(json);
                    }
                    var serializer = new Dev2JsonSerializer();
                    request.ExecuteResult = serializer.SerializeToBuilder(finalresult);
                }
            }
        }

        public static string MapToJson(Common.X6.X6RequestInfo requestInfo)
        {
            var builder = ReadXamlDefinition(requestInfo.ActivityXaml);
            if (builder == null) { return string.Empty; }

            var graph = new WorkflowToX6Converter().ConvertToX6Json(builder, requestInfo.WorkflowXML);
            return graph;
        }

        public static ActivityBuilder ReadXamlDefinition(string xaml)
        {
            try
            {
                if (!string.IsNullOrEmpty(xaml))
                {
                    using (var sw = new System.IO.StringReader(xaml))
                    {
                        var xamlXmlWriterSettings = new XamlXmlReaderSettings();
                        var xw = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(sw, new XamlSchemaContext(), xamlXmlWriterSettings));
                        var load = XamlServices.Load(xw);
                        return load as ActivityBuilder;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error loading XAML: ", e, GlobalConstants.WarewolfError);
            }
            return null;
        }
    }

    
}
