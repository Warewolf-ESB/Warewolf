
namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    /// Defines the requirements for a debug writer
    /// </summary>
    public interface IDebugWriter
    {
        /// <summary>
        /// Writes the given state.
        /// <remarks>
        /// This must implement the one-way (fire and forget) message exchange pattern.
        /// </remarks>
        /// </summary>
        /// <param name="debugState">The state to be written.</param>
        void Write(IDebugState debugState);
    }
}
