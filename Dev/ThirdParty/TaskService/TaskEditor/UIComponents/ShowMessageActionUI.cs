
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
	internal partial class ShowMessageActionUI : UserControl, IActionHandler
	{
		public ShowMessageActionUI()
		{
			InitializeComponent();
		}

		[System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Action Action
		{
			get
			{
				return new ShowMessageAction(msgMsgText.Text, msgTitleText.Text);
			}
			set
			{
				msgTitleText.Text = ((ShowMessageAction)value).Title;
				msgMsgText.Text = ((ShowMessageAction)value).MessageBody;
			}
		}

		public bool IsActionValid()
		{
			return msgMsgText.TextLength > 0;
		}

		public event EventHandler KeyValueChanged;

		private void msgMsgText_TextChanged(object sender, EventArgs e)
		{
			EventHandler h = this.KeyValueChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}
	}
}
