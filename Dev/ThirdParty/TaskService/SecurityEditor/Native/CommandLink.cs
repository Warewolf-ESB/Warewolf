
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace System.Windows.Forms
{
	public class CommandLink : Button
	{
		const int BS_COMMANDLINK = 0x0000000E;

		public CommandLink()
		{
			this.FlatStyle = FlatStyle.System;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cParams = base.CreateParams;
				cParams.Style |= BS_COMMANDLINK;
				return cParams;
			}
		}
	}
}
