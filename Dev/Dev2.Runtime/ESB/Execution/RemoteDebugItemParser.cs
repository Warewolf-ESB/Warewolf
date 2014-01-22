using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Diagnostics;

namespace Dev2.Runtime.ESB.Execution
{
    public class RemoteDebugItemParser
    {
        /// <summary>
        /// Parses the items.
        /// </summary>
        /// <param name="data">The data.</param>
        public static IList<DebugState> ParseItems(string data)
        {
            try
            {
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                IList<DebugState> debugItems = serializer.Deserialize<List<DebugState>>(data);

                return debugItems;
            }
            catch (Exception e)
            {
                ServerLogger.LogError("RemoteDebugItemParser", e);
            }

            return null;
        }

    }
}
