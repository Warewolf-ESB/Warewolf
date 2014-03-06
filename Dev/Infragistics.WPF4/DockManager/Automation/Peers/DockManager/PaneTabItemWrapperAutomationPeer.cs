using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Infragistics.Windows.DockManager;

namespace Infragistics.Windows.Automation.Peers.DockManager
{
	/// <summary>
	/// Exposes the <see cref="PaneTabItem"/> to UI Automation
	/// </summary>
	public class PaneTabItemWrapperAutomationPeer : TabItemWrapperAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneTabItemWrapperAutomationPeer"/>
		/// </summary>
		/// <param name="owner">The associated <see cref="PaneTabItem"/></param>
		public PaneTabItemWrapperAutomationPeer(PaneTabItem owner)
			: base(owner)
		{

		} 
		#endregion //Constructor

		#region Base class overrides

		#region IsOffscreenCore
		/// <summary>
		/// Gets a value that indicates whether the tab element that is associated with this peer is off the screen.
		/// </summary>
		/// <returns>true if the element is not on the screen; otherwise, false.</returns>
		protected override bool IsOffscreenCore()
		{
			bool isOffscreen = base.IsOffscreenCore();

			// AS 2/2/10 TFS27037
			if (isOffscreen)
			{
				PaneTabItem tab = this.Owner as PaneTabItem;
				TabGroupPane tgp = ItemsControl.ItemsControlFromItemContainer(tab) as TabGroupPane;

				// if this is a case where the header area is hidden but the tab control itself is visible...
				if (null != tgp && tgp.IsVisible && !tgp.IsTabItemAreaVisible)
				{
					int tabIndex = tgp.ItemContainerGenerator.IndexFromContainer(tab);

					if (tabIndex == tgp.SelectedIndex)
						isOffscreen = false;
				}
			}

			return isOffscreen;
		}
		#endregion //IsOffscreenCore

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