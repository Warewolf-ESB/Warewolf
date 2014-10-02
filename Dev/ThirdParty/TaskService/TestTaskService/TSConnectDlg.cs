
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestTaskService
{
	public partial class TSConnectDlg : Form
	{
		public TSConnectDlg()
		{
			InitializeComponent();
		}

		public string TargetServer
		{
			get { return serverText.Text; }
			set { serverText.Text = value; }
		}

		public string User
		{
			get { return userText.Text; }
			set { userText.Text = value; }
		}

		public string Domain
		{
			get { return domainText.Text; }
			set { domainText.Text = value; }
		}

		public string Password
		{
			get { return pwdText.Text; }
			set { pwdText.Text = value; }
		}

		public bool ForceV1
		{
			get { return v1Check.Checked; }
			set { v1Check.Checked = value; }
		}

		private void runButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
