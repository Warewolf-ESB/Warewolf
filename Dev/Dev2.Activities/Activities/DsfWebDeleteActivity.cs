﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Resources-Service", "Delete Web Service", ToolType.Native, "6C5F6D7E-4B42-4874-8197-DBE68D4A9F2D", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfWebDeleteActivity : DsfWebActivityBase
    {

        public DsfWebDeleteActivity()
            : base(WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Delete, "Web Delete Request Connector", "Web Delete Request Connector"))
        {

        }

        #region Overrides



        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
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

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var webRequestResult = PerformWebPostRequest(head, query, url, string.Empty);
            PushXmlIntoEnvironment(webRequestResult, update, dataObject);
        }

        #endregion

    }
}
