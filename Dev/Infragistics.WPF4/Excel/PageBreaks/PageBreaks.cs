using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Base class for the collections of horizontal and vertical page breaks on a <see cref="Worksheet"/>.
	/// </summary>
	/// <typeparam name="T">The type of page break the collection contains.</typeparam>
	/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
	/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
	/// <seealso cref="HorizontalPageBreakCollection"/>
	/// <seealso cref="VerticalPageBreakCollection"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 abstract class PageBreakCollection<T> : IList<T>
		where T : PageBreak
	{
		#region Member Variables

		private PrintOptions owner;
		private List<T> pageBreaks;

		#endregion  // Member Variables

		#region Constructors

		internal PageBreakCollection(PrintOptions owner)
		{
			this.owner = owner;
			this.pageBreaks = new List<T>();
		}

		#endregion  // Constructors

		#region Interfaces

		#region IList<T> Members

		int IList<T>.IndexOf(T item)
		{
			return this.pageBreaks.IndexOf(item);
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new InvalidOperationException(SR.GetString("LE_ArgumentException_PB_CantInsertBreakAtIndex"));
		}

		void IList<T>.RemoveAt(int index)
		{
			this.pageBreaks.RemoveAt(index);
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set
			{
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_PB_CantSetBreakAtIndex"));
			}
		}

		#endregion

		#region ICollection<T> Members

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			this.pageBreaks.CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.pageBreaks.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.pageBreaks.GetEnumerator();
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a page break to the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="pageBreak">The page break to add to the Worksheet.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="pageBreak"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="pageBreak"/> overlaps with another page break already in the collection.
		/// </exception>
		/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetCell)"/>
		/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetColumn)"/>
		/// <seealso cref="PrintOptions.InsertPageBreak(WorksheetRow)"/>
		public void Add(T pageBreak)
		{
			this.AddInternal(pageBreak, true);
		}

		internal bool AddInternal(T pageBreak, bool throwIfPresent)
		{
			if (pageBreak == null)
				throw new ArgumentNullException("pageBreak");

			if (pageBreak.PrintArea != null)
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (pageBreak.PrintArea.Worksheet == null)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PrintAreaShiftedOffWorksheet"), "pageBreak");

				if (pageBreak.PrintArea.Worksheet != this.Worksheet)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PrintAreaMustBeOnSameWorksheet"), "pageBreak");

				if (this.owner.PrintAreas.Contains(pageBreak.PrintArea) == false)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PrintAreaMustBeInPrintAreas"), "pageBreak");
			}

			int index = this.pageBreaks.BinarySearch(pageBreak);

			if (0 <= index)
			{
				if (throwIfPresent)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreaksCannotOverlap"), "pageBreak");

				return false;
			}

			index = ~index;
			if (0 < index)
			{
				PageBreak previousBreak = this.pageBreaks[index - 1];
				if (pageBreak.Id == previousBreak.Id && pageBreak.MinResolved <= previousBreak.MaxResolved)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreaksCannotOverlap"), "pageBreak");
			}

			if (index < this.pageBreaks.Count)
			{
				PageBreak nextBreak = this.pageBreaks[index];
				if (pageBreak.Id == nextBreak.Id && nextBreak.MinResolved <= pageBreak.MaxResolved)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreaksCannotOverlap"), "pageBreak");
			}

			this.pageBreaks.Insert(index, pageBreak);
			return true;
		}

		#endregion  // Add

		#region Clear

		/// <summary>
		/// Clears the collection of page breaks.
		/// </summary>
		/// <seealso cref="PrintOptions.ClearPageBreaks"/>
		public void Clear()
		{
			this.pageBreaks.Clear();
		}

		#endregion  // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified page break exists on the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="pageBreak">The page break to test.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="pageBreak"/> is null.
		/// </exception>
		/// <returns>True if the page break is on the Worksheet; False otherwise.</returns>
		public bool Contains(T pageBreak)
		{
			if (pageBreak == null)
				throw new ArgumentNullException("pageBreak");

			return this.pageBreaks.Contains(pageBreak);
		}

		#endregion  // Contains

		#region IndexOf

		/// <summary>
		/// Gets the 0-based index of the specified page break.
		/// </summary>
		/// <param name="pageBreak">The page break for which the index should be obtained.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="pageBreak"/> is null.
		/// </exception>
		/// <returns>The 0-based index of the page break or -1 if the page break is no tin the collection.</returns>
		public int IndexOf(T pageBreak)
		{
			if (pageBreak == null)
				throw new ArgumentNullException("pageBreak");

			return this.pageBreaks.IndexOf(pageBreak);
		}

		#endregion  // IndexOf

		#region Remove

		/// <summary>
		/// Removes the specified page break from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="pageBreak">The page break which should be removed.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="pageBreak"/> is null.
		/// </exception>
		/// <returns>True if the page break was contained on the Worksheet before removal; False otherwise.</returns>
		/// <seealso cref="PrintOptions.ClearPageBreaks"/>
		public bool Remove(T pageBreak)
		{
			if (pageBreak == null)
				throw new ArgumentNullException("pageBreak");

			return this.pageBreaks.Remove(pageBreak);
		}

		#endregion  // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the page break at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the page break to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when <paramref name="index"/> is less than zero or greater than or equal to the number of page breaks in the collection.
		/// </exception>
		/// <seealso cref="PrintOptions.ClearPageBreaks"/>
		public void RemoveAt(int index)
		{
			this.pageBreaks.RemoveAt(index);
		}

		#endregion  // RemoveAt

		#endregion  // Public Methods

		#region Internal Methods

		#region GetManualBreakCount

		internal int GetManualBreakCount()
		{
			int count = 0;
			for (int i = 0; i < this.pageBreaks.Count; i++)
			{
				if (this.pageBreaks[i].ManuallyCreated)
					count++;
			}

			return count;
		}

		#endregion  // GetManualBreakCount

		// MD 4/6/12 - TFS102169
		// We no longer need this code now that the workbook part is loaded before the worksheet parts.
		#region Removed

		//#region OnPrintAreasLoaded

		//internal void OnPrintAreasLoaded()
		//{
		//    PrintAreasCollection printAreas = this.owner.PrintAreas;

		//    for (int i = 0; i < this.Count; i++)
		//    {
		//        PageBreak pageBreak = this[i];

		//        if (pageBreak.PrintArea != null)
		//            continue;

		//        pageBreak.PrintArea = printAreas.GetPrintArea(pageBreak.Id, pageBreak.Min, pageBreak.Max, this.IsVertical);
		//    }
		//}

		//#endregion  // OnPrintAreasLoaded

		#endregion // Removed

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of page breaks in this collection.
		/// </summary>
		public int Count
		{
			get { return this.pageBreaks.Count; }
		}

		#endregion  // Count

		#region Indexer

		/// <summary>
		/// Gets the page break at the specified index.
		/// </summary>
		/// <param name="index">The index of the page break to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Occurs when <paramref name="index"/> is less than 0 or greater than of equal to <see cref="Count"/>.
		/// </exception>
		/// <returns>A <see cref="PageBreak"/>-derived instance.</returns>
		public T this[int index]
		{
			get { return this.pageBreaks[index]; }
		}

		#endregion  // Indexer

		#endregion  // Public Properties

		#region Internal Properties

		internal abstract bool IsVertical { get; }

		// MD 3/26/12 - 12.1 - Table Support
		#region Worksheet

		internal Worksheet Worksheet
		{
			get { return this.owner.AssociatedWorksheet; }
		}

		#endregion // Worksheet

		#endregion  // Internal Properties

		#endregion  // Properties
	}

	/// <summary>
	/// A collection of horizontal page breaks on a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.HorizontalPageBreaks"/>
	/// <seealso cref="HorizontalPageBreak"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class HorizontalPageBreakCollection : PageBreakCollection<HorizontalPageBreak>
	{
		internal HorizontalPageBreakCollection(PrintOptions owner)
			: base(owner) { }

		internal override bool IsVertical
		{
			get { return false; }
		}

		// MD 3/26/12 - 12.1 - Table Support
		#region ShiftPageBreaksVertically

		internal void ShiftPageBreaksVertically(CellShiftOperation shiftOperation)
		{
			// Note: the WorksheetRegion instances have already been shifted when this is called.

			List<HorizontalPageBreak> horizontalPageBreaks = new List<HorizontalPageBreak>(this);
			foreach (HorizontalPageBreak horizontalPageBreak in horizontalPageBreaks)
			{
				WorksheetRegionAddress regionAddress;
				if (horizontalPageBreak.PrintArea == null)
				{
					regionAddress = new WorksheetRegionAddress(
						horizontalPageBreak.FirstRowOnPage, horizontalPageBreak.FirstRowOnPage,
						0, (short)(this.Worksheet.Columns.MaxCount - 1));
				}
				else
				{
					WorksheetRegion printArea = horizontalPageBreak.PrintArea;
					if (printArea.Worksheet == null)
					{
						this.Remove(horizontalPageBreak);
						continue;
					}

					regionAddress = new WorksheetRegionAddress(
						horizontalPageBreak.FirstRowOnPage, horizontalPageBreak.FirstRowOnPage,
						printArea.FirstColumnInternal, printArea.LastColumnInternal);
				}

				ShiftAddressResult result = shiftOperation.ShiftRegionAddress(ref regionAddress, true);
				if (result.DidShift)
				{
					this.Remove(horizontalPageBreak);
					if (result.IsDeleted == false)
					{
						horizontalPageBreak.Id = regionAddress.FirstRowIndex;
						this.AddInternal(horizontalPageBreak, false);
					}
				}
			}
		}

		#endregion // ShiftPageBreaksVertically
	}

	/// <summary>
	/// A collection of horizontal page breaks on a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="PrintOptions.VerticalPageBreaks"/>
	/// <seealso cref="VerticalPageBreak"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class VerticalPageBreakCollection : PageBreakCollection<VerticalPageBreak>
	{
		internal VerticalPageBreakCollection(PrintOptions owner)
			: base(owner) { }

		internal override bool IsVertical
		{
			get { return true; }
		}
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