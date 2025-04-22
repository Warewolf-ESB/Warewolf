/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities;
#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation.Services;
#endif
using System.Text;

namespace Dev2.Utilities
{
    // BUG 9304 - 2013.05.08 - TWR - Added this
    public interface IWorkflowHelper
    {
#if WINDOWS || NETFRAMEWORK
        StringBuilder SerializeWorkflow(ModelService modelService);
#endif

        ActivityBuilder CreateWorkflow(string displayName);

#if WINDOWS || NETFRAMEWORK
        ActivityBuilder EnsureImplementation(ModelService modelService);
#endif

        StringBuilder SanitizeXaml(StringBuilder workflowXaml);
    }
}
