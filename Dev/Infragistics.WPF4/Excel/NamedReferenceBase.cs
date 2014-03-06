using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class for a named reference defined in the workbook.
	/// </summary>



	public

		 abstract class NamedReferenceBase
	{
		#region Constants

		private const string ConsolidateAreaValue = "Consolidate_Area";
		private const string AutoOpenValue = "Auto_Open";
		private const string AutoCloseValue = "Auto_Close";
		private const string ExtractValue = "Extract";
		private const string DatabaseValue = "Database";
		private const string CriteriaValue = "Criteria";

		// MD 5/25/11 - Data Validations / Page Breaks
		// Made internal
		//private const string PrintAreaValue = "Print_Area";
		//private const string PrintTitlesValue = "Print_Titles";
		internal const string PrintAreaValue = "Print_Area";
		internal const string PrintTitlesValue = "Print_Titles";

		private const string RecorderValue = "Recorder";
		private const string DataFormValue = "Data_Form";
		private const string AutoActivateValue = "Auto_Activate";
		private const string AutoDeactivateValue = "Auto_Deactivate";
		private const string SheetTitleValue = "Sheet_Title";
		private const string FilterDatabaseValue = "_FilterDatabase";

		// MD 5/25/11 - Data Validations / Page Breaks
		internal const string BuildInNamePrefixFor2007 = "_xlnm.";

		#endregion Constants

		#region Member Variables

		private Formula formula;
		private string name;
		private object scope;

		private bool isBuiltIn;
		private BuiltInName builtInName;

		// MD 12/22/11 - 12.1 - Table Support
		// Moved this from NamedReference so it could be shared with WorksheetTable.
		private string comment;

		private bool hidden;
		private bool isFunction;
		private bool isMacroName;

		#endregion Member Variables

		#region Constructor

		internal NamedReferenceBase( object scope, bool hidden )
		{
			this.scope = scope;
			this.hidden = hidden;

			this.CheckForBuiltInName();
		}

		#endregion Constructor

		#region Methods

		#region Internal Methods

		// MD 5/25/11 - Data Validations / Page Breaks
		#region LoadPrintAreas

		internal static void LoadPrintAreas(NamedReference reference, PrintOptions printOptions)
		{
			Worksheet worksheet = printOptions.AssociatedWorksheet;

			Debug.Assert(printOptions.PrintAreas.Count == 0, "There shouldn't be any print areas here.");

			List<CellAddressRange> ranges = Utilities.GetRangesFromFormula(reference.FormulaInternal);
			for (int i = 0; i < ranges.Count; i++)
			{
				// MD 2/20/12 - 12.1 - Table Support
				//printOptions.PrintAreas.AddHelper(ranges[i].GetTargetRegion(worksheet, null, -1, false));
				WorksheetRegion region = ranges[i].GetTargetRegion(worksheet, -1, -1, false);
				if (region != null)
					printOptions.PrintAreas.AddHelper(region);
				else
					Utilities.DebugFail("This is unexpected.");
			}
		}

		#endregion  // LoadPrintAreas

		// MD 5/25/11 - Data Validations / Page Breaks
		#region LoadPrintTitles

		internal static void LoadPrintTitles(NamedReference reference, PrintOptions printOptions)
		{
			Worksheet worksheet = printOptions.AssociatedWorksheet;

			List<CellAddressRange> ranges = Utilities.GetRangesFromFormula(reference.FormulaInternal);
			Debug.Assert(ranges.Count <= 2, "There are too many print titles.");

			for (int i = 0; i < ranges.Count; i++)
			{
				CellAddressRange range = ranges[i];

				if (range.TopLeftCellAddress.Column == 0 && range.BottomRightCellAddress.Column == worksheet.Columns.MaxCount - 1)
					printOptions.RowsToRepeatAtTop = new RepeatTitleRange(range.TopLeftCellAddress.Row, range.BottomRightCellAddress.Row);

				if (range.TopLeftCellAddress.Row == 0 && range.BottomRightCellAddress.Row == worksheet.Rows.MaxCount - 1)
					printOptions.ColumnsToRepeatAtLeft = new RepeatTitleRange(range.TopLeftCellAddress.Column, range.BottomRightCellAddress.Column);
			}
		}

		#endregion  // LoadPrintTitles

		#region NameFromBuiltInName

		internal static string NameFromBuiltInName( BuiltInName name )
		{
			switch ( name )
			{
				case BuiltInName.ConsolidateArea:	return NamedReferenceBase.ConsolidateAreaValue;
				case BuiltInName.AutoOpen:			return NamedReferenceBase.AutoOpenValue;
				case BuiltInName.AutoClose:			return NamedReferenceBase.AutoCloseValue;
				case BuiltInName.Extract:			return NamedReferenceBase.ExtractValue;
				case BuiltInName.Database:			return NamedReferenceBase.DatabaseValue;
				case BuiltInName.Criteria:			return NamedReferenceBase.CriteriaValue;
				case BuiltInName.PrintArea:			return NamedReferenceBase.PrintAreaValue;
				case BuiltInName.PrintTitles:		return NamedReferenceBase.PrintTitlesValue;
				case BuiltInName.Recorder:			return NamedReferenceBase.RecorderValue;
				case BuiltInName.DataForm:			return NamedReferenceBase.DataFormValue;
				case BuiltInName.AutoActivate:		return NamedReferenceBase.AutoActivateValue;
				case BuiltInName.AutoDeactivate:	return NamedReferenceBase.AutoDeactivateValue;
				case BuiltInName.SheetTitle:		return NamedReferenceBase.SheetTitleValue;
				case BuiltInName.FilterDatabase:	return NamedReferenceBase.FilterDatabaseValue;

				default:
					Utilities.DebugFail( "Unknown built in name." );
					return null;
			}
		}

		#endregion NameFromBuiltInName

		// MD 7/15/08 - Excel formula solving
		#region OnFormulaChanged






		internal virtual void OnFormulaChanged() { }

		#endregion OnFormulaChanged

		#region OnNameChanged







		// MD 2/22/12 - 12.1 - Table Support
		// Made this non virtual and move the overload from NamedReference to the base so NamedReference and WorksheetTable 
		// could use the same implementation.
		//internal virtual void OnNameChanged( string oldName )
		//{
		//    this.CheckForBuiltInName();
		//}
		internal void OnNameChanged(string oldName)
		{
			Workbook workbook = this.Workbook;

			// MD 1/24/12 - TFS100119
			// It is not a rename when the named reference is first being named.
			if (workbook != null && oldName != null)
			{
				// MD 4/9/12 - TFS101506
				//if (String.Compare(this.Name, oldName, StringComparison.CurrentCultureIgnoreCase) != 0)
				if (String.Compare(this.Name, oldName, workbook.CultureResolved, CompareOptions.IgnoreCase) != 0)
					workbook.OnNamedReferenceRenamed(this, oldName);
			}

			this.CheckForBuiltInName();
		}

		#endregion OnNameChanged

		#region OnNameChanging







		// MD 2/22/12 - 12.1 - Table Support
		// Made this non virtual and move the overload from NamedReference to the base so NamedReference and WorksheetTable 
		// could use the same implementation.
		//internal virtual void OnNameChanging( string newName ) { }
		internal void OnNameChanging(string newName) 
		{
			Workbook workbook = this.Workbook;

			if (workbook != null)
				workbook.VerifyItemName(newName, this);
		}

		#endregion OnNameChanging

		// MD 5/25/11 - Data Validations / Page Breaks
		#region ParseBuiltInNameInfo

		internal static void ParseBuiltInNameInfo(NamedReference reference, PrintOptions printOptions)
		{
			switch (reference.BuiltInName)
			{
				case BuiltInName.PrintArea:
					NamedReferenceBase.LoadPrintAreas(reference, printOptions);
					break;

				case BuiltInName.PrintTitles:
					NamedReferenceBase.LoadPrintTitles(reference, printOptions);
					break;

				default:
					Utilities.DebugFail("Implemented logic to get info from other built in names here.");
					break;
			}
		}

		#endregion ParseBuiltInNameInfo

		#region SetNameInternal

		// MD 3/22/11 - TFS67606
		// Added a new overload.
		internal void SetNameInternal(string value, bool validateName)
		{
			this.SetNameInternal(value, "value", this.CurrentFormat, validateName);
		}

		// MD 7/9/08 - Excel 2007 Format
		//internal void SetNameInternal( string value, string paramName )
		internal void SetNameInternal( string value, string paramName, WorkbookFormat currentFormat )
		{
			// MD 3/22/11 - TFS67606
			// Moved all code to the new overload. By default, we will validate the name.
			this.SetNameInternal(value, paramName, currentFormat, true);
		}

		// MD 3/22/11 - TFS67606
		// Added a new overload to take a validateName parameter
		internal void SetNameInternal(string value, string paramName, WorkbookFormat currentFormat, bool validateName)
		{
			// MD 3/22/11 - TFS67606
			// Wrapped these checks in an if statement. We will only check the name if validateName is True.
			if (validateName)
			{

			if ( String.IsNullOrEmpty( value ) )
				throw new ArgumentNullException( paramName, SR.GetString( "LE_ArgumentNullException_NamedReferenceNameCantBeNull" ) );

			if ( value.Length > 255 )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_NamedReferenceNameTooLong" ), paramName );

			// MD 7/9/08 - Excel 2007 Format
			//if ( FormulaParser.IsValidNamedReference( value ) == false )
			// MD 4/6/12 - TFS101506
			//if ( FormulaParser.IsValidNamedReference( value, currentFormat ) == false )
			if (FormulaParser.IsValidNamedReference(value, currentFormat, this.Culture) == false)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidNamedReferenceName" ), paramName );

			}

			this.OnNameChanging( value );

			string oldName = this.name;
			this.name = value;

			this.OnNameChanged( oldName );
		}

		#endregion SetNameInternal

        #region ToString

        internal virtual string ToString(Dictionary<WorkbookReferenceBase, int> externalReferences)
        {
            return this.ToString();
        }
        #endregion //ToString

        #endregion Internal Methods

        #region Private Methods

        #region BuiltInNameFromName

        private static BuiltInName BuiltInNameFromName( string name )
		{
			if (String.Compare(name, NamedReferenceBase.ConsolidateAreaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.ConsolidateArea;

			if (String.Compare(name, NamedReferenceBase.AutoOpenValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.AutoOpen;

			if (String.Compare(name, NamedReferenceBase.AutoCloseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.AutoClose;

			if (String.Compare(name, NamedReferenceBase.ExtractValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.Extract;

			if (String.Compare(name, NamedReferenceBase.DatabaseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.Database;

			if (String.Compare(name, NamedReferenceBase.CriteriaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.Criteria;

			if (String.Compare(name, NamedReferenceBase.PrintAreaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.PrintArea;

			if (String.Compare(name, NamedReferenceBase.PrintTitlesValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.PrintTitles;

			if (String.Compare(name, NamedReferenceBase.RecorderValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.Recorder;

			if (String.Compare(name, NamedReferenceBase.DataFormValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.DataForm;

			if (String.Compare(name, NamedReferenceBase.AutoActivateValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.AutoActivate;

			if (String.Compare(name, NamedReferenceBase.AutoDeactivateValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.AutoDeactivate;

			if (String.Compare(name, NamedReferenceBase.SheetTitleValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.SheetTitle;

			if (String.Compare(name, NamedReferenceBase.FilterDatabaseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return BuiltInName.FilterDatabase;

			Utilities.DebugFail( "Unknown build in name." );
			return BuiltInName.PrintArea;
		}

		#endregion BuiltInNameFromName

		#region CheckForBuiltInName

		private void CheckForBuiltInName()
		{
			this.isBuiltIn = NamedReferenceBase.IsBuiltInName( this.Name );

			if ( this.isBuiltIn )
				this.builtInName = NamedReferenceBase.BuiltInNameFromName( this.Name );
		}

		#endregion CheckForBuiltInName

		#region IsBuiltInName

		private static bool IsBuiltInName( string name )
		{
			if (String.Compare(name, NamedReferenceBase.ConsolidateAreaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.AutoOpenValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.AutoCloseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.ExtractValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.DatabaseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.CriteriaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.PrintAreaValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.PrintTitlesValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.RecorderValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.DataFormValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.AutoActivateValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.AutoDeactivateValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.SheetTitleValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (String.Compare(name, NamedReferenceBase.FilterDatabaseValue, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			return false;
		}

		#endregion IsBuiltInName

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 3/30/11 - TFS69969
		// We also need a CalcReference for the ExternalNamedReference, so this property was moved from NamedReference to the common base class.
		internal abstract IExcelCalcReference CalcReference { get; }

		// MD 4/6/12 - TFS101506
		internal abstract CultureInfo Culture { get; }

		// MD 7/9/08 - Excel 2007 Format
		// MD 2/24/12 - 12.1 - Table Support
		//internal abstract WorkbookFormat CurrentFormat { get; }
		internal virtual WorkbookFormat CurrentFormat 
		{
			get
			{
				Workbook workbook = this.Workbook;

				if (workbook == null)
					return Workbook.LatestFormat;

				return workbook.CurrentFormat;
			}
		}

		internal abstract Workbook Workbook { get; }

		#region Public Properties

		// MD 12/22/11 - 12.1 - Table Support
		// Moved this from NamedReference so it could be shared with WorksheetTable.
		#region Comment

		/// <summary>
		/// Gets or sets the comment associated with the named reference or table.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// The value assigned is greater than 255 characters in length.
		/// </exception>
		/// <value>The comment associated with the named reference or table.</value>
		public string Comment
		{
			get { return this.comment; }
			set
			{
				if (this.comment != value)
				{
					if (value != null && 255 < value.Length)
						throw new ArgumentException(SR.GetString("LE_ArgumentException_CommentTooLong"), "value");

					this.comment = value;
				}
			}
		}

		#endregion Comment

		#region Name

		/// <summary>
		/// Gets or sets the name of the reference.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// See the overview on <see cref="Scope"/> for details on how to access a named reference by name in formulas.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is not a valid named reference. The name must begin with a letter, underscore (_), or a backslash (\).
		/// All other characters in the name must be letters, numbers, periods, underscores (_), or backslashes (\).
		/// The name cannot be a an A1 cell reference (1 to 3 letters followed by 1 to 6 numbers). In addition, the name
		/// cannot be 'r', 'R', 'c', or 'C' or start with a row or column reference in R1C1 cell reference mode 
		/// ('R' followed by 1 to 6 numbers or 'C' followed by 1 to 6 numbers).
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is the name of another named reference with the same Scope. 
		/// Names are compared case-insensitively.
		/// </exception>
		/// <value>The name of the reference.</value>
		public string Name
		{
			get { return this.name; }
			set
			{
				if ( this.name != value )
				{
					// MD 7/9/08 - Excel 2007 Format
					//this.SetNameInternal( value, "value" );
					this.SetNameInternal( value, "value", this.CurrentFormat );
				}
			}
		}

		#endregion Name

		#region Scope

		/// <summary>
		/// Gets the scope of the named reference.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This can either be the workbook which the named reference belongs to or one of the worksheets 
		/// in the workbook.
		/// </p>
		/// <p class="body">
		/// The scope determines how formulas need to preface a name in order to use the named reference.
		/// </p>
		/// <p class="body">
		/// If the scope is the workbook, formulas in any cell in the workbook can reference the named reference
		/// by specifying just the name or the workbook's file name, an exclamation point, and the name:
		/// <list type="bullet">
		/// <item>=MyWorkbookName</item>
		/// <item>='C:\MyWorkbook.xls'!MyWorkbookName</item>
		/// </list>
		/// When cells in other workbook's want to reference the named reference, they must use the second format
		/// by first specifying the file name when the workbook-scoped named reference exists.
		/// </p>
		/// <p class="body">
		/// If the scope is a worksheet, formulas in cells of the worksheet can reference the named reference
		/// by specifying just the name. In addition, they can fully qualify the named reference with the worksheet name
		/// and, optionally, the workbook file name:
		/// <list type="bullet">
		/// <item>=MyWorksheetName</item>
		/// <item>=Sheet1!MyWorksheetName</item>
		/// <item>='C:\[MyWorkbook.xls]Sheet1'!MyWorksheetName</item>
		/// </list>
		/// Formulas in cells of other worksheets in the same workbook can use the named reference as well, but they must 
		/// specify the worksheet name and, optionally, the workbook file name:
		/// <list type="bullet">
		/// <item>=Sheet2!OtherWorksheetName</item>
		/// <item>='C:\[MyWorkbook.xls]Sheet2'!OtherWorksheetName</item>
		/// </list>
		/// Formulas in cells of other workbooks can also used the named reference, but they must specify the workbook file
		/// name, worksheet name, and named reference name.
		/// </p>
		/// </remarks>
		/// <value>The scope of the named reference.</value>
		public object Scope
		{
			get { return this.scope; }
		}

		#endregion Scope

		#endregion Public Properties

		#region Internal Properties

		#region BuiltInName

		internal BuiltInName BuiltInName
		{
			get { return this.builtInName; }
		}

		#endregion BuiltInName

		#region FormulaInternal

		internal Formula FormulaInternal
		{
			get { return this.formula; }

			// MD 7/15/08 - Excel formula solving
			//set { this.formula = value; }
			set 
			{
				if ( this.formula == value )
					return;

				// MD 6/16/12 - CalcEngineRefactor
				if (this.formula != null)
					this.formula.DisconnectReferences();

				this.formula = value;

				// MD 6/16/12 - CalcEngineRefactor
				if (this.formula != null)
				{
					this.formula.ConnectReferences(
						new FormulaContext(this.Workbook, this.scope as Worksheet, null, -1, this.formula)
					);
				}

				this.OnFormulaChanged();
			}
		}

		#endregion FormulaInternal

		#region Hidden







		internal bool Hidden
		{
			get { return this.hidden; }
			set { this.hidden = value; }
		}

		#endregion Hidden

		#region IsBuiltIn

		internal bool IsBuiltIn
		{
			get { return this.isBuiltIn; }
		}

		#endregion IsBuiltIn

		#region IsFunction

		internal bool IsFunction
		{
			get { return this.isFunction; }
			set { this.isFunction = value; }
		}

		#endregion IsFunction

		#region IsMacroName

		internal bool IsMacroName
		{
			get { return this.isMacroName; }
			set { this.isMacroName = value; }
		}

		#endregion IsMacroName

		// MD 3/2/12 - 12.1 - Table Support
		#region IsNameUniqueAcrossScopes

		internal virtual bool IsNameUniqueAcrossScopes
		{
			get { return false; }
		}

		#endregion // IsNameUniqueAcrossScopes

		// MD 8/21/08 - Excel formula solving
		#region ScopeInternal

		internal object ScopeInternal
		{
			get { return this.scope; }
			set { this.scope = value; }
		}

		#endregion ScopeInternal

		// MD 10/8/07 - BR27172
		#region WorkbookReference







		internal virtual WorkbookReferenceBase WorkbookReference
		{
			get
			{
				Utilities.DebugFail( "This named reference does not have an associated workbook reference." );
				return null;
			}
		}

		#endregion WorkbookReference

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