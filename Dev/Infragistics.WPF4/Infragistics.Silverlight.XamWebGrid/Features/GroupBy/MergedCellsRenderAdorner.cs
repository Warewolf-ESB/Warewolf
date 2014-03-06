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
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// An object uesed to add elements to the <see cref="RowsPanel"/> specifically revoling around Merged Cells.
    /// </summary>
    public class MergedCellsRenderAdorner : XamGridRenderAdorner
    {
        #region Members

        Dictionary<RowBase, List<MergedColumnInfo>> _summaryRowLookup = new Dictionary<RowBase, List<MergedColumnInfo>>();
        Dictionary<RowBase, CellBase> _separtorCells = new Dictionary<RowBase, CellBase>();
        Dictionary<MergedColumnInfo, CellBase> _cellsToUpdate = new Dictionary<MergedColumnInfo, CellBase>();

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializs a new instance of the <see cref="MergedCellsRenderAdorner"/>
        /// </summary>
        /// <param name="grid"></param>
        public MergedCellsRenderAdorner(XamGrid grid) : base(grid)
        {
        }

        #endregion // Constructor

        #region Overrides

        #region Reset

        /// <summary>
        /// Releases all elements that this specific <see cref="XamGridRenderAdorner"/> has added to the <see cref="RowsPanel"/>
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            this._summaryRowLookup.Clear();
            this._separtorCells.Clear();
            this._cellsToUpdate.Clear();
        }

        #endregion // Reset      

        #region Initialize

        /// <summary>
        /// Allows the <see cref="XamGridRenderAdorner"/> to prepare itself for a new Layout phase
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            this._summaryRowLookup.Clear();
            this._separtorCells.Clear();
            this._cellsToUpdate.Clear();
        }

        #endregion // Initialize

        #region MeasureAfterRow

        /// <summary>
        /// If additional content is going to added after the row, this is where its measured
        /// </summary>
        /// <param name="row"></param>
        /// <returns>The additional height that will be appended to the row. </returns>
        public override double MeasureAfterRow(RowBase row)
        {
            double totalHeight = 0;
            bool sepAdded = false;

            MergedRowInfo mri = row.MergeData;          

            // If mri isn't null, then we have merged columns for this row. 
            // So lets build out all the info we need about it.
            if (mri != null && mri.MergedGroups.Count > 0)
            {
                RowsManagerBase rmb = row.Manager;

                int index = 0;
                ReadOnlyCollection<Column> mergedColumns = row.Columns.GroupByColumns[row.ColumnLayout];

                // Loop through all merged columns
                foreach (Column col in mergedColumns)
                {
                    CellBase currentCell = row.Cells[col];

                    // We only need to know about cells that have controls.
                    if (currentCell.Control != null)
                    {
                        // Find the specific MergedColumnInfo, by the index of the column we're at.
                        MergedColumnInfo mci = mri.MergedGroups[index];

                        List<RenderMergeInfo> mergedObjects;

                        // On the RowsManager, we store all the cells for a particular column that we've already come across
                        // If we we haven't come accross this column before, create a new List of RenderMergeInfo objects
                        // otherwise, load up the ons that we've already come across
                        if (!rmb.RenderedMergedCells.ContainsKey(col))
                        {
                            mergedObjects = new List<RenderMergeInfo>();
                            rmb.RenderedMergedCells.Add(col, mergedObjects);
                        }
                        else
                        {
                            mergedObjects = rmb.RenderedMergedCells[col];
                        }

                        CellBase topMostVisibleCellForMergedGroup = null;
                        RenderMergeInfo rmi = null;

                        // If we have come across this column before
                        // Then load the last RenderMergeInfo, and find out the top most cell
                        if (mergedObjects.Count > 0)
                        {
                            rmi = mergedObjects[mergedObjects.Count - 1];

                            if (rmi.Key == mci.Key && rmi.ParentMergedColumnInfo == mci.ParentMergedColumnInfo)
                                topMostVisibleCellForMergedGroup = rmi.Cell;
                        }

                        // If we're dealing with ourselves, then don't do anything.
                        if (topMostVisibleCellForMergedGroup != currentCell)
                        {
                            ExpandableRowBase currentRow = currentCell.Row as ExpandableRowBase;
                            bool currentRowExpanded = (currentRow != null && currentRow.IsExpanded);

                            // If the topMostCell is null, create a new RMI, or  if last accessed cell of the rmi
                            // was expanded, also create a new rmi.
                            if (topMostVisibleCellForMergedGroup != null && !rmi.LastCellIsExpanded && !currentRow.IsExpanded)
                            {
                                // Make sure the topMostCell still has a control (should always be the case)
                                if (topMostVisibleCellForMergedGroup.Control != null)
                                {
                                    int currentIndex = currentCell.Row.Index;
                                    int topIndex = topMostVisibleCellForMergedGroup.Row.Index;

                                    if (topIndex > currentIndex)
                                    {
                                        CellBase temp = topMostVisibleCellForMergedGroup;
                                        topMostVisibleCellForMergedGroup = currentCell;                                        
                                        currentCell = temp;
                                        topMostVisibleCellForMergedGroup.ActualMergedHeight = currentCell.ActualMergedHeight;
                                        rmi.Cell = topMostVisibleCellForMergedGroup;
                                    }

                                    // Only add to the ActualMergedHeight for cells that we haven't accounted for.
                                    if (!rmi.CellsAlreadyAccountedFor.Contains(currentCell))
                                    {
                                        // if we haven accounted for this particualr row,
                                        // Add it to the needed height that this MergedCell will need to be.
                                        topMostVisibleCellForMergedGroup.ActualMergedHeight += row.ActualHeight;
                                        rmi.CellsAlreadyAccountedFor.Add(currentCell);
                                    }

                                    // Since this cell isn't the top most, make sure we dont' stroe a height on it. 
                                    currentCell.ActualMergedHeight = 0;

                                    // Currently this is the last visible cell.
                                    rmi.LastVisibleCell = currentCell;
                                }
                            }
                            else
                            {
                                // Ok, so we need to create a new RenderMergeInfo
                                currentCell.ActualMergedHeight = row.ActualHeight;
                                rmi = new RenderMergeInfo() { Key = mci.Key, Cell = currentCell };
                                rmi.MergedColumnInfo = mci;
                                rmi.ParentMergedColumnInfo = mci.ParentMergedColumnInfo;
                                mergedObjects.Add(rmi);
                                rmi.LastVisibleCell = currentCell;

                                rmi.LastCellIsExpanded = currentRowExpanded;
                            }
                        }

                        if (rmi != null)
                        {
                            // Keep track of all Cells that we're measure and their associated MCI.
                            if (!_cellsToUpdate.ContainsKey(mci))
                            {
                                _cellsToUpdate.Add(mci, rmi.Cell);
                            }
                            else
                            {
                                _cellsToUpdate[mci] = rmi.Cell;
                            }

                            // Are we the lat row in the group?
                            if (mri.IsLastRowInGroup[rmi.LastVisibleCell.Column])
                            {
                                // Do we have any summaries?
                                if (mci.Summaries.Count > 0)
                                {
                                    bool applySummary = false; 

                                    // We can display the summaries except when the first column merged by is the only
                                    // column with summaries. B/c otherwise, you'll never see the summaries. 
                                    foreach (SummaryDefinition sd in mci.Summaries)
                                    {
                                        Column sdCol = row.ColumnLayout.Columns.AllColumns[sd.ColumnKey] as Column;
                                        if (sdCol != null)
                                        {
                                            if (!sdCol.IsGroupBy || mergedColumns.IndexOf(sdCol) > 0)
                                            {
                                                applySummary = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (applySummary)
                                    {
                                        // Ok, see if we've renderd a summary row for this row before. if not, the register it.
                                        if (!this._summaryRowLookup.ContainsKey(row))
                                            this._summaryRowLookup.Add(row, new List<MergedColumnInfo>());

                                        // The latest should always be on top
                                        this._summaryRowLookup[row].Insert(0, mci);

                                        // See if we registered a SummaryRow yet.
                                        CellsPanel cp = this.GetElement(mci, "MergedSummaryRow") as CellsPanel;
                                        if (cp == null)
                                        {
                                            // If we haven't, createa new instance and Render it.
                                            MergedSummaryRow sr = new MergedSummaryRow((RowsManager)row.Manager);
                                            sr.MergedColumnInfo = mci;
                                            this.RowsPanel.RenderRow(sr);
                                            cp = (CellsPanel)this.AddElement(mci, "MergedSummaryRow", sr.Control);
                                        }
                                        else
                                        {
                                            // Otherwise, makse sure we register its MergedColumnInfo and render it.
                                            ((MergedSummaryRow)cp.Row).MergedColumnInfo = mci;
                                            this.RowsPanel.RenderRow(cp.Row);
                                        }

                                        // Since we added a summary row, we need to adjust the ActualMergedHeight for every
                                        // MergedColumnInfo in the hierarchy, so we walk up through our Parents
                                        MergedColumnInfo currentMCI = mci;
                                        while (currentMCI != null)
                                        {
                                            _cellsToUpdate[currentMCI].ActualMergedHeight += cp.DesiredSize.Height;
                                            currentMCI = currentMCI.ParentMergedColumnInfo;
                                        }

                                        // Update the overall height we've appended
                                        totalHeight += cp.DesiredSize.Height;
                                    }

                                }

                                // Measure FixedRowSeparator
                                FixedRowSeparator separator = this.GetElement(row, "MergedRowSeparator") as FixedRowSeparator;
                                if (separator == null)
                                    separator = (FixedRowSeparator)this.AddElement(row, "MergedRowSeparator", new FixedRowSeparator());
                                separator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                                if (!this._separtorCells.ContainsKey(row))
                                    this._separtorCells.Add(row, rmi.LastVisibleCell);

                                // Only update the ActualMergedHeight on parentMci's when we're on the last column, 
                                // otherwise we risk duplicating adding multiple separator heights. 
                                if (index == mergedColumns.Count - 1)
                                {
                                    bool childIsLastOne = true;

                                    // Walk through the parents of the last row.
                                    MergedColumnInfo parentMci = mci.ParentMergedColumnInfo;
                                    while (parentMci != null)
                                    {
                                        if (this._cellsToUpdate.ContainsKey(parentMci))
                                        {
                                            CellBase parentCell = this._cellsToUpdate[parentMci];

                                            // IF this isn't the parent's last row, then increase its MergedHeight
                                            if (!row.MergeData.IsLastRowInGroup[parentCell.Column])
                                            {
                                                parentCell.ActualMergedHeight += separator.DesiredSize.Height;
                                                childIsLastOne = false;
                                            }
                                            else
                                            {
                                                // If our child wasn't the last row, update our merged height, 
                                                // otherwise, don't bother, we've already gotten the height appended to us. 
                                                if (!childIsLastOne)
                                                    parentCell.ActualMergedHeight += separator.DesiredSize.Height;

                                                childIsLastOne = true;
                                            }

                                            parentMci = parentMci.ParentMergedColumnInfo;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (!sepAdded)
                                {
                                    // Update the overall height we've appended
                                    totalHeight += separator.DesiredSize.Height;
                                    sepAdded = true;
                                }
                            }
                        }
                    }

                    // increase the index, so that we get the correct next MergedColumnInfo
                    index++;
                }
            }

            return totalHeight;
        }

        #endregion // MeasureAfterRow

        #region MeasureAdorners

        /// <summary>
        /// Measures MergedContentControls, SummaryRows and separators for the merged cells feature.
        /// </summary>
        /// <param name="availableSize"></param>
        protected override void MeasureAdorners(Size availableSize)
        {
            // For merged cells, we need to insert seperators.
            foreach (RowsManagerBase manager in this.RowsPanel.VisibleManagers)
            {
                // See if a particular manager has any RenderedMergedCells, if not then we don't need to do anything else.
                if (manager.RenderedMergedCells.Count > 0)
                {
                    // Ok We have RenderedMergedCells, so lets loop through them.
                    foreach (KeyValuePair<Column, List<RenderMergeInfo>> kvp in manager.RenderedMergedCells)
                    {
                        foreach (RenderMergeInfo rmi in kvp.Value)
                        {
                            Cell cell = (Cell)rmi.Cell;
                            // See if we have rendered a MCC before, if not, add it. 
                            // Note: don't update height, as this is an adorner, and not something rendred inline.
                            MergedContentControl mcc = this.GetElement(rmi.Cell, "MergedContentControl") as MergedContentControl;
                            if (mcc == null)
                            {
                                mcc = (MergedContentControl)this.AddElement(rmi.Cell, "MergedContentControl", new MergedContentControl());

                                if (cell.Column.MergedItemTemplate != null)
                                {
                                    mcc.DataContext = new MergeDataContext() { Records = rmi.MergedColumnInfo.Children, Value = rmi.Key };
                                    mcc.Content = cell.Column.MergedItemTemplate.LoadContent();
                                }
                                else
                                {
                                    ColumnContentProviderBase ccpb = rmi.Cell.Column.GenerateContentProvider();
                                    // Mark it as a tooltip, so that we don't raise any events, or have the content added to the cell.Control.
                                    ccpb.IsToolTip = true;
                                    Binding b = ccpb.ResolveBinding(cell);
                                    mcc.Content = ccpb.ResolveDisplayElement(cell, b);

                                    TemplateColumn tc = cell.Column as TemplateColumn;

                                    if (tc != null && tc.ItemTemplate == null && mcc.Content is ContentPresenter)
                                    {
                                        // [v12.1] Support for implicit DataTemplates
                                        ((ContentPresenter)mcc.Content).Content = rmi.Cell.Value;
                                    }
                                    else
                                    {
                                        mcc.DataContext = rmi.Cell.Row.Data;
                                    }
                                    
                                    mcc.ContentProvider = ccpb;
                                }
                            }

                            mcc.HorizontalContentAlignment = cell.Column.HorizontalContentAlignment;

                            mcc.Measure(new Size(rmi.Cell.Column.ActualWidth, rmi.Cell.ActualMergedHeight));

                            if (mcc.ContentProvider != null)
                                mcc.ContentProvider.AdjustDisplayElement(cell);
                        }
                    }
                }
            }
        }
        #endregion // MeasureAdorners

        #region ArrangeAfterRow

        /// <summary>
        /// If needed, SummaryRows and Separators are appended to a <see cref="RowBase"/>
        /// </summary>
        /// <param name="row"></param>
        /// <param name="top"></param>
        /// <param name="finalSize"></param>
        /// <returns>The total height of the elements after the row.</returns>
        public override double ArrangeAfterRow(RowBase row, double top, Size finalSize)
        {
            double totalHeight = 0;

            if (row.MergeData != null)
            {
                if(row.IsMouseOver && row.ColumnLayout.Grid.RowHover == RowHoverType.Row && row.RowType == RowType.DataRow)
                    Canvas.SetZIndex(row.Control, 10001);

                // If we have an ActiveRow, display it above the MergedContentControl
                if (row.IsActive)
                    Canvas.SetZIndex(row.Control, 10001);

                Row r = row as Row;
                if(r != null && r.IsSelected)
                    Canvas.SetZIndex(row.Control, 10001);

                foreach (MergedColumnInfo mci in row.MergeData.MergedGroups)
                {
                    if (mci.MergingObject != null)
                    {
                        if (row.Cells[(Column)mci.MergingObject].IsSelected)
                            Canvas.SetZIndex(row.Control, 10001);
                    }
                }

                List<MergedColumnInfo> mcis;
                this._summaryRowLookup.TryGetValue(row, out mcis);

                // Are there any summary rows to arrange?
                if (mcis != null)
                {
                    foreach (MergedColumnInfo mci in mcis)
                    {   UIElement element = this.GetElement(mci, "MergedSummaryRow");
                        if (element != null)
                        {
                            element.Arrange(new Rect(0, top, finalSize.Width, element.DesiredSize.Height));
                            totalHeight += element.DesiredSize.Height;
                            top += element.DesiredSize.Height;
                            Canvas.SetZIndex(element, 0);
                        }
                    }
                }

                UIElement separator = this.GetElement(row, "MergedRowSeparator");
                // Is there a separator to arrange?
                if (separator != null)
                {
                    CellBase cell = this._separtorCells[row];
                    if (cell != null && cell.Control != null)
                    {
                        Point p = cell.Control.TransformToVisual(this.RowsPanel).Transform(new Point(0, 0));

                        separator.Arrange(new Rect(p.X, top, finalSize.Width, separator.DesiredSize.Height));
                        double height = separator.DesiredSize.Height;
                        totalHeight += height;
                        top += height;
                        Canvas.SetZIndex(separator, 0);

                        // The Separator will actually cause a space to be put in between all the Fixed AdornerColumns (Expansion Indicator, RowSelector, etc..)
                        foreach (Column col in cell.Column.ColumnLayout.Columns.FixedAdornerColumns)
                        {
                            // So loop through each column, and increase control size, and the clip of the row control, by the separator height.
                            CellBase adornderCell = row.Cells[col];
                            if (adornderCell.Control != null)
                            {
                                Rect layoutSlot = LayoutInformation.GetLayoutSlot(adornderCell.Control);
                                adornderCell.Control.Arrange(new Rect(layoutSlot.Left, layoutSlot.Top, col.ActualWidth, layoutSlot.Height + height));
                                Rect currentClip = adornderCell.Row.Control.ClipRect.Rect;
                                if (currentClip.IsEmpty)
                                    currentClip = new Rect(0, 0, 0, 0);
                                adornderCell.Row.Control.ClipRect.Rect = new Rect(currentClip.X, currentClip.Y, currentClip.Width, currentClip.Height + height);
                            }
                        }
                    }
                }
            }

            return totalHeight;
        }
        #endregion // ArrangeAfterRow

        #region ArrangeAdornments

        /// <summary>
        /// Arranges MergedContentcontrols over the <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="finalSize"></param>
        /// <param name="dataRowTop"></param>
        public override void ArrangeAdornments(Size finalSize, double dataRowTop)
        {
            foreach (RowsManagerBase manager in this.RowsPanel.VisibleManagers)
            {
                // Are there any merged cells?
                if (manager.RenderedMergedCells.Count > 0)
                {
                    foreach (KeyValuePair<Column, List<RenderMergeInfo>> kvp in manager.RenderedMergedCells)
                    {
                        foreach (RenderMergeInfo rmi in kvp.Value)
                        {
                            UIElement elem = this.GetElement(rmi.Cell, "MergedContentControl");
                            
                            // We have a MergedCell, now we need to figure out where to arrange it. 
                            if (elem != null)
                            {
                                double height = rmi.Cell.ActualMergedHeight;

                                // Remove the border width, so that we can still see the underlying cells border width.
                                double width = rmi.Cell.Column.ActualWidth - rmi.Cell.Control.BorderThickness.Right;
                                if (width < 0)
                                    width = 0; 

                                // Look at the top most Cell control, and figure out its position.
                                Point p = rmi.Cell.Control.TransformToVisual(this.RowsPanel).Transform(new Point(0, 0));
                                
                                // Just make sure its not arranged out of view, which is (-1000)
                                if (p.Y > -999)
                                {
                                    // is it clipped by the fixed rows?
                                    if (p.Y < dataRowTop)
                                    {
                                        double diff = dataRowTop - p.Y;
                                        p.Y = dataRowTop;
                                        height -= diff;
                                        if (height < 0)
                                            height = 0;

                                        if (height < rmi.Cell.Row.ActualHeight)
                                        {
                                            diff = rmi.Cell.Row.ActualHeight - height;
                                            height = rmi.Cell.Row.ActualHeight;
                                            p.Y -= diff;
                                        }
                                    }

                                    // Arrange it, and set its position on top of everything else.
                                    elem.Arrange(new Rect(p.X, p.Y, width, height));
                                    Canvas.SetZIndex(elem, 1000);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion // ArrangeAdornments

        #endregion // Overrides
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