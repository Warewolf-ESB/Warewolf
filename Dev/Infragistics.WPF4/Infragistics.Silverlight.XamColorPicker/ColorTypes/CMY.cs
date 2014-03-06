using System;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// An internal struct representing a color using a CMY colorspace.
    /// </summary>
    internal struct CMY
    {
        #region Members
        private double _c;
        private double _m;
        private double _y;
        private byte _alpha;
        #endregion // Members

        #region Properties

        #region C

        /// <summary>
        /// Gets / sets the C value for a CMY color.  The expected values are between 0 and 100.
        /// </summary>        
        public double C
        {
            get
            {
                return this._c;
            }
            set
            {
                this._c = value;
            }
        }

        #endregion // C

        #region M

        /// <summary>
        /// Gets / sets the M value for a CMY color.  The expected values are between 0 and 100.
        /// </summary>
        public double M
        {
            get
            {
                return this._m;
            }
            set
            {
                this._m = value;
            }
        }

        #endregion // M

        #region Y

        /// <summary>
        /// Gets / sets the Y value for a CMY color.  The expected values are between 0 and 100.
        /// </summary>
        public double Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }

        #endregion // Y

        #region Alpha

        /// <summary>
        /// Gets / sets the Alpha value that will be used when this color is converted to a Color struct.
        /// </summary>
        public Byte Alpha
        {
            get
            {
                return this._alpha;
            }
            set
            {
                this._alpha = value;
            }
        }

        #endregion // Alpha

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CMY"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        private CMY(Byte red, Byte green, Byte blue, Byte alpha)
        {
            _c = Math.Round((1.0 - (red / 255.0)) * 100.0, 2);
            _m = Math.Round((1.0 - (green / 255.0)) * 100.0, 2);
            _y = Math.Round((1.0 - (blue / 255.0)) * 100.0, 2);
            _alpha = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMY"/> structure.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <param name="alpha"></param>
        private CMY(double c, double m, double y, byte alpha)
        {
            _c = Math.Round(c, 2);
            _m = Math.Round(m, 2);
            _y = Math.Round(y, 2);
            _alpha = alpha;
        }

        #endregion // Constructor

        #region Methods

        #region ToColor
        /// <summary>
        /// Returns a <see cref="Color"/> struct based on the values of the <see cref="CMY"/> color.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            double c = 100.0 - Math.Max(0, Math.Min(100, this.C));
            double m = 100.0 - Math.Max(0, Math.Min(100, this.M));
            double y = 100.0 - Math.Max(0, Math.Min(100, this.Y));
            Byte red = Convert.ToByte((c / 100.0) * 255);
            Byte green = Convert.ToByte((m / 100.0) * 255);
            Byte blue = Convert.ToByte((y / 100.0) * 255);
            return Color.FromArgb(this.Alpha, red, green, blue);
        }
        #endregion // ToColor

        #region ToCMYK
        /// <summary>
        /// Returns a <see cref="CMYK"/> struct based on the values of the <see cref="CMY"/> color.
        /// </summary>
        /// <returns></returns>
        public CMYK ToCMYK()
        {
            double k = 100.0;

            if (this.C < k)
                k = this.C;
            if (this.M < k)
                k = this.M;
            if (this.Y < k)
                k = this.Y;

            if (k == 100.0)
            {
                return CMYK.FromCMYK(0, 0, 0, k, this.Alpha);
            }

            double denom = 100.0 - k;
            double c = (this.C - k) / denom;
            double m = (this.M - k) / denom;
            double y = (this.Y - k) / denom;

            return CMYK.FromCMYK(c * 100.0, m * 100.0, y * 100.0, k, this.Alpha);

        }
        #endregion // ToCMYK

        #region FromColor
        /// <summary>
        /// Initializes a new instance of the <see cref="CMY"/> structure.
        /// </summary>
        /// <param name="c"></param>
        public static CMY FromColor(Color c)
        {
            return new CMY(c.R, c.G, c.B, c.A);
        }

        #endregion // FromColor

        #region FromARGB
        /// <summary>
        /// Initializes a new instance of the <see cref="CMY"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public static CMY FromARGB(Byte red, Byte green, Byte blue, Byte alpha)
        {
            return new CMY(red, green, blue, alpha);
        }
        #endregion // FromARGB

        #region FromCMY

        /// <summary>
        /// Initializes a new instance of the <see cref="CMY"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        public static CMY FromCMY(double c, double m, double y, byte alpha)
        {
            return new CMY(c, m, y, alpha);
        }
        #endregion // FromCMY

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