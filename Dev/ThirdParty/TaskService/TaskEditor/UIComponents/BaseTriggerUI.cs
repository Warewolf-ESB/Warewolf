
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
using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Win32.TaskScheduler.UIComponents
{
	internal partial class BaseTriggerUI : UserControl, ITriggerHandler
	{
		protected bool isV2 = true;
		protected bool onAssignment = false;
		protected Trigger trigger;
		private bool showStart = true;

		public BaseTriggerUI()
		{
			InitializeComponent();
		}

		[Browsable(false), DefaultValue(null)]
		public virtual Trigger Trigger
		{
			get { return trigger; }
			set
			{
				onAssignment = true;
				trigger = value;
				schedStartDatePicker.Value = trigger.StartBoundary;
			}
		}

		[DefaultValue(true)]
		public virtual bool IsV2
		{
			get { return isV2; }
			set { isV2 = value; }
		}

		[DefaultValue(true)]
		public virtual bool ShowStartBoundary
		{
			get { return showStart; }
			set
			{
				if (showStart != value)
				{
					showStart = value;
					panel1.Visible = showStart;
				}
			}
		}

		public virtual bool IsTriggerValid() { return true; }

		private void schedStartDatePicker_ValueChanged(object sender, EventArgs e)
		{
			if (showStart)
				trigger.StartBoundary = schedStartDatePicker.Value;
		}
	}
}
