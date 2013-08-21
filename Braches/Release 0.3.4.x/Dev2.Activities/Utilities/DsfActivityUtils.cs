using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    internal static class DsfActivityUtils {

        /// <summary>
        /// Used to clone an activity, mainly used for the ForEach activity
        /// </summary>
        /// <param name="toClone"></param>
        /// <returns></returns>
        internal static DsfActivity Clone(DsfActivity toClone) {
            var transientHandler = new DsfActivity(toClone.ToolboxFriendlyName, 
                                                    toClone.IconPath.Expression.ToString(), 
                                                    toClone.ServiceName, 
                                                    toClone.DataTags, 
                                                    toClone.ResultValidationRequiredTags, 
                                                    toClone.ResultValidationExpression);

            transientHandler.InputMapping = toClone.InputMapping;
            transientHandler.OutputMapping = toClone.OutputMapping;
            transientHandler.ParentServiceName = toClone.ParentServiceName;
            transientHandler.ParentWorkflowInstanceId = toClone.ParentWorkflowInstanceId;
            transientHandler.IsWorkflow = toClone.IsWorkflow;
            transientHandler.IsSimulationEnabled = toClone.IsSimulationEnabled;
            transientHandler.DisplayName = toClone.DisplayName;
            transientHandler.Category = toClone.Category;
            transientHandler.AuthorRoles = toClone.AuthorRoles;

            return transientHandler;

        }
    }
}
