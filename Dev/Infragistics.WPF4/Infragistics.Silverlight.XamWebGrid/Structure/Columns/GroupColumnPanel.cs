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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// A Panel used to organize cells for a particular <see cref="GroupColumn"/>
    /// </summary>
    public class GroupColumnPanel : Panel
    {
        #region Members

        bool _inMeasure;
        List<CellBase> _usedCells = new List<CellBase>();
        ReadOnlyKeyedColumnBaseCollection<Column> _previousColumns;

        #endregion // Members

        #region Properties

        #region Public

        #region Cell

        /// <summary>
        /// Identifies the <see cref="Cell"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(CellBase), typeof(GroupColumnPanel), new PropertyMetadata(new PropertyChangedCallback(CellChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="CellBase"/> that represents either a <see cref="GroupCell"/> or a <see cref="GroupHeaderCell"/>
        /// </summary>
        public CellBase Cell
        {
            get { return (CellBase)this.GetValue(CellProperty); }
            set { this.SetValue(CellProperty, value); }
        }

        private static void CellChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GroupColumnPanel panel = (GroupColumnPanel)obj;

            if (panel._usedCells.Count > 0)
            {
                foreach (CellBase cell in panel._usedCells)
                    RecyclingManager.Manager.ReleaseElement(cell, panel);

                panel._usedCells.Clear();
            }

            if (e.NewValue != null)
            {
                panel.InvalidateMeasure();

                CellBase cb = (CellBase)e.NewValue;
                panel._previousColumns = panel.Column.AllColumns;
                foreach (Column col in panel._previousColumns)
                {
                    col.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(panel.Col_PropertyChanged);
                }

                panel.Column.Columns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(panel.Columns_CollectionChanged);
            }
            
            if(e.OldValue != null)
            {
                CellBase cb = (CellBase)e.OldValue;
                
                foreach (Column col in panel._previousColumns)
                {
                    col.PropertyChanged -= panel.Col_PropertyChanged;
                }

                GroupColumn gc = (GroupColumn)cb.Column;
                gc.Columns.CollectionChanged -= panel.Columns_CollectionChanged;
            }
        }        

        #endregion // Cell 
				
        #region Row

        /// <summary>
        /// Gets the <see cref="RowBase"/> associated with this <see cref="GroupColumnPanel"/>
        /// </summary>
        public RowBase Row
        {
            get
            {
                if (this.Cell != null)
                    return this.Cell.Row;

                return null;
            }
        }
        #endregion // Row

        #region Column

        /// <summary>
        /// Gets the <see cref="GroupColumn"/> associated with this <see cref="Column"/>
        /// </summary>
        public GroupColumn Column
        {
            get
            {
                if (this.Cell != null)
                    return this.Cell.Column as GroupColumn;

                return null;
            }
        }
        #endregion // Column

        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Protected

        #region SetupStarCells

        /// <summary>
        /// Walks through the star cells of a <see cref="Column"/> and sets their width to take up the remaining available space.
        /// </summary>
        /// <param name="availableWidth"></param>
        /// <param name="currentWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="starColumns"></param>
        /// <returns></returns>
        protected double SetupStarCells(double availableWidth, double currentWidth, ref double maxHeight, Collection<CellBase> starColumns)
        {
            // Now that we've rendered all the cells, lets see if any of the cells are of type 
            // Star, and allow them to fill the remaining space.
            double remainingWidth = availableWidth - currentWidth;
            double usedWidth = remainingWidth;
            double remainder = 0;
            if (remainingWidth > 0)
            {
                if (starColumns.Count > 0)
                {
                    double divider = 0;

                    // Calculate the divider
                    foreach (CellBase cell in starColumns)
                    {
                        divider += cell.Column.WidthResolved.Value;
                    }

                    if (divider != 0)
                    {
                        double individualWidth = remainingWidth / divider;

                        bool reEvaluate = false;

                        List<CellBase> actualStarCols = new List<CellBase>(starColumns);


                        do
                        {
                            reEvaluate = false;
                            List<CellBase> newList = new List<CellBase>();

                            // Walk through all the start cells and evaluate whether they will participate in star sizing. 
                            // If the their min width is larger than the available width alloted to it, then it will not particapte
                            // and the remaining width and divider need to be updated appropriately. 
                            // We need to do this in side a while stmt, so that we account for the new remaining width if it changes
                            foreach (CellBase cell in actualStarCols)
                            {
                                double requestedValue = (int)individualWidth * cell.Column.WidthResolved.Value;
                                if (requestedValue < cell.Column.MinimumWidth)
                                {
                                    remainingWidth -= cell.Column.MinimumWidth;
                                    divider -= cell.Column.WidthResolved.Value;
                                    cell.Column.ActualWidth = cell.Column.MinimumWidth;

                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.MinimumWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.MinimumWidth;

                                    reEvaluate = true;
                                }
                                else
                                    newList.Add(cell);
                            }

                            actualStarCols = newList;
                            individualWidth = remainingWidth / divider;

                        } while (reEvaluate);

                        if (remainingWidth > 0)
                        {
                            if (actualStarCols.Count > 0)
                            {
                                foreach (CellBase cell in actualStarCols)
                                {
                                    // We don't want to add decimal values to the widths as it can mess with the way the cells look
                                    // So, let's store the remainder.
                                    cell.Column.ActualWidth = (int)individualWidth * cell.Column.WidthResolved.Value;

                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

                                    remainder += individualWidth - (int)individualWidth;
                                }

                                // Now that we have remaining pixels to apply, for every whole pixel, lets start from the begining and append
                                // them to the width of the cell. 
                                int counter = 0;
                                while (remainder > 1)
                                {
                                    CellBase cell = starColumns[counter];
                                    cell.Column.ActualWidth += 1;

                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

                                    counter++;
                                    remainder--;
                                }

                                // We'll add the last few decimal points to the last star column. 
                                actualStarCols[actualStarCols.Count - 1].Column.ActualWidth += remainder;
                            }
                        }
                        else
                        {
                            // After min widths were calculated, we ran out of width, so no start widths
                            usedWidth = 0;
                            foreach (CellBase cell in actualStarCols)
                            {
                                // These min width here is probably 0
                                cell.Column.ActualWidth = cell.Column.MinimumWidth;

                                if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                    this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                else
                                    this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;
                            }
                        }

                        // Now loop through and make sure these columns are measured correctly, so that all their content is visible.
                        foreach (CellBase cell in starColumns)
                        {
                            double width = 0;
                            if (this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                width = this.Row.Manager.StarColumnWidths[cell.Column];

                            cell.Control.Measure(new Size(width, double.PositiveInfinity));
                            usedWidth -= width;
                            maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
                        }
                    }
                }
            }
            else
            {
                // No remaining width?
                // Then we need to set each column's width to the minimum width (Not 0)
                usedWidth = 0;
                foreach (CellBase cell in starColumns)
                {
                    cell.Column.ActualWidth = cell.Column.MinimumWidth;

                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                    else
                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

                    usedWidth -= cell.Column.ActualWidth;
                }
            }

            if (double.IsInfinity(usedWidth))
                usedWidth = 0;

            return currentWidth + Math.Abs(usedWidth);
        }

        #endregion // SetupStarCells

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region MeasureOverride

        /// <summary>
        /// Measures <see cref="Cell"/> objects stacked horizontally for a <see cref="GroupColumn"/>
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this._inMeasure = true;

            if (this.Row == null)
            {
                return base.MeasureOverride(availableSize);
            }

            double totalWidth = 0, totalHeight = 0;
            
            if (this.Column != null && this.Cell.Control != null)
            {
                List<CellBase> usedCellsCopy = new List<CellBase>(this._usedCells);

                foreach (var cellBase in usedCellsCopy)
                {
                    if (cellBase.Row != this.Row)
                    {
                        RecyclingManager.Manager.ReleaseElement(cellBase, this);
                        this._usedCells.Remove(cellBase);
                    }
                }

                Collection<CellBase> starColumns = new Collection<CellBase>();

                foreach (Column column in this.Column.Columns)
                {
                    CellBase cell = this.Row.ResolveCell(column);
                    if (column.Visibility != Visibility.Collapsed)
                    {
                        if (cell != null)
                        {
                            

                            ColumnWidth colWidth = column.WidthResolved;
                            ColumnWidthType widthType = colWidth.WidthType;

                            double widthToMeasure = double.PositiveInfinity;

                            if (widthType == ColumnWidthType.Numeric)
                                widthToMeasure = colWidth.Value;
                            else if (double.IsPositiveInfinity(availableSize.Width) && widthType == ColumnWidthType.Star)
                                widthType = ColumnWidthType.Auto;
                            else if (widthType == ColumnWidthType.InitialAuto && column.IsInitialAutoSet)
                                widthToMeasure = column.ActualWidth;

                            

                            double heightToMeasure = double.PositiveInfinity;
                            RowHeight rowHeight = this.Row.HeightResolved;
                            if (rowHeight.HeightType == RowHeightType.Numeric)
                                heightToMeasure = rowHeight.Value;

                            if (cell.Control != null && cell.Control.Parent != this)
                            {

                                
                                GroupColumnPanel parentPanel = cell.Control.Parent as GroupColumnPanel;

                                if (parentPanel != null)
                                {
                                    RecyclingManager.Manager.ReleaseElement(cell, parentPanel);
                                    parentPanel._usedCells.Remove(cell);
                                }
                                else

                                {
                                    CellsPanel cellsParentPanel = cell.Control.Parent as CellsPanel;
                                    if (cellsParentPanel != null)
                                    {
                                        cellsParentPanel.ReleaseCell(cell);
                                    }
                                }
                            }

                            if (cell.Control == null)
                            {
                                if (!this._usedCells.Contains(cell))
                                    this._usedCells.Add(cell);

                                RecyclingManager.Manager.AttachElement(cell, this);
                                cell.Control.EnsureContent();
                                cell.MeasuringSize = Size.Empty;

                                if (widthType != ColumnWidthType.Star)
                                {
                                    cell.Control.MeasureRaised = false;
                                    cell.Control.Measure(new Size(widthToMeasure, heightToMeasure));

                                    // Check to see that cell was actually measured.
                                    // If measure wasn't called, that's our first clue, as this cell was just attached. 
                                    if (!cell.Control.MeasureRaised)
                                    {
                                        // If it's content is smaller than the desired size, thats our second clue.
                                        FrameworkElement elem = cell.Control.Content as FrameworkElement;

                                        // In some cases, depeneding on what type of element we're dealing with, the elements don't always invalidate properly
                                        // So i added another case, which is kind of random, but if the width of the element is less than 10, then we're going to invalidate it.
                                        if (elem != null && ((elem.DesiredSize.Width > cell.Control.DesiredSize.Width) || elem.DesiredSize.Width < 10))
                                        {
                                            // So, set an invalid size first
                                            cell.Control.Measure(new Size(1, 1));

                                            // Then reapply the valid size, this will ensure that measure is called. 
                                            cell.Control.Measure(new Size(widthToMeasure, heightToMeasure));
                                        }
                                    }
                                }

                                cell.EnsureCurrentState();
                            }
                            else
                            {
                                cell.ApplyStyle();
                                cell.Control.EnsureContent();
                                if (widthType != ColumnWidthType.Star)
                                {
                                    cell.Control.MeasureRaised = false;

                                    Size sizeToMeasure;

                                    if (cell.MeasuringSize.IsEmpty)
                                        sizeToMeasure = new Size(widthToMeasure, heightToMeasure);
                                    else
                                        sizeToMeasure = cell.MeasuringSize;

                                    cell.Control.Measure(sizeToMeasure);

                                    if (!cell.Control.MeasureRaised)
                                    {
                                        cell.Control.ManuallyInvokeMeasure(sizeToMeasure);
                                    }

                                    // NZ 21 April 2012 - TFS96627 - Last try to remeasure the CellControl
                                    if (!cell.Control.MeasureRaised)
                                    {
                                        FrameworkElement elem = cell.Control.Content as FrameworkElement;

                                        if (elem != null && elem.DesiredSize.Width > cell.Control.DesiredSize.Width)
                                        {
                                            // So, set an invalid size first
                                            cell.Control.Measure(new Size(1, 1));

                                            // Then reapply the valid size, this will ensure that measure is called. 
                                            cell.Control.Measure(new Size(widthToMeasure, heightToMeasure));
                                        }
                                    }
                                }
                                cell.EnsureCurrentState();
                            }

                            CellControlBase control = cell.Control;
                            double width = control.DesiredSize.Width;

                            // Added some Validation for Columns what support Overflow tooltips
                            if (column.CachedAllowToolTips == AllowToolTips.Overflow)
                            {
                                CellControl cc = control as CellControl;
                                // Only supported on Cells
                                if (cc != null)
                                {
                                    // Make sure we didn't alreayd measure agains it's max
                                    if ((widthToMeasure != double.PositiveInfinity || heightToMeasure != double.PositiveInfinity) && cell.MeasuringSize.IsEmpty)
                                    {
                                        // Measure against it's max
                                        control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                                        // Store off value
                                        double desiredWidth = control.DesiredSize.Width;

                                        // Measure against its reality
                                        control.Measure(new Size(widthToMeasure, heightToMeasure));

                                        // Compare
                                        if (desiredWidth > width)
                                        {
                                            cc.ShowToolTip();
                                        }
                                        else
                                        {
                                            cc.HideToolTip();
                                        }
                                    }
                                }

                            }

                            switch (widthType)
                            {
                                case ColumnWidthType.InitialAuto:

                                    if (column.ActualWidth == 0)
                                        column.IsInitialAutoSet = false;

                                    if (!column.IsInitialAutoSet && column.ActualWidth < width)
                                        column.ActualWidth = width;
                                    break;

                                case ColumnWidthType.Auto:

                                    if (column.ActualWidth < width)
                                        column.ActualWidth = width;
                                    break;

                                case ColumnWidthType.Numeric:

                                    column.ActualWidth = colWidth.Value;

                                    break;

                                case ColumnWidthType.SizeToHeader:

                                    if (this.Row is HeaderRow)
                                    {
                                        if (column.ActualWidth < width)
                                            column.ActualWidth = width;
                                    }

                                    break;

                                case ColumnWidthType.SizeToCells:

                                    if (!(this.Row is HeaderRow) && !(this.Row is FooterRow))
                                    {
                                        if (column.ActualWidth < width)
                                            column.ActualWidth = width;
                                    }

                                    break;
                                case ColumnWidthType.Star:

                                    if (starColumns != null)
                                        starColumns.Add(cell);
                                    continue;
                            }

                            if (column.ActualWidth < column.MinimumWidth)
                            {
                                column.ActualWidth = column.MinimumWidth;
                                cell.Control.Measure(new Size(column.ActualWidth, heightToMeasure));
                            }

                            if (column.ActualWidth > column.MaximumWidth)
                                column.ActualWidth = column.MaximumWidth;

                            totalHeight = Math.Max(totalHeight, cell.Control.DesiredSize.Height);
                            totalWidth += column.ActualWidth;
                        }
                    }
                    else
                    {
                        cell.MeasuringSize = Size.Empty;
                    }
                }

                if (starColumns.Count > 0)
                {
                    this.SetupStarCells(availableSize.Width, totalWidth, ref totalHeight, starColumns);
                    totalWidth = Math.Max(totalWidth, availableSize.Width);
                }

                // NZ 8 May 2012 - TFS110390 - Do not overwrite the column size if the column is star-sized
                // and if there is a child column with star-size.
                if (!(starColumns.Count > 0 && this.Column.WidthResolved.WidthType == ColumnWidthType.Star))
                {
                    this.Column.Width = new ColumnWidth(totalWidth, false);
                }
            }

            return new Size(totalWidth, totalHeight);
        }

        #endregion // MeasureOverride

        #region ArrangeOverride

        /// <summary>
        /// Arranges <see cref="Cell"/> objects horizontally for a <see cref="GroupColumn"/>
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this._inMeasure = false;
            // Move all Cells that aren't being used, out of view. 
            // Note: we have to use GetAvailElements, instead of recent, b/c if we get taken out of the visual tree,
            // and then added back, all elements that were out of view, will not be arranged where we had arranged them.
            List<FrameworkElement> unusedCells = RecyclingManager.Manager.GetAvailableElements(this);
            foreach (FrameworkElement cell in unusedCells)
            {
                cell.Arrange(new Rect(-1000,-1000, 0,0));
            }

            if (this.Column != null && this.Row != null && this.Cell.Control != null)
            {
                double left = 0;
                foreach (Column column in this.Column.Columns)
                {
                    if (!column.IsInitialAutoSet)
                    {
                        bool ignoreAutoSet = false;

                        RowsManager manager = this.Row.Manager as RowsManager;
                        if (manager != null)
                        {
                            ignoreAutoSet = manager.RegisteredTopRows.Contains(this.Row);

                            if (!ignoreAutoSet)
                                ignoreAutoSet = manager.RegisteredBottomRows.Contains(this.Row);
                        }

                        if (!ignoreAutoSet && this.Cell != null && this.Cell.Control.IsCellLoaded)
                            column.IsInitialAutoSet = true;
                    }

                    CellBase cell = this.Row.ResolveCell(column);
                    if (cell != null && cell.Control != null)
                    {
                        if (column.IsMoving)
                        {
                            cell.Control.Arrange(new Rect(left, 0, 0, 0));
                            left += column.ActualWidth;
                        }
                        else
                        {
                            double leftVal = left;
                            // An Animation must be occurring, so lets arrange these columns into place gradually.
                            if (column.PercentMoved > 0)
                            {
                                if (!column.ReverseMove)
                                    leftVal = (left - column.MovingColumnsWidth) + (column.MovingColumnsWidth * column.PercentMoved);
                                else
                                    leftVal = (left + column.MovingColumnsWidth) - (column.MovingColumnsWidth * column.PercentMoved);
                            }


                            if (cell.Column.ActualWidth == 0 && cell.Column.Visibility == Visibility.Visible && cell.Column.WidthResolved.Value != 0)



                            {
                                // We were invalidated to zero width, even though we shouldn't be.
                                this.InvalidateMeasure();
                                break;
                            }

                            cell.Control.Arrange(new Rect(leftVal, 0, column.ActualWidth, finalSize.Height));
                            left += column.ActualWidth;
                        }
                    }
                }
            }


            return finalSize;
        }

        #endregion // ArrangeOverride

        #endregion // Overrides        

        #region EventHandlers

        void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this._previousColumns != null)
            {
                foreach (Column col in this._previousColumns)
                {
                    col.PropertyChanged -= this.Col_PropertyChanged;
                }
            }

            if (this.Column != null)
            {
                this._previousColumns = this.Column.AllColumns;

                foreach (var previousColumn in _previousColumns)
                {
                    previousColumn.PropertyChanged += this.Col_PropertyChanged;
                }

                if (!this._inMeasure)
                {
                    this.InvalidateMeasure();
                }
            }
            else
            {
                this._previousColumns = null;
            }
        }

        void Col_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!this._inMeasure && e.PropertyName != "IsSorted")
            {
                this.InvalidateMeasure();
            }
        }

        #endregion // EventHandlers
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