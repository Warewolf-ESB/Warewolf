using System.Windows.Automation.Peers;
using Infragistics.Controls.Editors;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Controls;
using System;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// An automation peer for ComboEditorBase allowing screenreaders and programatic user interaction.
    /// </summary>
    public class XamComboEditorAutomationPeer<T, TControl> : FrameworkElementAutomationPeer, ISelectionProvider, IExpandCollapseProvider, IValueProvider
        where T : ComboEditorItemBase<TControl>
        where TControl : FrameworkElement
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the XamComboEditorAutomationPeer class.
        /// </summary>
        /// <param name="owner">The ComboEditorBase that this automation peer controls.</param>
        public XamComboEditorAutomationPeer(ComboEditorBase<T, TControl> owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningComboEditor.DropDownOpened += OwningComboEditor_DropDownOpened;
            this.OwningComboEditor.DropDownClosed += OwningComboEditor_DropDownClosed;

			// JM 02-14-12 TFS82804
			//this.OwningComboEditor.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        #endregion Constructors

        #region Overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns the control type for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>A value of the enumeration.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }
        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Overrides the framework invocation requesting a string that describes this control.
        /// </summary>
        /// <returns>A string describing the name of this control.</returns>
        protected override string GetClassNameCore()
        {
            return this.OwningComboEditor.GetType().Name;
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
            if (string.IsNullOrEmpty(nameCore))
            {
                return this.OwningComboEditor.Name;
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
            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }

            return null;
        }
        #endregion //GetPattern

        #endregion Overrides

        #region Properties

        private ComboEditorBase<T, TControl> OwningComboEditor
        {
            get
            {
                return (ComboEditorBase<T, TControl>)Owner;
            }
        }

        #endregion Properties

        #region Methods

        #region Internal

        #region RaiseExpandCollapseStatePropertyChangedEvent
        /// <summary>
        /// Raises the ExpandCollapseStatePropertyChangedEvent.
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

        #region RaiseSelectionSelectionPropertyChangedEvent
		// JM 02-14-12 TFS82804
//#if DEBUG
//        /// <summary>
//        /// Raises the selection selection property changed event.
//        /// </summary>
//        /// <param name="oldValue">The old value.</param>
//        /// <param name="newValue">The new value.</param>
//#endif
//        internal void RaiseSelectionPropertyChangedEvent(int oldValue, int newValue)
//        {
//            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
//            {
//                this.RaisePropertyChangedEvent(SelectionPatternIdentifiers.SelectionProperty, oldValue, newValue);
//            }
//        }
        #endregion //RaiseSelectionSelectionPropertyChangedEvent

        #endregion //Internal

        #endregion //Methods

        #region Event Handlers

        #region OwningComboEditor_DropDownOpened
        /// <summary>
        /// Handles the DropDownOpened event of the OwningComboEditor property as XamWebComboEditor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OwningComboEditor_DropDownOpened(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded);
        }
        #endregion //OwningComboEditor_DropDownOpened

        #region OwningComboEditor_DropDownClosed
        /// <summary>
        /// Handles the DropDownClosed event of the OwningComboEditor property as XamWebComboEditor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OwningComboEditor_DropDownClosed(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed);
        }
        #endregion //OwningComboEditor_DropDownClosed

        #region SelectedItems_CollectionChanged

		// JM 02-14-12 TFS82804
		//private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		//{
		//    int oldItems = 0;
		//    int newItems = 0;

		//    if (e.NewItems != null)
		//    {
		//        newItems = e.NewItems.Count;
		//    }

		//    if (e.OldItems != null)
		//    {
		//        oldItems = e.OldItems.Count;
		//    }

		//    this.RaiseSelectionPropertyChangedEvent(oldItems, newItems);
		//}

        #endregion //SelectedItems_CollectionChanged

        #endregion //Event Handlers

        #region ISelectionProvider Members

        #region CanSelectMultiple

        /// <summary>
        /// Gets a value indicating whether the UI automation provider allows more than one child element to be selected concurrently.
        /// </summary>
        /// <value></value>
        /// <returns>true if multiple selection is allowed; otherwise, false.</returns>
        public bool CanSelectMultiple
        {
            get
            {
                return this.OwningComboEditor.AllowMultipleSelection;
            }
        }

        #endregion //CanSelectMultiple

        #region GetSelection

        /// <summary>
        /// Retrieves a UI automation provider for each child element that is selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        public IRawElementProviderSimple[] GetSelection()
        {
            List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
            if (this.OwningComboEditor.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.OwningComboEditor.SelectedItems.Count; i++)
                {
                    UIElement element = this.OwningComboEditor.CachedRows[this.OwningComboEditor.SelectedItems[i]].Control;
                    AutomationPeer peer = FromElement(element) ?? CreatePeerForElement(element);
                    list.Add(ProviderFromPeer(peer));
                }
            }

            return list.ToArray();
        }

        #endregion //GetSelection

        #region IsSelectionRequired
        /// <summary>
        /// Gets a value indicating whether the UI automation provider requires at least one child element to be selected.
        /// </summary>
        /// <value></value>
        /// <returns>true if selection is required; otherwise, false.</returns>
        public bool IsSelectionRequired
        {
            get
            {
                return false;
            }
        }
        #endregion //IsSelectionRequired

        #endregion

        #region IExpandCollapseProvider Members

        #region Collapse

        /// <summary>
        /// Hides all nodes, controls, or content that are descendants of the control.
        /// </summary>
        public void Collapse()
        {
            this.OwningComboEditor.IsDropDownOpen = false;
        }

        #endregion //Collapse

        #region Expand

        /// <summary>
        /// Displays all child nodes, controls, or content of the control.
        /// </summary>
        public void Expand()
        {
            this.OwningComboEditor.IsDropDownOpen = true;
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
                if (this.OwningComboEditor.IsDropDownOpen)
                {
                    return ExpandCollapseState.Expanded;
                }

                return ExpandCollapseState.Collapsed;
            }
        }

        #endregion //ExpandCollapseState

        #endregion

        #region IValueProvider Members

        /// <summary>
        /// Gets if the <see cref="IValueProvider"/> member is editable.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (!this.OwningComboEditor.IsEnabled || !this.OwningComboEditor.IsEditableResolved);
            }
        }

        /// <summary>
        /// Sets the value to <see cref="IValueProvider"/> object.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            if (this.OwningComboEditor.IsEditableResolved)
            {
                this.OwningComboEditor.TextEditor.Text = value;
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="IValueProvider"/> object.
        /// </summary>
        public string Value
        {
            get
            {
                string result = string.Empty;

                if (this.OwningComboEditor.IsEditableResolved)
                {
                    result = this.OwningComboEditor.TextEditor.Text;
                }
                else
                {
                    foreach (var item in this.OwningComboEditor.MultiSelectPanel.Children)
                    {
                        if (item.GetType().Equals(typeof(ContentControl)))
                        {
                            result += (item as ContentControl).Content.ToString();
                        }
                        else if (item.GetType().Equals(typeof(TextBlock)))
                        {
                            result += (item as TextBlock).Text;
                        }
                    }
                }

                return result;
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