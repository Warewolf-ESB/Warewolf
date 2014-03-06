using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a custom view in Microsoft Excel.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Custom views provide a way to save display options and optionally print options for the workbook and each worksheet in the workbook.
	/// These options can be different from the options currently set on the workbook and worksheets.
	/// </p>
	/// <p class="body">
	/// Multiple custom views can be saved with a workbook, and the options from a custom view can be applied to its associated workbook by 
	/// calling the <see cref="Apply"/> method on it.
	/// </p>
	/// </remarks>
	/// <seealso cref="Infragistics.Documents.Excel.Workbook.CustomViews"/>
	[DebuggerDisplay( "CustomView: {name}" )]



	public

		 class CustomView
	{
		#region Member Variables

		private Workbook workbook;

		private Guid id;
		private bool savePrintOptions;
		private bool saveHiddenRowsAndColumns;
		private string name;
		private CustomViewWindowOptions windowOptions;

		private Dictionary<Worksheet, CustomViewDisplayOptions> worksheetDisplayOptions;
		private Dictionary<Worksheet, HiddenColumnCollection> worksheetHiddenColumns;
		private Dictionary<Worksheet, HiddenRowCollection> worksheetHiddenRows;
		private Dictionary<Worksheet, PrintOptions> worksheetPrintOptions;

		// MD 4/6/12 - TFS102169
		// We do not need to store these in this way anymore. Now that the workbook part is loaded before the
		// worksheet part, we can set the options directly on the custom view.
		////  BF 8/8/08   Excel2007 Format
		//private Dictionary<Worksheet, CustomViewDisplayOptions> pendingWorksheetCustomViewDisplayOptions;
		//private Dictionary<Worksheet, PrintOptions> pendingWorksheetPrintOptions;

		#endregion Member Variables

		#region Constructor

		internal CustomView( Workbook workbook, bool savePrintOptions, bool saveHiddenRowsAndColumns )
		{
			this.workbook = workbook;

			this.savePrintOptions = savePrintOptions;
			this.saveHiddenRowsAndColumns = saveHiddenRowsAndColumns;

			// Create the options collections that will be needed
			this.worksheetDisplayOptions = new Dictionary<Worksheet, CustomViewDisplayOptions>();

			if ( this.savePrintOptions )
				this.worksheetPrintOptions = new Dictionary<Worksheet, PrintOptions>();

			if ( this.saveHiddenRowsAndColumns )
			{
				this.worksheetHiddenColumns = new Dictionary<Worksheet, HiddenColumnCollection>();
				this.worksheetHiddenRows = new Dictionary<Worksheet, HiddenRowCollection>();
			}

			// Initialize the window options from the workbook's current state
			this.windowOptions = new CustomViewWindowOptions( this );
			this.windowOptions.InitializeFrom( workbook.WindowOptions );

			// Initialize the worksheet options from each worksheet's current state
			foreach ( Worksheet worksheet in workbook.Worksheets )
				this.OnWorksheetAdded( worksheet );
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region Apply

		/// <summary>
		/// Applies all options from the custom view to the associated workbook and its worksheets.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// There is no state of the workbook indicating the custom view currently applied, so applying a custom view
		/// simply copies over all options saved with it to the workbook and its worksheet. If an applied custom view
		/// is then changed, those changes will not be updated on the workbook or worksheets. Instead, the custom view will need
		/// to be applied again for those changes to be reflected on the workbook or worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The custom view has previously been removed from its associated workbook.
		/// </exception>
		public void Apply()
		{
			if ( this.workbook == null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantApplyRemovedCustomView" ) );

			// Copy over the workbook options
			this.workbook.WindowOptions.InitializeFrom( this.WindowOptions );

			// Copy over the display options for each worksheet
			foreach ( KeyValuePair<Worksheet, CustomViewDisplayOptions> displayOptionPair in this.worksheetDisplayOptions )
				displayOptionPair.Key.DisplayOptions.InitializeFrom( displayOptionPair.Value );

			if ( this.savePrintOptions )
			{
				// Copy over the print options for each worksheet
				foreach ( KeyValuePair<Worksheet, PrintOptions> printOptionPair in this.worksheetPrintOptions )
					printOptionPair.Key.PrintOptions.InitializeFrom( printOptionPair.Value );
			}

			if ( this.saveHiddenRowsAndColumns )
			{
				// Copy over the hidden columns for each worksheet
				foreach ( KeyValuePair<Worksheet, HiddenColumnCollection> hiddenColumnsPair in this.worksheetHiddenColumns )
					hiddenColumnsPair.Key.Columns.InitializeFrom( hiddenColumnsPair.Value );

				// Copy over the hidden rows for each worksheet
				foreach ( KeyValuePair<Worksheet, HiddenRowCollection> hiddenRowsPair in this.worksheetHiddenRows )
					hiddenRowsPair.Key.Rows.InitializeFrom( hiddenRowsPair.Value );
			}
		}

		#endregion Apply

		#region GetDisplayOptions

		/// <summary>
		/// Gets the display options associated with the specified worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Setting properties on the returned <see cref="DisplayOptions"/> instance will not change the actual
		/// display of the worksheet. After setting properties, the <see cref="Apply"/> method of the 
		/// <see cref="CustomView"/> will apply them to the worksheet.
		/// </p>
		/// </remarks>
		/// <param name="worksheet">The worksheet whose associated display options are to be retrieved.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <returns>
		/// Null if the worksheet does not belong to the workbook associated with this custom view; 
		/// otherwise, the display options associated with the worksheet.
		/// </returns>
		/// <seealso cref="Worksheet.DisplayOptions"/>
		public CustomViewDisplayOptions GetDisplayOptions( Worksheet worksheet )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			CustomViewDisplayOptions displayOptions;

			if ( this.worksheetDisplayOptions.TryGetValue( worksheet, out displayOptions ) )
				return displayOptions;

			return null;
		}

		#endregion GetDisplayOptions

		#region GetHiddenColumns

		/// <summary>
		/// Gets the hidden columns associated with the specified worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Adding columns on the returned <see cref="HiddenColumnCollection"/> instance will not actually hide 
		/// columns in the worksheet. After modifying the hidden columns in this collection, the <see cref="Apply"/> 
		/// method of the <see cref="CustomView"/> will hide or unhide the columns.
		/// </p>
		/// </remarks>
		/// <param name="worksheet">The worksheet whose associated hidden columns are to be retrieved.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <returns>
		/// Null if <see cref="SaveHiddenRowsAndColumns"/> is False or if the worksheet does not belong to the workbook 
		/// associated with this custom view; otherwise, the collection of hidden columns associated with the worksheet.
		/// </returns>
		/// <seealso cref="RowColumnBase.Hidden"/>
		/// <seealso cref="SaveHiddenRowsAndColumns"/>
		public HiddenColumnCollection GetHiddenColumns( Worksheet worksheet )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			if ( this.saveHiddenRowsAndColumns == false )
				return null;

			HiddenColumnCollection hiddenColumns;

			if ( this.worksheetHiddenColumns.TryGetValue( worksheet, out hiddenColumns ) )
				return hiddenColumns;

			return null;
		}

		#endregion GetHiddenColumns

		#region GetHiddenRows

		/// <summary>
		/// Gets the hidden rows associated with the specified worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Adding rows on the returned <see cref="HiddenRowCollection"/> instance will not actually hide 
		/// rows in the worksheet. After modifying the hidden rows in this collection, the <see cref="Apply"/> 
		/// method of the <see cref="CustomView"/> will hide or unhide the rows.
		/// </p>
		/// </remarks>
		/// <param name="worksheet">The worksheet whose associated hidden rows are to be retrieved.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <returns>
		/// Null if <see cref="SaveHiddenRowsAndColumns"/> is False or if the worksheet does not belong to the workbook 
		/// associated with this custom view; otherwise, the collection of hidden rows associated with the worksheet.
		/// </returns>
		/// <seealso cref="RowColumnBase.Hidden"/>
		/// <seealso cref="SaveHiddenRowsAndColumns"/>
		public HiddenRowCollection GetHiddenRows( Worksheet worksheet )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			if ( this.saveHiddenRowsAndColumns == false )
				return null;

			HiddenRowCollection hiddenRows;

			if ( this.worksheetHiddenRows.TryGetValue( worksheet, out hiddenRows ) )
				return hiddenRows;

			return null;
		}

		#endregion GetHiddenRows

		#region GetPrintOptions

		/// <summary>
		/// Gets the print options associated with the specified worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Setting properties on the returned <see cref="PrintOptions"/> instance will not change the actual
		/// print settings of the worksheet. After setting properties, the <see cref="Apply"/> method of the 
		/// <see cref="CustomView"/> will apply them to the worksheet.
		/// </p>
		/// </remarks>
		/// <param name="worksheet">The worksheet whose associated print options are to be retrieved.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <returns>
		/// Null if <see cref="SavePrintOptions"/> is False or if the worksheet does not belong to the workbook associated
		/// with this custom view; otherwise, the print options associated with the worksheet.
		/// </returns>
		/// <seealso cref="Worksheet.PrintOptions"/>
		/// <seealso cref="SavePrintOptions"/>
		public PrintOptions GetPrintOptions( Worksheet worksheet )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			if ( this.savePrintOptions == false )
				return null;

			PrintOptions printOptions;

			if ( this.worksheetPrintOptions.TryGetValue( worksheet, out printOptions ) )
				return printOptions;

			return null;
		}

		#endregion GetPrintOptions

		#endregion Public Methods

		#region Internal Methods

		#region OnRemovedFromCollection

		internal void OnRemovedFromCollection()
		{
			this.workbook = null;

			this.worksheetDisplayOptions.Clear();

			if ( this.savePrintOptions )
				this.worksheetPrintOptions.Clear();

			if ( this.saveHiddenRowsAndColumns )
			{
				this.worksheetHiddenColumns.Clear();
				this.worksheetHiddenRows.Clear();
			}
		}

		#endregion OnRemovedFromCollection

		#region OnWorksheetAdded

        internal void OnWorksheetAdded( Worksheet worksheet )
        {
            CustomViewDisplayOptions displayOptions = new CustomViewDisplayOptions( worksheet );
            displayOptions.InitializeFrom( worksheet.DisplayOptions );

			// MD 4/6/12 - TFS102169
			// We do not need to store these in this way anymore. Now that the workbook part is loaded before the
			// worksheet part, we can set the options directly on the custom view.
			////  BF 8/8/08   Excel2007Format
			////  Since CustomSheetViews can be (are) serialized in before the asociated
			////  Worksheet, and because we initialize these instances from the worksheet's
			////  DisplayOptions, we need to apply the CustomViewDisplayOptions settings after
			////  the worksheet's DisplayOptions.
			//if ( this.pendingWorksheetCustomViewDisplayOptions != null &&
			//     this.pendingWorksheetCustomViewDisplayOptions.ContainsKey(worksheet) )
			//{
			//    CustomViewDisplayOptions customOptions = this.pendingWorksheetCustomViewDisplayOptions[worksheet];
			//    displayOptions.InitializeFrom( customOptions );
			//    this.pendingWorksheetCustomViewDisplayOptions.Remove( worksheet );
			//}

            this.worksheetDisplayOptions.Add( worksheet, displayOptions );

            if ( this.savePrintOptions )
            {
                // MD 8/1/08 - BR35121
                // The PrintOptions constructor now takes more arguments.
                //PrintOptions printOptions = new PrintOptions();
                PrintOptions printOptions = new PrintOptions( worksheet );

                printOptions.InitializeFrom( worksheet.PrintOptions );

				// MD 4/6/12 - TFS102169
				// We do not need to store these in this way anymore. Now that the workbook part is loaded before the
				// worksheet part, we can set the options directly on the custom view.
				////  BF 8/8/08   Excel2007Format (see above)
				//if ( this.pendingWorksheetPrintOptions != null &&
				//     this.pendingWorksheetPrintOptions.ContainsKey(worksheet) )
				//{
				//    PrintOptions customPrintOptions = this.pendingWorksheetPrintOptions[worksheet];
				//    printOptions.InitializeFrom( customPrintOptions );
				//    this.pendingWorksheetPrintOptions.Remove( worksheet );
				//}

                this.worksheetPrintOptions.Add( worksheet, printOptions );
            }

            if ( this.saveHiddenRowsAndColumns )
            {
                HiddenColumnCollection hiddenColumns = new HiddenColumnCollection( worksheet, this );
                hiddenColumns.InitializeFrom( worksheet );
                this.worksheetHiddenColumns.Add( worksheet, hiddenColumns );

                HiddenRowCollection hiddenRows = new HiddenRowCollection( worksheet, this );
                hiddenRows.InitializeFrom( worksheet );
                this.worksheetHiddenRows.Add( worksheet, hiddenRows );
            }
        }

        #endregion OnWorksheetAdded


        #region OnWorksheetRemoved

        internal void OnWorksheetRemoved( Worksheet worksheet )
		{
			if ( this.WindowOptions.SelectedWorksheet == worksheet )
				this.WindowOptions.SelectedWorksheet = null;

			Debug.Assert( this.worksheetDisplayOptions.ContainsKey( worksheet ) );
			this.worksheetDisplayOptions.Remove( worksheet );

			if ( this.savePrintOptions )
			{
				Debug.Assert( this.worksheetPrintOptions.ContainsKey( worksheet ) );
				this.worksheetPrintOptions.Remove( worksheet );
			}

			if ( this.saveHiddenRowsAndColumns )
			{
				Debug.Assert( this.worksheetHiddenColumns.ContainsKey( worksheet ) );
				this.worksheetHiddenColumns.Remove( worksheet );

				Debug.Assert( this.worksheetHiddenRows.ContainsKey( worksheet ) );
				this.worksheetHiddenRows.Remove( worksheet );
			}
		}

		#endregion OnWorksheetRemoved

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    foreach ( CustomViewDisplayOptions displayOptions in this.worksheetDisplayOptions.Values )
		//        displayOptions.RemoveUsedColorIndicies( unusedIndicies );
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

        //  BF 8/6/08   Excel2007 Format
        #region SetSaveHiddenRowsAndColumns
        internal void SetSaveHiddenRowsAndColumns( bool value )
        { 
            this.saveHiddenRowsAndColumns = value; 

            //  If the value is set to false, nullify the worksheetHiddenColumns/Rows
            //  dictionaries since they may have been populated by the worksheet's
            //  custom sheet view element logic.
            if ( this.saveHiddenRowsAndColumns == false )
            {
                this.worksheetHiddenColumns = null;
                this.worksheetHiddenRows = null;
            }
        }
        #endregion SetSaveHiddenRowsAndColumns

        //  BF 8/6/08   Excel2007 Format
        #region SetSavePrintOptions
        internal void SetSavePrintOptions( bool value )
        { 
            this.savePrintOptions = value;

            //  If the value is set to false, nullify the worksheetPrintOptions
            //  dictionary since they may have been populated by the worksheet's
            //  custom sheet view element logic.
            if ( this.savePrintOptions == false )
            {
                this.worksheetPrintOptions = null;

				// MD 4/6/12 - TFS102169
                //this.pendingWorksheetPrintOptions = null;
            }
        }
        #endregion SetSavePrintOptions

		// MD 4/6/12 - TFS102169
		// This code is no longer needed.
		#region Removed

		////  BF 8/8/08   Excel2007 Format
		//#region SetPendingCustomViewDisplayOptions
		//internal void SetPendingCustomViewDisplayOptions( Worksheet worksheet, CustomViewDisplayOptions displayOptions )
		//{
		//    if ( this.pendingWorksheetCustomViewDisplayOptions == null )
		//        this.pendingWorksheetCustomViewDisplayOptions = new Dictionary<Worksheet,CustomViewDisplayOptions>();

		//    this.pendingWorksheetCustomViewDisplayOptions.Add( worksheet, displayOptions );
		//}
		//#endregion SetPendingCustomViewDisplayOptions

		////  BF 8/8/08   Excel2007 Format
		//#region SetPendingPrintOptions
		//internal void SetPendingPrintOptions( Worksheet worksheet, PrintOptions displayOptions )
		//{
		//    if ( this.pendingWorksheetPrintOptions == null )
		//        this.pendingWorksheetPrintOptions = new Dictionary<Worksheet,PrintOptions>();

		//    this.pendingWorksheetPrintOptions.Add( worksheet, displayOptions );
		//}
		//#endregion SetPendingPrintOptions

		#endregion // Removed

		// MD 5/25/11 - Data Validations / Page Breaks
		#region VerifyFormatLimits

		internal void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			if (this.worksheetPrintOptions != null)
			{
				foreach (PrintOptions printOptions in this.worksheetPrintOptions.Values)
					printOptions.VerifyFormatLimits(limitErrors, testFormat);
			}
		}

		#endregion  // VerifyFormatLimits

        #endregion Internal Methods

        #endregion Methods

        #region Properties

        #region Public Properties

        #region Name

        // The exception comments should be similar to the exception comments on the 
		// Add method of the CustomViewCollection.

		/// <summary>
		/// Gets or sets the name of the custom view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The name of the custom view is displayed in the custom views dialog of Microsoft Excel and must be 
		/// case-insensitively unique to other custom views in the workbook.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is a null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is the name of another custom view in this custom view's associated workbook (custom view 
		/// names are compared case-insensitively).
		/// </exception>
		/// <value>The name of the custom view.</value>
		public string Name
		{
			get { return this.name; }
			set
			{
				if ( this.name != value )
				{
					if ( String.IsNullOrEmpty( value ) )
						throw new ArgumentNullException( "value", SR.GetString( "LE_ArgumentNullException_CustomViewName" ) );

					if ( this.workbook != null && this.workbook.HasCustomViews )
					{
						foreach ( CustomView customView in this.workbook.CustomViews )
						{
							if ( customView == this )
								continue;

							// MD 4/6/12 - TFS101506
							//if ( String.Compare( value, customView.Name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
							if (String.Compare(value, customView.Name, this.workbook.CultureResolved, CompareOptions.IgnoreCase) == 0)
								throw new ArgumentException( SR.GetString( "LE_ArgumentException_CustomViewNameAlreadyExists", customView.Name ), "value" );
						}
					}

					this.name = value;
				}
			}
		}

		#endregion Name

		#region SaveHiddenRowsAndColumns

		/// <summary>
		/// Gets the value indicating whether hidden row and column settings are saved with the custom view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the value is False, the return value of <see cref="GetHiddenColumns"/> and <see cref="GetHiddenRows"/>
		/// will always be null, regardless of the worksheet specified.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether hidden row and column settings are saved with the custom view.</value>
		/// <seealso cref="GetHiddenColumns"/>
		/// <seealso cref="GetHiddenRows"/>
		/// <seealso cref="HiddenColumnCollection"/>
		/// <seealso cref="HiddenRowCollection"/>
		public bool SaveHiddenRowsAndColumns
		{
			get { return this.saveHiddenRowsAndColumns; }
		}

		#endregion SaveHiddenRowsAndColumns

		#region SavePrintOptions

		/// <summary>
		/// Gets the value indicating whether print options are saved with the custom view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the value is False, the return value of <see cref="GetPrintOptions"/> will always be null, 
		/// regardless of the worksheet specified.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether print options are saved with the custom view.</value>
		/// <seealso cref="GetPrintOptions"/>
		/// <seealso cref="PrintOptions"/>
		public bool SavePrintOptions
		{
			get { return this.savePrintOptions; }
		}

		#endregion SavePrintOptions

		#region WindowOptions

		/// <summary>
		/// Gets the window options for the workbook associated with the custom view.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Setting properties on the returned <see cref="WindowOptions"/> instance will not change the actual
		/// window options of the associated workbook. After setting properties, the <see cref="Apply"/> method 
		/// of the <see cref="CustomView"/> will apply them to the workbook.
		/// </p>
		/// </remarks>
		/// <value>The window options for the workbook associated with the custom view..</value>
		/// <seealso cref="T:Workbook.WindowOptions"/>
		public CustomViewWindowOptions WindowOptions
		{
			get { return this.windowOptions; }
		}

		#endregion WindowOptions

		#endregion Public Properties

		#region Internal Properties

		#region Id

		internal Guid Id
		{
			get
			{
				if ( this.id == Guid.Empty )
					this.id = Guid.NewGuid();

				return this.id;
			}
			set { this.id = value; }
		}

		#endregion Id

		#region Workbook

		internal Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

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