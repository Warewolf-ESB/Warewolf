namespace Dev2.Common
{
    /// <summary>
    /// Studio logger
    /// </summary>
    public static class StudioLogger
    {
        
        public static void LogMessage(string message)
        {
            ServerLogger.EnableInfoOutput = true;
            ServerLogger.LogMessage(message);
        }
    }
}