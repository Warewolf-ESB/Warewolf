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
using Dev2.Common.X6;
using Dev2.Communication;
using Dev2.WorkflowConverters;
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

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();
            base.ToX6Json(cell);

            cell.shape = Constants.RECT;
            cell.data[Constants.TYPE] = Constants.DSFWORKFLOWACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_WORKFLOW;
            cell.data[Constants.UNIQUEID] = UniqueID;
            cell.data[Constants.WORKFLOW_RESOURCEID] = ResourceID.Expression.ToString();
            cell.data[Constants.WORKFLOW_SERVICENAME] = ServiceName;
            cell.data[Constants.WORKFLOW_SERVICESERVER] = ServiceServer;
            cell.data[Constants.WORKFLOW_SOURCEID] = SourceId;
            cell.data[Constants.WORKFLOW_RUNWORKFLOWASYNC] = RunWorkflowAsync;
            cell.data[Constants.WORKFLOW_ISOBJECT] = IsObject;
            cell.data[Constants.WORKFLOW_OBJECTNAME] = ObjectName;
            cell.data[Constants.WORKFLOW_OBJECTRESULT] = ObjectResult;
            cell.data[Constants.WORKFLOW_INPUTMAPPING] = InputMapping;
            cell.data[Constants.WORKFLOW_OUTPUTMAPPING] = OutputMapping;
            cell.data[Constants.WORKFLOW_ISWORKFLOW] = IsWorkflow;
            cell.data[Constants.WORKFLOW_INPUTS] = Inputs;
            cell.data[Constants.WORKFLOW_OUTPUTS] = Outputs;
            cell.data[Constants.WORKFLOW_CATEGORY] = Category;
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;
            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;
            if (cell.data.TryGetGuid(Constants.WORKFLOW_RESOURCEID, out var resourceid)) ResourceID = new System.Activities.InArgument<System.Guid>(resourceid);
            if (cell.data.TryGetString(Constants.WORKFLOW_SERVICENAME, out var serviceName)) ServiceName = serviceName;
            if (cell.data.TryGetGuid(Constants.WORKFLOW_SERVICESERVER, out var serviceServer)) ServiceServer = serviceServer;
            if (cell.data.TryGetGuid(Constants.WORKFLOW_SOURCEID, out var sourceId)) SourceId = sourceId;
            if (cell.data.TryGetBool(Constants.WORKFLOW_RUNWORKFLOWASYNC, out var runAsync)) RunWorkflowAsync = runAsync;
            if (cell.data.TryGetBool(Constants.WORKFLOW_ISOBJECT, out var isObject)) IsObject = isObject;
            if (cell.data.TryGetString(Constants.WORKFLOW_OBJECTNAME, out var objectName)) ObjectName = objectName;
            if (cell.data.TryGetString(Constants.WORKFLOW_OBJECTRESULT, out var objectResult)) ObjectResult = objectResult;
            if (cell.data.TryGetString(Constants.WORKFLOW_INPUTMAPPING, out var inputMapping)) InputMapping = inputMapping;
            if (cell.data.TryGetString(Constants.WORKFLOW_OUTPUTMAPPING, out var outputMapping)) OutputMapping = outputMapping;
            if (cell.data.TryGetBool(Constants.WORKFLOW_ISWORKFLOW, out var isWorkflow)) IsWorkflow = isWorkflow;
            if (cell.data.TryGetString(Constants.WORKFLOW_CATEGORY, out var category)) Category = category;

            if (CommonHelper.TryGetList<ServiceInput, Dev2.Common.Interfaces.DB.IServiceInput>(cell.data, out var inputs, Constants.WORKFLOW_INPUTS))
            {
                Inputs = new List<Dev2.Common.Interfaces.DB.IServiceInput>(inputs);
            }

            if (CommonHelper.TryGetOutputs(cell.data, Constants.WORKFLOW_OUTPUTS, out var outputs))
            {
               Outputs = new List<Dev2.Common.Interfaces.DB.IServiceOutputMapping>(outputs);
            }
        }
    }
}
