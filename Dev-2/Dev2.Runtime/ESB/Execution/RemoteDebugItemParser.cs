using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Diagnostics;
using Newtonsoft.Json;
using System.Xml;

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
                int start = data.IndexOf("<" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);

                if (start >= 0)
                {
                    start += GlobalConstants.ManagementServicePayload.Length + 2;

                    int end = data.IndexOf("</" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);

                    if (end > start)
                    {
                        var tmp = data.Substring(start, (end - start));
                        IList<DebugState> debugItems = JsonConvert.DeserializeObject<List<DebugState>>(tmp);

                        return debugItems;
                    }
                }
            }
            catch (Exception e)
            {
                ServerLogger.LogError(e);
            }

            return null;
        }

    }
}
