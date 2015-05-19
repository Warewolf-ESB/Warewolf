
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.Execution
{
    public class RemoteDebugItemParser
    {
        /// <summary>
        /// Parses the items.
        /// </summary>
        /// <param name="data">The data.</param>
        public static IList<IDebugState> ParseItems(string data)
        {
            try
            {
                // Amend for namespace change ;)
                var parseData = data.Replace("Dev2.Diagnostics.DebugState", "Dev2.Diagnostics.Debug.DebugState");
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                IList<IDebugState> debugItems = serializer.Deserialize<List<IDebugState>>(parseData);

                return debugItems;
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("RemoteDebugItemParser", e);
            }

            return null;
        }

    }
}
