using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Activities.Specs
{
    interface ISpecExternalProcessExecutor : IExternalProcessExecutor
    {
        List<string> WebResult { get; set; }
    }
}