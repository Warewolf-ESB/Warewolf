
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.PathOperations;
using Dev2.PathOperations;

namespace Dev2.Tests.Activities.Mocks
{
    public class MockAbstractMultipleFilesActivity : DsfAbstractMultipleFilesActivity
    {
        public MockAbstractMultipleFilesActivity(string displayName)
            : base(displayName)
        {
            ExecuteBrokerCalled = false;
            MoveRemainingIteratorsCalled = false;
        }

        #region Overrides of DsfAbstractMultipleFilesActivity
        public bool ExecuteBrokerCalled { get; set; }
        public bool MoveRemainingIteratorsCalled { get; set; }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            ExecuteBrokerCalled = true;
            return "";
        }

        protected override void MoveRemainingIterators()
        {
            MoveRemainingIteratorsCalled = true;
        }

        #endregion
    }
}
