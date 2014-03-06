using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
    // JJD 8/22/08 - added automation support for XamTabControl's ExpandCollapse pattern
    /// <summary>
    /// Exposes the <see cref="XamTabControl"/> to UI Automation.
    /// </summary>
    public class XamTabControlAutomationPeer : TabControlAutomationPeer, IExpandCollapseProvider
    {
        private ExpandCollapseState? _lastReturnedExpandState;

		#region Constructor
		/// <summary>
        /// Initializes a new <see cref="XamTabControlAutomationPeer"/>
		/// </summary>
        /// <param name="tabControl">XamTabControl that this automation peer will represent.</param>
        public XamTabControlAutomationPeer(XamTabControl tabControl)
			: base(tabControl)
		{
		} 
		#endregion //Constructor

		#region Base class overrides

			// AS 7/13/09 TFS18399
			#region CreateItemAutomationPeer
		/// <summary>
		/// Creates a new automation peer for the child item.
		/// </summary>
		/// <param name="item">The item for which the peer is being created</param>
		/// <returns>A new <see cref="TabItemExAutomationPeer"/></returns>
		protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
		{
			return new TabItemExAutomationPeer(item, this);
		} 
			#endregion //CreateItemAutomationPeer

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
			XamTabControl tc = (XamTabControl)this.Owner;
			tc.UpdateLayout();

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
		/// Returns the name of the <see cref="XamTabControl"/>
		/// </summary>
		/// <returns>A string that contains 'XamTabControl'</returns>
		protected override string GetClassNameCore()
		{
			return "XamTabControl";
		}
			#endregion //GetClassNameCore

		    #region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="XamTabControl"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.ExpandCollapse)
			{
				return this;
			}

			return base.GetPattern(patternInterface);
		}
		    #endregion //GetPattern

		#endregion //Base class overrides

        #region Methods

            #region RaiseExpandCollapseStateChanged

        internal void RaiseExpandCollapseStateChanged()
        {
            if (this._lastReturnedExpandState.HasValue &&
                AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                ExpandCollapseState newState = ((IExpandCollapseProvider)this).ExpandCollapseState;

                if (newState != this._lastReturnedExpandState.Value)
                {
                    this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, this._lastReturnedExpandState.Value, newState);
                }
            }
        }
            #endregion //RaiseExpandCollapseStateChanged

            #region VerifyEnabled

        private void VerifyEnabled()
        {
            if (!base.IsEnabled())
                throw new ElementNotEnabledException();

            if (((XamTabControl)this.Owner).AllowMinimize == false)
                throw new InvalidOperationException();
        }

            #endregion //VerifyEnabled	
    
        #endregion //Methods

        #region IExpandCollapseProvider Members

        void IExpandCollapseProvider.Collapse()
        {
            this.VerifyEnabled();

            ((XamTabControl)this.Owner).IsMinimized = true;
        }

        void IExpandCollapseProvider.Expand()
        {
            this.VerifyEnabled();

            ((XamTabControl)this.Owner).IsMinimized = false;
         }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get 
            {
                if (((XamTabControl)this.Owner).AllowMinimize == true)
                {
                    if (((XamTabControl)this.Owner).IsMinimized)
                        this._lastReturnedExpandState = ExpandCollapseState.Collapsed;
                    else
                        this._lastReturnedExpandState = ExpandCollapseState.Expanded;
                }
                else
                    this._lastReturnedExpandState = ExpandCollapseState.LeafNode;

                return this._lastReturnedExpandState.Value;
            }
        }

        #endregion
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