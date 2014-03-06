using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.DockManager;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Automation.Peers.DockManager
{
    /// <summary>
    /// Exposes the <see cref="TabGroupPane"/> to UI Automation
    /// </summary>
    public class TabGroupPaneAutomationPeer : TabControlAutomationPeer
    {
		#region Member Variables

		// AS 7/15/11 TFS81248
		private PropertyValueTracker _selectedContentTracker; 

		#endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="TabGroupPaneAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="TabGroupPane"/> for which the peer is being created</param>
        public TabGroupPaneAutomationPeer(TabGroupPane owner)
            : base(owner)
        {
			// AS 7/15/11 TFS81248
			_selectedContentTracker = new PropertyValueTracker(owner, TabControl.SelectedContentProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnSelectedContentChanged), true);
        }
        #endregion //Constructor

        #region Base class overrides

		// AS 2/1/10 TFS26795
		// An ItemsControl only includes the peers for its children.
		//
		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of automation peers that represents the children of the group.
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			List<AutomationPeer> children = base.GetChildrenCore();
			TabGroupPane tc = (TabGroupPane)this.Owner;

			// AS 6/29/11 TFS80361
			// We called UpdateLayout in order to ensure that the child elements are present so we can 
			// include the automation peers of other elements like buttons in the header area. Rather 
			// than force the measure/arrange of other elements which could cause some other automation 
			// processing we'll just make sure the tabgroup's template is applied so we can get its 
			// children.
			//
			//tc.UpdateLayout();
			tc.ApplyTemplate();

			FrameworkElement header = tc.HeaderArea;

			if (null != header)
			{
				List<AutomationPeer> headerChildren = new FrameworkElementAutomationPeer(header).GetChildren();

				if (null != headerChildren)
				{
					Predicate<AutomationPeer> match = delegate(AutomationPeer peer)
					{
						UIElementAutomationPeer uiPeer = peer as UIElementAutomationPeer;

						if (uiPeer == null)
							return true;

						UIElement element = uiPeer.Owner;

						// skip any tab items
						if (ItemsControl.ItemsControlFromItemContainer(element) == tc)
							return false;

						// similarly if we have scroll viewer in the header area
						// then it is probably scrolling the tabs and we want to 
						// exclude that or the tab items will be returned twice 
						// which causes problems for automation clients.
						ScrollViewer sv = element as ScrollViewer;

						if (null != sv)
						{
							if (sv.TemplatedParent == tc)
								return false;
						}

						return true;
					};

					List<AutomationPeer> nonTabChildren = headerChildren.FindAll(match);

					if (null == children)
						children = nonTabChildren;
					else
						children.AddRange(nonTabChildren);
				}
			}

			return children;
		} 
		#endregion //GetChildrenCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="TabGroupPane"/>
        /// </summary>
        /// <returns>A string that contains 'TabGroupPane'</returns>
        protected override string GetClassNameCore()
        {
            return "TabGroupPane";
        }

        #endregion //GetClassNameCore

        #endregion //Base class overrides

		#region Methods

		// AS 7/15/11 TFS81248
		// The TabItemAutomationPeer includes the elements within the ContentPresenter within the tab control so if 
		// that changes then the selected item's children cache needs to be reset so it can be updated.
		//
		#region OnSelectedContentChanged
		private void OnSelectedContentChanged()
		{
			var tgp = this.Owner as TabGroupPane;
			int selectedIndex = tgp.SelectedIndex;

			if (selectedIndex >= 0 && selectedIndex <= tgp.Items.Count)
			{
				var tabItem = tgp.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as TabItem;

				if (null != tabItem)
				{
					AutomationPeer tabItemPeer = UIElementAutomationPeer.FromElement(tabItem);

					if (null != tabItemPeer && null != tabItemPeer.EventsSource)
					{
						tabItemPeer.EventsSource.ResetChildrenCache();
					}
				}
			}
		}
		#endregion //OnSelectedContentChanged 

		#endregion //Methods

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