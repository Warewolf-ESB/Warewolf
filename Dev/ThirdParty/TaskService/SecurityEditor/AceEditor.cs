
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Security.AccessControl;
using System.Windows.Forms;

namespace SecurityEditor
{
	internal partial class AceEditor : Form
	{
		private string objName;
		private CommonObjectSecurity sec;

		public AceEditor()
		{
			InitializeComponent();
		}

		public CommonObjectSecurity ObjectSecurity
		{
			get { return sec; }
			set
			{
				sec = value;
				accessPermissionList1.Initialize(sec);
			}
		}

		public string ObjectName
		{
			get { return objName; }
			set { objName = value; SetTitle(); }
		}

		public SecurityRuleType Display { get; set; }

		private void SetTitle()
		{
			this.Text = string.Format("Permission Entry for {0}", objName);
		}

		private void changeNameBtn_Click(object sender, System.EventArgs e)
		{
			string acctName = string.Empty, sid; bool isGroup, isService;
			if (Microsoft.Win32.TaskScheduler.HelperMethods.SelectAccount(this, null, ref acctName, out isGroup, out isService, out sid))
			{
				var si = new System.Security.Principal.SecurityIdentifier(sid);
				nameText.Text = acctName;
				accessPermissionList1.CurrentSid = si;
			}
		}

		private void clearAllBtn_Click(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void noInheritCheck_CheckedChanged(object sender, System.EventArgs e)
		{

		}

		private void applyToCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{

		}
	}
}
