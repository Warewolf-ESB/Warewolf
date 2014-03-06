using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Editors;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    ///  Exposes <see cref="Infragistics.Controls.Editors.XamSliderBase{T}" /> types to UI
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class XamSliderBaseAutomationPeer<T> : FrameworkElementAutomationPeer, IRangeValueProvider, ISelectionProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamSliderBaseAutomationPeer{T}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamSliderBaseAutomationPeer(XamSliderBase<T> owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningSliderBase.IsEnabledChanged += OwningSliderBase_IsEnabledChanged;
            this.OwningSliderBase.PropertyChanged += OwningSliderBase_PropertyChanged;
            this.OwningSliderBase.ThumbValueChanged += OwningSliderBase_ThumbValueChanged;

            this.OldLargeChange = this.LargeChange;
            this.OldMaximum = this.Maximum;
            this.OldMinimum = this.Minimum;
            this.OldSmallChange = this.SmallChange;
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
            return AutomationControlType.Slider;
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
            ObservableCollection<XamSliderThumb<T>> items = this.OwningSliderBase.Thumbs;
            if (items.Count <= 0)
            {
                return null;
            }

            List<AutomationPeer> list = new List<AutomationPeer>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                UIElement element = this.OwningSliderBase.Thumbs[i];
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
            return this.OwningSliderBase.GetType().Name;
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
                return this.OwningSliderBase.Name;
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
            if(patternInterface == PatternInterface.Selection || patternInterface == PatternInterface.RangeValue)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion //Overrides

        #region Properties

        #region OldLargeChange

        private double OldLargeChange {get; set;}

        #endregion OldLargeChange

        #region OldMaximum

        private double OldMaximum { get; set; }

        #endregion OldMaximum

        #region OldMinimum

        private double OldMinimum { get; set; }

        #endregion OldMinimum

        #region OldSmallChange

        private double OldSmallChange { get; set; }

        #endregion OldSmallChange


        #region OwningSliderBase
        private XamSliderBase<T> OwningSliderBase
        {
            get
            {
                return (XamSliderBase<T>)Owner;
            }
        }
        #endregion //OwningSliderBase

        #endregion //Properties

        #region Methods

        #region Internal

        #region RaiseIsReadOnlyPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseIsReadOnlyPropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.IsReadOnlyProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseIsReadOnlyPropertyChangedEvent

        #region RaiseLargeChangePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseLargeChangePropertyChangedEvent(double oldValue, double newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.LargeChangeProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseLargeChangePropertyChangedEvent

        #region RaiseMaximumPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseMaximumPropertyChangedEvent(double oldValue, double newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MaximumProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseMaximumPropertyChangedEvent

        #region RaiseMinimumPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseMinimumPropertyChangedEvent(double oldValue, double newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MinimumProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseMinimumPropertyChangedEvent

        #region RaiseSmallChangePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseSmallChangePropertyChangedEvent(double oldValue, double newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.SmallChangeProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseSmallChangePropertyChangedEvent

        #region RaiseValuePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseValueThumbPropertyChangedEvent(double oldValue, double newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseValuePropertyChangedEvent

        #endregion Internal

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the IsEnabledChanged event of the OwningSliderBase property as <see cref="XamSliderBase{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        void OwningSliderBase_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.RaiseIsReadOnlyPropertyChangedEvent(!this.OwningSliderBase.IsEnabled, this.OwningSliderBase.IsEnabled);
        }


        #region OwningSliderBase_PropertyChanged
        /// <summary>
        /// Handles the PropertyChanged event of the OwningSliderBase property as <see cref="XamSliderBase{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void OwningSliderBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            switch (propertyName)
            {
                case "LargeChange":
                    this.RaiseLargeChangePropertyChangedEvent(this.OldLargeChange, this.LargeChange);
                    this.OldLargeChange = this.LargeChange;
                    break;
                case "Maximum":
                    this.RaiseMaximumPropertyChangedEvent(this.OldMaximum, this.Maximum);
                    this.OldMaximum = this.Maximum;
                    break;
                case "Minimum":
                    this.RaiseMaximumPropertyChangedEvent(this.OldMinimum, this.Minimum);
                    this.OldMinimum = this.Minimum;
                    break;
                case "SmallChange":
                    this.RaiseMaximumPropertyChangedEvent(this.OldSmallChange, this.SmallChange);
                    this.OldSmallChange = this.SmallChange;
                    break;
            }
        }
        #endregion //OwningSliderBase_PropertyChanged


        #region OwningSliderBase_ThumbValueChanged
        /// <summary>
        /// Handles the ThumbValueChanged event of the OwningSliderBase property as <see cref="XamSliderBase{T}"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Infragistics.Controls.Editors.ThumbValueChangedEventArgs&lt;T&gt;"/> instance containing the event data.</param>
        void OwningSliderBase_ThumbValueChanged(object sender, ThumbValueChangedEventArgs<T> e)
        {
            XamSliderBase<T> slider = sender as XamSliderBase<T>;
            if (slider != null)
            {
                double oldValue = slider.ToDouble(e.OldValue);
                double newValue = slider.ToDouble(e.NewValue);

                // Raises an automation event
                if (oldValue != newValue)
                {
                    this.RaiseValueThumbPropertyChangedEvent(oldValue, newValue);
                }
            }
        }
        #endregion //OwningSliderBase_ThumbValueChanged

        #endregion Event Handlers

        #region ISelectionProvider

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
                return false;
            }
        }
        #endregion //CanSelectMultiple

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
                return true;
            }
        }
        #endregion //IsSelectionRequired

        #region GetSelection
        /// <summary>
        /// Retrieves a UI automation provider for each child element that is selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        public IRawElementProviderSimple[] GetSelection()
        {
            List<IRawElementProviderSimple> selection = new List<IRawElementProviderSimple>(1);

            XamSliderThumb<T> item = this.OwningSliderBase.ActiveThumb;

            if (item != null)
            {
                AutomationPeer peer = FromElement(item) ?? CreatePeerForElement(item);

                if (peer != null)
                {
                    selection.Add(ProviderFromPeer(peer));
                }
            }

            return selection.ToArray();
        }
        #endregion //GetSelection

        #endregion ISelectionProvider

        #region IValueRangeProvider

        #region Properties

        #region IsReadOnly
        /// <summary>
        /// Gets a value indicating whether the value of a control is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the value is read-only; false if it can be modified. </returns>
        public bool IsReadOnly
        {
            get
            {
                return !this.OwningSliderBase.IsEnabled;
            }
        }
        #endregion //IsReadOnly

        #region LargeChange
        /// <summary>
        /// Gets the value that is added to or subtracted from the <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.Value"/> property when a large change is made, such as with the PAGE DOWN key.
        /// </summary>
        /// <value></value>
        /// <returns>The large-change value that is supported by the control, or null if the control does not support <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.LargeChange"/>. </returns>
        public double LargeChange
        {
            get
            {
                return this.OwningSliderBase.LargeChange;
            }
        }
        #endregion //LargeChange

        #region Maximum
        /// <summary>
        /// Gets the maximum range value that is supported by the control.
        /// </summary>
        /// <value></value>
        /// <returns>The maximum value that is supported by the control, or null if the control does not support <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.Maximum"/>. </returns>
        public double Maximum
        {
            get
            {
                return this.OwningSliderBase.ToDouble(this.OwningSliderBase.MaxValue);
            }
        }
        #endregion //Maximum

        #region Minimum
        /// <summary>
        /// Gets the minimum range value that is supported by the control.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum value that is supported by the control, or null if the control does not support <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.Minimum"/>. </returns>
        public double Minimum
        {
            get
            {
                return this.OwningSliderBase.ToDouble(this.OwningSliderBase.MinValue);
            }
        }
        #endregion //Minimum

        #region SmallChange
        /// <summary>
        /// Gets the value that is added to or subtracted from the <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.Value"/> property when a small change is made, such as with an arrow key.
        /// </summary>
        /// <value></value>
        /// <returns>The small-change value supported by the control, or null if the control does not support <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.SmallChange"/>. </returns>
        public double SmallChange
        {
            get
            {
                return this.OwningSliderBase.SmallChange;
            }
        }
        #endregion //SmallChange

        #region Value
        /// <summary>
        /// Gets the value of the control.
        /// </summary>
        /// <value></value>
        /// <returns>The value of the control, or null if the control does not support <see cref="P:System.Windows.Automation.Provider.IRangeValueProvider.Value"/>.</returns>
        public double Value
        {
            get
            {
                if (this.OwningSliderBase.ActiveThumb != null)
                {
                    return this.OwningSliderBase.ToDouble(this.OwningSliderBase.ActiveThumb.Value);
                }
                return 0;
            }
        }
        #endregion //Value 

        #endregion //Properties

        #region Methods

        #region SetValue
        /// <summary>
        /// Sets the value of the control.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetValue(double value)
        {
            if (this.OwningSliderBase.ActiveThumb != null)
            {
                this.OwningSliderBase.ActiveThumb.Value = this.OwningSliderBase.ToValue(value);
            }
        }
        #endregion //SetValue
 
        #endregion //Methods

        #endregion //IValueRangeProvider
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