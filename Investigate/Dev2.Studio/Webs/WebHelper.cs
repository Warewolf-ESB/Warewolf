using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.AppResources.Browsers;

namespace Dev2.Studio.Webs
{
    public static class WebHelper
    {
        internal static string CleanModelData(Dev2DecisionCallbackHandler callBackHandler)
        {
            // Remove naughty chars...
            string tmp = callBackHandler.ModelData;
            // remove the silly Choose... from the string
            tmp = Dev2DecisionStack.RemoveDummyOptionsFromModel(tmp);
            // remove [[]], &, !
            tmp = Dev2DecisionStack.RemoveNaughtyCharsFromModel(tmp);
            return tmp;
        }

        internal static Dev2DecisionCallbackHandler ShowWebpage(Uri requestUri, string webModel, double width,
                                                               double height)
        {
            string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, GlobalConstants.NullDataListID);
            var callBackHandler = new Dev2DecisionCallbackHandler { ModelData = webModel };
            WebSites.ShowWebPageDialog(uriString, callBackHandler, width, height);
            return callBackHandler;
        }
    }
}
