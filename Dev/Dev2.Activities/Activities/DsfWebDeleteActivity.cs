#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.X6;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.WorkflowConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "DELETE", ToolType.Native, "6C5F6D7E-4B42-4874-8197-DBE68D4A9F2D", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_WebMethod_Delete")]
    public class DsfWebDeleteActivity : DsfWebActivityBase
    {

        
        public DsfWebDeleteActivity()
            : base(WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Delete, "DELETE Web Method", "DELETE Web Method"))
        {

        }
        
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();

            var (head, query, _) = ConfigureHttp(dataObject, update);

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            if (url == null
                && AmbientSourceLoader.Current?.EnsureSourceLoaded(SourceId) == true
                && ResourceCatalog.WorkspaceResources
                       .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
            {
                lock (ws)
                    url = ws.OfType<WebSource>().FirstOrDefault(r => r.ResourceID == SourceId);
            }
            var webRequestResult = PerformWebRequest(head, query, url, string.Empty);

            tmpErrors.MergeErrors(_errorsTo);

            var bytes = webRequestResult.Base64StringToByteArray();
            var response = bytes.ReadToString();
            response = Scrubber.Scrub(response);

            ResponseManager = new ResponseManager
            { 
                OutputDescription = OutputDescription, 
                Outputs = Outputs, 
                IsObject = IsObject, 
                ObjectName = ObjectName 
            };
            ResponseManager.PushResponseIntoEnvironment(response, update, dataObject);
        }

        private (IEnumerable<NameValue> head, string query, string data) ConfigureHttp(IDSFDataObject dataObject, int update)
        {
            IEnumerable<NameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Value, update))));
            }
            var query = "";
            if (QueryString != null)
            {
                query = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(QueryString, update));
            }
            return (head, query, null);
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();
            base.ToX6Json(cell);

            cell.shape = Constants.WEBDELETEACTIVITY;
            cell.data[Constants.TYPE] = Constants.WEBDELETEACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_WEBDELETE;
            cell.data.Add(Constants.UNIQUEID, UniqueID);

            cell.data.Add(Constants.WEBMETHOD_HEADERS, Headers);
            cell.data.Add(Constants.WEBMETHOD_QUERYSTRING, QueryString);
            cell.data.Add(Constants.WEBMETHOD_SOURCEID, SourceId);
            cell.data.Add(Constants.WEBMETHOD_OUTPUTDESCRIPTION, OutputDescription);
            cell.data.Add(Constants.WEBMETHOD_INPUTS, Inputs);
            cell.data.Add(Constants.WEBMETHOD_OUTPUTS, Outputs);
            cell.data.Add(Constants.WEBMETHOD_ISOBJECT, IsObject);
            cell.data.Add(Constants.WEBMETHOD_OBJECTNAME, ObjectName);
            cell.data.Add(Constants.WEBMETHOD_OBJECTRESULT, ObjectResult);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell?.data == null) return;
            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueid)) UniqueID = uniqueid;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTRESULT, out var objectresult)) ObjectResult = objectresult;
            if (cell.data.TryGetString(Constants.WEBMETHOD_QUERYSTRING, out var queryString)) QueryString = queryString;
            if (cell.data.TryGetGuid(Constants.WEBMETHOD_SOURCEID, out var sourceId)) SourceId = sourceId;
            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISOBJECT, out var isObject)) IsObject = isObject;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTNAME, out var objectName)) ObjectName = objectName;

            if (cell.data.TryGetHeaders(out var headers)) Headers = headers;
            if (cell.data.TryGetInputs(out var inputs)) Inputs = inputs;
            if (cell.data.TryGetOutputs(out var outputs)) Outputs = outputs;
            if (cell.data.TryGetOutputDescription(out var outputDesc)) OutputDescription = outputDesc;

            // Omitting the optional headers key left Headers null; ConfigureHttp then silently
            // sent no headers instead of hard-failing, but null vs. empty should not be a
            // distinction callers have to know about (F10).
            Headers ??= new List<INameValue>();
        }
    }
}
