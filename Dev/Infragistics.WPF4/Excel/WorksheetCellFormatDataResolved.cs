using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Documents.Excel.Serialization.BIFF8;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 1/8/12 - 12.1 - Cell Format Updates
	internal class WorksheetCellFormatDataResolved :
		IWorksheetCellFormat
	{
		#region Member Variables

		private WorkbookFontDataResolved _fontResolved;
		private WorksheetCellFormatProxy _proxy;

		#endregion // Member Variables

		#region Constructor

		public WorksheetCellFormatDataResolved(WorksheetCellFormatProxy proxy)
		{
			_proxy = proxy;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			WorksheetCellFormatDataResolved other = obj as WorksheetCellFormatDataResolved;
			if (other == null)
				return false;

			return _proxy.Equals(other._proxy);
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return _proxy.GetHashCode();
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Interfaces

		#region IWorksheetCellFormat Members

		Color IWorksheetCellFormat.BottomBorderColor
		{
			get
			{
				return this.BottomBorderColorInfo.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.DiagonalBorderColor
		{
			get
			{
				return this.DiagonalBorderColorInfo.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		FillPatternStyle IWorksheetCellFormat.FillPattern
		{
			get
			{
				WorksheetCellFormatData formatData = _proxy.Element;
				return formatData.GetFillPattern(formatData.FillResolved);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.FillPatternBackgroundColor
		{
			get
			{
				WorksheetCellFormatData formatData = _proxy.Element;
				return formatData.GetFileFormatFillPatternColor(formatData.FillResolved, true, true).GetResolvedColor(formatData.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.FillPatternForegroundColor
		{
			get
			{
				WorksheetCellFormatData formatData = _proxy.Element;
				return formatData.GetFileFormatFillPatternColor(formatData.FillResolved, false, true).GetResolvedColor(formatData.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.LeftBorderColor
		{
			get
			{
				return this.LeftBorderColorInfo.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.RightBorderColor
		{
			get
			{
				return this.RightBorderColorInfo.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		Color IWorksheetCellFormat.TopBorderColor
		{
			get
			{
				return this.TopBorderColorInfo.GetResolvedColor(_proxy.Element.Workbook);
			}
			set
			{
				this.ThrowOnSet();
			}
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region GetAdjacentBorderValue

		internal virtual bool GetAdjacentBorderValue(CellFormatValue borderProperty, out object value)
		{
			value = null;

			if (_proxy.Owner == null)
				return false;

			WorksheetCellFormatData adjacentFormat = _proxy.Owner.GetAdjacentFormatForBorderResolution(_proxy, borderProperty);
			if (adjacentFormat == null ||
				Utilities.TestFlag(adjacentFormat.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting) == false)
				return false;

			value = adjacentFormat.GetResolvedValue(Utilities.GetOppositeBorderValue(borderProperty), null);
			return true;
		}

		#endregion // GetAdjacentBorderValue

		#region SetFormatting

		public void SetFormatting(IWorksheetCellFormat source)
		{
			this.ThrowOnSet();
		}

		#endregion // SetFormatting

		#region ThrowOnSet

		private void ThrowOnSet()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ResolvedFormatCannotBeModified"));
		}

		#endregion // ThrowOnSet

		#endregion // Methods

		#region Properties

		public HorizontalCellAlignment Alignment
		{
			get { return _proxy.Element.AlignmentResolved; }
			set { this.ThrowOnSet(); }
		}

		public WorkbookColorInfo BottomBorderColorInfo
		{
			get { return (WorkbookColorInfo)_proxy.Element.GetResolvedValue(CellFormatValue.BottomBorderColorInfo, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public CellBorderLineStyle BottomBorderStyle
		{
			get { return (CellBorderLineStyle)_proxy.Element.GetResolvedValue(CellFormatValue.BottomBorderStyle, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public WorkbookColorInfo DiagonalBorderColorInfo
		{
			get { return _proxy.Element.DiagonalBorderColorInfoResolved; }
			set { this.ThrowOnSet(); }
		}

		public DiagonalBorders DiagonalBorders
		{
			get { return _proxy.Element.DiagonalBordersResolved; }
			set { this.ThrowOnSet(); }
		}

		public CellBorderLineStyle DiagonalBorderStyle
		{
			get { return _proxy.Element.DiagonalBorderStyleResolved; }
			set { this.ThrowOnSet(); }
		}

		public CellFill Fill
		{
			get { return _proxy.Element.FillResolved; }
			set { this.ThrowOnSet(); }
		}

		public IWorkbookFont Font
		{
			get
			{
				if (_fontResolved == null)
					_fontResolved = new WorkbookFontDataResolved(_proxy);

				return _fontResolved;
			}
		}

		public WorksheetCellFormatOptions FormatOptions
		{
			get { return _proxy.Element.FormatOptionsResolved; }
			set { this.ThrowOnSet(); }
		}

		public string FormatString
		{
			get { return _proxy.Element.FormatStringResolved; }
			set { this.ThrowOnSet(); }
		}

		public int Indent
		{
			get { return _proxy.Element.IndentResolved; }
			set { this.ThrowOnSet(); }
		}

		public WorkbookColorInfo LeftBorderColorInfo
		{
			get { return (WorkbookColorInfo)_proxy.Element.GetResolvedValue(CellFormatValue.LeftBorderColorInfo, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public CellBorderLineStyle LeftBorderStyle
		{
			get { return (CellBorderLineStyle)_proxy.Element.GetResolvedValue(CellFormatValue.LeftBorderStyle, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public ExcelDefaultableBoolean Locked
		{
			get { return _proxy.Element.LockedResolved; }
			set { this.ThrowOnSet(); }
		}

		public WorkbookColorInfo RightBorderColorInfo
		{
			get { return (WorkbookColorInfo)_proxy.Element.GetResolvedValue(CellFormatValue.RightBorderColorInfo, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public CellBorderLineStyle RightBorderStyle
		{
			get { return (CellBorderLineStyle)_proxy.Element.GetResolvedValue(CellFormatValue.RightBorderStyle, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public int Rotation
		{
			get { return _proxy.Element.RotationResolved; }
			set { this.ThrowOnSet(); }
		}

		public ExcelDefaultableBoolean ShrinkToFit
		{
			get { return _proxy.Element.ShrinkToFitResolved; }
			set { this.ThrowOnSet(); }
		}

		public WorkbookStyle Style
		{
			get { return _proxy.Element.StyleResolved; }
			set { this.ThrowOnSet(); }
		}

		public WorkbookColorInfo TopBorderColorInfo
		{
			get { return (WorkbookColorInfo)_proxy.Element.GetResolvedValue(CellFormatValue.TopBorderColorInfo, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public CellBorderLineStyle TopBorderStyle
		{
			get { return (CellBorderLineStyle)_proxy.Element.GetResolvedValue(CellFormatValue.TopBorderStyle, this.GetAdjacentBorderValue); }
			set { this.ThrowOnSet(); }
		}

		public VerticalCellAlignment VerticalAlignment
		{
			get { return _proxy.Element.VerticalAlignmentResolved; }
			set { this.ThrowOnSet(); }
		}

		public ExcelDefaultableBoolean WrapText
		{
			get { return _proxy.Element.WrapTextResolved; }
			set { this.ThrowOnSet(); }
		}

		#endregion // Properties
	}

	internal class WorksheetMergedCellFormatDataResolved : WorksheetCellFormatDataResolved
	{
		private WorksheetMergedCellsRegion _mergedCellRegion;

		public WorksheetMergedCellFormatDataResolved(WorksheetMergedCellsRegion mergedCellRegion)
			: base(mergedCellRegion.CellFormatInternal)
		{
			_mergedCellRegion = mergedCellRegion;
		}

		internal override bool GetAdjacentBorderValue(CellFormatValue borderProperty, out object value)
		{
			Dictionary<CellFormatValue, object> borderValues = _mergedCellRegion.GetMergedCellBorderValues(borderProperty, true);
			borderValues.TryGetValue(borderProperty, out value);
			return Object.Equals(value, WorksheetCellFormatData.GetDefaultValue(borderProperty)) == false;
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