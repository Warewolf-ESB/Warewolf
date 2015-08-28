/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.UndoFramework
{
    public class SimpleHistoryNode
    {
        public SimpleHistoryNode()
        {
        }

        public SimpleHistoryNode(IAction lastExistingAction, SimpleHistoryNode lastExistingState)
        {
            PreviousAction = lastExistingAction;
            PreviousNode = lastExistingState;
        }

        public IAction NextAction { get; set; }

        public SimpleHistoryNode NextNode { get; set; }

        public IAction PreviousAction { get; set; }

        public SimpleHistoryNode PreviousNode { get; set; }
    }
}