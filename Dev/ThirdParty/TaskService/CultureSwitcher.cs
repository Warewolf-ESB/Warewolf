
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

namespace Microsoft.Win32.TaskScheduler
{
	internal class CultureSwitcher : IDisposable
	{
		System.Globalization.CultureInfo cur, curUI;

		public CultureSwitcher(System.Globalization.CultureInfo culture)
		{
			cur = System.Threading.Thread.CurrentThread.CurrentCulture;
			curUI = System.Threading.Thread.CurrentThread.CurrentUICulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
		}

		public void Dispose()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = cur;
			System.Threading.Thread.CurrentThread.CurrentUICulture = curUI;
		}
	}
}
