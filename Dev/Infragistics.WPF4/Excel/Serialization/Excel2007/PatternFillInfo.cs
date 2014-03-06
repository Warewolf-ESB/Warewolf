using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class PatternFillInfo
    {
        #region Members

        private ColorInfo bgColor = null;
        private ColorInfo fgColor = null;
        private FillPatternStyle? patternStyle;

        #endregion Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			PatternFillInfo other = obj as PatternFillInfo;
			if (other == null)
				return false;

			return
				this.patternStyle == other.patternStyle &&
				Object.Equals(this.bgColor, other.bgColor) &&
				Object.Equals(this.fgColor, other.fgColor);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = this.patternStyle.GetHashCode();

			if (this.bgColor != null)
				hashCode ^= this.bgColor.GetHashCode();

			if (this.fgColor != null)
				hashCode ^= this.fgColor.GetHashCode();

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties






        public ColorInfo BackgroundColor
        {
            get { return this.bgColor; }
            set { this.bgColor = value; }
        }






        public ColorInfo ForegroundColor
        {
            get { return this.fgColor; }
            set { this.fgColor = value; }
        }






        public FillPatternStyle? PatternStyle
        {
            get { return this.patternStyle; }
            set { this.patternStyle = value; }
        }

        #endregion Properties

        #region Methods

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region CreatePatternFillInfo

		internal static PatternFillInfo CreatePatternFillInfo(WorkbookSerializationManager manager, WorksheetCellFormatData formatData, CellFillPattern patternFill)
		{
			PatternFillInfo patternFillInfo = new PatternFillInfo();
			patternFillInfo.PatternStyle = patternFill.PatternStyle;

			// MD 5/7/12
			// Found while fixing TFS106831
			// For the None pattern, it should not have the child elements.
			if (patternFill.PatternStyle == FillPatternStyle.None)
				return patternFillInfo;

			WorkbookColorInfo foregroundColorInfo = patternFill.GetFileFormatForegroundColorInfo(formatData);
			if (foregroundColorInfo != WorkbookColorInfo.Automatic)
				patternFillInfo.ForegroundColor = ColorInfo.CreateColorInfo(manager, foregroundColorInfo, ColorableItem.CellFill);

			WorkbookColorInfo backgroundColorInfo = patternFill.GetFileFormatBackgroundColorInfo(formatData);
			if (backgroundColorInfo != new WorkbookColorInfo(Utilities.SystemColorsInternal.WindowColor))
				patternFillInfo.BackgroundColor = ColorInfo.CreateColorInfo(manager, backgroundColorInfo, ColorableItem.CellFill);

			return patternFillInfo;
		}

		#endregion // CreatePatternFillInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(PatternFillInfo fill1, PatternFillInfo fill2)
		//{
		//    if (ReferenceEquals(fill1, null) &&
		//        ReferenceEquals(fill2, null))
		//        return true;
		//    if (ReferenceEquals(fill1, null) ||
		//        ReferenceEquals(fill2, null))
		//        return false;
		//    return (ColorInfo.HasSameData(fill1.bgColor, fill2.bgColor) &&
		//        ColorInfo.HasSameData(fill1.fgColor, fill2.fgColor) &&
		//        fill1.patternStyle == fill2.patternStyle);
		//}

		//#endregion // HasSameData

		#endregion // Removed

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region ToCellFill

		internal CellFillPattern ToCellFill(WorkbookSerializationManager manager, WorksheetCellFormatData format)
		{
			FillPatternStyle patternStyle = FillPatternStyle.None;

			WorkbookColorInfo backgroundColorInfo = new WorkbookColorInfo(Utilities.SystemColorsInternal.WindowColor);
			if (this.BackgroundColor != null)
			{
				patternStyle = FillPatternStyle.Solid;
				backgroundColorInfo = this.BackgroundColor.ResolveColorInfo(manager);
			}

			WorkbookColorInfo foregroundColorInfo = WorkbookColorInfo.Automatic;
			if (this.ForegroundColor != null)
			{
				patternStyle = FillPatternStyle.Solid;
				foregroundColorInfo = this.ForegroundColor.ResolveColorInfo(manager);
			}

			if (this.PatternStyle.HasValue)
				patternStyle = this.PatternStyle.Value;

			return new CellFillPattern(backgroundColorInfo, foregroundColorInfo, patternStyle, format);
		} 

		#endregion // ToCellFill

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