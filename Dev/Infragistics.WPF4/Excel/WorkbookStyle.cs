using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;


using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/8/12 - 12.1 - Cell Format Updates
	// The code we changed too much for the cell format updates, so I just commented out the old code.
	#region Old Code

	//    /// <summary>
	//    /// Represents a complex format which can be easily applied to a cell in Microsoft Excel.
	//    /// </summary>
	//#if REPORTING
	//    internal
	//#else
	//    public
	//#endif
	//         abstract class WorkbookStyle
	//    {
	//        #region Member Variables

	//        private WorksheetCellFormatProxy styleFormat;

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 11/12/07 - BR27987
	//        // Added a parameter to the constructor
	//        //internal WorkbookStyle( Workbook workbook, IWorksheetCellFormat styleFormat )
	//        internal WorkbookStyle( Workbook workbook, IWorksheetCellFormat styleFormat, bool resolveFormat )
	//        {
	//            WorksheetCellFormatData cellFormatData = styleFormat as WorksheetCellFormatData;

	//            if ( cellFormatData == null )
	//            {
	//                WorksheetCellFormatProxy proxy = styleFormat as WorksheetCellFormatProxy;

	//                if ( proxy != null )
	//                {
	//                    cellFormatData = proxy.Element;
	//                }
	//                else
	//                {
	//                    Utilities.DebugFail( "Unknown IWorksheetCellFormat implementation." );
	//                    cellFormatData = workbook.CellFormats.DefaultElement;
	//                }
	//            }

	//            // MD 11/12/07 - BR27987
	//            // The new parameter added to the constructor determines whether we should resolve the cell format
	//            // with a match in the cell format collection if it exists. But sometimes we want the actual instance 
	//            // of the format data, not a match in the collection. In that case, don't even pass in a collection
	//            // to the proxy, because then it can't resolve a match.
	//            //this.styleFormat = new WorksheetCellFormatProxy( cellFormatData, workbook.CellFormats, workbook );
	//            if ( resolveFormat )
	//                this.styleFormat = new WorksheetCellFormatProxy( cellFormatData, workbook.CellFormats, workbook );
	//            else
	//                this.styleFormat = new WorksheetCellFormatProxy( cellFormatData, null, workbook );

	//            this.styleFormat.Style = true;
	//        }

	//        #endregion Constructor

	//        #region Methods

	//        #region OnRemovedFromCollection

	//#if DEBUG
	//        /// <summary>
	//        /// Gets called when the style is removed from its parent collection.
	//        /// </summary> 
	//#endif
	//        internal void OnRemovedFromCollection()
	//        {
	//            this.styleFormat.OnUnrooted();
	//        }

	//        #endregion OnRemovedFromCollection

	//        #endregion Methods

	//        #region Properties

	//        #region Name

	//        /// <summary>
	//        /// Gets the name of the workbook style.
	//        /// </summary>
	//        /// <value>The name of the workbook style.</value>
	//        public abstract string Name { get;}

	//        #endregion Name

	//        #region StyleFormat

	//        /// <summary>
	//        /// Gets the cell format which applies to the workbook style.
	//        /// </summary>
	//        /// <value>The cell format which applies to the workbook style.</value>
	//        public IWorksheetCellFormat StyleFormat
	//        {
	//            get { return this.StyleFormatInternal; }
	//        }

	//        #endregion StyleFormat

	//        #region StyleFormatInternal

	//        internal WorksheetCellFormatProxy StyleFormatInternal
	//        {
	//            get { return this.styleFormat; }
	//        }

	//        #endregion StyleFormatInternal

	//        #endregion Properties
	//    }

	//#if DEBUG
	//    /// <summary>
	//    /// Represents a built in style saved with the workbook.
	//    /// </summary>  
	//#endif
	//    [DebuggerDisplay( "Built in style: {Name,nq}" )]
	//    internal class WorkbookBuiltInStyle : WorkbookStyle
	//    {
	//        #region Member Variables

	//        private BuiltInStyleType type;
	//        private byte outlineLevel;

	//        #endregion Member Variables

	//        #region Constructor

	//        internal WorkbookBuiltInStyle( Workbook workbook, IWorksheetCellFormat styleFormat, BuiltInStyleType type, byte outlineLevel )
	//            // MD 11/12/07 - BR27987
	//            // Added a parameter, call the other contstructor taking the new parameter
	//            //: base( workbook, styleFormat )
	//            : this( workbook, styleFormat, type, outlineLevel, true ) { }

	//        // MD 11/12/07 - BR27987
	//        // Added a parameter to the base constructor, added a new constructor overload that takes the new parameter
	//        internal WorkbookBuiltInStyle( Workbook workbook, IWorksheetCellFormat styleFormat, BuiltInStyleType type, byte outlineLevel, bool resolveFormat )
	//            : base( workbook, styleFormat, resolveFormat )
	//        {
	//            this.type = type;
	//            this.outlineLevel = outlineLevel;
	//        }

	//        #endregion Constructor

	//        #region Properties

	//        #region Name

	//        public override string Name
	//        {
	//            get
	//            {
	//                if ( this.type == BuiltInStyleType.RowLevelX )
	//                    return "RowLevel " + this.outlineLevel;

	//                if ( this.type == BuiltInStyleType.ColLevelX )
	//                    return "ColLevel " + this.outlineLevel;

	//                return this.type.ToString();
	//            }
	//        }

	//        #endregion Name

	//        #region OutlineLevel

	//        public byte OutlineLevel
	//        {
	//            get { return this.outlineLevel; }
	//        }

	//        #endregion OutlineLevel

	//        #region Type

	//        internal BuiltInStyleType Type
	//        {
	//            get { return this.type; }
	//        }

	//        #endregion Type

	//        #endregion Properties
	//    }

	//#if DEBUG
	//    /// <summary>
	//    /// Represents a user defined style which can be added with the <see cref="Workbook.Styles"/> collection.
	//    /// </summary> 
	//#endif
	//    [DebuggerDisplay( "User defined style: {Name,nq}" )]
	//    internal class WorkbookUserDefinedStyle : WorkbookStyle
	//    {
	//        #region Member Variables

	//        // MD 12/27/11 - TFS98569
	//        // If this is made read/write again, update the WorkbookStyleCollection.stylesByName when the name changes.
	//        //private string name;
	//        private readonly string name;

	//        #endregion Member Variables

	//        #region Constructor

	//        internal WorkbookUserDefinedStyle( Workbook workbook, IWorksheetCellFormat styleFormat, string name )
	//            // MD 11/12/07 - BR27987
	//            // Added a parameter to the base constructor
	//            //: base( workbook, styleFormat )
	//            : base( workbook, styleFormat, true )
	//        {
	//            this.name = name;
	//        }

	//        #endregion Constructor

	//        #region Properties

	//        #region Name

	//        public override string Name
	//        {
	//            get { return this.name; }
	//        }

	//        #endregion Name

	//        #endregion Properties
	//    }

	#endregion // Old Code

	/// <summary>
	/// Represents a complex format which can be applied to a cell's format.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// When a style is applied to a cell by setting its CellFormat.Style property, the cell's format will pick up subsequent
	/// changes to the format properties set by the style when it was applied. This will continue to happen until those format
	/// properties are set to other values on the cell format.
	/// </p>
	/// </remarks>
	/// <seealso cref="IWorksheetCellFormat.Style"/>
	/// <seealso cref="Excel.Workbook.Styles"/>



	public

		abstract class WorkbookStyle
	{
		#region Constants

		internal const int MaxNameLength = 255;

		#endregion // Constants

		#region Member Variables

		private int _referenceCount;
		private WorksheetCellFormatData _styleFormat;
		private Workbook _workbook;

		#endregion Member Variables

		#region Constructor

		internal WorkbookStyle(Workbook workbook, IWorksheetCellFormat styleFormat)
		{
			_workbook = workbook;

			WorksheetCellFormatData cellFormatData = styleFormat as WorksheetCellFormatData;

			if (cellFormatData == null)
			{
				WorksheetCellFormatProxy proxy = styleFormat as WorksheetCellFormatProxy;

				if (proxy != null)
				{
					cellFormatData = proxy.Element;
				}
				else
				{
					cellFormatData = workbook.CellFormats.DefaultElement;
					cellFormatData.SetFormatting(styleFormat);
				}
			}

			if (cellFormatData.ReferenceCount != 0 || _workbook != cellFormatData.Workbook)
				cellFormatData = cellFormatData.CloneInternal(workbook);

			_styleFormat = cellFormatData;
			_styleFormat.Type = WorksheetCellFormatType.StyleFormat;
			_styleFormat.ValueChangeCallback = new CellFormatValueChangeCallback(this.OnCellFormatValueChanged);
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region Reset

		/// <summary>
		/// Resets the style to its original state.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is a user defined style, the format of the style will be reset so that it provides no formatting properties. 
		/// If this is a built in style, this format will revert back to its preset state if it has previously been changed.
		/// </p>
		/// </remarks>
		/// <see cref="IsBuiltIn"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public abstract void Reset();

		#endregion // Reset

		#endregion // Public Methods

		#region Internal Methods

		#region OnAddedToCollection

		internal void OnAddedToCollection()
		{
			_styleFormat.IncrementReferenceCount();
		}

		#endregion OnAddedToCollection

		#region OnCellFormatValueChanged

		internal virtual void OnCellFormatValueChanged(WorksheetCellFormatOptions? associatedStyleOption) { }

		#endregion // OnCellFormatValueChanged

		#region OnChildFormatAttached

		internal void OnChildFormatAttached()
		{
			_referenceCount++;
		}

		#endregion // OnChildFormatAttached

		#region OnChildFormatDetached

		internal void OnChildFormatDetached()
		{
			_referenceCount--;
		}

		#endregion // OnChildFormatDetached

		#region OnRemovedFromCollection

		internal void OnRemovedFromCollection()
		{
			_styleFormat.DecrementReferenceCount();
		}

		#endregion OnRemovedFromCollection

		#endregion // Internal Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region IsBuiltIn

		/// <summary>
		/// Gets the value indicating whether the style is a built in style in Microsoft Excel.
		/// </summary>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		public abstract bool IsBuiltIn { get; }

		#endregion // IsBuiltIn

		#region Name

		/// <summary>
		/// Gets or sets the name of the workbook style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The style names must be case-insensitively unique and the names for built in styles cannot be changed.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned and the style is a built.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned matches the name of another style in the collection.
		/// </exception>
		/// <value>The name of the workbook style.</value>
		/// <seealso cref="IsBuiltIn"/>
		public abstract string Name { get; set; }

		#endregion Name

		#region StyleFormat

		/// <summary>
		/// Gets the cell format which applies to the workbook style.
		/// </summary>
		/// <value>The cell format which applies to the workbook style.</value>
		public IWorksheetCellFormat StyleFormat
		{
			get { return this.StyleFormatInternal; }
		}

		#endregion StyleFormat

		#endregion // Public Properties

		#region Internal Properties

		#region Category

		internal abstract StyleCategory Category { get; }

		#endregion // Category

		#region IsAutomatic






		internal virtual bool IsAutomatic
		{
			get { return false; }
		}

		#endregion // IsAutomatic

		#region IsNormalStyle

		internal abstract bool IsNormalStyle { get; }

		#endregion // IsNormalStyle

		#region ShouldSaveIn2003

		internal virtual bool ShouldSaveIn2003
		{
			get { return _referenceCount > 0; }
		}

		#endregion // ShouldSaveIn2003

		#region ShouldSaveIn2007

		internal virtual bool ShouldSaveIn2007
		{
			get { return _referenceCount > 0; }
		}

		#endregion // ShouldSaveIn2007

		#region StyleFormatInternal

		// MD 1/3/12 - 12.1 - Table Support
		// Since styles no longer store their cell formats in the shared cell formats collection, they don't need a proxy.
		//internal WorksheetCellFormatProxy StyleFormatInternal
		internal WorksheetCellFormatData StyleFormatInternal
		{
			get { return _styleFormat; }
		}

		#endregion StyleFormatInternal

		#region Workbook

		internal Workbook Workbook
		{
			get { return _workbook; }
		}

		#endregion // Workbook

		#endregion // Internal Properties

		#endregion Properties
	}

	[DebuggerDisplay("Built in style: {Name,nq}")]
	internal class WorkbookBuiltInStyle : WorkbookStyle,
		IComparable<WorkbookBuiltInStyle>
	{
		#region Member Variables

		private bool _isCustomized;
		private WorksheetCellFormatData _originalBuiltInStyle;
		private readonly byte _outlineLevel;
		private readonly BuiltInStyleType _type;

		#endregion Member Variables

		#region Constructor

		internal WorkbookBuiltInStyle(Workbook workbook, IWorksheetCellFormat styleFormat, BuiltInStyleType type, byte outlineLevel)
			: base(workbook, styleFormat)
		{
			_type = type;

			// According to these docs: http://msdn.microsoft.com/en-us/library/dd953733(v=office.12).aspx, the value of iLevel should 
			// be 0xFF when it is not used.
			if (this.UsesOutlineLevel)
				_outlineLevel = outlineLevel;
			else
				_outlineLevel = 0xFF;

			_originalBuiltInStyle = this.StyleFormatInternal.CloneInternal();
		}

		#endregion Constructor

		#region Interfaces

		#region IComparable<WorkbookBuiltInStyle> Members

		int IComparable<WorkbookBuiltInStyle>.CompareTo(WorkbookBuiltInStyle other)
		{
			if (this.UsesOutlineLevel && other.UsesOutlineLevel)
			{
				int result = _outlineLevel - other._outlineLevel;
				if (result != 0)
					return result;
			}

			return _type - other._type;
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region Category

		internal override StyleCategory Category
		{
			get
			{
				switch (this.Type)
				{
					case BuiltInStyleType.Bad:
					case BuiltInStyleType.Good:
					case BuiltInStyleType.Neutral:
					case BuiltInStyleType.Normal:
						return StyleCategory.GoodBadNeutral;

					case BuiltInStyleType.Calculation:
					case BuiltInStyleType.CheckCell:
					case BuiltInStyleType.Emphasis1:
					case BuiltInStyleType.Emphasis2:
					case BuiltInStyleType.Emphasis3:
					case BuiltInStyleType.ExplanatoryText:
					case BuiltInStyleType.FollowedHyperlink:
					case BuiltInStyleType.Hyperlink:
					case BuiltInStyleType.Input:
					case BuiltInStyleType.LinkedCell:
					case BuiltInStyleType.Note:
					case BuiltInStyleType.Output:
					case BuiltInStyleType.WarningText:
						return StyleCategory.DataModel;

					case BuiltInStyleType.Heading1:
					case BuiltInStyleType.Heading2:
					case BuiltInStyleType.Heading3:
					case BuiltInStyleType.Heading4:
					case BuiltInStyleType.Title:
					case BuiltInStyleType.Total:
						return StyleCategory.TitleAndHeading;

					case BuiltInStyleType.Accent1:
					case BuiltInStyleType.Accent1pct20:
					case BuiltInStyleType.Accent1pct40:
					case BuiltInStyleType.Accent1pct60:
					case BuiltInStyleType.Accent2:
					case BuiltInStyleType.Accent2pct20:
					case BuiltInStyleType.Accent2pct40:
					case BuiltInStyleType.Accent2pct60:
					case BuiltInStyleType.Accent3:
					case BuiltInStyleType.Accent3pct20:
					case BuiltInStyleType.Accent3pct40:
					case BuiltInStyleType.Accent3pct60:
					case BuiltInStyleType.Accent4:
					case BuiltInStyleType.Accent4pct20:
					case BuiltInStyleType.Accent4pct40:
					case BuiltInStyleType.Accent4pct60:
					case BuiltInStyleType.Accent5:
					case BuiltInStyleType.Accent5pct20:
					case BuiltInStyleType.Accent5pct40:
					case BuiltInStyleType.Accent5pct60:
					case BuiltInStyleType.Accent6:
					case BuiltInStyleType.Accent6pct20:
					case BuiltInStyleType.Accent6pct40:
					case BuiltInStyleType.Accent6pct60:
					case BuiltInStyleType.ColLevelX:
					case BuiltInStyleType.RowLevelX:
						return StyleCategory.Themed;

					case BuiltInStyleType.Comma:
					case BuiltInStyleType.Currency:
					case BuiltInStyleType.Percent:
					case BuiltInStyleType.Comma0:
					case BuiltInStyleType.Currency0:
						return StyleCategory.NumberFormat;

					default:
						Utilities.DebugFail("Unknown type: " + this.Type);
						return StyleCategory.Custom;
				}
			}
		}

		#endregion // Category

		#region IsAutomatic

		internal override bool IsAutomatic
		{
			get
			{
				switch (this.Type)
				{
					case BuiltInStyleType.Normal:
					case BuiltInStyleType.RowLevelX:
					case BuiltInStyleType.ColLevelX:
					case BuiltInStyleType.Hyperlink:
					case BuiltInStyleType.FollowedHyperlink:
						return true;
				}

				return false;
			}
		}

		#endregion // IsAutomatic

		#region IsBuiltIn

		public override bool IsBuiltIn
		{
			get { return true; }
		}

		#endregion // IsBuiltIn

		#region IsNormalStyle

		internal override bool IsNormalStyle
		{
			get { return _type == BuiltInStyleType.Normal; }
		}

		#endregion // IsNormalStyle

		#region Name

		public override string Name
		{
			get
			{
				switch (this.Type)
				{
					case BuiltInStyleType.Accent1:
					case BuiltInStyleType.Accent2:
					case BuiltInStyleType.Accent3:
					case BuiltInStyleType.Accent4:
					case BuiltInStyleType.Accent5:
					case BuiltInStyleType.Accent6:
					case BuiltInStyleType.Bad:
					case BuiltInStyleType.Calculation:
					case BuiltInStyleType.Comma:
					case BuiltInStyleType.Currency:
					case BuiltInStyleType.Good:
					case BuiltInStyleType.Hyperlink:
					case BuiltInStyleType.Input:
					case BuiltInStyleType.Neutral:
					case BuiltInStyleType.Normal:
					case BuiltInStyleType.Note:
					case BuiltInStyleType.Output:
					case BuiltInStyleType.Percent:
					case BuiltInStyleType.Title:
					case BuiltInStyleType.Total:
						return this.Type.ToString();

					case BuiltInStyleType.Accent1pct20:
					case BuiltInStyleType.Accent1pct40:
					case BuiltInStyleType.Accent1pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent1;
							return string.Format("{0}% - Accent1", diff * 20);
						}

					case BuiltInStyleType.Accent2pct20:
					case BuiltInStyleType.Accent2pct40:
					case BuiltInStyleType.Accent2pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent2;
							return string.Format("{0}% - Accent2", diff * 20);
						}

					case BuiltInStyleType.Accent3pct20:
					case BuiltInStyleType.Accent3pct40:
					case BuiltInStyleType.Accent3pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent3;
							return string.Format("{0}% - Accent3", diff * 20);
						}

					case BuiltInStyleType.Accent4pct20:
					case BuiltInStyleType.Accent4pct40:
					case BuiltInStyleType.Accent4pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent4;
							return string.Format("{0}% - Accent4", diff * 20);
						}

					case BuiltInStyleType.Accent5pct20:
					case BuiltInStyleType.Accent5pct40:
					case BuiltInStyleType.Accent5pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent5;
							return string.Format("{0}% - Accent5", diff * 20);
						}

					case BuiltInStyleType.Accent6pct20:
					case BuiltInStyleType.Accent6pct40:
					case BuiltInStyleType.Accent6pct60:
						{
							int diff = this.Type - BuiltInStyleType.Accent6;
							return string.Format("{0}% - Accent6", diff * 20);
						}

					case BuiltInStyleType.CheckCell:
						return "Check Cell";

					case BuiltInStyleType.ColLevelX:
						return "ColLevel_" + this.OutlineLevel;

					case BuiltInStyleType.Comma0:
						return "Comma [0]";

					case BuiltInStyleType.Currency0:
						return "Currency [0]";

					case BuiltInStyleType.Emphasis1:
						return "Emphasis 1";

					case BuiltInStyleType.Emphasis2:
						return "Emphasis 2";

					case BuiltInStyleType.Emphasis3:
						return "Emphasis 3";

					case BuiltInStyleType.ExplanatoryText:
						return "Explanatory Text";

					case BuiltInStyleType.FollowedHyperlink:
						return "Followed Hyperlink";

					case BuiltInStyleType.Heading1:
						return "Heading 1";

					case BuiltInStyleType.Heading2:
						return "Heading 2";

					case BuiltInStyleType.Heading3:
						return "Heading 3";

					case BuiltInStyleType.Heading4:
						return "Heading 4";

					case BuiltInStyleType.LinkedCell:
						return "Linked Cell";

					case BuiltInStyleType.RowLevelX:
						return "RowLevel_" + this.OutlineLevel;

					case BuiltInStyleType.WarningText:
						return "Warning Text";

					default:
						Utilities.DebugFail("Unknown type: " + this.Type);
						return this.Type.ToString();
				}
			}
			set
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_BuiltInStyleNameCannotBeChanged"));
			}
		}

		#endregion Name

		#region OnCellFormatValueChanged

		internal override void OnCellFormatValueChanged(WorksheetCellFormatOptions? associatedStyleOption)
		{
			base.OnCellFormatValueChanged(associatedStyleOption);

			_isCustomized = (this.StyleFormatInternal.Equals(_originalBuiltInStyle) == false);

			Workbook workbook = this.Workbook;
			if (workbook != null &&
				associatedStyleOption == WorksheetCellFormatOptions.ApplyFontFormatting &&
				this.IsNormalStyle)
			{
				workbook.OnDefaultFontChanged();
			}
		}

		#endregion // OnCellFormatValueChanged

		#region Reset

		public override void Reset()
		{
			this.StyleFormatInternal.SetFormatting(_originalBuiltInStyle);
		}

		#endregion // Reset

		#region ShouldSaveIn2003

		internal override bool ShouldSaveIn2003
		{
			get
			{
				if (base.ShouldSaveIn2003 || this.IsCustomized)
					return true;

				// Always write out these styles in 2003.
				switch (this.Type)
				{
					case BuiltInStyleType.Comma:
					case BuiltInStyleType.Comma0:
					case BuiltInStyleType.Currency:
					case BuiltInStyleType.Currency0:
					case BuiltInStyleType.Normal:
					case BuiltInStyleType.Percent:
						return true;
				}

				// Always write out all built in styles from the newer formats.
				if (this.IsBuiltInStyleIn2003 == false)
					return true;

				return false;
			}
		}

		#endregion // ShouldSaveIn2003

		#region ShouldSaveIn2007

		internal override bool ShouldSaveIn2007
		{
			get
			{
				return
					base.ShouldSaveIn2007 ||
					this.IsNormalStyle ||
					this.IsCustomized;
			}
		}

		#endregion // ShouldSaveIn2007

		#endregion // Base Class Overrides

		#region Methods

		#region Clone

		internal WorkbookBuiltInStyle Clone(Workbook workbook)
		{
			return new WorkbookBuiltInStyle(
				workbook,
				this.StyleFormatInternal,
				this.Type,
				this.OutlineLevel);
		}

		#endregion // Clone

		#endregion // Methods

		#region Properties

		#region Internal Properties

		#region IsBuiltInStyleIn2003

		internal bool IsBuiltInStyleIn2003
		{
			get
			{
				switch (this.Type)
				{
					case BuiltInStyleType.Normal:
					case BuiltInStyleType.RowLevelX:
					case BuiltInStyleType.ColLevelX:
					case BuiltInStyleType.Comma:
					case BuiltInStyleType.Currency:
					case BuiltInStyleType.Percent:
					case BuiltInStyleType.Comma0:
					case BuiltInStyleType.Currency0:
					case BuiltInStyleType.Hyperlink:
					case BuiltInStyleType.FollowedHyperlink:
						return true;
				}

				return false;
			}
		}

		#endregion // IsBuiltInStyleIn2003

		#region IsCustomized

		internal bool IsCustomized
		{
			get { return _isCustomized; }
			set { _isCustomized = value; }
		}

		#endregion // IsCustomized

		#region OutlineLevel

		public byte OutlineLevel
		{
			get { return _outlineLevel; }
		}

		#endregion OutlineLevel

		#region Type

		internal BuiltInStyleType Type
		{
			get { return _type; }
		}

		#endregion Type

		#region UsesOutlineLevel

		internal bool UsesOutlineLevel
		{
			get { return this.Type == BuiltInStyleType.ColLevelX || this.Type == BuiltInStyleType.RowLevelX; }
		}

		#endregion // UsesOutlineLevel

		#endregion // Internal Properties

		#endregion Properties
	}

	[DebuggerDisplay("User defined style: {Name,nq}")]
	internal class WorkbookUserDefinedStyle : WorkbookStyle
	{
		#region Member Variables

		private string _name;

		#endregion Member Variables

		#region Constructor

		internal WorkbookUserDefinedStyle(Workbook workbook, IWorksheetCellFormat styleFormat, string name)
			: base(workbook, styleFormat)
		{
			_name = name;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Category

		internal override StyleCategory Category
		{
			get { return StyleCategory.Custom; }
		}

		#endregion // Category

		#region IsBuiltIn

		public override bool IsBuiltIn
		{
			get { return false; }
		}

		#endregion // IsBuiltIn

		#region IsNormalStyle

		internal override bool IsNormalStyle
		{
			get { return false; }
		}

		#endregion // IsNormalStyle

		#region Name

		public override string Name
		{
			get { return _name; }
			set
			{
				if (_name == value)
					return;

				// MD 4/9/12 - TFS101506
				//if (String.Equals(_name, value, StringComparison.CurrentCultureIgnoreCase) == false)
				if (String.Compare(_name, value, this.Workbook.CultureResolved, CompareOptions.IgnoreCase) != 0)
					this.Workbook.Styles.ValidateNewStyleName(value, "value");

				string oldName = _name;
				_name = value;

				this.Workbook.Styles.OnStyleRenamed(this, oldName);
			}
		}

		#endregion Name

		#region Reset

		public override void Reset()
		{
			this.StyleFormatInternal.FormatOptions = WorksheetCellFormatOptions.None;
		}

		#endregion // Reset

		#region ShouldSaveIn2003

		internal override bool ShouldSaveIn2003
		{
			get { return true; }
		}

		#endregion // ShouldSaveIn2003

		#region ShouldSaveIn2007

		internal override bool ShouldSaveIn2007
		{
			get { return true; }
		}

		#endregion // ShouldSaveIn2007

		#endregion // Base Class Overrides
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