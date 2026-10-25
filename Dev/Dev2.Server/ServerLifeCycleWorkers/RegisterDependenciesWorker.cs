/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common;
using Dev2.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ESB.WF;
using Warewolf.Auditing;
using Warewolf.Bite;

namespace Dev2.ServerLifeCycleWorkers
{
    public class RegisterDependenciesWorker : IServerLifecycleWorker
    {
        public void Execute()
        {
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register<IExecutionManager>(new ExecutionManager());
            CustomContainer.Register<IStateNotifierFactory>(new StateNotifierFactory());
            CustomContainer.Register<IResumableExecutionContainerFactory>(new ResumableExecutionContainerFactory());
            CustomContainer.Register<IFieldAndPropertyMapper>(new FieldAndPropertyMapper());
            ChatbotContextBuilder.XamlToX6Json = WorkflowToJsonMapper.MapToJson;

            // WF removal T1.7 shadow mode: off unless WAREWOLF_BITE_SHADOW=true. Freshly parsed workflows are also read
            // by the WF-free reader and compared; the WF graph is still the one executed.
            if (BiteShadow.IsEnabled())
            {
                CustomContainer.Register<IWorkflowParseObserver>(new BiteShadowParseObserver(result =>
                {
                    if (result.Matched)
                    {
                        Dev2Logger.Info(result.ToString(), GlobalConstants.WarewolfInfo);
                    }
                    else
                    {
                        Dev2Logger.Warn(result.ToString(), GlobalConstants.WarewolfWarn);
                    }
                }));
                Dev2Logger.Info($"{BiteShadow.EnabledVariable} is on: workflows are shadow-read by the WF-free reader", GlobalConstants.WarewolfInfo);
            }
        }
    }
}
