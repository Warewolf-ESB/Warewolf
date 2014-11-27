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
using Dev2.DynamicServices.Objects.Base;

namespace Dev2.DynamicServices
{
    public class WorkflowActivityDef : DynamicServiceObjectBase
    {
        public WorkflowActivityDef()
        {
            ObjectType = enDynamicServiceObjectType.WorkflowActivity;
        }

        public string ServiceName { get; set; }
        public DynamicService Service { get; set; }
        public string DataTags { get; set; }
        public string ResultValidationRequiredTags { get; set; }
        public string ResultValidationExpression { get; set; }
        public bool DeferExecution { get; set; }
        public string AdminRoles { get; set; }

        public override bool Compile()
        {
            base.Compile();

            if (string.IsNullOrEmpty(Name))
            {
                WriteCompileError(Resources.CompilerError_MissingActivityName);
            }

            if (Service == null)
            {
                WriteCompileError(Resources.CompilerError_ServiceNotFound);
            }

            if (string.IsNullOrEmpty(IconPath))
            {
                WriteCompileError(Resources.CompilerError_MissingIconPath);
            }

            return IsCompiled;
        }
    }
}