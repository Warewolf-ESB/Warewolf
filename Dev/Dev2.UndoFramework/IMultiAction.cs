using System.Collections.Generic;

namespace Dev2.UndoFramework
{
    public interface IMultiAction : IAction, IList<IAction>
    {
        bool IsDelayed { get; set; }
    }
}

