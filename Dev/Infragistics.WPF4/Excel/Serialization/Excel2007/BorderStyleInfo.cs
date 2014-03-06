using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{






    internal class BorderStyleInfo
    {
        #region Members

        private ColorInfo colorInfo = null;

		// MD 4/4/12 - TFS107655
		// Use the actual default instead of Excel's default. The style should only be None if it is specified.
        //private CellBorderLineStyle style = CellBorderLineStyle.None;
		private CellBorderLineStyle style = CellBorderLineStyle.Default;

        #endregion Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			BorderStyleInfo other = obj as BorderStyleInfo;
			if (other == null)
				return false;

			return
				this.style == other.style &&
				Object.Equals(this.colorInfo, other.colorInfo);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = style.GetHashCode();
			if (this.colorInfo != null)
				hashCode ^= this.colorInfo.GetHashCode();

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties






        public ColorInfo ColorInfo
        {
            get
            {
                if (this.colorInfo == null)
                    this.colorInfo = new ColorInfo();
                return this.colorInfo;
            }
            set
            {
                this.colorInfo = value;
            }
        }






        public CellBorderLineStyle BorderStyle
        {
            get { return this.style; }
            set { this.style = value; }
        }

        #endregion Properties

		#region Removed

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		//#region Methods

		//internal static bool HasSameData(BorderStyleInfo border1, BorderStyleInfo border2)
		//{
		//    if (ReferenceEquals(border1, null) &&
		//        ReferenceEquals(border2, null))
		//        return true;
		//    if (ReferenceEquals(border1, null) ||
		//        ReferenceEquals(border2, null))
		//        return false;
		//    return (border1.style == border2.style &&
		//        ColorInfo.HasSameData(border1.colorInfo, border2.colorInfo));
		//}

		//#endregion Methods

		#endregion // Removed
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