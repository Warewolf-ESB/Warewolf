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
	/// Immutable class which describes a color transition in a cell fill gradient.
	/// </summary>
	/// <seealso cref="CellFillLinearGradient"/>
	/// <seealso cref="CellFillRectangularGradient"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

 sealed class CellFillGradientStop
	{
		#region Member Variables

		private readonly WorkbookColorInfo _colorInfo;
		private readonly double _offset;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CellFillGradientStop"/> instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When used in a <see cref="CellFillLinearGradient"/>, an <paramref name="offset"/> of 0.0 is at the beginning of the gradient and
		/// 1.0 is at the end of the gradient. When used in a <see cref="CellFillRectangularGradient"/>, an offset of 0.0 is at the inner 
		/// rectangle and 1.0 is at the outer edges of the cell.
		/// </p>
		/// </remarks>
		/// <param name="color">The color transition for the gradient stop.</param>
		/// <param name="offset">
		/// The position in the gradient of the color transition for the gradient stop, ranging from 0.0 to 1.0.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> is less than 0.0 or greater than 1.0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="color"/> is the empty or system color or has a non-opaque alpha channel.
		/// </exception>
		public CellFillGradientStop(Color color, double offset)
			: this(new WorkbookColorInfo(color), offset) { }

		/// <summary>
		/// Creates a new <see cref="CellFillGradientStop"/> instance.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When used in a <see cref="CellFillLinearGradient"/>, an <paramref name="offset"/> of 0.0 is at the beginning of the gradient and
		/// 1.0 is at the end of the gradient. When used in a <see cref="CellFillRectangularGradient"/>, an offset of 0.0 is at the inner 
		/// rectangle and 1.0 is at the outer edges of the cell.
		/// </p>
		/// </remarks>
		/// <param name="colorInfo">
		/// The <see cref="WorkbookColorInfo"/> describing the color transition for the gradient stop.
		/// </param>
		/// <param name="offset">
		/// The position in the gradient of the color transition for the gradient stop, ranging from 0.0 to 1.0.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="colorInfo"/> is automatic or a system color.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> is less than 0.0 or greater than 1.0.
		/// </exception>
		public CellFillGradientStop(WorkbookColorInfo colorInfo, double offset)
		{
			if (colorInfo.IsSystemColor)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidGradientStopColor"), "colorInfo");

			if (offset < 0 || 1 < offset)
				throw new ArgumentOutOfRangeException("offset", offset, SR.GetString("LE_ArgumentOutOfRangeException_InvalidGradientStopOffset"));

			_colorInfo = colorInfo;
			_offset = offset;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="CellFillGradientStop"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			CellFillGradientStop other = obj as CellFillGradientStop;
			if (other == null)
				return false;

			if (_offset != other._offset)
				return false;

			if (_colorInfo != other._colorInfo)
				return false;

			return true;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="CellFillGradientStop"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return
				_colorInfo.GetHashCode() ^
				_offset.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		// MD 5/7/12 - TFS106831
		#region ToResolvedColorStop

		internal CellFillGradientStop ToResolvedColorStop(Workbook workbook)
		{
			if (_colorInfo.IsResolved)
				return this;

			return new CellFillGradientStop(_colorInfo.ToResolved(workbook), _offset);
		}

		#endregion // ToResolvedColorStop

		#endregion // Methods

		#region Properties

		#region ColorInfo

		/// <summary>
		/// Gets the <see cref="WorkbookColorInfo"/> describing the color transition for the gradient stop.
		/// </summary>
		/// <seealso cref="Offset"/>
		public WorkbookColorInfo ColorInfo
		{
			get { return _colorInfo; }
		}

		#endregion // ColorInfo

		#region Offset

		/// <summary>
		/// Gets the position in the gradient of the color transition for the gradient stop, ranging from 0.0 to 1.0.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When used in a <see cref="CellFillLinearGradient"/>, a value of 0.0 is at the beginning of the gradient and 1.0 is at the end of the 
		/// gradient. When used in a <see cref="CellFillRectangularGradient"/>, a value of 0.0 is at the inner rectangle and 1.0 is at the outer 
		/// edges of the cell.
		/// </p>
		/// </remarks>
		/// <seealso cref="ColorInfo"/>
		public double Offset
		{
			get { return _offset; }
		}

		#endregion // Offset

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