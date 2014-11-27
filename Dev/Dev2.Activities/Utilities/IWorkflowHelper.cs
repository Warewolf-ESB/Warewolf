
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
using System.Activities.Presentation.Services;
using System.Text;

namespace Dev2.Utilities
{
    // BUG 9304 - 2013.05.08 - TWR - Added this
    public interface IWorkflowHelper
    {
        StringBuilder SerializeWorkflow(ModelService modelService);

        ActivityBuilder CreateWorkflow(string displayName);

        ActivityBuilder EnsureImplementation(ModelService modelService);

        void CompileExpressions(DynamicActivity dynamicActivity);

        void CompileExpressions<TResult>(DynamicActivity<TResult> dynamicActivity);

        StringBuilder SanitizeXaml(StringBuilder workflowXaml);
    }
}
