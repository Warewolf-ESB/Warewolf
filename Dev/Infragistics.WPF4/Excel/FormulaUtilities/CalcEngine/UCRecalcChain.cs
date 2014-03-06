using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;

namespace Infragistics.Documents.Excel.CalcEngine
{



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

	internal class UCRecalcChain : IExcelCalcReferenceCollection
	{
		#region HSparseArray Class







		private class HSparseArray : SparseArray
		{
			private Dictionary<object, object> table = null;

			internal HSparseArray()
				: base(20, 1.0f, true)
			{
				this.table = new Dictionary<object, object>();
			}

			protected override object GetOwnerData(object item)
			{
				if (this.table.ContainsKey(item))
					return table[item];
				else
					return null;
			}

			protected override void SetOwnerData(object item, object ownerData)
			{
				if (null != ownerData)
					this.table[item] = ownerData;
				else if (this.table.ContainsKey(item))
					this.table.Remove(item);
			}

			internal new IExcelCalcReference this[int index]
			{
				get
				{
					return (IExcelCalcReference)this.GetItem(index);
				}
			}
		}

		#endregion // HSparseArray Class

		#region Private Vars

		private HSparseArray recalcChain;
		private int dirtyChainStartIndex;
		private int recalcChainVersion;
		private CellReferenceLookupTable cellReferenceLookupTable = new CellReferenceLookupTable();

		#endregion // Private Vars

		#region Constructor






		internal UCRecalcChain()
		{
			this.recalcChain = new HSparseArray();
		}

		#endregion // Constructor

		#region DirtyChainStartIndex



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int DirtyChainStartIndex
		{
			get
			{
				return this.dirtyChainStartIndex;
			}
		}

		#endregion // DirtyChainStartIndex

		#region HasDirtyItems






		internal bool HasDirtyItems
		{
			get
			{
				return this.dirtyChainStartIndex < this.Count;
			}
		}

		#endregion // HasDirtyItems

		#region ResetDirtyChainStartIndex






		internal void ResetDirtyChainStartIndex()
		{
			this.dirtyChainStartIndex = this.Count;
		}

		#endregion // ResetDirtyChainStartIndex

		#region BumpRecalcChainVersion







		private void BumpRecalcChainVersion()
		{
			this.recalcChainVersion++;
		}

		#endregion // BumpRecalcChainVersion

		#region Indexer






		internal IExcelCalcReference this[int index]
		{
			get
			{
				return this.recalcChain[index];
			}
		}

		#endregion // Indexer

		#region Add







		internal bool Add(IExcelCalcReference reference)
		{
			if (!(reference is UltraCalcReferenceError) && !this.Contains(reference))
			{
				Debug.Assert(!(reference is UCReference), "An UCReference should not be added to the recalc chain.");
				this.recalcChain.Add(reference);

				// MD 1/12/12 - TFS99279
				// If a cell is being added to the chain, also add it to the cellReferenceLookupTable.
				CellCalcReference cellCalcReference = reference as CellCalcReference;
				if (cellCalcReference != null)
					this.cellReferenceLookupTable.Add(cellCalcReference);
				else
					Debug.Assert(reference is NamedCalcReferenceBase, "We are only expecting cells and named references to be in the recalc chain.");

				this.BumpRecalcChainVersion();
				return true;
			}

			return false;
		}

		#endregion // Add

		#region IndexOf



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int IndexOf(IExcelCalcReference reference)
		{
			return this.recalcChain.IndexOf(reference);
		}

		#endregion // IndexOf

		#region Contains



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool Contains(IExcelCalcReference reference)
		{
			ExcelRefBase excelRef = reference as ExcelRefBase;
			if (excelRef == null)
				return false;

			if (excelRef is NamedCalcReferenceBase)
				return this.IndexOf(reference) >= 0;

			return this.cellReferenceLookupTable.Contains(excelRef);
		}

		#endregion // Contains

		#region IsRecalcListed

		internal bool IsRecalcListed(IExcelCalcReference reference)
		{
			return this.Contains(reference);
		}

		#endregion // IsRecalcListed

		#region RemoveAt







		internal void RemoveAt(int index)
		{
			Debug.Assert(index != -1 && index < this.recalcChain.Count);

			// MD 1/12/12 - TFS99279
			// If a cell is being removed from the chain, also remove it from the cellReferenceLookupTable.
			IExcelCalcReference reference = this.recalcChain[index];
			CellCalcReference cellCalcReference = reference as CellCalcReference;
			if (cellCalcReference != null)
				this.cellReferenceLookupTable.Remove(cellCalcReference);
			else
				Debug.Assert(reference is NamedCalcReferenceBase, "We are only expecting cells and named references to be in the recalc chain.");

			this.recalcChain.RemoveAt(index);

			if (index < this.dirtyChainStartIndex)
				this.dirtyChainStartIndex--;

			this.BumpRecalcChainVersion();
		}

		#endregion // RemoveAt

		#region Remove







		internal void Remove(IExcelCalcReference reference)
		{
			int index = this.IndexOf(reference);
			if (index >= 0)
				this.RemoveAt(index);
		}

		#endregion // Remove

		#region Sort

		// SSP 9/27/04 - Circular Relative Index Support
		// Added Sort method.
		//






		internal void Sort(IComparer comparer)
		{
			this.recalcChain.Sort(comparer);
		}

		#endregion // Sort

		#region RemoveSubsetReferences



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void RemoveSubsetReferences(IExcelCalcReference reference)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (reference.IsSubsetReference(this[i]))
					this.RemoveAt(i);
			}
		}

		#endregion // RemoveSubsetReferences

		#region Implementation of ICollection

		/// <summary>
		/// Copys the collection to an array.
		/// </summary>
		/// <param name="array">Array used for the desitnation of the copy.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(System.Array array, int index)
		{
			recalcChain.CopyTo(array, index);
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return recalcChain.IsSynchronized;
			}
		}

		/// <summary>
		/// Returns a number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return recalcChain.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return recalcChain.SyncRoot;
			}
		}

		#endregion

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns the collection enumerator.
		/// </summary>
		/// <returns>Collection enumerator.</returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return recalcChain.GetEnumerator();
		}

		#endregion
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