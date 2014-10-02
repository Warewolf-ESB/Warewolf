
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
using System.Windows.Forms;

namespace TestTaskService
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
			this.Icon = Properties.Resources.TaskScheduler;
			radioButtonList1.SelectedIndex = 0;
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void reconnectLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			taskServiceConnectDialog1.ShowDialog(this);
		}

		private void runButton_Click(object sender, EventArgs e)
		{
			System.IO.StringWriter output = new System.IO.StringWriter();

			switch (radioButtonList1.SelectedIndex)
			{
				case 0: // Short
					Program.ShortTest(ts, output);
					break;
				case 1: // Long
					Program.LongTest(ts, output, textBox2.Text);
					break;
				case 2: // Editor
					Program.EditorTest(ts, output);
					break;
				case 3: // Find action
					//Program.FindTaskWithProperty(ts, output, textBox2.Text);
					Program.FluentTest(ts, output);
					break;
				case 4: // Wiz
					Program.WizardTest(ts, output);
					break;
				case 5: // MMC
					Program.MMCTest(ts, output);
					break;
				case 6: // Find task
					Program.FindTask(ts, output, textBox2.Text);
					break;
				case 7: // Output XML
					Program.OutputXml(ts, output);
					break;
				default:
					break;
			}

			textBox1.Text = output.ToString();
		}
	}
}
