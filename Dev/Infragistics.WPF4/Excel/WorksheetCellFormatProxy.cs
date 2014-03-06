using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Serialization.BIFF8;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	internal class WorksheetCellFormatProxy : GenericCacheElementProxy<WorksheetCellFormatData>,
		IWorksheetCellFormat
	{
		#region Member Variables

		private WorkbookFontOwnerAccessor fontAccessor;

		// MD 5/12/10 - TFS26732
		private IWorksheetCellFormatProxyOwner owner;

		#endregion Member Variables

		#region Constructor

		// MD 4/18/11 - TFS62026
		// Rewrote the constructors: the workbook is no longer needed as a parameter.
		#region Old Code

		//public WorksheetCellFormatProxy( WorksheetCellFormatData target, GenericCachedCollection<WorksheetCellFormatData> parentCollection, Workbook workbook )
		//    // MD 7/26/10 - TFS34398
		//    // The derived proxies must now manage the workbook themselves.
		//    //: base( target, parentCollection, workbook ) { }
		//    : base(target, parentCollection) 
		//{
		//    this.owner = workbook;
		//}
		//
		//// MD 10/27/10
		//// Found while fixing TFS56976
		//// We need a constructor that can take initial info and an owner.
		//public WorksheetCellFormatProxy(WorksheetCellFormatData target, GenericCachedCollection<WorksheetCellFormatData> parentCollection, Workbook workbook, IWorksheetCellFormatProxyOwner owner)
		//    : this(target, parentCollection, workbook)
		//{
		//    this.owner = owner;
		//}
		//
		//// MD 5/12/10 - TFS26732
		//// Added an owner parameter to the constructor.
		////public WorksheetCellFormatProxy( GenericCachedCollection<WorksheetCellFormatData> parentCollection, Workbook workbook )
		////	: this( parentCollection.DefaultElement, parentCollection, workbook ) { }
		//// MD 10/27/10
		//// Found while fixing TFS56976
		//// Call off to the new constructor which takes initial data and an owner.
		////public WorksheetCellFormatProxy(GenericCachedCollection<WorksheetCellFormatData> parentCollection, Workbook workbook, IWorksheetCellFormatProxyOwner owner)
		////    : this( parentCollection.DefaultElement, parentCollection, workbook ) 
		////{
		////    this.owner = owner;
		////}
		//public WorksheetCellFormatProxy(GenericCachedCollection<WorksheetCellFormatData> parentCollection, Workbook workbook, IWorksheetCellFormatProxyOwner owner)
		//    // MD 2/15/11 - TFS66333
		//    // Use the EmptyElement for the initial data element. The DefaultElement will be populated with data if 
		//    // the workbook was loaded from a file or stream.
		//    //: this(parentCollection.DefaultElement, parentCollection, workbook, owner) { }
		//    : this(parentCollection.EmptyElement, parentCollection, workbook, owner) { }
		// 

		#endregion  // Old Code
		// MD 2/2/12 - TFS100573
		//public WorksheetCellFormatProxy(WorksheetCellFormatData target, GenericCachedCollection<WorksheetCellFormatData> parentCollection, IWorksheetCellFormatProxyOwner owner)
		public WorksheetCellFormatProxy(WorksheetCellFormatData target, GenericCachedCollectionEx<WorksheetCellFormatData> parentCollection, IWorksheetCellFormatProxyOwner owner)
			: base(target, parentCollection)
		{
			this.owner = owner;
		}

		// MD 2/27/12 - 12.1 - Table Support
		//// MD 2/2/12 - TFS100573
		////public WorksheetCellFormatProxy(GenericCachedCollection<WorksheetCellFormatData> parentCollection, IWorksheetCellFormatProxyOwner owner)
		//public WorksheetCellFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> parentCollection, IWorksheetCellFormatProxyOwner owner)
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // The parent collection could be null.
		//    //: this(parentCollection.EmptyElement, parentCollection, owner) { }
		//    // MD 1/8/12 - 12.1 - Cell Format Updates
		//    // Use the CreateNewWorksheetCellFormatInternal method, which will set the IsStyle value to False before returning the cell format.
		//    //: this(parentCollection == null ? new WorksheetCellFormatData(null, null) : parentCollection.EmptyElement, parentCollection, owner) { }
		//    : this(parentCollection == null ? new WorksheetCellFormatData(null) : parentCollection.Workbook.CreateNewWorksheetCellFormatInternal(), parentCollection, owner) { }
		public WorksheetCellFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> parentCollection, IWorksheetCellFormatProxyOwner owner)
			: this(parentCollection == null ? new WorksheetCellFormatData(null, WorksheetCellFormatType.CellFormat) : parentCollection.Workbook.CellFormats.DefaultElement, parentCollection, owner) { }

		#endregion Constructor

		#region Interfaces

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region IWorksheetCellFormat Members

		Color IWorksheetCellFormat.BottomBorderColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.BottomBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.BottomBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.DiagonalBorderColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.DiagonalBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.DiagonalBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		FillPatternStyle IWorksheetCellFormat.FillPattern
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetFillPattern(formatData.Fill);
			}
			set
			{
				WorksheetCellFormatData formatData = this.Element;
				this.Fill = formatData.UpdatedFillPattern(formatData.FillResolved, value);
			}
		}

		Color IWorksheetCellFormat.FillPatternBackgroundColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.GetFileFormatFillPatternColor(formatData.Fill, true, false));
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				WorksheetCellFormatData formatData = this.Element;
				this.Fill = formatData.UpdatedFillPatternColor(formatData.FillResolved, value, true);
			}
		}

		Color IWorksheetCellFormat.FillPatternForegroundColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.GetFileFormatFillPatternColor(formatData.Fill, false, false));
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				WorksheetCellFormatData formatData = this.Element;
				this.Fill = formatData.UpdatedFillPatternColor(formatData.FillResolved, value, false);
			}
		}

		Color IWorksheetCellFormat.LeftBorderColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.LeftBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.LeftBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.RightBorderColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.RightBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.RightBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		Color IWorksheetCellFormat.TopBorderColor
		{
			get
			{
				WorksheetCellFormatData formatData = this.Element;
				return formatData.GetColor(formatData.TopBorderColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.TopBorderColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion // IWorksheetCellFormat Members

		#endregion // Interfaces

		#region Base Class Overrides

		// MD 12/22/11 - 12.1 - Table Support
		// This is no longer needed.
		//// MD 7/26/10 - TFS34398
		//// The derived proxies must now manage the workbook themselves.
		//#region Workbook

		//public override Workbook Workbook
		//{
		//    get { return this.owner.Workbook; }
		//}

		//#endregion // Workbook 

		#endregion // Base Class Overrides

		#region Methods

		// MD 2/29/12 - 12.1 - Table Support
		#region GetDifferences

		private List<CellFormatValue> GetDifferences(IWorksheetCellFormat source)
		{
			List<CellFormatValue> changedValues = new List<CellFormatValue>();

			WorksheetCellFormatData formatData = this.Element;

			if (formatData.Alignment != source.Alignment)
				changedValues.Add(CellFormatValue.Alignment);

			if (formatData.BottomBorderColorInfo != source.BottomBorderColorInfo)
				changedValues.Add(CellFormatValue.BottomBorderColorInfo);

			if (formatData.BottomBorderStyle != source.BottomBorderStyle)
				changedValues.Add(CellFormatValue.BottomBorderStyle);

			if (formatData.DiagonalBorderColorInfo != source.DiagonalBorderColorInfo)
				changedValues.Add(CellFormatValue.DiagonalBorderColorInfo);

			if (formatData.DiagonalBorders != source.DiagonalBorders)
				changedValues.Add(CellFormatValue.DiagonalBorders);

			if (formatData.DiagonalBorderStyle != source.DiagonalBorderStyle)
				changedValues.Add(CellFormatValue.DiagonalBorderStyle);

			if (Object.Equals(formatData.Fill, source.Fill) == false)
				changedValues.Add(CellFormatValue.Fill);

			if (formatData.FormatString != source.FormatString)
				changedValues.Add(CellFormatValue.FormatString);

			if (formatData.Indent != source.Indent)
				changedValues.Add(CellFormatValue.Indent);

			if (formatData.LeftBorderColorInfo != source.LeftBorderColorInfo)
				changedValues.Add(CellFormatValue.LeftBorderColorInfo);

			if (formatData.LeftBorderStyle != source.LeftBorderStyle)
				changedValues.Add(CellFormatValue.LeftBorderStyle);

			if (formatData.Locked != source.Locked)
				changedValues.Add(CellFormatValue.Locked);

			if (formatData.RightBorderColorInfo != source.RightBorderColorInfo)
				changedValues.Add(CellFormatValue.RightBorderColorInfo);

			if (formatData.RightBorderStyle != source.RightBorderStyle)
				changedValues.Add(CellFormatValue.RightBorderStyle);

			if (formatData.Rotation != source.Rotation)
				changedValues.Add(CellFormatValue.Rotation);

			if (formatData.ShrinkToFit != source.ShrinkToFit)
				changedValues.Add(CellFormatValue.ShrinkToFit);

			if (formatData.Style != source.Style)
				changedValues.Add(CellFormatValue.Style);

			if (formatData.FormatOptions != source.FormatOptions)
				changedValues.Add(CellFormatValue.FormatOptions);

			if (formatData.TopBorderColorInfo != source.TopBorderColorInfo)
				changedValues.Add(CellFormatValue.TopBorderColorInfo);

			if (formatData.TopBorderStyle != source.TopBorderStyle)
				changedValues.Add(CellFormatValue.TopBorderStyle);

			if (formatData.VerticalAlignment != source.VerticalAlignment)
				changedValues.Add(CellFormatValue.VerticalAlignment);

			if (formatData.WrapText != source.WrapText)
				changedValues.Add(CellFormatValue.WrapText);

			if (formatData.Font.Bold != source.Font.Bold)
				changedValues.Add(CellFormatValue.FontBold);

			if (formatData.Font.ColorInfo != source.Font.ColorInfo)
				changedValues.Add(CellFormatValue.FontColorInfo);

			if (formatData.Font.Height != source.Font.Height)
				changedValues.Add(CellFormatValue.FontHeight);

			if (formatData.Font.Italic != source.Font.Italic)
				changedValues.Add(CellFormatValue.FontItalic);

			if (formatData.Font.Name != source.Font.Name)
				changedValues.Add(CellFormatValue.FontName);

			if (formatData.Font.Strikeout != source.Font.Strikeout)
				changedValues.Add(CellFormatValue.FontStrikeout);

			if (formatData.Font.SuperscriptSubscriptStyle != source.Font.SuperscriptSubscriptStyle)
				changedValues.Add(CellFormatValue.FontSuperscriptSubscriptStyle);

			if (formatData.Font.UnderlineStyle != source.Font.UnderlineStyle)
				changedValues.Add(CellFormatValue.FontUnderlineStyle);

			return changedValues;
		}

		#endregion // GetDifferences

		// MD 5/12/10 - TFS26732
		#region GetValue

		public object GetValue(CellFormatValue valueToGet)
		{
			// MD 4/18/11 - TFS62026
			// Moved this logic to the WorksheetCellFormatData.GetValue method.
			#region Moved

			//switch (valueToGet)
			//{
			//    case CellFormatValue.Alignment:
			//        return this.Alignment;

			//    case CellFormatValue.BottomBorderColor:
			//        return this.BottomBorderColor;

			//    case CellFormatValue.BottomBorderStyle:
			//        return this.BottomBorderStyle;

			//    case CellFormatValue.FillPattern:
			//        return this.FillPattern;

			//    case CellFormatValue.FillPatternBackgroundColor:
			//        return this.FillPatternBackgroundColor;

			//    case CellFormatValue.FillPatternForegroundColor:
			//        return this.FillPatternForegroundColor;

			//    // MD 10/13/10 - TFS43003
			//    case CellFormatValue.FontBold:
			//        return this.Font.Bold;

			//    case CellFormatValue.FontColor:
			//        return this.Font.Color;

			//    case CellFormatValue.FontHeight:
			//        return this.Font.Height;

			//    case CellFormatValue.FontItalic:
			//        return this.Font.Italic;

			//    case CellFormatValue.FontName:
			//        return this.Font.Name;

			//    case CellFormatValue.FontStrikeout:
			//        return this.Font.Strikeout;

			//    case CellFormatValue.FontSuperscriptSubscriptStyle:
			//        return this.Font.SuperscriptSubscriptStyle;

			//    case CellFormatValue.FontUnderlineStyle:
			//        return this.Font.UnderlineStyle;
			//    // ***************** End of TFS43003 Fix ********************

			//    case CellFormatValue.FormatString:
			//        return this.FormatString;

			//    case CellFormatValue.Indent:
			//        return this.Indent;

			//    case CellFormatValue.LeftBorderColor:
			//        return this.LeftBorderColor;

			//    case CellFormatValue.LeftBorderStyle:
			//        return this.LeftBorderStyle;

			//    case CellFormatValue.Locked:
			//        return this.Locked;

			//    case CellFormatValue.RightBorderColor:
			//        return this.RightBorderColor;

			//    case CellFormatValue.RightBorderStyle:
			//        return this.RightBorderStyle;

			//    case CellFormatValue.Rotation:
			//        return this.Rotation;

			//    case CellFormatValue.ShrinkToFit:
			//        return this.ShrinkToFit;

			//    case CellFormatValue.Style:
			//        return this.Style;

			//    case CellFormatValue.TopBorderColor:
			//        return this.TopBorderColor;

			//    case CellFormatValue.TopBorderStyle:
			//        return this.TopBorderStyle;

			//    case CellFormatValue.VerticalAlignment:
			//        return this.VerticalAlignment;

			//    case CellFormatValue.WrapText:
			//        return this.WrapText;

			//    default:
			//        Utilities.DebugFail("Unknown format value: " + valueToGet);
			//        return null;
			//} 

			#endregion // Moved
			return this.Element.GetValue(valueToGet);
		} 

		#endregion // GetValue

		// MD 3/27/12 - 12.1 - Table Support
		#region InitializeDefaultValuesFrom

		// MD 5/4/12 - TFS107276
		// Added optional CellFormatValue parameters so the caller can specify only the properties they want initialized.
		//public void InitializeDefaultValuesFrom(WorksheetCellFormatProxy other)
		public void InitializeDefaultValuesFrom(WorksheetCellFormatProxy other, params CellFormatValue[] values)
		{
			GenericCachedCollection<WorksheetCellFormatData> collection = this.BeforeSet();

			// MD 5/4/12 - TFS107276
			// Any any values were specified, ony initialize them. Otherwise, initialize all properties.
			//IList<CellFormatValue> allValues = WorksheetCellFormatData.AllCellFormatValues;
			IList<CellFormatValue> allValues = values.Length == 0 ? WorksheetCellFormatData.AllCellFormatValues : values;

			for (int i = 0; i < allValues.Count; i++)
			{
				CellFormatValue currentProperty = allValues[i];
				if (this.IsValueDefault(currentProperty) == false)
					continue;

				object copyValue = other.GetValue(currentProperty);
				if (WorksheetCellFormatData.IsValueDefault(currentProperty, copyValue) == false)
					this.SetValue(currentProperty, copyValue, false, CellFormatValueChangedOptions.DefaultBehavior);
			}

			this.AfterSet(collection);
		}

		#endregion // InitializeDefaultValuesFrom

		// MD 5/12/10 - TFS26732
		#region IsValueDefault

		public bool IsValueDefault(CellFormatValue value)
		{
			return WorksheetCellFormatData.IsValueDefault(value, this.GetValue(value));
		}

		#endregion // IsValueDefault

		// MD 5/12/10 - TFS26732
		#region OnPropertyChanged

		// MD 10/13/10 - TFS43003
		// Made internal so this can be used from other classes.
		//private void OnPropertyChanged(CellFormatValue value)
		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//internal void OnPropertyChanged(CellFormatValue value)
		internal void OnPropertyChanged(CellFormatValue value, CellFormatValueChangedOptions options)
		{
			// MD 4/18/11 - TFS62026
			// Call off to the new overload
			this.OnPropertyChanged(new CellFormatValue[] { value }, options);
		}

		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		internal void OnPropertyChanged(IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			if (this.owner != null)
			{
				// MD 10/21/10 - TFS34398
				// We need to pass along options to the handlers of the cell format value change.
				//this.owner.OnCellFormatValueChanged(value);
				// MD 4/12/11 - TFS67084
				// We need to pass along the sender now because some object own multiple cell formats.
				//this.owner.OnCellFormatValueChanged(value, options);
				// MD 4/18/11 - TFS62026
				// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
				//this.owner.OnCellFormatValueChanged(this, value, options);
				this.owner.OnCellFormatValueChanged(this, values, options);
			}
		} 

		#endregion // OnPropertyChanged

		// MD 2/29/12 - 12.1 - Table Support
		#region OnPropertyChanging

		internal void OnPropertyChanging(CellFormatValue value)
		{
			this.OnPropertyChanging(new CellFormatValue[] { value });
		}

		internal void OnPropertyChanging(IList<CellFormatValue> values)
		{
			if (this.owner != null)
				this.owner.OnCellFormatValueChanging(this, values);
		}

		#endregion // OnPropertyChanged

		// MD 3/5/12 - 12.1 - Table Support
		#region Reset

		internal void Reset()
		{
			this.Style = null;
			this.FormatOptions = WorksheetCellFormatOptions.None;
		}

		#endregion // Reset

		// MD 5/12/10 - TFS26732
		#region ResetValue

		public void ResetValue(CellFormatValue value)
		{
			// MD 10/13/10 - TFS43003
			// It seems unecessary to duplicate the definition of default values when we can just get it through a method
			#region Removed

			//switch (value)
			//{
			//    case CellFormatValue.Alignment:
			//        this.Alignment = HorizontalCellAlignment.Default;
			//        break;
			//
			//    case CellFormatValue.BottomBorderColor:
			//        this.BottomBorderColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.BottomBorderStyle:
			//        this.BottomBorderStyle = CellBorderLineStyle.Default;
			//        break;
			//
			//    case CellFormatValue.FillPattern:
			//        this.FillPattern = FillPatternStyle.Default;
			//        break;
			//
			//    case CellFormatValue.FillPatternBackgroundColor:
			//        this.FillPatternBackgroundColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.FillPatternForegroundColor:
			//        this.FillPatternForegroundColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.FormatString:
			//        this.FormatString = null;
			//        break;
			//
			//    case CellFormatValue.Indent:
			//        this.Indent = -1;
			//        break;
			//
			//    case CellFormatValue.LeftBorderColor:
			//        this.LeftBorderColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.LeftBorderStyle:
			//        this.LeftBorderStyle = CellBorderLineStyle.Default;
			//        break;
			//
			//    case CellFormatValue.Locked:
			//        this.Locked = ExcelDefaultableBoolean.Default;
			//        break;
			//
			//    case CellFormatValue.RightBorderColor:
			//        this.RightBorderColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.RightBorderStyle:
			//        this.RightBorderStyle = CellBorderLineStyle.Default;
			//        break;
			//
			//    case CellFormatValue.Rotation:
			//        this.Rotation = -1;
			//        break;
			//
			//    case CellFormatValue.ShrinkToFit:
			//        this.ShrinkToFit = ExcelDefaultableBoolean.Default;
			//        break;
			//
			//    case CellFormatValue.Style:
			//        this.Style = false;
			//        break;
			//
			//    case CellFormatValue.TopBorderColor:
			//        this.TopBorderColor = Utilities.ColorEmpty;
			//        break;
			//
			//    case CellFormatValue.TopBorderStyle:
			//        this.TopBorderStyle = CellBorderLineStyle.Default;
			//        break;
			//
			//    case CellFormatValue.VerticalAlignment:
			//        this.VerticalAlignment = VerticalCellAlignment.Default;
			//        break;
			//
			//    case CellFormatValue.WrapText:
			//        this.WrapText = ExcelDefaultableBoolean.Default;
			//        break;
			//} 

			#endregion // Removed
			// MD 4/18/11 - TFS62026
			// Call off to the new overload.
			//this.SetValue(value, WorksheetCellFormatProxy.GetDefaultValue(value));
			this.ResetValue(value, CellFormatValueChangedOptions.DefaultBehavior);
		}

		// MD 4/18/11 - TFS62026
		// Added a way to reset a value and pass along CellFormatValueChangedOptions.
		public void ResetValue(CellFormatValue value, CellFormatValueChangedOptions options)
		{
			// MD 12/22/11 - 12.1 - Table Support
			// The DiagonalBorders enumeration now has a default value.
			//// MD 10/26/11 - TFS91546
			//// GetDefaultValue for DiagonalBorders returns null, because it has no default. So when we should reset it, set it to None.
			////this.SetValue(value, WorksheetCellFormatProxy.GetDefaultValue(value), true, options);
			//if (value == CellFormatValue.DiagonalBorders)
			//{
			//    this.SetValue(value, DiagonalBorders.None, true, options);
			//    return;
			//}

			object defaultValue = WorksheetCellFormatData.GetDefaultValue(value);

			// MD 12/31/11 - 12.1 - Cell Format Updates
			// This is no longer true. The Style property has a null default value.
			//if (defaultValue == null)
			//{
			//    Utilities.DebugFail("The default value is null here.");
			//    return;
			//}

			this.SetValue(value, defaultValue, true, options);
		}

		#endregion // ResetValue

		// MD 12/21/11 - 12.1 - Table Support
		#region SetFormatProperty

		private bool SetFormatProperty<TValue>(TValue value, CellFormatValue valueType, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options,
			Utilities.PropertyGetter<WorksheetCellFormatData, TValue> propertyGetter,
			Utilities.PropertySetter<WorksheetCellFormatData, TValue> propertySetter)
		{
			TValue existingValue = propertyGetter(this.Element);
			if (EqualityComparer<TValue>.Default.Equals(value, existingValue))
				return false;

			this.OnPropertyChanging(valueType);

			GenericCachedCollection<WorksheetCellFormatData> collection = null;
			if (callBeforeAndAfterSet)
				collection = this.BeforeSet();

			propertySetter(this.Element, value);

			if (callBeforeAndAfterSet)
				this.AfterSet(collection);

			this.OnPropertyChanged(valueType, options);
			return true;
		}

		#endregion // SetFormatProperty

		#region SetFormatting

		public void SetFormatting( IWorksheetCellFormat source )
		{
			// MD 10/21/10 - TFS34398
			// Refactored this code to make it faster. Instead of going through the proxy for the property values, we will now go through the data element.
			// Also, instead of getting color values to see whether properties have changes, we will just get the color index values. That way, we don't have 
			// to do two lookups in the color palette: once before setting the formatting and once after.
			#region Refactored

			//// MD 5/12/10 - TFS26732
			//// Cache the original values.
			//HorizontalCellAlignment alignment = this.Alignment;
			//Color bottomBorderColor = this.BottomBorderColor;
			//CellBorderLineStyle bottomBorderStyle = this.BottomBorderStyle;
			//FillPatternStyle fillPattern = this.FillPattern;
			//Color fillPatternBackgroundColor = this.FillPatternBackgroundColor;
			//Color fillPatternForegroundColor = this.FillPatternForegroundColor;
			//string formatString = this.FormatString;
			//int indent = this.Indent;
			//Color leftBorderColor = this.LeftBorderColor;
			//CellBorderLineStyle leftBorderStyle = this.LeftBorderStyle;
			//ExcelDefaultableBoolean locked = this.Locked;
			//Color rightBorderColor = this.RightBorderColor;
			//CellBorderLineStyle rightBorderStyle = this.RightBorderStyle;
			//int rotation = this.Rotation;
			//ExcelDefaultableBoolean shrinkToFit = this.ShrinkToFit;
			//bool style = this.Style;
			//Color topBorderColor = this.TopBorderColor;
			//CellBorderLineStyle topBorderStyle = this.TopBorderStyle;
			//VerticalCellAlignment verticalAlignment = this.VerticalAlignment;
			//ExcelDefaultableBoolean wrapText = this.WrapText;
			//
			//// MD 10/13/10 - TFS43003
			//ExcelDefaultableBoolean fontBold = this.Font.Bold;
			//Color fontColor = this.Font.Color;
			//int fontHeight = this.Font.Height;
			//ExcelDefaultableBoolean fontItalic = this.Font.Italic;
			//string fontName = this.Font.Name;
			//ExcelDefaultableBoolean fontStrikeout = this.Font.Strikeout;
			//FontSuperscriptSubscriptStyle fontSuperscriptSubscriptStyle = this.Font.SuperscriptSubscriptStyle;
			//FontUnderlineStyle fontUnderlineStyle = this.Font.UnderlineStyle;
			//
			//// MD 5/12/10 - TFS26732
			//// Moved all code to SetFormattingHelper
			//this.SetFormattingHelper(source);
			//
			//// MD 5/12/10 - TFS26732
			//// If any values have changed, notify the owner.
			//if (alignment != this.Alignment)
			//    this.OnPropertyChanged(CellFormatValue.Alignment);
			//
			//if (bottomBorderColor != this.BottomBorderColor)
			//    this.OnPropertyChanged(CellFormatValue.BottomBorderColor);
			//
			//if (bottomBorderStyle != this.BottomBorderStyle)
			//    this.OnPropertyChanged(CellFormatValue.BottomBorderStyle);
			//
			//if (fillPattern != this.FillPattern)
			//    this.OnPropertyChanged(CellFormatValue.FillPattern);
			//
			//if (fillPatternBackgroundColor != this.FillPatternBackgroundColor)
			//    this.OnPropertyChanged(CellFormatValue.FillPatternBackgroundColor);
			//
			//if (fillPatternForegroundColor != this.FillPatternForegroundColor)
			//    this.OnPropertyChanged(CellFormatValue.FillPatternForegroundColor);
			//
			//if (formatString != this.FormatString)
			//    this.OnPropertyChanged(CellFormatValue.FormatString);
			//
			//if (indent != this.Indent)
			//    this.OnPropertyChanged(CellFormatValue.Indent);
			//
			//if (leftBorderColor != this.LeftBorderColor)
			//    this.OnPropertyChanged(CellFormatValue.LeftBorderColor);
			//
			//if (leftBorderStyle != this.LeftBorderStyle)
			//    this.OnPropertyChanged(CellFormatValue.LeftBorderStyle);
			//
			//if (locked != this.Locked)
			//    this.OnPropertyChanged(CellFormatValue.Locked);
			//
			//if (rightBorderColor != this.RightBorderColor)
			//    this.OnPropertyChanged(CellFormatValue.RightBorderColor);
			//
			//if (rightBorderStyle != this.RightBorderStyle)
			//    this.OnPropertyChanged(CellFormatValue.RightBorderStyle);
			//
			//if (rotation != this.Rotation)
			//    this.OnPropertyChanged(CellFormatValue.Rotation);
			//
			//if (shrinkToFit != this.ShrinkToFit)
			//    this.OnPropertyChanged(CellFormatValue.ShrinkToFit);
			//
			//if (style != this.Style)
			//    this.OnPropertyChanged(CellFormatValue.Style);
			//
			//if (topBorderColor != this.TopBorderColor)
			//    this.OnPropertyChanged(CellFormatValue.TopBorderColor);
			//
			//if (topBorderStyle != this.TopBorderStyle)
			//    this.OnPropertyChanged(CellFormatValue.TopBorderStyle);
			//
			//if (verticalAlignment != this.VerticalAlignment)
			//    this.OnPropertyChanged(CellFormatValue.VerticalAlignment);
			//
			//if (wrapText != this.WrapText)
			//    this.OnPropertyChanged(CellFormatValue.WrapText);
			//
			//// MD 10/13/10 - TFS43003
			//if (fontBold != this.Font.Bold)
			//    this.OnPropertyChanged(CellFormatValue.FontBold);
			//
			//if (fontColor != this.Font.Color)
			//    this.OnPropertyChanged(CellFormatValue.FontColor);
			//
			//if (fontHeight != this.Font.Height)
			//    this.OnPropertyChanged(CellFormatValue.FontHeight);
			//
			//if (fontItalic != this.Font.Italic)
			//    this.OnPropertyChanged(CellFormatValue.FontItalic);
			//
			//if (fontName != this.Font.Name)
			//    this.OnPropertyChanged(CellFormatValue.FontName);
			//
			//if (fontStrikeout != this.Font.Strikeout)
			//    this.OnPropertyChanged(CellFormatValue.FontStrikeout);
			//
			//if (fontSuperscriptSubscriptStyle != this.Font.SuperscriptSubscriptStyle)
			//    this.OnPropertyChanged(CellFormatValue.FontSuperscriptSubscriptStyle);
			//
			//if (fontUnderlineStyle != this.Font.UnderlineStyle)
			//    this.OnPropertyChanged(CellFormatValue.FontUnderlineStyle);
			//// ***************** End of TFS43003 Fix ******************** 

			#endregion // Refactored
			// MD 2/29/12 - 12.1 - Table Support
			// Refactored this code so we could have a before notification as well.
			#region Refactored

			//WorksheetCellFormatData formatData = this.Element;

			//// MD 5/12/10 - TFS26732
			//// Cache the original values.
			//HorizontalCellAlignment alignment = formatData.Alignment;

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////int bottomBorderColorIndex = formatData.BottomBorderColorIndex;
			//WorkbookColorInfo bottomBorderColorInfo = formatData.BottomBorderColorInfo;

			//CellBorderLineStyle bottomBorderStyle = formatData.BottomBorderStyle;

			//// MD 10/26/11 - TFS91546
			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////int diagonalBorderColorIndex = formatData.DiagonalBorderColorIndex;
			//WorkbookColorInfo diagonalBorderColorInfo = formatData.DiagonalBorderColorInfo;

			//DiagonalBorders diagonalBorders = formatData.DiagonalBorders;
			//CellBorderLineStyle diagonalBorderStyle = formatData.DiagonalBorderStyle;

			//// MD 1/19/12 - 12.1 - Cell Format Updates
			////FillPatternStyle fillPattern = formatData.FillPattern;
			////int fillPatternBackgroundColorIndex = formatData.FillPatternBackgroundColorIndex;
			////int fillPatternForegroundColorIndex = formatData.FillPatternForegroundColorIndex;
			////
			////// MD 11/24/10 - TFS34598
			////// We now also store RGB data if possible
			////Color fillPatternBackgroundColor = formatData.FillPatternBackgroundColorInternal;
			////Color fillPatternForegroundColor = formatData.FillPatternForegroundColorInternal;
			//CellFill fill = formatData.Fill;

			//int formatStringIndex = formatData.FormatStringIndex;
			//int indent = formatData.Indent;

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////int leftBorderColorIndex = formatData.LeftBorderColorIndex;
			//WorkbookColorInfo leftBorderColorInfo = formatData.LeftBorderColorInfo;

			//CellBorderLineStyle leftBorderStyle = formatData.LeftBorderStyle;
			//ExcelDefaultableBoolean locked = formatData.Locked;

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////int rightBorderColorIndex = formatData.RightBorderColorIndex;
			//WorkbookColorInfo rightBorderColorInfo = formatData.RightBorderColorInfo;

			//CellBorderLineStyle rightBorderStyle = formatData.RightBorderStyle;
			//int rotation = formatData.Rotation;
			//ExcelDefaultableBoolean shrinkToFit = formatData.ShrinkToFit;

			//// MD 2/27/12 - 12.1 - Table Support
			//// This should not be changed on the element.
			////bool isStyle = formatData.IsStyle;

			//// MD 12/31/11 - 12.1 - Cell Format Updates
			//WorkbookStyle style = formatData.Style;
			//WorksheetCellFormatOptions formatOptions = formatData.FormatOptions;

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////int topBorderColorIndex = formatData.TopBorderColorIndex;
			//WorkbookColorInfo topBorderColorInfo = formatData.TopBorderColorInfo;

			//CellBorderLineStyle topBorderStyle = formatData.TopBorderStyle;
			//VerticalCellAlignment verticalAlignment = formatData.VerticalAlignment;
			//ExcelDefaultableBoolean wrapText = formatData.WrapText;

			//WorkbookFontData fontData = formatData.FontInternal.Element;

			//// MD 10/13/10 - TFS43003
			//ExcelDefaultableBoolean fontBold = fontData.Bold;

			//// MD 1/17/12 - 12.1 - Cell Format Updates
			////int fontColorIndex = fontData.ColorIndex;
			//WorkbookColorInfo fontColorInfo = fontData.ColorInfo;

			//int fontHeight = fontData.Height;
			//ExcelDefaultableBoolean fontItalic = fontData.Italic;
			//string fontName = fontData.Name;
			//ExcelDefaultableBoolean fontStrikeout = fontData.Strikeout;
			//FontSuperscriptSubscriptStyle fontSuperscriptSubscriptStyle = fontData.SuperscriptSubscriptStyle;
			//FontUnderlineStyle fontUnderlineStyle = fontData.UnderlineStyle;

			//// MD 1/17/12 - 12.1 - Cell Format Updates
			////// MD 1/19/11 - TFS62268
			////// We now also store RGB data if possible
			////Color colorInternal = fontData.ColorInternal;

			//// MD 5/12/10 - TFS26732
			//// Moved all code to SetFormattingHelper
			//this.SetFormattingHelper(source);

			//// MD 11/1/11 - TFS94534
			//// The rows, columns, and cells can't have the Style property set to True.
			//// MD 2/29/12 - 12.1 - Table Support
			////if (this.element.IsStyle && 
			////    this.owner != null && 
			////    this.owner.CanOwnStyleFormat == false)
			//if (this.element.Type != WorksheetCellFormatType.CellFormat)
			//{
			//    // MD 12/21/11 - 12.1 - Table Support
			//    //this.BeforeSet();
			//    //this.element.Style = false;
			//    //this.AfterSet();
			//    GenericCachedCollection<WorksheetCellFormatData> collection = this.BeforeSet();

			//    // MD 2/27/12 - 12.1 - Table Support
			//    //this.element.IsStyle = false;
			//    this.element.Type = WorksheetCellFormatType.CellFormat;

			//    this.AfterSet(collection);
			//}

			//formatData = this.element;
			//fontData = formatData.FontInternal.Element;

			//// MD 4/18/11 - TFS62026
			//// Instead of sending off individual OnPropertyChanged notifications, we will send out one with a list of changed values.
			//// Replaced all calls to OnPropertyChanged below with adds into the changedValues collection.
			//List<CellFormatValue> changedValues = new List<CellFormatValue>();

			//// MD 5/12/10 - TFS26732
			//// If any values have changed, notify the owner.
			//if (alignment != formatData.Alignment)
			//    changedValues.Add(CellFormatValue.Alignment);

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (bottomBorderColorIndex != formatData.BottomBorderColorIndex)
			////    changedValues.Add(CellFormatValue.BottomBorderColor);
			//if (bottomBorderColorInfo != formatData.BottomBorderColorInfo)
			//    changedValues.Add(CellFormatValue.BottomBorderColorInfo);

			//if (bottomBorderStyle != formatData.BottomBorderStyle)
			//    changedValues.Add(CellFormatValue.BottomBorderStyle);

			//// MD 10/26/11 - TFS91546
			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (diagonalBorderColorIndex != formatData.DiagonalBorderColorIndex)
			////    changedValues.Add(CellFormatValue.DiagonalBorderColor);
			//if (diagonalBorderColorInfo != formatData.DiagonalBorderColorInfo)
			//    changedValues.Add(CellFormatValue.DiagonalBorderColorInfo);

			//// MD 10/26/11 - TFS91546
			//if (diagonalBorders != formatData.DiagonalBorders)
			//    changedValues.Add(CellFormatValue.DiagonalBorders);

			//// MD 10/26/11 - TFS91546
			//if (diagonalBorderStyle != formatData.DiagonalBorderStyle)
			//    changedValues.Add(CellFormatValue.DiagonalBorderStyle);

			//// MD 1/19/12 - 12.1 - Cell Format Updates
			//#region Removed

			////if (fillPattern != formatData.FillPattern)
			////    changedValues.Add(CellFormatValue.FillPattern);
			////
			////// MD 11/24/10 - TFS34598
			////// We now also store RGB data if possible
			//////if (fillPatternBackgroundColorIndex != formatData.FillPatternBackgroundColorIndex)
			////if (fillPatternBackgroundColorIndex != formatData.FillPatternBackgroundColorIndex || fillPatternBackgroundColor != formatData.FillPatternBackgroundColorInternal)
			////    changedValues.Add(CellFormatValue.FillPatternBackgroundColor);
			////
			////// MD 11/24/10 - TFS34598
			////// We now also store RGB data if possible
			//////if (fillPatternForegroundColorIndex != formatData.FillPatternForegroundColorIndex)
			////if (fillPatternForegroundColorIndex != formatData.FillPatternForegroundColorIndex || fillPatternForegroundColor != formatData.FillPatternForegroundColorInternal)
			////    changedValues.Add(CellFormatValue.FillPatternForegroundColor);

			//#endregion // Removed
			//if (Object.Equals(fill, formatData.Fill) == false)
			//    changedValues.Add(CellFormatValue.Fill);

			//if (formatStringIndex != formatData.FormatStringIndex)
			//    changedValues.Add(CellFormatValue.FormatString);

			//if (indent != formatData.Indent)
			//    changedValues.Add(CellFormatValue.Indent);

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (leftBorderColorIndex != formatData.LeftBorderColorIndex)
			////    changedValues.Add(CellFormatValue.LeftBorderColor);
			//if (leftBorderColorInfo != formatData.LeftBorderColorInfo)
			//    changedValues.Add(CellFormatValue.LeftBorderColorInfo);

			//if (leftBorderStyle != formatData.LeftBorderStyle)
			//    changedValues.Add(CellFormatValue.LeftBorderStyle);

			//if (locked != formatData.Locked)
			//    changedValues.Add(CellFormatValue.Locked);

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (rightBorderColorIndex != formatData.RightBorderColorIndex)
			////    changedValues.Add(CellFormatValue.RightBorderColor);
			//if (rightBorderColorInfo != formatData.RightBorderColorInfo)
			//    changedValues.Add(CellFormatValue.RightBorderColorInfo);

			//if (rightBorderStyle != formatData.RightBorderStyle)
			//    changedValues.Add(CellFormatValue.RightBorderStyle);

			//if (rotation != formatData.Rotation)
			//    changedValues.Add(CellFormatValue.Rotation);

			//if (shrinkToFit != formatData.ShrinkToFit)
			//    changedValues.Add(CellFormatValue.ShrinkToFit);

			//// MD 2/27/12 - 12.1 - Table Support
			////if (isStyle != formatData.IsStyle)
			////    changedValues.Add(CellFormatValue.IsStyle);

			//// MD 12/31/11 - 12.1 - Cell Format Updates
			//if (style != formatData.Style)
			//    changedValues.Add(CellFormatValue.Style);

			//// MD 12/31/11 - 12.1 - Cell Format Updates
			//if (formatOptions != formatData.FormatOptions)
			//    changedValues.Add(CellFormatValue.FormatOptions);

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (topBorderColorIndex != formatData.TopBorderColorIndex)
			////    changedValues.Add(CellFormatValue.TopBorderColor);
			//if (topBorderColorInfo != formatData.TopBorderColorInfo)
			//    changedValues.Add(CellFormatValue.TopBorderColorInfo);

			//if (topBorderStyle != formatData.TopBorderStyle)
			//    changedValues.Add(CellFormatValue.TopBorderStyle);

			//if (verticalAlignment != formatData.VerticalAlignment)
			//    changedValues.Add(CellFormatValue.VerticalAlignment);

			//if (wrapText != formatData.WrapText)
			//    changedValues.Add(CellFormatValue.WrapText);

			//// MD 10/13/10 - TFS43003
			//if (fontBold != fontData.Bold)
			//    changedValues.Add(CellFormatValue.FontBold);

			//// MD 1/19/11 - TFS62268
			//// We now also store RGB data if possible
			////if (fontColorIndex != fontData.ColorIndex)
			//// MD 1/17/12 - 12.1 - Cell Format Updates
			////if (fontColorIndex != fontData.ColorIndex || colorInternal != fontData.ColorInternal)
			////    changedValues.Add(CellFormatValue.FontColor);
			//if (fontColorInfo != fontData.ColorInfo)
			//    changedValues.Add(CellFormatValue.FontColorInfo);

			//if (fontHeight != fontData.Height)
			//    changedValues.Add(CellFormatValue.FontHeight);

			//if (fontItalic != fontData.Italic)
			//    changedValues.Add(CellFormatValue.FontItalic);

			//if (fontName != fontData.Name)
			//    changedValues.Add(CellFormatValue.FontName);

			//if (fontStrikeout != fontData.Strikeout)
			//    changedValues.Add(CellFormatValue.FontStrikeout);

			//if (fontSuperscriptSubscriptStyle != fontData.SuperscriptSubscriptStyle)
			//    changedValues.Add(CellFormatValue.FontSuperscriptSubscriptStyle);

			//if (fontUnderlineStyle != fontData.UnderlineStyle)
			//    changedValues.Add(CellFormatValue.FontUnderlineStyle);
			//// ***************** End of TFS43003 Fix ********************

			//// MD 4/18/11 - TFS62026
			//// Fire off the one OnPropertyChanged call.
			//if (changedValues.Count > 0)
			//    this.OnPropertyChanged(changedValues, CellFormatValueChangedOptions.DefaultBehavior);

			#endregion // Refactored
			WorksheetCellFormatType expectedType = this.element.Type;
			List<CellFormatValue> changedValues = this.GetDifferences(source);

			if (changedValues.Count > 0)
				this.OnPropertyChanging(changedValues);

			this.SetFormattingHelper(source);

			if (this.element.Type != expectedType)
			{
				GenericCachedCollection<WorksheetCellFormatData> collection = this.BeforeSet();
				this.element.Type = expectedType;
				this.AfterSet(collection);
			}

			if (changedValues.Count > 0)
				this.OnPropertyChanged(changedValues, CellFormatValueChangedOptions.DefaultBehavior);
		}

		// MD 5/12/10 - TFS26732
		// Moved all code from SetFormatting
		internal void SetFormattingHelper(IWorksheetCellFormat source)
		{
			if ( source == null )
				throw new ArgumentNullException( "source", SR.GetString( "LE_ArgumentNullException_SourceFormatting" ) );

			WorksheetCellFormatData data = source as WorksheetCellFormatData;

			// If the source is a data element, just set the element as this proxy's element
			if ( data != null )
			{
				// MD 12/22/11 - 12.1 - Table Support
				// We don't need this anymore because SetToElement clones the new element if it is from a different cache collection.
				//if ( data.Workbook != this.Workbook )
				//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_FormatFromOtherWorkbook" ), "source" );

				this.SetToElement( data );
				return;
			}

			WorksheetCellFormatProxy proxy = source as WorksheetCellFormatProxy;

			// If the source is another proxy, apply that proxy's element as this proxy's element
			if ( proxy != null )
			{
				// MD 12/22/11 - 12.1 - Table Support
				// We don't need this anymore because SetToElement clones the new element if it is from a different cache collection.
				//if ( proxy.Workbook != this.Workbook )
				//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_FormatFromOtherWorkbook" ), "source" );

				this.SetToElement( proxy.Element );
				return;
			}

			// MD 12/21/11 - 12.1 - Table Support
			#region Old Code

			//// MD 6/19/07 - BR24109
			//// Since we are changing the element, call the appropriate method before setting any properties
			//this.BeforeSet();

			//// Otherwise, let the element determine how to set the formatting from the source
			//this.Element.SetFormatting( source );

			//// MD 6/19/07 - BR24109
			//// Since we changed the element, call the appropriate method afterwards
			//this.AfterSet();

			#endregion // Old Code
			// Otherwise, let the element determine how to set the formatting from the source
			GenericCachedCollection<WorksheetCellFormatData> collection = this.BeforeSet();
			this.Element.SetFormatting(source);
			this.AfterSet(collection);
		}

		#endregion SetFormatting

		// MD 3/5/12 - 12.1 - Table Support
		#region SetOwner

		protected void SetOwner(IWorksheetCellFormatProxyOwner owner)
		{
			this.owner = owner;
		}

		#endregion // SetOwner

		// MD 5/12/10 - TFS26732
		#region SetValue

		public void SetValue(CellFormatValue valueToSet, object value)
		{
			// MD 10/21/10 - TFS34398
			// Moved all code to the new overload
			this.SetValue(valueToSet, value, true, CellFormatValueChangedOptions.DefaultBehavior);
		}

		// MD 10/21/10 - TFS34398
		// Added a new overload that takes a boolean indicating wether to call the BeforeSet and AfterSet methods on the proxy when setting the value.
		public void SetValue(CellFormatValue valueToSet, object value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			switch (valueToSet)
			{
				case CellFormatValue.Alignment:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.Alignment = (HorizontalCellAlignment)value;
					this.SetAlignment((HorizontalCellAlignment)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.BottomBorderColorInfo:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.BottomBorderColor = (Color)value;
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//this.SetBottomBorderColor((Color)value, callBeforeAndAfterSet, options);
					this.SetBottomBorderColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.BottomBorderStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.BottomBorderStyle = (CellBorderLineStyle)value;
					this.SetBottomBorderStyle((CellBorderLineStyle)value, callBeforeAndAfterSet, options);
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderColorInfo:
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//this.SetDiagonalBorderColor((Color)value, callBeforeAndAfterSet, options);
					this.SetDiagonalBorderColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorders:
					this.SetDiagonalBorders((DiagonalBorders)value, callBeforeAndAfterSet, options);
					break;

				// MD 10/26/11 - TFS91546
				case CellFormatValue.DiagonalBorderStyle:
					this.SetDiagonalBorderStyle((CellBorderLineStyle)value, callBeforeAndAfterSet, options);
					break;

				// MD 1/19/12 - 12.1 - Cell Format Updates
				#region Removed

				//case CellFormatValue.FillPattern:
				//    // MD 10/21/10 - TFS34398
				//    // Call the new setter method.
				//    //this.FillPattern = (FillPatternStyle)value;
				//    this.SetFillPattern((FillPatternStyle)value, callBeforeAndAfterSet, options);
				//    break;

				//case CellFormatValue.FillPatternBackgroundColor:
				//    // MD 10/21/10 - TFS34398
				//    // Call the new setter method.
				//    //this.FillPatternBackgroundColor = (Color)value;
				//    this.SetFillPatternBackgroundColor((Color)value, callBeforeAndAfterSet, options);
				//    break;

				//case CellFormatValue.FillPatternForegroundColor:
				//    // MD 10/21/10 - TFS34398
				//    // Call the new setter method.
				//    //this.FillPatternForegroundColor = (Color)value;
				//    this.SetFillPatternForegroundColor((Color)value, callBeforeAndAfterSet, options);
				//    break;

				#endregion // Removed
				case CellFormatValue.Fill:
					this.SetFill((CellFill)value, callBeforeAndAfterSet, options);
					break;

				// MD 10/13/10 - TFS43003
				case CellFormatValue.FontBold:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetBold((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontColorInfo:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					// MD 1/17/12 - 12.1 - Cell Format Updates
					//this.FontInternal.SetColor((Color)value, callBeforeAndAfterSet, options);
					this.FontInternal.SetColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontHeight:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetHeight((int)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontItalic:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetItalic((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontName:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetName((string)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontStrikeout:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetStrikeout((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontSuperscriptSubscriptStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetSuperscriptSubscriptStyle((FontSuperscriptSubscriptStyle)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FontUnderlineStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					this.FontInternal.SetUnderlineStyle((FontUnderlineStyle)value, callBeforeAndAfterSet, options);
					break;
				// ***************** End of TFS43003 Fix ********************

				// MD 12/31/11 - 12.1 - Cell Format Updates
				case CellFormatValue.FormatOptions:
					this.SetFormatOptions((WorksheetCellFormatOptions)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.FormatString:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.FormatString = (string)value;
					this.SetFormatString((string)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.Indent:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.Indent = (int)value;
					this.SetIndent((int)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.LeftBorderColorInfo:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.LeftBorderColor = (Color)value;
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//this.SetLeftBorderColor((Color)value, callBeforeAndAfterSet, options);
					this.SetLeftBorderColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.LeftBorderStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.LeftBorderStyle = (CellBorderLineStyle)value;
					this.SetLeftBorderStyle((CellBorderLineStyle)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.Locked:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.Locked = (ExcelDefaultableBoolean)value;
					this.SetLocked((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.RightBorderColorInfo:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.RightBorderColor = (Color)value;
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//this.SetRightBorderColor((Color)value, callBeforeAndAfterSet, options);
					this.SetRightBorderColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.RightBorderStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.RightBorderStyle = (CellBorderLineStyle)value;
					this.SetRightBorderStyle((CellBorderLineStyle)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.Rotation:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.Rotation = (int)value;
					this.SetRotation((int)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.ShrinkToFit:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.ShrinkToFit = (ExcelDefaultableBoolean)value;
					this.SetShrinkToFit((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				// MD 2/27/12 - 12.1 - Table Support
				// This is not really a format property, so there shouldn't be a setter for it.
				//case CellFormatValue.IsStyle:
				//    // MD 10/21/10 - TFS34398
				//    // Call the new setter method.
				//    //this.Style = (bool)value;
				//    this.SetIsStyle((bool)value, callBeforeAndAfterSet, options);
				//    break;

				// MD 12/31/11 - 12.1 - Cell Format Updates
				case CellFormatValue.Style:
					this.SetStyle((WorkbookStyle)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.TopBorderColorInfo:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.TopBorderColor = (Color)value;
					// MD 1/16/12 - 12.1 - Cell Format Updates
					//this.SetTopBorderColor((Color)value, callBeforeAndAfterSet, options);
					this.SetTopBorderColorInfo((WorkbookColorInfo)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.TopBorderStyle:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.TopBorderStyle = (CellBorderLineStyle)value;
					this.SetTopBorderStyle((CellBorderLineStyle)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.VerticalAlignment:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.VerticalAlignment = (VerticalCellAlignment)value;
					this.SetVerticalAlignment((VerticalCellAlignment)value, callBeforeAndAfterSet, options);
					break;

				case CellFormatValue.WrapText:
					// MD 10/21/10 - TFS34398
					// Call the new setter method.
					//this.WrapText = (ExcelDefaultableBoolean)value;
					this.SetWrapText((ExcelDefaultableBoolean)value, callBeforeAndAfterSet, options);
					break;

				default:
					Utilities.DebugFail("Unknown format value: " + value);
					break;
			}
		} 

		#endregion // SetValue

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Alignment

		public HorizontalCellAlignment Alignment
		{
			get { return this.Element.Alignment; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.Alignment != value )
				//{
				//    this.BeforeSet();
				//    this.element.Alignment = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.Alignment);
				//}
				this.SetAlignment(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetAlignment(HorizontalCellAlignment value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.Alignment != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.Alignment = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.Alignment, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.Alignment, callBeforeAndAfterSet, options, Utilities.CellFormatAlignmentGetter, Utilities.CellFormatAlignmentSetter);
		}

		#endregion Alignment

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Removed

		//#region BottomBorderColor

		//public Color BottomBorderColor
		//{
		//    get { return this.Element.BottomBorderColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.BottomBorderColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.BottomBorderColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.BottomBorderColor);
		//        //}
		//        this.SetBottomBorderColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetBottomBorderColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.BottomBorderColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.BottomBorderColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.BottomBorderColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.BottomBorderColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.BottomBorderColor, callBeforeAndAfterSet, options, Utilities.CellFormatBottomBorderColorGetter, Utilities.CellFormatBottomBorderColorSetter))
		//        this.element.ClearRoundTripProp(ExtPropType.BottomBorderColor);

		//}

		//#endregion BottomBorderColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region BottomBorderColorInfo

		public WorkbookColorInfo BottomBorderColorInfo
		{
			get { return this.Element.BottomBorderColorInfo; }
			set
			{
				this.SetBottomBorderColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetBottomBorderColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.BottomBorderColorInfo, callBeforeAndAfterSet, options, Utilities.CellFormatBottomBorderColorInfoGetter, Utilities.CellFormatBottomBorderColorInfoSetter);
		}

		#endregion BottomBorderColorInfo

		#region BottomBorderStyle

		public CellBorderLineStyle BottomBorderStyle
		{
			get { return this.Element.BottomBorderStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.BottomBorderStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.BottomBorderStyle = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.BottomBorderStyle);
				//}
				this.SetBottomBorderStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetBottomBorderStyle(CellBorderLineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.BottomBorderStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.BottomBorderStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.BottomBorderStyle, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.BottomBorderStyle, callBeforeAndAfterSet, options, Utilities.CellFormatBottomBorderStyleGetter, Utilities.CellFormatBottomBorderStyleSetter);
		}

		#endregion BottomBorderStyle

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Removed

		//// MD 10/26/11 - TFS91546
		//#region DiagonalBorderColor

		//public Color DiagonalBorderColor
		//{
		//    get { return this.Element.DiagonalBorderColor; }
		//    set
		//    {
		//        this.SetDiagonalBorderColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//public void SetDiagonalBorderColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.DiagonalBorderColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.DiagonalBorderColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.DiagonalBorderColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    this.OnPropertyChanged(CellFormatValue.DiagonalBorderColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.DiagonalBorderColor, callBeforeAndAfterSet, options, Utilities.CellFormatDiagonalBorderColorGetter, Utilities.CellFormatDiagonalBorderColorSetter))
		//        this.element.ClearRoundTripProp(ExtPropType.DiagonalBorderColor);
		//}

		//#endregion DiagonalBorderColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region DiagonalBorderColorInfo

		public WorkbookColorInfo DiagonalBorderColorInfo
		{
			get { return this.Element.DiagonalBorderColorInfo; }
			set
			{
				this.SetDiagonalBorderColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetDiagonalBorderColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.DiagonalBorderColorInfo, callBeforeAndAfterSet, options, Utilities.CellFormatDiagonalBorderColorInfoGetter, Utilities.CellFormatDiagonalBorderColorInfoSetter);
		}

		#endregion DiagonalBorderColorInfo

		// MD 10/26/11 - TFS91546
		#region DiagonalBorders

		public DiagonalBorders DiagonalBorders
		{
			get { return this.Element.DiagonalBorders; }
			set
			{
				this.SetDiagonalBorders(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetDiagonalBorders(DiagonalBorders value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.DiagonalBorders != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.DiagonalBorders = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    this.OnPropertyChanged(CellFormatValue.DiagonalBorders, options);
			//} 

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.DiagonalBorders, callBeforeAndAfterSet, options, Utilities.CellFormatDiagonalBordersGetter, Utilities.CellFormatDiagonalBordersSetter);
		}

		#endregion DiagonalBorderStyle

		// MD 10/26/11 - TFS91546
		#region DiagonalBorderStyle

		public CellBorderLineStyle DiagonalBorderStyle
		{
			get { return this.Element.DiagonalBorderStyle; }
			set
			{
				this.SetDiagonalBorderStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetDiagonalBorderStyle(CellBorderLineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.DiagonalBorderStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.DiagonalBorderStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    this.OnPropertyChanged(CellFormatValue.DiagonalBorderStyle, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.DiagonalBorderStyle, callBeforeAndAfterSet, options, Utilities.CellFormatDiagonalBorderStyleGetter, Utilities.CellFormatDiagonalBorderStyleSetter);
		}

		#endregion DiagonalBorderStyle

		#region Fill

		public CellFill Fill
		{
			get { return this.Element.Fill; }
			set
			{
				this.SetFill(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetFill(CellFill value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.Fill, callBeforeAndAfterSet, options, Utilities.CellFormatFillGetter, Utilities.CellFormatFillSetter);
		}

		#endregion Fill

		// MD 1/19/12 - 12.1 - Cell Format Updates
		#region Removed

		//#region FillPattern

		//public FillPatternStyle FillPattern
		//{
		//    get { return this.Element.FillPattern; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.FillPattern != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.FillPattern = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.FillPattern);
		//        //}
		//        this.SetFillPattern(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetFillPattern(FillPatternStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.FillPattern != value)
		//    //{
		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.FillPattern = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.FillPattern, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.FillPattern, callBeforeAndAfterSet, options, Utilities.CellFormatFillPatternGetter, Utilities.CellFormatFillPatternSetter))
		//    {
		//        this.element.ClearRoundTripProp(ExtPropType.BackgroundColor);
		//        this.element.ClearRoundTripProp(ExtPropType.GradientFill);
		//    }
		//}

		//#endregion FillPattern

		//#region FillPatternBackgroundColor

		//public Color FillPatternBackgroundColor
		//{
		//    get { return this.Element.FillPatternBackgroundColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.FillPatternBackgroundColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.FillPatternBackgroundColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.FillPatternBackgroundColor);
		//        //}
		//        this.SetFillPatternBackgroundColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetFillPatternBackgroundColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.FillPatternBackgroundColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.BackgroundColor);
		//    //    this.element.ClearRoundTripProp(ExtPropType.GradientFill);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.FillPatternBackgroundColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.FillPatternBackgroundColor, options);
		//    //} 

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.FillPatternBackgroundColor, callBeforeAndAfterSet, options, Utilities.CellFormatFillPatternBackgroundColorGetter, Utilities.CellFormatFillPatternBackgroundColorSetter))
		//    {
		//        this.element.ClearRoundTripProp(ExtPropType.BackgroundColor);
		//        this.element.ClearRoundTripProp(ExtPropType.GradientFill);
		//    }
		//}

		//#endregion FillPatternBackgroundColor

		//#region FillPatternForegroundColor

		//public Color FillPatternForegroundColor
		//{
		//    get { return this.Element.FillPatternForegroundColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.FillPatternForegroundColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.FillPatternForegroundColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.FillPatternForegroundColor);
		//        //}
		//        this.SetFillPatternForegroundColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetFillPatternForegroundColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.FillPatternForegroundColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.ForegroundColor);
		//    //    this.element.ClearRoundTripProp(ExtPropType.GradientFill);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.FillPatternForegroundColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.FillPatternForegroundColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.FillPatternForegroundColor, callBeforeAndAfterSet, options, Utilities.CellFormatFillPatternForegroundColorGetter, Utilities.CellFormatFillPatternForegroundColorSetter))
		//    {
		//        this.element.ClearRoundTripProp(ExtPropType.BackgroundColor);
		//        this.element.ClearRoundTripProp(ExtPropType.GradientFill);
		//    }
		//}

		//#endregion FillPatternForegroundColor

		#endregion // Removed

		#region Font

		public IWorkbookFont Font
		{
			// MD 10/21/10 - TFS34398
			// Moved this code to a FontInternal property so we don't have to cast this to a WorkbookFontOwnerAccessor everytime we need a reference to it.
			//get
			//{
			//    if ( this.fontAccessor == null )
			//        this.fontAccessor = new WorkbookFontOwnerAccessor( this );
			//
			//    return this.fontAccessor;
			//}
			get { return this.FontInternal; }
		}

		#endregion Font

		// MD 12/31/11 - 12.1 - Cell Format Updates
		#region FormatOptions

		public WorksheetCellFormatOptions FormatOptions
		{
			get { return this.Element.FormatOptions; }
			set
			{
				this.SetFormatOptions(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetFormatOptions(WorksheetCellFormatOptions value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			if (this.owner != null)
				this.owner.VerifyFormatOptions(this, value);

			this.SetFormatProperty(value, CellFormatValue.FormatOptions, callBeforeAndAfterSet, options, Utilities.CellFormatFormatOptionsGetter, Utilities.CellFormatFormatOptionsSetter);
		}

		#endregion // FormatOptions

		#region FormatString

		public string FormatString
		{
			get { return this.Element.FormatString; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.FormatString != value )
				//{
				//    this.BeforeSet();
				//    this.element.FormatString = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.FormatString);
				//}
				this.SetFormatString(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetFormatString(string value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.FormatString != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.FormatString = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.FormatString, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.FormatString, callBeforeAndAfterSet, options, Utilities.CellFormatFormatStringGetter, Utilities.CellFormatFormatStringSetter);
		}

		#endregion FormatString

		#region Indent

		public int Indent
		{
			get { return this.Element.Indent; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.Indent != value )
				//{
				//    this.BeforeSet();
				//    this.element.Indent = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.Indent);
				//}
				this.SetIndent(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetIndent(int value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.Indent != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.Indent = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.Indent, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.Indent, callBeforeAndAfterSet, options, Utilities.CellFormatIndentGetter, Utilities.CellFormatIndentSetter);
		}

		#endregion Indent

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Removed

		//#region LeftBorderColor

		//public Color LeftBorderColor
		//{
		//    get { return this.Element.LeftBorderColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.LeftBorderColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.LeftBorderColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.LeftBorderColor);
		//        //}
		//        this.SetLeftBorderColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetLeftBorderColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.LeftBorderColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.LeftBorderColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.LeftBorderColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.LeftBorderColor, options);
		//    //} 

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.LeftBorderColor, callBeforeAndAfterSet, options, Utilities.CellFormatLeftBorderColorGetter, Utilities.CellFormatLeftBorderColorSetter))
		//        this.element.ClearRoundTripProp(ExtPropType.LeftBorderColor);
		//}

		//#endregion LeftBorderColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region LeftBorderColorInfo

		public WorkbookColorInfo LeftBorderColorInfo
		{
			get { return this.Element.LeftBorderColorInfo; }
			set
			{
				this.SetLeftBorderColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetLeftBorderColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.LeftBorderColorInfo, callBeforeAndAfterSet, options, Utilities.CellFormatLeftBorderColorInfoGetter, Utilities.CellFormatLeftBorderColorInfoSetter);
		}

		#endregion LeftBorderColorInfo

		#region LeftBorderStyle

		public CellBorderLineStyle LeftBorderStyle
		{
			get { return this.Element.LeftBorderStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.LeftBorderStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.LeftBorderStyle = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.LeftBorderStyle);
				//}
				this.SetLeftBorderStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetLeftBorderStyle(CellBorderLineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.LeftBorderStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.LeftBorderStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.LeftBorderStyle, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.LeftBorderStyle, callBeforeAndAfterSet, options, Utilities.CellFormatLeftBorderStyleGetter, Utilities.CellFormatLeftBorderStyleSetter);
		}

		#endregion LeftBorderStyle

		#region Locked

		public ExcelDefaultableBoolean Locked
		{
			get { return this.Element.Locked; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.Locked != value )
				//{
				//    this.BeforeSet();
				//    this.element.Locked = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.Locked);
				//}
				this.SetLocked(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetLocked(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.Locked != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.Locked = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.Locked, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.Locked, callBeforeAndAfterSet, options, Utilities.CellFormatLockedGetter, Utilities.CellFormatLockedSetter);
		}

		#endregion Locked

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Removed

		//#region RightBorderColor

		//public Color RightBorderColor
		//{
		//    get { return this.Element.RightBorderColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.RightBorderColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.RightBorderColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.RightBorderColor);
		//        //}
		//        this.SetRightBorderColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetRightBorderColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.RightBorderColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.RightBorderColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.RightBorderColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.RightBorderColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.RightBorderColor, callBeforeAndAfterSet, options, Utilities.CellFormatRightBorderColorGetter, Utilities.CellFormatRightBorderColorSetter))
		//        this.element.ClearRoundTripProp(ExtPropType.RightBorderColor);
		//}

		//#endregion RightBorderColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region RightBorderColorInfo

		public WorkbookColorInfo RightBorderColorInfo
		{
			get { return this.Element.RightBorderColorInfo; }
			set
			{
				this.SetRightBorderColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetRightBorderColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.RightBorderColorInfo, callBeforeAndAfterSet, options, Utilities.CellFormatRightBorderColorInfoGetter, Utilities.CellFormatRightBorderColorInfoSetter);
		}

		#endregion RightBorderColorInfo

		#region RightBorderStyle

		public CellBorderLineStyle RightBorderStyle
		{
			get { return this.Element.RightBorderStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.RightBorderStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.RightBorderStyle = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.RightBorderStyle);
				//}
				this.SetRightBorderStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetRightBorderStyle(CellBorderLineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.RightBorderStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.RightBorderStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.RightBorderStyle, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.RightBorderStyle, callBeforeAndAfterSet, options, Utilities.CellFormatRightBorderStyleGetter, Utilities.CellFormatRightBorderStyleSetter);
		}

		#endregion RightBorderStyle

		#region Rotation

		public int Rotation
		{
			get { return this.Element.Rotation; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.Rotation != value )
				//{
				//    this.BeforeSet();
				//    this.element.Rotation = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.Rotation);
				//}
				this.SetRotation(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetRotation(int value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.Rotation != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.Rotation = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.Rotation, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.Rotation, callBeforeAndAfterSet, options, Utilities.CellFormatRotationGetter, Utilities.CellFormatRotationSetter);
		}

		#endregion Rotation

		#region ShrinkToFit

		public ExcelDefaultableBoolean ShrinkToFit
		{
			get { return this.Element.ShrinkToFit; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.ShrinkToFit != value )
				//{
				//    this.BeforeSet();
				//    this.element.ShrinkToFit = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.ShrinkToFit);
				//}
				this.SetShrinkToFit(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetShrinkToFit(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.ShrinkToFit != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.ShrinkToFit = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.ShrinkToFit, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.ShrinkToFit, callBeforeAndAfterSet, options, Utilities.CellFormatShrinkToFitGetter, Utilities.CellFormatShrinkToFitSetter);
		}

		#endregion ShrinkToFit

		// MD 2/27/12 - 12.1 - Table Support
		// The proxies always own cell formats (not style or differential formats), so there is no need for this.
		#region Removed

		//#region IsStyle

		//// MD 12/30/11 - 12.1 - Cell Format Updates
		//// Renamed for clarity
		////public bool Style
		//internal bool IsStyle
		//{
		//    get { return this.Element.IsStyle; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.Style != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.Style = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.Style);
		//        //}
		//        this.SetIsStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//// MD 12/30/11 - 12.1 - Cell Format Updates
		//// Renamed for clarity
		////public void SetStyle(bool value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//internal void SetIsStyle(bool value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.Style != value)
		//    //{
		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.Style = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.Style, options);
		//    //} 

		//    #endregion // Refactored
		//    this.SetFormatProperty(value, CellFormatValue.IsStyle, callBeforeAndAfterSet, options, Utilities.CellFormatIsStyleGetter, Utilities.CellFormatIsStyleSetter);
		//}

		//#endregion IsStyle

		#endregion // Removed

		// MD 12/31/11 - 12.1 - Cell Format Updates
		#region Style

		public WorkbookStyle Style
		{
			get { return this.Element.Style; }
			set
			{
				this.SetStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		internal void SetStyle(WorkbookStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.Style, callBeforeAndAfterSet, options, Utilities.CellFormatStyleGetter, Utilities.CellFormatStyleSetter);
		}

		#endregion // Style

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region Removed

		//#region TopBorderColor

		//public Color TopBorderColor
		//{
		//    get { return this.Element.TopBorderColor; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.element.TopBorderColor != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.TopBorderColor = value;
		//        //    this.AfterSet();
		//        //
		//        //    // MD 5/12/10 - TFS26732
		//        //    this.OnPropertyChanged(CellFormatValue.TopBorderColor);
		//        //}
		//        this.SetTopBorderColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//// MD 10/21/10 - TFS34398
		//// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		//public void SetTopBorderColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFormatProperty method.
		//    #region Refactored

		//    //if (this.TopBorderColor != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.element.ClearRoundTripProp(ExtPropType.TopBorderColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.BeforeSet();

		//    //    this.element.TopBorderColor = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.AfterSet();

		//    //    // MD 5/12/10 - TFS26732
		//    //    this.OnPropertyChanged(CellFormatValue.TopBorderColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFormatProperty(value, CellFormatValue.TopBorderColor, callBeforeAndAfterSet, options, Utilities.CellFormatTopBorderColorGetter, Utilities.CellFormatTopBorderColorSetter))
		//        this.element.ClearRoundTripProp(ExtPropType.TopBorderColor);
		//}

		//#endregion TopBorderColor

		#endregion // Removed

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region TopBorderColorInfo

		public WorkbookColorInfo TopBorderColorInfo
		{
			get { return this.Element.TopBorderColorInfo; }
			set
			{
				this.SetTopBorderColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetTopBorderColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFormatProperty(value, CellFormatValue.TopBorderColorInfo, callBeforeAndAfterSet, options, Utilities.CellFormatTopBorderColorInfoGetter, Utilities.CellFormatTopBorderColorInfoSetter);
		}

		#endregion TopBorderColorInfo

		#region TopBorderStyle

		public CellBorderLineStyle TopBorderStyle
		{
			get { return this.Element.TopBorderStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.TopBorderStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.TopBorderStyle = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.TopBorderStyle);
				//}
				this.SetTopBorderStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetTopBorderStyle(CellBorderLineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.TopBorderStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.TopBorderStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.TopBorderStyle, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.TopBorderStyle, callBeforeAndAfterSet, options, Utilities.CellFormatTopBorderStyleGetter, Utilities.CellFormatTopBorderStyleSetter);
		}

		#endregion TopBorderStyle

		#region VerticalAlignment

		public VerticalCellAlignment VerticalAlignment
		{
			get { return this.Element.VerticalAlignment; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.VerticalAlignment != value )
				//{
				//    this.BeforeSet();
				//    this.element.VerticalAlignment = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.VerticalAlignment);
				//}
				this.SetVerticalAlignment(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetVerticalAlignment(VerticalCellAlignment value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.VerticalAlignment != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.VerticalAlignment = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.VerticalAlignment, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.VerticalAlignment, callBeforeAndAfterSet, options, Utilities.CellFormatVerticalAlignmentGetter, Utilities.CellFormatVerticalAlignmentSetter);
		}

		#endregion VerticalAlignment

		#region WrapText

		public ExcelDefaultableBoolean WrapText
		{
			get { return this.Element.WrapText; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.element.WrapText != value )
				//{
				//    this.BeforeSet();
				//    this.element.WrapText = value;
				//    this.AfterSet();
				//
				//    // MD 5/12/10 - TFS26732
				//    this.OnPropertyChanged(CellFormatValue.WrapText);
				//}
				this.SetWrapText(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		// MD 10/21/10 - TFS34398
		// Added a helper method so we can prevent calling BeforeSet and AfterSet in certain cases.
		public void SetWrapText(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFormatProperty method.
			#region Refactored

			//if (this.WrapText != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.BeforeSet();

			//    this.element.WrapText = value;

			//    if (callBeforeAndAfterSet)
			//        this.AfterSet();

			//    // MD 5/12/10 - TFS26732
			//    this.OnPropertyChanged(CellFormatValue.WrapText, options);
			//}

			#endregion // Refactored
			this.SetFormatProperty(value, CellFormatValue.WrapText, callBeforeAndAfterSet, options, Utilities.CellFormatWrapTextGetter, Utilities.CellFormatWrapTextSetter);
		}

		#endregion WrapText

		#endregion Public Properties

		#region Internal Properties

		// MD 10/21/10 - TFS34398
		#region FontInternal

		internal WorkbookFontOwnerAccessor FontInternal
		{
			get
			{
				if (this.fontAccessor == null)
					this.fontAccessor = new WorkbookFontOwnerAccessor(this);

				return this.fontAccessor;
			}
		} 

		#endregion // FontInternal

		// MD 3/2/12 - 12.1 - Table Support
		#region IsEmpty

		internal bool IsEmpty
		{
			get { return this.Element.IsEmpty; }
		}

		#endregion // IsEmpty

		// MD 4/18/11 - TFS62026
		#region Owner

		internal IWorksheetCellFormatProxyOwner Owner
		{
			get { return this.owner; }
		}

		#endregion  // Owner

		#endregion // Internal Properties

		#endregion Properties
	}

	// MD 5/12/10 - TFS26732
	internal interface IWorksheetCellFormatProxyOwner
	{
		// MD 3/22/12 - TFS104630
		WorksheetCellFormatData GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue);

		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//void OnCellFormatValueChanged(CellFormatValue value);
		// MD 4/12/11 - TFS67084
		// We need to pass along the sender now because some object own multiple cell formats.
		//void OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options);
		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		//void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options);
		void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options);

		// MD 2/29/12 - 12.1 - Table Support
		void OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values);

		// MD 2/29/12 - 12.1 - Table Support
		void VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions);

		// MD 2/29/12 - 12.1 - Table Support
		// This is no longer needed.
		//// MD 11/1/11 - TFS94534
		//bool CanOwnStyleFormat { get; }

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 7/26/10 - TFS34398
		//Workbook Workbook { get; }
	}

	// MD 10/21/10 - TFS34398
	[Flags]
	internal enum CellFormatValueChangedOptions
	{
		DefaultBehavior = 0,

		PreventAdjacentBorderSyncronization = 0x01,

		// MD 4/18/11 - TFS62026
		PreventCellToMergedRegionSyncronization = 0x02,
		PreventMergedRegionToCellSyncronization = 0x04,

		PreventAllSyncronization = -1,
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