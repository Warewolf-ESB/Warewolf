using Dev2.Activities.WF;
using Dev2.Common;
using Dev2.Communication;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Dev2.Runtime.ESB.WF
{
    /// <summary>
    /// Workflow to Json Mapper
    /// </summary>
    public class WorkflowToJsonMapper
    {
        /// <summary>
        /// Processes request to create map workflow to Json
        /// </summary>
        /// <param name="request">Execution request</param>
        public static void Process(EsbExecuteRequest request)
        {
            var data = request.ExecuteResult;
            var finalresult = new ExecuteMessage();

            if (data != null)
            {
                Dev2Logger.Debug($"WorkflowToJsonMapper.Process called. executeResult length: {(data?.ToString()?.Length ?? 0)}", GlobalConstants.WarewolfError);
                var executionResult = JsonConvert.DeserializeObject<ExecuteMessage>(data.ToString());

                if (null != executionResult)
                {
                    Dev2Logger.Debug($"ExecutionResult.HasError={executionResult.HasError}, MessageLength={(executionResult.Message?.ToString()?.Length ?? 0)}", GlobalConstants.WarewolfError);
                    var requestInfo = JsonConvert.DeserializeObject<Common.X6.X6RequestInfo>(executionResult.Message.ToString());
                    if (requestInfo != null)
                    {
                        Dev2Logger.Debug($"RequestInfo.ResourceName={requestInfo.ResourceName}, ActivityXamlLength={(requestInfo.ActivityXaml?.Length ?? 0)}, WorkflowXMLLength={(requestInfo.WorkflowXML?.Length ?? 0)}", GlobalConstants.WarewolfError);
                        var json = MapToJson(requestInfo);
                        Dev2Logger.Debug($"MapToJson returned length={(json?.Length ?? 0)} for resource={requestInfo.ResourceName}", GlobalConstants.WarewolfError);

                        finalresult.Message = new StringBuilder(json ?? string.Empty);
                    }
                    var serializer = new Dev2JsonSerializer();
                    request.ExecuteResult = serializer.SerializeToBuilder(finalresult);
                }
            }
        }

        /// <summary>
        /// Maps request to Json
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        public static string MapToJson(Common.X6.X6RequestInfo requestInfo)
        {
            try
            {
                var activityXamlLength = requestInfo?.ActivityXaml?.Length ?? 0;
                Dev2Logger.Debug($"MapToJson called. ActivityXamlLength={activityXamlLength}, WorkflowXMLLength={(requestInfo?.WorkflowXML?.Length ?? 0)}", GlobalConstants.WarewolfError);
                var builder = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(new StringBuilder(requestInfo.ActivityXaml));
                if (builder == null)
                {
                    Dev2Logger.Warn("MapToJson: XamlActivityBuilder is null - returning empty graph.", GlobalConstants.WarewolfError);
                    return string.Empty;
                }

                var converter = new WorkflowToX6Converter();
                var graph = converter.ConvertToX6Json(builder, requestInfo.WorkflowXML);
                Dev2Logger.Debug($"MapToJson: converter output length={(graph?.Length ?? 0)}", GlobalConstants.WarewolfError);
                return graph;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"MapToJson failed: {ex.Message}", ex, GlobalConstants.WarewolfError);
                return string.Empty;
            }
        }
    }
}
