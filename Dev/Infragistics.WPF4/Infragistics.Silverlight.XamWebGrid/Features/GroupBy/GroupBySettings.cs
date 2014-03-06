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
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using GroupBy on the <see cref="XamGrid"/>
	/// </summary>
	public class GroupBySettings : SettingsBase, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBySettings"/> class.
        /// </summary>
        public GroupBySettings()
        {
            this.EmptyGroupByAreaContent = SRGrid.GetString("EmptyGroupByAreaText");
        }

        #endregion // Constructor

        #region Members

        GroupByColumnsCollection _groupByColumns;

		#endregion // Members

		#region Properties

		#region AllowGroupByArea

		/// <summary>
		/// Identifies the <see cref="AllowGroupByArea"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowGroupByAreaProperty = DependencyProperty.Register("AllowGroupByArea", typeof(GroupByAreaLocation), typeof(GroupBySettings), new PropertyMetadata(new PropertyChangedCallback(AllowGroupByAreaChanged)));

		/// <summary>
		/// Gets/Sets the location of the GroupByArea in the <see cref="XamGrid"/>.
		/// </summary>
		public GroupByAreaLocation AllowGroupByArea
		{
			get { return (GroupByAreaLocation)this.GetValue(AllowGroupByAreaProperty); }
			set { this.SetValue(AllowGroupByAreaProperty, value); }
		}

		private static void AllowGroupByAreaChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("AllowGroupByArea");
		}

		#endregion // AllowGroupByArea 
				
		#region IsGroupable

		/// <summary>
		/// Identifies the <see cref="IsGroupable"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsGroupableProperty = DependencyProperty.Register("IsGroupable", typeof(bool), typeof(GroupBySettings), new PropertyMetadata(true, new PropertyChangedCallback(IsGroupableChanged)));

		/// <summary>
		/// Gets/Sets whether <see cref="Column"/> objects can be grouped via the UI for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public bool IsGroupable
		{
			get { return (bool)this.GetValue(IsGroupableProperty); }
			set { this.SetValue(IsGroupableProperty, value); }
		}

		private static void IsGroupableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("IsGroupable");
		}

		#endregion // IsGroupable 

		#region GroupByColumns

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that the data is GroupedBy.
		/// </summary>
        [Browsable(false)]
		public GroupByColumnsCollection GroupByColumns
		{
			get
			{
				if (this._groupByColumns == null)
				{
					this._groupByColumns = new GroupByColumnsCollection();
					this._groupByColumns.Grid = this.Grid;
				}

				return this._groupByColumns;
			}
		}

		#endregion // GroupByColumns

		#region EmptyGroupByAreaContent

		/// <summary>
		/// Identifies the <see cref="EmptyGroupByAreaContent"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EmptyGroupByAreaContentProperty = DependencyProperty.Register("EmptyGroupByAreaContent", typeof(object), typeof(GroupBySettings), new PropertyMetadata(new PropertyChangedCallback(EmptyGroupByAreaContentChanged)));

		/// <summary>
		/// Gets/Sets the content that will be displayed in the GroupByArea of the <see cref="XamGrid"/> when there are no Grouped columns.
		/// </summary>
		public object EmptyGroupByAreaContent
		{
			get { return (object)this.GetValue(EmptyGroupByAreaContentProperty); }
			set { this.SetValue(EmptyGroupByAreaContentProperty, value); }
		}

		private static void EmptyGroupByAreaContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("EmptyGroupByAreaContent");
		}

		#endregion // EmptyGroupByAreaContent 
				
		#region GroupByMovingIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="GroupByMovingIndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByMovingIndicatorStyleProperty = DependencyProperty.Register("GroupByMovingIndicatorStyle", typeof(Style), typeof(GroupBySettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByMovingIndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> for the <see cref="GroupByMovingIndicator"/> when rearranging headers in the GroupByArea for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style GroupByMovingIndicatorStyle
		{
			get { return (Style)this.GetValue(GroupByMovingIndicatorStyleProperty); }
			set { this.SetValue(GroupByMovingIndicatorStyleProperty, value); }
		}

		private static void GroupByMovingIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByMovingIndicatorStyle");
		}

		#endregion // GroupByMovingIndicatorStyle 

		#region GroupByAreaStyle

		/// <summary>
		/// Identifies the <see cref="GroupByAreaStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByAreaStyleProperty = DependencyProperty.Register("GroupByAreaStyle", typeof(Style), typeof(GroupBySettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByAreaStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to the droppable display area for the GroupBy feature.
		/// </summary>
		public Style GroupByAreaStyle
		{
			get { return (Style)this.GetValue(GroupByAreaStyleProperty); }
			set { this.SetValue(GroupByAreaStyleProperty, value); }
		}

		private static void GroupByAreaStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByAreaStyle");
		}

		#endregion // GroupByAreaStyle 

		#region GroupByHeaderStyle

		/// <summary>
		/// Identifies the <see cref="GroupByHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByHeaderStyleProperty = DependencyProperty.Register("GroupByHeaderStyle", typeof(Style), typeof(GroupBySettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to the draggable <see cref="GroupByHeaderCellControl"/> objects in the GroupByArea for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style GroupByHeaderStyle
		{
			get { return (Style)this.GetValue(GroupByHeaderStyleProperty); }
			set { this.SetValue(GroupByHeaderStyleProperty, value); }
		}

		private static void GroupByHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByHeaderStyle");
		}

		#endregion // GroupByHeaderStyle 

		#region GroupByColumnLayoutHeaderStyle

		/// <summary>
		/// Identifies the <see cref="GroupByColumnLayoutHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByColumnLayoutHeaderStyleProperty = DependencyProperty.Register("GroupByColumnLayoutHeaderStyle", typeof(Style), typeof(GroupBySettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByColumnLayoutHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to the <see cref="GroupByColumnLayoutHeaderCellControl"/> which represents the <see cref="ColumnLayout"/> of a <see cref="Column"/> that is grouped for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style GroupByColumnLayoutHeaderStyle
		{
			get { return (Style)this.GetValue(GroupByColumnLayoutHeaderStyleProperty); }
			set { this.SetValue(GroupByColumnLayoutHeaderStyleProperty, value); }
		}

		private static void GroupByColumnLayoutHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByColumnLayoutHeaderStyle");
		}

		#endregion // GroupByColumnLayoutHeaderStyle 

		#region GroupByRowStyle

		/// <summary>
		/// Identifies the <see cref="GroupByRowStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByRowStyleProperty = DependencyProperty.Register("GroupByRowStyle", typeof(Style), typeof(GroupBySettings), new PropertyMetadata(null, new PropertyChangedCallback(GroupByRowStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to every GroupBy row for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public Style GroupByRowStyle
		{
			get { return (Style)this.GetValue(GroupByRowStyleProperty); }
			set { this.SetValue(GroupByRowStyleProperty, value); }
		}

		private static void GroupByRowStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByRowStyle");
		}

		#endregion // GroupByRowStyle 

		#region IsGroupByAreaExpanded

		/// <summary>
		/// Identifies the <see cref="IsGroupByAreaExpanded"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsGroupByAreaExpandedProperty = DependencyProperty.Register("IsGroupByAreaExpanded", typeof(bool), typeof(GroupBySettings), new PropertyMetadata(true, new PropertyChangedCallback(IsGroupByAreaExpandedChanged)));

		/// <summary>
		/// Gets/Sets whether the GroupByArea is expanded. 
		/// </summary>
		/// <remarks>
		/// Note: this property is ignored, if the <see cref="ExpansionIndicatorVisibility"/> property is set to <see cref="Visibility.Collapsed"/>
		/// </remarks>
		public bool IsGroupByAreaExpanded
		{
			get { return (bool)this.GetValue(IsGroupByAreaExpandedProperty); }
			set { this.SetValue(IsGroupByAreaExpandedProperty, value); }
		}

		private static void IsGroupByAreaExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("IsGroupByAreaExpanded");
		}

		#endregion // IsGroupByAreaExpanded 

		#region ExpansionIndicatorVisibility

		/// <summary>
		/// Identifies the <see cref="ExpansionIndicatorVisibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorVisibilityProperty = DependencyProperty.Register("ExpansionIndicatorVisibility", typeof(Visibility), typeof(GroupBySettings), new PropertyMetadata( Visibility.Collapsed, new PropertyChangedCallback(ExpansionIndicatorVisibilityChanged)));

		/// <summary>
		/// Gets/Sets whether the ExpansionIndicator in the GroupByArea is visible. 
		/// </summary>
		public Visibility ExpansionIndicatorVisibility
		{
			get { return (Visibility)this.GetValue(ExpansionIndicatorVisibilityProperty); }
			set { this.SetValue(ExpansionIndicatorVisibilityProperty, value); }
		}

		private static void ExpansionIndicatorVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("ExpansionIndicatorVisibility");
		}

		#endregion // ExpansionIndicatorVisibility 

		#region GroupByRowHeight

		/// <summary>
		/// Identifies the <see cref="GroupByRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByRowHeightProperty = DependencyProperty.Register("GroupByRowHeight", typeof(RowHeight), typeof(GroupBySettings), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(GroupByRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="GroupByRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight GroupByRowHeight
		{
			get { return (RowHeight)this.GetValue(GroupByRowHeightProperty); }
			set { this.SetValue(GroupByRowHeightProperty, value); }
		}

		private static void GroupByRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByRowHeight");
		}

		#endregion // GroupByRowHeight

		#region GroupByAreaRowHeight

		/// <summary>
		/// Identifies the <see cref="GroupByAreaRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByAreaRowHeightProperty = DependencyProperty.Register("GroupByAreaRowHeight", typeof(RowHeight), typeof(GroupBySettings), new PropertyMetadata(RowHeight.Dynamic, new PropertyChangedCallback(GroupByAreaRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="GroupByAreaRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight GroupByAreaRowHeight
		{
			get { return (RowHeight)this.GetValue(GroupByAreaRowHeightProperty); }
			set { this.SetValue(GroupByAreaRowHeightProperty, value); }
		}

		private static void GroupByAreaRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettings settings = (GroupBySettings)obj;
			settings.OnPropertyChanged("GroupByAreaRowHeight");
		}

		#endregion // GroupByAreaRowHeight

        #region GroupByOperation

        /// <summary>
        /// Identifies the <see cref="GroupByOperation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty GroupByOperationProperty = DependencyProperty.Register("GroupByOperation", typeof(GroupByOperation), typeof(GroupBySettings), new PropertyMetadata(GroupByOperation.GroupByRows, new PropertyChangedCallback(GroupByOperationChanged)));

        /// <summary>
        /// Gets/Sets the type of operation that should occur when a column is grouped.
        /// </summary>
        public GroupByOperation GroupByOperation
        {
            get { return (GroupByOperation)this.GetValue(GroupByOperationProperty); }
            set { this.SetValue(GroupByOperationProperty, value); }
        }

        private static void GroupByOperationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GroupBySettings settings = (GroupBySettings)obj;
            settings.OnPropertyChanged("GroupByOperation");
            if (settings.Grid != null)
            {
                settings.Grid.InvalidateData();
                settings.Grid.Columns.InvalidateColumnsCollections(true);
            }
        }

        #endregion // GroupByOperation 

        #region DisplayCountOnGroupedRow

        /// <summary>
        /// Identifies the <see cref="DisplayCountOnGroupedRow"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DisplayCountOnGroupedRowProperty = DependencyProperty.Register("DisplayCountOnGroupedRow", typeof(bool), typeof(GroupBySettings), new PropertyMetadata(true, new PropertyChangedCallback(DisplayCountOnGroupedRowChanged)));

        /// <summary>
        /// Gets/Sets whether the count of how many child items are in the grouping is displayed on the <see cref="GroupByRow"/>
        /// </summary>
        public bool DisplayCountOnGroupedRow
        {
            get { return (bool)this.GetValue(DisplayCountOnGroupedRowProperty); }
            set { this.SetValue(DisplayCountOnGroupedRowProperty, value); }
        }

        private static void DisplayCountOnGroupedRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GroupBySettings settings = (GroupBySettings)obj;
            settings.OnPropertyChanged("DisplayCountOnGroupedRow");

            if (settings.Grid != null)
                settings.Grid.InvalidateScrollPanel(true, true);
        }

        #endregion // DisplayCountOnGroupedRow 
				

		#endregion // Properties

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="GroupBySettings"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._groupByColumns != null)
				this._groupByColumns.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="GroupBySettings"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
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