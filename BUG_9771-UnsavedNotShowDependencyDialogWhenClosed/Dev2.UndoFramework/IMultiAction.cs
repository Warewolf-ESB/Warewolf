namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IMultiAction : IAction, IList<IAction>, ICollection<IAction>, IEnumerable<IAction>, IEnumerable
    {
        bool IsDelayed { get; set; }
    }
}

