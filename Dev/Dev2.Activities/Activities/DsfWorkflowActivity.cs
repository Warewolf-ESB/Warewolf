#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Communication;
using Warewolf.Core;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Resources-Service", "Service", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_Service")]
    public class DsfWorkflowActivity : DsfActivity
    {
        public override IEnumerable<StateVariable> GetState()
        {
            var serializer = new Dev2JsonSerializer();
            var inputs = serializer.Serialize(Inputs);
            var outputs = serializer.Serialize(Outputs);

            return new[]
            {
                new StateVariable
                {
                    Name="Inputs",
                    Type = StateVariable.StateType.Input,
                    Value = inputs
                },
                 new StateVariable
                {
                    Name="Outputs",
                    Type = StateVariable.StateType.Output,
                    Value = outputs
                 },
                 new StateVariable
                {
                    Name="ServiceServer",
                    Type = StateVariable.StateType.Input,
                    Value = ServiceServer.ToString()
                 },
                 new StateVariable
                {
                    Name="EnvironmentID",
                    Type = StateVariable.StateType.Input,
                    Value =EnvironmentID.Expression.ToString()
                 },
                 new StateVariable
                {
                    Name="IsWorkflow",
                    Type = StateVariable.StateType.Input,
                    Value = IsWorkflow.ToString()
                 },
                 new StateVariable
                {
                    Name="ServiceUri",
                    Type = StateVariable.StateType.Input,
                    Value = ServiceUri
                 },
                 new StateVariable
                {
                    Name="ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = ResourceID.Expression.ToString()
                 },
                 new StateVariable
                {
                    Name="ServiceName",
                    Type = StateVariable.StateType.Input,
                    Value = ServiceName
                 },
                 new StateVariable
                 {
                     Name="ParentServiceName",
                     Type = StateVariable.StateType.Input,
                     Value = ParentServiceName
                 }
            };
        }
    }
}
