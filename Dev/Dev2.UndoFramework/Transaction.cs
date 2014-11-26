
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.UndoFramework
{
    internal class Transaction : TransactionBase
    {
        protected Transaction(ActionManager actionManager, bool delayed) : base(actionManager, delayed)
        {
            base._accumulatingAction = new MultiAction();
        }

        public override void Commit()
        {
            base.AccumulatingAction.IsDelayed = base.IsDelayed;
            base.Commit();
        }

        public static Transaction Create(ActionManager actionManager)
        {
            return Create(actionManager, true);
        }

        public static Transaction Create(ActionManager actionManager, bool delayed)
        {
            return new Transaction(actionManager, delayed);
        }
    }
}

