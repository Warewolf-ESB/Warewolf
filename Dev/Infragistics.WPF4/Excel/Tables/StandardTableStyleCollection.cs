using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;
using System.Diagnostics;
using System.Globalization;




using System.IO.Compression;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// A collection of standard <see cref="WorksheetTableStyle"/> instances which can be applied to a <see cref="WorksheetTable"/> in a 
	/// <see cref="Workbook"/>.
	/// </summary>
	/// <seealso cref="Workbook.DefaultTableStyle"/>
	/// <seealso cref="Workbook.StandardTableStyles"/>
	[DebuggerDisplay("StandardTableStyleCollection: Count = {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		class StandardTableStyleCollection : IEnumerable<WorksheetTableStyle>
	{
		#region Static Variables

		private static readonly object InstanceLock = new object();
		private static StandardTableStyleCollection _instance;

		#endregion // Static Variables

		#region Member Variables

		private List<WorksheetTableStyle> _standardTableStyles;

		#endregion // Member Variables

		#region Constructor

		private StandardTableStyleCollection() { }

		#endregion // Constructor

		#region Interfaces

		#region IEnumerable<WorksheetTableStyle> Members

		IEnumerator<WorksheetTableStyle> IEnumerable<WorksheetTableStyle>.GetEnumerator()
		{
			return _standardTableStyles.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _standardTableStyles.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Private Methods

		#region InitializeStandardTableStyles

		private void InitializeStandardTableStyles()
		{
			_standardTableStyles = new List<WorksheetTableStyle>();

			ExcelXmlDocument document = StandardTableStyleCollection.LoadPresetTableStylesDocument();
			if (document == null)
				return;

			Workbook tempWorkbook = new Workbook(WorkbookFormat.Excel2007);


			XmlReader reader = document.Reader;
			bool isReaderOnNextNode = false;
			while (true)
			{
				if (isReaderOnNextNode == false && reader.Read() == false)
					break;

				bool isEmptyElement;
				ExcelXmlNode childNode = XmlElementBase.CreateNextNode(document, reader, out isReaderOnNextNode, out isEmptyElement);
				if (childNode != null && childNode.LocalName == "presetTableStyles")
					break;

				if (reader.EOF)
				{
					Utilities.DebugFail("We did not find the presetTableStyles element.");
					return;
				}
			}

			while (true)
			{
				if (isReaderOnNextNode == false && reader.Read() == false)
					break;

				bool isEmptyElement;
				ExcelXmlNode childNode = XmlElementBase.CreateNextNode(document, reader, out isReaderOnNextNode, out isEmptyElement);
				if (childNode == null)
					continue;

				string tableStyleName = childNode.Name;
				WorksheetTableStyle tableStyle = new WorksheetTableStyle(tableStyleName, false);

				using (XLSXWorkbookSerializationManager manager = new XLSXWorkbookSerializationManager(null, tempWorkbook, (string)null))
				{
					manager.IsLoadingPresetTableStyles = true;
					tableStyle.IsLoading = true;
					manager.ContextStack.Push(tableStyle);
					XmlElementBase.LoadChildElements(manager, childNode, ref isReaderOnNextNode);
					manager.ContextStack.Pop(); // tableStyle
					tableStyle.IsLoading = false;
				}

				if (tableStyle.Name.StartsWith("Table"))
					_standardTableStyles.Add(tableStyle);
			}

			reader.Close();
		}

		#endregion // InitializeStandardTableStyles

		#region LoadPresetTableStylesDocument

		private static ExcelXmlDocument LoadPresetTableStylesDocument()
		{
			MemoryStream unzippedData = new MemoryStream();

			using (Stream presetTableStylesZipStream =
				typeof(StandardTableStyleCollection).Assembly.GetManifestResourceStream(Utilities.RootEmbeddedResourcePath + "Tables.presetTableStyles.xml.gz"))
			{
				if (presetTableStylesZipStream == null)
				{
					Utilities.DebugFail("Cannot find the presetTableStyles.xml.gz stream.");
					return null;
				}

				try
				{
					using (GZipStream unzipStream = new GZipStream(presetTableStylesZipStream, CompressionMode.Decompress))
					{
						byte[] buffer = new byte[4096];
						while (true)
						{
							int bytesRead = unzipStream.Read(buffer, 0, buffer.Length);
							if (bytesRead == 0)
								break;

							unzippedData.Write(buffer, 0, bytesRead);
						}
					}
				}
				catch (Exception ex)
				{
					Utilities.DebugFail("Exception when loading ppresetTableStyles.xml.gzre file (this may be expected on SL): " + ex.ToString());
				}
			}

			unzippedData.Position = 0;

			ExcelXmlDocument document = new ExcelXmlDocument(unzippedData, true);
			return document;
		}

		#endregion // LoadPresetTableStylesDocument

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of custom <see cref="WorksheetTableStyle"/> instances in the collection.
		/// </summary>
		public int Count
		{
			get { return _standardTableStyles.Count; }
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
			get { return _standardTableStyles[index]; }
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
				if (String.IsNullOrEmpty(name))
					return null;

				// MD 4/9/12 - TFS101506
				CultureInfo culture = CultureInfo.CurrentCulture;

				for (int i = 0; i < _standardTableStyles.Count; i++)
				{
					// MD 4/9/12 - TFS101506
					//if (String.Equals(name, _standardTableStyles[i].Name, StringComparison.CurrentCultureIgnoreCase))
					if (String.Compare(name, _standardTableStyles[i].Name, culture, CompareOptions.IgnoreCase) == 0)
						return _standardTableStyles[i];
				}

				return null;
			}
		}

		#endregion // Indexer[string]

		#endregion // Public Properties

		#region Internal Properties

		#region DefaultTableStyle

		internal WorksheetTableStyle DefaultTableStyle
		{
			get { return this["TableStyleMedium2"]; }
		}

		#endregion // DefaultTableStyle

		#region Instance

		internal static StandardTableStyleCollection Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (StandardTableStyleCollection.InstanceLock)
					{
						if (_instance == null)
						{
							// MD 4/5/12 - TFS108269
							// Don't set _instance to the collection until it is initialized because we want other threads to
							// Get to the lock above while we are initializing.
							//_instance = new StandardTableStyleCollection();
							//_instance.InitializeStandardTableStyles();
							StandardTableStyleCollection instance = new StandardTableStyleCollection();
							instance.InitializeStandardTableStyles();
							_instance = instance;
						}
					}
				}

				return _instance;
			}
		}

		#endregion // Instance

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