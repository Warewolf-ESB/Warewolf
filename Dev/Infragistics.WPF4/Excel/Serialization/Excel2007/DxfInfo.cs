using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class DxfInfo
    {

        #region Members

        private AlignmentInfo alignment = null;
        private BorderInfo border = null;
        private FillInfo fill = null;
        private IWorkbookFont font = null;

		// MD 2/17/12 - 12.1 - Table Support
		private ColorInfo fontColorInfo;

        private NumberFormatInfo numberFormat = null;
        private ProtectionInfo protection = null;

        #endregion Members

		// MD 12/21/11 - 12.1 - Table Support
		#region Constructor

		public DxfInfo()
		{

		}

		public DxfInfo(WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting))
				this.alignment = AlignmentInfo.CreateAlignmentInfo(formatData, true);

			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting))
				this.border = BorderInfo.CreateBorderInfo(manager, formatData);

			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting))
				this.fill = FillInfo.CreateFillInfo(manager, formatData);

			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting))
				this.numberFormat = NumberFormatInfo.CreateNumberFormatInfo(formatData);

			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting))
				this.protection = ProtectionInfo.CreateProtectionInfo(formatData, true);

			if (Utilities.TestFlag(formatData.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting))
				this.font = formatData.FontInternal.Element.ResolvedFontData(formatData);
		}

		#endregion // Constructor

		#region Methods

		// MD 12/21/11 - 12.1 - Table Support
		#region ToCellFormat

		internal WorksheetCellFormatData ToCellFormat(WorkbookSerializationManager manager)
		{
			WorksheetCellFormatData formatData = manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);

			if (this.alignment != null)
				this.alignment.ApplyTo(formatData);

			if (this.border != null)
				this.border.ApplyTo(formatData, manager);

			if (this.fill != null)
				this.fill.ApplyTo(formatData, manager);

			if (this.font != null)
				formatData.Font.SetFontFormatting(this.font);

			if (this.fontColorInfo != null)
				formatData.Font.ColorInfo = this.fontColorInfo.ResolveColorInfo(manager);

			if (this.numberFormat != null)
				this.numberFormat.ApplyTo(formatData, manager);

			if (this.protection != null)
				this.protection.ApplyTo(formatData);

			return formatData;
		}

		#endregion // ToCellFormat

		#endregion // Methods

        #region Properties

        #region Alignment






        public AlignmentInfo Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        #endregion Alignment

        #region Border






        public BorderInfo Border
        {
            get { return this.border; }
            set { this.border = value; }
        }

        #endregion Border

        #region Fill






        public FillInfo Fill
        {
            get { return this.fill; }
            set { this.fill = value; }
        }

        #endregion Fill

        #region Font






        public IWorkbookFont Font
        {
            get { return this.font; }
            set { this.font = value; }
        }

        #endregion Font

		// MD 2/17/12 - 12.1 - Table Support
		#region FontColorInfo

		public ColorInfo FontColorInfo
		{
			get { return this.fontColorInfo; }
			set { this.fontColorInfo = value; }
		}

		#endregion FontColorInfo

		#region NumberFormat






        public NumberFormatInfo NumberFormat
        {
            get { return this.numberFormat; }
            set { this.numberFormat = value; }
        }

        #endregion NumberFormat

        #region Protection






        public ProtectionInfo Protection
        {
            get { return this.protection; }
            set { this.protection = value; }
        }

        #endregion Protection

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