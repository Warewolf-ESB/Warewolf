using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;





using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a named reference defined in the workbook.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Named references allow for names to be used in formulas instead of complex formulas or cell references.
	/// For example, instead of using the formula =SUM(E1:E20), a named reference with a name of 'Sales' can be 
	/// defined to point to the range of E1:E20 (the named reference's formula would be defined like this: 
	/// =Sheet1!$E$1:$E$20). Then the original formula could be expressed as =SUM(Sales).
	/// </p>
	/// <p class="body">
	/// Each named reference has an associated scope, which can either be the <see cref="Workbook"/> 
	/// to which the named reference belongs or one of the <see cref="Worksheet"/> instances in the Workbook. The scope 
	/// determines how the name must be referenced in formulas for different cells. A scope of the workbook means
	/// the named reference must be accessed by a formula in any cell of the workbook by specifying only the name.
	/// A scope of the worksheet means formulas used in other worksheets must reference the name by first 
	/// specifying the worksheet scope, such as =SUM( Sheet2!Sales ). If the formula is in the same worksheet as 
	/// the scope of the named reference, the formula can reference the name with or without the worksheet name.
	/// </p>
	/// <p class="body">
	/// Named references from external workbooks must always be referenced with the scope first. If the named 
	/// reference's scope is the external workbook, the name is accessed by specifying the workbook file name
	/// followed by the name, such as in the following formula: ='C:\ExternalWorkbook.xls'!SalesTax. If the named
	/// reference has a scope of a worksheet in the workbook, it is referenced by specifying the file name, 
	/// worksheet, and name: ='C:\[ExternalWorkbook.xls]Sheet1'!SalesTax.
	/// </p>
	/// <p class="body">
	/// Named references with different scopes can have the same names, but if two named references have the same
	/// scope, they must have case-insensitively unique names.
	/// </p>
	/// </remarks>



	public

		 class NamedReference : NamedReferenceBase
	{
		#region Member Variables

		// MD 7/15/08 - Excel formula solving
		private NamedCalcReference calcReference;

		private NamedReferenceCollection collection;

		// MD 12/22/11 - 12.1 - Table Support
		// Moved to NamedReferenceBase to so it could be shared with WorksheetTable.
		//private string comment;

		// MD 7/19/12 - TFS116808 (Table resizing)
		// We need to know when 
		private bool isInitializing;

		#endregion Member Variables

		#region Constructor

		internal NamedReference( NamedReferenceCollection collection, object scope )
			: this( collection, scope, false ) { }

		internal NamedReference( NamedReferenceCollection collection, object scope, bool hidden )
			: base( scope, hidden )
		{
			// MD 5/25/11 - Data Validations / Page Breaks
			// We shouldn't allow a null workbook in the constructor.
			Debug.Assert(collection != null, "We should always have a collection so the formula is parsed with the correct format.");

			this.collection = collection;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 4/6/12 - TFS101506
		#region Culture

		internal override CultureInfo Culture
		{
			get 
			{
				Workbook workbook = this.Workbook;
				if (workbook != null)
					return workbook.CultureResolved;

				return CultureInfo.CurrentCulture;
			}
		}

		#endregion // Culture

		// MD 2/24/12 - 12.1 - Table Support
		// Moved this implementation to the base class.
		#region Moved

		//// MD 7/9/08 - Excel 2007 Format
		//#region CurrentFormat

		//internal override WorkbookFormat CurrentFormat
		//{
		//    get
		//    {
		//        Workbook workbook = this.Workbook;

		//        if ( workbook == null )
		//            return WorkbookFormat.Excel97To2003;

		//        return workbook.CurrentFormat;
		//    }
		//}

		//#endregion CurrentFormat

		#endregion // Moved

		// MD 7/15/08 - Excel formula solving
		#region OnFormulaChanged

		internal override void OnFormulaChanged()
		{
			// MD 7/19/12 - TFS116808 (Table resizing)
			// Don't do anything if the named reference is initializing.
			if (this.isInitializing)
				return;

			if ( this.FormulaInternal == null || this.FormulaInternal.PostfixTokenList.Count == 0 )
				return;

			// MD 7/19/12 - TFS116808 (Table resizing)
			// The calc reference can be null here.
			//this.CalcReferenceInternal.SetAndCompileFormula( this.FormulaInternal, false );
			ExcelRefBase calcReference = this.CalcReferenceInternal;
			if (calcReference != null)
				calcReference.SetAndCompileFormula(this.FormulaInternal, false);
		}

		#endregion OnFormulaChanged

		// MD 2/22/12 - 12.1 - Table Support
		// Made these non virtual and moved the implementations to the base so NamedReference and WorksheetTable 
		// could use the same implementation.
		#region Moved

		//        #region OnNameChanged

		//#if DEBUG
		//        /// <summary>
		//        /// Gets called after the named reference's name has changed
		//        /// </summary>
		//        /// <param name="oldName">The previous name of the named reference.</param>  
		//#endif
		//        internal override void OnNameChanged( string oldName )
		//        {
		//            // MD 1/24/12 - TFS100119
		//            // It is not a rename when the named reference is first being named.
		//            if (oldName != null)
		//            {

		//            Workbook workbook = this.Workbook;

		//            if ( workbook != null &&
		//                String.Compare( this.Name, oldName, StringComparison.CurrentCultureIgnoreCase ) != 0 )
		//            {
		//                workbook.OnNamedReferenceRenamed( this, oldName );
		//            }

		//            }

		//            base.OnNameChanged( oldName );
		//        }

		//        #endregion OnNameChanged

		//        #region OnNameChanging

		//#if DEBUG
		//        /// <summary>
		//        /// Gets called before the named reference's name changes
		//        /// </summary>
		//        /// <param name="newName">The new name to give to the named reference.</param>  
		//#endif
		//        internal override void OnNameChanging( string newName )
		//        {
		//            Workbook workbook = this.Workbook;

		//            if ( workbook != null && workbook.HasNamedReferences )
		//            {
		//                NamedReference[] nameMatches = workbook.NamedReferences.FindAll( newName );

		//                foreach ( NamedReference namedReference in nameMatches )
		//                {
		//                    if ( namedReference == this )
		//                        continue;

		//                    if ( namedReference.Scope == this.Scope )
		//                        throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_NamedReferenceNameAlreadyExists", newName ) );
		//                }
		//            }

		//            base.OnNameChanging( newName );
		//        }

		//        #endregion OnNameChanging

		#endregion // Moved

		#region ToString

		/// <summary>
		/// Gets the string representation of the named reference.
		/// </summary>
		/// <returns>The string representation of the named reference.</returns>
		public override string ToString()
		{
			if ( this.Scope is Workbook )
				return this.Name;

			Worksheet worksheet = this.Scope as Worksheet;

			if ( worksheet != null )
				return Utilities.CreateReferenceString( null, worksheet.Name ) + this.Name;

			Utilities.DebugFail( "Unknown scope." );
			return this.Name;
		}

		#endregion ToString

		// MD 6/16/12 - CalcEngineRefactor
		#region WorkbookReference

		internal override WorkbookReferenceBase WorkbookReference
		{
			get
			{
				Workbook workbook = this.Workbook;
				if (workbook == null)
					return null;

				return workbook.CurrentWorkbookReference;
			}
		}

		#endregion // WorkbookReference

		#endregion Base Class Overrides

		#region Methods

		#region SetFormula( string )

		/// <summary>
		/// Sets the formula for a named reference.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The formula will be parsed using the <see cref="CellReferenceMode"/> of the <see cref="Workbook"/> 
		/// to which the NamedReference is applied. If the NamedReference has been removed from its collection, the A1 reference mode 
		/// will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="formula">The string containing the formula value.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <seealso cref="Formula"/>
		public void SetFormula( string formula )
		{
			CellReferenceMode cellReferenceMode = CellReferenceMode.A1;

			Workbook workbook = this.Workbook;
			if ( workbook != null )
				cellReferenceMode = workbook.CellReferenceMode;

			this.SetFormula( formula, cellReferenceMode );
		} 

		#endregion SetFormula( string )

		#region SetFormula( string, CellReferenceMode )

		/// <summary>
		/// Sets the formula for a named reference.
		/// </summary>
		/// <param name="formula">The string containing the formula value.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <seealso cref="Formula"/>
		public void SetFormula( string formula, CellReferenceMode cellReferenceMode )
		{
			// MD 4/6/12 - TFS101506
			//this.SetFormula( formula, cellReferenceMode, CultureInfo.CurrentCulture );
			this.SetFormula(formula, cellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Sets the formula for a named reference.
		/// </summary>
		/// <param name="formula">The string containing the formula value.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <seealso cref="Formula"/>
		public void SetFormula( string formula, CellReferenceMode cellReferenceMode, CultureInfo culture )
		{
			if ( String.IsNullOrEmpty( formula ) )
				throw new ArgumentNullException( "formula", SR.GetString( "LE_ArgumentNullException_FormulaCantBeNull" ) );

			if ( Enum.IsDefined( typeof( CellReferenceMode ), cellReferenceMode ) == false )
				throw new InvalidEnumArgumentException( "cellReferenceMode", (int)cellReferenceMode, typeof( CellReferenceMode ) );

			Formula parsedFormula;
			FormulaParseException exc;

			// MD 7/9/08 - Excel 2007 Format
			//if ( Infragistics.Documents.Excel.Formula.TryParse( formula, cellReferenceMode, FormulaType.NamedReferenceFormula, out parsedFormula, out exc ) == false )
			// MD 2/23/12 - TFS101504
			// Pass along null for the new indexedReferencesDuringLoad parameter.
			//if ( Excel.Formula.TryParse( formula, cellReferenceMode, FormulaType.NamedReferenceFormula, this.CurrentFormat, culture, out parsedFormula, out exc ) == false )
			if (Excel.Formula.TryParse(formula, cellReferenceMode, FormulaType.NamedReferenceFormula, this.CurrentFormat, culture, null, out parsedFormula, out exc) == false)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidFormula" ), "formula", exc );

			// MD 7/2/08 - Excel 2007 Format
			FormatLimitErrors limitErrors = new FormatLimitErrors();

            // MBS 7/10/08 - Excel 2007 Format
            // Added additional parameters CellReferenceMode and also to tell the method not to bother
            // re-parsing the formula since we just did that earlier in the method
			// MD 3/2/12 - 12.1 - Table Support
            //parsedFormula.VerifyFormatLimits(limitErrors, this.CurrentFormat, cellReferenceMode, false);
			parsedFormula.VerifyFormatLimits(this.Workbook, limitErrors, this.CurrentFormat, cellReferenceMode, false);

			if ( limitErrors.HasErrors )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_FormulaReferencesInvalidCells" ), "formula" );

			// MD 3/2/12 - 12.1 - Table Support
			if (this.Workbook != null)
				this.Workbook.VerifyFormula(parsedFormula, null, -1);

			this.FormulaInternal = parsedFormula;
		}

		#endregion SetFormula( string, CellReferenceMode )

		#region OnRemovedFromCollection






		internal void OnRemovedFromCollection()
		{
			// MD 6/16/12 - CalcEngineRefactor
			if (this.FormulaInternal != null)
				this.FormulaInternal.DisconnectReferences();

			// MD 8/26/08 - Excel formula solving
			// When the named reference is removed, the reference should be removed from the calc network so formulas referencing 
			// it are updated.
			Workbook workbook = this.Workbook;
			if ( workbook != null )
				workbook.RemoveReference(this.CalcReference);

			// MD 5/25/11 - Data Validations / Page Breaks
			// If the named reference being removed is synced with some settings on the worksheet, make sure to change them appropriately
			if (this.IsBuiltIn)
			{
				Worksheet worksheet = this.Scope as Worksheet;
				if (worksheet != null)
				{
					switch (this.BuiltInName)
					{
						case BuiltInName.PrintArea:
							worksheet.PrintOptions.PrintAreas.Clear();
							break;

						case BuiltInName.PrintTitles:
							worksheet.PrintOptions.ColumnsToRepeatAtLeft = null;
							worksheet.PrintOptions.RowsToRepeatAtTop = null;
							break;

						default:
							Utilities.DebugFail("We aren't handling the removal of this built in named reference.");
							break;
					}
				}
				// MD 2/23/12 - 12.1 - Table Support
				else if (this.Scope == workbook)
				{
					workbook.OnNamedReferenceRemoved(this);
				}
				else
				{
					Utilities.DebugFail("This is unexpected.");
				}
			}

			this.collection = null;
		}

		#endregion OnRemovedFromCollection

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			Formula formula = this.FormulaInternal;

			if ( formula == null )
			{
				Utilities.DebugFail( "The formula should not be null on a named references." );
				return;
			}
            
            // MBS 7/10/08 - Excel 2007 Format
            // Also pass in the current CellReferenceMode and specify that the formula should be re-evaluated
            CellReferenceMode cellReferenceMode = this.Workbook != null ? this.Workbook.CellReferenceMode : CellReferenceMode.A1;

			// MD 3/2/12 - 12.1 - Table Support
			//formula.VerifyFormatLimits( limitErrors, testFormat, cellReferenceMode, true);
			formula.VerifyFormatLimits(this.Workbook, limitErrors, testFormat, cellReferenceMode, true);
		} 

		#endregion VerifyFormatLimits

		#endregion Methods

		#region Properties

		#region Public Properties

		// MD 12/22/11 - 12.1 - Table Support
		// Moved to NamedReferenceBase to so it could be shared with WorksheetTable.
		#region Moved

		//#region Comment

		///// <summary>
		///// Gets or sets the comment associated with the named reference.
		///// </summary>
		///// <exception cref="ArgumentException">
		///// The value assigned is greater than 255 characters in length.
		///// </exception>
		///// <value>The comment associated with the named reference.</value>
		//public string Comment
		//{
		//    get { return this.comment; }
		//    set 
		//    {
		//        if ( this.comment != value )
		//        {
		//            if ( value != null && 255 < value.Length )
		//                throw new ArgumentException( SR.GetString( "LE_ArgumentException_CommentTooLong" ), "value" );

		//            this.comment = value;
		//        }
		//    }
		//}

		//#endregion Comment

		#endregion // Moved

		#region Formula

		/// <summary>
		/// Gets the formula which defines the named reference.
		/// </summary>
		/// <value>The formula which defines the named reference.</value>
		/// <seealso cref="SetFormula(string)"/>
		/// <seealso cref="SetFormula(string,CellReferenceMode)"/>
		public string Formula
		{
			get
			{
				if ( this.FormulaInternal == null )
					return null;

				Workbook workbook = this.Workbook;

				if ( workbook == null )
					return this.FormulaInternal.ToString();

				return this.FormulaInternal.ToString( workbook.CellReferenceMode );
			}
		}

		#endregion Formula

		// MD 2/7/12 - 12.1 - Get Cell By Name Reference
		#region ReferencedCell

		/// <summary>
		/// Gets the <see cref="WorksheetCell"/> referenced by the <see cref="Formula"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A valid instance will only be returned if the named reference refers to a single cell in the same workbook as the 
		/// named reference. If the cell reference is surrounded by parentheses or whitespace, or the named reference has some 
		/// other complex formula, or is a reference to on or more regions, this will return null.
		/// </p>
		/// </remarks>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetCell ReferencedCell
		{
			get
			{
				Workbook workbook = this.Workbook;
				Formula formula = this.FormulaInternal;
				if (formula == null || workbook == null)
					return null;

				FormulaContext context = new FormulaContext(workbook, formula);

				bool isNamedReference;
				CellCalcReference parsedCellReference = this.Workbook.ParseReference(context, out isNamedReference) as CellCalcReference;
				if (parsedCellReference == null)
					return null;

				return parsedCellReference.Row.Cells[parsedCellReference.ColumnIndex];
			}
		}

		#endregion // ReferencedCell

		// MD 2/7/12 - 12.1 - Get Cell By Name Reference
		#region ReferencedRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> referenced by the <see cref="Formula"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A valid instance will only be returned if the named reference refers to a single region in the same workbook as the 
		/// named reference. If the region reference is surrounded by parentheses or whitespace, or the named reference has some 
		/// other complex formula, or is a reference to a single cell or multiple regions, this will return null.
		/// </p>
		/// </remarks>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion ReferencedRegion
		{
			get
			{
				Workbook workbook = this.Workbook;
				Formula formula = this.FormulaInternal;
				if (formula == null || workbook == null)
					return null;

				FormulaContext context = new FormulaContext(workbook, formula);

				bool isNamedReference;
				RegionCalcReferenceBase parsedRegionReference = this.Workbook.ParseReference(context, out isNamedReference) as RegionCalcReferenceBase;
				if (parsedRegionReference == null)
					return null;

				return parsedRegionReference.Region;
			}
		}

		#endregion // ReferencedRegion

		// MD 2/27/12 - TFS102520
		#region ReferencedRegions

		/// <summary>
		/// Gets the array of <see cref="WorksheetRegion"/> instances referenced by the <see cref="Formula"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A valid instance will only be returned if the named reference refers to multiple regions in the same workbook as the 
		/// named reference. If the regions referenced are surrounded by parentheses or whitespace, or the named reference has some 
		/// other complex formula, or is a reference to a single cell or region, this will return null.
		/// </p>
		/// </remarks>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelGetCellByName)]

		public WorksheetRegion[] ReferencedRegions
		{
			get
			{
				Workbook workbook = this.Workbook;
				Formula formula = this.FormulaInternal;
				if (formula == null || workbook == null)
					return null;

				FormulaContext context = new FormulaContext(workbook, formula);

				bool isNamedReference;
				RegionGroupCalcReference parsedRegionGroupReference = this.Workbook.ParseReference(context, out isNamedReference) as RegionGroupCalcReference;
				if (parsedRegionGroupReference == null)
					return null;

				WorksheetRegion[] regions = new WorksheetRegion[parsedRegionGroupReference.Regions.Count];
				parsedRegionGroupReference.Regions.CopyTo(regions, 0);
				return regions;
			}
		}

		#endregion // ReferencedRegions

		#endregion Public Properties

		#region Internal Properties

		// MD 7/15/08 - Excel formula solving
		#region CalcReference

		// MD 3/30/11 - TFS69969
		// CalcReference is now a virtual on the base class with a type of ExcelRefBase.
		//internal NamedCalcReference CalcReference
		internal override IExcelCalcReference CalcReference
		{
			get { return this.CalcReferenceInternal; }
		}

		private ExcelRefBase CalcReferenceInternal
		{
			get
			{
				if ( this.calcReference == null )
				{
					this.calcReference = new NamedCalcReference( this );

					Workbook workbook = this.Workbook;

					if ( workbook != null )
						workbook.AddReference( this.calcReference );
				}

				return this.calcReference;
			}
		}

		#endregion CalcReference

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region IsInitializing

		internal bool IsInitializing
		{
			get { return this.isInitializing; }
			set { this.isInitializing = value; }
		}

		#endregion // IsInitializing

		#endregion Internal Properties

		#region Private Properties

		#region Workbook

		// MD 7/15/08 - Excel formula solving
		//private Workbook Workbook
		//internal Workbook Workbook
		internal override Workbook Workbook
		{
			get
			{
				if ( this.collection == null )
					return null;

				return this.collection.Workbook;
			}
		}

		#endregion Workbook

		#endregion Private Properties

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