using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// A collection of formats for areas of a <see cref="WorksheetTable"/>.
	/// </summary>
	/// <typeparam name="TArea">
	/// An enumeration defining the various table areas which can contain formats.
	/// </typeparam>
	/// <seealso cref="WorksheetTable.AreaFormats"/>
	/// <seealso cref="WorksheetTableColumn.AreaFormats"/>
	/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
	[DebuggerDisplay("WorksheetTableAreaFormatsCollection: Count - {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class WorksheetTableAreaFormatsCollection<TArea> :
		IEnumerable<KeyValuePair<TArea, IWorksheetCellFormat>>
	{
		#region Member Variables

		private Dictionary<TArea, WorksheetTableAreaFormatProxy<TArea>> _formats;
		private IAreaFormatsOwner<TArea> _owner;

		#endregion // Member Variables

		#region Constructor

		static WorksheetTableAreaFormatsCollection()
		{
			Debug.Assert(typeof(TArea).IsEnum, "This class should only be used for Enum types.");
		}

		internal WorksheetTableAreaFormatsCollection(IAreaFormatsOwner<TArea> owner)
		{
			_formats = new Dictionary<TArea, WorksheetTableAreaFormatProxy<TArea>>();
			_owner = owner;
		}

		#endregion // Constructor

		#region Interfaces

		#region IEnumerable<KeyValuePair<TArea,IWorksheetCellFormat>> Members

		IEnumerator<KeyValuePair<TArea, IWorksheetCellFormat>> IEnumerable<KeyValuePair<TArea, IWorksheetCellFormat>>.GetEnumerator()
		{
			foreach (KeyValuePair<TArea, WorksheetTableAreaFormatProxy<TArea>> pair in _formats)
			{
				if (pair.Value.IsEmpty == false)
					yield return new KeyValuePair<TArea, IWorksheetCellFormat>(pair.Key, pair.Value);
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<TArea, IWorksheetCellFormat>>)this).GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Determines whether the area has a non-default format applied.
		/// </summary>
		/// <param name="area">The area of which to test the format.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="area"/> is not defined.
		/// </exception>
		/// <returns>True if the area has a non-default format applied; False otherwise.</returns>
		public bool HasFormat(TArea area)
		{
			Utilities.VerifyEnumValue<TArea>(area);

			WorksheetTableAreaFormatProxy<TArea> proxy;
			if (_formats.TryGetValue(area, out proxy))
				return proxy.IsEmpty == false;

			return false;
		}

		#endregion // Contains

		#endregion // Public Methods

		#region Internal Methods

		#region GetFormatElement

		internal WorksheetCellFormatData GetFormatElement(Workbook workbook, TArea area)
		{
			return this.GetFormatElement(workbook, area, false);
		}

		internal WorksheetCellFormatData GetFormatElement(Workbook workbook, TArea area, bool shouldCreateIfNotPresent)
		{
			WorksheetTableAreaFormatProxy<TArea> proxy = this.GetFormatProxy(workbook, area, shouldCreateIfNotPresent);
			if (proxy == null)
				return null;

			return proxy.Element;
		}

		#endregion // GetFormatElement

		#region GetFormatProxy

		internal WorksheetTableAreaFormatProxy<TArea> GetFormatProxy(Workbook workbook, TArea area)
		{
			return this.GetFormatProxy(workbook, area, true);
		}

		internal WorksheetTableAreaFormatProxy<TArea> GetFormatProxy(Workbook workbook, TArea area, bool shouldCreateIfNotPresent)
		{
			WorksheetTableAreaFormatProxy<TArea> proxy;
			if (_formats.TryGetValue(area, out proxy) == false && shouldCreateIfNotPresent)
			{
				WorksheetCellFormatData format;
				if (workbook == null)
					format = new WorksheetCellFormatData(null, WorksheetCellFormatType.DifferentialFormat);
				else
					format = workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);

				proxy = new WorksheetTableAreaFormatProxy<TArea>(area, format, _owner);
				if (_owner.IsReadOnly)
					proxy.Element.Freeze();

				_formats[area] = proxy;

				proxy.Element.OnAddedToRootCollection(_owner);
				_owner.OnAreaFormatAdded(area, proxy.Element);
			}

			return proxy;
		}

		#endregion // GetFormatProxy

		#region GetFormatProxies

		internal IEnumerable<WorksheetTableAreaFormatProxy<TArea>> GetFormatProxies()
		{
			return _formats.Values;
		}

		#endregion // GetFormatProxies

		#region OnRooted

		internal void OnRooted(Workbook workbook)
		{
			foreach (WorksheetCellFormatProxy proxy in _formats.Values)
				proxy.Element.SetWorkbookInternal(workbook, false);
		}

		#endregion // OnRooted

		#region OnUnrooted

		internal void OnUnrooted()
		{
			foreach (WorksheetCellFormatProxy proxy in _formats.Values)
				proxy.Element.OnRemovedFromRootCollection();
		}

		#endregion // OnUnrooted

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of area formats in the collection.
		/// </summary>
		public int Count
		{
			get { return _formats.Count; }
		}

		#endregion // Count

		#region Indexer[TArea]

		/// <summary>
		/// Gets the format for the specified area.
		/// </summary>
		/// <param name="area">The area for which to get the format.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="area"/> is not defined.
		/// </exception>
		/// <value>
		/// An <see cref="IWorksheetCellFormat"/> instance describing the appearance of the specified area.
		/// </value>
		public IWorksheetCellFormat this[TArea area]
		{
			get
			{
				Utilities.VerifyEnumValue<TArea>(area);
				return this.GetFormatProxy(_owner.Workbook, area);
			}
		}

		#endregion // Indexer[TArea]

		#endregion // Properties
	}

	internal delegate bool CanAreaFormatValueBeSetCallback<TArea>(TArea area, CellFormatValue value);

	internal interface IAreaFormatsOwner<TArea> : IWorksheetCellFormatProxyOwner, IGenericCachedCollectionEx
	{
		bool IsReadOnly { get; }

		void OnAreaFormatAdded(TArea area, WorksheetCellFormatData format);
		void VerifyCanBeModified();
	}

	internal class WorksheetTableAreaFormatProxy<TArea> : WorksheetCellFormatProxy
	{
		private TArea _area;

		public WorksheetTableAreaFormatProxy(TArea area, WorksheetCellFormatData data, IWorksheetCellFormatProxyOwner owner)
			: base(data, null, owner)
		{
			_area = area;
		}

		public TArea Area
		{
			get { return _area; }
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