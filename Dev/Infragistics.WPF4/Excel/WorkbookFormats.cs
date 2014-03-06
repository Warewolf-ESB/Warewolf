using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel
{





	// MD 10/19/07
	// Found while fixing BR27421
	// This class doesn't need to derive from CollectionBase anymore
	//internal class WorkbookFormatCollection : CollectionBase
	// MD 11/28/11 - TFS96486
	// Added the partial keyword so we can have another file in this class which contains unicode characters.
	//internal class WorkbookFormatCollection
	internal partial class WorkbookFormatCollection
	{
		#region Constants

		internal const int FormatTableOffset = 164;

		// MD 3/25/12 - TFS104630
		internal const int ShortDateFormatIndex = 0x0E;
		internal const int ShortDateAndTimeFormatIndex = 0x16;
		internal const string ShortTimePatternSuffix = " h:mm";

		#endregion Constants

		#region Static Variables

		// MD 2/7/12 - 12.1 - Table Support
		private static Dictionary<int, int> dxfIndexToIndexMap;
		private static Dictionary<int, int> indexToDxfIndexMap;

		#endregion // Static Variables

		#region Member Variables

		// MD 10/19/07
		// Found while fixing BR27421
		// We need to store multiple hashtables now
		//private Dictionary<string, int> formatHash;
		private Dictionary<string, int> formatHashByFormat;
		private Dictionary<int, string> formatHashByIndex;
		private int customFormatCount;

		// MD 2/27/12 - 12.1 - Table Support
		private Dictionary<int, ValueFormatter> valueFormattersByIndex;
		private Workbook workbook;

		// MD 5/10/12 - TFS104961
		// The built in formats are culture based, so we should cache the symbols and separators we used when creating the 
		// built in formats.
		private string currencySymbolAtInitialization;
		private string decimalSeparatorAtInitialization;
		private string groupSeparatorAtInitialization;
		private bool isVerifyingBuiltInFormats;

		#endregion Member Variables

		#region Constructor

		// MD 2/15/11 - TFS66403
		// Take the format because some of the built in formats are based on the current workbook format.
		//internal WorkbookFormatCollection()
		// MD 2/27/12 - 12.1 - Table Support
		//internal WorkbookFormatCollection(WorkbookFormat workbookFormat)
		internal WorkbookFormatCollection(Workbook workbook)
		{
			// MD 2/27/12 - 12.1 - Table Support
			this.workbook = workbook;

			// MD 10/19/07
			// Found while fixing BR27421
			// The collection should be initialized with default values
			//this.formatHash = new Dictionary<string, int>();
			this.formatHashByIndex = new Dictionary<int, string>();

			// MD 5/20/08 - BR33136
			// The format strings are case sensitive.
			//this.formatHashByFormat = new Dictionary<string, int>( StringComparer.InvariantCultureIgnoreCase );
			this.formatHashByFormat = new Dictionary<string, int>( StringComparer.InvariantCulture );

			// MD 5/10/12 - TFS104961
			// Moved this code to VerifyBuiltInFormats because we may need to reinitialize the formats when the culture changes.
			#region Moved

			//this.AddFormat(0x00, "General");
			//this.AddFormat(0x01, "0");
			//this.AddFormat(0x02, "0.00");
			//this.AddFormat(0x03, "#,##0");
			//this.AddFormat(0x04, "#,##0.00");
			//// MD 11/12/07 - BR27987
			//// The format strings are slightly off from what is written out by Excel
			////this.AddFormat( 0x05, "\"$\"#,##0_);(\"$\"#,##0)" );
			////this.AddFormat( 0x06, "\"$\"#,##0_);[Red](\"$\"#,##0)" );
			////this.AddFormat( 0x07, "\"$\"#,##0.00_);(\"$\"#,##0.00)" );
			////this.AddFormat( 0x08, "\"$\"#,##0.00_);[Red](\"$\"#,##0.00)" );
			//this.AddFormat(0x05, "\"$\"#,##0_);\\(\"$\"#,##0\\)");
			//this.AddFormat(0x06, "\"$\"#,##0_);[Red]\\(\"$\"#,##0\\)");
			//this.AddFormat(0x07, "\"$\"#,##0.00_);\\(\"$\"#,##0.00\\)");
			//this.AddFormat(0x08, "\"$\"#,##0.00_);[Red]\\(\"$\"#,##0.00\\)");
			//this.AddFormat(0x09, "0%");
			//this.AddFormat(0x0A, "0.00%");
			//this.AddFormat(0x0B, "0.00E+00");
			//this.AddFormat(0x0C, "# ?/?");
			//this.AddFormat(0x0D, "# ??/??");

			//// MD 2/15/11 - TFS66403
			//// This format is different based on the workbook format, so call AddVariantDateFormats, which will add 
			//// the right format.
			////this.AddFormat( 0x0E, "m/d/yy" );
			//// MD 2/27/12 - 12.1 - Table Support
			////this.AddVariantDateFormats(workbookFormat);
			//this.AddVariantDateFormats(this.workbook.CurrentFormat);

			//this.AddFormat(0x0F, "d-mmm-yy");
			//this.AddFormat(0x10, "d-mmm");
			//this.AddFormat(0x11, "mmm-yy");
			//this.AddFormat(0x12, "h:mm AM/PM");
			//this.AddFormat(0x13, "h:mm:ss AM/PM");
			//this.AddFormat(0x14, "h:mm");
			//this.AddFormat(0x15, "h:mm:ss");

			//// MD 2/15/11 - TFS66403
			//// This will get added in the call to AddVariantDateFormats above.
			////this.AddFormat( 0x16, "m/d/yy h:mm" );

			//this.AddFormat(0x25, "(#,##0_);(#,##0)");
			//this.AddFormat(0x26, "(#,##0_);[Red](#,##0)");
			//this.AddFormat(0x27, "(#,##0.00_);(#,##0.00)");
			//this.AddFormat(0x28, "(#,##0.00_);[Red](#,##0.00)");
			//// MD 11/12/07 - BR27987
			//// The format strings are slightly off from what is written out by Excel
			////this.AddFormat( 0x29, "_(* #,##0_);_(* (#,##0);_(* \"-\"_);_(@_)" );
			////this.AddFormat( 0x2A, "_($* #,##0_);_($* (#,##0);_($* \"-\"_);_(@_)" );
			////this.AddFormat( 0x2B, "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)" );
			////this.AddFormat( 0x2C, "_($* #,##0.00_);_($* (#,##0.00);_($* \"-\"??_);_(@_)" );
			//this.AddFormat(0x29, "_(* #,##0_);_(* \\(#,##0\\);_(* \"-\"_);_(@_)");
			//this.AddFormat(0x2A, "_(\"$\"* #,##0_);_(\"$\"* \\(#,##0\\);_(\"$\"* \"-\"_);_(@_)");
			//this.AddFormat(0x2B, "_(* #,##0.00_);_(* \\(#,##0.00\\);_(* \"-\"??_);_(@_)");
			//this.AddFormat(0x2C, "_(\"$\"* #,##0.00_);_(\"$\"* \\(#,##0.00\\);_(\"$\"* \"-\"??_);_(@_)");
			//this.AddFormat(0x2D, "mm:ss");
			//this.AddFormat(0x2E, "[h]:mm:ss");
			//this.AddFormat(0x2F, "mm:ss.0");
			//this.AddFormat(0x30, "##0.0E+0");
			//this.AddFormat(0x31, "@");

			//// MD 11/28/11 - TFS96486
			//// MD 4/6/12 - TFS101506
			////this.AddCultureSpecificFormats();
			//this.AddCultureSpecificFormats(this.workbook.CultureResolved);

			#endregion // Moved
			this.VerifyBuiltInFormats();
		}

		#endregion Constructor

		#region Methods

		#region Add

		internal int Add( string formatString )
		{
			// MD 5/10/12 - TFS104961
			// Make sure the built in formats are based on the current culture before we search for one.
			this.VerifyBuiltInFormats();

			int index;
			// MD 10/19/07
			// Found while fixing BR27421
			// Use the new variable name
			//if ( this.formatHash.TryGetValue( formatString, out index ) )
			if ( this.formatHashByFormat.TryGetValue( formatString, out index ) )
				return index;

			// MD 3/25/12 - TFS104630
			// If the format code matches the current culture's short date (and time) patterns, use the special format code for 
			// those items culture specific formats.
			// MD 4/6/12 - TFS101506
			//string shortDatePattern = WorkbookFormatCollection.GetShortDatePattern();
			string shortDatePattern = WorkbookFormatCollection.GetShortDatePattern(this.workbook.CultureResolved);

			if (String.Equals(shortDatePattern, formatString, StringComparison.InvariantCulture))
				return WorkbookFormatCollection.ShortDateFormatIndex;
			else if (String.Equals(shortDatePattern + WorkbookFormatCollection.ShortTimePatternSuffix, formatString, StringComparison.InvariantCulture))
				return WorkbookFormatCollection.ShortDateAndTimeFormatIndex;

			// MD 10/19/07
			// Found while fixing BR27421
			// This is no longer a collection. Calculate the index and add the format manually
			//index = ( (IList)this ).Add( formatString ) + FormatTableOffset;
			//this.formatHash.Add( formatString, index );
			index = FormatTableOffset + customFormatCount;
			this.AddFormat( index, formatString );

			return index;
		}

		#endregion Add

		// MD 10/19/07
		// Found while fixing BR27421
		#region AddFormat

		internal void AddFormat( int index, string format )
		{
			// MD 9/8/08 - TFS6778
			// Moved all code to the new overload
			this.AddFormat( index, format, false );
		}

		// MD 9/8/08 - TFS6778
		// Added a new overload let the caller allow dupliate format strings accross two or more indexes.
		internal void AddFormat( int index, string format, bool allowDuplicates )
		{
			// MD 5/10/12 - TFS104961
			// Make sure the built in formats are based on the current culture before we search for one.
			this.VerifyBuiltInFormats();

			// MD 3/25/12 - TFS104630
			Debug.Assert(
				index != WorkbookFormatCollection.ShortDateFormatIndex && index != WorkbookFormatCollection.ShortDateAndTimeFormatIndex,
				"We are not expecting the culture specific indexes to have a custom format applied.");

			string existingFormat;
			if ( this.formatHashByIndex.TryGetValue( index, out existingFormat ) )
			{
				if ( StringComparer.InvariantCultureIgnoreCase.Compare( format, existingFormat ) == 0 )
					return;

				this.formatHashByIndex[ index ] = format;
				this.formatHashByFormat.Remove( existingFormat );

				// MD 12/6/11 - 12.1 - Table Support
				// The format may already be in the collection when tables are used.
				if (allowDuplicates)
					this.formatHashByFormat.Remove(format);

				this.formatHashByFormat.Add( format, index );
				return;
			}

			if ( index >= FormatTableOffset )
				customFormatCount++;

			// MD 9/8/08 - TFS6778
			// Make sure the format string is not in the formatHashByFormat dictionary if duplicates are allowed.
			if ( allowDuplicates )
				this.formatHashByFormat.Remove( format );

			this.formatHashByFormat.Add( format, index );
			this.formatHashByIndex.Add( index, format );
		}

		#endregion AddFormat

		// MD 5/10/12 - TFS104961
		// This is no longer needed.
		#region Removed

		//// MD 2/15/11 - TFS66403
		//#region AddVariantDateFormats

		//internal void AddVariantDateFormats(WorkbookFormat workbookFormat)
		//{
		//    // MD 3/25/12 - TFS104630
		//    // This was incorrect or has been changed with an update to Excel 2003, because these codes always use the 4 digit year
		//    // in the English culture, but they actually use the short date pattern for the current culture, so we can't add them to
		//    // our dictionaries because they may change. Instead, the code which gets/sets format codes will use the short date 
		//    // pattern.
		//    //bool is2003Format = Utilities.Is2003Format(workbookFormat);
		//    //this.AddFormat(0x0E, is2003Format ? "m/d/yy" : "m/d/yyyy");
		//    //this.AddFormat(0x16, is2003Format ? "m/d/yy h:mm" : "m/d/yyyy h:mm");
		//}

		//#endregion // AddVariantDateFormats 

		#endregion // Removed

		// MD 2/7/12 - 12.1 - Table Support
		#region FromDxfIndex

		public static int FromDxfIndex(int dxfIndex)
		{
			WorkbookFormatCollection.InitializeDxfMaps();

			int index;
			if (WorkbookFormatCollection.dxfIndexToIndexMap.TryGetValue(dxfIndex, out index))
				return index;

			return dxfIndex;
		}

		#endregion // FromDxfIndex

		// MD 9/9/08 - TFS 6913
		#region GetCustomFormatIndices

		public IEnumerable<int> GetCustomFormatIndices()
		{
			foreach ( KeyValuePair<int, string> formatByIndex in this.formatHashByIndex )
			{
				int index = formatByIndex.Key;

				// MD 2/20/12 - 12.1 - Table Support
				// Moved this code to a method.
				//// Skip the standard formats
				//if ( 0x00 <= index && index <= 0x16 )
				//    continue;
				//
				//if ( 0x25 <= index && index <= 0x31 )
				//    continue;
				if (this.IsStandardFormat(index))
					continue;

				yield return index;
			}
		} 

		#endregion GetCustomFormatIndices

		// MD 3/25/12 - TFS104630
		#region GetShortDatePattern

		// MD 4/6/12 - TFS101506
		//private static string GetShortDatePattern()
		//{
		//    return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("M", "m");
		//}
		private static string GetShortDatePattern(CultureInfo culture)
		{
			return culture.DateTimeFormat.ShortDatePattern.Replace("M", "m");
		}

		#endregion // GetShortDatePattern

		// MD 2/27/12 - 12.1 - Table Support
		#region GetValueFormatter

		internal ValueFormatter GetValueFormatter(int index)
		{
			if (this.valueFormattersByIndex == null)
				this.valueFormattersByIndex = new Dictionary<int, ValueFormatter>();

			ValueFormatter formatter;
			if (this.valueFormattersByIndex.TryGetValue(index, out formatter) == false)
			{
				string format = this[index] ?? WorksheetCellFormatData.DefaultFormatString;

				// MD 4/9/12 - TFS101506
				//formatter = new ValueFormatter(this.workbook, format);
				formatter = new ValueFormatter(this.workbook, format, this.workbook.CultureResolved);

				this.valueFormattersByIndex[index] = formatter;
			}

			return formatter;
		}

		#endregion // GetValueFormatter

		// MD 2/7/12 - 12.1 - Table Support
		#region InitializeDxfMaps

		private static void InitializeDxfMaps()
		{
			if (WorkbookFormatCollection.dxfIndexToIndexMap != null)
				return;

			WorkbookFormatCollection.dxfIndexToIndexMap = new Dictionary<int, int>();
			WorkbookFormatCollection.indexToDxfIndexMap = new Dictionary<int, int>();

			WorkbookFormatCollection.InitializeDxfMapsHelper(0x00, 0x00);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x01, 0x01);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x02, 0x02);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x03, 0x03);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x04, 0x04);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x05, 0x09);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x06, 0x0A);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x07, 0x0B);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x08, 0x0C);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x09, 0x0D);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0A, 0x0E);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0B, 0x0F);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0C, 0x11);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0D, 0x12);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0E, 0x13);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x0F, 0x14);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x10, 0x15);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x11, 0x16);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x12, 0x17);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x13, 0x18);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x14, 0x19);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x15, 0x1A);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x16, 0x1B);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x25, 0x05);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x26, 0x06);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x27, 0x07);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x28, 0x08);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x29, 0x21);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2A, 0x20);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2B, 0x23);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2C, 0x22);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2D, 0x1C);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2E, 0x1F);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x2F, 0x1D);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x30, 0x10);
			WorkbookFormatCollection.InitializeDxfMapsHelper(0x31, 0x1E);
		}

		private static void InitializeDxfMapsHelper(int index, int dxfIndex)
		{
			WorkbookFormatCollection.dxfIndexToIndexMap.Add(dxfIndex, index);
			WorkbookFormatCollection.indexToDxfIndexMap.Add(index, dxfIndex);
		}

		#endregion // InitializeDxfMaps

		// MD 2/20/12 - 12.1 - Table Support
		#region IsStandardFormat

		public bool IsStandardFormat(int formatIndex)
		{
			if (0x00 <= formatIndex && formatIndex <= 0x16)
				return true;

			if (0x25 <= formatIndex && formatIndex <= 0x31)
				return true;

			return false;
		}

		#endregion // IsStandardFormat

		// MD 2/7/12 - 12.1 - Table Support
		#region ToDxfIndex

		public static int ToDxfIndex(int index)
		{
			WorkbookFormatCollection.InitializeDxfMaps();

			int dxfIndex;
			if (WorkbookFormatCollection.indexToDxfIndexMap.TryGetValue(index, out dxfIndex))
				return dxfIndex;

			return index;
		}

		#endregion // ToDxfIndex

		// MD 5/10/12 - TFS104961
		#region VerifyBuiltInFormats

		private void VerifyBuiltInFormats()
		{
			if (this.isVerifyingBuiltInFormats)
				return;

			try
			{
				this.isVerifyingBuiltInFormats = true;

				NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
				string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
				string groupSeparator = numberFormatInfo.NumberGroupSeparator;
				string currencySymbol = numberFormatInfo.CurrencySymbol;

				if (currencySymbol == this.currencySymbolAtInitialization &&
					decimalSeparator == this.decimalSeparatorAtInitialization &&
					groupSeparator == this.groupSeparatorAtInitialization)
					return;

				this.currencySymbolAtInitialization = currencySymbol;
				this.decimalSeparatorAtInitialization = decimalSeparator;
				this.groupSeparatorAtInitialization = groupSeparator;

				this.AddFormat(0x00, "General");
				this.AddFormat(0x01, "0");
				this.AddFormat(0x02, string.Format("0{0}00", this.decimalSeparatorAtInitialization));
				this.AddFormat(0x03, string.Format("#{0}##0", this.groupSeparatorAtInitialization));
				this.AddFormat(0x04, string.Format("#{0}##0{1}00", this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x05, string.Format("\"{0}\"#{1}##0_);\\(\"{0}\"#{1}##0\\)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization));
				this.AddFormat(0x06, string.Format("\"{0}\"#{1}##0_);[Red]\\(\"{0}\"#{1}##0\\)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization));
				this.AddFormat(0x07, string.Format("\"{0}\"#{1}##0{2}00_);\\(\"{0}\"#{1}##0{2}00\\)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x08, string.Format("\"{0}\"#{1}##0{2}00_);[Red]\\(\"{0}\"#{1}##0{2}00\\)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x09, "0%");
				this.AddFormat(0x0A, string.Format("0{0}00%", this.decimalSeparatorAtInitialization));
				this.AddFormat(0x0B, string.Format("0{0}00E+00", this.decimalSeparatorAtInitialization));
				this.AddFormat(0x0C, "# ?/?");
				this.AddFormat(0x0D, "# ??/??");
				this.AddFormat(0x0F, "d-mmm-yy");
				this.AddFormat(0x10, "d-mmm");
				this.AddFormat(0x11, "mmm-yy");
				this.AddFormat(0x12, "h:mm AM/PM");
				this.AddFormat(0x13, "h:mm:ss AM/PM");
				this.AddFormat(0x14, "h:mm");
				this.AddFormat(0x15, "h:mm:ss");
				this.AddFormat(0x25, string.Format("(#{0}##0_);(#{0}##0)", this.groupSeparatorAtInitialization));
				this.AddFormat(0x26, string.Format("(#{0}##0_);[Red](#{0}##0)", this.groupSeparatorAtInitialization));
				this.AddFormat(0x27, string.Format("(#{0}##0{1}00_);(#{0}##0{1}00)", this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x28, string.Format("(#{0}##0{1}00_);[Red](#{0}##0{1}00)", this.groupSeparatorAtInitialization, decimalSeparatorAtInitialization));
				this.AddFormat(0x29, string.Format("_(* #{0}##0_);_(* \\(#{0}##0\\);_(* \"-\"_);_(@_)", this.groupSeparatorAtInitialization));
				this.AddFormat(0x2A, string.Format("_(\"{0}\"* #{1}##0_);_(\"{0}\"* \\(#{1}##0\\);_(\"{0}\"* \"-\"_);_(@_)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization));
				this.AddFormat(0x2B, string.Format("_(* #{0}##0{1}00_);_(* \\(#{0}##0{1}00\\);_(* \"-\"??_);_(@_)", this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x2C, string.Format("_(\"{0}\"* #{1}##0{2}00_);_(\"{0}\"* \\(#{1}##0{2}00\\);_(\"{0}\"* \"-\"??_);_(@_)", this.currencySymbolAtInitialization, this.groupSeparatorAtInitialization, this.decimalSeparatorAtInitialization));
				this.AddFormat(0x2D, "mm:ss");
				this.AddFormat(0x2E, "[h]:mm:ss");
				this.AddFormat(0x2F, string.Format("mm:ss{0}0", this.decimalSeparatorAtInitialization));
				this.AddFormat(0x30, string.Format("##0{0}0E+0", this.decimalSeparatorAtInitialization));
				this.AddFormat(0x31, "@");
				this.AddCultureSpecificFormats(this.workbook.CultureResolved);
			}
			finally
			{
				this.isVerifyingBuiltInFormats = false;
			}
		}

		#endregion // VerifyBuiltInFormats

		#endregion Methods

		#region Properties

		// MD 10/19/07
		// Found while fixing BR27421
		// Since we are not a collection anymore, we need to define the Count property
		#region Count

		internal int Count
		{
			get { return this.customFormatCount; }
		}

		#endregion Count

		#region Indexer[ int ]

		internal string this[ int index ]
		{
			get
			{
				// MD 10/19/07
				// Found while fixing BR27421
				// Since we are not a collection anymore, we need to get the index manually
				//int realIndex = index - FormatTableOffset;
				//
				//if ( realIndex < 0 )
				//    return null;
				//
				//return (string)( ( (IList)this )[ realIndex ] );
				if ( index < 0 )
					return null;

				// MD 3/25/12 - TFS104630
				// If the special codes for culture specific dates is supplied, generate the format string based on the current culture.
				if (index == WorkbookFormatCollection.ShortDateFormatIndex ||
					index == WorkbookFormatCollection.ShortDateAndTimeFormatIndex)
				{
					Debug.Assert(this.formatHashByIndex.ContainsKey(index) == false, "These indexes should not be in the dictionary.");

					// MD 4/6/12 - TFS101506
					//string shortDatePattern = WorkbookFormatCollection.GetShortDatePattern();
					string shortDatePattern = WorkbookFormatCollection.GetShortDatePattern(this.workbook.CultureResolved);

					if (index == WorkbookFormatCollection.ShortDateFormatIndex)
						return shortDatePattern;

					return shortDatePattern + WorkbookFormatCollection.ShortTimePatternSuffix;
				}

				// MD 5/10/12 - TFS104961
				// Make sure the built in formats are based on the current culture before we search for them.
				this.VerifyBuiltInFormats();

				string format;
				if ( this.formatHashByIndex.TryGetValue( index, out format ) )
					return format;

				// MD 11/28/11 - TFS96486
				// Return defaults for the Asian culture specific date formats.
				if (27 <= index && index <= 36)
				{
					if (index == 32 || index == 33)
						return "h:mm:ss";

					return "m/d/yyyy";
				}

				// MD 11/28/11 - TFS96486
				// Return defaults for the Thailand culture specific date formats.
				if (50 <= index && index <= 81)
					return "m/d/yyyy";

				Utilities.DebugFail( "Invalid index" );
				return null;
			}
		}

		#endregion Indexer[ int ]

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