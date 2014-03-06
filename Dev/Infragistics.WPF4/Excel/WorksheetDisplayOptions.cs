using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Class which exposes the display options which can only be controlled through the worksheet.
	/// </summary>
	/// <seealso cref="Worksheet.DisplayOptions"/>
	/// <seealso cref="CustomViewDisplayOptions"/>



	public

		 class WorksheetDisplayOptions : DisplayOptions
	{
		#region Member Variables

		private int magnificationInNormalView = DisplayOptions.DefaultMagnificationInNormalView;
		private int magnificationInPageBreakView = DisplayOptions.DefaultMagnificationInPageBreakView;
		private int magnificationInPageLayoutView = DisplayOptions.DefaultMagnificationInPageLayoutView;
		private bool orderColumnsRightToLeft;
		private bool showWhitespaceInPageLayoutView = true;

		// MD 1/26/12 - 12.1 - Cell Format Updates
		//private int tabColorIndex = WorkbookColorCollection.AutomaticColor;
		private WorkbookColorInfo tabColorInfo;

		#endregion Member Variables

		#region Constructors

		internal WorksheetDisplayOptions( Worksheet worksheet )
			: base( worksheet ) { }

		#endregion Constructors

		#region Base Class Overrides

		#region InitializeFrom






		internal override void InitializeFrom( DisplayOptions displayOptions )
		{
			base.InitializeFrom( displayOptions );

			// If the specified instance is another WorksheetDisplayOptions instance, copy all memeber variables
			WorksheetDisplayOptions worksheetOptions = displayOptions as WorksheetDisplayOptions;

			if ( worksheetOptions != null )
			{
				this.magnificationInNormalView = worksheetOptions.magnificationInNormalView;
				this.magnificationInPageBreakView = worksheetOptions.magnificationInPageBreakView;
				this.magnificationInPageLayoutView = worksheetOptions.magnificationInPageLayoutView;
				this.orderColumnsRightToLeft = worksheetOptions.orderColumnsRightToLeft;
				this.showWhitespaceInPageLayoutView = worksheetOptions.showWhitespaceInPageLayoutView;

				// MD 1/26/12 - 12.1 - Cell Format Updates
				//this.tabColorIndex = worksheetOptions.tabColorIndex;
				this.tabColorInfo = worksheetOptions.tabColorInfo;

				return;
			}

			// If the specified instance is a CustomViewDisplayOptions instance, copy the appropriate values
			CustomViewDisplayOptions customViewOptions = displayOptions as CustomViewDisplayOptions;

			if ( customViewOptions != null )
			{
				switch ( customViewOptions.View )
				{
					case WorksheetView.Normal:
						this.magnificationInNormalView = customViewOptions.MagnificationInCurrentView;
						break;

					case WorksheetView.PageBreakPreview:
						this.magnificationInPageBreakView = customViewOptions.MagnificationInCurrentView;
						break;

					case WorksheetView.PageLayout:
						this.magnificationInPageLayoutView = customViewOptions.MagnificationInCurrentView;
						break;

					default:
						Utilities.DebugFail( "Unknown view type" );
						break;
				}

				return;
			}

			Utilities.DebugFail( "Unknown display options" );
		}

		#endregion InitializeFrom

		#region Reset

		/// <summary>
		/// Resets the display options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public override void Reset()
		{
			base.Reset();

			this.magnificationInNormalView = DisplayOptions.DefaultMagnificationInNormalView;
			this.magnificationInPageBreakView = DisplayOptions.DefaultMagnificationInPageBreakView;
			this.magnificationInPageLayoutView = DisplayOptions.DefaultMagnificationInPageLayoutView;
			this.orderColumnsRightToLeft = false;
			this.showWhitespaceInPageLayoutView = true;

			// MD 1/26/12 - 12.1 - Cell Format Updates
			//this.tabColorIndex = WorkbookColorCollection.AutomaticColor;
			this.tabColorInfo = WorkbookColorInfo.Automatic;
		}

		#endregion Reset

		#endregion Base Class Overrides

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal override void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    Utilities.RemoveValueFromSortedList( this.tabColorIndex, unusedIndicies );
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

		#region Properties

		#region Public Properties

		#region MagnificationInNormalView

		/// <summary>
		/// Gets or sets the magnification level of the worksheet when it is displayed in normal view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Magnifications are stored as percentages of the normal viewing magnification. A value of 100 indicates normal magnification
		/// whereas a value of 200 indicates a zoom that is twice the normal viewing magnification.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of magnification levels for a worksheet. The level must be between 10 and 400.
		/// </exception>
		/// <value>The magnification level of the worksheet when it is displayed in normal view.</value>
		/// <seealso cref="MagnificationInPageBreakView"/>
		/// <seealso cref="MagnificationInPageLayoutView"/>
		/// <seealso cref="DisplayOptions.View"/>
		/// <seealso cref="CustomViewDisplayOptions.MagnificationInCurrentView"/>
		public int MagnificationInNormalView
		{
			get { return this.magnificationInNormalView; }
			set
			{
				if ( this.magnificationInNormalView != value )
				{
					if ( value < 10 || 400 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MagnificationLevel" ) );

					this.magnificationInNormalView = value;
				}
			}
		}

		#endregion MagnificationInNormalView

		#region MagnificationInPageBreakView

		/// <summary>
		/// Gets or sets the magnification level of the worksheet when it is displayed in the page break preview.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Magnifications are stored as percentages of the normal viewing magnification. A value of 100 indicates normal magnification
		/// whereas a value of 200 indicates a zoom that is twice the normal viewing magnification.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of magnification levels for a worksheet. The level must be between 10 and 400.
		/// </exception>
		/// <value>The magnification level of the worksheet when it is displayed in the page break preview.</value>
		/// <seealso cref="MagnificationInNormalView"/>
		/// <seealso cref="MagnificationInPageLayoutView"/>
		/// <seealso cref="DisplayOptions.View"/>
		/// <seealso cref="CustomViewDisplayOptions.MagnificationInCurrentView"/>
		public int MagnificationInPageBreakView
		{
			get { return this.magnificationInPageBreakView; }
			set
			{
				if ( this.magnificationInPageBreakView != value )
				{
					if ( value < 10 || 400 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MagnificationLevel" ) );

					this.magnificationInPageBreakView = value;
				}
			}
		}

		#endregion MagnificationInPageBreakView

		#region MagnificationInPageLayoutView

		/// <summary>
		/// Gets or sets the magnification level of the worksheet when it is displayed in page layout view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Magnifications are stored as percentages of the normal viewing magnification. A value of 100 indicates normal magnification
		/// whereas a value of 200 indicates a zoom that is twice the normal viewing magnification.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of magnification levels for a worksheet. The level must be between 10 and 400.
		/// </exception>
		/// <value>The magnification level of the worksheet when it is displayed in page layout view.</value>
		/// <seealso cref="MagnificationInNormalView"/>
		/// <seealso cref="MagnificationInPageBreakView"/>
		/// <seealso cref="DisplayOptions.View"/>
		/// <seealso cref="CustomViewDisplayOptions.MagnificationInCurrentView"/>
		public int MagnificationInPageLayoutView
		{
			get { return this.magnificationInPageLayoutView; }
			set
			{
				if ( this.magnificationInPageLayoutView != value )
				{
					if ( value < 10 || 400 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MagnificationLevel" ) );

					this.magnificationInPageLayoutView = value;
				}
			}
		}

		#endregion MagnificationInPageLayoutView

		#region OrderColumnsRightToLeft

		/// <summary>
		/// Gets or sets the value indicating whether the columns are ordered right to left.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the columns are ordered right to left, other aspects of the worksheet display differently. The vertical scrollbar
		/// will display on the left side of the worksheet and the worksheet tab bar, usually displayed on the left side of the
		/// worksheet, will display on the right side.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether the columns are ordered right to left.</value>
		public bool OrderColumnsRightToLeft
		{
			get { return this.orderColumnsRightToLeft; }
			set { this.orderColumnsRightToLeft = value; }
		}

		#endregion OrderColumnsRightToLeft

		#region ShowWhitespaceInPageLayoutView

		/// <summary>
		/// Gets or sets the value which indicates whether to show whitespace between worksheet pages in page layout view.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> For this value to affect the display of the worksheet, the <see cref="DisplayOptions.View"/> must 
		/// be set to PageLayout. However, if a different view is used, this is still saved with the workbook.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> The <see cref="WorksheetView"/> value of PageLayout is only supported in Excel 2007. 
		/// If a worksheet with that View is viewed in earlier versions of Microsoft Excel, the view will default to Normal view.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether to show white page between worksheet pages in page layout view.</value>
		/// <seealso cref="DisplayOptions.View"/>
		public bool ShowWhitespaceInPageLayoutView
		{
			get { return this.showWhitespaceInPageLayoutView; }
			set { this.showWhitespaceInPageLayoutView = value; }
		}

		#endregion ShowWhitespaceInPageLayoutView

		#region TabColor

		// MD 1/26/12 - 12.1 - Cell Format Updates
		#region Old Code

		///// <summary>
		///// Gets or sets the color to use for the associated worksheet's tab in the tab bar of Microsoft Excel.
		///// </summary>
		///// <remarks>
		///// <p class="body">
		///// If the tab bar is not visible, this color will not be seen.
		///// </p>
		///// </remarks>
		///// <exception cref="InvalidOperationException">
		///// The workbook is using a custom palette and setting this color would cause the custom palette to use 
		///// too many colors.
		///// </exception>
		///// <value>The color to use for the associated worksheet's tab in the tab bar of Microsoft Excel.</value>
		///// <seealso cref="WindowOptions.TabBarVisible"/>
		//public Color TabColor
		//{
		//    // MD 1/16/12 - 12.1 - Cell Format Updates
		//    //get { return this.Worksheet.Workbook.Palette[ this.tabColorIndex ]; }
		//    get { return this.Worksheet.Workbook.Palette.GetColorAtAbsoluteIndex(this.tabColorIndex); }
		//    set
		//    {
		//        if ( this.TabColor != value )
		//        {
		//            if (Utilities.ColorIsEmpty(value))
		//            {
		//                // MD 1/16/12 - 12.1 - Cell Format Updates
		//                //this.tabColorIndex = WorkbookColorCollection.AutomaticColor;
		//                this.tabColorIndex = WorkbookColorPalette.AutomaticColor;
		//            }
		//            else
		//            {
		//                // MD 8/24/07 - BR25848
		//                // Another parameter is now needed for the Add method
		//                //this.tabColorIndex = this.Worksheet.Workbook.Palette.Add( value );
		//                // MD 8/30/07 - BR26111
		//                // Changed parameter to provide more info about the item getting a color
		//                //this.tabColorIndex = this.Worksheet.Workbook.Palette.Add( value, false );
		//                // MD 1/16/12 - 12.1 - Cell Format Updates
		//                //this.tabColorIndex = this.Worksheet.Workbook.Palette.Add( value, ColorableItem.WorksheetTab );
		//                this.tabColorIndex = this.Worksheet.Workbook.Palette.FindIndex(value, ColorableItem.WorksheetTab);
		//            }
		//        }
		//    }
		//}

		#endregion // Old Code
		/// <summary>
		/// Obsolete. Use <see cref="TabColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("The WorksheetDisplayOptions.TabColor is deprecated. It has been replaced by WorksheetDisplayOptions.TabColorInfo.")]
		public Color TabColor
		{
			get 
			{
				if (this.TabColorInfo == null)
					return Utilities.ColorEmpty;

				return this.TabColorInfo.GetResolvedColor(this.Worksheet.Workbook); 
			}
			set
			{
				if (Utilities.ColorIsEmpty(value))
					this.TabColorInfo = null;
				else
					this.TabColorInfo = new WorkbookColorInfo(value);
			}
		}

		#endregion TabColor

		// MD 1/27/12 - 12.1 - Cell Format Updates
		#region TabColorInfo

		/// <summary>
		/// Gets or sets the <see cref="WorkbookColorInfo"/> to use for the associated worksheet's tab in the tab bar of Microsoft Excel.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the tab bar is not visible, this color will not be seen.
		/// </p>
		/// </remarks>
		/// <value>The WorkbookColorInfo to use for the associated worksheet's tab in the tab bar of Microsoft Excel.</value>
		/// <seealso cref="WindowOptions.TabBarVisible"/>
		public WorkbookColorInfo TabColorInfo
		{
			get { return this.tabColorInfo; }
			set { this.tabColorInfo = value; }
		}

		#endregion TabColorInfo

		#endregion Public Properties

		#region Internal Properties

		// MD 7/23/12 - TFS117431
		#region CurrentMagnificationLevel

		internal int CurrentMagnificationLevel
		{
			get
			{
				switch (this.View)
				{
					case WorksheetView.Normal:
						return this.MagnificationInNormalView;

					case WorksheetView.PageBreakPreview:
						return this.MagnificationInPageBreakView;

					case WorksheetView.PageLayout:
						return this.MagnificationInPageLayoutView;

					default:
						Utilities.DebugFail("Unknown WorksheetView: " + this.View);
						goto case WorksheetView.Normal;
				}
			}
			set
			{
				switch (this.View)
				{
					case WorksheetView.Normal:
						this.MagnificationInNormalView = value;
						break;

					case WorksheetView.PageBreakPreview:
						this.MagnificationInPageBreakView = value;
						break;

					case WorksheetView.PageLayout:
						this.MagnificationInPageLayoutView = value;
						break;

					default:
						Utilities.DebugFail("Unknown WorksheetView: " + this.View);
						goto case WorksheetView.Normal;
				}
			}
		}

		#endregion // CurrentMagnificationLevel

		// MD 1/26/12 - 12.1 - Cell Format Updates
		//#region TabColorIndex

		//internal int TabColorIndex
		//{
		//    get { return this.tabColorIndex; }
		//    set { this.tabColorIndex = value; }
		//}

		//#endregion TabColorIndex

		#endregion Internal Properties

		#endregion Properties
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