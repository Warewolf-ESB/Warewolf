
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Management;

namespace Microsoft.Win32.TaskScheduler
{
	private static class ComputerInfo
	{
		public static string OSFullName
		{
			get
			{
				string result = string.Empty;
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
				foreach (ManagementObject os in searcher.Get())
				{
					result = os["Caption"].ToString();
					break;
				}
				return result;
			}
		}
	}
}
