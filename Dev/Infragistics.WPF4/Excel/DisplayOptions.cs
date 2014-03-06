using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;





using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class which exposes the various display options available for a worksheet which can be 
	/// saved with both a worksheet and a custom view.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This class provides a way to control how a worksheet is displayed when it is viewed in Microsoft Excel.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomViewDisplayOptions"/>
	/// <seealso cref="WorksheetDisplayOptions"/>



	public

		 abstract class DisplayOptions
	{
		#region Constants

		internal const int DefaultMagnificationInPageBreakView = 60;
		internal const int DefaultMagnificationInPageLayoutView = 100;
		internal const int DefaultMagnificationInNormalView = 100;

		#endregion Constants

		#region Member Variables

		private Worksheet worksheet;

		private FrozenPaneSettings frozenPaneSettings;
		private bool useAutomaticGridlineColor = true;

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//private int gridlineColorIndex = WorkbookColorCollection.AutomaticColor;
		private int gridlineColorIndex = WorkbookColorPalette.AutomaticColor;

		private bool panesAreFrozen;

		// MD 6/4/10 - ChildRecordsDisplayOrder feature
		private ExcelDefaultableBoolean showExpansionIndicatorBelowGroupedRows;
		private ExcelDefaultableBoolean showExpansionIndicatorToRightOfGroupedColumns;

		private bool showFormulasInCells;
		private bool showGridlines = true;
		private bool showOutlineSymbols = true;
		private bool showRowAndColumnHeaders = true;
		private bool showRulerInPageLayoutView = true;
		private bool showZeroValues = true;
		private UnfrozenPaneSettings unfrozenPaneSettings;
		private WorksheetView view = WorksheetView.Normal;
		private WorksheetVisibility visibility = WorksheetVisibility.Visible;

		#endregion Member Variables

		#region Constructors

		internal DisplayOptions( Worksheet worksheet ) 
		{
			this.worksheet = worksheet;
		}

		#endregion Constructors

		#region Methods

		#region Public Methods

		#region Reset

		/// <summary>
		/// Resets the display options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public virtual void Reset()
		{
			// MD 6/24/08 - BR34172
			// The frozenPaneSettings member could be null
			//this.frozenPaneSettings.Reset();
			if ( this.frozenPaneSettings != null )
				this.frozenPaneSettings.Reset();

			this.useAutomaticGridlineColor = true;

			// MD 1/16/12 - 12.1 - Cell Format Updates
			//this.gridlineColorIndex = WorkbookColorCollection.AutomaticColor;
			this.gridlineColorIndex = WorkbookColorPalette.AutomaticColor;

			this.panesAreFrozen = false;
			this.showFormulasInCells = false;
			this.showGridlines = true;
			this.showOutlineSymbols = true;
			this.showRowAndColumnHeaders = true;
			this.showRulerInPageLayoutView = true;
			this.showZeroValues = true;

			// MD 6/24/08 - BR34172
			// The unfrozenPaneSettings member could be null
			//this.unfrozenPaneSettings.Reset();
			if ( this.unfrozenPaneSettings != null )
				this.unfrozenPaneSettings.Reset();

			this.view = WorksheetView.Normal;
			this.visibility = WorksheetVisibility.Visible;
		}

		#endregion Reset

		#endregion Public Methods

		#region Internal Methods

		#region InitializeFrom






		internal virtual void InitializeFrom( DisplayOptions displayOptions )
		{
			this.FrozenPaneSettings.InitializeFrom( displayOptions.FrozenPaneSettings );
			this.useAutomaticGridlineColor = displayOptions.useAutomaticGridlineColor;
			this.gridlineColorIndex = displayOptions.gridlineColorIndex;
			this.panesAreFrozen = displayOptions.panesAreFrozen;
			this.showFormulasInCells = displayOptions.showFormulasInCells;
			this.showGridlines = displayOptions.showGridlines;
			this.showOutlineSymbols = displayOptions.showOutlineSymbols;
			this.showRowAndColumnHeaders = displayOptions.showRowAndColumnHeaders;
			this.showRulerInPageLayoutView = displayOptions.showRulerInPageLayoutView;
			this.showZeroValues = displayOptions.showZeroValues;
			this.UnfrozenPaneSettings.InitializeFrom( displayOptions.UnfrozenPaneSettings );
			this.view = displayOptions.view;
			this.visibility = displayOptions.visibility;
		}

		#endregion InitializeFrom

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal virtual void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    Utilities.RemoveValueFromSortedList( this.gridlineColorIndex, unusedIndicies );
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

		#endregion Internal Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region FrozenPaneSettings

		/// <summary>
		/// Gets the settings which control the frozen panes in the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// For these settings to be saved in the workbook file, <see cref="PanesAreFrozen"/> must be True.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> Frozen and unfrozen panes cannot be used simultaneously, so depending on whether the panes are 
		/// frozen or unfrozen, these settings may not be used.
		/// </p>
		/// </remarks>
		/// <value>The settings which control the frozen panes in the worksheet..</value>
		/// <seealso cref="DisplayOptions.PanesAreFrozen"/>
		/// <seealso cref="DisplayOptions.UnfrozenPaneSettings"/>
		public FrozenPaneSettings FrozenPaneSettings
		{
			get
			{
				if ( this.frozenPaneSettings == null )
				{
					// MD 6/31/08 - Excel 2007 Format
					//this.frozenPaneSettings = new FrozenPaneSettings();
					this.frozenPaneSettings = new FrozenPaneSettings( this );
				}

				return this.frozenPaneSettings;
			}
		}

		#endregion FrozenPaneSettings

		#region GridlineColor

		/// <summary>
		/// Gets or sets the color of the gridlines on the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the workbook is using a standard palette, the color set may be changed if it is not in the palette.
		/// In this case, the closest color in the standard palette will be used.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The workbook is using a custom palette and setting this color would cause the custom palette to use 
		/// too many colors.
		/// </exception>
		/// <value>The color of the gridlines on the worksheet.</value>
		public Color GridlineColor
		{
			// MD 1/16/12 - 12.1 - Cell Format Updates
			//get { return this.worksheet.Workbook.Palette[ this.gridlineColorIndex ]; }
			get { return this.worksheet.Workbook.Palette.GetColorAtAbsoluteIndex(this.gridlineColorIndex); }
			set
			{
				if ( this.GridlineColor != value )
				{
					if ( Utilities.ColorIsEmpty(value) )
					{
						// MD 1/16/12 - 12.1 - Cell Format Updates
						//this.gridlineColorIndex = WorkbookColorCollection.AutomaticColor;
						this.gridlineColorIndex = WorkbookColorPalette.AutomaticColor;

						this.useAutomaticGridlineColor = true;
					}
					else
					{
						// MD 8/24/07 - BR25848
						// Another parameter is now needed for the Add method
						//this.gridlineColorIndex = this.worksheet.Workbook.Palette.Add( value );
						// MD 8/30/07 - BR26111
						// Changed parameter to provide more info about the item getting a color
						//this.gridlineColorIndex = this.worksheet.Workbook.Palette.Add( value, false );
						// MD 1/16/12 - 12.1 - Cell Format Updates
						//this.gridlineColorIndex = this.worksheet.Workbook.Palette.Add( value, ColorableItem.WorksheetGrid );
						this.gridlineColorIndex = this.worksheet.Workbook.Palette.FindIndex(value, ColorableItem.WorksheetGrid);

						this.useAutomaticGridlineColor = false;
					}
				}
			}
		}

		#endregion GridlineColor

		#region PanesAreFrozen

		/// <summary>
		/// Gets or sets the value which indicates if the panes in the worksheet are frozen.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Depending on the value of this property, either the <see cref="FrozenPaneSettings"/> or the
		/// <see cref="UnfrozenPaneSettings"/> will be used for the worksheet. The unused settings are 
		/// ignored and are not saved with the workbook stream.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates if the panes in the worksheet are frozen.</value>
		/// <seealso cref="FrozenPaneSettings"/>
		/// <seealso cref="UnfrozenPaneSettings"/>
		public bool PanesAreFrozen
		{
			get { return this.panesAreFrozen; }
			set { this.panesAreFrozen = value; }
		}

		#endregion PanesAreFrozen

		// MD 6/4/10 - ChildRecordsDisplayOrder feature
		#region ShowExpansionIndicatorBelowGroupedRows

		/// <summary>
		/// Gets or sets the value which indicates whether the expansion indicators should be shown below grouped, 
		/// or indented rows.
		/// </summary>
		public ExcelDefaultableBoolean ShowExpansionIndicatorBelowGroupedRows
		{
			get { return this.showExpansionIndicatorBelowGroupedRows; }
			set { this.showExpansionIndicatorBelowGroupedRows = value; }
		} 

		#endregion // ShowExpansionIndicatorBelowGroupedRows

		// MD 6/4/10 - ChildRecordsDisplayOrder feature
		#region ShowExpansionIndicatorToRightOfGroupedColumns

		/// <summary>
		/// Gets or sets the value which indicates whether the expansion indicators should be shown to the right of 
		/// grouped, or indented rows.
		/// </summary>
		public ExcelDefaultableBoolean ShowExpansionIndicatorToRightOfGroupedColumns
		{
			get { return this.showExpansionIndicatorToRightOfGroupedColumns; }
			set { this.showExpansionIndicatorToRightOfGroupedColumns = value; }
		} 

		#endregion // ShowExpansionIndicatorToRightOfGroupedColumns

		#region ShowFormulasInCells

		/// <summary>
		/// Gets or sets the value which indicates whether formulas are shown in cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this value is True, the formula string will be displayed in the cell. If the value is
		/// False, the result of the formula will be displayed in the cell.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether formulas are shown in cells.</value>
		public bool ShowFormulasInCells
		{
			get { return this.showFormulasInCells; }
			set { this.showFormulasInCells = value; }
		}

		#endregion ShowFormulasInCells

		#region ShowGridlines

		/// <summary>
		/// Gets or sets the value which indicates whether gridlines are shown between cells.
		/// </summary>
		/// <value>The value which indicates whether gridlines are shown between cells.</value>
		/// <seealso cref="PrintOptions.PrintGridlines"/>
		public bool ShowGridlines
		{
			get { return this.showGridlines; }
			set { this.showGridlines = value; }
		}

		#endregion ShowGridlines

		#region ShowOutlineSymbols

		/// <summary>
		/// Gets or sets the value which indicates whether outline symbols are shown for outlined columns and rows.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the outline symbols are displayed, they provide a visual representation of the outline levels or rows 
		/// and columns in Microsoft Excel.  In addition, the outline symbols include the expansion indicators which
		/// allow for the expanding and collapsing of outline groups.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether outline symbols are shown for outlined columns and rows.</value>
		/// <seealso cref="RowColumnBase.OutlineLevel"/>
		public bool ShowOutlineSymbols
		{
			get { return this.showOutlineSymbols; }
			set { this.showOutlineSymbols = value; }
		}

		#endregion ShowOutlineSymbols

		#region ShowRowAndColumnHeaders

		/// <summary>
		/// Gets or sets the value which indicates whether to display row and column headers.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The row and column headers show the identifier of the row or column. They also allow the user to easily select
		/// all cells in a row or column by clicking them.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether to display row and column headers.</value>
		/// <seealso cref="PrintOptions.PrintRowAndColumnHeaders"/>
		public bool ShowRowAndColumnHeaders
		{
			get { return this.showRowAndColumnHeaders; }
			set { this.showRowAndColumnHeaders = value; }
		}

		#endregion ShowRowAndColumnHeaders

		#region ShowRulerInPageLayoutView

		/// <summary>
		/// Gets or sets the value which indicates whether to show rulers in the page layout view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When this value is True, one ruler will display above the column headers of the active page
		/// in page layout view. Another ruler will also display before the row headers of the active page.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> This property will only affect the worksheet view if the <see cref="View"/> is 
		/// PageLayout.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> The <see cref="WorksheetView"/> value of PageLayout is only supported in Excel 2007. 
		/// If a worksheet with that View is viewed in earlier versions of Microsoft Excel, the view will 
		/// default to Normal view.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether to show rulers in the page layout view.</value>
		/// <seealso cref="View"/>
		public bool ShowRulerInPageLayoutView
		{
			get { return this.showRulerInPageLayoutView; }
			set { this.showRulerInPageLayoutView = value; }
		}

		#endregion ShowRulerInPageLayoutView

		#region ShowZeroValues

		/// <summary>
		/// Gets or sets the value which indicates whether zero values are shown in cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this value is True, cells with a value of zero will display their values; otherwise,
		/// those cells will display as blanks.
		/// </p>
		/// </remarks>
		/// <value>The value which indicates whether zero values are shown in cells.</value>
		public bool ShowZeroValues
		{
			get { return this.showZeroValues; }
			set { this.showZeroValues = value; }
		}

		#endregion ShowZeroValues

		#region UnfrozenPaneSettings

		/// <summary>
		/// Gets the settings which control the unfrozen panes in the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// For these settings to be saved in the workbook file, <see cref="PanesAreFrozen"/> must be False.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> Frozen and unfrozen panes cannot be used simultaneously, so depending on whether the panes are 
		/// frozen or unfrozen, these settings may not be used.
		/// </p>
		/// </remarks>
		/// <value>The settings which control the unfrozen panes in the worksheet.</value>
		/// <seealso cref="DisplayOptions.PanesAreFrozen"/>
		/// <seealso cref="DisplayOptions.FrozenPaneSettings"/>
		public UnfrozenPaneSettings UnfrozenPaneSettings
		{
			get
			{
				if ( this.unfrozenPaneSettings == null )
				{
					// MD 6/31/08 - Excel 2007 Format
					//this.unfrozenPaneSettings = new UnfrozenPaneSettings();
					this.unfrozenPaneSettings = new UnfrozenPaneSettings( this );
				}

				return this.unfrozenPaneSettings;
			}
		}

		#endregion UnfrozenPaneSettings 

		#region View

		/// <summary>
		/// Gets or sets the current view of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The view determines the overall display of the worksheet in Microsoft Excel.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> The <see cref="WorksheetView"/> value of PageLayout is only supported in Excel 2007. 
		/// If a worksheet with that View is viewed in earlier versions of Microsoft Excel, the view will 
		/// default to Normal view.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the WorksheetView enumeration.
		/// </exception>
		/// <value>The current view of the worksheet.</value>
		/// <seealso cref="CustomViewDisplayOptions.MagnificationInCurrentView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInNormalView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageBreakView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageLayoutView"/>
		public WorksheetView View
		{
			get { return this.view; }
			set
			{
				if ( this.view != value )
				{
					if ( Enum.IsDefined( typeof( WorksheetView ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( WorksheetView ) );

					this.view = value;
				}
			}
		}

		#endregion View

		#region Visibility

		/// <summary>
		/// Gets or sets the visibility of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The visibility determines whether the worksheet's tab will appear in the tab bar at the
		/// bottom of Microsoft Excel. 
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="WorksheetVisibility"/> enumeration.
		/// </exception>
		/// <value>The visibility of the worksheet.</value>
		public WorksheetVisibility Visibility
		{
			get { return this.visibility; }
			set
			{
				if ( this.visibility != value )
				{
					if ( Enum.IsDefined( typeof( WorksheetVisibility ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( WorksheetVisibility ) );

					this.visibility = value;
				}
			}
		}

		#endregion Visibility

		#endregion Public Properties

		#region Internal Properties

		#region GridlineColorIndex

		internal int GridlineColorIndex
		{
			get { return this.gridlineColorIndex; }
			set
			{
				if ( this.useAutomaticGridlineColor )
				{
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//Debug.Assert( value == WorkbookColorCollection.AutomaticColor );
					Debug.Assert(value == WorkbookColorPalette.AutomaticColor);
				}

				this.gridlineColorIndex = value;
			}
		}

		#endregion GridlineColorIndex

		// MD 6/7/10 - ChildRecordsDisplayOrder feature
		#region ShowExpansionIndicatorBelowGroupedRowsResolved

		internal bool ShowExpansionIndicatorBelowGroupedRowsResolved
		{
			get
			{
				if (this.showExpansionIndicatorBelowGroupedRows != ExcelDefaultableBoolean.Default)
					return showExpansionIndicatorBelowGroupedRows == ExcelDefaultableBoolean.True;

				return this.Worksheet.ShowExpansionIndicatorBelowGroup;
			}
		}

		#endregion // ShowExpansionIndicatorBelowGroupedRowsResolved

		// MD 6/7/10 - ChildRecordsDisplayOrder feature
		#region ShowExpansionIndicatorToRightOfGroupedColumnsResolved

		internal bool ShowExpansionIndicatorToRightOfGroupedColumnsResolved
		{
			get
			{
				if (this.showExpansionIndicatorToRightOfGroupedColumns != ExcelDefaultableBoolean.Default)
					return this.showExpansionIndicatorToRightOfGroupedColumns == ExcelDefaultableBoolean.True;

				return true;
			}
		}

		#endregion // ShowExpansionIndicatorToRightOfGroupedColumnsResolved

		#region UseAutomaticGridlineColor

		internal bool UseAutomaticGridlineColor
		{
			get { return this.useAutomaticGridlineColor; }
			set { this.useAutomaticGridlineColor = value; }
		}

		#endregion UseAutomaticGridlineColor

		#region Worksheet

		internal Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

		#endregion Internal Properties

		#endregion Properties

        #region Methods

        //  BF 8/11/08  Excel2007 Format
        #region ShouldSerializeFrozenPaneSettings
        internal bool ShouldSerializeFrozenPaneSettings()
        { 
            return  this.panesAreFrozen &&
                    this.frozenPaneSettings != null &&
                    this.frozenPaneSettings.ShouldSerialize();
        }
        #endregion ShouldSerializeFrozenPaneSettings

        //  BF 8/11/08  Excel2007 Format
        #region ShouldSerializeUnfrozenPaneSettings
        internal bool ShouldSerializeUnfrozenPaneSettings()
        { 
            return  this.panesAreFrozen == false &&
                    this.unfrozenPaneSettings != null &&
                    this.unfrozenPaneSettings.ShouldSerialize();
        }
        #endregion ShouldSerializeUnfrozenPaneSettings

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