using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Class which controls the way frozen panes are arranged and used for a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="note">
	/// <B>Note:</B> Frozen and unfrozen panes cannot be used simultaneously, so depending whether the panes are 
	/// frozen or unfrozen, these settings may not be used or saved.
	/// </p>
	/// </remarks>
	/// <seealso cref="DisplayOptions.PanesAreFrozen"/>
	/// <seealso cref="UnfrozenPaneSettings"/>



	public

		 class FrozenPaneSettings : PaneSettingsBase
	{
		#region Member Variables

		private int frozenColumns;
		private int frozenRows;

		#endregion Member Variables

		#region Constructor

		// MD 6/31/08 - Excel 2007 Format
		//internal FrozenPaneSettings() { }
		internal FrozenPaneSettings( DisplayOptions displayOptions )
			: base( displayOptions ) { }

		#endregion Constructor

		#region Base Class Overrides

		#region HasHorizontalSplit

		internal override bool HasHorizontalSplit
		{
			get { return this.frozenRows > 0; }
		}

		#endregion HasHorizontalSplit

		#region HasVerticalSplit

		internal override bool HasVerticalSplit
		{
			get { return this.frozenColumns > 0; }
		}

		#endregion HasVerticalSplit

		#region InitializeFrom

		internal override void InitializeFrom( PaneSettingsBase paneSettings )
		{
			base.InitializeFrom( paneSettings );

			FrozenPaneSettings frozenPaneSettings = (FrozenPaneSettings)paneSettings;

			this.frozenColumns = frozenPaneSettings.frozenColumns;
			this.frozenRows = frozenPaneSettings.frozenRows;
		}

		#endregion InitializeFrom

		#region Reset

		/// <summary>
		/// Resets the frozen pane settings to their defaults.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public override void Reset()
		{
			base.Reset();

			this.frozenColumns = 0;
			this.frozenRows = 0;
		}

		#endregion Reset

		#endregion Base Class Overrides

		#region Properties

		#region FrozenColumns

		/// <summary>
		/// Gets or sets the number of columns frozen at the left of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The frozen columns will always remain in view, regardless of the horizontal scroll position of 
		/// the worksheet.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> If the number of frozen columns specified is more than the amount of visible columns 
		/// in the worksheet, the worksheet may not scroll correctly.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is not a valid column count (0 to <see cref="Workbook.MaxExcelColumnCount"/> or 
		/// <see cref="Workbook.MaxExcel2007ColumnCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The number of columns frozen at the left of the worksheet.</value>
		public int FrozenColumns
		{
			get { return this.frozenColumns; }
			set 
			{
				if ( this.frozenColumns != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyColumnCount( value, "value" );
					// MD 2/24/12 - 12.1 - Table Support
					// The workbook may be null.
					//Utilities.VerifyColumnCount( this.DisplayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyColumnCount(this.DisplayOptions.Worksheet, value, "value");

					this.frozenColumns = value;
				}
			}
		}

		#endregion FrozenColumns

		#region FrozenRows

		/// <summary>
		/// Gets or sets the number of rows frozen at the top of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The frozen rows will always remain in view, regardless of the vertical scroll position of 
		/// the worksheet.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> If the number of frozen rows specified is more than the amount of visible rows 
		/// in the worksheet, the worksheet may not scroll correctly.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is not a valid row count (0 to <see cref="Workbook.MaxExcelRowCount"/> or 
		/// <see cref="Workbook.MaxExcel2007RowCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The number of rows frozen at the top of the worksheet.</value>
		public int FrozenRows
		{
			get { return this.frozenRows; }
			set 
			{
				if ( this.frozenRows != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyRowCount( value, "value" );
					// MD 2/24/12 - 12.1 - Table Support
					// The workbook may be null.
					//Utilities.VerifyRowCount( this.DisplayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyRowCount(this.DisplayOptions.Worksheet, value, "value");

					this.frozenRows = value;
				}
			}
		}

		#endregion FrozenRows

		#endregion Properties

        #region Methods

        //  BF 8/11/08  Excel2007 Format
        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return  base.ShouldSerialize() &&
                    this.frozenRows != 0 &&
                    this.frozenColumns != 0;
        }
        #endregion ShouldSerialize

        #endregion Methods
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