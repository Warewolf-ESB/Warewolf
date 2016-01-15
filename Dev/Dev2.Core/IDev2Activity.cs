using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2
{
    public interface IDev2Activity
    {
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        string UniqueID { get; set; }
        IList<IActionableErrorInfo> PerformValidation();
        enFindMissingType GetFindMissingType();
        IDev2Activity Execute(IDSFDataObject data, int update);
        IEnumerable<IDev2Activity> NextNodes { get; set; }
        Guid ActivityId { get; set; }
    }
}