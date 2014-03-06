using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
    internal class GradientFillInfo
    {
        #region Constants

        public const double DefaultValueBottom = 0;
        public const double DefaultValueTop = 0;
        public const double DefaultValueRight = 0;
        public const double DefaultValueLeft = 0;
        public const double DefaultValueDegree = 0;
        public const ST_GradientType DefaultFillType = ST_GradientType.linear;

        #endregion Contstants

        #region Members

        private List<StopInfo> stops = new List<StopInfo>();
        private double bottom = GradientFillInfo.DefaultValueBottom;
        private double top = GradientFillInfo.DefaultValueTop;
        private double right = GradientFillInfo.DefaultValueRight;
        private double left = GradientFillInfo.DefaultValueLeft;
        private double degree = GradientFillInfo.DefaultValueDegree;
        private ST_GradientType fillType = GradientFillInfo.DefaultFillType;

        #endregion Members

		#region Base Class Overrides

		// MD 3/4/12 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			GradientFillInfo other = obj as GradientFillInfo;
			if (other == null)
				return false;

			// MD 5/4/12 - TFS110608
			// This looks like a copy-paste error. The equality check here was incorrect.
			//if (this.bottom == other.bottom &&
			//    this.degree == other.degree &&
			//    this.fillType == other.fillType &&
			//    this.left == other.left &&
			//    this.right == other.right &&
			//    this.top == other.top &&
			//    this.stops.Count != other.stops.Count)
			//    return false;
			if (this.bottom != other.bottom ||
				this.degree != other.degree ||
				this.fillType != other.fillType ||
				this.left != other.left ||
				this.right != other.right ||
				this.top != other.top ||
				this.stops.Count != other.stops.Count)
				return false;

			for (int i = 0; i < this.stops.Count; i++)
			{
				if (Object.Equals(this.stops[i], other.stops[i]) == false)
					return false;
			}

			return true;
		}

		#endregion // Equals

		// MD 3/4/12 - 12.1 - Table Support
		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode =
				this.bottom.GetHashCode() ^
				this.top.GetHashCode() ^
				this.right.GetHashCode() ^
				this.left.GetHashCode() ^
				this.degree.GetHashCode() ^
				this.fillType.GetHashCode() ^
				this.stops.Count;

			if (this.stops.Count != 0)
			{
				hashCode ^= this.stops[0].GetHashCode();
				hashCode ^= this.stops[this.stops.Count - 1].GetHashCode();
			}

			return hashCode;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

        #region Properties






        public List<StopInfo> Stops
        {
            get { return this.stops; }
        }







        public double Bottom
        {
            get { return this.bottom; }
            set { this.bottom = value; }
        }






        public double Top
        {
            get { return this.top; }
            set { this.top = value; }
        }







        public double Left
        {
            get { return this.left; }
            set { this.left = value; }
        }







        public double Right
        {
            get { return this.right; }
            set { this.right = value; }
        }



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        public double Degree
        {
            get { return this.degree; }
            set { this.degree = value; }
        }






        public ST_GradientType FillType
        {
            get { return this.fillType; }
            set { this.fillType = value; }
        }

        #endregion Properties

        #region Methods

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region CreateGradientFillInfo

		internal static GradientFillInfo CreateGradientFillInfo(WorkbookSerializationManager manager, CellFillGradient gradientFill)
		{
			GradientFillInfo gradientFillInfo = new GradientFillInfo();

			for (int i = 0; i < gradientFill.Stops.Count; i++)
				gradientFillInfo.Stops.Add(StopInfo.CreateStopInfo(manager, gradientFill.Stops[i]));

			CellFillLinearGradient linearGradient = gradientFill as CellFillLinearGradient;
			CellFillRectangularGradient rectangularGradient = gradientFill as CellFillRectangularGradient;
			if (linearGradient != null)
			{
				gradientFillInfo.FillType = ST_GradientType.linear;
				gradientFillInfo.Degree = linearGradient.Angle;
			}
			else if (rectangularGradient != null)
			{
				gradientFillInfo.FillType = ST_GradientType.path;
				gradientFillInfo.Bottom = rectangularGradient.Bottom;
				gradientFillInfo.Left = rectangularGradient.Left;
				gradientFillInfo.Right = rectangularGradient.Right;
				gradientFillInfo.Top = rectangularGradient.Top;
			}
			else
			{
				Utilities.DebugFail("Unknown cell gradient fill.");
			}

			return gradientFillInfo;
		}

		#endregion // CreateGradientFillInfo

		// MD 3/4/12 - 12.1 - Table Support
		// Removed and replaced with Equals and GetHashCode overrides.
		#region Removed

		//#region HasSameData

		//internal static bool HasSameData(GradientFillInfo fill1, GradientFillInfo fill2)
		//{
		//    if (ReferenceEquals(fill1, null) &&
		//        ReferenceEquals(fill2, null))
		//        return true;
		//    if (ReferenceEquals(fill1, null) ||
		//        ReferenceEquals(fill2, null))
		//        return false;

		//    if (fill1.stops.Count != fill2.stops.Count)
		//        return false;

		//    for (int i = 0; i < fill2.stops.Count; i++)
		//    {
		//        if (!StopInfo.HasSameData(fill1.stops[i], fill2.stops[i]))
		//            return false;
		//    }

		//    return (fill1.bottom == fill2.bottom &&
		//        fill1.degree == fill2.degree &&
		//        fill1.fillType == fill2.fillType &&
		//        fill1.left == fill2.left &&
		//        fill1.right == fill2.right &&
		//        fill1.top == fill2.top);
		//}

		//#endregion // HasSameData

		#endregion // Removed

		// MD 1/24/12 - 12.1 - Cell Format Updates
		#region ToCellFill

		internal CellFill ToCellFill(WorkbookSerializationManager manager)
		{
			CellFillGradientStop[] gradientStops = new CellFillGradientStop[this.Stops.Count];
			for (int i = 0; i < gradientStops.Length; i++)
				gradientStops[i] = this.Stops[i].ToCellFillGradientStop(manager);

			switch (this.FillType)
			{
				case ST_GradientType.linear:
					return CellFill.CreateLinearGradientFill(this.Degree, gradientStops);

				case ST_GradientType.path:
					return CellFill.CreateRectangularGradientFill(this.Left, this.Top, this.Right, this.Bottom, gradientStops);

				default:
					Utilities.DebugFail("Unknown gradient fill: " + this.FillType);
					return null;
			}
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