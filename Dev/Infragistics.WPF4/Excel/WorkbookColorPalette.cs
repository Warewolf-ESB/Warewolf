using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/16/12 - 12.1 - Cell Format Updates



	/// <summary>
	/// Represents the color palette used when the saved file is opened in Microsoft Excel 2003 and earlier versions. 
	/// </summary>
	/// <seealso cref="Workbook.Palette"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

		sealed class WorkbookColorPalette : ICollection<Color>
	{
		#region Constants

		internal const int AutomaticColor = 64;
		internal const int UserPaletteSize = 56;
		internal const int UserPaletteStart = 8;

		private static readonly Color BlackColor = Color.FromArgb(255, 0, 0, 0);
		private static readonly Color WhiteColor = Color.FromArgb(255, 255, 255, 255);
		private static readonly Color RedColor = Color.FromArgb(255, 255, 0, 0);
		private static readonly Color LimeColor = Color.FromArgb(255, 0, 255, 0);
		private static readonly Color BlueColor = Color.FromArgb(255, 0, 0, 255);
		private static readonly Color YellowColor = Color.FromArgb(255, 255, 255, 0);
		private static readonly Color MagentaColor = Color.FromArgb(255, 255, 0, 255);
		private static readonly Color CyanColor = Color.FromArgb(255, 0, 255, 255);

		private static Color[] ExcelStandardColors = new Color[]
            {
				Color.FromArgb(0xFF, 0x00, 0x00, 0x00),	// 8
				Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF),	// 9
				Color.FromArgb(0xFF, 0xFF, 0x00, 0x00),	// 10
				Color.FromArgb(0xFF, 0x00, 0xFF, 0x00),	// 11
				Color.FromArgb(0xFF, 0x00, 0x00, 0xFF),	// 12
				Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00),	// 13
				Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF),	// 14
				Color.FromArgb(0xFF, 0x00, 0xFF, 0xFF),	// 15
				Color.FromArgb(0xFF, 0x80, 0x00, 0x00),	// 16
				Color.FromArgb(0xFF, 0x00, 0x80, 0x00),	// 17
				Color.FromArgb(0xFF, 0x00, 0x00, 0x80),	// 18
				Color.FromArgb(0xFF, 0x80, 0x80, 0x00),	// 19
				Color.FromArgb(0xFF, 0x80, 0x00, 0x80),	// 20
				Color.FromArgb(0xFF, 0x00, 0x80, 0x80),	// 21
				Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0),	// 22
				Color.FromArgb(0xFF, 0x80, 0x80, 0x80),	// 23
				Color.FromArgb(0xFF, 0x99, 0x99, 0xFF),	// 24
				Color.FromArgb(0xFF, 0x99, 0x33, 0x66),	// 25
				Color.FromArgb(0xFF, 0xFF, 0xFF, 0xCC),	// 26
				Color.FromArgb(0xFF, 0xCC, 0xFF, 0xFF),	// 27
				Color.FromArgb(0xFF, 0x66, 0x00, 0x66),	// 28
				Color.FromArgb(0xFF, 0xFF, 0x80, 0x80),	// 29
				Color.FromArgb(0xFF, 0x00, 0x66, 0xCC),	// 30
				Color.FromArgb(0xFF, 0xCC, 0xCC, 0xFF),	// 31
				Color.FromArgb(0xFF, 0x00, 0x00, 0x80),	// 32
				Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF),	// 33
				Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00),	// 34
				Color.FromArgb(0xFF, 0x00, 0xFF, 0xFF),	// 35
				Color.FromArgb(0xFF, 0x80, 0x00, 0x80),	// 36
				Color.FromArgb(0xFF, 0x80, 0x00, 0x00),	// 37
				Color.FromArgb(0xFF, 0x00, 0x80, 0x80),	// 38
				Color.FromArgb(0xFF, 0x00, 0x00, 0xFF),	// 39
				Color.FromArgb(0xFF, 0x00, 0xCC, 0xFF),	// 40
				Color.FromArgb(0xFF, 0xCC, 0xFF, 0xFF),	// 41
				Color.FromArgb(0xFF, 0xCC, 0xFF, 0xCC),	// 42
				Color.FromArgb(0xFF, 0xFF, 0xFF, 0x99),	// 43
				Color.FromArgb(0xFF, 0x99, 0xCC, 0xFF),	// 44
				Color.FromArgb(0xFF, 0xFF, 0x99, 0xCC),	// 45
				Color.FromArgb(0xFF, 0xCC, 0x99, 0xFF),	// 46
				Color.FromArgb(0xFF, 0xFF, 0xCC, 0x99),	// 47
				Color.FromArgb(0xFF, 0x33, 0x66, 0xFF),	// 48
				Color.FromArgb(0xFF, 0x33, 0xCC, 0xCC),	// 49
				Color.FromArgb(0xFF, 0x99, 0xCC, 0x00),	// 50
				Color.FromArgb(0xFF, 0xFF, 0xCC, 0x00),	// 51
				Color.FromArgb(0xFF, 0xFF, 0x99, 0x00),	// 52
				Color.FromArgb(0xFF, 0xFF, 0x66, 0x00),	// 53
				Color.FromArgb(0xFF, 0x66, 0x66, 0x99),	// 54
				Color.FromArgb(0xFF, 0x96, 0x96, 0x96),	// 55
				Color.FromArgb(0xFF, 0x00, 0x33, 0x66),	// 56
				Color.FromArgb(0xFF, 0x33, 0x99, 0x66),	// 57
				Color.FromArgb(0xFF, 0x00, 0x33, 0x00),	// 58
				Color.FromArgb(0xFF, 0x33, 0x33, 0x00),	// 59
				Color.FromArgb(0xFF, 0x99, 0x33, 0x00),	// 60
				Color.FromArgb(0xFF, 0x99, 0x33, 0x66),	// 61
				Color.FromArgb(0xFF, 0x33, 0x33, 0x99),	// 62
				Color.FromArgb(0xFF, 0x33, 0x33, 0x33),	// 63
			};

		#endregion Constants

		#region Member Variables

		private bool _isCustom;
		private Color[] _userColors;

		#endregion Member Variables

		#region Constructor

		internal WorkbookColorPalette()
		{
			_userColors = (Color[])WorkbookColorPalette.ExcelStandardColors.Clone();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<Color> Members

		void ICollection<Color>.Add(Color item)
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotAddColorsToPaletteDirectly"));
		}

		void ICollection<Color>.Clear()
		{
			this.Reset();
		}

		void ICollection<Color>.CopyTo(Color[] array, int arrayIndex)
		{
			_userColors.CopyTo(array, arrayIndex);
		}

		bool ICollection<Color>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<Color>.Remove(Color item)
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotRemoveColorsToPaletteDirectly"));
		}

		#endregion

		#region IEnumerable<Color> Members

		IEnumerator<Color> IEnumerable<Color>.GetEnumerator()
		{
			for (int i = 0; i < _userColors.Length; i++)
				yield return _userColors[i];
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _userColors.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Determines whether the specified color is in the color palette.
		/// </summary>
		/// <param name="color">The color to find in the palette.</param>
		/// <returns>Determines whether an equivalent color is in the palette.</returns>
		public bool Contains(Color color)
		{
			int index = this.GetIndexOfNearestColor(color);
			return WorkbookColorPalette.AreEquivalent(color, this[index]);
		}

		#endregion // Contains

		#region GetIndexOfNearestColor

		/// <summary>
		/// Gets the index of the closest color in the color palette, which will be seen when the file is opened in Microsoft Excel 2003 
		/// and older versions.
		/// </summary>
		/// <param name="color">The color to match in the palette.</param>
		/// <returns>A 0-based index into the collection of the closest color available in the palette.</returns>
		public int GetIndexOfNearestColor(Color color)
		{
			int index = 0;
			double shortestDistance = double.MaxValue;

			double l;
			double a;
			double b;
			Utilities.RGBToCIELAB(color, out l, out a, out b);

			for (int i = 0; i < _userColors.Length; i++)
			{
				Color testColor = _userColors[i];

				double l2;
				double a2;
				double b2;
				Utilities.RGBToCIELAB(testColor, out l2, out a2, out b2);

				double distance = (Math.Pow(l - l2, 2)) + (Math.Pow(a - a2, 2)) + (Math.Pow(b - b2, 2));
				if (distance < shortestDistance)
				{
					index = i;
					shortestDistance = distance;
				}
			}

			return index;
		}

		#endregion GetIndexOfNearestColor

		#region Reset

		/// <summary>
		/// Resets the palette back to the default colors for Microsoft Excel.
		/// </summary>
		/// <seealso cref="IsCustom"/>
		public void Reset()
		{
			if (_isCustom == false)
				return;

			_userColors = (Color[])WorkbookColorPalette.ExcelStandardColors.Clone();
			_isCustom = false;
		}

		#endregion // Reset

		#endregion // Public Methods

		#region Internal Methods

		#region FindIndex

		internal int FindIndex(Color color, ColorableItem item)
		{
			try
			{
				return this.FindIndexHelper(color, item);
			}
			catch (UnauthorizedAccessException exc)
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_SystemColorsAccessedFromWrongThread"), exc);
			}
		}

		internal int FindIndexHelper(Color color, ColorableItem item)
		{
			if (Utilities.ColorIsEmpty(color))
				return -1;

			if (item == ColorableItem.WorksheetTab)
			{
				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.BlackColor))
					return 0x00;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.WhiteColor))
					return 0x01;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.RedColor))
					return 0x02;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.LimeColor))
					return 0x03;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.BlueColor))
					return 0x04;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.YellowColor))
					return 0x05;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.MagentaColor))
					return 0x06;

				if (WorkbookColorPalette.AreEquivalent(color, WorkbookColorPalette.CyanColor))
					return 0x07;
			}

			if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.WindowTextColor))
			{
				if (item == ColorableItem.CellFont)
					return 0x7FFF;

				return 0x40;
			}

			if (item != ColorableItem.WorksheetGrid && item != ColorableItem.WorksheetTab)
			{
				if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.WindowColor))
					return 0x41;

				if (item != ColorableItem.CellFill)
				{
					if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.WindowFrameColor))
						return 0x42;

					if (item != ColorableItem.CellBorder)
					{
						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.ControlColor))
							return 0x43;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.ControlTextColor))
							return 0x44;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.ControlLightLightColor))
							return 0x45;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.ControlDarkColor))
							return 0x46;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.HighlightColor))
							return 0x47;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.ScrollBarColor))
							return 0x49;

						if (WorkbookColorPalette.AreEquivalent(color, InvertColor(Utilities.SystemColorsInternal.ScrollBarColor)))
							return 0x4A;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.InfoColor))
							return 0x50;

						if (WorkbookColorPalette.AreEquivalent(color, Utilities.SystemColorsInternal.InfoTextColor))
							return 0x51;
					}
				}
			}

			return this.GetIndexOfNearestColor(color) + WorkbookColorPalette.UserPaletteStart;
		}

		#endregion FindIndex

		#region GetColorAtAbsoluteIndex

		internal Color GetColorAtAbsoluteIndex(int index)
		{
			try
			{
				return this.GetColorAtAbsoluteIndexHelper(index);
			}
			catch (UnauthorizedAccessException exc)
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_SystemColorsAccessedFromWrongThread"), exc);
			}
		}

		private Color GetColorAtAbsoluteIndexHelper(int index)
		{
			// Chart fills: 24 - 31
			// Chart lines: 32 - 39

			switch (index)
			{
				case 0x00: return WorkbookColorPalette.BlackColor;
				case 0x01: return WorkbookColorPalette.WhiteColor;
				case 0x02: return WorkbookColorPalette.RedColor;
				case 0x03: return WorkbookColorPalette.LimeColor;
				case 0x04: return WorkbookColorPalette.BlueColor;
				case 0x05: return WorkbookColorPalette.YellowColor;
				case 0x06: return WorkbookColorPalette.MagentaColor;
				case 0x07: return WorkbookColorPalette.CyanColor;

				case 0x40: return Utilities.SystemColorsInternal.WindowTextColor;
				case 0x41: return Utilities.SystemColorsInternal.WindowColor;
				case 0x42: return Utilities.SystemColorsInternal.WindowFrameColor;
				case 0x43: return Utilities.SystemColorsInternal.ControlColor;
				case 0x44: return Utilities.SystemColorsInternal.ControlTextColor;
				case 0x45: return Utilities.SystemColorsInternal.ControlLightLightColor;
				case 0x46: return Utilities.SystemColorsInternal.ControlDarkColor;
				case 0x47: return Utilities.SystemColorsInternal.HighlightColor;
				case 0x48: return Utilities.SystemColorsInternal.WindowTextColor;
				case 0x49: return Utilities.SystemColorsInternal.ScrollBarColor;
				case 0x4A: return InvertColor(Utilities.SystemColorsInternal.ScrollBarColor);
				case 0x4B: return Utilities.SystemColorsInternal.WindowColor;
				case 0x4C: return Utilities.SystemColorsInternal.WindowFrameColor;
				case 0x4D: return Utilities.SystemColorsInternal.WindowTextColor;
				case 0x4E: return Utilities.SystemColorsInternal.WindowColor;
				case 0x4F: return WorkbookColorPalette.BlackColor;
				case 0x50: return Utilities.SystemColorsInternal.InfoColor;
				case 0x51: return Utilities.SystemColorsInternal.InfoTextColor;

				case 0x7FFF: return Utilities.SystemColorsInternal.WindowTextColor;

				default:
					if (0x51 < index)
						return Utilities.SystemColorsInternal.WindowColor;

					break;
			}

			int userIndex = index - WorkbookColorPalette.UserPaletteStart;
			if (userIndex < 0 || WorkbookColorPalette.UserPaletteSize <= userIndex)
			{
				Utilities.DebugFail("This is unexpected.");
				return Utilities.ColorEmpty;
			}

			return this[userIndex];
		}

		#endregion // GetColorAtAbsoluteIndex

		#endregion Internal Methods

		#region Private Methods

		#region AreEquivalent

		internal static bool AreEquivalent(Color color1, Color color2)
		{
			if (Utilities.ColorIsSystem(color1) || Utilities.ColorIsSystem(color2))
				return color1 == color2;

			if (Utilities.ColorIsEmpty(color1) != Utilities.ColorIsEmpty(color2))
				return false;

			return Utilities.ColorToArgb(color1) == Utilities.ColorToArgb(color2);
		}

		#endregion // AreEquivalent

		#region InvertColor

		private static Color InvertColor(Color color)
		{
			return Color.FromArgb(color.A, (byte)~color.R, (byte)~color.G, (byte)~color.B);
		}

		#endregion InvertColor

		#region ValidateUserIndex

		private void ValidateUserIndex(int index)
		{
			if (index < 0 || _userColors.Length <= index)
				throw new ArgumentOutOfRangeException("index");
		}

		#endregion // ValidateUserIndex

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of colors in the palette, which is always 56.
		/// </summary>
		public int Count
		{
			get { return _userColors.Length; }
		}

		#endregion // Count

		#region Indexer[int]

		/// <summary>
		/// Gets or sets a color in the palette.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When a color is set in the palette, <see cref="IsCustom"/> will return True. The palette can than be reset with the 
		/// <see cref="Reset"/> method.
		/// </p>
		/// <p class="body">
		/// Colors added to the palette must be opaque.
		/// </p>
		/// </remarks>
		/// <param name="index">The index of the color to get or set in the palette.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than 55.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is an empty color, a system color, or is not opaque.
		/// </exception>
		/// <seealso cref="IsCustom"/>
		/// <seealso cref="Reset"/>
		public Color this[int index]
		{
			get
			{
				this.ValidateUserIndex(index);
				return _userColors[index];
			}
			set
			{
				this.ValidateUserIndex(index);
				if (Utilities.ColorIsEmpty(value) || Utilities.ColorIsSystem(value))
					throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidPaletteColor_EmptyOrSystem"));

				if (value.A != 255)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidPaletteColor_NonOpaque"));

				Color oldColor = _userColors[index];
				_userColors[index] = value;

				if (WorkbookColorPalette.AreEquivalent(oldColor, value) == false)
					_isCustom = true;
			}
		}

		#endregion // Indexer[int]

		#region IsCustom

		/// <summary>
		/// Gets the value which indicates whether the palette has been cusotmized.
		/// </summary>
		/// <seealso cref="Reset"/>
		public bool IsCustom
		{
			get { return _isCustom; }
		}

		#endregion // IsCustom

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