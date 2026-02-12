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
    [ToolDescriptorInfo("FileFolder-Copy", "Copy", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Copy")]
    public class DsfPathCopy : DsfAbstractMultipleFilesActivity, IPathOverwrite, IPathInput, IPathOutput,
                               IDestinationUsernamePassword
    {
        public DsfPathCopy()
            : base("Copy")
        {
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            var opTO = new Dev2CRUDOperationTO(Overwrite);
            return broker.Copy(scrEndPoint, dstEndPoint, opTO);
        }

        protected override void MoveRemainingIterators()
        {
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = nameof(InputPath),
                    Value = InputPath,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Username),
                    Value = Username,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(PrivateKeyFile),
                    Value = PrivateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(OutputPath),
                    Value = OutputPath,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(DestinationUsername),
                    Value = DestinationUsername,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(DestinationPrivateKeyFile),
                    Value = DestinationPrivateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Overwrite),
                    Value = Overwrite.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.DSFPATHCOPY;
            cell.data[Constants.TYPE] = Constants.DSFPATHCOPY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_PATHCOPY;
            cell.data[Constants.UNIQUEID] = UniqueID;

            // Path operation specific properties
            cell.data.TryAdd(Constants.PATHCOPY_INPUTPATH, InputPath);
            cell.data.TryAdd(Constants.PATHCOPY_OUTPUTPATH, OutputPath);
            cell.data.TryAdd(Constants.RESULT, Result);
            cell.data.TryAdd(Constants.PATHCOPY_OVERWRITE, Overwrite);
            
            // Destination credentials
            cell.data.TryAdd(Constants.PATHCOPY_DESTINATIONUSERNAME, DestinationUsername);
            cell.data.TryAdd(Constants.PATHCOPY_DESTINATIONPASSWORD, DestinationPassword);
            cell.data.TryAdd(Constants.PATHCOPY_DESTINATIONPRIVATEKEYFILE, DestinationPrivateKeyFile);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) 
                DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) 
                UniqueID = uniqueId;

            // Path operation specific properties
            if (cell.data.TryGetString(Constants.PATHCOPY_INPUTPATH, out var inputPath)) 
                InputPath = inputPath;
            if (cell.data.TryGetString(Constants.PATHCOPY_OUTPUTPATH, out var outputPath)) 
                OutputPath = outputPath;
            if (cell.data.TryGetString(Constants.RESULT, out var result)) 
                Result = result;
            if (cell.data.TryGetBool(Constants.PATHCOPY_OVERWRITE, out var overwrite)) 
                Overwrite = overwrite;

            // Destination credentials
            if (cell.data.TryGetString(Constants.PATHCOPY_DESTINATIONUSERNAME, out var destUsername)) 
                DestinationUsername = destUsername;
            if (cell.data.TryGetString(Constants.PATHCOPY_DESTINATIONPASSWORD, out var destPassword)) 
                DestinationPassword = destPassword;
            if (cell.data.TryGetString(Constants.PATHCOPY_DESTINATIONPRIVATEKEYFILE, out var destPrivateKey)) 
                DestinationPrivateKeyFile = destPrivateKey;

            // Defensive initialization
            InputPath ??= string.Empty;
            OutputPath ??= string.Empty;
        }
    }
}
