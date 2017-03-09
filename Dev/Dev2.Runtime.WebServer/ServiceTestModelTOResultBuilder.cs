﻿using System;
using Dev2.Common.Interfaces;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestModelTOResultBuilder
    {
        public static JObject BuildTestResultForWebRequest(this IServiceTestModelTO result)
        {
            var resObj = new JObject { { "Test Name", result.TestName } };
            if (result.Result.RunTestResult == RunResult.TestPassed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PassedResult);
            }
            else if (result.Result.RunTestResult == RunResult.TestFailed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_FailureResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestInvalid)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_InvalidResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourceDeleted)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourceDeleteResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourcePathUpdated)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourcpathUpdatedResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }

            else if (result.Result.RunTestResult == RunResult.TestPending)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PendingResult);
                resObj.Add("Message", result.Result.Message);
            }
            return resObj;
        }
    }
}
