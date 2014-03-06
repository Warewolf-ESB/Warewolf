using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal sealed class ExcelCalcFormula : IExcelCalcFormula
	{
		#region Member Variables

		private IExcelCalcReference baseReference;
		private List<UltraCalcFormulaToken> convertedTokens;
		private int dependencySortNumber = -1;
		private bool dynamicReferenceIsDirty;
		private DynamicReferencesCollection dynamicReferences;
		private Formula excelFormula;
		private bool isArrayFormula;
		private bool isCircularFormula;
		private bool isNamedReferenceCircularFormula;
		private int iterationCount;
		private int originalDependencySortNumber;
		private UltraCalcFormulaReferenceCollection references;
		private DynamicReferencesCollection staleDynamicReferences;
		private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		public ExcelCalcFormula(Formula excelFormula, IExcelCalcReference baseReference, Workbook workbook)
		{
			this.baseReference = baseReference;
			this.excelFormula = excelFormula;
			this.workbook = workbook;

			this.convertedTokens = CalcEngineTokenConverter.ConvertTokens(this, this.workbook);
			this.IsArrayFormula = excelFormula is ArrayFormula;
		}

		#endregion Constructor

		#region Methods

		#region AddDynamicReference






		public bool AddDynamicReference(IExcelCalcReference dynamicReference)
		{
			dynamicReference = ExcelCalcEngine.GetResolvedReference(dynamicReference);

			// MD 1/25/12 - TFS100119
			// Reference errors cannot be dynamic references.
			if (dynamicReference is ExcelReferenceError)
				return false;

			// If the reference is in the static references collection already, we don't need to do anything.
			if (this.ReferencesInternal.Contains(dynamicReference, false))
				return false;

			// If the reference is in the dynamic references collection already, we don't need to do anything.
			if (this.DynamicReferences.Contains(dynamicReference, false))
				return false;

			// Add the reference to the dynamic references collection.
			this.DynamicReferences.Add(dynamicReference);

			if (this.staleDynamicReferences != null && this.staleDynamicReferences.Contains(dynamicReference, true))
			{
				// Remove it from the stale collection if it exists: the reference is no longer stale.
				this.staleDynamicReferences.Remove(dynamicReference);

				// The stale reference was not yet removed from the ancestor map, so we don't need to try to add it 
				// anymore, so just return.
				return false;
			}

			ExcelCalcEngine calcEngine = this.CalcEngine;

			if (calcEngine != null)
			{
				// Add the dependency to the ancestor map of the calc engine.
				bool predecessorIsDirty = true;
				calcEngine.AddDynamicPredecessor(dynamicReference, this.BaseReference, out predecessorIsDirty);

				// If the dynamic reference needed to be added and was dirty, set the flag indicating this on the formula.
				if (predecessorIsDirty)
					this.dynamicReferenceIsDirty = true;
			}

			return true;
		}

		#endregion AddDynamicReference

		#region Evaluate



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public ExcelCalcValue Evaluate(IExcelCalcReference reference)
		{
			// MD 8/20/08 - Excel formula solving
			// Named reference formulas must be reevaluated for each cell anyway, so don't do anything if the value is being solved for the named reference.
			// MD 3/30/11 - TFS69969
			// Check for the common base class of NamedCalcReference and ExternalNamedCalcReference because we don't want to solve the formula for either.
			//if ( ExcelCalcEngine.GetResolvedReference( reference ) is NamedCalcReference )
			if (ExcelCalcEngine.GetResolvedReference(reference) is NamedCalcReferenceBase)
				return null;

			// MD 7/21/08 - Excel formula solving
			// Moved all code to new overload except the code that initializes the new number stack.

			// Initialize Number Stack
			ExcelCalcNumberStack NumberStack = new ExcelCalcNumberStack();

			// MD 7/16/08 - Excel formula solving
			// Some functions need a reference to the reference owning the formula.
			NumberStack.FormulaOwner = this.BaseReference;

			return this.EvaluateHelper(reference, NumberStack);
		}

		// MD 7/21/08 - Excel formula solving
		// Created helper method out of the original Evaluate method. This does everything Evaluate did except intialize the
		// numnber stack. It is passed to this helper method. This is so the same number stack can be used to evaluate an
		// ExcelCalcFormula if it is a value on the number stack.
		// MD 8/20/08 - Excel formula solving
		// Added another parameter to indicate whether or not to dereference the cell.
		//internal ExcelCalcValue EvaluateHelper( IExcelCalcReference reference, ExcelCalcNumberStack NumberStack )
		internal ExcelCalcValue EvaluateHelper(IExcelCalcReference reference, ExcelCalcNumberStack NumberStack)
		{
			// MD 3/2/12 - 12.1 - Table Support
			// When there is a circularity of all named references, they return #NAME? errors.
			if (this.IsNamedReferenceCircularFormula)
				return new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Name));

			// MD 8/21/08 - Excel formula solving
			// Store the original count so we can verify the number stack count later
			int originalCount = NumberStack.Count();

			// Initialize result value to error.
			ExcelCalcValue Value = new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Num));

			try
			{
				// Loop over the token collection applying operators
				foreach (UltraCalcFormulaToken token in this.Tokens)
				{
					// If (Token Type is an operand) {
					if (token.Type == UltraCalcFormulaTokenType.Value)
					{
						// MD 7/12/12 - TFS109194
						// Unwrapping and re-wrapping references was needed for the other calc managers, but not for Excel formula
						// solving. In addition, we were losing some state from the ExcelCalcValue by doing this. Since we no longer
						// share code with the calc managers, we can stop doing this.
						#region Old Code

						//// Reference value?
						//if (((UltraCalcValueToken)token).Value.IsReference)
						//{
						//    // Push a reference value that has been resolved against its base reference
						//    NumberStack.Push(new ExcelCalcValue(((UltraCalcValueToken)token).Value.ToReference()));
						//}
						//else
						//{
						//    // Push the non-reference token
						//    NumberStack.Push(((UltraCalcValueToken)token).Value);
						//}

						#endregion // Old Code
						NumberStack.Push(((UltraCalcValueToken)token).Value);
					}
					else
					{
						// Evaluate the function against its args pushed onto the number stack
						// MD 4/10/12 - TFS108678
						// For Excel, the number stack needs a reference to the function being solved.
						//((ExcelCalcFunctionToken)token).Evaluate(NumberStack);
						UltraCalcFunctionToken functionToken = (UltraCalcFunctionToken)token;
						NumberStack.CurrentFunctionToken = functionToken;

						functionToken.Evaluate(NumberStack);

						NumberStack.CurrentFunctionToken = null;
					}
				}

				// Pop result of number stack
				Value = NumberStack.Pop();

				//???? TEMP FIX TO CONVERT ULTRACALCERROREXCEPTION
				if (Value.Value is UltraCalcErrorException)
					Value = new ExcelCalcValue(new ExcelCalcErrorValue(((UltraCalcErrorException)Value.Value).Value.Code));

				// MD 8/21/08 - Excel formula solving
				//Debug.Assert(NumberStack.Count() == 0);
				Debug.Assert(NumberStack.Count() == originalCount, "The number stack was not cleared after evaluating the formula.");
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				Value = new ExcelCalcValue(e);
			}

			// Pop Result Value off number stack and return it
			return Value;
		}

		#endregion //Evaluate

		#region IncrementIterationCount






		public void IncrementIterationCount()
		{
			this.iterationCount++;
		}

		#endregion IncrementIterationCount

		#region OnAfterEvaluate






		public void OnAfterEvaluate(ExcelCalcEngine calcEngine)
		{
			if (this.staleDynamicReferences == null)
				return;

			if (calcEngine != null)
			{
				// Anything left in the stale collection should be removed from the ancestor map
				foreach (IExcelCalcReference staleReference in this.staleDynamicReferences)
					calcEngine.RemoveAncestorMapEntry(staleReference, this.BaseReference);
			}

			// Clear the stale references collection
			this.staleDynamicReferences = null;
		}

		#endregion OnAfterEvaluate

		#region OnBeforeEvaluate






		public void OnBeforeEvaluate()
		{
			// Reset the flag indicating whether dynamic references are dirty.
			this.dynamicReferenceIsDirty = false;

			// Move dynamic references to the stale collection.
			this.staleDynamicReferences = this.dynamicReferences;

			// Clear the dynamic references collection.
			this.dynamicReferences = null;
		}

		#endregion OnBeforeEvaluate

		#region ResetCircularFormula






		public void ResetCircularFormula()
		{
			Debug.Assert(this.isCircularFormula, "This should only be called on circular formulas.");
			this.dependencySortNumber = this.originalDependencySortNumber;
			this.iterationCount = 0;
		}

		#endregion ResetCircularFormula

		#region SetDependancySortNumberForCircularReference






		public void SetDependancySortNumberForCircularReference(int newDependancySortNumber)
		{
			Debug.Assert(this.isCircularFormula, "This should only be called on circular formulas.");
			this.dependencySortNumber = newDependancySortNumber;
		}

		#endregion SetDependancySortNumberForCircularReference

		#endregion // Methods

		#region Properties

		#region BaseReference

		public IExcelCalcReference BaseReference
		{
			get { return this.baseReference; }
		}

		#endregion BaseReference

		
		#region CalcEngine






		private ExcelCalcEngine CalcEngine
		{
			get
			{
				ExcelRefBase excelRef = ExcelCalcEngine.GetResolvedReference(this.BaseReference) as ExcelRefBase;

				if (excelRef != null)
					return excelRef.Workbook.CalcEngine;

				return null;
			}
		}

		#endregion CalcEngine

		#region DependencySortNumber







		internal int DependencySortNumber
		{
			get
			{
				return this.dependencySortNumber;
			}
			set
			{
				this.dependencySortNumber = value;
				this.originalDependencySortNumber = dependencySortNumber;
			}
		}

		#endregion // DependencySortNumber

		#region DynamicReferenceIsDirty






		internal bool DynamicReferenceIsDirty
		{
			get { return this.dynamicReferenceIsDirty; }
		}

		#endregion DynamicReferenceIsDirty

		#region DynamicReferences






		internal DynamicReferencesCollection DynamicReferences
		{
			get
			{
				if (this.dynamicReferences == null)
				{
					// MD 1/25/12 - TFS100119
					// Use a new collection which is optimized for excel references.
					//this.dynamicReferences = new ExcelCalcFormulaReferenceCollection();
					this.dynamicReferences = new DynamicReferencesCollection();
				}

				return this.dynamicReferences;
			}
		}

		IExcelCalcReferenceCollection IExcelCalcFormula.DynamicReferences
		{
			get { return this.DynamicReferences; }
		}

		#endregion DynamicReferences

		#region ExcelFormula

		public Formula ExcelFormula
		{
			get { return this.excelFormula; }
		}

		#endregion ExcelFormula

		#region FormulaString

		public string FormulaString
		{
			get { return this.excelFormula.ToString(); }
		}

		#endregion FormulaString

		#region HasAlwaysDirty

		public bool HasAlwaysDirty
		{
			get { return this.excelFormula.RecalculateAlways; }
		}

		#endregion HasAlwaysDirty

		#region IsArrayFormula






		internal bool IsArrayFormula
		{
			get { return this.isArrayFormula; }
			set
			{
				Debug.Assert(value == false || this.BaseReference is CellCalcReference, "Array formulas should only be set on cells.");
				this.isArrayFormula = value;
			}
		}

		#endregion IsArrayFormula

		#region IsCircularFormula






		public bool IsCircularFormula
		{
			get { return this.isCircularFormula; }
			set { this.isCircularFormula = value; }
		}

		#endregion IsCircularFormula

		#region IsNamedReferenceCircularFormula

		public bool IsNamedReferenceCircularFormula
		{
			get { return isNamedReferenceCircularFormula; }
			set { isNamedReferenceCircularFormula = value; }
		}

		#endregion // IsNamedReferenceCircularFormula

		#region IterationCount






		public int IterationCount
		{
			get { return this.iterationCount; }
		}

		#endregion IterationCount

		#region References

		public IExcelCalcReferenceCollection References
		{
			get { return this.ReferencesInternal; }
		}

		internal UltraCalcFormulaReferenceCollection ReferencesInternal
		{
			get
			{
				if (this.references == null)
					this.references = new UltraCalcFormulaReferenceCollection((ExcelCalcFormula)this);

				return this.references;
			}
		}

		#endregion //References

		#region Tokens

		internal List<UltraCalcFormulaToken> Tokens
		{
			get { return this.convertedTokens; }
		}

		#endregion Tokens

		
		#region Workbook

		public Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

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