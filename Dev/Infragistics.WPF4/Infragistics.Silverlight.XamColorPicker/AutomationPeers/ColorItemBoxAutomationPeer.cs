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
using Infragistics.Controls.Editors.Primitives;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="ColorItemBox" /> types to UI
    /// </summary>
    public class ColorItemBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ISelectionItemProvider
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamColorPickerAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="Infragistics.Controls.Editors.XamColorPicker"/> that this automation peer controls.</param>
        public ColorItemBoxAutomationPeer(ColorItemBox owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningColorItemBox.ColorItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ColorItem_PropertyChanged);
        }




        #endregion // Constructors

        #region Properties

        #region ColorItemBox

        private ColorItemBox OwningColorItemBox
        {
            get
            {
                return (ColorItemBox)this.Owner;
            }
        }

        #endregion // ColorItemBox

        #region IsReadOnly
        private bool IsReadOnly
        {
            get { return true; }
        }
        #endregion // IsReadOnly

        #region Value

        private string Value
        {
            get
            {
                if (this.OwningColorItemBox.ColorItem == null)
                {
                    return null;
                }
                return this.OwningColorItemBox.ColorItem.Color.ToString();
            }
        }

        #endregion // Value

        #region IsSelected
        private bool IsSelected
        {
            get
            {
                return this.OwningColorItemBox.ColorItem.IsSelected;
            }
            set
            {
                this.OwningColorItemBox.ColorItem.IsSelected = value;
            }
        }
        #endregion // IsSelected

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
                nameCore = this.OwningColorItemBox.Name;
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

        #region RaiseIsSelectedPropertyChanged






        internal void RaiseIsSelectedPropertyChanged(bool isSelected)
        {
            if (ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected, isSelected);
            }
        }
        #endregion //RaiseIsSelectedPropertyChanged

        #endregion // Methods

        #region IValueProvider Members

        bool IValueProvider.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        void IValueProvider.SetValue(string value)
        {
            return;
        }

        string IValueProvider.Value
        {
            get { return this.Value; }
        }

        #endregion

        #region ISelectionItemProvider Members

        void ISelectionItemProvider.AddToSelection()
        {
            this.IsSelected = true;
        }

        bool ISelectionItemProvider.IsSelected
        {
            get { return this.IsSelected; }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            this.IsSelected = false;
        }

        void ISelectionItemProvider.Select()
        {
            this.IsSelected = true;
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get { return null; }
        }

        #endregion

        #region Event Handlers
        void ColorItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                this.RaiseIsSelectedPropertyChanged(this.IsSelected);
            }
        }
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