using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;





using System.IO.Compression;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of workbook styles, or complex formats which can be easily applied to cells in Microsoft Excel.
	/// </summary>
	/// <seealso cref="WorkbookStyle"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class WorkbookStyleCollection : 
		ICollection<WorkbookStyle>
	{
		#region Static Variables

		// MD 12/28/11 - 12.1 - Cell Format Updates
		private static readonly object PresetCellStylesLock = new object();
		private static List<WorkbookBuiltInStyle> _presetCellStyles;
		private static Dictionary<string, BuiltInStyleInfo> _styleTypesByName;

		#endregion // Static Variables

		#region Member Variables

		// MD 1/1/12 - 12.1 - Cell Format Updates
		private WorkbookStyle[] colLevelStyles;
		private Dictionary<BuiltInStyleType, WorkbookBuiltInStyle> hiddenStylesByType;
		private WorkbookStyle[] rowLevelStyles;

		private List<WorkbookStyle> styles;

		// MD 12/27/11 - TFS98569
		private Dictionary<string, WorkbookStyle> stylesByName;

		// MD 1/1/12 - 12.1 - Cell Format Updates
		private Dictionary<BuiltInStyleType, WorkbookStyle> stylesByType;

		private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		internal WorkbookStyleCollection( Workbook workbook )
		{
			// MD 1/1/12 - 12.1 - Cell Format Updates
			this.colLevelStyles = new WorkbookStyle[8];
			this.rowLevelStyles = new WorkbookStyle[8];

			this.styles = new List<WorkbookStyle>();

			// MD 12/27/11 - TFS98569
			// MD 4/6/12 - TFS101506
			//this.stylesByName = new Dictionary<string, WorkbookStyle>(StringComparer.CurrentCultureIgnoreCase);
			this.stylesByName = new Dictionary<string, WorkbookStyle>(StringComparer.Create(workbook.CultureResolved, true));

			// MD 1/1/12 - 12.1 - Cell Format Updates
			this.stylesByType = new Dictionary<BuiltInStyleType, WorkbookStyle>();

			this.workbook = workbook;

			// MD 1/1/12 - 12.1 - Cell Format Updates
			// Load the preset cells styles and make sure we have a normal style.
			List<WorkbookBuiltInStyle> presetCellStyles = WorkbookStyleCollection.PresetCellStyles;
			for (int i = 0; i < presetCellStyles.Count; i++)
				this.Add(presetCellStyles[i].Clone(this.workbook));

			this.EnsureNormalStyle();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorkbookStyle> Members

		void ICollection<WorkbookStyle>.Add( WorkbookStyle item )
		{
			throw new InvalidOperationException( "Styles cannot be added directly to the collection." );
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed. This is redundant.
		//void ICollection<WorkbookStyle>.Clear()
		//{
		//    this.Clear();
		//}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed. This is redundant.
		//bool ICollection<WorkbookStyle>.Contains( WorkbookStyle item )
		//{
		//    return this.Contains( item );
		//}

		void ICollection<WorkbookStyle>.CopyTo( WorkbookStyle[] array, int arrayIndex )
		{
			this.styles.CopyTo( array, arrayIndex );
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed. This is redundant.
		//int ICollection<WorkbookStyle>.Count
		//{
		//    get { return this.Count; }
		//}

		bool ICollection<WorkbookStyle>.IsReadOnly
		{
			get { return false; }
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// Removed. This is redundant.
		//bool ICollection<WorkbookStyle>.Remove( WorkbookStyle item )
		//{
		//    return this.Remove( item );
		//}

		#endregion

		#region IEnumerable<WorkbookStyle> Members

		IEnumerator<WorkbookStyle> IEnumerable<WorkbookStyle>.GetEnumerator()
		{
			return this.styles.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.styles.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region AddUserDefinedStyle

		// MD 1/1/12 - 12.1 - Cell Format Updates
		// Added an overload of AddUserDefinedStyle which just takes a name.
		/// <summary>
		/// Adds new user defined style to the workbook.
		/// </summary>
		/// <param name="name">The name which will identify the style in Microsoft Excel.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A style already exists with the a name of <paramref name="name"/>. Names are compared case-insensitively.
		/// </exception>
		/// <returns>The added user defined style as a <see cref="WorkbookStyle"/> instance.</returns>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public WorkbookStyle AddUserDefinedStyle(string name)
		{
			return this.AddUserDefinedStyle(this.workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.StyleFormat), name);
		}

		/// <summary>
		/// Adds new user defined style to the workbook.
		/// </summary>
		/// <param name="styleFormat">A cell format of the style.</param>
		/// <param name="name">The name which will identify the style in Microsoft Excel.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A style already exists with the a name of <paramref name="name"/>. Names are compared case-insensitively.
		/// </exception>
		/// <returns>The added user defined style as a <see cref="WorkbookStyle"/> instance.</returns>
		public WorkbookStyle AddUserDefinedStyle( IWorksheetCellFormat styleFormat, string name )
		{
			// MD 1/14/12 - 12.1 - Cell Format Updates
			// Moved this code to a helper method.
			#region Moved

			//if ( String.IsNullOrEmpty( name ) )
			//    throw new ArgumentNullException( "name", SR.GetString( "LE_ArgumentNullException_StyleName" ) );
			//
			//// MD 12/27/11 - TFS98569
			//// This is a O(N) algorithm, which turns into a O(N^2) algorithm when N styles are added into the collection.
			//// Now we will use a dictionary to make this a O(1) operation, which turns into a O(N) operation when N styles are added.
			////foreach ( WorkbookStyle style in this.styles )
			////{
			////    if ( String.Compare( style.Name, name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
			////        throw new ArgumentException( SR.GetString( "LE_ArgumentException_StyleNameAlreadyExists", name ), "name" );
			////}
			//if (this.stylesByName.ContainsKey(name))
			//    throw new ArgumentException(SR.GetString("LE_ArgumentException_StyleNameAlreadyExists", name), "name");

			#endregion // Moved
			this.ValidateNewStyleName(name, "name");

			// MD 1/14/12 - 12.1 - Cell Format Updates
			// If a style is added with a name that matches a built in style name, we need to create a built in style instead.
			//WorkbookUserDefinedStyle wsStyle = new WorkbookUserDefinedStyle( this.workbook, styleFormat, name );
			WorkbookStyle wsStyle;
			BuiltInStyleInfo info;
			if (WorkbookStyleCollection.StyleTypesByName.TryGetValue(name, out info))
				wsStyle = new WorkbookBuiltInStyle(this.workbook, styleFormat, info.Type, info.OutlineLevel);
			else
				wsStyle = new WorkbookUserDefinedStyle(this.workbook, styleFormat, name);

			// MD 12/27/11 - TFS98569
			// Use the Add method so the style can be added to the dictionary as well.
			//this.styles.Add( wsStyle );
			this.Add(wsStyle);

			return wsStyle;
		}

		#endregion AddUserDefinedStyle

		#region Clear

		/// <summary>
		/// Clears all style, other than the Normal style, from the collection.
		/// </summary>
		/// <see cref="AddUserDefinedStyle(string)"/>
		/// <see cref="AddUserDefinedStyle(IWorksheetCellFormat,string)"/>
		/// <seealso cref="NormalStyle"/>
		public void Clear()
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			//for ( int i = this.styles.Count - 1; i >= 0; i-- )
			//    this.RemoveAt( i );
			this.Reset(RemovalType.HideBuiltInStyles);
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether a style is in the collection.
		/// </summary>
		/// <param name="style">The style to locate in the collection.</param>
		/// <returns>True if the style is found; False otherwise.</returns>
		public bool Contains( WorkbookStyle style )
		{
			// MD 12/27/11 - TFS98569
			// Instead of doing a linear search, we can not do a constant time search with the dictionary.
			//return this.styles.Contains( style );
			WorkbookStyle foundStyle;
			if (this.stylesByName.TryGetValue(style.Name, out foundStyle))
				return style == foundStyle;

			return false;
		}

		#endregion Contains

		#region Remove

		/// <summary>
		/// Removes the specified style from the collection.
		/// </summary>
		/// <param name="style">The style to remove from the collection.</param>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="style"/> is the Normal style.
		/// </exception>
		/// <returns>
		/// True if the style was successfully removed; False if the style was not 
		/// in the collection.
		/// </returns>
		/// <seealso cref="NormalStyle"/>
		public bool Remove( WorkbookStyle style )
		{
			if ( style == null )
				return false;

			int index = this.styles.IndexOf( style );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the style at the specified index from the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the style in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The style to remove at <paramref name="index"/> is the Normal style.
		/// </exception>
		/// <seealso cref="NormalStyle"/>
		public void RemoveAt( int index )
		{
			// MD 1/1/12 - 12.1 - Cell Format Updates
			// Moved all code to the new RemoveAt overload.
			this.RemoveAt(index, RemovalType.HideBuiltInStyles);
		}

		// MD 1/1/12 - 12.1 - Cell Format Updates
		// Added an overload which allows built in styles to be removed.
		private void RemoveAt(int index, RemovalType removalType)
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			WorkbookStyle style = this.styles[ index ];

			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			if (builtInStyle != null)
			{
				Debug.Assert(removalType != RemovalType.ResetBuiltInStyles, "We should not be removing built in styles when the removal type is ResetBuiltInStyles.");
				if (builtInStyle.IsNormalStyle)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotRemoveNormalStyle"));
			}

			style.OnRemovedFromCollection();
			this.styles.RemoveAt( index );

			// MD 12/27/11 - TFS98569
			// Also remove the style from the dictionary.
			this.stylesByName.Remove(style.Name);

			// MD 1/2/12 - 12.1 - Cell Format Updates
			if (builtInStyle != null)
			{
				switch (builtInStyle.Type)
				{
					case BuiltInStyleType.ColLevelX:
						this.colLevelStyles[builtInStyle.OutlineLevel] = null;
						break;

					case BuiltInStyleType.RowLevelX:
						this.rowLevelStyles[builtInStyle.OutlineLevel] = null;
						break;

					default:
						this.stylesByType.Remove(builtInStyle.Type);

						if (removalType == RemovalType.HideBuiltInStyles)
						{
							if (this.hiddenStylesByType == null)
								this.hiddenStylesByType = new Dictionary<BuiltInStyleType, WorkbookBuiltInStyle>();

							this.hiddenStylesByType.Add(builtInStyle.Type, builtInStyle);
						}
						break;
				}
			}

			this.workbook.OnStyleRemoved(style);
		}

		#endregion RemoveAt

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region Reset

		/// <summary>
		/// Resets the collection to its original state by clearing all styles and adding in preset built in styles.
		/// </summary>
		/// <see cref="AddUserDefinedStyle(string)"/>
		/// <see cref="AddUserDefinedStyle(IWorksheetCellFormat,string)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public void Reset()
		{
			this.Reset(RemovalType.ResetBuiltInStyles);

			if (this.hiddenStylesByType != null)
			{
				IEnumerable<WorkbookBuiltInStyle> hiddenStyles = this.hiddenStylesByType.Values;
				this.hiddenStylesByType = null;

				foreach (WorkbookBuiltInStyle builtInStyle in hiddenStyles)
				{
					builtInStyle.Reset();
					this.Add(builtInStyle);
				}
			}

			List<WorkbookBuiltInStyle> presetCellStyles = WorkbookStyleCollection.PresetCellStyles;
			for (int i = 0; i < presetCellStyles.Count; i++)
			{
				WorkbookBuiltInStyle presetStyle = presetCellStyles[i];
				if (this.ContainsBuiltInStyle(presetStyle.Type))
					continue;

				this.Add(presetStyle.Clone(this.workbook));
			}
		}

		private void Reset(RemovalType removalType)
		{
			for (int i = this.styles.Count - 1; i >= 0; i--)
			{
				WorkbookStyle style = this.styles[i];
				if (style.IsBuiltIn)
				{
					// Always reset the normal style, regardless of the ResetType passed in.
					if (removalType == RemovalType.ResetBuiltInStyles || style.IsNormalStyle)
					{
						style.Reset();
						continue;
					}
				}

				this.RemoveAt(i, removalType);
			}
		}

		#endregion // Reset

		#endregion Public Methods

		#region Internal Methods

		#region Add

		// MD 1/1/12 - 12.1 - Cell Format Updates
		// Added a return value.
		//internal void Add( WorkbookStyle style )
		internal WorkbookStyle Add(WorkbookStyle style)
		{
			// MD 12/28/11 - 12.1 - Cell Format Updates
			// If the style has the same name as a built in style, replace the style format, otherwise, replace the whole style.
			WorkbookStyle existingStyle = this[style.Name];
			if (existingStyle != null)
			{
				if (existingStyle.IsBuiltIn)
				{
					existingStyle.StyleFormatInternal.SetFormatting(style.StyleFormatInternal);
					existingStyle.StyleFormatInternal.FormatOptions = style.StyleFormatInternal.FormatOptions;
					return existingStyle;
				}

				this.Remove(existingStyle);
			}

			this.styles.Add( style );

			style.OnAddedToCollection();

			// MD 12/27/11 - TFS98569
			// Also add the style to the dictionary.
			this.stylesByName[style.Name] = style;

			// MD 1/1/12 - 12.1 - Cell Format Updates
			// Cache the built in styles in their own collections as well.
			WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
			if (builtInStyle != null)
			{
				switch (builtInStyle.Type)
				{
					case BuiltInStyleType.ColLevelX:
						this.colLevelStyles[builtInStyle.OutlineLevel] = builtInStyle;
						break;

					case BuiltInStyleType.RowLevelX:
						this.rowLevelStyles[builtInStyle.OutlineLevel] = builtInStyle;
						break;

					default:
						this.stylesByType[builtInStyle.Type] = builtInStyle;
						break;
				}

				if (this.hiddenStylesByType != null)
					this.hiddenStylesByType.Remove(builtInStyle.Type);
			}

			// MD 1/1/12 - 12.1 - Cell Format Updates
			return style;
		}

		#endregion Add

		// MD 2/4/12 - 12.1 - Cell Format Updates
		#region AddHiddenStyle

		internal WorkbookStyle AddHiddenStyle(WorkbookBuiltInStyle style)
		{
			WorkbookStyle existingStyle = this[style.Name];
			if (existingStyle != null)
			{
				Debug.Assert(existingStyle.IsBuiltIn, "The existing style should be built in.");
				existingStyle.StyleFormatInternal.SetFormatting(style.StyleFormatInternal);
				existingStyle.StyleFormatInternal.FormatOptions = style.StyleFormatInternal.FormatOptions;
				this.Remove(existingStyle);
				return existingStyle;
			}

			if (style.UsesOutlineLevel)
			{
				Utilities.DebugFail("These built in styles should not be saved as hidden.");
				return style;
			}

			if (this.hiddenStylesByType == null)
				this.hiddenStylesByType = new Dictionary<BuiltInStyleType, WorkbookBuiltInStyle>();

			WorkbookBuiltInStyle existingHiddenStyle;
			if (this.hiddenStylesByType.TryGetValue(style.Type, out existingHiddenStyle))
			{
				existingHiddenStyle.StyleFormatInternal.SetFormatting(style.StyleFormatInternal);
				existingHiddenStyle.StyleFormatInternal.FormatOptions = style.StyleFormatInternal.FormatOptions;
				return existingHiddenStyle;
			}

			this.hiddenStylesByType.Add(style.Type, style);
			return style;
		}

		#endregion AddHiddenStyle

		// MD 11/12/07 - BR27987
		#region Clone

		internal WorkbookStyleCollection Clone()
		{
			WorkbookStyleCollection clone = new WorkbookStyleCollection( this.workbook );

			// MD 12/27/11 - TFS98569
			// Add each style manually so it can be added to the dictionary as well.
			//clone.styles.AddRange( this.styles );
			for (int i = 0; i < this.styles.Count; i++)
				clone.Add(this.styles[i]);

			return clone;
		}

		#endregion Clone

		// MD 11/12/07 - BR27987
		#region ContainsBuiltInStyle







		internal bool ContainsBuiltInStyle( BuiltInStyleType builtInStyleType )
		{
			foreach ( WorkbookStyle style in this.styles )
			{
				WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;

				if ( builtInStyle != null && builtInStyle.Type == builtInStyleType )
					return true;
			}

			return false;
		}

		#endregion ContainsBuiltInStyle

		// MD 1/1/12 - 12.1 - Cell Format Updates
		#region GetBuiltInStyle

		internal WorkbookStyle GetBuiltInStyle(BuiltInStyleType type)
		{
			WorkbookStyle style;
			this.stylesByType.TryGetValue(type, out style);
			return style;
		}

		internal WorkbookStyle GetBuiltInStyle(BuiltInStyleType type, int outlineLevel)
		{
			switch (type)
			{
				case BuiltInStyleType.ColLevelX:
					return this.colLevelStyles[outlineLevel];

				case BuiltInStyleType.RowLevelX:
					return this.rowLevelStyles[outlineLevel];

				default:
					return this.GetBuiltInStyle(type);
			}
		}

		#endregion // GetBuiltInStyle

		// MD 2/4/12 - 12.1 - Cell Format Updates
		#region GetHiddenStyles

		internal IEnumerable<WorkbookBuiltInStyle> GetHiddenStyles()
		{
			if (this.hiddenStylesByType != null)
			{
				foreach (WorkbookBuiltInStyle style in this.hiddenStylesByType.Values)
					yield return style;
			}
		}

		#endregion // GetHiddenStyles

		// MD 1/15/12 - 12.1 - Cell Format Updates
		#region OnStyleRenamed

		internal void OnStyleRenamed(WorkbookUserDefinedStyle style, string oldName)
		{
			Debug.Assert(this.stylesByName[oldName] == style, "Something is wrong here.");
			this.stylesByName.Remove(oldName);
			this.stylesByName.Add(style.Name, style);
		}

		#endregion // OnStyleRenamed

		// MD 1/14/12 - 12.1 - Cell Format Updates
		#region ValidateNewStyleName

		internal void ValidateNewStyleName(string name, string paramName)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException(paramName, SR.GetString("LE_ArgumentNullException_StyleName"));

			if (WorkbookStyle.MaxNameLength < name.Length)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_StyleNameTooLong"), paramName);

			if (this.stylesByName.ContainsKey(name))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_StyleNameAlreadyExists", name), paramName);
		}

		#endregion // ValidateNewStyleName

		#endregion Internal Methods

		#region Private Methods

		// MD 1/1/12 - 12.1 - Cell Format Updates
		#region EnsureNormalStyle

		private void EnsureNormalStyle()
		{
			if (this.ContainsBuiltInStyle(BuiltInStyleType.Normal) == false)
				this.Add(new WorkbookBuiltInStyle(this.workbook, new WorksheetCellFormatData(this.workbook, WorksheetCellFormatType.StyleFormat), BuiltInStyleType.Normal, 0));
		}

		#endregion // EnsureNormalStyle

		// MD 12/28/11 - 12.1 - Cell Format Updates
		#region InitializePresetCellStyles

		private static void InitializePresetCellStyles()
		{
			_presetCellStyles = new List<WorkbookBuiltInStyle>();

			ExcelXmlDocument document = WorkbookStyleCollection.LoadPresetCellStylesDocument();
			if (document == null)
				return;

			Workbook tempWorkbook = new Workbook(WorkbookFormat.Excel2007);


			XmlReader reader = document.Reader;
			bool isReaderOnNextNode = false;
			while (true)
			{
				if (isReaderOnNextNode == false && reader.Read() == false)
					break;

				if (reader != null && reader.LocalName == "styleSheet")
				{
					isReaderOnNextNode = true;
					using (XLSXWorkbookSerializationManager manager = new XLSXWorkbookSerializationManager(null, tempWorkbook, (string)null))
						XmlElementBase.LoadChildElements(manager, new ExcelXmlNode(document), ref isReaderOnNextNode);

					foreach (WorkbookStyle style in tempWorkbook.Styles)
					{
						WorkbookBuiltInStyle builtInStyle = style as WorkbookBuiltInStyle;
						if (builtInStyle == null)
						{
							Utilities.DebugFail("All loaded styles should be built in.");
							continue;
						}

						_presetCellStyles.Add(builtInStyle);
					}

					_presetCellStyles.Sort();
					break;
				}

				if (reader.EOF)
				{
					Utilities.DebugFail("We did not find the styleSheet element.");
					return;
				}
			}

			// Reset the _styleTypesByName collection so it can be repopulated the next time it is requested.
			_styleTypesByName = null;

			reader.Close();
		}

		#endregion // InitializePresetCellStyles

		// MD 12/28/11 - 12.1 - Cell Format Updates
		#region LoadPresetTableStylesDocument

		private static ExcelXmlDocument LoadPresetCellStylesDocument()
		{
			MemoryStream unzippedData = new MemoryStream();

			using (Stream presetCellStylesZipStream =
				typeof(StandardTableStyleCollection).Assembly.GetManifestResourceStream(Utilities.RootEmbeddedResourcePath + "presetCellStyles.xml.gz"))
			{
				if (presetCellStylesZipStream == null)
				{
					Utilities.DebugFail("Cannot find the presetCellStyles.xml.gz stream.");
					return null;
				}

				try
				{
					using (GZipStream unzipStream = new GZipStream(presetCellStylesZipStream, CompressionMode.Decompress))
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
					Utilities.DebugFail("Exception when loading presetCellStyles.xml.gzre file (this may be expected on SL): " + ex.ToString());
				}
			}

			unzippedData.Position = 0;

			ExcelXmlDocument document = new ExcelXmlDocument(unzippedData, true);
			return document;
		}

		#endregion // LoadPresetTableStylesDocument

		#endregion // Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of styles in the collection.
		/// </summary>
		/// <value>The number of styles in the collection.</value>
		public int Count
		{
			get { return this.styles.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the style at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the style to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The style at the specified index.</value>
		public WorkbookStyle this[ int index ]
		{
			get { return this.styles[ index ]; }
		}

		#endregion Indexer [ int ]

		#region Indexer [ string ]

		/// <summary>
		/// Gets the style with the specified name.
		/// </summary>
		/// <param name="name">The name of the style to get.</param>
		/// <remarks>
		/// <p class="body">
		/// Style names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null.
		/// </exception>
		/// <value>The style with the specified name or null if no style with that name exists.</value>
		public WorkbookStyle this[string name]
		{
			get 
			{
				if (name == null)
					throw new ArgumentNullException("name");

				WorkbookStyle style;
				this.stylesByName.TryGetValue(name, out style);
				return style;
			}
		}

		#endregion Indexer [ string ]

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region NormalStyle

		/// <summary>
		/// Gets the default style for the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The normal style is the parent style for all cell and differential formats in the workbook, unless another parent style is specified.
		/// </p>
		/// </remarks>
		/// <seealso cref="IWorksheetCellFormat.Style"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public WorkbookStyle NormalStyle
		{
			get { return this.GetBuiltInStyle(BuiltInStyleType.Normal); }
		}

		#endregion // NormalStyle

		#endregion // Public Properties

		#region Internal Properties

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region StyleTypesByName

		internal static Dictionary<string, BuiltInStyleInfo> StyleTypesByName
		{
			get
			{
				if (_styleTypesByName == null)
				{
					// Use the PresetCellStylesLock here instead of a separate lock so we don't get into a race condition with 
					// another thread when asking for the PresetCellStyles below.
					lock (WorkbookStyleCollection.PresetCellStylesLock)
					{
						if (_styleTypesByName == null)
						{
							Dictionary<string, BuiltInStyleInfo> styleTypesByName = new Dictionary<string, BuiltInStyleInfo>(StringComparer.CurrentCultureIgnoreCase);

							List<WorkbookBuiltInStyle> presetCellStyles = WorkbookStyleCollection.PresetCellStyles;
							for (int i = 0; i < presetCellStyles.Count; i++)
							{
								WorkbookBuiltInStyle style = presetCellStyles[i];
								styleTypesByName[style.Name] = new BuiltInStyleInfo(style.Type, style.OutlineLevel);
							}

							_styleTypesByName = styleTypesByName;
						}
					}
				}

				return _styleTypesByName;
			}
		}

		#endregion // StyleTypesByName

		#endregion // Internal Properties

		#region Private Properties

		// MD 12/28/11 - 12.1 - Cell Format Updates
		#region PresetCellStyles

		private static List<WorkbookBuiltInStyle> PresetCellStyles
		{
			get
			{
				if (_presetCellStyles == null)
				{
					lock (PresetCellStylesLock)
					{
						if (_presetCellStyles == null)
							WorkbookStyleCollection.InitializePresetCellStyles();
					}
				}

				return _presetCellStyles;
			}
		} 

		#endregion // PresetCellStyles

		#endregion // Private Properties

		#endregion Properties


		#region BuiltInStyleInfo struct

		internal struct BuiltInStyleInfo
		{
			public readonly BuiltInStyleType Type;
			public readonly byte OutlineLevel;

			public BuiltInStyleInfo(BuiltInStyleType type, byte outlineLevel)
			{
				this.Type = type;
				this.OutlineLevel = outlineLevel;
			}
		}

		#endregion // BuiltInStyleInfo struct

		#region RemovalType enum

		private enum RemovalType
		{
			HideBuiltInStyles,
			RemoveBuiltInStyles,
			ResetBuiltInStyles,
		}

		#endregion // RemovalType enum
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