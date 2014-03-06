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
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A panel that lays out <see cref="ComboCellsPanel"/> objects in virtualized vertical list.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class MultiColumnComboItemsPanel : ItemsPanelBase<ComboRow, ComboCellsPanel>
	{
		#region Member Variables

		private List<ComboRowBase> _fixedRowsTop;

		#endregion //Member Variables

		#region Overrides

		#region FixedRowsTop

		/// <summary>
		/// Gets the rows that are currently fixed to the top of the Viewport.
		/// </summary>
		public override List<ComboRowBase> FixedRowsTop
		{
			get 
			{
				if (this._fixedRowsTop == null)
					this._fixedRowsTop = new List<ComboRowBase>();

				return this._fixedRowsTop;
			}
		}

		#endregion // FixedRowsTop
        
        #region OnControlAttachedToItem

        /// <summary>
        /// Raised when a control is attached to an item, before measure is called.
        /// </summary>
        /// <param name="cntrl"></param>
        protected override void OnControlAttachedToItem(ComboCellsPanel cntrl)
        {
            cntrl.Owner = this;
            base.OnControlAttachedToItem(cntrl);
        }

        #endregion // OnControlAttachedToItem

        #region MeasureOverride

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the FrameworkElement-derived class. 
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes. </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.ComboEditor != null)
            {
                XamMultiColumnComboEditor combo = ((XamMultiColumnComboEditor)this.ComboEditor);
                combo.IsFirstRowRenderingInThisLayoutCycle = true;
                combo.OverflowAdjustment = 0;
                combo.RowWidth = 0;
                combo.IndexOfFirstColumnRendered = 0; 
            }

            return base.MeasureOverride(availableSize);
        }
        #endregion

        #region UseAvailableWidthToMeasureItem
        /// <summary>
        /// Gets whether an item should measured with the available width or Infinity
        /// </summary>
        protected override bool UseAvailableWidthToMeasureItem
        {
            get
            {
                return true;
            }
        }
        #endregion // UseAvailableWidthToMeasureItem

        #region UpdateHorizontalScrollBar

        /// <summary>
        /// Updates the horizontal scrollbar, based on how many items it can scroll horizontally
        /// </summary>
        /// <param name="horizBar"></param>
        /// <param name="finalSize"></param>
        protected override void UpdateHorizontalScrollBar(ScrollBar horizBar, Size finalSize)
        {
            XamMultiColumnComboEditor combo = (XamMultiColumnComboEditor)this.ComboEditor;
            double max = combo.OverrideHorizontalMax;
            double totalCellCount = combo.ScrollableCellCount, totalVisibleCellCount = combo.VisibleCellCount;
            
            if (max != -1 && !double.IsPositiveInfinity(max) && !double.IsNaN(max))
            {
                // The Scrollbar, actually stores off the original value, when the new max is greater than the previous value
                // Which means, that we can get some weird jumping behavior when resizing columns that cause the horizbar's
                // maximum to change. So, lets make sure the value changes along with the maximum.
                if (horizBar.Value > max)
                    horizBar.Value = max;
                horizBar.Maximum = max;

                // Turns out setting this to 1, is actually better than totalVisibleCellCount
                horizBar.ViewportSize = 1;
            }

            horizBar.LargeChange = totalVisibleCellCount;
            horizBar.SmallChange = 1;

            bool collapsed = (max <= 0) && (totalVisibleCellCount == totalCellCount || totalCellCount == 0);

            // Once the scrollbar becomes visible, keep it, until otherwise told to reset. 
            if (horizBar.Visibility == Visibility.Collapsed)
            {
                horizBar.Visibility = (collapsed) ? Visibility.Collapsed : Visibility.Visible;

                horizBar.IsEnabled = true;
            }
            else if (collapsed)
            {
	            if (horizBar.Visibility == Visibility.Visible)
					horizBar.Visibility = Visibility.Collapsed;
            }

            if (horizBar.Visibility == Visibility.Collapsed)
                horizBar.Value = 0; 
        }

        #endregion // UpdateHorizontalScrollBar

        #region ArrangeItem

        /// <summary>
        /// Arranges the specified item in the panel
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        protected override void ArrangeItem(FrameworkElement elem, double left, double top, double width)
        {
            elem.Arrange(new Rect(0, top, elem.DesiredSize.Width, elem.DesiredSize.Height));
        }

        #endregion // ArrangeItem

        #endregion //Overrides

        #region Methods

        #region Internal Methods

        #region RegisterFixedRowTop

        internal void RegisterFixedRowTop(ComboRowBase row)
		{
			if (false == this.FixedRowsTop.Contains(row))
				this.FixedRowsTop.Add(row);
		}

		#endregion //RegisterFixedRowTop

		#region UnregisterFixedRowTop

		internal void UnregisterFixedRowTop(ComboRowBase row)
		{
			if (this.FixedRowsTop.Contains(row))
				this.FixedRowsTop.Remove(row);
		}

		#endregion //UnregisterFixedRowTop

		#endregion //Internal Methods

		#endregion //Methods
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