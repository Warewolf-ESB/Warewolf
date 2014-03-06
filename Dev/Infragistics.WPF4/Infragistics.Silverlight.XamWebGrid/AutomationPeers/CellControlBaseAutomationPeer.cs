using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="CellControlBase" /> types to UI
    /// </summary>
    public class CellControlBaseAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ISelectionItemProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CellControlBaseAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public CellControlBaseAutomationPeer(CellControlBase owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (this.OwningCell != null)
            {
                this.OwningCell.PropertyChanged += OwningCell_PropertyChanged;
            }
            
        }

        #endregion Constructor

        #region Overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns the control type for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>A value of the enumeration.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
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
                if (this.OwningCellControl.Cell != null)
                {
                    nameCore = this.OwningCellControl.Cell.Column.HeaderText;
                }
            }

            if (string.IsNullOrEmpty(nameCore))
            {
                if (this.OwningCellControl.Cell != null)
                {
                    nameCore = this.OwningCellControl.Cell.Column.Key;
                }
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
            if (patternInterface == PatternInterface.SelectionItem)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion Overrides

        #region Properties

        #region Grid
        private XamGrid Grid
        {
            get
            {
                if (this.Panel != null)
                {
                    return this.Panel.Row.ColumnLayout.Grid;
                }

                return null;
            }
        }
        #endregion //Grid

        #region OwningCell
        private Cell OwningCell
        {
            get
            {
                return ((CellControlBase)Owner).Cell as Cell;
            }
        }
        #endregion //OwningCelll

        #region OwningCellControl
        private CellControlBase OwningCellControl
        {
            get
            {
                return (CellControlBase)Owner;
            }
        }
        #endregion //OwningCellControl

        #region Panel
        private CellsPanel Panel
        {
            get
            {
                return this.OwningCellControl.Parent as CellsPanel;
            }
        }
        #endregion //Panel

        #endregion //Properties

        #region Methods

        #region Internal

        #region RaiseIsSelectedPropertyChanged






        internal void RaiseIsSelectedPropertyChanged(bool isSelected)
        {
            if (ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, !isSelected,
                                               isSelected);
            }
        }
        #endregion //RaiseIsSelectedPropertyChanged

        #endregion //Internal

        #endregion //Methods


        #region Event Handlers
        /// <summary>
        /// Handles the PropertyChanged event of the OwningCell property like Cell.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void OwningCell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.RaiseIsSelectedPropertyChanged(this.IsSelected);
        }
        #endregion //Event Handlers

        #region IValueProvider
        /// <summary>
        /// Sets the value of a control.
        /// </summary>
        /// <param name="value">The value to set. The provider is responsible for converting the value to the appropriate data type.</param>
        public void SetValue(string value)
        {
            return;
        }

        /// <summary>
        /// Gets a value indicating whether the value of a control is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the value is read-only; false if it can be modified. </returns>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the value of the control.
        /// </summary>
        /// <value></value>
        /// <returns>The value of the control.</returns>
        public string Value
        {
            get
            {
                if (this.OwningCellControl.Cell != null &&
                    this.OwningCellControl.Cell.Value != null)
                {
                    return this.OwningCellControl.Cell.Value.ToString();
                }

                return string.Empty;
            }
        }
        #endregion IValueProvider

        #region ISelectionItemProvider
        /// <summary>
        /// Adds the current element to the collection of selected items.
        /// </summary>
        public void AddToSelection()
        {
            this.Grid.SelectionSettings.SelectedCells.Add((Cell)this.OwningCellControl.Cell);
        }

        /// <summary>
        /// Removes the current element from the collection of selected items.
        /// </summary>
        public void RemoveFromSelection()
        {
            this.Grid.SelectionSettings.SelectedCells.Remove((Cell)this.OwningCellControl.Cell);
        }

        /// <summary>
        /// Clears any existing selection and then selects the current element.
        /// </summary>
        public void Select()
        {
            this.OwningCellControl.Cell.IsSelected = true;
        }

        /// <summary>
        /// Gets a value indicating whether an item is selected.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element is selected; otherwise, false.</returns>
        public bool IsSelected
        {
            get
            {
                return this.OwningCellControl.Cell.IsSelected;
            }
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
                if (this.Panel != null)
                {
                    AutomationPeer peer = FromElement(this.Panel) ?? CreatePeerForElement(this.Panel);
                    return ProviderFromPeer(peer);
                }

                return null;
            }
        }
        #endregion //ISelectionItemProvider
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