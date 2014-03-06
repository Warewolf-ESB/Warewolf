using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Editors;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Initilizes a new nstance of <see cref="Infragistics.Controls.Editors.XamSliderThumb{T}" /> types to UI
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class XamSliderThumbAutomationPeer<T> : FrameworkElementAutomationPeer, IValueProvider, ISelectionItemProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderThumbAutomationPeer{T}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamSliderThumbAutomationPeer(XamSliderThumb<T> owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningSliderThumb.PropertyChanged += this.OwningSliderThumb_PropertyChanged;
            this.OwningSliderThumb.ValueChanged += this.OwningSliderThumb_ValueChanged;
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
            return AutomationControlType.Thumb;
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
            return this.OwningSliderThumb.GetType().Name;
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
                return this.OwningSliderThumb.Name;
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
            if (patternInterface == PatternInterface.Value || patternInterface == PatternInterface.SelectionItem)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion Overrides

        #region Properties

        #region OwningSliderThumb
        private XamSliderThumb<T> OwningSliderThumb
        {
            get
            {
                return (XamSliderThumb<T>)Owner;
            }
        }
        #endregion //OwningSliderThumb

        #endregion Properties

        #region Methods

        #region Internal

        #region RaiseIsSelectedPropertyChangedEvent
        /// <summary>
        /// Raises the IsSelectedPropertyChangedEvent.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        internal void RaiseIsSelectedPropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseIsSelectedPropertyChangedEvent

        #region RaiseValuePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseValuePropertyChangedEvent(string oldValue, string newValue)
        {
            this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
        }
        #endregion //RaiseValuePropertyChangedEvent

        #endregion //Internal

        #endregion //Methods

        #region Event Handlers

        #region OwningSliderBase_PropertyChanged
        /// <summary>
        /// Handles the PropertyChanged event of the OwningSliderThumb property as <see cref="XamSliderBase{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OwningSliderThumb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            switch (propertyName)
            {
                case "IsActive":
                    this.RaiseIsSelectedPropertyChangedEvent(!this.OwningSliderThumb.IsActive, this.OwningSliderThumb.IsActive);
                    break;
            }
        }
        #endregion //OwningSliderBase_PropertyChanged

        #region OwningSliderThumb_ValueChanged
        /// <summary>
        /// Handles the ValueChanged event of the OwningSliderThumb property as <see cref="XamSliderThumb{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedPropertyChangedEventArgs&lt;T&gt;"/> instance containing the event data.</param>
        private void OwningSliderThumb_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<T> e)
        {
            this.RaiseValuePropertyChangedEvent(this.OwningSliderThumb.Owner.ToDouble(e.OldValue).ToString(), this.OwningSliderThumb.ResolveValue().ToString());
        }
        #endregion //OwningSliderThumb_ValueChanged

        #endregion Event Handlers

        #region ISelectionItemProvider

        /// <summary>
        /// Adds the current element to the collection of selected items.
        /// </summary>
        public void AddToSelection()
        {
            XamSliderBase<T> parent = this.OwningSliderThumb.Owner;
            if (parent == null)
            {
                throw new InvalidOperationException("Operation cannot be performed");
            }

            parent.ActiveThumb = this.OwningSliderThumb;
        }

        /// <summary>
        /// Gets a value indicating whether an item is selected.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element is selected; otherwise, false.</returns>
        public bool IsSelected
        {
            get { return this.OwningSliderThumb.IsActive; }
        }

        /// <summary>
        /// Removes the current element from the collection of selected items.
        /// </summary>
        public void RemoveFromSelection()
        {
            this.OwningSliderThumb.IsActive = false;
        }

        /// <summary>
        /// Clears any existing selection and then selects the current element.
        /// </summary>
        public void Select()
        {
            this.OwningSliderThumb.IsActive = true;
        }

        /// <summary>
        /// Gets the UI automation provider that implements <see cref="T:System.Windows.Automation.Provider.ISelectionProvider"/> and acts as the container for the calling object.
        /// </summary>
        /// <value></value>
        /// <returns>The UI automation provider.</returns>
        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                XamSliderBase<T> parent = this.OwningSliderThumb.Owner;
                if (parent != null)
                {
                    AutomationPeer peer = FromElement(parent) ?? CreatePeerForElement(parent);

                    if (peer != null)
                    {
                        return ProviderFromPeer(peer);
                    }
                }

                return null;
            }
        }

        #endregion ISelectionItemProvider

        #region IValueProvider

        #region IsReadOnly
        /// <summary>
        /// Gets a value that indicates whether the value of a control is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the value is read-only; false if it can be modified. </returns>
        public bool IsReadOnly
        {
            get
            {
                return !this.OwningSliderThumb.IsEnabled;
            }
        }

        #endregion //IsReadOnly

        #region SetValue
        /// <summary>
        /// Sets the value of slider.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetValue(string value)
        {
            double number;
            bool result = Double.TryParse(value, out number);
            if (result)
            {
                this.OwningSliderThumb.Value = this.OwningSliderThumb.Owner.ToValue(number);
            }
        }
        #endregion //SetValue

        #region Value
        /// <summary>
        /// Gets the value of the XamSlider.
        /// </summary>
        /// <value></value>
        /// <returns>The value of the control.</returns>
        public string Value
        {
            get
            {
                return this.OwningSliderThumb.Owner.ToDouble(this.OwningSliderThumb.Value).ToString();
            }
        }
        #endregion //Value 
        #endregion //IValueProvider
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