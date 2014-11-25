
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities;
using System.Activities.Hosting;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Hosting
{
    public class WorkflowInstanceInfo : IWorkflowInstanceExtension
    {
        public IEnumerable<object> GetAdditionalExtensions()
        {
            yield break;
        }

        public void SetInstance(WorkflowInstanceProxy instance)
        {
            Proxy = instance;
        }

        public WorkflowInstanceProxy Proxy { get; private set; }

        public string ProxyName
        {
            get
            {
                var dynamicActivity = Proxy.WorkflowDefinition as DynamicActivity;
                return dynamicActivity != null ? dynamicActivity.Name : Proxy.WorkflowDefinition.DisplayName;
            }
        }
    }
}
