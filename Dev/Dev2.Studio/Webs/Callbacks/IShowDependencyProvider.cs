using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public interface IShowDependencyProvider
    {
        void ShowDependencyViewer(IContextualResourceModel resource, IList<string> numberOfDependants);
    }
}