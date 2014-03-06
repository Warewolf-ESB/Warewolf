using System;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// An struct representing a color using a CMYK colorspace.
    /// </summary>
    public struct CMYK
    {
        #region Members
        private double _c;
        private double _m;
        private double _y;
        private double _k;
        private byte _alpha;
        #endregion // Members

        #region Properties

        #region C

        /// <summary>
        /// Gets / sets the C value for a CMYK color.  The expected values are between 0 and 100.
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
        /// Gets / sets the M value for a CMYK color.  The expected values are between 0 and 100.
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
        /// Gets / sets the Y value for a CMYK color.  The expected values are between 0 and 100.
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

        #region K

        /// <summary>
        /// Gets / sets the K value for a CMYK color.  The expected values are between 0 and 100.
        /// </summary>        
        public double K
        {
            get
            {
                return this._k;
            }
            set
            {
                this._k = value;
            }
        }

        #endregion // K

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
        /// Initializes a new instance of the <see cref="CMYK"/> structure.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <param name="k"></param>
        /// <param name="alpha"></param>
        private CMYK(double c, double m, double y, double k, byte alpha)
        {
            _c = Math.Round(c, 2);
            _m = Math.Round(m, 2);
            _y = Math.Round(y, 2);
            _k = Math.Round(k, 2);
            _alpha = alpha;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CMYK"/> structure.
        /// </summary>
        /// <param name="c"></param>
        private CMYK(Color c)
        {
            CMY color = CMY.FromColor(c);

            CMYK converted = color.ToCMYK();

            _c = converted.C;
            _m = converted.M;
            _y = converted.Y;
            _k = converted.K;
            _alpha = converted.Alpha;
        }

        #endregion // Constructor

        #region Methods

        #region ToCMY

        /// <summary>
        /// Returns a <see cref="CMY"/> struct based on the values of the <see cref="CMY"/> color.
        /// </summary>
        /// <returns></returns>
        internal CMY ToCMY()
        {
            double c = this.C * (100.0 - this.K) / 100 + K;
            double m = this.M * (100.0 - this.K) / 100 + K;
            double y = this.Y * (100.0 - this.K) / 100 + K;

            return CMY.FromCMY(c, m, y, this.Alpha);
        }

        #endregion // ToCMY

        #region ToColor
        /// <summary>
        /// Returns a <see cref="Color"/> struct based on the values of the <see cref="CMYK"/> color.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            return this.ToCMY().ToColor();
        }
        #endregion // ToColor

        #region FromColor
        /// <summary>
        /// Initializes a new instance of the <see cref="CMYK"/> structure.
        /// </summary>
        /// <param name="c"></param>
        public static CMYK FromColor(Color c)
        {
            return new CMYK(c);
        }

        #endregion // FromColor

        #region FromCMYK
        /// <summary>
        /// Initializes a new instance of the <see cref="CMYK"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <param name="k"></param>
        public static CMYK FromCMYK(double c, double m, double y, double k, byte alpha)
        {
            return new CMYK(c, m, y, k, alpha);
        }
        #endregion // FromCMYK

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