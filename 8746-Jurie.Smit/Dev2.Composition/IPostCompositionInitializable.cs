using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Composition
{
    public interface IPostCompositionInitializable
    {
        void Initialize();
        bool AlreadyInitialized { get; }
    }
}
