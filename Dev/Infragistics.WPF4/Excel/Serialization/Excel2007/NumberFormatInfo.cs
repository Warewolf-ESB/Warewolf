using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class NumberFormatInfo
    {
        #region Members

        private string formatCode = string.Empty;
        private int numFmtId = -1;

        #endregion Members

        #region Properties






        public string FormatCode
        {
            get { return this.formatCode; }
            set { this.formatCode = value; }
        }






        public int NumberFormatId
        {
            get { return this.numFmtId; }
            set { this.numFmtId = value; }
        }

        #endregion Properties

		// MD 12/21/11 - 12.1 - Table Support
		// Moved this code from the FormatInfo.CreateWorksheetCellFormatData so it can be used in other places.
		#region Methods

		#region ApplyTo

		internal void ApplyTo(WorksheetCellFormatData formatData, WorkbookSerializationManager manager)
		{
			if (String.IsNullOrEmpty(this.formatCode) == false)
				formatData.FormatString = this.formatCode;
			else if (numFmtId >= 0)
				formatData.FormatString = manager.Workbook.Formats[numFmtId];
		}

		#endregion // ApplyTo

		// MD 12/30/11 - 12.1 - Table Support
		#region CreateNumberFormatInfo

		public static NumberFormatInfo CreateNumberFormatInfo(WorksheetCellFormatData formatData)
		{
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.FormatCode = formatData.FormatStringResolved;
			numberFormatInfo.NumberFormatId = formatData.FormatStringIndex;
			return numberFormatInfo;
		}

		#endregion // CreateNumberFormatInfo

		#endregion // Methods
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