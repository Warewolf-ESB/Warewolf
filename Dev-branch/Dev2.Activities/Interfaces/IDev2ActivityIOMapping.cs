using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// Travis.Frisinger : 28.11.2012
    /// Moved there here for the ForEach activity
    /// </summary>
    public interface IDev2ActivityIOMapping
    {
        string InputMapping { get; set; }
        string OutputMapping { get; set; }

    }
}
