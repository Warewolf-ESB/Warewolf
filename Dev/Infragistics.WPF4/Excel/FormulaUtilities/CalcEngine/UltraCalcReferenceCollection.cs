using System;
using System.Collections;
using System.Collections.Generic;

namespace Infragistics.Documents.Excel.CalcEngine
{
	internal class UltraCalcReferenceCollection : IExcelCalcReferenceCollection
	{
		#region Member Variables

		private List<IExcelCalcReference> references = new List<IExcelCalcReference>();

		#endregion //Member Variables

		#region Constructor





		public UltraCalcReferenceCollection() { }







		public UltraCalcReferenceCollection(UltraCalcReferenceCollection copy)
			: this()
		{
			foreach (IExcelCalcReference r in copy)
			{
				this.Add(r);
			}
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Indexer method that returns a <b>IUltraCalcReference</b> by position
		/// </summary>
		public IExcelCalcReference this[int pos]
		{
			get
			{
				if (pos >= 0 && pos < references.Count)
					return (IExcelCalcReference)references[pos];
				else
					throw new IndexOutOfRangeException("Index out of range");
			}
			set { references.Insert(pos, value); }
		}
		#endregion //Properties

		#region Methods

		/// <summary>
		/// Adds the contents of the specified <see cref="UltraCalcReferenceCollection"/> into this instances collection.
		/// </summary>
		/// <param name="merge">Collection whose contents should be merged with this list.</param>
		public void Merge(UltraCalcReferenceCollection merge)
		{
			foreach (IExcelCalcReference r in merge)
			{
				if (this.IndexOf(r) == -1)
					this.Add(r);
			}
		}

		/// <summary>
		/// Add an <b>IUltraCalcReference> elements to the collection</b>
		/// </summary>
		/// <param name="reference">Element to add to collection</param>
		/// <returns>Ordinal Position within collection where element was inserted </returns>
		public int Add(IExcelCalcReference reference)
		{
			references.Add(reference);
			return references.Count - 1;
		}

		/// <summary>
		/// Insert an <b>IUltraCalcReference> elements to the collection</b>
		/// </summary>
		/// <param name="pos">Ordinal postion to insert reference</param>
		/// <param name="reference">Reference element to be inserted into collecction</param>
		public void Insert(int pos, IExcelCalcReference reference)
		{
			references.Insert(pos, reference);
		}

		/// <summary>
		/// Remove <b>IUltraCalcReference</b> element from collection
		/// </summary>
		/// <param name="reference">Element to be removed from collection</param>
		public void Remove(IExcelCalcReference reference)
		{
			int pos = IndexOf(reference);
			if (pos != -1)
				references.RemoveAt(pos);
		}

		/// <summary>
		/// Remove element at given postion from collection
		/// </summary>
		/// <param name="pos">Ordinal index denoting element to remove</param>
		public void RemoveAt(int pos)
		{
			references.RemoveAt(pos);
		}

		/// <summary>
		/// Return ordinal index of given element
		/// </summary>
		/// <param name="reference">Element whose position is desired</param>
		/// <returns>Ordinal index of reference</returns>
		public int IndexOf(IExcelCalcReference reference)
		{
			for (int i = 0; i < references.Count; i++)
			{
				// SSP 9/7/04
				// Perform a case insensitive comparison since absolute names are case insensitive.
				//
				//if (String.Compare(((IUltraCalcReference)references[i]).AbsoluteName, reference.AbsoluteName) == 0)
				if (ExcelCalcEngine.CompareReferences(((IExcelCalcReference)references[i]), reference))
					return i;
			}
			return -1;
		}

		#endregion //Methods

		#region Implementation of ICollection
		/// <summary>
		/// Copys the collection to an array.
		/// </summary>
		/// <param name="array">Array used for the desitnation of the copy.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(System.Array array, int index)
		{
			references.CopyTo(array as IExcelCalcReference[], index); 
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return CalcManagerUtilities.IsSynchronized(references);
			}
		}

		/// <summary>
		/// Returns a number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return references.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return CalcManagerUtilities.SyncRoot(references);
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
			return references.GetEnumerator();
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