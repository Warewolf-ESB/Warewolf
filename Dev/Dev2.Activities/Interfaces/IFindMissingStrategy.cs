using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Patterns;

namespace Dev2.Interfaces
{
    public interface IFindMissingStrategy : ISpookyLoadable<Enum>
    {
        /// <summary>
        /// Gets all the fields for a specific activity
        /// </summary>
        /// <param name="activity">The activity that the fields will be retrieved from</param>
        /// <returns>Returns all the fields in a list of strings</returns>
        List<string> GetActivityFields(object activity);
    }
}
