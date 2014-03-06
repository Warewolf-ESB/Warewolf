using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{





	// MD 3/4/12 - 12.1 - Table Support
    //internal class StopInfo
	internal sealed class StopInfo
    {
        #region Members

        private double position = -1.0;
        private ColorInfo colorInfo = null;

        #endregion Members

        #region Properties






        public double Position
        {
            get { return this.position; }
            set { this.position = value; }
        }






        public ColorInfo ColorInfo
        {
            get { return this.colorInfo; }
            set { this.colorInfo = value; }
        }

        #endregion Properties

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			StopInfo other = obj as StopInfo;
			if (other == null)
				return false;

			return
				Object.Equals(this.position, other.position) &&
				Object.Equals(this.colorInfo, other.colorInfo);
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = this.position.GetHashCode();
			if (this.colorInfo != null)
				hashCode ^= this.colorInfo.GetHashCode();

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Methods

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region CreateStopInfo

		internal static StopInfo CreateStopInfo(WorkbookSerializationManager manager, CellFillGradientStop stop)
		{
			StopInfo stopInfo = new StopInfo();
			stopInfo.ColorInfo = ColorInfo.CreateColorInfo(manager, stop.ColorInfo, ColorableItem.CellFill);
			stopInfo.Position = stop.Offset;
			return stopInfo;
		}

		#endregion // CreateStopInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(StopInfo stop1, StopInfo stop2)
		//{
		//    if (ReferenceEquals(stop1, null) &&
		//        ReferenceEquals(stop2, null))
		//        return true;
		//    if (ReferenceEquals(stop1, null) ||
		//        ReferenceEquals(stop2, null))
		//        return false;
		//    return (ColorInfo.HasSameData(stop1.colorInfo, stop2.colorInfo) &&
		//        stop1.position == stop2.position);
		//}

		//#endregion // HasSameData

		#endregion // Removed

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region ToCellFillGradientStop

		internal CellFillGradientStop ToCellFillGradientStop(WorkbookSerializationManager manager)
		{
			WorkbookColorInfo colorInfo = new WorkbookColorInfo(Utilities.ColorsInternal.Black);
			if (this.ColorInfo != null)
				colorInfo = this.ColorInfo.ResolveColorInfo(manager);

			return new CellFillGradientStop(colorInfo, this.Position);
		}

		#endregion // ToCellFillGradientStop

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