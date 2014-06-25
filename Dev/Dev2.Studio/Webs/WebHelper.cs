using Dev2.Data.SystemTemplates.Models;
using Dev2.Webs.Callbacks;

// ReSharper disable once CheckNamespace
namespace Dev2.Webs
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
    }
}
