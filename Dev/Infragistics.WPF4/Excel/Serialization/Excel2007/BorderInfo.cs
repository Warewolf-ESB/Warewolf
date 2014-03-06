using System;
using System.Collections.Generic;
using System.Text;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





    internal class BorderInfo
    {
        #region Members

        private BorderStyleInfo left = new BorderStyleInfo();
        private BorderStyleInfo right = new BorderStyleInfo();
        private BorderStyleInfo top = new BorderStyleInfo();
        private BorderStyleInfo bottom = new BorderStyleInfo();
        private BorderStyleInfo diagonal = new BorderStyleInfo();
        private BorderStyleInfo horizontal = new BorderStyleInfo();
        private BorderStyleInfo vertical = new BorderStyleInfo();

        private bool diagonalDown = false;
        private bool diagonalUp = false;
        private bool outline = false;

        #endregion Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			BorderInfo other = obj as BorderInfo;
			if (other == null)
				return false;

			return
				this.diagonalDown == other.diagonalDown &&
				this.diagonalUp == other.diagonalUp &&
				this.outline == other.outline &&
				Object.Equals(this.bottom, other.bottom) &&
				Object.Equals(this.diagonal, other.diagonal) &&
				Object.Equals(this.horizontal, other.horizontal) &&
				Object.Equals(this.left, other.left) &&
				Object.Equals(this.right, other.right) &&
				Object.Equals(this.top, other.top) &&
				Object.Equals(this.vertical, other.vertical);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode =
				this.diagonalDown.GetHashCode() ^
				this.diagonalUp.GetHashCode() ^
				this.outline.GetHashCode();

			if (this.bottom != null)
				hashCode ^= this.bottom.GetHashCode();

			if (this.diagonal != null)
				hashCode ^= this.diagonal.GetHashCode();

			if (this.horizontal != null)
				hashCode ^= this.horizontal.GetHashCode();

			if (this.left != null)
				hashCode ^= this.left.GetHashCode();

			if (this.right != null)
				hashCode ^= this.right.GetHashCode();

			if (this.top != null)
				hashCode ^= this.top.GetHashCode();

			if (this.vertical != null)
				hashCode ^= this.vertical.GetHashCode();

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties






        public BorderStyleInfo Left
        {
            get { return this.left; }
            set { this.left = value; }
        }






        public BorderStyleInfo Right
        {
            get { return this.right; }
            set { this.right = value; }
        }






        public BorderStyleInfo Top
        {
            get { return this.top; }
            set { this.top = value; }
        }






        public BorderStyleInfo Bottom
        {
            get { return this.bottom; }
            set { this.bottom = value; }
        }






        public BorderStyleInfo Diagonal
        {
            get { return this.diagonal; }
            set { this.diagonal = value; }
        }






        public BorderStyleInfo Horizontal
        {
            get { return this.horizontal; }
            set { this.horizontal = value; }
        }






        public BorderStyleInfo Vertical
        {
            get { return this.vertical; }
            set { this.vertical = value; }
        }






        public bool DiagonalDown
        {
            get { return this.diagonalDown; }
            set { this.diagonalDown = value; }
        }






        public bool DiagonalUp
        {
            get { return this.diagonalUp; }
            set { this.diagonalUp = value; }
        }






        public bool Outline
        {
            get { return this.outline; }
            set { this.outline = value; }
        }

        #endregion Properties

        #region Methods

		// MD 12/21/11 - 12.1 - Table Support
		// Moved this code from the FormatInfo.CreateWorksheetCellFormatData so it can be used in other places.
		#region ApplyTo

		internal void ApplyTo(WorksheetCellFormatData formatData, WorkbookSerializationManager manager)
		{
			if (this.Bottom != null)
			{
				formatData.BottomBorderColorInfo = this.Bottom.ColorInfo.ResolveColorInfo(manager);
				formatData.BottomBorderStyle = this.Bottom.BorderStyle;
			}

			if (this.Top != null)
			{
				formatData.TopBorderColorInfo = this.Top.ColorInfo.ResolveColorInfo(manager);
				formatData.TopBorderStyle = this.Top.BorderStyle;
			}

			if (this.Left != null)
			{
				formatData.LeftBorderColorInfo = this.Left.ColorInfo.ResolveColorInfo(manager);
				formatData.LeftBorderStyle = this.Left.BorderStyle;
			}

			if (this.Right != null)
			{
				formatData.RightBorderColorInfo = this.Right.ColorInfo.ResolveColorInfo(manager);
				formatData.RightBorderStyle = this.Right.BorderStyle;
			}

			if (this.Diagonal != null)
			{
				formatData.DiagonalBorderColorInfo = this.Diagonal.ColorInfo.ResolveColorInfo(manager);
				formatData.DiagonalBorderStyle = this.Diagonal.BorderStyle;
			}

			DiagonalBorders diagonalBorders = DiagonalBorders.None;

			if (this.DiagonalDown)
				diagonalBorders |= DiagonalBorders.DiagonalDown;

			if (this.DiagonalUp)
				diagonalBorders |= DiagonalBorders.DiagonalUp;

			formatData.DiagonalBorders = diagonalBorders;
		}

		#endregion // ApplyTo

		// MD 12/30/11 - 12.1 - Table Support
		#region CreateBorderInfo

		public static BorderInfo CreateBorderInfo(WorkbookSerializationManager manager, WorksheetCellFormatData formatData)
		{
			BorderInfo borderInfo = new BorderInfo();

			BorderInfo.CreateBorderInfoHelper(manager, 
				borderInfo.Bottom,
				formatData.BottomBorderStyleResolved,
				formatData.BottomBorderColorInfo,
				formatData.BottomBorderColorInfoResolved);
			BorderInfo.CreateBorderInfoHelper(manager, 
				borderInfo.Top,
				formatData.TopBorderStyleResolved,
				formatData.TopBorderColorInfo,
				formatData.TopBorderColorInfoResolved);
			BorderInfo.CreateBorderInfoHelper(manager, 
				borderInfo.Left,
				formatData.LeftBorderStyleResolved,
				formatData.LeftBorderColorInfo,
				formatData.LeftBorderColorInfoResolved);
			BorderInfo.CreateBorderInfoHelper(manager, 
				borderInfo.Right,
				formatData.RightBorderStyleResolved,
				formatData.RightBorderColorInfo,
				formatData.RightBorderColorInfoResolved);

			BorderInfo.CreateBorderInfoHelper(manager, 
				borderInfo.Diagonal,
				formatData.DiagonalBorderStyleResolved,
				formatData.DiagonalBorderColorInfo,
				formatData.DiagonalBorderColorInfoResolved);

			DiagonalBorders diagonalBorders = formatData.DiagonalBordersResolved;
			if (Utilities.IsDiagonalDownSet(diagonalBorders))
				borderInfo.DiagonalDown = true;

			if (Utilities.IsDiagonalUpSet(diagonalBorders))
				borderInfo.DiagonalUp = true;

			return borderInfo;
		}

		private static void CreateBorderInfoHelper(WorkbookSerializationManager manager, BorderStyleInfo borderStyleInfo, 
			CellBorderLineStyle borderLineStyle, WorkbookColorInfo borderColorInfo, WorkbookColorInfo borderColorInfoResolved)
		{
			WorkbookColorInfo borderColorInfoToSave = (borderLineStyle == CellBorderLineStyle.None) ? borderColorInfo : borderColorInfoResolved;

			borderStyleInfo.BorderStyle = borderLineStyle;
			borderStyleInfo.ColorInfo = ColorInfo.CreateColorInfo(manager, borderColorInfoToSave, ColorableItem.CellBorder);
		}

		#endregion // CreateBorderInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(BorderInfo border1, BorderInfo border2)
		//{
		//    if (ReferenceEquals(border1, null) &&
		//        ReferenceEquals(border2, null))
		//        return true;
		//    if (ReferenceEquals(border1, null) ||
		//        ReferenceEquals(border2, null))
		//        return false;
		//    return (BorderStyleInfo.HasSameData(border1.bottom, border2.bottom) &&
		//        BorderStyleInfo.HasSameData(border1.diagonal, border2.diagonal) &&
		//        border1.diagonalDown == border2.diagonalDown &&
		//        border1.diagonalUp == border2.diagonalUp &&
		//        BorderStyleInfo.HasSameData(border1.horizontal, border2.horizontal) &&
		//        BorderStyleInfo.HasSameData(border1.left, border2.left) &&
		//        border1.outline == border2.outline &&
		//        BorderStyleInfo.HasSameData(border1.right, border2.right) &&
		//        BorderStyleInfo.HasSameData(border1.top, border2.top) &&
		//        BorderStyleInfo.HasSameData(border1.vertical, border2.vertical));
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