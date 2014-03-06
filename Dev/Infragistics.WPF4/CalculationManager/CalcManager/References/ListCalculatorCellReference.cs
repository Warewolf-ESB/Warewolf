using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using System.Collections;

namespace Infragistics.Calculations
{
	internal class ListCalculatorCellReference : ItemPropertyReference
	{
		#region Private Members

		private ListCalculatorColumnReference _columnRef;
		private int _index = -1;
		private string _elementName;

		#endregion //Private Members	
    
		#region Constructor

		internal ListCalculatorCellReference(ListCalculatorColumnReference columnRef, object item)
			: base(columnRef.BaseParent as ItemCalculatorReferenceBase, columnRef.ElementName, item, columnRef.ValueAccessor, false)
		{
			_columnRef = columnRef;
		}

		#endregion //Constructor

		#region Properties

		#region Calculator

		internal ListCalculator Calculator { get { return _columnRef.Calculator; } }

		#endregion //Calculator

		#region ColumnRef

		internal ListCalculatorColumnReference ColumnRef { get { return _columnRef; } }

		#endregion //ColumnRef

		#region Index

		internal int Index
		{
			get
			{
				// Optimization. Cache the index since getting the item at an index is
				// much more efficient than calling IndexOf
				IList items = this.Calculator.Items;
				if (_index < 0 || _index >= items.Count || this.Item != items[_index])
				{
					_index = items.IndexOf(this.Item);
					_elementName = null;
				}

				return _index;

			}
		}

		#endregion //Index	
    
		#endregion //Properties

		#region Base class overrides

		#region Properties

		#region AbsoluteName

		public override string AbsoluteName
		{
			get
			{
				return _columnRef.BaseParent.AbsoluteName + RefParser.RefSeperatorString + this.ElementName;
			}
		}

		#endregion AbsoluteName

		#region BaseParent

		public override Engine.RefBase BaseParent
		{
			get
			{
				return _columnRef;
			}
		}

		#endregion //BaseParent

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
				{
					StringBuilder sb = new StringBuilder(4);

					sb.Append(base.ElementName);
					sb.Append(RefParser.RefBeginScope);
					sb.Append(this.Index);
					sb.Append(RefParser.RefEndScope);

					_elementName = sb.ToString();
				}

				return _elementName;
			}
		}

		#endregion //ElementName

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _columnRef.Formula;
			}
		}

		#endregion //Formula	

		#region FormulaSyntaxError






		internal override string FormulaSyntaxError
		{
			get
			{
				return _columnRef.FormulaSyntaxError;
			}
		}

		#endregion // FormulaSyntaxError
 		
		#region FormulaSyntaxErrorValue

		internal override CalculationValue FormulaSyntaxErrorValue { get { return _columnRef.FormulaSyntaxErrorValue; } }

		#endregion //FormulaSyntaxErrorValue	
    
		#region HasFormula







		internal override bool HasFormula
		{
			get
			{
				return _columnRef.HasFormula;
			}
		}

		#endregion // HasFormula

		#region HasFormulaSyntaxError






		internal override bool HasFormulaSyntaxError
		{
			get
			{
				return _columnRef.HasFormulaSyntaxError;
			}
		}

		#endregion // HasFormulaSyntaxError
   
		#region IsDataReference

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		#endregion //IsDataReference	

		#endregion // Properties

		#region Methods

		#region CreateReference

		public override ICalculationReference CreateReference(string inReference)
		{
			// see if the associated reference is a column ref
			ListCalculatorColumnReference columnRef = this._columnRef.Calculator.GetReference(inReference, true, true, false) as ListCalculatorColumnReference;

			// if so then return the corresponding cell
			if (columnRef != null)
				return columnRef.GetCellRef(this.Item);

			return base.CreateReference(inReference);
		}

		#endregion //CreateReference	

		#region ContainsReference

		public override bool ContainsReference(ICalculationReference inReference)
		{
			// since Contains really means intersects with we can just call the
			// column's implementation of this method
			return _columnRef.ContainsReference(inReference);
		}

		#endregion //ContainsReference	

		#region FindItem

		public override ICalculationReference FindItem(string name)
		{
			// call FindItem on our parent column ref
			ICalculationReference refernce = _columnRef.FindItem(name);

			ListCalculatorColumnReference columnRef = refernce as ListCalculatorColumnReference;

			// see if the associated reference is a column ref
			if (columnRef != null && columnRef.BaseParent == this._columnRef.BaseParent)
				return columnRef.GetCellRef(this.Item);

			return refernce;
		}
    
		public override ICalculationReference FindItem(string name, int index, bool isRelative)
		{
			// call FindItem on our parent column ref
			return _columnRef.FindItem(name, index, isRelative);
		}
		public override ICalculationReference FindItem(string name, string index)
		{
			// call FindItem on our parent column ref
			return _columnRef.FindItem(name, index);
		}

		#endregion //FindItem	
		
		#region FindAll

		public override ICalculationReference FindAll(string name)
		{
			// call FindItem on our parent column ref
			return _columnRef.FindAll(name);
		}

		#endregion //FindAll

		#region IsSiblingReference

		public override bool IsSiblingReference(ICalculationReference reference)
		{
			ListCalculatorCellReference cellref = reference as ListCalculatorCellReference;

			// if the passed in ref is a cell reference from the same calculator for
			// the same item then it is a sibling reference
			if (cellref != null)
				return cellref.Calculator == this.Calculator && cellref.Item == this.Item;

			RefUnAnchored ru = reference as RefUnAnchored;

			if (ru != null)
			{
				ListCalculatorColumnReference columnRef = ru.WrappedReference as ListCalculatorColumnReference;

				if (columnRef != null)
					return columnRef == this._columnRef;
			}

			return base.IsSiblingReference(reference);

		}

		#endregion //IsSiblingReference	
    
		#region IsSubsetReference

		public override bool IsSubsetReference(ICalculationReference inReference)
		{
			ListCalculatorCellReference cellRef = inReference as ListCalculatorCellReference;

			if (cellRef != null)
				return cellRef == this;

			return base.IsSubsetReference(inReference);
		}

		#endregion //IsSubsetReference	
    
		#region ResolveReference

		public override ICalculationReference ResolveReference(ICalculationReference reference, ResolveReferenceType referenceType)
		{
			ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

			if (columnRef != null && columnRef.BaseParent == this._columnRef.BaseParent)
				return columnRef.GetCellRef(this.Item);

			RefUnAnchored ru = reference as RefUnAnchored;

			if (ru != null)
			{
				columnRef = ru.WrappedReference as ListCalculatorColumnReference;

				RefParser parser = ru.RelativeReference;

				if (columnRef != null && parser != null)
				{
					// if the reference is for all the cells in the column then return the column ref
					if (parser.HasScopeAll)
						return columnRef;

					if (parser.TupleCount > 0)
					{
						RefTuple tuple = parser.LastTuple;

						int index = -1;
						if (parser.HasRelativeIndex)
						{
							// Since the index is a relative index add it to this cell's index
							int thisIndex = this.Index;

							if (thisIndex >= 0)
								index = tuple.ScopeIndex + this.Index;
						}
						else
						{
							// use the index as the absolute index.
							index = tuple.ScopeIndex;
						}

						if (index >= 0 && index < this.Calculator.Items.Count)
							return columnRef.GetCellRefAtIndex(index);
						else
							return new CalculationReferenceError(columnRef.AbsoluteName, SRUtil.GetString("CellIndexOutOfRange", index, columnRef.AbsoluteName));
					}
				}
			}

			return base.ResolveReference(reference, referenceType);
		}

		#endregion //ResolveReference	

		#endregion // Methods
    
		#endregion // Base class overrides



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