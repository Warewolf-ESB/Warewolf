using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public interface IShowDependencyProvider
    {
        void ShowDependencyViewer(IContextualResourceModel resource, IList<string> numberOfDependants);
    }
}