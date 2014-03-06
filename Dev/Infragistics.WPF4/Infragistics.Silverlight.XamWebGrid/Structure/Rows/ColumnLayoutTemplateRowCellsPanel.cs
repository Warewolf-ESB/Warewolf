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
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A panel that organizes the <see cref="CellBase"/>s of a <see cref="ColumnLayoutTemplateRow"/>.
	/// </summary>
	public class ColumnLayoutTemplateRowCellsPanel : CellsPanel
	{
		/// <summary>
		/// Lays out which cells will be displayed in the given viewport. 
		/// </summary>
		/// <param propertyName="availableWidth">The total width that the cells have to work with.</param>
		protected override Size RenderCells(double availableWidth)
		{
			RowsManager manager = (RowsManager)this.Row.Manager;
			double maxHeight = 0;

            this.Row.Manager.CachedIndentation = this.Row.Manager.ResolveIndentation(this.Row);
            double width = this.Row.Manager.CachedIndentation;
			availableWidth -= width;

            

			double heightToMeasure = double.PositiveInfinity;
			RowHeight rowHeight = this.Row.HeightResolved;
			if (rowHeight.HeightType == RowHeightType.Numeric)
				heightToMeasure = rowHeight.Value;

			CellBase cell = this.Row.ResolveCell(manager.ColumnLayoutTemplateColumn);
            if (cell != null)
            {
                this.VisibleCells.Add(cell);

                if (cell.Control == null)
                {
                    RecyclingManager.Manager.AttachElement(cell, this);
                    cell.Control.EnsureContent();
                    cell.MeasuringSize = Size.Empty;
                    cell.Control.Measure(new Size(availableWidth, heightToMeasure));
                    cell.EnsureCurrentState();
                }
                else
                {
                    cell.ApplyStyle();
                    cell.Control.EnsureContent();
                    if (cell.MeasuringSize.IsEmpty)
                        cell.Control.Measure(new Size(availableWidth, heightToMeasure));
                    else
                        cell.Control.Measure(cell.MeasuringSize);
                    cell.EnsureCurrentState();
                }

                width = cell.Control.DesiredSize.Width;

                if(!double.IsPositiveInfinity(width))
                    width = availableWidth;

                manager.ColumnLayoutTemplateColumn.ActualWidth = width;
                maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
            }

            return new Size(manager.ColumnLayoutTemplateColumn.ActualWidth, maxHeight);
		}
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