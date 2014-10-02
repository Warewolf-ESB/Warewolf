
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.Win32.TaskScheduler;
using System;
using System.Windows.Forms;

namespace SecurityEditor
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//using (TaskService ts = new TaskService())
			{
				var dlg = new AdvancedSecuritySettingsDialog();
				//var dlg = new SecurityPropertiesDialog();
				//dlg.Initialize(ts.GetTask(@"Microsoft\Office\Office 15 Subscription Heartbeat"), false);
				dlg.Initialize(new System.IO.FileInfo(@"C:\RAT2Llog.txt"), false);
				//dlg.Initialize(new System.IO.DirectoryInfo(@"C:\Windows"), false);
				//dlg.Initialize(Microsoft.Win32.Registry.LocalMachine, false);
				//dlg.Initialize(System.IO.MemoryMappedFiles.MemoryMappedFile.CreateNew(@"RAT2Llog.txt", 1024), false);
				Application.Run(dlg);
				//ts.RootFolder.DeleteTask("Test");
			}
		}
	}
}
