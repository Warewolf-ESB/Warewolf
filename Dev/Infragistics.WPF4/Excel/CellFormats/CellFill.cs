using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.BIFF8;







using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/18/12 - 12.1 - Cell Format Updates



	/// <summary>
	/// Abstract base class for the fill of a cell.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.Fill"/>
	/// <seealso cref="CellFillPattern"/>
	/// <seealso cref="CellFillGradient"/>
	/// <seealso cref="CellFillLinearGradient"/>
	/// <seealso cref="CellFillRectangularGradient"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 abstract class CellFill
	{
		#region Static Members

		private static CellFill _noColor = new CellFillPattern(null, null, FillPatternStyle.None);

		#endregion // Static Members

		#region Methods

		#region Abstract Methods

		internal abstract void PopulateXFEXTProps(WorksheetCellFormatData format, List<ExtProp> xfextProps);
		internal abstract void PopulateXFProps(WorksheetCellFormatData format, List<XFProp> xfextProps);

		// MD 5/7/12 - TFS106831
		// We may need to resolve all WorkbookColorInfos in the fill.
		internal abstract CellFill ToResolvedColorFill(Workbook workbook);

		#endregion // Abstract Methods

		#region Public Methods

		#region CreateLinearGradientFill

		/// <summary>
		/// Creates a linear gradient that can be applied to a cell's fill.
		/// </summary>
		/// <param name="angle">
		/// The angle, in degrees, of the direction of the linear gradient, going clockwise from the left-to-right direction.
		/// </param>
		/// <param name="color1">The color at the start of the gradient.</param>
		/// <param name="color2">The color at the end of the gradient.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="color1"/> or <paramref name="color2"/> are the empty or system colors or have a non-opaque alpha channel.
		/// </exception>
		/// <seealso cref="CellFillLinearGradient"/>
		public static CellFillLinearGradient CreateLinearGradientFill(double angle, Color color1, Color color2)
		{
			return CellFill.CreateLinearGradientFill(angle, new WorkbookColorInfo(color1), new WorkbookColorInfo(color2));
		}

		/// <summary>
		/// Creates a linear gradient that can be applied to a cell's fill.
		/// </summary>
		/// <param name="angle">
		/// The angle, in degrees, of the direction of the linear gradient, going clockwise from the left-to-right direction.
		/// </param>
		/// <param name="colorInfo1">A <see cref="WorkbookColorInfo"/> which describes the color at the start of the gradient.</param>
		/// <param name="colorInfo2">A <see cref="WorkbookColorInfo"/> which describes the color at the end of the gradient.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="colorInfo1"/> or <paramref name="colorInfo2"/> is an automatic or a system color.
		/// </exception>
		/// <seealso cref="CellFillLinearGradient"/>
		public static CellFillLinearGradient CreateLinearGradientFill(double angle, WorkbookColorInfo colorInfo1, WorkbookColorInfo colorInfo2)
		{
			return CellFill.CreateLinearGradientFill(angle, new CellFillGradientStop(colorInfo1, 0), new CellFillGradientStop(colorInfo2, 1));
		}

		/// <summary>
		/// Creates a linear gradient that can be applied to a cell's fill.
		/// </summary>
		/// <param name="angle">
		/// The angle, in degrees, of the direction of the linear gradient, going clockwise from the left-to-right direction.
		/// </param>
		/// <param name="stops">
		/// Two or more gradient stops which describe the color transitions and their positions within the gradient.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="stops"/> contains less than two gradient stops. There must be at least two stops to define the gradient.
		/// </exception>
		/// <seealso cref="CellFillLinearGradient"/>
		public static CellFillLinearGradient CreateLinearGradientFill(double angle, params CellFillGradientStop[] stops)
		{
			return new CellFillLinearGradient(angle, stops);
		}

		#endregion // CreateLinearGradientFill

		#region CreatePatternFill

		/// <summary>
		/// Creates a solid color or pattern fill that can be applied to a cell.
		/// </summary>
		/// <param name="backgroundColor">
		/// The background color of the cell, which will only be seen if the <paramref name="patternStyle"/> is not None.
		/// </param>
		/// <param name="patternColor">
		/// The pattern color of the cell, which will only be seen if the <paramref name="patternStyle"/> is not None or Solid.
		/// </param>
		/// <param name="patternStyle">The fill pattern for the cell.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="backgroundColor"/> or <paramref name="patternColor"/> are the empty color or have a non-opaque alpha channel.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="patternStyle"/> is Default or not defined in the <see cref="FillPatternStyle"/> enumeration.
		/// </exception>
		/// <seealso cref="CellFillPattern"/>
		public static CellFillPattern CreatePatternFill(Color backgroundColor, Color patternColor, FillPatternStyle patternStyle)
		{
			return CellFill.CreatePatternFill(new WorkbookColorInfo(backgroundColor), new WorkbookColorInfo(patternColor), patternStyle);
		}

		/// <summary>
		/// Creates a solid color or pattern fill that can be applied to a cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="backgroundColorInfo"/> and <paramref name="patternColorInfo"/> can be specified as null to use the default colors.
		/// </p>
		/// </remarks>
		/// <param name="backgroundColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the background color of the cell, which will only be seen if the 
		/// <paramref name="patternStyle"/> is not None.
		/// </param>
		/// <param name="patternColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the pattern color of the cell, which will only be seen if the 
		/// <paramref name="patternStyle"/> is not None or Solid.
		/// </param>
		/// <param name="patternStyle">The fill pattern for the cell.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="patternStyle"/> is Default or not defined in the <see cref="FillPatternStyle"/> enumeration.
		/// </exception>
		/// <seealso cref="CellFillPattern"/>
		public static CellFillPattern CreatePatternFill(WorkbookColorInfo backgroundColorInfo, WorkbookColorInfo patternColorInfo, FillPatternStyle patternStyle)
		{
			return new CellFillPattern(backgroundColorInfo, patternColorInfo, patternStyle);
		}

		#endregion // CreatePatternFill

		#region CreateRectangularGradientFill

		/// <summary>
		/// Creates a rectangular gradient that can be applied to a cell's fill.
		/// </summary>
		/// <param name="color1">The color at the inner rectangle (cell center) of the gradient.</param>
		/// <param name="color2">The color at the outer rectangle (cell edges) of the gradient.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="color1"/> or <paramref name="color2"/> are empty or system colors or have a non-opaque alpha channel.
		/// </exception>
		/// <seealso cref="CellFillRectangularGradient"/>
		public static CellFillRectangularGradient CreateRectangularGradientFill(Color color1, Color color2)
		{
			return CellFill.CreateRectangularGradientFill(0.5, 0.5, 0.5, 0.5, color1, color2);
		}

		/// <summary>
		/// Creates a rectangular gradient that can be applied to a cell's fill.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The rectangular gradient is defined by specifying an inner rectangle and a set of gradient stops. The gradient goes from the 
		/// edges of the inner rectangle to the edges of the cell. If the inner rectangle does not have a height or width of 0, the color
		/// of the first gradient stop will be filled in the center of the inner rectangle.
		/// </p>
		/// <p class="body">
		/// The inner rectangle is defined by the <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and 
		/// <paramref name="bottom"/> parameters. They are relative values ranging from 0.0 to 1.0 and they go from the top/left to the
		/// bottom/right. So, for example, to specify a gradient that goes out from the center, all values would be 0.5. Or to specify a
		/// gradient which goes out from the bottom-left corner of the cell, the following values would be used: left = 0.0, top = 1.0, 
		/// right = 0.0, bottom = 1.0.
		/// </p>
		/// </remarks>
		/// <param name="left">
		/// The left edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="top">
		/// The top edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="right">
		/// The right edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="bottom">
		/// The bottom edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="color1">The color at the inner rectangle of the gradient.</param>
		/// <param name="color2">The color at the outer rectangle (cell edges) of the gradient.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, or <paramref name="bottom"/> are less than 0.0 or 
		/// greater than 1.0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="color1"/> or <paramref name="color2"/> are empty or system colors or have a non-opaque alpha channel.
		/// </exception>
		/// <seealso cref="CellFillRectangularGradient"/>
		public static CellFillRectangularGradient CreateRectangularGradientFill(double left, double top, double right, double bottom, Color color1, Color color2)
		{
			return CellFill.CreateRectangularGradientFill(left, top, right, bottom, new WorkbookColorInfo(color1), new WorkbookColorInfo(color2));
		}

		/// <summary>
		/// Creates a rectangular gradient that can be applied to a cell's fill.
		/// </summary>
		/// <param name="colorInfo1">
		/// A <see cref="WorkbookColorInfo"/> which describes the color at the inner rectangle (cell center) of the gradient.
		/// </param>
		/// <param name="colorInfo2">
		/// A <see cref="WorkbookColorInfo"/> which describes the color at the outer rectangle (cell edges) of the gradient.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="colorInfo1"/> or <paramref name="colorInfo2"/> is an automatic or a system color.
		/// </exception>
		/// <seealso cref="CellFillRectangularGradient"/>
		public static CellFillRectangularGradient CreateRectangularGradientFill(WorkbookColorInfo colorInfo1, WorkbookColorInfo colorInfo2)
		{
			return CellFill.CreateRectangularGradientFill(0.5, 0.5, 0.5, 0.5, colorInfo1, colorInfo2);
		}

		/// <summary>
		/// Creates a rectangular gradient that can be applied to a cell's fill.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The rectangular gradient is defined by specifying an inner rectangle and a set of gradient stops. The gradient goes from the 
		/// edges of the inner rectangle to the edges of the cell. If the inner rectangle does not have a height or width of 0, the color
		/// of the first gradient stop will be filled in the center of the inner rectangle.
		/// </p>
		/// <p class="body">
		/// The inner rectangle is defined by the <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and 
		/// <paramref name="bottom"/> parameters. They are relative values ranging from 0.0 to 1.0 and they go from the top/left to the
		/// bottom/right. So, for example, to specify a gradient that goes out from the center, all values would be 0.5. Or to specify a
		/// gradient which goes out from the bottom-left corner of the cell, the following values would be used: left = 0.0, top = 1.0, 
		/// right = 0.0, bottom = 1.0.
		/// </p>
		/// </remarks>
		/// <param name="left">
		/// The left edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="top">
		/// The top edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="right">
		/// The right edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="bottom">
		/// The bottom edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="colorInfo1">
		/// A <see cref="WorkbookColorInfo"/> which describes the color at the inner rectangle of the gradient.
		/// </param>
		/// <param name="colorInfo2">
		/// A <see cref="WorkbookColorInfo"/> which describes the color at the outer rectangle (cell edges) of the gradient.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="colorInfo1"/> or <paramref name="colorInfo2"/> is an automatic or a system color.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, or <paramref name="bottom"/> are less than 0.0 or 
		/// greater than 1.0.
		/// </exception>
		/// <seealso cref="CellFillRectangularGradient"/>
		public static CellFillRectangularGradient CreateRectangularGradientFill(double left, double top, double right, double bottom, WorkbookColorInfo colorInfo1, WorkbookColorInfo colorInfo2)
		{
			return CellFill.CreateRectangularGradientFill(left, top, right, bottom, new CellFillGradientStop(colorInfo1, 0), new CellFillGradientStop(colorInfo2, 1));
		}

		/// <summary>
		/// Creates a rectangular gradient that can be applied to a cell's fill.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The rectangular gradient is defined by specifying an inner rectangle and a set of gradient stops. The gradient goes from the 
		/// edges of the inner rectangle to the edges of the cell. If the inner rectangle does not have a height or width of 0, the color
		/// of the first gradient stop will be filled in the center of the inner rectangle.
		/// </p>
		/// <p class="body">
		/// The inner rectangle is defined by the <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and 
		/// <paramref name="bottom"/> parameters. They are relative values ranging from 0.0 to 1.0 and they go from the top/left to the
		/// bottom/right. So, for example, to specify a gradient that goes out from the center, all values would be 0.5. Or to specify a
		/// gradient which goes out from the bottom-left corner of the cell, the following values would be used: left = 0.0, top = 1.0, 
		/// right = 0.0, bottom = 1.0.
		/// </p>
		/// </remarks>
		/// <param name="left">
		/// The left edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="top">
		/// The top edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="right">
		/// The right edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="bottom">
		/// The bottom edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="stops">
		/// Two or more gradient stops which describe the color transitions and their positions within the gradient.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, or <paramref name="bottom"/> are less than 0.0 or 
		/// greater than 1.0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="stops"/> contains less than two gradient stops. There must be at least two stops to define the gradient.
		/// </exception>
		/// <seealso cref="CellFillRectangularGradient"/>
		public static CellFillRectangularGradient CreateRectangularGradientFill(double left, double top, double right, double bottom, params CellFillGradientStop[] stops)
		{
			return new CellFillRectangularGradient(left, top, right, bottom, stops);
		}

		#endregion // CreateRectangularGradientFill

		#region CreateSolidFill

		/// <summary>
		/// Creates a solid color fill that can be applied to a cell.
		/// </summary>
		/// <param name="solidColor">The solid color of the fill.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="solidColor"/> is the empty color or has a non-opaque alpha channel.
		/// </exception>
		/// <seealso cref="CellFillPattern"/>
		public static CellFillPattern CreateSolidFill(Color solidColor)
		{
			return CellFill.CreateSolidFill(new WorkbookColorInfo(solidColor));
		}

		/// <summary>
		/// Creates a solid color fill that can be applied to a cell.
		/// </summary>
		/// <param name="solidColorInfo">A <see cref="WorkbookColorInfo"/> which describes the solid color of the fill.</param>
		/// <seealso cref="CellFillPattern"/>
		public static CellFillPattern CreateSolidFill(WorkbookColorInfo solidColorInfo)
		{
			return new CellFillPattern(solidColorInfo, null, FillPatternStyle.Solid);
		}

		#endregion // CreateSolidFill

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region NoColor

		/// <summary>
		/// Gets the default cell fill, which is no background color.
		/// </summary>
		public static CellFill NoColor
		{
			get { return _noColor; }
		}

		#endregion // NoColor

		#endregion // Properties
	}




	/// <summary>
	/// An immutable object which represents a solid or pattern fill for a cell.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.Fill"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 sealed class CellFillPattern : CellFill
	{
		#region Member Variables

		private readonly WorkbookColorInfo _backgroundColorInfo;
		private readonly WorkbookColorInfo _patternColorInfo;
		private readonly FillPatternStyle _patternStyle;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CellFillPattern"/> instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="backgroundColorInfo"/> and <paramref name="patternColorInfo"/> can be specified as null to use the default colors.
		/// </p>
		/// </remarks>
		/// <param name="backgroundColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the background color of the cell, which will only be seen if the 
		/// <paramref name="patternStyle"/> is not None.
		/// </param>
		/// <param name="patternColorInfo">
		/// A <see cref="WorkbookColorInfo"/> which describes the pattern color of the cell, which will only be seen if the 
		/// <paramref name="patternStyle"/> is not None or Solid.
		/// </param>
		/// <param name="patternStyle">The fill pattern for the cell.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="patternStyle"/> is Default or not defined in the <see cref="FillPatternStyle"/> enumeration.
		/// </exception>
		public CellFillPattern(WorkbookColorInfo backgroundColorInfo, WorkbookColorInfo patternColorInfo, FillPatternStyle patternStyle)
			: this(backgroundColorInfo, patternColorInfo, patternStyle, null) { }

		internal CellFillPattern(WorkbookColorInfo backgroundColorInfo, WorkbookColorInfo patternColorInfo, FillPatternStyle patternStyle, WorksheetCellFormatData owningFormat)
		{
			if (patternStyle == FillPatternStyle.Solid &&
				owningFormat != null &&
				owningFormat.DoesReverseColorsForSolidFill)
			{
				Utilities.SwapValues(ref backgroundColorInfo, ref patternColorInfo);
			}

			Utilities.VerifyEnumValue(patternStyle, "patternStyle");

#pragma warning disable 0618
			if (patternStyle == FillPatternStyle.Default)
#pragma warning restore 0618
			{
				throw new InvalidEnumArgumentException(SR.GetString("LE_InvalidEnumArgumentException_DefaultPatternCannotBeUsed"));
			}

			if (backgroundColorInfo == null)
				backgroundColorInfo = new WorkbookColorInfo(Utilities.SystemColorsInternal.WindowColor);

			if (patternColorInfo == null)
				patternColorInfo = WorkbookColorInfo.Automatic;

			_backgroundColorInfo = backgroundColorInfo;
			_patternStyle = patternStyle;
			_patternColorInfo = patternColorInfo;
		}

		internal CellFillPattern(int backgroundColorIndex, int foregroundColorIndex, FillPatternStyle patternStyle, WorksheetCellFormatData owningFormat)
			: this(
			new WorkbookColorInfo(owningFormat.Workbook, backgroundColorIndex),
			new WorkbookColorInfo(owningFormat.Workbook, foregroundColorIndex),
			patternStyle,
			owningFormat)
		{
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CellFillPattern"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CellFillPattern other = obj as CellFillPattern;
			if (other == null)
				return false;

			if (_patternStyle != other._patternStyle)
				return false;

			if (_backgroundColorInfo != other._backgroundColorInfo)
				return false;

			if (_patternColorInfo != other._patternColorInfo)
				return false;

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CellFillPattern"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			int hashCode = _backgroundColorInfo.GetHashCode();
			hashCode ^= _patternColorInfo.GetHashCode();
			hashCode ^= (int)_patternStyle;
			return hashCode;
		}

		#endregion // GetHashCode

		#region PopulateXFEXTProps

		internal override void PopulateXFEXTProps(WorksheetCellFormatData format, List<ExtProp> xfextProps)
		{
			WorkbookColorInfo backgroundColor = this.GetFileFormatBackgroundColorInfo(format);
			WorkbookColorInfo foregroundColor = this.GetFileFormatForegroundColorInfo(format);

			switch (this.PatternStyle)
			{
				case FillPatternStyle.None:
					break;

				case FillPatternStyle.Solid:
					xfextProps.Add(new ExtPropColor(foregroundColor, ExtPropType.ForegroundColor));
					format.TryAddXFEXTColorInfo(backgroundColor, ExtPropType.BackgroundColor, xfextProps);
					break;

				default:
					xfextProps.Add(new ExtPropColor(backgroundColor, ExtPropType.BackgroundColor));
					format.TryAddXFEXTColorInfo(foregroundColor, ExtPropType.ForegroundColor, xfextProps);
					break;
			}
		}

		#endregion // PopulateXFEXTProps

		#region PopulateXFProps

		internal override void PopulateXFProps(WorksheetCellFormatData format, List<XFProp> xfextProps)
		{
			xfextProps.Add(new XFPropFillPattern(this.PatternStyle));

			if (format.Type == WorksheetCellFormatType.DifferentialFormat)
			{
				xfextProps.Add(new XFPropColor(XFPropType.ForegroundColor, this.GetFileFormatForegroundColorInfo(format)));
				xfextProps.Add(new XFPropColor(XFPropType.BackgroundColor, this.GetFileFormatBackgroundColorInfo(format)));
			}
			else
			{
				WorkbookColorInfo backgroundColor = this.GetFileFormatBackgroundColorInfo(format);
				WorkbookColorInfo foregroundColor = this.GetFileFormatForegroundColorInfo(format);
				switch (this.PatternStyle)
				{
					case FillPatternStyle.None:
						break;

					case FillPatternStyle.Solid:
						xfextProps.Add(new XFPropColor(XFPropType.ForegroundColor, foregroundColor));
						format.TryAddXFPropColorInfo(backgroundColor, XFPropType.BackgroundColor, xfextProps);
						break;

					default:
						xfextProps.Add(new XFPropColor(XFPropType.BackgroundColor, backgroundColor));
						format.TryAddXFPropColorInfo(foregroundColor, XFPropType.ForegroundColor, xfextProps);
						break;
				}
			}
		}

		#endregion // PopulateXFProps

		// MD 5/7/12 - TFS106831
		// We may need to resolve all WorkbookColorInfos in the fill.
		#region ToResolvedColorFill

		internal override CellFill ToResolvedColorFill(Workbook workbook)
		{
			if (_backgroundColorInfo.IsResolved && _patternColorInfo.IsResolved)
				return this;

			return new CellFillPattern(
				_backgroundColorInfo.ToResolved(workbook), 
				_patternColorInfo.ToResolved(workbook), 
				_patternStyle);
		}

		#endregion // ToResolvedColorFill

		#endregion // Base Class Overrides

		#region Methods

		#region GetFileFormatBackgroundColorInfo

		internal WorkbookColorInfo GetFileFormatBackgroundColorInfo(WorksheetCellFormatData owningFormat)
		{
			if (owningFormat.DoesReverseColorsForSolidFill && _patternStyle == FillPatternStyle.Solid)
				return _patternColorInfo;

			return _backgroundColorInfo;
		}

		#endregion // GetFileFormatBackgroundColorInfo

		#region GetFileFormatForegroundColorInfo

		internal WorkbookColorInfo GetFileFormatForegroundColorInfo(WorksheetCellFormatData owningFormat)
		{
			if (owningFormat.DoesReverseColorsForSolidFill && _patternStyle == FillPatternStyle.Solid)
				return _backgroundColorInfo;

			return _patternColorInfo;
		}

		#endregion // GetFileFormatForegroundColorInfo

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region BackgroundColorInfo

		/// <summary>
		/// Gets the <see cref="WorkbookColorInfo"/> which describes the background color of the cell, which will only be seen if the 
		/// <see cref="PatternStyle"/> is not None.
		/// </summary>
		public WorkbookColorInfo BackgroundColorInfo
		{
			get { return _backgroundColorInfo; }
		}

		#endregion // BackgroundColorInfo

		#region Pattern

		/// <summary>
		/// Gets the fill pattern for the cell.
		/// </summary>
		public FillPatternStyle PatternStyle
		{
			get { return _patternStyle; }
		}

		#endregion // Pattern

		#region PatternColorInfo

		/// <summary>
		/// Gets the <see cref="WorkbookColorInfo"/> which describes the pattern color of the cell, which will only be seen if the 
		/// <see cref="PatternStyle"/> is not None or Solid.
		/// </summary>
		public WorkbookColorInfo PatternColorInfo
		{
			get { return _patternColorInfo; }
		}

		#endregion // PatternColorInfo

		#endregion // Public Properties

		#endregion // Properties
	}




	/// <summary>
	/// Abstract base class for a gradient fill of a cell.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.Fill"/>
	/// <seealso cref="CellFillLinearGradient"/>
	/// <seealso cref="CellFillRectangularGradient"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 abstract class CellFillGradient : CellFill
	{
		#region Member Variables

		private readonly ReadOnlyCollection<CellFillGradientStop> _stops;

		#endregion // Member Variables

		#region Constructor

		internal CellFillGradient(CellFillGradientStop[] stops)
		{
			if (stops == null || stops.Length < 2)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_LessThanTwoGradientStops"), "stops");

			_stops = new ReadOnlyCollection<CellFillGradientStop>(stops);
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CellFillGradient"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			CellFillGradient other = obj as CellFillGradient;
			if (other == null)
				return false;

			if (_stops.Count != other._stops.Count)
				return false;

			for (int i = 0; i < _stops.Count; i++)
			{
				if (_stops[i].Equals(other._stops[i]) == false)
					return false;
			}

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CellFillGradient"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return
				_stops.Count ^
				_stops[0].GetHashCode() ^
				_stops[_stops.Count - 1].GetHashCode();
		}

		#endregion // GetHashCode

		#region PopulateXFEXTProps

		internal override void PopulateXFEXTProps(WorksheetCellFormatData format, List<ExtProp> xfextProps)
		{
			xfextProps.Add(new ExtPropGradientFill(this));
		}

		#endregion // PopulateXFEXTProps

		#endregion // Base Class Overrides

		#region Methods

		// MD 5/7/12 - TFS106831
		// We may need to resolve all WorkbookColorInfos in the fill.
		#region GetResolvedStops

		internal CellFillGradientStop[] GetResolvedStops(Workbook workbook, out bool allStopsResolved)
		{
			CellFillGradientStop[] resolvedStops = new CellFillGradientStop[this.Stops.Count];

			allStopsResolved = true;
			for (int i = 0; i < this.Stops.Count; i++)
			{
				CellFillGradientStop stop = this.Stops[i];
				resolvedStops[i] = stop.ToResolvedColorStop(workbook);

				allStopsResolved &= stop.ColorInfo.IsResolved;
			}

			return resolvedStops;
		}

		#endregion // GetResolvedStops

		#region PopulateStopXFProps

		internal void PopulateStopXFProps(List<XFProp> xfextProps)
		{
			for (int i = 0; i < this.Stops.Count; i++)
			{
				CellFillGradientStop stop = this.Stops[i];
				xfextProps.Add(new XFPropGradientStop(stop.ColorInfo, stop.Offset));
			}
		}

		#endregion // PopulateStopXFProps

		#endregion // Methods

		#region Properties

		#region Stops

		/// <summary>
		/// Gets the read-only collection of gradient stops which describe the color transitions and their positions within the gradient.
		/// </summary>
		public IList<CellFillGradientStop> Stops
		{
			get { return _stops; }
		}

		#endregion // Stops

		#endregion // Properties
	}




	/// <summary>
	/// An immutable object which represents a linear gradient fill for a cell.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.Fill"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 sealed class CellFillLinearGradient : CellFillGradient
	{
		#region Member Variables

		private readonly double _angle;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CellFillLinearGradient"/> instance.
		/// </summary>
		/// <param name="angle">
		/// The angle, in degrees, of the direction of the linear gradient, going clockwise from the left-to-right direction.
		/// </param>
		/// <param name="stops">
		/// Two or more gradient stops which describe the color transitions and their positions within the gradient.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="stops"/> contains less than two gradient stops. There must be at least two stops to define the gradient.
		/// </exception>
		public CellFillLinearGradient(double angle, params CellFillGradientStop[] stops)
			: base(stops)
		{
			_angle = angle;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CellFillLinearGradient"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CellFillLinearGradient other = obj as CellFillLinearGradient;
			if (other == null)
				return false;

			if (_angle != other._angle)
				return false;

			if (base.Equals(other) == false)
				return false;

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CellFillLinearGradient"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return
				_angle.GetHashCode() ^
				base.GetHashCode();
		}

		#endregion // GetHashCode

		#region PopulateXFProps

		internal override void PopulateXFProps(WorksheetCellFormatData format, List<XFProp> xfextProps)
		{
			xfextProps.Add(new XFPropGradientFill(false, this.Angle, 0, 0, 0, 0));
			this.PopulateStopXFProps(xfextProps);
		}

		#endregion // PopulateXFProps

		// MD 5/7/12 - TFS106831
		// We may need to resolve all WorkbookColorInfos in the fill.
		#region ToResolvedColorFill

		internal override CellFill ToResolvedColorFill(Workbook workbook)
		{
			bool allStopsResolved;
			CellFillGradientStop[] resolvedStops = this.GetResolvedStops(workbook, out allStopsResolved);

			if (allStopsResolved)
				return this;

			return new CellFillLinearGradient(_angle, resolvedStops);
		}

		#endregion // ToResolvedColorFill

		#endregion // Base Class Overrides

		#region Properties

		#region Angle

		/// <summary>
		/// Gets the angle, in degrees, of the direction of the linear gradient, going clockwise from the left-to-right direction.
		/// </summary>
		public double Angle
		{
			get { return _angle; }
		}

		#endregion // Angle

		#endregion // Properties
	}




	/// <summary>
	/// An immutable object which represents a rectangular gradient fill for a cell.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.Fill"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 sealed class CellFillRectangularGradient : CellFillGradient
	{
		#region Member Variables

		private readonly double _bottom;
		private readonly double _left;
		private readonly double _right;
		private readonly double _top;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CellFillRectangularGradient"/> instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The rectangular gradient is defined by specifying an inner rectangle and a set of gradient stops. The gradient goes from the 
		/// edges of the inner rectangle to the edges of the cell. If the inner rectangle does not have a height or width of 0, the color
		/// of the first gradient stop will be filled in the center of the inner rectangle.
		/// </p>
		/// <p class="body">
		/// The inner rectangle is defined by the <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, and 
		/// <paramref name="bottom"/> parameters. They are relative values ranging from 0.0 to 1.0 and they go from the top/left to the
		/// bottom/right. So, for example, to specify a gradient that goes out from the center, all values would be 0.5. Or to specify a
		/// gradient which goes out from the bottom-left corner of the cell, the following values would be used: left = 0.0, top = 1.0, 
		/// right = 0.0, bottom = 1.0.
		/// </p>
		/// </remarks>
		/// <param name="left">
		/// The left edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="top">
		/// The top edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="right">
		/// The right edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </param>
		/// <param name="bottom">
		/// The bottom edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </param>
		/// <param name="stops">
		/// Two or more gradient stops which describe the color transitions and their positions within the gradient.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="left"/>, <paramref name="top"/>, <paramref name="right"/>, or <paramref name="bottom"/> are less than 0.0 or 
		/// greater than 1.0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="stops"/> contains less than two gradient stops. There must be at least two stops to define the gradient.
		/// </exception>
		public CellFillRectangularGradient(double left, double top, double right, double bottom, params CellFillGradientStop[] stops)
			: base(stops)
		{
			CellFillRectangularGradient.ValidateInnerRectangleValue(left, "left");
			CellFillRectangularGradient.ValidateInnerRectangleValue(top, "top");
			CellFillRectangularGradient.ValidateInnerRectangleValue(right, "right");
			CellFillRectangularGradient.ValidateInnerRectangleValue(bottom, "bottom");

			_bottom = bottom;
			_left = left;
			_right = right;
			_top = top;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CellFillRectangularGradient"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CellFillRectangularGradient other = obj as CellFillRectangularGradient;
			if (other == null)
				return false;

			if (_bottom != other._bottom)
				return false;

			if (_left != other._left)
				return false;

			if (_right != other._right)
				return false;

			if (_top != other._top)
				return false;

			if (base.Equals(other) == false)
				return false;

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CellFillRectangularGradient"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return
				_bottom.GetHashCode() ^
				_left.GetHashCode() ^
				_right.GetHashCode() ^
				_top.GetHashCode() ^
				base.GetHashCode();
		}

		#endregion // GetHashCode

		#region PopulateXFProps

		internal override void PopulateXFProps(WorksheetCellFormatData format, List<XFProp> xfextProps)
		{
			xfextProps.Add(new XFPropGradientFill(true, 0, this.Left, this.Right, this.Top, this.Bottom));
			this.PopulateStopXFProps(xfextProps);
		}

		#endregion // PopulateXFProps

		// MD 5/7/12 - TFS106831
		// We may need to resolve all WorkbookColorInfos in the fill.
		#region ToResolvedColorFill

		internal override CellFill ToResolvedColorFill(Workbook workbook)
		{
			bool allStopsResolved;
			CellFillGradientStop[] resolvedStops = this.GetResolvedStops(workbook, out allStopsResolved);

			if (allStopsResolved)
				return this;

			return new CellFillRectangularGradient(_left, _top, _right, _bottom, resolvedStops);
		}

		#endregion // ToResolvedColorFill

		#endregion // Base Class Overrides

		#region Methods

		#region ValidateInnerRectangleValue

		private static void ValidateInnerRectangleValue(double value, string paramName)
		{
			if (value < 0 || 1 < value)
				throw new ArgumentOutOfRangeException(paramName, value, SR.GetString("LE_ArgumentOutOfRangeException_InvalidRelativeRectangleValueForGradient"));
		}

		#endregion // ValidateInnerRectangleValue

		#endregion // Methods

		#region Properties

		#region Bottom

		/// <summary>
		/// Gets the bottom edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </summary>
		public double Bottom
		{
			get { return _bottom; }
		}

		#endregion // Bottom

		#region Left

		/// <summary>
		/// Gets the left edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </summary>
		public double Left
		{
			get { return _left; }
		}

		#endregion // Left

		#region Right

		/// <summary>
		/// Gets the right edge of the inner rectangle of the gradient, ranging from 0.0 (the left of the cell) to 1.0 (the right of the cell).
		/// </summary>
		public double Right
		{
			get { return _right; }
		}

		#endregion // Right

		#region Top

		/// <summary>
		/// Gets the top edge of the inner rectangle of the gradient, ranging from 0.0 (the top of the cell) to 1.0 (the bottom of the cell).
		/// </summary>
		public double Top
		{
			get { return _top; }
		}

		#endregion // Top

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