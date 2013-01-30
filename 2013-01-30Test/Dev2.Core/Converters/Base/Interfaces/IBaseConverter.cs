using Dev2.Common;

namespace Dev2.Converters
{
    /// <summary>
    /// The interface all conversion operations use
    /// </summary>
    public interface IBaseConverter : ISpookyLoadable
    {
        /// <summary>
        /// Confirms that the payload is of the selected from type
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool IsType(string payload);


        /// <summary>
        /// Returns for spooky action the type that is handled
        /// </summary>
        /// <returns></returns>
        //Brendon.Page 2013.01.16 Commented out because the ISpookyLoadable interface already deffines this member
        //Enum HandlesType();

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
