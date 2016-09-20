using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestStep
    {
        Guid UniqueId { get; set; }
        string ActivityType { get; set; }
        StepType Type { get; set; }
        List<IServiceTestOutput> Outputs { get; set; }

    }
}