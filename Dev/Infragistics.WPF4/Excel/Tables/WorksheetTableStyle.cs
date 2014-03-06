using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a style which can be applied to a <see cref="WorksheetTable"/>.
	/// </summary>
	/// <seealso cref="WorksheetTable.Style"/>
	/// <seealso cref="Excel.Workbook.DefaultTableStyle"/>
	/// <seealso cref="Excel.Workbook.CustomTableStyles"/>
	/// <seealso cref="Excel.Workbook.StandardTableStyles"/>
	[DebuggerDisplay("WorksheetTableStyle: {Name,nq}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		class WorksheetTableStyle :
		IAreaFormatsOwner<WorksheetTableStyleArea>
	{
		#region Member Variables

		private int _alternateColumnStripeWidth = 1;
		private int _alternateRowStripeHeight = 1;
		private WorksheetTableAreaFormatsCollection<WorksheetTableStyleArea> _areaFormats;
		private int _columnStripeWidth = 1;
		private Dictionary<WorksheetTableStyleArea, uint> _dxfIdsByAreaDuringSave;
		private readonly bool _isCustom;
		private bool _isLoading;
		private string _name;
		private CustomTableStyleCollection _customCollection;
		private int _rowStripeHeight = 1;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a custom <see cref="WorksheetTableStyle"/> instance.
		/// </summary>
		/// <param name="name">The name of the new style.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		public WorksheetTableStyle(string name)
			: this(name, true) { }

		internal WorksheetTableStyle(string name, bool isCustom)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_isCustom = isCustom;
			_name = name;
		}

		#endregion // Constructor

		#region Interfaces

		#region IAreaFormatsOwner Members

		bool IAreaFormatsOwner<WorksheetTableStyleArea>.IsReadOnly
		{
			get { return this.IsReadOnly; }
		}

		void IAreaFormatsOwner<WorksheetTableStyleArea>.OnAreaFormatAdded(WorksheetTableStyleArea area, WorksheetCellFormatData format)
		{
			// Nothing needs to be done here.
		}

		void IAreaFormatsOwner<WorksheetTableStyleArea>.VerifyCanBeModified()
		{
			this.VerifyCanBeModified();
		}

		#endregion

		#region IGenericCachedCollectionEx Members

		Workbook IGenericCachedCollectionEx.Workbook
		{
			get { return this.Workbook; }
		}

		#endregion

		#region IWorksheetCellFormatProxyOwner Members

		WorksheetCellFormatData IWorksheetCellFormatProxyOwner.GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			return null;
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values)
		{
			this.VerifyCanBeModified();

			for (int i = 0; i < values.Count; i++)
				WorksheetTableStyle.VerifyAreaFormatValueCanBeSet(values[i]);
		}

		void IWorksheetCellFormatProxyOwner.VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions) 
		{
			const WorksheetCellFormatOptions invalidOptions = 
				WorksheetCellFormatOptions.ApplyAlignmentFormatting | 
				WorksheetCellFormatOptions.ApplyNumberFormatting | 
				WorksheetCellFormatOptions.ApplyProtectionFormatting;

			if ((formatOptions & invalidOptions) != WorksheetCellFormatOptions.None)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidTableStyleAreaFormatOptions"));
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Clone

		/// <summary>
		/// Duplicates the style and returns a deep copy.
		/// </summary>
		/// <param name="name">The name to give to the cloned style.</param>
		/// <remarks>
		/// <p class="body">
		/// A cloned style must be added to the <see cref="Excel.Workbook.CustomTableStyles"/> collection before it can be applied to a 
		/// <see cref="WorksheetTable"/>.
		/// </p>
		/// <p class="body">
		/// The only value not cloned from the style is the <see cref="IsCustom"/> value. Cloning a standard style creates a custom style with 
		/// the same style settings which can then be changed.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <returns>The clone of the style.</returns>
		public WorksheetTableStyle Clone(string name)
		{
			WorksheetTableStyle clone = new WorksheetTableStyle(name, true);

			clone._alternateColumnStripeWidth = _alternateColumnStripeWidth;
			clone._alternateRowStripeHeight = _alternateRowStripeHeight;
			clone._columnStripeWidth = _columnStripeWidth;
			clone._rowStripeHeight = _rowStripeHeight;

			if (_areaFormats != null)
			{
				foreach (KeyValuePair<WorksheetTableStyleArea, IWorksheetCellFormat> pair in _areaFormats)
					clone.AreaFormats[pair.Key].SetFormatting(pair.Value);
			}

			return clone;
		}

		#endregion // Clone

		#endregion // Public Methods

		#region Internal Methods

		#region CanAreaFormatValueBeSet

		internal static bool CanAreaFormatValueBeSet(CellFormatValue value)
		{
			string message;
			return WorksheetTableStyle.VerifyAreaFormatValueCanBeSetHelper(value, out message);
		}

		#endregion // CanAreaFormatValueBeSet

		#region GetAreaPrecedence

		internal static int GetAreaPrecedence(WorksheetTableStyleArea area)
		{
			// This is the precedence order for the ST_TableStyleType
			//wholeTable,
			//pageFieldLabels,
			//pageFieldValues,
			//firstColumnStripe,
			//secondColumnStripe,
			//firstRowStripe,
			//secondRowStripe,
			//lastColumn,
			//firstColumn,
			//headerRow,
			//totalRow,
			//firstHeaderCell,
			//lastHeaderCell,
			//firstSubtotalColumn,
			//secondSubtotalColumn,
			//thirdSubtotalColumn,
			//blankRow,
			//firstSubtotalRow,
			//secondSubtotalRow,
			//thirdSubtotalRow,
			//firstColumnSubheading,
			//secondColumnSubheading,
			//thirdColumnSubheading,
			//firstRowSubheading,
			//secondRowSubheading,
			//thirdRowSubheading,
			//firstTotalCell,
			//lastTotalCell,

			switch (area)
			{
				case WorksheetTableStyleArea.WholeTable: return 0;
				case WorksheetTableStyleArea.ColumnStripe: return 1;
				case WorksheetTableStyleArea.AlternateColumnStripe: return 2;
				case WorksheetTableStyleArea.RowStripe: return 3;
				case WorksheetTableStyleArea.AlternateRowStripe: return 4;
				case WorksheetTableStyleArea.LastColumn: return 5;
				case WorksheetTableStyleArea.FirstColumn: return 6;
				case WorksheetTableStyleArea.HeaderRow: return 7;
				case WorksheetTableStyleArea.TotalRow: return 8;
				case WorksheetTableStyleArea.FirstHeaderCell: return 9;
				case WorksheetTableStyleArea.LastHeaderCell: return 10;
				case WorksheetTableStyleArea.FirstTotalCell: return 11;
				case WorksheetTableStyleArea.LastTotalCell: return 12;

				default:
					Utilities.DebugFail("Unknown WorksheetTableStyleArea: " + area);
					return 13;
			}
		}

		#endregion // GetAreaPrecedence

		#region GetAreaSize

		internal uint? GetAreaSize(WorksheetTableStyleArea area)
		{
			switch (area)
			{
				case WorksheetTableStyleArea.ColumnStripe:
					return (uint)this.ColumnStripeWidth;

				case WorksheetTableStyleArea.AlternateColumnStripe:
					return (uint)this.AlternateColumnStripeWidth;

				case WorksheetTableStyleArea.RowStripe:
					return (uint)this.RowStripeHeight;

				case WorksheetTableStyleArea.AlternateRowStripe:
					return (uint)this.AlternateRowStripeHeight;
			}

			return null;
		}

		#endregion // GetAreaSize

		#region InitSerializationCache

		internal void InitSerializationCache(WorkbookSerializationManager manager)
		{
			Debug.Assert(this.IsCustom, "We shouldn't be calling this for standard styles.");

			_dxfIdsByAreaDuringSave = new Dictionary<WorksheetTableStyleArea, uint>();
			foreach (WorksheetTableAreaFormatProxy<WorksheetTableStyleArea> areaFormat in this.AreaFormats.GetFormatProxies())
				_dxfIdsByAreaDuringSave.Add(areaFormat.Area, manager.AddDxf(areaFormat.Element));

			this.InitSerializationCacheHelper(manager,
				this.ColumnStripeWidth != 1,
				WorksheetTableStyleArea.ColumnStripe);

			this.InitSerializationCacheHelper(manager,
				this.AlternateColumnStripeWidth != 1,
				WorksheetTableStyleArea.AlternateColumnStripe);

			this.InitSerializationCacheHelper(manager,
				this.RowStripeHeight != 1,
				WorksheetTableStyleArea.RowStripe);

			this.InitSerializationCacheHelper(manager,
				this.AlternateRowStripeHeight != 1,
				WorksheetTableStyleArea.AlternateRowStripe);
		}

		private void InitSerializationCacheHelper(WorkbookSerializationManager manager,
			bool forceWrite, WorksheetTableStyleArea area)
		{
			if (forceWrite && _dxfIdsByAreaDuringSave.ContainsKey(area) == false)
			{
				WorksheetCellFormatData dxf = manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);
				_dxfIdsByAreaDuringSave.Add(area, manager.AddDxf(dxf));
			}
		}

		#endregion // InitSerializationCache

		#region OnAddedToCollection

		internal void OnAddedToCollection(CustomTableStyleCollection collection)
		{
			Debug.Assert(_customCollection == null, "The owner should be null here.");
			Debug.Assert(this.IsCustom, "Only custom formats should be added to the CustomTableStyleCollection.");
			_customCollection = collection;

			if (_areaFormats != null)
				_areaFormats.OnRooted(collection.Workbook);
		}

		#endregion // OnAddedToCollection

		#region OnRemovedFromCollection

		internal void OnRemovedFromCollection()
		{
			Debug.Assert(_customCollection != null, "The owner should not be null here.");
			_customCollection = null;
		}

		#endregion // OnRemovedFromCollection

		#region VerifyIsCustom

		internal void VerifyCanBeModified()
		{
			if (this.IsReadOnly)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotModifyStandardTableStyle"));
		}

		#endregion // VerifyIsCustom

		#endregion // Internal Methods

		#region Private Methods

		#region VerifyAreaFormatValueCanBeSet

		private static void VerifyAreaFormatValueCanBeSet(CellFormatValue value)
		{
			string message;
			if (WorksheetTableStyle.VerifyAreaFormatValueCanBeSetHelper(value, out message) == false)
				throw new InvalidOperationException(message);
		}

		private static bool VerifyAreaFormatValueCanBeSetHelper(CellFormatValue value, out string message)
		{
			switch (value)
			{
				case CellFormatValue.Alignment:
				case CellFormatValue.DiagonalBorderColorInfo:
				case CellFormatValue.DiagonalBorders:
				case CellFormatValue.DiagonalBorderStyle:
				case CellFormatValue.FormatString:
				case CellFormatValue.Indent:
				case CellFormatValue.Locked:
				case CellFormatValue.Rotation:
				case CellFormatValue.ShrinkToFit:
				case CellFormatValue.VerticalAlignment:
				case CellFormatValue.WrapText:
				case CellFormatValue.Style:
					message = SR.GetString("LE_InvalidOperationException_InvalidTableStyleAreaFormatProperty", value);
					return false;

				case CellFormatValue.FontHeight:
				case CellFormatValue.FontName:
				case CellFormatValue.FontSuperscriptSubscriptStyle:
					message = SR.GetString("LE_InvalidOperationException_InvalidTableStyleAreaFontProperty", value);
					return false;

				default:
					message = null;
					return true;
			}
		}

		#endregion // VerifyAreaFormatValueCanBeSet

		#region VerifyStripeExtent

		private static void VerifyStripeExtent(int value, string description)
		{
			if (value < 1 || 9 < value)
				throw new ArgumentOutOfRangeException("value", value, SR.GetString("LE_ArgumentOutOfRangeException_InvalidTableStyleAreaStripeExtent", description));
		}

		#endregion // VerifyStripeExtent

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region AlternateColumnStripeWidth

		/// <summary>
		/// Gets or sets the number of columns which will span each alternate column stripe. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The stripe sizes must be between 1 and 9, inclusive.
		/// </p>
		/// <p class="body">
		/// The column stripes are assigned from left to right in the table, first assigning the column stripe, then the alternate column 
		/// stripe, then repeating.
		/// </p>
		/// <p class="body">
		/// The alternate column stripe format is defined in the <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the 
		/// <see cref="WorksheetTableStyleArea"/>.AlternateColumnStripe value.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value is set and <see cref="IsCustom"/> is False, indicating that the style is a read-only, standard table style.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than 1 or greater than 9.
		/// </exception>
		/// <value>The number of columns which will span each alternate column stripe.</value>
		/// <seealso cref="ColumnStripeWidth"/>
		public int AlternateColumnStripeWidth
		{
			get { return _alternateColumnStripeWidth; }
			set
			{
				this.VerifyCanBeModified();

				if (_alternateColumnStripeWidth == value)
					return;

				WorksheetTableStyle.VerifyStripeExtent(value, "AlternateColumnStripeWidth");
				_alternateColumnStripeWidth = value;
			}
		}

		#endregion // AlternateColumnStripeWidth

		#region AlternateRowStripeHeight

		/// <summary>
		/// Gets or sets the number of rows which will span each alternate row stripe. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The stripe sizes must be between 1 and 9, inclusive.
		/// </p>
		/// <p class="body">
		/// The row stripes are assigned from top to bottom in the table, first assigning the row stripe, then the alternate row
		/// stripe, then repeating.
		/// </p>
		/// <p class="body">
		/// The alternate row stripe format is defined in the <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the 
		/// <see cref="WorksheetTableStyleArea"/>.AlternateRowStripe value.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value is set and <see cref="IsCustom"/> is False, indicating that the style is a read-only, standard table style.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than 1 or greater than 9.
		/// </exception>
		/// <value>The number of rows which will span each alternate row stripe.</value>
		/// <seealso cref="RowStripeHeight"/>
		public int AlternateRowStripeHeight
		{
			get { return _alternateRowStripeHeight; }
			set
			{
				this.VerifyCanBeModified();

				if (_alternateRowStripeHeight == value)
					return;

				WorksheetTableStyle.VerifyStripeExtent(value, "AlternateRowStripeHeight");
				_alternateRowStripeHeight = value;
			}
		}

		#endregion // AlternateRowStripeHeight

		#region AreaFormats

		/// <summary>
		/// Gets the collection of formats used for each area of a <see cref="WorksheetTable"/> to which the style is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The area formats specified are differential formats. In other words, only the properties that are set to non-default values will be
		/// applied to the appropriate cells. An area format can define only a background color or only font information and that format will be 
		/// applied to the cells while all other formatting properties on the cells will be maintained.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTable.AreaFormats"/>
		/// <seealso cref="WorksheetTableColumn.AreaFormats"/>
		public WorksheetTableAreaFormatsCollection<WorksheetTableStyleArea> AreaFormats
		{
			get
			{
				if (_areaFormats == null)
					_areaFormats = new WorksheetTableAreaFormatsCollection<WorksheetTableStyleArea>(this);

				return _areaFormats;
			}
		}

		#endregion // AreaFormats

		#region ColumnStripeWidth

		/// <summary>
		/// Gets or sets the number of columns which will span each column stripe. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The stripe sizes must be between 1 and 9, inclusive.
		/// </p>
		/// <p class="body">
		/// The column stripes are assigned from left to right in the table, first assigning the column stripe, then the alternate column 
		/// stripe, then repeating.
		/// </p>
		/// <p class="body">
		/// The column stripe format is defined in the <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the 
		/// <see cref="WorksheetTableStyleArea"/>.ColumnStripe value.
		/// </p>
		/// </remarks>
		/// <value>The number of columns which will span each column stripe.</value>
		/// <exception cref="InvalidOperationException">
		/// The value is set and <see cref="IsCustom"/> is False, indicating that the style is a read-only, standard table style.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than 1 or greater than 9.
		/// </exception>
		/// <seealso cref="AlternateColumnStripeWidth"/>
		public int ColumnStripeWidth
		{
			get { return _columnStripeWidth; }
			set
			{
				this.VerifyCanBeModified();

				if (_columnStripeWidth == value)
					return;

				WorksheetTableStyle.VerifyStripeExtent(value, "ColumnStripeWidth");
				_columnStripeWidth = value;
			}
		}

		#endregion // ColumnStripeWidth

		#region IsCustom

		/// <summary>
		/// Gets the value indicating whether the style is a custom style.
		/// </summary>
		/// <value>True id this is a custom table style; False if this is a read-only, standard table style.</value>
		public bool IsCustom
		{
			get { return _isCustom; }
		}

		#endregion // IsCustom

		#region Name

		/// <summary>
		/// Gets or sets the name of the style.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set and <see cref="IsCustom"/> is False, indicating that the style is a read-only, standard table style.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is null, empty, or greater than 255 characters in length.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned matches the name of another custom style in the owning <see cref="Excel.Workbook.CustomTableStyles"/> collection.
		/// Table names are compared case-insensitively.
		/// </exception>
		/// <value>The name of the style.</value>
		public string Name
		{
			get { return _name; }
			set
			{
				this.VerifyCanBeModified();

				if (_name == value)
					return;

				if (_customCollection != null)
					_customCollection.OnTableStyleNameChanging(this, value);

				_name = value;
			}
		}

		#endregion // Name

		#region RowStripeHeight

		/// <summary>
		/// Gets or sets the number of rows which will span each row stripe. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The stripe sizes must be between 1 and 9, inclusive.
		/// </p>
		/// <p class="body">
		/// The row stripes are assigned from top to bottom in the table, first assigning the row stripe, then the alternate row
		/// stripe, then repeating.
		/// </p>
		/// <p class="body">
		/// The row stripe format is defined in the <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the 
		/// <see cref="WorksheetTableStyleArea"/>.RowStripe value.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value is set and <see cref="IsCustom"/> is False, indicating that the style is a read-only, standard table style.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is less than 1 or greater than 9.
		/// </exception>
		/// <value>The number of rows which will span each row stripe.</value>
		/// <seealso cref="AlternateRowStripeHeight"/>
		public int RowStripeHeight
		{
			get { return _rowStripeHeight; }
			set
			{
				this.VerifyCanBeModified();

				if (_rowStripeHeight == value)
					return;

				WorksheetTableStyle.VerifyStripeExtent(value, "ColumnStripeWidth");
				_rowStripeHeight = value;
			}
		}

		#endregion // RowStripeHeight

		#endregion // Public Properties

		#region Internal Properties

		#region CustomCollection

		internal CustomTableStyleCollection CustomCollection
		{
			get { return _customCollection; }
		}

		#endregion // CustomCollection

		#region DxfIdsByAreaDuringSave

		internal Dictionary<WorksheetTableStyleArea, uint> DxfIdsByAreaDuringSave
		{
			get { return _dxfIdsByAreaDuringSave; }
		}

		#endregion // DxfIdsByAreaDuringSave

		#region IsLoading

		internal bool IsLoading
		{
			get { return _isLoading; }
			set 
			{
				if (_isLoading == value)
					return;

				_isLoading = value;

				if (_isLoading == value)
				{
					foreach (WorksheetTableAreaFormatProxy<WorksheetTableStyleArea> format in this.AreaFormats.GetFormatProxies())
						format.Element.Freeze();
				}
			}
		}

		#endregion // IsLoading

		#region IsReadOnly

		internal bool IsReadOnly
		{
			get { return _isCustom == false && _isLoading == false; }
		} 

		#endregion // IsReadOnly

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (_customCollection != null)
					return _customCollection.Workbook;

				return null;
			}
		}

		#endregion // Workbook

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