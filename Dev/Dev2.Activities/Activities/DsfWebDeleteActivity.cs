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

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
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
            var webRequestResult = PerformWebRequest(head, query, url, string.Empty);

            tmpErrors.MergeErrors(_errorsTo);

            var bytes = webRequestResult.Base64StringToByteArray();
            var response = bytes.ReadToString();

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
    }
}
