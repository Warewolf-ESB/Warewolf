using System;
using Dev2.Common.Interfaces.Patterns;

namespace Dev2.Common.Interfaces.Core.Convertors.Base
{
    /// <summary>
    /// The interface all conversion operations use
    /// </summary>
    public interface IBaseConverter : ISpookyLoadable<Enum>
    {
        /// <summary>
        /// Confirms that the payload is of the selected from type
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool IsType(string payload);

        /// <summary>
        /// Convert to the selected type
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        string ConvertToBase(byte[] payload);

        /// <summary>
        /// Neutralize to a single common format
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        byte[] NeutralizeToCommon(string payload);
    }
}
