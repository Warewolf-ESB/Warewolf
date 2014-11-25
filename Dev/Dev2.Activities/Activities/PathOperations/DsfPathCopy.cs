
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.PathOperations;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfPathCopy : DsfAbstractMultipleFilesActivity, IPathOverwrite, IPathInput, IPathOutput,
                               IDestinationUsernamePassword
    {
        public DsfPathCopy()
            : base("Copy")
        {
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {

            Dev2CRUDOperationTO opTO = new Dev2CRUDOperationTO(Overwrite);
            return broker.Copy(scrEndPoint, dstEndPoint, opTO);
        }

        protected override void MoveRemainingIterators()
        {
        }
    }
}
