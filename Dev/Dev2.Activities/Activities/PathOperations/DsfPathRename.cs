/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("FileFolder-Rename", "Rename", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Rename")]
    public class DsfPathRename : DsfAbstractMultipleFilesActivity, IPathInput, IPathOutput, IPathOverwrite, IDestinationUsernamePassword
    {
        public DsfPathRename()
            : base("Rename")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            Dev2CRUDOperationTO opTo = new Dev2CRUDOperationTO(Overwrite);
            var result = broker.Rename(scrEndPoint, dstEndPoint, opTo);
            return result.Replace("Move", "Rename");
        }

        protected override void MoveRemainingIterators()
        {
        }
    }
}
