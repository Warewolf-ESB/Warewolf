using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;
using System.Diagnostics;
using System.Globalization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// A collection of custom <see cref="WorksheetTableStyle"/> instances which can be applied to a <see cref="WorksheetTable"/> in the 
	/// <see cref="Workbook"/>.
	/// </summary>
	/// <seealso cref="Excel.Workbook.DefaultTableStyle"/>
	/// <seealso cref="Excel.Workbook.CustomTableStyles"/>
	[DebuggerDisplay("CustomTableStyleCollection: Count = {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class CustomTableStyleCollection : ICollection<WorksheetTableStyle>
	{
		#region Member Variables

		private List<WorksheetTableStyle> _styles;
		private Workbook _workbook;

		#endregion // Member Variables

		#region Constructor

		internal CustomTableStyleCollection(Workbook workbook)
		{
			_workbook = workbook;
			_styles = new List<WorksheetTableStyle>();
		}

		#endregion // Constructor

		#region Interfaces

		#region ICollection<WorksheetTableStyle> Members

		void ICollection<WorksheetTableStyle>.CopyTo(WorksheetTableStyle[] array, int arrayIndex)
		{
			_styles.CopyTo(array, arrayIndex);
		}

		bool ICollection<WorksheetTableStyle>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<WorksheetTableStyle> Members

		IEnumerator<WorksheetTableStyle> IEnumerable<WorksheetTableStyle>.GetEnumerator()
		{
			return _styles.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _styles.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a custom <see cref="WorksheetTableStyle"/> to the collection.
		/// </summary>
		/// <param name="style">The custom table style to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="style"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="style"/> is a standard table style.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="style"/> is already in a CustomTableStyleCollection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="style"/> has a name which matches one of the other custom table styles in the collection.
		/// Names are compared case-insensitively.
		/// </exception>
		/// <seealso cref="WorksheetTableStyle.IsCustom"/>
		/// <seealso cref="WorksheetTableStyle.Name"/>
		public void Add(WorksheetTableStyle style)
		{
			if (style == null)
				throw new ArgumentNullException("style");

			if (style.IsCustom == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CannotAddStandardTableStyle"), "style");

			if (style.CustomCollection != null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateTableStyle"), "style");

			if (this[style.Name] != null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateTableStyleName_New"), "style");

			_styles.Add(style);
			style.OnAddedToCollection(this);
		}

		#endregion // Add

		#region Clear

		/// <summary>
		/// Clears the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any <see cref="WorksheetTable"/> instances in the workbook have their style removed due to this operation, their style 
		/// will be set to the <see cref="Excel.Workbook.DefaultTableStyle"/>.
		/// </p>
		/// </remarks>
		public void Clear()
		{
			for (int i = _styles.Count - 1; i >= 0; i--)
				this.RemoveAt(i);
		}

		#endregion // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified <see cref="WorksheetTableStyle"/> is contained in the collection.
		/// </summary>
		/// <param name="style">The table style to find in the collection.</param>
		/// <returns>True if the style is in the collection; False otherwise.</returns>
		public bool Contains(WorksheetTableStyle style)
		{
			return _styles.Contains(style);
		}

		#endregion // Contains

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified style in the collection.
		/// </summary>
		/// <param name="style">The style to find in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="style"/> is null.
		/// </exception>
		/// <returns>
		/// The 0-based index of the specified style in the collection or -1 if the style is not in the collection.
		/// </returns>
		public int IndexOf(WorksheetTableStyle style)
		{
			if (style == null)
				throw new ArgumentNullException("style");

			return _styles.IndexOf(style);
		}

		#endregion // IndexOf

		#region Remove

		/// <summary>
		/// Removes the specified <see cref="WorksheetTableStyle"/> from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any <see cref="WorksheetTable"/> instances in the workbook have their style removed due to this operation, their style 
		/// will be set to the <see cref="Excel.Workbook.DefaultTableStyle"/>.
		/// </p>
		/// </remarks>
		/// <param name="style">The table style to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="style"/> is null.
		/// </exception>
		/// <returns>True if the style was in the collection and therefore removed; False otherwise.</returns>
		public bool Remove(WorksheetTableStyle style)
		{
			if (style == null)
				throw new ArgumentNullException("style");

			int index = this.IndexOf(style);
			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the <see cref="WorksheetTableStyle"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any <see cref="WorksheetTable"/> instances in the workbook have their style removed due to this operation, their style 
		/// will be set to the <see cref="Excel.Workbook.DefaultTableStyle"/>.
		/// </p>
		/// </remarks>
		/// <param name="index">The index of the table style to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt(int index)
		{
			Utilities.VerifyCollectionIndex(index, this.Count);

			WorksheetTableStyle removedStyle = this[index];
			_styles.RemoveAt(index);
			removedStyle.OnRemovedFromCollection();

			if (removedStyle == this.Workbook.DefaultTableStyle)
				this.Workbook.DefaultTableStyle = null;

			foreach (Worksheet worksheet in this.Workbook.Worksheets)
			{
				foreach (WorksheetTable table in worksheet.Tables)
				{
					if (table.Style == removedStyle)
						table.Style = this.Workbook.DefaultTableStyle;
				}
			}
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#region Internal Methods

		#region OnTableStyleNameChanging

		internal void OnTableStyleNameChanging(WorksheetTableStyle style, string newName)
		{
			WorksheetTableStyle existingStyle = this[newName];
			if (existingStyle == null || existingStyle == style)
				return;

			throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateTableStyleName_Existing"), "value");
		}

		#endregion // OnTableStyleNameChanging

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of custom <see cref="WorksheetTableStyle"/> instances in the collection.
		/// </summary>
		public int Count
		{
			get { return _styles.Count; }
		}

		#endregion // Count

		#region Indexer[int]

		/// <summary>
		/// Gets the <see cref="WorksheetTableStyle"/> at the specified index.
		/// </summary>
		/// <param name="index">The index at which to get the WorksheetTableStyle.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <returns>The WorksheetTableStyle instance at the specified index.</returns>
		public WorksheetTableStyle this[int index]
		{
			get
			{
				Utilities.VerifyCollectionIndex(index, this.Count);
				return _styles[index];
			}
		}

		#endregion // Indexer[int]

		#region Indexer[string]

		/// <summary>
		/// Gets the <see cref="WorksheetTableStyle"/> with the specified name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Table style names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the WorksheetTableStyle to find.</param>
		/// <returns>
		/// The WorksheetTableStyle instance with the specified name or null if a table style with that name does not exist.
		/// </returns>
		public WorksheetTableStyle this[string name]
		{
			get
			{
				if (string.IsNullOrEmpty(name))
					return null;

				// MD 4/9/12 - TFS101506
				CultureInfo culture = _workbook.CultureResolved;

				for (int i = 0; i < _styles.Count; i++)
				{
					// MD 4/9/12 - TFS101506
					//if (String.Equals(name, _styles[i].Name, StringComparison.CurrentCultureIgnoreCase))
					if (String.Compare(name, _styles[i].Name, culture, CompareOptions.IgnoreCase) == 0)
						return _styles[i];
				}

				return null;
			}
		}

		#endregion // Indexer[string]

		#endregion // Public Properties

		#region Internal Properties

		internal Workbook Workbook
		{
			get { return _workbook; }
		}

		#endregion // Internal Properties

		#endregion // Properties
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