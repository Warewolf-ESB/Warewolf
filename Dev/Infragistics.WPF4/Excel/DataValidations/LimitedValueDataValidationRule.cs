using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using System.Globalization;





using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Base class for all data validations rules which prevent certain values form being applied to a cell.
	/// </summary>
	/// <seealso cref="ListDataValidationRule"/>
	/// <seealso cref="CustomDataValidationRule"/>
	/// <seealso cref="OneConstraintDataValidationRule"/>
	/// <seealso cref="TwoConstraintDataValidationRule"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 abstract class LimitedValueDataValidationRule : DataValidationRule
	{
		#region Member Variables

		private bool allowNull;

		#endregion  // Member Variables

		#region Constructor

		internal LimitedValueDataValidationRule()
		{
			this.allowNull = true;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal sealed override bool AllowNullInternal
		{
			get { return this.AllowNull; }
			set { this.AllowNull = value; }
		}

		#endregion  // Base Class Overrides

		#region Internal Methods

		// MD 12/21/11 - TFS97840
		// Added more DV formula restrictions based on the MSDN documentation.
		#region IsTokenAllowedInFormula

		// MD 2/24/12 - 12.1 - Table Support
		//internal virtual bool IsTokenAllowedInFormula(FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1)
		internal virtual bool IsTokenAllowedInFormula(FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1, WorkbookFormat format)
		{
			// These restrictions can be found here: http://msdn.microsoft.com/en-us/library/dd953412(v=office.12).aspx

			// If this is formula2, the root node must be a value type.
			if (isFormula1 == false && isRootNode)
			{
				bool isValueType = token.TokenClass == TokenClass.Value || token.TokenClass == TokenClass.Array;

				if (isValueType == false)
					return false;
			}

			switch (token.Token)
			{
				// These tokens are not allowed in either formula.
				case Token.Exp:
				case Token.Tbl:
				case Token.Isect:
				case Token.Union:
				case Token.ArrayA:
				case Token.ArrayR:
				case Token.ArrayV:
				case Token.MemAreaA:
				case Token.MemAreaR:
				case Token.MemAreaV:
				case Token.MemNoMemA:
				case Token.MemNoMemR:
				case Token.MemNoMemV:
					return false;

				// These tokens are documented as not allowed in either formula in the 2003 format, but they are allowed in Excel 2007.
				case Token.Ref3dA:
				case Token.Ref3dR:
				case Token.Ref3dV:
				case Token.RefErr3dA:
				case Token.RefErr3dR:
				case Token.RefErr3dV:
				case Token.NameXA:
				case Token.NameXR:
				case Token.NameXV:
					// MD 2/24/12 - 12.1 - Table Support
					//return Utilities.Is2003Format(this.CurrentFormat) == false;
					return Utilities.Is2003Format(format) == false;

				// These tokens are not allowed in formula2.
				case Token.Area3DA:
				case Token.Area3DR:
				case Token.Area3DV:
				case Token.AreaErr3dA:
				case Token.AreaErr3dR:
				case Token.AreaErr3dV:
					return isFormula1;

				// These tokens are not allowed in formula2 if they are the only token.
				case Token.AreaA:
				case Token.AreaR:
				case Token.AreaV:
				case Token.AreaErrA:
				case Token.AreaErrR:
				case Token.AreaErrV:
				case Token.AreaNA:
				case Token.AreaNR:
				case Token.AreaNV:
					if (isFormula1)
						return true;

					return isOnlyToken == false;

				case Token.Extended:
					Utilities.DebugFail("Figure out which extended tokens are allowed here.");
					return true;

				default:
					return true;
			}
		}

		#endregion  // IsTokenAllowedInFormula

		// MD 12/21/11 - TFS97840
		// Added more DV formula restrictions based on the MSDN documentation.
		#region IsTokenAllowedInNonListFormula

		internal static bool IsTokenAllowedInNonListFormula(FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1)
		{
			// These restrictions can be found here: http://msdn.microsoft.com/en-us/library/dd953412(v=office.12).aspx

			if (isFormula1)
			{
				// The root node of the parse tree of this field MUST be a VALUE_TYPE, as described in Rgce.
				if (isRootNode)
				{
					bool isValueType = token.TokenClass == TokenClass.Value || token.TokenClass == TokenClass.Array;

					if (isValueType == false)
						return false;
				}

				switch (token.Token)
				{
					// rgce MUST NOT contain a PtgArea3d or a PtgAreaErr3d.
					case Token.Area3DA:
					case Token.Area3DR:
					case Token.Area3DV:
					case Token.AreaErr3dA:
					case Token.AreaErr3dR:
					case Token.AreaErr3dV:
						return false;

					// A PtgArea, a PtgAreaErr, or a PtgAreaN, MUST NOT be the only Ptg in rgce.
					case Token.AreaA:
					case Token.AreaR:
					case Token.AreaV:
					case Token.AreaErrA:
					case Token.AreaErrR:
					case Token.AreaErrV:
					case Token.AreaNA:
					case Token.AreaNR:
					case Token.AreaNV:
						return isOnlyToken == false;
				}
			}

			return true;
		}

		#endregion  // IsTokenAllowedInNonListFormula

		#region ParseFormula

		// MD 12/21/11 - TFS97840
		// ParseFormula is no longer static because it need to access some instance members.
		//internal static Formula ParseFormula(string formula, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		internal Formula ParseFormula(string formula, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			if (Enum.IsDefined(typeof(WorkbookFormat), format) == false)
				throw new InvalidEnumArgumentException("format", (int)cellReferenceMode, typeof(WorkbookFormat));

			if (Enum.IsDefined(typeof(CellReferenceMode), cellReferenceMode) == false)
				throw new InvalidEnumArgumentException("cellReferenceMode", (int)cellReferenceMode, typeof(CellReferenceMode));

			if (String.IsNullOrEmpty(formula))
				return null;

			Formula parsedFormula;
			FormulaParseException exc;

			// MD 12/21/11 - TFS97840
			// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
			//if (Formula.TryParse(formula, cellReferenceMode, FormulaType.Formula, format, culture, out parsedFormula, out exc) == false)
			// MD 2/23/12 - TFS101504
			// Pass along null for the indexedReferencesDuringLoad parameter.
			//if (Formula.TryParse(formula, cellReferenceMode, this.FormulaType, format, culture, out parsedFormula, out exc) == false)
			if (Formula.TryParse(formula, cellReferenceMode, this.FormulaType, format, culture, null, out parsedFormula, out exc) == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidFormula"), exc);

			return parsedFormula;
		}

		#endregion  // ParseFormula

		#region ResolveFormula

		// MD 4/9/12 - TFS101506
		//internal static Formula ResolveFormula(Formula originalFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, bool addressIsTarget)
		// MD 6/13/12 - CalcEngineRefactor
		//internal static Formula ResolveFormula(Formula originalFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture, bool addressIsTarget)
		internal Formula ResolveFormula(Formula originalFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture, bool addressIsTarget)
		{
			if (Enum.IsDefined(typeof(WorkbookFormat), format) == false)
				throw new InvalidEnumArgumentException("format", (int)format, typeof(WorkbookFormat));

			if (Enum.IsDefined(typeof(CellReferenceMode), cellReferenceMode) == false)
				throw new InvalidEnumArgumentException("cellReferenceMode", (int)cellReferenceMode, typeof(CellReferenceMode));

			if (originalFormula == null || address == null)
				return originalFormula;

			// MD 4/9/12 - TFS101506
			if (culture == null)
				culture = originalFormula.Culture;

			int firstRowIndex;
			short firstColumnIndex;
			int lastRowIndex;
			short lastColumnIndex;
			// MD 4/9/12 - TFS101506
			//Utilities.ParseRegionAddress(address, format, cellReferenceMode, null, -1,
			Utilities.ParseRegionAddress(address, format, cellReferenceMode, culture, null, -1,
				out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			if (firstRowIndex < 0 || firstColumnIndex < 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_InvalidAddress"), "address");

			Formula formulaClone = originalFormula.Clone();
			FormulaContext context = new FormulaContext(this.Worksheet, firstRowIndex, firstColumnIndex, format, formulaClone);
			for (int i = 0; i < formulaClone.PostfixTokenList.Count; i++)
			{
				FormulaToken token = formulaClone.PostfixTokenList[i];
				if (addressIsTarget)
					token = token.GetNonSharedEquivalent(context);
				else
					token = token.GetSharedEquivalent(context);

				formulaClone.PostfixTokenList[i] = token;
			}

			formulaClone.ClearCache();

			// MD 6/16/12 - CalcEngineRefactor
			if (addressIsTarget == false && this.Worksheet != null)
				formulaClone.ConnectReferences(context);

			return formulaClone;
		}

		#endregion  // ResolveFormula

		#region VerifyFormula

		internal virtual void VerifyFormula(Formula formula, 
			DataValidationRuleCollection collection, 
			WorksheetReferenceCollection references, 
			string formulaPropertyName, 
			bool isOriginalFormula,
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			bool isFormula1,
			WorkbookFormat format)
		{
			if (formula == null)
			{
				if (collection != null)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_FormulaCannotBeNull", formulaPropertyName));

				return;
			}

			// MD 2/24/12 - 12.1 - Table Support
			Workbook workbook = null;
			if (collection != null)
			{
				workbook = collection.Worksheet.Workbook;
				format = collection.Worksheet.CurrentFormat;
			}

			// MD 12/21/11 - TFS97840
			// Added more DV formula restrictions based on the MSDN documentation.
			if (isOriginalFormula)
			{
				int rootNodeIndex = formula.PostfixTokenList.Count - 1;
				bool isOnlyNode = rootNodeIndex == 0;
				for (int i = 0; i < formula.PostfixTokenList.Count; i++)
				{
					// MD 2/24/12 - 12.1 - Table Support
					//if (this.IsTokenAllowedInFormula(formula.PostfixTokenList[i], i == rootNodeIndex, isOnlyNode, isFormula1) == false)
					if (this.IsTokenAllowedInFormula(formula.PostfixTokenList[i], i == rootNodeIndex, isOnlyNode, isFormula1, format) == false)
						throw new ArgumentException(SR.GetString("Invalid formula used in the data validation rule.")); 
				}
			}

			Workbook owningWorkbook = null;
			Worksheet owningWorksheet = null;

			// MD 6/11/12 - TFS113884
			string loadingPath = null;

			if (collection != null)
			{
				owningWorksheet = collection.Worksheet;
				owningWorkbook = owningWorksheet.Workbook;

				// MD 6/11/12 - TFS113884
				if (owningWorkbook != null)
					loadingPath = owningWorkbook.LoadingPath;

				if (formula.CurrentFormat != collection.Worksheet.CurrentFormat)
				{
					// MD 2/24/12 - 12.1 - Table Support
					// This is now passed in.
					//WorkbookFormat format = collection.Worksheet.CurrentFormat;

					CellReferenceMode cellReferenceMode = collection.Worksheet.CellReferenceMode;

					if (references == null)
						references = collection[this];

					if (references != null)
					{
						// MD 1/23/12
						// Found while fixing TFS99849
						// We only need two corner cells here.
						#region Old Code

						//WorksheetCell leftMostCell;
						//WorksheetCell topMostCell;
						//WorksheetCell bottomMostCell;
						//WorksheetCell rightMostCell;
						//references.GetBoundingCells(out leftMostCell, out topMostCell, out rightMostCell, out bottomMostCell);

						//Dictionary<WorksheetCell, Formula> formulas = new Dictionary<WorksheetCell, Formula>();
						//formulas[leftMostCell] = LimitedValueDataValidationRule.ResolveFormula(formula, leftMostCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);

						//if (formulas.ContainsKey(topMostCell) == false)
						//    formulas[topMostCell] = LimitedValueDataValidationRule.ResolveFormula(formula, topMostCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);

						//if (formulas.ContainsKey(rightMostCell) == false)
						//    formulas[rightMostCell] = LimitedValueDataValidationRule.ResolveFormula(formula, rightMostCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);

						//if (formulas.ContainsKey(bottomMostCell) == false)
						//    formulas[bottomMostCell] = LimitedValueDataValidationRule.ResolveFormula(formula, bottomMostCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);

						#endregion // Old Code
						WorksheetCell topLeftCell;
						WorksheetCell bottomRightCell;
						references.GetBoundingCells(out topLeftCell, out bottomRightCell);

						if (topLeftCell != null)
						{
							// MD 4/9/12 - TFS101506
							//Formula topLeftCellFormula = LimitedValueDataValidationRule.ResolveFormula(formula, topLeftCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);
							CultureInfo culture = formula.Culture;
							Formula topLeftCellFormula = this.ResolveFormula(formula, topLeftCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, culture, true);

							// MD 3/2/12 - 12.1 - Table Support
							//LimitedValueDataValidationRule.VerifyFormulaHelper(topLeftCellFormula, formulaPropertyName, format, cellReferenceMode);
							LimitedValueDataValidationRule.VerifyFormulaHelper(workbook, topLeftCellFormula, formulaPropertyName, format, cellReferenceMode);

							if (topLeftCell != bottomRightCell)
							{
								// MD 4/9/12 - TFS101506
								//Formula bottomRightCellFormula = LimitedValueDataValidationRule.ResolveFormula(formula, bottomRightCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, true);
								Formula bottomRightCellFormula = this.ResolveFormula(formula, bottomRightCell.ToString(cellReferenceMode, false, false, false), formula.CurrentFormat, cellReferenceMode, culture, true);

								// MD 3/2/12 - 12.1 - Table Support
								//LimitedValueDataValidationRule.VerifyFormulaHelper(bottomRightCellFormula, formulaPropertyName, format, cellReferenceMode);
								LimitedValueDataValidationRule.VerifyFormulaHelper(workbook, bottomRightCellFormula, formulaPropertyName, format, cellReferenceMode);
							}
						}
					}
					else
					{
						// MD 3/2/12 - 12.1 - Table Support
						//LimitedValueDataValidationRule.VerifyFormulaHelper(formula, formulaPropertyName, format, cellReferenceMode);
						LimitedValueDataValidationRule.VerifyFormulaHelper(workbook, formula, formulaPropertyName, format, cellReferenceMode);
					}
				}
			}
			// MD 6/11/12 - TFS113884
			else
			{
				loadingPath = this.LoadingWorkbookPath;
			}

			for (int i = 0; i < formula.PostfixTokenList.Count; i++)
			{
				ReferenceToken referenceToken = formula.PostfixTokenList[i] as ReferenceToken;
				if (referenceToken == null)
					continue;

				if (referenceToken.IsExternalReference)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_FormulaCannotReferenceOtherWorkbook", formulaPropertyName));

				// If the workbook is loading, the remaining worksheets and/or named references may not be loaded yet, so we can't 
				// verify that they exist. We can just assume that if the rule was in the saved workbook, the file producer verified 
				// these conditions when the file was created.
				if (owningWorkbook == null || owningWorkbook.IsLoading)
					continue;

				Worksheet worksheet = owningWorksheet;

				WorksheetReferenceLocal worksheetReferenceLocal = referenceToken.WorksheetReference as WorksheetReferenceLocal;
				if (worksheetReferenceLocal != null)
					worksheet = worksheetReferenceLocal.Worksheet;

				NameToken nameToken = referenceToken as NameToken;
				if (nameToken == null)
					continue;

				NamedReferenceBase namedReference = owningWorkbook.NamedReferences.Find(nameToken.Name, worksheet);
				if (namedReference == null)
				{
					if (nameToken.ScopeReference == null || nameToken.ScopeReference == owningWorkbook)
						namedReference = owningWorkbook.GetWorkbookScopedNamedItem(nameToken.Name);
				}

				if (namedReference == null)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_FormulaCannotFindNamedReference", formulaPropertyName));

				// MD 12/21/11 - TFS97840
				// Added a parameter to determine whether formula1 or formula2 is being validated.
				//this.VerifyFormula(namedReference.FormulaInternal, collection, references, formulaPropertyName, false);
				// MD 2/24/12 - 12.1 - Table Support
				//this.VerifyFormula(namedReference.FormulaInternal, collection, references, formulaPropertyName, false, isFormula1);
				this.VerifyFormula(namedReference.FormulaInternal, collection, references, formulaPropertyName, false, isFormula1, format);
			}
		}

		#endregion  // VerifyFormula

		#region VerifyFormulaHelper

		// MD 3/2/12 - 12.1 - Table Support
		//private static void VerifyFormulaHelper(Formula formula, string formulaPropertyName, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private static void VerifyFormulaHelper(Workbook workbook, Formula formula, string formulaPropertyName, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		{
			FormatLimitErrors limitErrors = new FormatLimitErrors();

			// MD 3/2/12 - 12.1 - Table Support
			//formula.VerifyFormatLimits(limitErrors, format, cellReferenceMode, true);
			formula.VerifyFormatLimits(workbook, limitErrors, format, cellReferenceMode, true);

			if (limitErrors.HasErrors)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_FormulaInvalidForWorkbookFormat", formulaPropertyName, format));
		}

		#endregion  // VerifyFormulaHelper

		#endregion  // Internal Methods

		#region Properties

		#region Public Properties

		#region AllowNull

		/// <summary>
		/// Gets or sets the value which indicates whether a null, or blank, value is allowed to be set on a cell.
		/// </summary>
		/// <value>True if a null value can be set on the cell; False otherwise.</value>
		public bool AllowNull
		{
			get { return this.allowNull; }
			set { this.allowNull = value; }
		}

		#endregion  // AllowNull

		#endregion  // Public Properties

		#region Internal Properties

		#region CellReferenceMode

		internal CellReferenceMode CellReferenceMode
		{
			get
			{
				if (this.ParentCollection == null)
					return CellReferenceMode.A1;

				return this.ParentCollection.Worksheet.CellReferenceMode;
			}
		}

		#endregion  // CellReferenceMode

		// MD 4/9/12 - TFS101506
		#region Culture

		internal CultureInfo Culture
		{
			get
			{
				if (this.ParentCollection == null)
					return CultureInfo.CurrentCulture;

				return this.ParentCollection.Worksheet.Culture;
			}
		}

		#endregion  // Culture

		#region CurrentFormat

		internal WorkbookFormat CurrentFormat
		{
			get
			{
				if (this.ParentCollection == null)
				{
					// MD 2/24/12
					// Found while implementing 12.1 - Table Support
					// We should use the least restrictive format version when there is no workbook, not the most.
					//return WorkbookFormat.Excel97To2003;
					return Workbook.LatestFormat;
				}

				return this.ParentCollection.Worksheet.CurrentFormat;
			}
		}

		#endregion  // CurrentFormat

		#endregion  // Internal Properties

		#endregion  // Properties
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