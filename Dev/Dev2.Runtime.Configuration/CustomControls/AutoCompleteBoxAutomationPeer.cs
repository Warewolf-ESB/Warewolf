/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers
{
    /// <summary>
    /// Exposes AutoCompleteBox types to UI Automation.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public sealed class AutoCompleteBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, IExpandCollapseProvider, ISelectionProvider
    {
        /// <summary>
        /// The name reported as the core class name.
        /// </summary>
        private const string AutoCompleteBoxClassNameCore = "AutoCompleteBox";

        /// <summary>
        /// Gets the AutoCompleteBox that owns this
        /// AutoCompleteBoxAutomationPeer.
        /// </summary>
        private AutoCompleteBox OwnerAutoCompleteBox => (AutoCompleteBox)Owner;

        /// <summary>
        /// Gets a value indicating whether the UI automation provider allows
        /// more than one child element to be selected concurrently.
        /// </summary>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        /// <value>True if multiple selection is allowed; otherwise, false.</value>
        bool ISelectionProvider.CanSelectMultiple => false;

        /// <summary>
        /// Gets a value indicating whether the UI automation provider
        /// requires at least one child element to be selected.
        /// </summary>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        /// <value>True if selection is required; otherwise, false.</value>
        bool ISelectionProvider.IsSelectionRequired => false;

        /// <summary>
        /// Initializes a new instance of the AutoCompleteBoxAutomationPeer
        /// class.
        /// </summary>
        /// <param name="owner">
        /// The AutoCompleteBox that is associated with this
        /// AutoCompleteBoxAutomationPeer.
        /// </param>
        public AutoCompleteBoxAutomationPeer(AutoCompleteBox owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Gets the control type for the AutoCompleteBox that is associated
        /// with this AutoCompleteBoxAutomationPeer. This method is called by
        /// GetAutomationControlType.
        /// </summary>
        /// <returns>ComboBox AutomationControlType.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ComboBox;
        }

        /// <summary>
        /// Gets the name of the AutoCompleteBox that is associated with this
        /// AutoCompleteBoxAutomationPeer. This method is called by
        /// GetClassName.
        /// </summary>
        /// <returns>The name AutoCompleteBox.</returns>
        protected override string GetClassNameCore()
        {
            return AutoCompleteBoxClassNameCore;
        }

        /// <summary>
        /// Gets the control pattern for the AutoCompleteBox that is associated
        /// with this AutoCompleteBoxAutomationPeer.
        /// </summary>
        /// <param name="patternInterface">The desired PatternInterface.</param>
        /// <returns>The desired AutomationPeer or null.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            object iface = null;
            AutoCompleteBox owner = OwnerAutoCompleteBox;

            if(patternInterface == PatternInterface.Value)
            {
                iface = this;
            }
            else if(patternInterface == PatternInterface.ExpandCollapse)
            {
                iface = this;
            }
            else
            {
                AutomationPeer peer = owner.SelectionAdapter?.CreateAutomationPeer();
                if(peer != null)
                {
                    iface = peer.GetPattern(patternInterface);
                }
            }

            // ReSharper disable ConvertIfStatementToNullCoalescingExpression
            if(iface == null)
            // ReSharper restore ConvertIfStatementToNullCoalescingExpression
            {
                iface = base.GetPattern(patternInterface);
            }

            return iface;
        }

        #region ExpandCollapse
        /// <summary>
        /// Blocking method that returns after the element has been expanded.
        /// </summary>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        void IExpandCollapseProvider.Expand()
        {
            if(!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            OwnerAutoCompleteBox.IsDropDownOpen = true;
        }

        /// <summary>
        /// Blocking method that returns after the element has been collapsed.
        /// </summary>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        void IExpandCollapseProvider.Collapse()
        {
            if(!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            OwnerAutoCompleteBox.IsDropDownOpen = false;
        }

        /// <summary>
        /// Gets an element's current Collapsed or Expanded state.
        /// </summary>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => OwnerAutoCompleteBox.IsDropDownOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

        /// <summary>
        /// Raises the ExpandCollapse automation event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }
        #endregion ExpandCollapse

        #region ValueProvider

        /// <summary>
        /// Sets the value of a control.
        /// </summary>
        /// <param name="value">The value to set. The provider is responsible
        /// for converting the value to the appropriate data type.</param>
        void IValueProvider.SetValue(string value)
        {
            OwnerAutoCompleteBox.Text = value;
        }

        /// <summary>
        /// Gets a value indicating whether the value of a control is
        /// read-only.
        /// </summary>
        /// <value>True if the value is read-only; false if it can be modified.</value>
        bool IValueProvider.IsReadOnly => !OwnerAutoCompleteBox.IsEnabled;

        /// <summary>
        /// Gets the value of the control.
        /// </summary>
        /// <value>The value of the control.</value>
        string IValueProvider.Value => OwnerAutoCompleteBox.Text ?? string.Empty;

        #endregion

        /// <summary>
        /// Gets the collection of child elements of the AutoCompleteBox that
        /// are associated with this AutoCompleteBoxAutomationPeer. This method
        /// is called by GetChildren.
        /// </summary>
        /// <returns>
        /// A collection of automation peer elements, or an empty collection
        /// if there are no child elements.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required by automation")]
        protected override List<AutomationPeer> GetChildrenCore()
        {
            List<AutomationPeer> children = new List<AutomationPeer>();
            AutoCompleteBox owner = OwnerAutoCompleteBox;

            // TextBox part.
            TextBox textBox = owner.TextBox;
            if(textBox != null)
            {
                AutomationPeer peer = CreatePeerForElement(textBox);
                if(peer != null)
                {
                    children.Insert(0, peer);
                }
            }

            // Include SelectionAdapter's children.
            AutomationPeer selectionAdapterPeer = owner.SelectionAdapter?.CreateAutomationPeer();
            List<AutomationPeer> listChildren = selectionAdapterPeer?.GetChildren();
            if(listChildren != null)
            {
                children.AddRange(listChildren);
            }

            return children;
        }

        /// <summary>
        /// Retrieves a UI automation provider for each child element that is
        /// selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        /// <remarks>
        /// This API supports the .NET Framework infrastructure and is not 
        /// intended to be used directly from your code.
        /// </remarks>
        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            object selectedItem = OwnerAutoCompleteBox.SelectionAdapter?.SelectedItem;
            UIElement uie = selectedItem as UIElement;
            if(uie != null)
            {
                AutomationPeer peer = CreatePeerForElement(uie);
                if(peer != null)
                {
                    return new[] { ProviderFromPeer(peer) };
                }
            }

            return new IRawElementProviderSimple[] { };
        }
    }
}
