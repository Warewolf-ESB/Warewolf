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

