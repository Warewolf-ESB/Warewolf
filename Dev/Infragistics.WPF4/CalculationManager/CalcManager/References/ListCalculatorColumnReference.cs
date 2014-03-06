using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using Infragistics.Collections;
using System.Collections;

namespace Infragistics.Calculations
{
	internal class ListCalculatorColumnReference : FormulaRefBase
	{
		#region Private Members

		private ListCalculatorReference _root;
		private ItemCalculation _calculation;
		private ListCalculator _calculator;
		private IItemPropertyValueAccessor _valueAccessor;
		private string _referenceId;
		private Dictionary<object, ListCalculatorCellReference> _cellReferences = new Dictionary<object, ListCalculatorCellReference>();
		private string _elementName;
		private bool _hasRelativeOrAbsoluteRefs;
		private bool _hasAllrefs;
		private bool _hasErrorBeenRaised;

		#endregion //Private Members	
    
		#region Constructor

		internal ListCalculatorColumnReference( ListCalculatorReference root, ItemCalculation calculation, string refernceId, IItemPropertyValueAccessor valueAccessor)
			: base(root.Calculator.CalculationManager)
		{
			_root = root;
			_calculation = calculation;
			_calculator = _root.Calculator as ListCalculator;
			_referenceId = refernceId;
			_valueAccessor = valueAccessor;
		}

		#endregion //Constructor

		#region Properties

		#region Calculation

		internal ItemCalculation Calculation { get { return _calculation; } }

		#endregion //Calculation

		#region Calculator

		internal ListCalculator Calculator { get { return _calculator; } }

		#endregion //Calculator
		
		#region ValueAccessor

		internal IItemPropertyValueAccessor ValueAccessor { get { return _valueAccessor; } }

		#endregion //ValueAccessor	
        
		#endregion //Properties	
 
		#region Methods

		#region Internal Methods

		#region GetAllCellsReference

		internal RefUnAnchored GetAllCellsReference()
		{
			RefUnAnchored ru = new RefUnAnchored(this);

			ru.RelativeReference = new RefParser(string.Format("{0}(*)", this.AbsoluteName));

			_hasAllrefs = true;

			return ru;
		}

		#endregion //GetAllCellsReference

		#region GetCellRef

		internal ListCalculatorCellReference GetCellRef(object item, bool createifNotFound = true)
		{
			if (item == null)
				return null;

			ListCalculatorCellReference cellRef;

			if (!_cellReferences.TryGetValue(item, out cellRef) && createifNotFound)
			{
				// call VerifyValueAccessor which will make sure we have an ValueAccessor if a TargetProperty was 
				// specified. If not it will raise the DataRerror event if it hasn't already been raised
				this.VerifyValueAccessor(item);

				cellRef = new ListCalculatorCellReference(this, item);

				_cellReferences.Add(item, cellRef);
			}

			return cellRef;
		}

		#endregion //GetCellRefAtIndex

		#region GetCellRefAtIndex

		internal ListCalculatorCellReference GetCellRefAtIndex(int index)
		{
			return GetCellRef( _calculator.Items[index]);
		}

		#endregion //GetCellRefAtIndex

		#region GetCellAtRelativeOrAbsoluteIndex

		internal ICalculationReference GetCellAtRelativeOrAbsoluteIndex(int index, bool isRelative)
		{
			// for absolute indices return the appropriate cell ref
			if (isRelative == false)
			{
				if (index >= 0 && index < this.Calculator.Items.Count)
				{
					this._hasRelativeOrAbsoluteRefs = true;
					return this.GetCellRefAtIndex(index);
				}

				return null;
			}

			RefUnAnchored wrapper = new RefUnAnchored(this);

			if (index > 0)
				wrapper.RelativeReference = new RefParser(string.Format("{0}(+{1})", this.AbsoluteName, index));
			else
				wrapper.RelativeReference = new RefParser(string.Format("{0}({1})", this.AbsoluteName, index));

			this._hasRelativeOrAbsoluteRefs = true;

			return wrapper;
		}

		#endregion //GetCellAtRelativeOrAbsoluteIndex	
    
		#region OnItemsAdded

		internal void OnItemsAdded(IList items)
		{
			XamCalculationManager mgr = this.CalculationManager;

			if (!mgr.IsInitialized)
				return;

			int count = items.Count;

			for (int i = 0; i < count; i++)
			{
				object item = items[i];

				ListCalculatorCellReference cellRef = this.GetCellRef(item);

				if (cellRef != null)
				{
					mgr.InternalAddReference(cellRef);
				}
			}

			this.OnItemsAddedOrRemoved();
		}

		#endregion //OnItemsAdded

		#region OnItemsRemoved

		internal void OnItemsRemoved(IList items)
		{
			int count = items.Count;

			for (int i = 0; i < count; i++)
			{
				object item = items[i];

				ListCalculatorCellReference cellRef;

				if (_cellReferences.TryGetValue(item, out cellRef))
				{
					cellRef.Dispose(true);
					_cellReferences.Remove(item);
				}
			}

			this.OnItemsAddedOrRemoved();
		}

		#endregion //OnItemsRemoved

		#endregion //Internal Methods

		#region Private Methods

		#region OnItemsAddedOrRemoved

		private void OnItemsAddedOrRemoved()
		{
			XamCalculationManager mgr = this.CalculationManager;

			if (_hasRelativeOrAbsoluteRefs)
				((ICalculationManager)mgr).RowsCollectionReferenceResynched(this);

			if (_hasAllrefs)
				mgr.InternalNotifyValueChanged(this);
		}

		#endregion //OnItemsAddedOrRemoved

		#region VerifyValueAccessor

		private void VerifyValueAccessor(object item)
		{
			if (_valueAccessor != null || _hasErrorBeenRaised)
				return;

			// if a target property is specified and 
			if (_calculation != null && _calculation.HasTargetProperty)
			{
				_valueAccessor = _calculator.GetAccessorForProp(_calculation.TargetProperty);

				if (_valueAccessor == null)
				{
					_hasErrorBeenRaised = true;

					_root.Calculator.RaiseTargetPropertyNotFoundError(_calculation.TargetProperty, item);
				}
			}
		}

		#endregion //VerifyValueAccessor	

		#endregion //Private Methods	
        
		#endregion //Methods

		#region Base class overrides

		#region Properties

		#region AbsoluteName

		public override string AbsoluteName
		{
			get
			{
				return this.BaseParent.AbsoluteName + RefParser.RefSeperator + this.ElementName;
			}
		}

		#endregion AbsoluteName

		#region BaseParent

		public override Engine.RefBase BaseParent
		{
			get
			{
				return _root;
			}
		}

		#endregion //BaseParent

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
					_elementName = RefParser.EscapeString(_referenceId, false);

				return _elementName;
			}
		}

		#endregion //ElementName	

		#region IsDataReference

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		#endregion //IsDataReference	
    
		#region IsEnumerable

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion //IsEnumerable

		#region References

		public override ICalculationReferenceCollection References
		{
			get
			{
				return this.ScopedReferences(this.ParsedReference); ;
			}
		}

		#endregion //References	
    
		#endregion // Properties

		#region Methods

		#region CreateReference

		public override ICalculationReference CreateReference(string inReference)
		{
			RefParser refParser = RefParser.Parse(inReference);

			if (refParser.TupleCount == 1)
			{
				if (refParser.IsRoot)
					return this.BaseParent.CreateReference(inReference);

				string name = refParser.LastTuple.Name;
				if (refParser.HasScopeAll)
					return this.FindAll(name);
				if (refParser.HasAbsoluteIndex)
					return this.FindItem(name, refParser.LastTuple.ScopeIndex, false);
				if (refParser.HasRelativeIndex)
					return this.FindItem(name, refParser.LastTuple.ScopeIndex, true);

				ICalculationReference childref = this.Calculator.GetReference(name, true, true, true, true);

				ListCalculatorColumnReference columnRef = childref as ListCalculatorColumnReference;

				if (columnRef != null)
					return columnRef;

				return new CalculationReferenceError(inReference, "Could not find Reference");
			}

			return base.CreateReference(inReference);
		}

		#endregion //CreateReference	
    
		#region ContainsReference

		public override bool ContainsReference(ICalculationReference inReference)
		{
			ListCalculatorColumnReference columnRef = inReference as ListCalculatorColumnReference;

			if (columnRef == null)
			{
				RefUnAnchored ru = inReference as RefUnAnchored;

				if (ru != null)
					columnRef = ru.WrappedReference as ListCalculatorColumnReference;
			}

			if (columnRef != null)
				return columnRef == this;

			ListCalculatorCellReference cellRef = inReference as ListCalculatorCellReference;

			if (cellRef != null)
				return cellRef.ColumnRef == this && cellRef.Index >= 0;

			return (inReference == this.BaseParent);
		}

		#endregion //ContainsReference	
    
		#region FindItem

		public override ICalculationReference FindItem(string name)
		{
			return this.Calculator.GetReference(name);
		}

		public override ICalculationReference FindItem(string name, int index, bool isRelative)
		{
			ICalculationReference cellRef = this.Calculator.GetCellAtIndexReference(name, index, isRelative);

			if (cellRef != null)
				return cellRef;

			return base.FindItem(name, index, isRelative);
		}

		#endregion //FindItem	
    
		#region FindAll

		public override ICalculationReference FindAll(string name)
		{
			ICalculationReference allref = this.Calculator.GetAllCellsInColumnReference(name);

			if (allref != null)
				return allref;

			return base.FindAll(name);
		}

		#endregion //FindAll	

		#region IsSiblingReference

		public override bool IsSiblingReference(ICalculationReference reference)
		{
			ListCalculatorColumnReference columnRef = reference as ListCalculatorColumnReference;

			if (columnRef != null)
				return columnRef.Calculator == this.Calculator;

			return base.IsSiblingReference(reference);
		}

		#endregion //IsSiblingReference	
    
		#region IsSubsetReference

		public override bool IsSubsetReference(ICalculationReference inReference)
		{
			ListCalculatorCellReference cellRef = inReference as ListCalculatorCellReference;

			if (cellRef != null)
				return cellRef.ColumnRef == this && !cellRef.IsDisposedReference;
			
			RefUnAnchored ru = inReference as RefUnAnchored;

			if (ru != null)
			{
				ListCalculatorColumnReference columnRef = ru.WrappedReference as ListCalculatorColumnReference;

				return this == columnRef;
			}

			return base.IsSubsetReference(inReference);
		}

		#endregion //IsSubsetReference	

		#region ScopedReferences

		public override ICalculationReferenceCollection ScopedReferences(RefParser scopeRP)
		{
			this._hasAllrefs = true;

			return new RefCellCollection(this, scopeRP); ;
		}

		#endregion //ScopedReferences	
    
		#endregion // Methods

		#endregion // Base class overrides

		#region RefCellCollection Class

		internal class RefCellCollection : ICalculationReferenceCollection
		{
			#region Private Variables

			private RefParser scopeRP = null;
			private ListCalculatorColumnReference referenceBeingEnumerated = null;

			#endregion // Private Variables

			#region Constructor



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			public RefCellCollection(ListCalculatorColumnReference referenceBeingEnumerated, RefParser scopeRP)
			{
				this.referenceBeingEnumerated = referenceBeingEnumerated;
				this.scopeRP = scopeRP;
			}

			#endregion // Constructor

			#region IEnumerable

			public IEnumerator GetEnumerator()
			{
				return new RefCellCollectionEnumerator(this);
			}

			#endregion //IEnumerable

			#region RefCellCollectionEnumerator

			private class RefCellCollectionEnumerator : IEnumerator
			{
				private RefCellCollection _collection;
				private ListCalculatorCellReference _cellref = null;
				private int _index = -1;
				private int _count;
				internal RefCellCollectionEnumerator(RefCellCollection collection)
				{
					this._collection = collection;
					this.Reset();
				}

				public object Current
				{
					get
					{
						if (null == this._cellref)
							throw new InvalidOperationException();

						return _cellref;
					}
				}

				public bool MoveNext()
				{
					_index++;

					if (_index >=  _count)
						return false;

					this._cellref = _collection.referenceBeingEnumerated.GetCellRefAtIndex(_index);
					return null != this._cellref;
				}

				public void Reset()
				{
					this._cellref = null;
					this._index = -1;
					this._count = _collection.referenceBeingEnumerated._calculator.Items.Count;
				}
			}
			#endregion
		}

		#endregion // RefCellCollection Class
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