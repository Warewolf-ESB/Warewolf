
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
