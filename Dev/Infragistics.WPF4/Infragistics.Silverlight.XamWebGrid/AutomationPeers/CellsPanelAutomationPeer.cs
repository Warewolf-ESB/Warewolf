using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="CellsPanel"/> types to UI
    /// </summary>
    public class CellsPanelAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider, ISelectionItemProvider, IExpandCollapseProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CellsPanelAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public CellsPanelAutomationPeer(CellsPanel owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (this.Row != null)
            {
                this.Row.PropertyChanged += Row_PropertyChanged;
            }

            if (this.Grid != null)
            {
                this.Grid.SelectionSettings.PropertyChanged += SelectionSettings_PropertyChanged;
            }

            this.OldCanSelectMultiple = this.CanSelectMultiple;

            if (this.ExpandableRow != null)
            {
                this.ExpandableRow.PropertyChanged += ExpandableRow_PropertyChanged;
            }
        }

        #endregion Constructor

        #region Properties

        #region Grid
        private XamGrid Grid
        {
            get
            {
                if (this.OwningCellsPanel.Row != null)
                {
                    return this.OwningCellsPanel.Row.ColumnLayout.Grid;
                }

                return null;
            }
        }
        #endregion //Grid

        #region OldCanSelectMultiple
		private bool OldCanSelectMultiple { get; set;} 
	    #endregion //OldCanSelectMultiple

        #region OwningCellsPanel
        private CellsPanel OwningCellsPanel
        {
            get
            {
                return (CellsPanel)Owner;
            }
        }
        #endregion //OwningCellsPanel

        #region Row
        private Row Row
        {
            get
            {
                return this.OwningCellsPanel.Row as Row;
            }
        }
        #endregion //Row

        #region ExpandableRow
        private ExpandableRowBase ExpandableRow
        {
            get
            {
                return this.OwningCellsPanel.Row as ExpandableRowBase;
            }
        }
        #endregion //ExpandableRow

        #endregion Properties

        #region Methods

        #region Internal

        #region RaiseCanSelectMultiplePropertyChangedEvent
        /// <summary>
        /// Raises the CanSelectMultiplePropertyChangedEvent.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        internal void RaiseCanSelectMultiplePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(SelectionPatternIdentifiers.CanSelectMultipleProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanSelectMultiplePropertyChangedEvent

        #region RaiseExpandCollapseStatePropertyChangedEvent
        /// <summary>
        /// Raises the ExpandCollapseStatePropertyChangedEvent.
        /// </summary>
        /// <param name="oldValue">if set to <c>true</c> The old value.</param>
        /// <param name="newValue">if set to <c>true</c> The new value.</param>
        internal void RaiseExpandCollapseStatePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged) && this.ExpandableRow != null)
            {
                ExpandCollapseState oldState = ExpandCollapseState.Collapsed;
                if (oldValue)
                {
                    oldState = ExpandCollapseState.Expanded;
                }

                ExpandCollapseState newState = ExpandCollapseState.Collapsed;
                if (newValue)
                {
                    newState = ExpandCollapseState.Expanded;
                }

                this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldState, newState);

            }
        }
        #endregion //RaiseExpandCollapseStatePropertyChangedEvent

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

        #region Overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns the control type for the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>A value of the enumeration.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Pane;
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
                Collection<CellBase> items = this.OwningCellsPanel.VisibleCells;
                if (items.Count <= 0)
                {
                    return null;
                }

                List<AutomationPeer> list = new List<AutomationPeer>(items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    UIElement element = items[i].Control;
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
                if (this.OwningCellsPanel.Row != null)
                {
                    return this.OwningCellsPanel.Row.Index.ToString(CultureInfo.CurrentCulture);
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

            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            if (patternInterface == PatternInterface.ExpandCollapse &&
                this.ExpandableRow != null && this.ExpandableRow.HasChildren)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion Overrides

        #region Event Handlers

        #region ExpandableRow_PropertyChanged
        /// <summary>
        /// Handles the PropertyChanged event of the ExpandableRow property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ExpandableRow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            switch (propertyName)
            {
                case "IsExpanded":
                    this.RaiseExpandCollapseStatePropertyChangedEvent(!this.ExpandableRow.IsExpanded, this.ExpandableRow.IsExpanded);
                    break;
            }
        }
        #endregion //ExpandableRow_PropertyChanged


        #region Row_PropertyChanged


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private void Row_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.Row == null)
                return;

            string propertyName = e.PropertyName;
            switch(propertyName)
            {
                case "IsSelected":
                    this.RaiseIsSelectedPropertyChanged(this.Row.IsSelected);
                    break;
            }
        }
        #endregion //Row_PropertyChanged

        #region SelectionSettings_PropertyChanged


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private void SelectionSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            string propertyChanged = e.PropertyName;

            switch (propertyChanged)
            {
                case "CellSelection":
                    this.RaiseCanSelectMultiplePropertyChangedEvent(this.OldCanSelectMultiple, this.CanSelectMultiple);
                    this.OldCanSelectMultiple = this.CanSelectMultiple;
                    break;
            }

        }
        #endregion //SelectionSettings_PropertyChanged

        #endregion //Event Handlers

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
                if (this.OwningCellsPanel.Owner.Grid.SelectionSettings.CellSelection == SelectionType.Multiple)
                {
                    return true;
                }

                return false;
            }
        }
        #endregion CanSelectMultiple

        #region GetSelection
        /// <summary>
        /// Retrieves a UI automation provider for each child element that is selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        public IRawElementProviderSimple[] GetSelection()
        {
            List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
            if (this.OwningCellsPanel.VisibleCells.Count > 0)
            {
                for (int i = 0; i < this.OwningCellsPanel.VisibleCells.Count; i++)
                {
                    CellControlBase element = this.OwningCellsPanel.VisibleCells[i].Control;
                    if (element.Cell.IsSelected)
                    {
                        AutomationPeer peer = FromElement(element) ?? CreatePeerForElement(element);
                        list.Add(ProviderFromPeer(peer));
                    }
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

        #endregion ISelectionProvider

        #region ISelectionItemProvider

        #region AddToSelection
        /// <summary>
        /// Selects the item this automation peer controls.
        /// </summary>
        public void AddToSelection()
        {
            if (this.OwningCellsPanel == null)
            {
                throw new InvalidOperationException();
            }

            Row row = this.OwningCellsPanel.Row as Row;
            if (row == null)
            {
                throw new InvalidOperationException();
            }
            
            if (this.Grid.SelectionSettings.RowSelection == SelectionType.Single)
            {
                this.Grid.SelectionSettings.SelectedRows.Clear();
            }
            if (this.Grid.SelectionSettings.RowSelection != SelectionType.None)
            {
                this.Grid.SelectionSettings.SelectedRows.Add(row);
            }
        }
        #endregion //AddToSelection

        #region IsSelected
        /// <summary>
        /// Gets a value indicating whether the item this automation peer controls is selected.
        /// </summary>
        /// <value></value>
        /// <returns>true if the element is selected; otherwise, false.</returns>
        public bool IsSelected
        {
            get
            {
                Row row = this.OwningCellsPanel.Row as Row;
                if (row != null)
                {
                    return row.IsSelected;
                }

                return false;
            }
        }
        #endregion //IsSelected

        #region RemoveFromSelection
        /// <summary>
        /// Deselects the item this automation peer controls.
        /// </summary>
        public void RemoveFromSelection()
        {
            if (this.OwningCellsPanel == null)
            {
                throw new InvalidOperationException();
            }

            Row row = this.OwningCellsPanel.Row as Row;
            if (row == null)
            {
                throw new InvalidOperationException();
            }

            row.IsSelected = false;
        }
        #endregion //RemoveFromSelection

        #region Select
        /// <summary>
        /// Clears the current selection then selects the item this automation peer controls.
        /// </summary>
        public void Select()
        {
            Row row = this.OwningCellsPanel.Row as Row;
            if (row == null)
            {
                throw new InvalidOperationException();
            }

            row.IsSelected = true;
        }
        #endregion //Select

        #region SelectionContainer
        /// <summary>
        /// Gets the provider for this control.
        /// </summary>
        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                if (this.Grid != null)
                {
                    AutomationPeer peer = FromElement(this.Grid) ?? CreatePeerForElement(this.Grid);
                    return ProviderFromPeer(peer);                    
                }

                return this.ProviderFromPeer(this);
            }
        }

        #endregion //SelectionContainer 

        #endregion //ISelectionItemProvider

        #region IExpandCollapseProvider

        #region Collapse
        /// <summary>
        /// Hides all nodes, controls, or content that are descendants of the control.
        /// </summary>
        public void Collapse()
        {
            this.ExpandableRow.IsExpanded = false;
        }
        #endregion //Collapse

        #region Expand
        /// <summary>
        /// Displays all child nodes, controls, or content of the control.
        /// </summary>
        public void Expand()
        {
            this.ExpandableRow.IsExpanded = true;
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
                if (this.ExpandableRow != null)
                {
                    if (this.ExpandableRow.IsExpanded)
                    {
                        return ExpandCollapseState.Expanded;
                    }
                }

                return ExpandCollapseState.Collapsed;
            }
        }
        #endregion //ExpandCollapseState

        #endregion //IExpandCollapseProvider
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