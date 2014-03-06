using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using Infragistics.Windows.Ribbon;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
	// AS 6/24/09 TFS18346
	/// <summary>
	/// Exposes <see cref="LabelTool"/> types to UI Automation
	/// </summary>
	public class LabelToolAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="LabelToolAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="LabelTool"/> for which the peer is being created</param>
		public LabelToolAutomationPeer(LabelTool owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Text</b> enumeration value</returns>
		override protected AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Text;
		}
		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="LabelTool"/>
		/// </summary>
		/// <returns>A string that contains 'LabelTool'</returns>
		protected override string GetClassNameCore()
		{
			return "LabelTool";
		}

		#endregion //GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Gets a human readable name for <see cref="LabelTool"/>. 
		/// </summary>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();

			if (string.IsNullOrEmpty(name))
			{
				LabelTool tool = (LabelTool)this.Owner;

				if (tool.Content is string)
					name = Utilities.StripMnemonics(tool.Content as string, false);
			}

			return ButtonToolAutomationPeer.GetToolName(this.Owner, name);
		}
		#endregion //GetNameCore

		#endregion //Base class overrides
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved