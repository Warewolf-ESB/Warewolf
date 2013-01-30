namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Runtime.CompilerServices;

    internal  class TransactionBase : ITransaction, IDisposable
    {
        protected IMultiAction accumulatingAction;

        public TransactionBase()
        {
            this.IsDelayed = true;
        }

        public TransactionBase(Unlimited.Applications.BusinessDesignStudio.Undo.ActionManager am) : this()
        {
            this.ActionManager = am;
            if (am != null)
            {
                am.OpenTransaction(this);
            }
        }

        public TransactionBase(Unlimited.Applications.BusinessDesignStudio.Undo.ActionManager am, bool isDelayed) : this(am)
        {
            this.IsDelayed = isDelayed;
        }

        public virtual void Commit()
        {
            if (this.ActionManager != null)
            {
                this.ActionManager.CommitTransaction();
            }
        }

        public virtual void Dispose()
        {
            if (!this.Aborted)
            {
                this.Commit();
            }
        }

        public virtual void Rollback()
        {
            if (this.ActionManager != null)
            {
                this.ActionManager.RollBackTransaction();
                this.Aborted = true;
            }
        }

        public bool Aborted { get; set; }

        public IMultiAction AccumulatingAction
        {
            get
            {
                return this.accumulatingAction;
            }
        }

        public Unlimited.Applications.BusinessDesignStudio.Undo.ActionManager ActionManager { get; private set; }

        public bool IsDelayed { get; set; }
    }
}

