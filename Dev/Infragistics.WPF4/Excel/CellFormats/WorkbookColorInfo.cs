using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;







using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/18/12 - 12.1 - Cell Format Updates



	/// <summary>
	/// An immutable object which represents a color in a Microsoft Excel workbook.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.BottomBorderColorInfo"/>
	/// <seealso cref="IWorksheetCellFormat.DiagonalBorderColorInfo"/>
	/// <seealso cref="IWorksheetCellFormat.LeftBorderColorInfo"/>
	/// <seealso cref="IWorksheetCellFormat.RightBorderColorInfo"/>
	/// <seealso cref="IWorksheetCellFormat.TopBorderColorInfo"/>
	/// <seealso cref="IWorkbookFont.ColorInfo"/>
	/// <seealso cref="CellFillPattern.BackgroundColorInfo"/>
	/// <seealso cref="CellFillPattern.PatternColorInfo"/>
	/// <seealso cref="CellFillGradientStop.ColorInfo"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 sealed class WorkbookColorInfo
	{
		#region Static Variables

		private static readonly WorkbookColorInfo _automatic = new WorkbookColorInfo();

		#endregion // Static Variables

		#region Member Variables

		private readonly Color _color;
		private readonly ColorInfoState _state;
		private readonly byte _themeColorType;
		private readonly double _tint;

		#endregion // Member Variables

		#region Constructor

		private WorkbookColorInfo()
		{
			_state = ColorInfoState.IsAutomatic;
		}

		/// <summary>
		/// Creates a new <see cref="WorkbookColorInfo"/> with the specified Color.
		/// </summary>
		/// <param name="color">The color which should be displayed when the WorkbookColorInfo is used.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="color"/> is the empty Color or has a non-opaque alpha channel.
		/// </exception>
		public WorkbookColorInfo(Color color)
			: this(color, null, null, false) { }

		/// <summary>
		/// Creates a new <see cref="WorkbookColorInfo"/> with the specified Color and tint.
		/// </summary>
		/// <param name="color">The base color which should be displayed when the WorkbookColorInfo is used.</param>
		/// <param name="tint">The tint to apply to the base color, from -1.0 (100% darken) to 1.0 (100% lighten).</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="color"/> is the empty Color or has a non-opaque alpha channel.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="tint"/> is outside the range of -1.0 to 1.0, inclusive.
		/// </exception>
		public WorkbookColorInfo(Color color, double tint)
			: this(color, null, tint, false) { }

		/// <summary>
		/// Creates a new <see cref="WorkbookColorInfo"/> with the specified theme color.
		/// </summary>
		/// <param name="themeColorType">The type of theme color which should be displayed when the WorkbookColorInfo is used.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="themeColorType"/> is not defined in the <see cref="WorkbookThemeColorType"/> enumeration.
		/// </exception>
		public WorkbookColorInfo(WorkbookThemeColorType themeColorType)
			: this(null, themeColorType, null, false) { }

		/// <summary>
		/// Creates a new <see cref="WorkbookColorInfo"/> with the specified theme color and tint.
		/// </summary>
		/// <param name="themeColorType">The type of theme color which should be the base color when the WorkbookColorInfo is used.</param>
		/// <param name="tint">The tint to apply to the base color, from -1.0 (100% darken) to 1.0 (100% lighten).</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="themeColorType"/> is not defined in the <see cref="WorkbookThemeColorType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="tint"/> is outside the range of -1.0 to 1.0, inclusive.
		/// </exception>
		public WorkbookColorInfo(WorkbookThemeColorType themeColorType, double tint)
			: this(null, themeColorType, tint, false) { }

		internal WorkbookColorInfo(Color? color, WorkbookThemeColorType? themeColorType, double? tint, bool isLoading)
			// MD 5/7/12 - TFS106831
			// Moved all code to the new constructor
			: this(color, themeColorType, tint, isLoading, false) { }

		// MD 5/7/12 - TFS106831
		// Added a new constructor to take a preventUsingAutomaticColor parameter.
		internal WorkbookColorInfo(Color? color, WorkbookThemeColorType? themeColorType, double? tint, bool isLoading, bool preventUsingAutomaticColor)
		{
			// MD 5/7/12 - TFS106831
			// If the preventUsingAutomaticColor parameter is True, don't use the automatic bit.
			//if (color == Utilities.SystemColorsInternal.WindowTextColor && themeColorType == null && tint == null)
			if (preventUsingAutomaticColor == false &&
				color == Utilities.SystemColorsInternal.WindowTextColor &&
				themeColorType == null &&
				tint == null)
			{
				_state = ColorInfoState.IsAutomatic;
				return;
			}

			if (color.HasValue)
			{
				_state = ColorInfoState.IsRGB;
				_color = color.Value;
				WorkbookColorInfo.ValidateColor(_color);
			}

			if (themeColorType.HasValue)
			{
				_state = ColorInfoState.IsThemeColor;
				WorkbookColorInfo.ValidateThemeColor(themeColorType.Value);
				_themeColorType = (byte)themeColorType.Value;
			}

			if (tint.HasValue)
			{
				_state |= ColorInfoState.IsTintValid;
				_tint = tint.Value;
				WorkbookColorInfo.ValidateTint(ref _tint, isLoading);
			}
		}

		internal WorkbookColorInfo(Workbook workbook, int index)
			: this(workbook.Palette.GetColorAtAbsoluteIndex(index)) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="WorkbookColorInfo"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			WorkbookColorInfo other = obj as WorkbookColorInfo;
			if (other == null)
				return false;

			return this == other;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="WorkbookColorInfo"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			int temp = 0;
			temp ^= _state.GetHashCode();
			temp ^= _color.GetHashCode();
			temp ^= _themeColorType.GetHashCode();
			temp ^= _tint.GetHashCode();
			return temp;
		}

		#endregion // GetHashCode

		#region ToString

		/// <summary>
		/// Gets the string representation of the <see cref="WorkbookColorInfo"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this.IsAutomatic)
				return SR.GetString("WorkbookColorInfo_Description", SR.GetString("WorkbookColorInfo_Automatic_Description"));

			string colorName;
			if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsRGB))
			{
				colorName = _color.ToString();
			}
			else if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsThemeColor))
			{
				colorName = ((WorkbookThemeColorType)_themeColorType).ToString();
			}
			else
			{
				Utilities.DebugFail("This was unexpected.");
				return base.ToString();
			}

			if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsTintValid))
				return SR.GetString("WorkbookColorInfo_WithTint_Description", colorName, _tint);

			return SR.GetString("WorkbookColorInfo_Description", colorName);
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Operators

		#region Operator ==

		/// <summary>
		/// Determines whether two <see cref="WorkbookColorInfo"/> instances are equal.
		/// </summary>
		/// <param name="a">The first WorkbookColorInfo instance.</param>
		/// <param name="b">The second WorkbookColorInfo instance.</param>
		/// <returns>True if the WorkbookColorInfo instances are equal; False otherwise.</returns>
		public static bool operator ==(WorkbookColorInfo a, WorkbookColorInfo b)
		{
			if (Object.ReferenceEquals(a, b))
				return true;

			if (null == (object)a || null == (object)b)
				return false;

			if (a._state != b._state)
				return false;

			if (a._themeColorType != b._themeColorType)
				return false;

			if (a._tint != b._tint)
				return false;

			if (WorkbookColorPalette.AreEquivalent(a._color, b._color) == false)
				return false;

			return true;
		}

		#endregion // Operator ==

		#region Operator !=

		/// <summary>
		/// Determines whether two <see cref="WorkbookColorInfo"/> instances are unequal.
		/// </summary>
		/// <param name="a">The first WorkbookColorInfo instance.</param>
		/// <param name="b">The second WorkbookColorInfo instance.</param>
		/// <returns>True if the WorkbookColorInfo instances are unequal; False otherwise.</returns>
		public static bool operator !=(WorkbookColorInfo a, WorkbookColorInfo b)
		{
			return !(a == b);
		}

		#endregion // Operator !=

		#endregion // Operators

		#region Methods

		#region Public Methods

		#region GetResolvedColor

		/// <summary>
		/// Gets the actual color which will be seen in Microsoft Excel if the <see cref="WorkbookColorInfo"/> is used.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// The <see cref="ThemeColorType"/> is not null. When the ThemeColorType is set, the <see cref="GetResolvedColor(Workbook)"/> method must be called with 
		/// a non-null <see cref="Workbook"/>.
		/// </exception>
		/// <returns>
		/// A Color which combines the <see cref="Color"/> and <see cref="Tint"/> if it is set.
		/// </returns>
		public Color GetResolvedColor()
		{
			return this.GetResolvedColor(null);
		}

		/// <summary>
		/// Gets the actual color which will be seen in Microsoft Excel if the <see cref="WorkbookColorInfo"/> is used.
		/// </summary>
		/// <param name="workbook">The workbook in which the WorkbookColorInfo is used.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="workbook"/> is null and <see cref="ThemeColorType"/> is not null. When the ThemeColorType is set, the method must be called with a 
		/// non-null <see cref="Workbook"/>.
		/// </exception>
		/// <returns>
		/// A Color which combines the <see cref="Color"/>, <see cref="ThemeColorType"/>, and/or <see cref="Tint"/>, depending on what is set.
		/// </returns>
		public Color GetResolvedColor(Workbook workbook)
		{
			if (this.IsAutomatic)
				return Utilities.SystemColorsInternal.WindowTextColor;

			Color baseColor;
			if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsRGB))
			{
				baseColor = _color;
			}
			else if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsThemeColor))
			{
				if (workbook == null)
					throw new ArgumentNullException("workbook", SR.GetString("LE_ArgumentNullException_WorkbookRequiredToResolveThemeColor"));

				baseColor = workbook.ThemeColors[(int)_themeColorType];
			}
			else
			{
				Utilities.DebugFail("This was unexpected.");
				return Utilities.ColorEmpty;
			}

			if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsTintValid))
				return Utilities.ApplyTint(baseColor, _tint);

			return baseColor;
		}

		#endregion // GetResolvedColor

		#endregion // Public Methods

		#region Internal Methods

		#region GetIndex

		internal int GetIndex(Workbook workbook, ColorableItem item)
		{
			return workbook.Palette.FindIndex(this.GetResolvedColor(workbook), item);
		}

		#endregion // GetIndex

		#region IsSupportedIn2003

		internal bool IsSupportedIn2003(Workbook workbook, ColorableItem item)
		{
			if (this.IsAutomatic)
				return true;

			if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsThemeColor) ||
				WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsTintValid))
			{
				return false;
			}

			Debug.Assert(WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsRGB), "This is unexpected.");
			Color indexedColor = workbook.Palette.GetColorAtAbsoluteIndex(this.GetIndex(workbook, item));
			return WorkbookColorPalette.AreEquivalent(_color, indexedColor);
		}

		#endregion // IsSupportedIn2003

		// MD 5/7/12 - TFS106831
		#region ToResolved

		internal WorkbookColorInfo ToResolved(Workbook workbook)
		{
			if (this.IsResolved)
				return this;

			return new WorkbookColorInfo(this.GetResolvedColor(workbook), null, null, false, true);
		}

		#endregion // ToResolved

		#endregion // Internal Methods

		#region Private Methods

		#region TestFlag

		private static bool TestFlag(ColorInfoState value, ColorInfoState flag)
		{
			return (value & flag) == flag;
		}

		#endregion // TestFlag

		#region ValidateColor

		private static void ValidateColor(Color color)
		{
			if (Utilities.ColorIsEmpty(color))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CannotCreateEmptyColorInfo"), "color");

			if (color.A != 255)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CannotCreateNonOpaqueColorInfo"), "color");
		}

		#endregion // ValidateColor

		#region ValidateThemeColor

		private static void ValidateThemeColor(WorkbookThemeColorType themeColor)
		{
			Utilities.VerifyEnumValue(themeColor, "themeColor");
		}

		#endregion // ValidateThemeColor

		#region ValidateTint

		private static void ValidateTint(ref double tint, bool isLoading)
		{
			if (tint < -1 || 1 < tint)
			{
				if (isLoading)
				{
					tint = tint % 2;

					if (tint < -1)
						tint += 2;
					else if (1 < tint)
						tint -= 2;
				}
				else
				{
					throw new ArgumentOutOfRangeException("tint", tint, SR.GetString("LE_ArgumentOutOfRangeException_InvalidColorInfoTint"));
				}
			}
		}

		#endregion // ValidateTint

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Automatic

		/// <summary>
		/// Gets the automatic color, which is the window text system color.
		/// </summary>
		/// <seealso cref="IsAutomatic"/>
		public static WorkbookColorInfo Automatic
		{
			get { return _automatic; }
		}

		#endregion // Automatic

		#region Color

		/// <summary>
		/// Gets the base color associated of the <see cref="WorkbookColorInfo"/>.
		/// </summary>
		/// <seealso cref="Tint"/>
		public Color? Color
		{
			get
			{
				if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsAutomatic))
					return Utilities.SystemColorsInternal.WindowTextColor;

				if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsRGB))
					return _color;

				return null;
			}
		}

		#endregion // Color

		#region IsAutomatic

		/// <summary>
		/// Gets the value which indicates whether the <see cref="WorkbookColorInfo"/> is automatic, or the window text system color.
		/// </summary>
		/// <see cref="Automatic"/>
		public bool IsAutomatic
		{
			get { return WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsAutomatic); }
		}

		#endregion // IsAutomatic

		#region ThemeColorType

		/// <summary>
		/// Gets the base theme color associated of the <see cref="WorkbookColorInfo"/>.
		/// </summary>
		/// <seealso cref="Tint"/>
		public WorkbookThemeColorType? ThemeColorType
		{
			get
			{
				if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsThemeColor))
					return (WorkbookThemeColorType)_themeColorType;

				return null;
			}
		}

		#endregion // ThemeColorType

		#region Tint

		/// <summary>
		/// Gets the to apply to the base color, from -1.0 (100% darken) to 1.0 (100% lighten).
		/// </summary>
		/// <seealso cref="Color"/>
		/// <seealso cref="ThemeColorType"/>
		public double? Tint
		{
			get
			{
				if (WorkbookColorInfo.TestFlag(_state, ColorInfoState.IsTintValid))
					return _tint;

				return null;
			}
		}

		#endregion // Tint

		#endregion // Public Properties

		#region Internal Properties

		// MD 5/7/12 - TFS106831
		#region IsResolved

		internal bool IsResolved
		{
			get { return _state == ColorInfoState.IsRGB; }
		}

		#endregion // IsResolved

		#region IsSystemColor

		internal bool IsSystemColor
		{
			get
			{
				if (this.IsAutomatic)
					return true;

				if (this.Color.HasValue && Utilities.ColorIsSystem(this.Color.Value))
					return true;

				return false;
			}
		}

		#endregion // IsSystemColor

		#endregion // Internal Properties

		#endregion // Properties


		#region ColorInfoState enum

		private enum ColorInfoState : byte
		{
			IsAutomatic = 0x01,
			IsRGB = 0x02,
			IsThemeColor = 0x04,
			IsTintValid = 0x08,
		}

		#endregion // ColorInfoState enum
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