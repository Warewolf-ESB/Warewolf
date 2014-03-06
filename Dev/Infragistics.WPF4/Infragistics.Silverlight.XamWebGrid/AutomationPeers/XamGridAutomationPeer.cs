using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Grids;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="Infragistics.Controls.Grids.XamGrid"/> types to UI
    /// </summary>
    public class XamGridAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider, ISelectionProvider, ITableProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="XamGridAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamGridAutomationPeer(XamGrid owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this.OwningGrid.SelectionSettings.PropertyChanged += SelectionSettings_PropertyChanged;

            this.OldCanSelectMultiple = this.CanSelectMultiple;
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
            return AutomationControlType.DataGrid;
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
            
            if (this.OwningGrid.Panel == null)
                return null;

            Collection<RowBase> items = this.OwningGrid.Panel.VisibleRows;
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
            return this.OwningGrid.GetType().Name;
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
                return this.OwningGrid.Name;
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
            if (patternInterface == PatternInterface.Scroll)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Grid)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Table)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion Overrides

        #region Properties

        #region HorizontalMaximum
        private double HorizontalMaximum
        {
            get
            {
                return this.HorizontalScrollBar.Maximum;
            }
        }
        #endregion //HorizontalMaximum

        #region HorizontalMinimum
        private double HorizontalMinimum
        {
            get
            {
                return this.HorizontalScrollBar.Minimum;
            }
        }
        #endregion //HorizontalMinimum

        #region HorizontalScrollBar
        private ScrollBar HorizontalScrollBar
        {
            get
            {
                IProvideScrollInfo scrollInfo = this.OwningGrid;
                if (scrollInfo != null)
                {
                    return scrollInfo.HorizontalScrollBar;
                }

                return null;
            }
        }
        #endregion //HorizontalScrollBar

        #region HorizontalValue
        private double HorizontalValue
        {
            get
            {
                return this.HorizontalScrollBar.Value;
            }
        }
        #endregion //HorizontalValue

        #region OldCanSelectMUltiple

        private bool OldCanSelectMultiple { get; set;}

        #endregion OldCanSelectMUltiple

        #region OwningGrid
        private XamGrid OwningGrid
        {
            get
            {
                return (XamGrid)Owner;
            }
        }
        #endregion //OwningGrid

        #region VerticalMaximum
        private double VerticalMaximum
        {
            get
            {
                return this.VerticalScrollBar.Maximum;
            }
        }
        #endregion //VerticalMaximum

        #region VerticalMinimum
        private double VerticalMinimum
        {
            get
            {
                return this.VerticalScrollBar.Minimum;
            }
        }
        #endregion //VerticalMinimum

        #region VerticalScrollBar
        private ScrollBar VerticalScrollBar
        {
            get
            {
                IProvideScrollInfo scrollInfo = this.OwningGrid;
                if (scrollInfo != null)
                {
                    return scrollInfo.VerticalScrollBar;
                }

                return null;
            }
        }
        #endregion //VerticalScrollBar

        #region VerticalValue
        private double VerticalValue
        {
            get
            {
                return this.VerticalScrollBar.Value;
            }
        }
        #endregion //VerticalValue

        #endregion Properties

        #region Methods

        #region Internal

        #region RaiseCanSelectMultiplePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void RaiseCanSelectMultiplePropertyChangedEvent(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && ListenerExists(AutomationEvents.PropertyChanged))
            {
                this.RaisePropertyChangedEvent(SelectionPatternIdentifiers.CanSelectMultipleProperty, oldValue, newValue);
            }
        }
        #endregion //RaiseCanSelectMultiplePropertyChangedEvent

        #endregion //Internal

        #endregion //Methods

        #region Event Handlers

        #region SelectionSettings_PropertyChanged


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private void SelectionSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            switch (propertyName)
            {
                case "RowSelection":
                    this.RaiseCanSelectMultiplePropertyChangedEvent(this.OldCanSelectMultiple, this.CanSelectMultiple);
                    this.OldCanSelectMultiple = this.CanSelectMultiple;
                    break;
            }
        }
        #endregion //SelectionSettings_PropertyChanged

        #endregion Event Handlers

        #region IScrollProvider

        /// <summary>
        /// Scrolls the grid by the specified ammounts.
        /// </summary>
        /// <param name="horizontalAmount"></param>
        /// <param name="verticalAmount"></param>
        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            if (!this.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            if (((horizontalAmount != ScrollAmount.NoAmount) && !this.HorizontallyScrollable) || ((verticalAmount != ScrollAmount.NoAmount) && !this.VerticallyScrollable))
            {
                throw new InvalidOperationException("Cannot scroll because the XamGrid is not scrollable.");
            }

            switch (horizontalAmount)
            {
                case ScrollAmount.SmallIncrement:
                    this.HorizontalScrollBar.Value += this.HorizontalScrollBar.SmallChange;
                    break;
                case ScrollAmount.SmallDecrement:
                    this.HorizontalScrollBar.Value -= this.HorizontalScrollBar.SmallChange;
                    break;
                case ScrollAmount.LargeIncrement:
                    this.HorizontalScrollBar.Value += this.HorizontalScrollBar.LargeChange;
                    break;
                case ScrollAmount.LargeDecrement:
                    this.HorizontalScrollBar.Value -= this.HorizontalScrollBar.LargeChange;
                    break;
            }

            switch (verticalAmount)
            {
                case ScrollAmount.SmallIncrement:
                    this.VerticalScrollBar.Value += this.VerticalScrollBar.SmallChange;
                    break;
                case ScrollAmount.SmallDecrement:
                    this.VerticalScrollBar.Value -= this.VerticalScrollBar.SmallChange;
                    break;
                case ScrollAmount.LargeIncrement:
                    this.VerticalScrollBar.Value += this.VerticalScrollBar.LargeChange;
                    break;
                case ScrollAmount.LargeDecrement:
                    this.VerticalScrollBar.Value -= this.VerticalScrollBar.LargeChange;
                    break;
            }

            this.OwningGrid.RowsManager.InvalidateRows();
        }

        #region HorizontallyScrollable
        /// <summary>
        /// Gets a value indicating whether the control can scroll horizontally.
        /// </summary>
        /// <value></value>
        /// <returns>true if the control can scroll horizontally; otherwise, false.</returns>
        public bool HorizontallyScrollable
        {
            get
            {
                return this.HorizontalScrollBar != null;
            }
        }
        #endregion //HorizontallyScrollable

        #region HorizontallyScrollable
        /// <summary>
        /// Gets a value indicating whether the control can scroll vertically.
        /// </summary>
        /// <value></value>
        /// <returns>true if the control can scroll vertically; otherwise, false. </returns>
        public bool VerticallyScrollable
        {
            get
            {
                return this.VerticalScrollBar != null;
            }
        }
        #endregion //HorizontallyScrollable

        #region SetScrollPercent
        /// <summary>
        /// Sets the horizontal and vertical scroll position as a percentage of the total content area within the control.
        /// </summary>
        /// <param name="horizontalPercent">The horizontal position as a percentage of the content area's total range. Pass <see cref="F:System.Windows.Automation.ScrollPatternIdentifiers.NoScroll"/> if the control cannot be scrolled in this direction.</param>
        /// <param name="verticalPercent">The vertical position as a percentage of the content area's total range. Pass <see cref="F:System.Windows.Automation.ScrollPatternIdentifiers.NoScroll"/> if the control cannot be scrolled in this direction.</param>
        public void SetScrollPercent(double horizontalPercent, double verticalPercent)
        {
            
            if (((horizontalPercent != -1.0) && !this.HorizontallyScrollable) || ((verticalPercent != -1.0) && !this.VerticallyScrollable))
            {
                throw new InvalidOperationException("Cannot scroll because the GridViewScrollViewer is not scrollable.");
            }

            if ((horizontalPercent != -1.0) && ((horizontalPercent < 0.0) || (horizontalPercent > 100.0)))
            {
                throw new ArgumentOutOfRangeException("horizontalPercent", "The horizontal perecentage must be between 0.0 and 100.0.");
            }

            if ((verticalPercent != -1.0) && ((verticalPercent < 0.0) || (verticalPercent > 100.0)))
            {
                throw new ArgumentOutOfRangeException("verticalPercent", "The vertical perecentage must be between 0.0 and 100.0.");
            }

            if ((horizontalPercent != -1.0) && (this.HorizontalScrollBar != null))
            {
                this.HorizontalScrollBar.Value = 0;
                this.OwningGrid.RowsManager.InvalidateRows();

                double smallHorizontalChange = this.HorizontalScrollBar.SmallChange;
                this.HorizontalScrollBar.SmallChange = (this.HorizontalMaximum -
                                                                   this.HorizontalMinimum) *
                                                                  horizontalPercent * 0.01;
                this.Scroll(ScrollAmount.SmallIncrement, ScrollAmount.NoAmount);
                this.HorizontalScrollBar.SmallChange = smallHorizontalChange;
            }

            if ((verticalPercent != -1.0) && (this.VerticalScrollBar != null))
            {
                this.VerticalScrollBar.Value = 0;
                this.OwningGrid.RowsManager.InvalidateRows();

                double smallVerticalChange = this.VerticalScrollBar.SmallChange;
                this.VerticalScrollBar.SmallChange = (this.VerticalMaximum -
                                                                   this.VerticalMinimum) *
                                                                  verticalPercent * 0.01;
                this.Scroll(ScrollAmount.NoAmount, ScrollAmount.SmallIncrement);
                this.VerticalScrollBar.SmallChange = smallVerticalChange;
            }

        }
        #endregion //SetScrollPercent

        /// <summary>
        /// Gets the current horizontal scroll position.
        /// </summary>
        /// <value></value>
        /// <returns>The horizontal scroll position as a percentage of the total content area within the control.</returns>
        public double HorizontalScrollPercent
        {
            get
            {
                if (this.HorizontallyScrollable)
                {
                    return (this.HorizontalValue - this.HorizontalMinimum) / (this.HorizontalMaximum - this.HorizontalMinimum) * 100.0;
                }

                return -1.0;
            }
        }

        /// <summary>
        /// Gets the current horizontal view size.
        /// </summary>
        /// <value></value>
        /// <returns>The horizontal size of the viewable region as a percentage of the total content area within the control. </returns>
        public double HorizontalViewSize
        {
            get
            {
                if (this.HorizontalScrollBar != null)
                {
                    return this.HorizontalMaximum - this.HorizontalMinimum;
                }

                return 0.0;
            }
        }

        /// <summary>
        /// Gets the current vertical scroll position.
        /// </summary>
        /// <value></value>
        /// <returns>The vertical scroll position as a percentage of the total content area within the control. </returns>
        public double VerticalScrollPercent
        {
            get
            {
                if (this.VerticallyScrollable)
                {
                    return (this.VerticalValue - this.VerticalMinimum) / (this.VerticalMaximum - this.VerticalMinimum) * 100.0;                
                }

                return -1.0;
            }
        }

        /// <summary>
        /// Gets the vertical view size.
        /// </summary>
        /// <value></value>
        /// <returns>The vertical size of the viewable region as a percentage of the total content area within the control. </returns>
        public double VerticalViewSize
        {
            get
            {
                if (this.VerticalScrollBar != null)
                {
                    return this.VerticalMaximum - this.VerticalMinimum;
                }

                return 0.0;
            }
        }

        #endregion IScrollProvider

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
                if (this.OwningGrid.SelectionSettings.RowSelection == SelectionType.Multiple ||
                    this.OwningGrid.SelectionSettings.CellSelection == SelectionType.Multiple)
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
            if (this.OwningGrid.SelectionSettings.SelectedRows.Count > 0)
            {
                for (int i = 0; i < this.OwningGrid.SelectionSettings.SelectedRows.Count; i++)
                {
                    UIElement element = this.OwningGrid.SelectionSettings.SelectedRows[i].Control;
                    AutomationPeer peer = FromElement(element) ?? CreatePeerForElement(element);
                    list.Add(ProviderFromPeer(peer));
                }                
            }
            else
            {
                for (int i = 0; i < this.OwningGrid.SelectionSettings.SelectedCells.Count; i++)
                {
                    UIElement element = this.OwningGrid.SelectionSettings.SelectedCells[i].Control;
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

        #endregion ISelectionProvider

        #region IGridProvider

        #region GetItem
        /// <summary>
        /// Retrieves the UI automation provider for the specified cell.
        /// </summary>
        /// <param name="row">The ordinal number of the row that contains the cell.</param>
        /// <param name="column">The ordinal number of the column that contains the cell.</param>
        /// <returns>
        /// The UI automation provider for the specified cell.
        /// </returns>
        public IRawElementProviderSimple GetItem(int row, int column)
        {
            IGridProvider provider = this;
            int rowCount = provider.RowCount;
            int columnCount = provider.ColumnCount;
            if ((row < 0) || (row >= rowCount))
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if ((column < 0) || (column >= columnCount))
            {
                throw new ArgumentOutOfRangeException("column");
            }

            UIElement element = this.OwningGrid.Rows[row].Cells[column].Control;
            AutomationPeer cellPeer = FromElement(element) ?? CreatePeerForElement(element);
            return ProviderFromPeer(cellPeer);
        }
        #endregion GetItem

        #region ColumnCount
        /// <summary>
        /// Gets the total number of columns in a grid.
        /// </summary>
        /// <value></value>
        /// <returns>The total number of columns in a grid.</returns>
        public int ColumnCount
        {
            get
            {
                return this.OwningGrid.Columns.Count;
            }
        }
        #endregion //ColumnCount

        #region RowCount
        /// <summary>
        /// Gets the total number of rows in a grid.
        /// </summary>
        /// <value></value>
        /// <returns>The total number of rows in a grid.</returns>
        public int RowCount
        {
            get
            {
                return this.OwningGrid.Rows.Count;
            }
        }
        #endregion //ColumnCount

        #endregion RowCount

        #region ITableProvider

        #region GetColumnHeaders
        /// <summary>
        /// Returns a collection of UI Automation providers that represents all the column headers in a table.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        public IRawElementProviderSimple[] GetColumnHeaders()
        {
            if (this.OwningGrid.HeaderVisibility == Visibility.Visible)
            {
                List<IRawElementProviderSimple> headerCellPeers = new List<IRawElementProviderSimple>();
                for (int i = 0; i < this.OwningGrid.RowsManager.HeaderRow.VisibleCells.Count; i++)
                {
                    UIElement element = this.OwningGrid.RowsManager.HeaderRow.VisibleCells[i].Control;
                    if (element != null)
                    {
                        AutomationPeer cellPeer = FromElement(element) ?? CreatePeerForElement(element);
                        headerCellPeers.Add(ProviderFromPeer(cellPeer));
                    }
                }

                return headerCellPeers.ToArray();
            }

            return null;
        }
        #endregion GetColumnHeaders

        #region GetRowHeaders
        /// <summary>
        /// Returns a collection of UI Automation providers that represents all row headers in the table.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        public IRawElementProviderSimple[] GetRowHeaders()
        {
            //return an empty list
            List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
            return list.ToArray();
        }
        #endregion GetRowHeaders

        #region RowOrColumnMajor
        /// <summary>
        /// Gets the primary direction of traversal for the table.
        /// </summary>
        /// <value></value>
        /// <returns>The primary direction of traversal, as a value of the enumeration. </returns>
        public RowOrColumnMajor RowOrColumnMajor
        {
            get
            {
                return RowOrColumnMajor.RowMajor;
            }
        }
        #endregion //RowOrColumnMajor

        #endregion ITableProvider
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