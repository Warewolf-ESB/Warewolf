namespace Unlimited.Applications.BusinessDesignStudio.Undo
{
    using System.Collections.Generic;

    public interface IMultiAction : IAction, IList<IAction>
    {
        bool IsDelayed { get; set; }
    }
}

