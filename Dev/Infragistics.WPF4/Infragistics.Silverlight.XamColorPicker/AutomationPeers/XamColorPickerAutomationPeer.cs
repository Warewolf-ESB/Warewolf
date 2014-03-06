using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Editors;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using Infragistics.Controls.Editors.Primitives;
using System.Diagnostics;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="XamColorPicker" /> types to UI
    /// </summary>
    public class XamColorPickerAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        IValueProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamColorPickerAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="XamColorPicker"/> that this automation peer controls.</param>
        public XamColorPickerAutomationPeer(XamColorPicker owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningColorPicker.DropDownOpened += OwningColorPicker_DropDownOpened;
            this.OwningColorPicker.DropDownClosed += OwningColorPicker_DropDownClosed;
        }

        #endregion // Constructors

        #region Properties

        #region OwningColorPicker

        private XamColorPicker OwningColorPicker
        {
            get
            {
                return (XamColorPicker)this.Owner;
            }
        }

        #endregion // OwningColorPicker

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
                if (this.OwningColorPicker.IsDropDownOpen)
                {
                    return ExpandCollapseState.Expanded;
                }

                return ExpandCollapseState.Collapsed;
            }
        }

        #endregion //ExpandCollapseState

        #region IsReadOnly

        /// <summary>
        /// Gets if the <see cref="XamColorPicker"/> is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (!this.OwningColorPicker.IsEnabled);
            }
        }

        #endregion // IsReadOnly

        #region CanSelectMultiple

        /// <summary>
        /// Gets a value indicating whether the UI automation provider allows more than one child element to be selected concurrently.
        /// </summary>
        /// <value></value>
        /// <returns>true if multiple selection is allowed; otherwise, false.</returns>
        protected bool CanSelectMultiple
        {
            get
            {
                return false;
            }
        }

        #endregion // CanSelectMultiple

        #region IsSelectionRequired

        /// <summary>
        /// Gets a value indicating whether the UI automation provider requires at least one child element to be selected.
        /// </summary>
        /// <value></value>
        /// <returns>true if selection is required; otherwise, false.</returns>
        protected bool IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        #endregion // IsSelectionRequired

        #region Value
        private string Value
        {
            get
            {
                if (this.OwningColorPicker.SelectedColor == null)
                {
                    return null;
                }
                return this.OwningColorPicker.SelectedColor.ToString();
            }
        }
        #endregion // Value

        #endregion // Properties

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
        /// Returns the name of the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName"/>.
        /// </summary>
        /// <returns>
        /// The name of the owner type that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. See Remarks.
        /// </returns>
        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
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
                nameCore = this.OwningColorPicker.Name;
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

        #endregion //  Overrides

        #region Methods

        #region Internal

        #region RaiseValuePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseValuePropertyChangedEvent(Color? oldValue, Color? newValue)
        {
            if (null != this.GetPattern(PatternInterface.Value))
            {
                string oldValueString = oldValue == null ? null : oldValue.ToString();
                string newValueString = newValue == null ? null : newValue.ToString();
                this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValueString, newValueString);
            }
        }

        #endregion //RaiseValuePropertyChangedEvent

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

        #endregion // Internal

        #region Protected

        #region Collapse

        /// <summary>
        /// Hides all nodes, controls, or content that are descendants of the control.
        /// </summary>
        protected virtual void Collapse()
        {
            this.OwningColorPicker.IsDropDownOpen = false;
        }

        #endregion // Collapse

        #region Expand
        /// <summary>
        /// Displays all child nodes, controls, or content of the control.
        /// </summary>
        protected virtual void Expand()
        {
            this.OwningColorPicker.IsDropDownOpen = true;
        }

        #endregion // Expand

        #region SetSelectedColor

        /// <summary>
        /// Attempts to set the value of the control from an ARGB representation of the color in string form.
        /// </summary>        
        /// <param name="colorInARGB">The expected string format is '#AARRGGBB', with each part being the HEX value for the argument.</param>
        protected virtual void SetSelectedColor(string colorInARGB)
        {
            string formattingError = "The expected string format is '#AARRGGBB', with each part being the HEX value for the argument.";

            if (string.IsNullOrEmpty(colorInARGB) || colorInARGB.Length != 9)
            {
                throw new ArgumentException(formattingError);
            }

            string alpha = colorInARGB.Substring(1, 2);
            string red = colorInARGB.Substring(3, 2);
            string green = colorInARGB.Substring(5, 2);
            string blue = colorInARGB.Substring(7, 2);

            Color c = Color.FromArgb(ConvertHexStringToByte(alpha),
                ConvertHexStringToByte(red),
               ConvertHexStringToByte(green),
                ConvertHexStringToByte(blue));

            this.OwningColorPicker.SelectedColor = c;
        }

        #endregion // SetSelectedColor

        #region GetSelection

        /// <summary>
        /// Retrieves a UI automation provider for each child element that is selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        protected IRawElementProviderSimple[] GetSelection()
        {
            List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
            if (this.OwningColorPicker.SelectedColor != null)
            {
                if (this.OwningColorPicker.ColorStripManager != null)
                {
                    if (this.OwningColorPicker.ColorStripManager.SelectedColorItem != null)
                    {
                        ColorItemBox box = this.OwningColorPicker.ColorStripManager.SelectedColorItem.ColorItemBox;

                        if (box != null)
                        {
                            AutomationPeer peer = FromElement(box) ?? CreatePeerForElement(box);
                            list.Add(ProviderFromPeer(peer));
                        }
                    }
                }
            }

            return list.ToArray();
        }

        #endregion // GetSelection

        #endregion // Protected

        #region Private

        #region ConvertHexStringToByte
        private byte ConvertHexStringToByte(string s)
        {
            // assumes that the hex string is 2 characters long

            s = s.ToUpper();

            char high = s[0];

            char low = s[1];

            int value = 0;

            if (Char.IsDigit(high))
            {
                int x = Convert.ToInt16(high.ToString());

                value = x * 16;
            }
            else
            {
                int diff = high - 'A';
                value = (10 + diff) * 16;
            }

            if (Char.IsDigit(low))
            {
                int x = Convert.ToInt16(low.ToString());

                value += x;
            }
            else
            {
                int diff = low - 'A';
                value += (10 + diff);
            }

            return Convert.ToByte(value);
        }
        #endregion // ConvertHexStringToByte

        #endregion // Private

        #endregion // Methods

        #region IExpandCollapseProvider Members

        void IExpandCollapseProvider.Collapse()
        {
            this.Collapse();
        }

        void IExpandCollapseProvider.Expand()
        {
            this.Expand();
        }

        System.Windows.Automation.ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get { return this.ExpandCollapseState; }
        }

        #endregion

        #region IValueProvider Members

        bool IValueProvider.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        void IValueProvider.SetValue(string value)
        {
            this.SetSelectedColor(value);
        }

        string IValueProvider.Value
        {
            get
            {
                return this.Value;
            }
        }

        #endregion

        #region Event Handlers

        #region OwningColorPicker_DropDownOpened
        /// <summary>
        /// Handles the <see cref="XamColorPicker.DropDownOpened"/> event of the owning <see cref="XamColorPicker"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OwningColorPicker_DropDownOpened(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Collapsed, ExpandCollapseState.Expanded);
        }
        #endregion // OwningColorPicker_DropDownOpened

        #region OwningColorPicker_DropDownClosed
        /// <summary>
        /// Handles the <see cref="XamColorPicker.DropDownClosed"/> event of the owning <see cref="XamColorPicker"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OwningColorPicker_DropDownClosed(object sender, EventArgs e)
        {
            this.RaiseExpandCollapseStatePropertyChangedEvent(ExpandCollapseState.Expanded, ExpandCollapseState.Collapsed);
        }
        #endregion // OwningColorPicker_DropDownClosed

        #endregion // Event Handlers
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