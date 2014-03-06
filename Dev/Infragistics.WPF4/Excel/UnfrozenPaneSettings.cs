using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Class which controls the way unfrozen panes are arranged and used for a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="note">
	/// <B>Note:</B> Frozen and unfrozen panes cannot be used simultaneously, so depending whether the panes are 
	/// frozen or unfrozen, these settings may not be used or saved.
	/// </p>
	/// </remarks>
	/// <seealso cref="DisplayOptions.PanesAreFrozen"/>
	/// <seealso cref="FrozenPaneSettings"/>



	public

		 class UnfrozenPaneSettings : PaneSettingsBase
	{
		#region Member Variables

		private int leftPaneWidth;
		private int topPaneHeight;

		private int firstColumnInLeftPane;
		private int firstRowInTopPane;

		#endregion Member Variables

		#region Constructor

		// MD 6/31/08 - Excel 2007 Format
		//internal UnfrozenPaneSettings() { }
		internal UnfrozenPaneSettings( DisplayOptions displayOptions )
			: base( displayOptions ) { }

		#endregion COnstructor

		#region Base Class Overrides

		#region HasHorizontalSplit

		internal override bool HasHorizontalSplit
		{
			get { return this.topPaneHeight > 0; }
		}

		#endregion HasHorizontalSplit

		#region HasVerticalSplit

		internal override bool HasVerticalSplit
		{
			get { return this.leftPaneWidth > 0; }
		}

		#endregion HasVerticalSplit

		#region InitializeFrom

		internal override void InitializeFrom( PaneSettingsBase paneSettings )
		{
			base.InitializeFrom( paneSettings );

			UnfrozenPaneSettings unfrozenPaneSettings = (UnfrozenPaneSettings)paneSettings;

			this.leftPaneWidth = unfrozenPaneSettings.leftPaneWidth;
			this.topPaneHeight = unfrozenPaneSettings.topPaneHeight;
			this.firstColumnInLeftPane = unfrozenPaneSettings.firstColumnInLeftPane;
			this.firstRowInTopPane = unfrozenPaneSettings.firstRowInTopPane;
		}

		#endregion InitializeFrom

		#region Reset

		/// <summary>
		/// Resets the unfrozen pane settings to their defaults.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public override void Reset()
		{
			base.Reset();

			this.leftPaneWidth = 0;
			this.topPaneHeight = 0;
			this.firstColumnInLeftPane = 0;
			this.firstRowInTopPane = 0;
		}

		#endregion Reset

		#endregion Base Class Overrides

		#region Properties

		#region FirstColumnInLeftPane

		/// <summary>
		/// Gets or sets the first visible column in the left pane(s) of the worksheet. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This affects the scroll position for the left pane(s) of the worksheet and is used regardless of whether or not the 
		/// worksheet is split vertically.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid column range (0 to one less than <see cref="Workbook.MaxExcelColumnCount"/> or 
		/// <see cref="Workbook.MaxExcel2007ColumnCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The zero-based index of the first visible column in the left pane(s).</value>
		public int FirstColumnInLeftPane
		{
			get { return this.firstColumnInLeftPane; }
			set
			{
				if ( this.firstColumnInLeftPane != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyColumnIndex( value, "value" );
					// MD 4/12/11 - TFS67084
					//Utilities.VerifyColumnIndex( this.DisplayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyColumnIndex(this.DisplayOptions.Worksheet, value, "value");

					this.firstColumnInLeftPane = value;
				}
			}
		}

		#endregion FirstColumnInLeftPane

		#region FirstRowInTopPane

		/// <summary>
		/// Gets or sets the first visible row in the top pane(s) of the worksheet. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This affects the scroll position for the top pane(s) of the worksheet and is used regardless of whether or not 
		/// the worksheet is split horizontally.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid row range (0 to one less than <see cref="Workbook.MaxExcelRowCount"/> or 
		/// <see cref="Workbook.MaxExcel2007RowCount"/>, depending on the workbook's 
		/// <see cref="Workbook.CurrentFormat"/>).
		/// </exception>
		/// <value>The zero-based index of the first visible row in the top pane(s).</value>
		public int FirstRowInTopPane
		{
			get { return this.firstRowInTopPane; }
			set
			{
				if ( this.firstRowInTopPane != value )
				{
					// MD 6/31/08 - Excel 2007 Format
					//Utilities.VerifyRowIndex( value, "value" );
					// MD 2/24/12 - 12.1 - Table Support
					// The workbook may be null.
					//Utilities.VerifyRowIndex( this.DisplayOptions.Worksheet.Workbook, value, "value" );
					Utilities.VerifyRowIndex(this.DisplayOptions.Worksheet, value, "value");

					this.firstRowInTopPane = value;
				}
			}
		}

		#endregion FirstRowInTopPane

		#region LeftPaneWidth

		/// <summary>
		/// Gets or sets the width of the left pane in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this value is zero, the left pane occupies the entire visible area of the worksheet.
		/// Otherwise, the left pane occupies the specified width and the right pane occupies
		/// the remaining area of the worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the width specified is outside the valid width range (0 to 65535).
		/// </exception>
		/// <value>The width of the left pane, or zero if there is no horizontal pane split.</value>
		public int LeftPaneWidth
		{
			get { return this.leftPaneWidth; }
			set
			{
				if ( this.leftPaneWidth != value )
				{
					if ( value < UInt16.MinValue || UInt16.MaxValue < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_LeftPaneWidth", UInt16.MinValue, UInt16.MaxValue ) );

					this.leftPaneWidth = value;
				}
			}
		}

		#endregion LeftPaneWidth

		#region TopPaneHeight

		/// <summary>
		/// Gets or sets the height of the top pane in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this value is zero, the top pane occupies the entire visible area of the worksheet.
		/// Otherwise, the top pane occupies the specified height and the bottom pane occupies
		/// the remaining area of the worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the height specified is outside the valid height range (0 to 65535).
		/// </exception>
		/// <value>The height of the top pane, or zero if there is no vertical pane split.</value>
		public int TopPaneHeight
		{
			get { return this.topPaneHeight; }
			set
			{
				if ( this.topPaneHeight != value )
				{
					if ( value < UInt16.MinValue || UInt16.MaxValue < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_TopPaneHeight", UInt16.MinValue, UInt16.MaxValue ) );

					this.topPaneHeight = value;
				}
			}
		}

		#endregion TopPaneHeight

		#endregion Properties

        #region Methods

        //  BF 8/11/08  Excel2007 Format
        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return  base.ShouldSerialize() &&
                    this.leftPaneWidth != 0 &&
                    this.topPaneHeight != 0 &&
                    this.firstColumnInLeftPane != 0 &&
                    this.firstRowInTopPane != 0;
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