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

using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.X6;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Dev2.WorkflowConverters;
using System.Collections.Generic;
using Warewolf.Core;


namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    [ToolDescriptorInfo("FileFolder-Rename", "Rename", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Rename")]
    public class DsfPathRename : DsfAbstractMultipleFilesActivity, IPathInput, IPathOutput, IPathOverwrite, IDestinationUsernamePassword
    {
        public DsfPathRename()
            : base("Rename")
        {
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            var opTo = new Dev2CRUDOperationTO(Overwrite);
            var result = broker.Rename(scrEndPoint, dstEndPoint, opTo);
            return result.Replace("Move", "Rename");
        }
        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name="InputPath",
                    Type = StateVariable.StateType.Input,
                    Value = InputPath
                },
                new StateVariable
                {
                    Name="OutputPath",
                    Type = StateVariable.StateType.Output,
                    Value = OutputPath
                },
                new StateVariable
                {
                    Name="DestinationUsername",
                    Type = StateVariable.StateType.Input,
                    Value =DestinationUsername
                },
                new StateVariable
                {
                    Name="Username",
                    Type = StateVariable.StateType.Input,
                    Value = Username
                },
                new StateVariable
                {
                    Name="Overwrite",
                    Type = StateVariable.StateType.Input,
                    Value = Overwrite.ToString()
                },
                new StateVariable
                {
                    Name="PrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value = PrivateKeyFile
                },
                new StateVariable
                {
                    Name="DestinationPrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value =DestinationPrivateKeyFile
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }

        protected override void MoveRemainingIterators()
        {
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.DSFPATHRENAME;
            cell.data[Constants.TYPE] = Constants.DSFPATHRENAME.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_PATHRENAME;
            cell.data[Constants.UNIQUEID] = UniqueID;
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
                DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
                UniqueID = uniqueId;
        }
    }
}
