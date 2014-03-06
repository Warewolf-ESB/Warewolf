using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using System.IO;





using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 11/8/11 - TFS85193
	/// <summary>
	/// A collection of <see cref="FormattedTextParagraph"/> instances in <see cref="T:FormattedText"/>.
	/// </summary>
	/// <seealso cref="FormattedText.Paragraphs"/>



	public

		 class FormattedTextParagraphCollection :
		ICollection<FormattedTextParagraph>
	{
		#region Member Variables

		private FormattedText owner;
		private List<FormattedTextParagraph> paragraphs = new List<FormattedTextParagraph>();

		#endregion  // Member Variables

		#region Constructor

		internal FormattedTextParagraphCollection(FormattedText owner)
		{
			this.owner = owner;
		}

		#endregion  // Constructor

		#region Interfaces

		#region ICollection<FormattedTextParagraph> Members

		void ICollection<FormattedTextParagraph>.Add(FormattedTextParagraph paragraph)
		{
			throw new NotSupportedException(SR.GetString("LE_InvalidOperationException_CannotAddParagraphDirectly"));
		}

		void ICollection<FormattedTextParagraph>.CopyTo(FormattedTextParagraph[] array, int arrayIndex)
		{
			this.paragraphs.CopyTo(array, arrayIndex);
		}

		bool ICollection<FormattedTextParagraph>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.paragraphs.GetEnumerator();
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a paragraph with the specified text and returns the <see cref="FormattedTextParagraph"/> 
		/// representing the new paragraph.
		/// </summary>
		/// <param name="paragraphText">The text in the paragraph.</param>
		/// <returns>The FormattedTextParagraph representing the new paragraph.</returns>
		public FormattedTextParagraph Add(string paragraphText)
		{
			return this.Insert(this.paragraphs.Count, paragraphText);
		}

		#endregion  // Add

		#region Clear

		/// <summary>
		/// Removes all paragraphs from the formatted text.
		/// </summary>
		public void Clear()
		{
			for (int i = this.paragraphs.Count - 1; i >= 0; i--)
				this.RemoveAt(i);
		}

		#endregion  // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified paragraph exists in the collection.
		/// </summary>
		/// <param name="paragraph">The paragraph to find in the collection.</param>
		/// <returns>True if the paragraph exists in the collection; False otherwise.</returns>
		public bool Contains(FormattedTextParagraph paragraph)
		{
			return this.paragraphs.Contains(paragraph);
		}

		#endregion  // Contains

		#region GetEnumerator

		/// <summary>
		/// Gets the enumerator to iterate over all paragraphs.
		/// </summary>
		public IEnumerator<FormattedTextParagraph> GetEnumerator()
		{
			return this.paragraphs.GetEnumerator();
		}

		#endregion  // GetEnumerator

		#region IndexOf

		/// <summary>
		/// Gets the zero-based index of the specified paragraph in the collection.
		/// </summary>
		/// <param name="paragraph">The paragraph to find in the collection.</param>
		/// <returns>
		/// The zero-based index of the paragraph in the collection or -1 if the paragraph doesn't exist in the collection.
		/// </returns>
		public int IndexOf(FormattedTextParagraph paragraph)
		{
			return this.paragraphs.IndexOf(paragraph);
		}

		#endregion  // IndexOf

		#region Insert

		/// <summary>
		/// Inserts a paragraph with the specified text and returns the <see cref="FormattedTextParagraph"/> 
		/// representing the new paragraph.
		/// </summary>
		/// <param name="index">The zero-based index in the collection where the paragraph should be inserted.</param>
		/// <param name="paragraphText">The text in the paragraph.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or greater than the number of paragraphs in the collection.
		/// </exception>
		/// <returns>The FormattedTextParagraph representing the new paragraph.</returns>
		public FormattedTextParagraph Insert(int index, string paragraphText)
		{
			if (index < 0 || this.paragraphs.Count < index)
				throw new ArgumentOutOfRangeException("index", SR.GetString("LE_ArgumentOutOfRangeException_BadInsertIndex"));

			int nextParagraphStartIndex = 0;
			if (0 < index)
			{
				FormattedTextParagraph previousParagraph = this.paragraphs[index - 1];
				nextParagraphStartIndex = previousParagraph.StartIndex + previousParagraph.GetLengthResolved(false);
			}

			FormattedTextParagraph paragraph = new FormattedTextParagraph(this.owner, nextParagraphStartIndex, paragraphText, null);
			this.InsertInternal(index, paragraph, null);

			return paragraph;
		}

		#endregion  // Insert

		#region Remove

		/// <summary>
		/// Removes the specified paragraph from the collection.
		/// </summary>
		/// <param name="paragraph">The paragraph to remove from the collection.</param>
		/// <returns>True if the paragraph was in the collection and was removed; False otherwise.</returns>
		public bool Remove(FormattedTextParagraph paragraph)
		{
			if (paragraph.FormattedText != this.owner)
				return false;

			int index = this.paragraphs.IndexOf(paragraph);
			if (index < 0)
			{
				Utilities.DebugFail("We should have found the paragraph here");
				return false;
			}

			this.RemoveAt(index);
			return true;
		}

		#endregion  // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the paragraph at the specified zero-based index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the paragraph to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or greater than or equal to the number of paragraphs in the collection.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || this.paragraphs.Count <= index)
				throw new ArgumentOutOfRangeException(SR.GetString("LE_ArgumentOutOfRangeException_BadRemoveAtIndex"));

			FormattedTextParagraph paragraph = this.paragraphs[index];
			int removedLength = paragraph.LengthResolved;

			this.paragraphs.RemoveAt(index);
			paragraph.OnRemovedFromFormattedString();

			for (int i = index; i < this.paragraphs.Count; i++)
				this.paragraphs[i].StartIndex -= removedLength;

			this.owner.ClearCache();
		}

		#endregion  // RemoveAt

		#endregion  // Public Methods

		#region Internal Methods

		#region InitializeFrom

		internal void InitializeFrom(FormattedTextParagraphCollection other)
		{
			for (int i = 0; i < other.paragraphs.Count; i++)
				this.paragraphs.Add(other.paragraphs[i].Clone(this.owner));
		}

		#endregion  // InitializeFrom

		#region InsertInternal

		internal void InsertInternal(int index, FormattedTextParagraph paragraph, string newLine)
		{
			if (0 < index)
				paragraph.Alignment = this.paragraphs[index - 1].Alignment;
			else if (index < this.paragraphs.Count)
				paragraph.Alignment = this.paragraphs[index].Alignment;

			int addedLength = paragraph.GetLengthResolved(index == this.paragraphs.Count);
			for (int i = index; i < this.paragraphs.Count; i++)
				this.paragraphs[i].StartIndex += addedLength;

			this.paragraphs.Insert(index, paragraph);
			this.owner.ClearCache();
		}

		#endregion  // InsertInternal

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of paragraphs in the collection.
		/// </summary>
		public int Count
		{
			get { return this.paragraphs.Count; }
		}

		#endregion  // Count

		#region Indexer

		/// <summary>
		/// Gets the paragraph at the specified zero-based index.
		/// </summary>
		/// <param name="index">The zero-based index of the paragraph to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or greater than or equal to the number of paragraphs in the collection.
		/// </exception>
		/// <returns>The <see cref="FormattedTextParagraph"/> representing the paragraph at the specified index.</returns>
		public FormattedTextParagraph this[int index]
		{
			get { return this.paragraphs[index]; }
		}

		#endregion  // Indexer

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