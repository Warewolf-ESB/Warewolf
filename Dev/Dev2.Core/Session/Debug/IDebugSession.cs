
namespace Dev2.Session {
    public interface IDebugSession {

        /// <summary>
        /// Start a debug session
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        DebugTO InitDebugSession(DebugTO to);

        /// <summary>
        /// Save debug session data
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        DebugTO PersistDebugSession(DebugTO to);

    }
}
