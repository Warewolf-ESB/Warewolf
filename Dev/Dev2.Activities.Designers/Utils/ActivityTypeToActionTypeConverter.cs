using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.Activities.Utils
{
    public static class ActivityTypeToActionTypeConverter
    {
        public static enActionType ConvertToActionType(string type)
        {
            switch(type)
            {
                case "Workflow":
                    return enActionType.Workflow;
                case "WebService":
                    return enActionType.InvokeWebService;
                case "PluginService":
                    return enActionType.Plugin;
                case "DbService":
                    return enActionType.InvokeStoredProc;
                case "RemoteService":
                    return enActionType.RemoteService;
                default:
                    return enActionType.BizRule;
            }
        }
    }
}
