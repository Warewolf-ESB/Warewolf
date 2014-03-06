using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;





using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
    /// <summary>
    /// Class used to represent the Fill element
    /// </summary>
    internal class FillInfo
    {

        #region Members

        private PatternFillInfo patternFillInfo = null;
        private GradientFillInfo gradientFillInfo = null;

        #endregion Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			FillInfo other = obj as FillInfo;
			if (other == null)
				return false;

			return
				Object.Equals(this.patternFillInfo, other.patternFillInfo) &&
				Object.Equals(this.gradientFillInfo, other.gradientFillInfo);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = 0;
			if (this.patternFillInfo != null)
				hashCode ^= this.patternFillInfo.GetHashCode();

			if (this.gradientFillInfo != null)
				hashCode ^= this.gradientFillInfo.GetHashCode();

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties






        public PatternFillInfo PatternFill
        {
            get { return this.patternFillInfo; }
            set { this.patternFillInfo = value; }
        }






        public GradientFillInfo GradientFill
        {
            get { return this.gradientFillInfo; }
            set { this.gradientFillInfo = value; }
        }

        #endregion Properties

        #region Methods

		// MD 12/21/11 - 12.1 - Cell Format Updates
		// Moved this code from the FormatInfo.CreateWorksheetCellFormatData so it can be used in other places.
		#region ApplyTo

		internal void ApplyTo(WorksheetCellFormatData formatData, WorkbookSerializationManager manager)
		{
			if (this.PatternFill != null)
				formatData.Fill = this.PatternFill.ToCellFill(manager, formatData);
			else if (this.GradientFill != null)
				formatData.Fill = this.GradientFill.ToCellFill(manager);
			else
				Utilities.DebugFail("Unknown cell fill.");
		}

		#endregion // ApplyTo

		// MD 12/30/11 - 12.1 - Cell Format Updates
		#region CreateFillInfo

		public static FillInfo CreateFillInfo(WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			FillInfo fillInfo = new FillInfo();

			CellFill fillResolved = formatData.FillResolved;

			CellFillPattern patternFill = fillResolved as CellFillPattern;
			if (patternFill != null)
			{
				fillInfo.PatternFill = PatternFillInfo.CreatePatternFillInfo(manager, formatData, patternFill);
			}
			else
			{
				CellFillGradient gradientFill = fillResolved as CellFillGradient;
				if (gradientFill != null)
				{
					fillInfo.GradientFill = GradientFillInfo.CreateGradientFillInfo(manager, gradientFill);
				}
				else
				{
					Utilities.DebugFail("Unknown cell fill.");
				}
			}

			return fillInfo;
		}

		#endregion // CreateFillInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(FillInfo fill1, FillInfo fill2)
		//{
		//    if (ReferenceEquals(fill1, null) &&
		//        ReferenceEquals(fill2, null))
		//        return true;
		//    if (ReferenceEquals(fill1, null) ||
		//        ReferenceEquals(fill2, null))
		//        return false;
		//    return (GradientFillInfo.HasSameData(fill1.gradientFillInfo, fill2.gradientFillInfo) &&
		//        PatternFillInfo.HasSameData(fill1.patternFillInfo, fill2.patternFillInfo));
		//}

		//#endregion // HasSameData

		#endregion // Removed

        #endregion Methods
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