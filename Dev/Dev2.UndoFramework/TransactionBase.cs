namespace Dev2.UndoFramework
{

    internal class TransactionBase : ITransaction
    {
// ReSharper disable InconsistentNaming
        protected IMultiAction _accumulatingAction;
// ReSharper restore InconsistentNaming

        public TransactionBase()
        {
            IsDelayed = true;
        }

        public TransactionBase(ActionManager am)
            : this()
        {
            ActionManager = am;
            if(am != null)
            {
                am.OpenTransaction(this);
            }
        }

        public TransactionBase(ActionManager am, bool isDelayed)
            : this(am)
        {
            IsDelayed = isDelayed;
        }

        public virtual void Commit()
        {
            if(ActionManager != null)
            {
                ActionManager.CommitTransaction();
            }
        }

        public virtual void Dispose()
        {
            if(!Aborted)
            {
                Commit();
            }
        }

        public virtual void Rollback()
        {
            if(ActionManager != null)
            {
                ActionManager.RollBackTransaction();
                Aborted = true;
            }
        }

        public bool Aborted { get; set; }

        public IMultiAction AccumulatingAction
        {
            get
            {
                return _accumulatingAction;
            }
        }

        public ActionManager ActionManager { get; private set; }

        public bool IsDelayed { get; set; }
    }
}

