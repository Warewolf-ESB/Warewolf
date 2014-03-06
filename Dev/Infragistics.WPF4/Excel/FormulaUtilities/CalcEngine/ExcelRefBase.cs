using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal abstract class ExcelRefBase : RefBase
	{
		#region Member Variables

		private bool addDynamicReferences;
		private Formula excelFormula;
		private TokenClass expectedParameterClass;
		private ExcelCalcFormula formula;
		private ExcelCalcNumberStack numberStack;
		private ExcelCalcValue value;

		#endregion Member Variables

		#region Constructor

		protected ExcelRefBase() { }

		#endregion Constructor

		#region Base Class Overrides

		#region AbsoluteName

		public sealed override string AbsoluteName
		{
			get
			{
				return "//" + this.ElementName;
			}
		}

		#endregion AbsoluteName

		#region CreateReference

		public sealed override IExcelCalcReference CreateReference( string inReference )
		{
			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//return CalcUtilities.GetReference( inReference, this.Cell, this.Worksheet, this.Workbook );
			return CalcUtilities.GetReference(inReference, this.Row, this.ColumnIndex, this.Worksheet, this.Workbook);
		} 

		#endregion CreateReference

		#region Formula

		public sealed override ExcelCalcFormula Formula
		{
			get { return this.formula; }
		}

		#endregion Formula

		#region NormalizedAbsoluteName

		public override string NormalizedAbsoluteName
		{
			get { return this.AbsoluteName.ToLower(); }
		} 

		#endregion NormalizedAbsoluteName

		#region Value

		public override ExcelCalcValue Value
		{
			get
			{
				// MD 7/25/08 - Excel formula solving
				// Whenever we dereference a cell or named reference, make sure it is added to the dynamic references 
				// of the formula if it was not already in the static references.
				if ( this.AddDynamicReferences &&
					this.NumberStack != null &&
					this.NumberStack.FormulaOwner != null )
				{
					this.NumberStack.FormulaOwner.Formula.AddDynamicReference( this );
				}

				ExcelCalcValue value = this.ValueInternal;

				if ( value != null )
				{
					value.ExpectedTokenClass = this.ExpectedParameterClass;
					value.Owner = this.NumberStack;
				}
				else
				{
					// MD 11/23/11
					// Found while fixing TFS96468
					// We should have a singleton value to represent a null value. It wastes space to create a new calc value for each null value.
					//value = new ExcelCalcValue();
					value = ExcelCalcValue.Empty;
				}

				return value;
			}
			set
			{
				object resolvedValue = value.GetResolvedValue();

				// If a formula evaluates to null, a zero is shown in the cell
				if ( resolvedValue == null || resolvedValue is DBNull )
					resolvedValue = 0d;

				// It might seem like extra work to recreate the calc value always here, but the calc value might get unwrapped and dereferenced 
				// when it is resolved and we want to store the final value on the cell.
				this.ValueInternal = new ExcelCalcValue( resolvedValue );
			}
		}

		#endregion Value 

		#endregion Base Class Overrides

		#region Methods

		// MD 12/1/11 - TFS96113
		// This is no longer needed now that we use a derived ArrayProxy for regions references.
		#region Removed

		//#region EnsureArrayValuesCreated

		//public virtual void EnsureArrayValuesCreated() { }

		//#endregion EnsureArrayValuesCreated

		#endregion // Removed

		#region EnsureCalculated

		internal void EnsureCalculated()
		{
			// MD 11/1/10 - TFS56976
			// There's no reason to make this virtual call twice. Call it once and cache it.
			//if ( this.Workbook != null )
			//    this.Workbook.EnsureCalculated( this );
			Workbook workbook = this.Workbook;
			if (workbook != null)
				workbook.EnsureCalculated(this);
		} 

		#endregion EnsureCalculated

		// MD 2/24/12 - 12.1 - Table Support
		#region GetRegionGroup

		public virtual IList<WorksheetRegion> GetRegionGroup()
		{
			return null;
		} 

		#endregion // GetRegionGroup

		#region RemoveFormula

		// MD 7/19/12 - TFS116808 (Table resizing)
		// Added the ability to leave the compiled ExcelCalcFormula in the Formula's compiledFormulas collection temporarily.
		//private void RemoveFormula( bool canClearPreviouslyCalculatedValue )
		private void RemoveFormula(bool canClearPreviouslyCalculatedValue, bool shouldRemoveCompiledFormula)
		{
			if ( this.formula == null )
				return;

			Workbook workbook = this.Workbook;

			if ( workbook == null )
				return;

			// MD 2/28/12 - 12.1 - Table Support
			//IExcelCalcFormula oldFormula = this.formula;
			ExcelCalcFormula oldFormula = this.formula;

			// MD 7/19/12 - TFS116808 (Table resizing)
			//if (oldFormula != null && oldFormula.ExcelFormula != null)
			if (shouldRemoveCompiledFormula && oldFormula != null && oldFormula.ExcelFormula != null)
				oldFormula.ExcelFormula.RemoveCompiledFormula(oldFormula);

			// MD 3/2/12 - 12.1 - Table Support
			// Use the SetFormula virtual method so derived classes can override it and respond to changes in the formula.
			//this.formula = null;
			this.SetFormula(null);

			this.excelFormula = null;

			workbook.RemoveFormula( oldFormula );

			if ( workbook.ReferencesRequiringFormulaCompilation != null )
				workbook.ReferencesRequiringFormulaCompilation.Remove( this );

			// If the formula is remove and the old value can be cleared, set it to null
			if ( canClearPreviouslyCalculatedValue )
				this.value = null;
		}

		#endregion RemoveFormula

		#region ResolveReference

		// MD 3/2/12 - 12.1 - Table Support
		//public ExcelRefBase ResolveReference( IExcelCalcReference formulaOwner, TokenClass expectedTokenClass, out ExcelCalcErrorValue errorValue )
		// MD 4/10/12 - TFS108678
		//public virtual ExcelRefBase ResolveReference( IExcelCalcReference formulaOwner, TokenClass expectedTokenClass, out ExcelCalcErrorValue errorValue )
		public virtual ExcelRefBase ResolveReference(IExcelCalcReference formulaOwner, TokenClass expectedTokenClass, bool splitArraysForValueParameters, out ExcelCalcErrorValue errorValue)
		{
			errorValue = null;

			if ( expectedTokenClass != TokenClass.Value )
				return this;

			CellCalcReference cellReference = formulaOwner as CellCalcReference;
			
			if ( cellReference == null || cellReference.HasArrayFormula == false )
				return this;

			// MD 4/10/12 - TFS108678
			if (splitArraysForValueParameters == false)
				return this;

			return this.ResolveReferenceForArrayFormula( cellReference, out errorValue );
		} 

		#endregion ResolveReference

		#region ResolveReferenceForArrayFormula

		protected virtual ExcelRefBase ResolveReferenceForArrayFormula( CellCalcReference formulaOwner, out ExcelCalcErrorValue errorValue )
		{
			errorValue = null;
			return this;
		} 

		#endregion ResolveReferenceForArrayFormula

		#region SetAndCompileFormula

		internal void SetAndCompileFormula( Formula newExcelFormula, bool canClearPreviouslyCalculatedValue )
		{
			// MD 2/28/12 - 12.1 - Table Support
			// Moved all code to the new overload.
			// MD 7/19/12 - TFS116808 (Table resizing)
			//this.SetAndCompileFormula(newExcelFormula, canClearPreviouslyCalculatedValue, false);
			this.SetAndCompileFormula(newExcelFormula, canClearPreviouslyCalculatedValue, false, true);
		}

		// MD 2/28/12 - 12.1 - Table Support
		// MD 7/19/12 - TFS116808 (Table resizing)
		// Added the ability to leave the old compiled ExcelCalcFormula in the Formula's compiledFormulas collection temporarily.
		//internal void SetAndCompileFormula(Formula newExcelFormula, bool canClearPreviouslyCalculatedValue, bool forceSet)
		internal void SetAndCompileFormula(Formula newExcelFormula, bool canClearPreviouslyCalculatedValue, bool forceSet, bool shouldRemoveCompiledFormulas)
		{
			// If the formula hasn't changed, nothing needs to be done
			// MD 2/28/12 - 12.1 - Table Support
			//if ( newExcelFormula == this.excelFormula )
			if (forceSet == false && newExcelFormula == this.excelFormula)
				return;

			if ( this.CanOwnFormula == false )
			{
				Utilities.DebugFail( "This reference type cannot own a formula." );
				return;
			}

			
			// Remove the old formula from the reference.
			// MD 7/19/12 - TFS116808 (Table resizing)
			// Added the ability to leave the old compiled ExcelCalcFormula in the Formula's compiledFormulas collection temporarily.
			//this.RemoveFormula( canClearPreviouslyCalculatedValue );
			this.RemoveFormula(canClearPreviouslyCalculatedValue, shouldRemoveCompiledFormulas);

			Workbook workbook = this.Workbook;

			// If the formula has been cleared or for some reason we cannot get a reference to the workbook, we shouldn't do anything else.
			if ( newExcelFormula == null || workbook == null )
				return;

			// If the calculations are currently suspended, let the workbook know that this reference should have its formula compiled and 
			// applied when the calculations are resumed and then return so we don't compile it now.
			if ( workbook.AreCalculationsSuspended && workbook.ReferencesRequiringFormulaCompilation != null )
			{
				workbook.ReferencesRequiringFormulaCompilation[ this ] = newExcelFormula;
				return;
			}

			ExcelCalcFormula calcFormula = workbook.CompileFormula( this, newExcelFormula );

			// If there was a problem compiling the formula, nothing else can be done.
			if ( calcFormula == null )
				return;

			// MD 2/28/12 - 12.1 - Table Support
			newExcelFormula.AddCompiledFormula(calcFormula);

			// MD 3/2/12 - 12.1 - Table Support
			// Use the SetFormula virtual method so derived classes can override it and respond to changes in the formula.
			// Also, the call to Workbook.CompileFormula should really be setting the IsArrayFormula value.
			//this.formula = calcFormula;
			//this.formula.IsArrayFormula = newExcelFormula is ArrayFormula;
			this.SetFormula(calcFormula);

			this.excelFormula = newExcelFormula;

			// Add the formula to the workbook's calc network.
			workbook.AddFormula( calcFormula );

			// When manual calculated are enabled, the formula should be calculated only once when first applied to a cell.
			if ( canClearPreviouslyCalculatedValue &&
				workbook.AreCalculationsSuspended == false &&
				workbook.CalculationMode == CalculationMode.Manual )
			{
				this.Value = calcFormula.Evaluate( this );
			}
		}

		#endregion SetAndCompileFormula 

		// MD 3/2/12 - 12.1 - Table Support
		#region SetFormula

		protected virtual void SetFormula(ExcelCalcFormula formula)
		{
			this.formula = formula;
		}

		#endregion // SetFormula

		#endregion Methods

		#region Properties

		#region AddDynamicReferences







		internal bool AddDynamicReferences
		{
			get { return this.addDynamicReferences; }
			set { this.addDynamicReferences = value; }
		} 

		#endregion AddDynamicReferences

		#region CanOwnFormula

		public virtual bool CanOwnFormula { get { return false; } } 

		#endregion CanOwnFormula

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//#region Cell
		//
		//public abstract WorksheetCell Cell { get; } 
		//
		//#endregion Cell

		// MD 4/12/11 - TFS67084
		#region ColumnIndex

		public abstract short ColumnIndex { get; }

		#endregion ColumnIndex

		#region ExcelFormula

		public Formula ExcelFormula
		{
			get { return this.excelFormula; }
		} 

		#endregion ExcelFormula

		#region ExpectedParameterClass

		public TokenClass ExpectedParameterClass
		{
			get { return this.expectedParameterClass; }
			set { this.expectedParameterClass = value; }
		}

		#endregion ExpectedParameterClass

		#region NumberStack

		public ExcelCalcNumberStack NumberStack
		{
			get { return this.numberStack; }
			set { this.numberStack = value; }
		}

		#endregion NumberStack

		// MD 4/12/11 - TFS67084
		#region Row

		public abstract WorksheetRow Row { get; }

		#endregion Row

		#region ValueInternal

		protected virtual ExcelCalcValue ValueInternal
		{
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion ValueInternal

		#region ValueMember

		internal ExcelCalcValue ValueMember
		{
			get { return this.value; }
		} 

		#endregion  // ValueMember

		#region Workbook

		public virtual Workbook Workbook
		{
			get
			{
				Worksheet worksheet = this.Worksheet;

				if ( worksheet == null )
					return null;

				return worksheet.Workbook;
			}
		} 

		#endregion Workbook

		#region Worksheet

		public Worksheet Worksheet
		{
			get 
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell cell = this.Cell;
				//
				//if ( cell == null )
				//    return null;
				//
				//return cell.Worksheet; 
				WorksheetRow row = this.Row;

				if (row == null)
					return null;

				return row.Worksheet; 
			}
		}

		#endregion Worksheet

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