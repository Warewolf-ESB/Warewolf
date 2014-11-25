
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces;

namespace Dev2.Messages
{
    public class SaveUnsavedWorkflowMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public string ResourceName { get; set; }
        public string ResourceCategory { get; set; }
        public bool KeepTabOpen { get; set; }

        public SaveUnsavedWorkflowMessage(IContextualResourceModel resourceModel, string resourceName, string resourceCategory, bool keepTabOpen)
        {
            ResourceModel = resourceModel;
            ResourceName = resourceName;
            ResourceCategory = resourceCategory;
            KeepTabOpen = keepTabOpen;
        }
    }
}
