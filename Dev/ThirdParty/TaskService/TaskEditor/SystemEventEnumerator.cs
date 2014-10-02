
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
#if NET_35_OR_GREATER
using System.Diagnostics.Eventing.Reader;
#endif

namespace Microsoft.Win32.TaskScheduler
{
	internal static class SystemEventEnumerator
	{
		public static string[] GetEventLogs(string computerName)
		{
#if NET_35_OR_GREATER
			bool isLocal = (string.IsNullOrEmpty(computerName) || computerName == "." || computerName.Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase));
			try
			{
				using (EventLogSession session = isLocal ? new EventLogSession() : new EventLogSession(computerName))
					return new List<string>(session.GetLogNames()).ToArray();
			}
			catch {}
#endif
			return new string[0];
		}

		public static string[] GetEventSources(string computerName, string log)
		{
#if NET_35_OR_GREATER
			bool isLocal = (string.IsNullOrEmpty(computerName) || computerName == "." || computerName.Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase));
			try
			{
				using (EventLogSession session = isLocal ? new EventLogSession() : new EventLogSession(computerName))
					using (EventLogConfiguration ec = new EventLogConfiguration(log, session))
						return new List<string>(ec.ProviderNames).ToArray();
			}
			catch {}
#endif
			return new string[0];
		}
	}
}
