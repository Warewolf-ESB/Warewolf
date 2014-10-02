
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

namespace Microsoft.Win32.TaskScheduler.UIComponents
{
	[System.ComponentModel.DefaultEvent("KeyValueChanged"), System.ComponentModel.DefaultProperty("Action")]
	internal partial class EmailActionUI : UserControl, IActionHandler
	{
		public EmailActionUI()
		{
			InitializeComponent();
		}

		private void emailAttachementBrowseBtn_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
				emailAttachmentText.Text = openFileDialog1.FileName;
		}

		[System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Action Action
		{
			get
			{
				Action ret = new EmailAction(emailSubjectText.Text, emailFromText.Text,
					emailToText.Text, emailTextText.Text, emailSMTPText.Text);
				if (emailAttachmentText.TextLength > 0)
					((EmailAction)ret).Attachments = new object[] { emailAttachmentText.Text };
				return ret;
			}
			set
			{
				emailFromText.Text = ((EmailAction)value).From;
				emailToText.Text = ((EmailAction)value).To;
				emailSubjectText.Text = ((EmailAction)value).Subject;
				emailTextText.Text = ((EmailAction)value).Body;
				if (((EmailAction)value).Attachments != null && ((EmailAction)value).Attachments.Length > 0)
					emailAttachmentText.Text = ((EmailAction)value).Attachments[0].ToString();
				emailSMTPText.Text = ((EmailAction)value).Server;
			}
		}

		public bool IsActionValid()
		{
			return emailSubjectText.TextLength > 0 && emailFromText.TextLength > 0 &&
				emailToText.TextLength > 0 && emailTextText.TextLength > 0 && emailSMTPText.TextLength > 0;
		}

		public event EventHandler KeyValueChanged;

		private void keyField_TextChanged(object sender, EventArgs e)
		{
			EventHandler h = this.KeyValueChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}
	}
}
