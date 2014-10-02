
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
	public partial class NewFolderDlg : Form
	{
		private string name;

		public NewFolderDlg()
		{
			InitializeComponent();
		}

		public string FolderName
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				nameText.Text = value;
			}
		}

		private void nameText_TextChanged(object sender, EventArgs e)
		{
			okBtn.Enabled = nameText.TextLength > 0;
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void okBtn_Click(object sender, EventArgs e)
		{
			name = nameText.Text;
			Close();
		}
	}
}
