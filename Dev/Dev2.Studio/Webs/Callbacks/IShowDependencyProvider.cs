using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;

namespace Dev2.Webs.Callbacks
{
    public interface IShowDependencyProvider
    {
        void ShowDependencyViewer(IContextualResourceModel resource, IList<string> numberOfDependants);
    }
}