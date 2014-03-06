using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Infragistics.Controls.Menus;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes XamMenuItem types to UI Automation.
    /// </summary>
    public class XamMenuItemAutomationPeer : ItemsControlAutomationPeer, IExpandCollapseProvider, IToggleProvider, IInvokeProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamMenuItemAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamMenuItemAutomationPeer(XamMenuItem owner) : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningMenuItem.Checked += OwningMenuItem_Checked;
            this.OwningMenuItem.SubmenuClosed += OwningMenuItem_SubmenuClosed;
            this.OwningMenuItem.SubmenuOpened += OwningMenuItem_SubmenuOpened;
            this.OwningMenuItem.Unchecked += OwningMenuItem_Unchecked;
        }

        #endregion //Constructors

        #region Overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns the control type for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>A value of the enumeration.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.MenuItem;
        }
        #endregion //GetAutomationControlTypeCore

        #region GetChildrenCore
        /// <summary>
        /// Returns the collection of child elements of the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren"/>.
        /// </summary>
        /// <returns>
        /// A list of child <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> elements.
        /// </returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            ItemCollection items = this.OwningMenuItem.Items;
            if (items.Count <= 0)
            {
                return null;
            }

            List<AutomationPeer> list = new List<AutomationPeer>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                UIElement element = this.OwningMenuItem.ItemContainerGenerator.ContainerFromIndex(i) as UIElement;
                if (element != null)
                {
                    AutomationPeer peer = FromElement(element) ?? CreatePeerForElement(element);
                    if (peer != null)
                    {
                        list.Add(peer);
                    }
                }
            }

            return list;
        }
        #endregion //GetChildrenCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName"/>.
        /// </summary>
        /// <returns>
        /// The name of the owner type that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. See Remarks.
        /// </returns>
        protected override string GetClassNameCore()
        {
            return "XamMenuItem";
        }
        #endregion //GetClassNameCore

        #region GetNameCore
        /// <summary>
        /// Returns the text label of the <see cref="T:System.Windows.FrameworkElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName"/>.
        /// </summary>
        /// <returns>
        /// The text label of the element that is associated with this automation peer.
        /// </returns>
        protected override string GetNameCore()
        {
            string nameCore = base.GetNameCore();
            if (string.IsNullOrEmpty(nameCore) && (this.OwningMenuItem.Header is string))
            {
                nameCore = (string)this.OwningMenuItem.Header;
            }

            return nameCore;
        }

        #endregion //GetNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">One of the enumeration values.</param>
        /// <returns>See Remarks.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if ((patternInterface == PatternInterface.Invoke) && !this.OwningMenuItem.HasChildren)
            {
                return this;
            }

            if ((patternInterface == PatternInterface.ExpandCollapse) && this.OwningMenuItem.HasChildren)
            {
                return this;
            }

            if ((patternInterface == PatternInterface.Toggle) && this.OwningMenuItem.IsCheckable)
            {
                return this;
            }

            return null;
        }
        #endregion //GetPattern

        #region CreateItemAutomationPeer
        /// <summary>
        /// When overridden in a derived class, creates a new instance of the System.Windows.Automation.Peers.ItemAutomationPeer
        /// for a data item in the System.Windows.Controls.ItemsControl.Items collection
        /// of this System.Windows.Controls.ItemsControl.
        /// </summary>
        /// <param name="item">The data item that is associated with this System.Windows.Automation.Peers.ItemAutomationPeer.</param>
        /// <returns>The new System.Windows.Automation.Peers.ItemAutomationPeer created.</returns>
        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            ItemAutomationPeer peer = null;
            if (item is UIElement)
            {
                peer = FrameworkElementAutomationPeer.CreatePeerForElement((UIElement)item) as ItemAutomationPeer;
            }
            return peer;
        }
        #endregion // CreateItemAutomationPeer

        #endregion //Overrides

        #region Properties

        private XamMenuItem OwningMenuItem
        {
            get
            {
                return (XamMenuItem)Owner;
            }
        }

        #endregion Properties

        #region Methods

        #region Internal

        #region RaiseExpandCollapseStatePropertyChangedEvent
        /// <summary>
        /// Raises the ExpandCollapseStatePropertyChanged event.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        internal void RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState oldValue, ExpandCollapseState newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue, newValue);
            }

        }

        #endregion //RaiseExpandCollapseStatePropertyChangedEvent

        #region RaiseToggleStatePropertyChangedEvent
        /// <summary>
        /// Raises the ToggleStatePropertyChanged event.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        internal void RaiseToggleStatePropertyChangedEvent(ToggleState oldValue, ToggleState newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseToggleStatePropertyChangedEvent

        #endregion Internal

        #endregion Methods

        #region Event Handlers

        #region OwningMenuItem_Checked
        /// <summary>
        /// Handles the Checked event of the OwningMenuItem property as XamMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OwningMenuItem_Checked(object sender, EventArgs e)
        {
            this.RaiseToggleStatePropertyChangedEvent(ToggleState.On, ToggleState.Off);
        }
        #endregion //OwningMenuItem_Checked

        #region OwningMenuItem_SubmenuClosed
        /// <summary>
        /// Handles the SubmenuClosed event of the OwningMenuItem property as XamMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OwningMenuItem_SubmenuClosed(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Expanded,
                                                             ExpandCollapseState.Collapsed);
        }
        #endregion //OwningMenuItem_SubmenuClosed


        #region OwningMenuItem_SubmenuOpened
        /// <summary>
        /// Handles the SubmenuOpened event of the OwningMenuItem property as XamMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OwningMenuItem_SubmenuOpened(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Collapsed,
                                                              ExpandCollapseState.Expanded);
        }
        #endregion //OwningMenuItem_SubmenuOpened

        #region OwningMenuItem_Unchecked
        /// <summary>
        /// Handles the Unchecked event of the OwningMenuItem property as XamMenuItem.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OwningMenuItem_Unchecked(object sender, EventArgs e)
        {
            this.RaiseToggleStatePropertyChangedEvent(ToggleState.Off, ToggleState.On);
        }
        #endregion //OwningMenuItem_Unchecked


        #endregion Event Handlers

        #region IExpandCollapseProvider

        #region Collapse
        /// <summary>
        /// Hides all nodes, controls, or content that are descendants of the control.
        /// </summary>
        public void Collapse()
        {
            if (!this.OwningMenuItem.HasChildren)
            {
                throw new InvalidOperationException();
            }

            this.OwningMenuItem.CloseSubmenu();
        }
        #endregion //Collapse

        #region Expand
        /// <summary>
        /// Displays all child nodes, controls, or content of the control.
        /// </summary>
        public void Expand()
        {
            if (!this.OwningMenuItem.HasChildren)
            {
                throw new InvalidOperationException();
            }

            this.OwningMenuItem.OpenSubmenu();
        }
        #endregion //Expand

        #region ExpandCollapseState
        /// <summary>
        /// Gets the state (expanded or collapsed) of the control.
        /// </summary>
        /// <value></value>
        /// <returns>The state (expanded or collapsed) of the control.</returns>
        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                if (!this.OwningMenuItem.HasChildren)
                {
                    return ExpandCollapseState.LeafNode;
                }

                if (!this.OwningMenuItem.IsSubmenuOpen)
                {
                    return ExpandCollapseState.Collapsed;
                }

                return ExpandCollapseState.Expanded;
            }
        }
        #endregion //ExpandCollapseState

        #endregion //IExpandCollapseProvider

        #region IInvokeProvider

        /// <summary>
        /// Sends a request to activate a control and initiate its single, unambiguous action.
        /// </summary>
        public void Invoke()
        {
            this.OwningMenuItem.AutomationMenuItemClick();
        }
        #endregion //IInvokeProvider

        #region IToggleProvider

        #region ToggleState
        /// <summary>
        /// Gets the toggle state of the control.
        /// </summary>
        /// <value></value>
        /// <returns>The toggle state of the control, as a value of the enumeration. </returns>
        public ToggleState ToggleState
        {
            get
            {
                if (!this.OwningMenuItem.IsChecked)
                {
                    return ToggleState.Off;
                }

                return ToggleState.On;
            }
        }
        #endregion //ToggleState

        #region Toggle
        /// <summary>
        /// Cycles through the toggle states of a control.
        /// </summary>
        public void Toggle()
        {
            if (!this.OwningMenuItem.IsCheckable)
            {
                throw new InvalidOperationException();
            }

            this.OwningMenuItem.IsChecked = !this.OwningMenuItem.IsChecked;
            this.OwningMenuItem.ChangeVisualState(false);
        }
        #endregion //Toggle

        #endregion //IToggleProvider
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