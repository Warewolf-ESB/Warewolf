using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Controls;
using System.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
	// AS 7/13/09 TFS18399
	// Since the XamTabControl supports minimizing and therefore the content can be in a different 
	// ContentPresenter, we need a custom TabItemAutomationPeer that will also get the children 
	// of the MinimizedContentPresenter if the ribbon is minimized.
	//
	/// <summary>
	/// Exposes the <see cref="TabItemEx"/> to UI Automation.
	/// </summary>
	public class TabItemExAutomationPeer : TabItemAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TabItemExAutomationPeer"/>
		/// </summary>
		/// <param name="item">Item that is associated with the peer</param>
		/// <param name="tabControlAutomationPeer">The owning control automation peer</param>
		public TabItemExAutomationPeer(object item, XamTabControlAutomationPeer tabControlAutomationPeer)
			: base(item, tabControlAutomationPeer)
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of automation peers that represents the children of the tab item
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			XamTabControl tc = ((XamTabControlAutomationPeer)this.ItemsControlAutomationPeer).Owner as XamTabControl;

			if (tc != null)
				tc.UpdateLayout();

			List<AutomationPeer> children = base.GetChildrenCore();

			if (tc != null)
			{
				TabItem tab = tc.ItemContainerGenerator.ContainerFromItem(this.Item) as TabItem;

				if (null != tab && tab.IsSelected && tc.IsMinimized)
				{
					ContentPresenter minimizedContent = tc.MinimizedContentPresenter;

					if (null != minimizedContent)
					{
						List<AutomationPeer> dropdownChildren = new FrameworkElementAutomationPeer(minimizedContent).GetChildren();

						if (null != dropdownChildren)
						{
							if (null == children)
								children = dropdownChildren;
							else
								children.AddRange(dropdownChildren);
						}
					}
				}
			}

			return children;
		}
		#endregion //GetChildrenCore

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