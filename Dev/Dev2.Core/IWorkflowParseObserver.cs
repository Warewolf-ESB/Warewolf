using System;
using System.Text;

namespace Dev2
{
    /// <summary>
    /// Told about each fresh parse of a workflow (not a cache hit), with the XAML it came from and the graph it
    /// produced. Lets the server compare a second reader against the WF path (WF removal T1.7 shadow mode)
    /// without Dev2.Runtime.Services referencing that reader. Must never throw into the caller.
    /// </summary>
    public interface IWorkflowParseObserver
    {
        void Parsed(Guid resourceId, StringBuilder xamlDefinition, IDev2Activity result);
    }
}
