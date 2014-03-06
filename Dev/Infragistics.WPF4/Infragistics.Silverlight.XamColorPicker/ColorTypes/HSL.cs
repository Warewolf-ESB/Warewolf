using System;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// An struct representing a color using a HSL colorspace.
    /// </summary>
    public struct HSL
    {
        #region Members
        double _h;
        double _s;
        double _l;
        byte _alpha;
        #endregion // Members

        #region Properties

        #region Alpha
        /// <summary>
        /// Gets / sets the Alpha value that will be used when this color is converted to a Color struct.
        /// </summary>
        public byte Alpha
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

        #region H
        /// <summary>
        /// Gets / sets the H value for a HSL color.  The expected values are between 0 and 360.
        /// </summary>
        public double H
        {
            get
            {
                return this._h;
            }
            set
            {
                this._h = value;
            }
        }
        #endregion // H

        #region S
        /// <summary>
        /// Gets / sets the S value for a HSL color.  The expected values are between 0 and 100.
        /// </summary>
        public double S
        {
            get
            {
                return this._s;
            }
            set
            {
                this._s = value;
            }
        }
        #endregion // S

        #region L
        /// <summary>
        /// Gets / sets the L value for a HSL color.  The expected values are between 0 and 100.
        /// </summary>
        public double L
        {
            get
            {
                return this._l;
            }
            set
            {
                this._l = value;
            }
        }
        #endregion // L

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="c"></param>
        private HSL(Color c)
            : this(0, 0, 0, 0)
        {
            this._alpha = c.A;

            double red = c.R / 255.0;
            double blue = c.B / 255.0;
            double green = c.G / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));

            double deltaMaxMin = max - min;

            this._l = (max + min) / 2;

            if (deltaMaxMin == 0)
            {
                this._h = 0;
                this._s = 0;
            }
            else
            {
                if (this._l < 0.5)
                    this._s = deltaMaxMin / (max + min);
                else
                    this._s = deltaMaxMin / (2 - max - min);

                double deltaRed = (((max - red) / 6) + (deltaMaxMin / 2)) / deltaMaxMin;
                double deltaGreen = (((max - green) / 6) + (deltaMaxMin / 2)) / deltaMaxMin;
                double deltaBlue = (((max - blue) / 6) + (deltaMaxMin / 2)) / deltaMaxMin;

                if (red == max)
                {
                    this._h = deltaBlue - deltaGreen;
                }
                else if (green == max)
                {
                    this._h = (1.0 / 3) + deltaRed - deltaBlue;
                }
                else if (blue == max)
                {
                    this._h = (2.0 / 3) + deltaGreen - deltaRed;
                }

                if (this._h < 0)
                    this._h += 1;
                if (this._h > 1)
                    this._h -= 1;
            }

            this._h *= 360.0;
            this._l *= 100.0;
            this._s *= 100.0;

            this._h = Math.Round(this._h, 2);
            this._l = Math.Round(this._l, 2);
            this._s = Math.Round(this._s, 2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="hue"></param>
        /// <param name="lightness"></param>
        /// <param name="saturation"></param>
        private HSL(double hue, double saturation, double lightness, byte alpha)
        {
            this._h = Math.Round(hue, 2);
            this._s = Math.Round(saturation, 2);
            this._l = Math.Round(lightness, 2);
            this._alpha = alpha;
        }

        #endregion // Constructor

        #region Methods

        #region ToColor
        /// <summary>
        /// Returns a <see cref="Color"/> struct based on the values of the <see cref="HSL"/> color.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            double l = this.L / 100.0;
            double s = this.S / 100.0;

            if (this.S == 0)
            {
                Byte b = Convert.ToByte(Math.Max(Math.Min(255, l * 255), 0));
                return Color.FromArgb(this.Alpha, b, b, b);
            }

            double temp2 = 0.0;

            if (l < 0.5)
            {
                temp2 = l * (1 + s);
            }
            else
            {
                temp2 = (s + l) - (s * l);
            }

            double temp1 = (2 * l) - temp2;

            double hue = this.H / 360.0;

            double red = 255 * ConvertHue(temp1, temp2, (hue + (1.0 / 3)));
            double green = 255 * ConvertHue(temp1, temp2, hue);
            double blue = 255 * ConvertHue(temp1, temp2, (hue - (1.0 / 3)));

            red = Math.Max(Math.Min(255, red), 0);
            green = Math.Max(Math.Min(255, green), 0);
            blue = Math.Max(Math.Min(255, blue), 0);

            return Color.FromArgb(this._alpha, Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));
        }
        #endregion // ToColor

        #region ConvertHue
        /// <summary>
        /// Shifts the hue of a color
        /// </summary>
        /// <param name="temp1"></param>
        /// <param name="temp2"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private double ConvertHue(double temp1, double temp2, double p)
        {
            if (p < 0)
                p += 1;
            if (p > 1)
                p -= 1;

            if (6 * p < 1)
                return temp1 + (temp2 - temp1) * 6 * p;

            if (2 * p < 1)
                return temp2;

            if (3 * p < 2)
                return temp1 + (temp2 - temp1) * ((0.666666666 - p) * 6);

            return temp1;
        }

        #endregion // ConvertHue

        #region Darker

        /// <summary>
        /// Shifts the lightness of the color based on the inputted value.
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public HSL Darker(double percent)
        {
            double lightness = this.L;

            if (percent < 0)
            {
                lightness -= (100.0 - lightness) * (percent / 100.0);
            }
            else
            {
                lightness -= (lightness * percent) / 100.0;
            }
            return new HSL(this.H, this.S, lightness, this.Alpha);
        }

        #endregion // Darker

        #region Lighter

        /// <summary>
        /// Shifts the lightness of the color based on the inputted value.
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public HSL Lighter(double percent)
        {
            double lightness = this.L;

            if (percent < 0)
            {
                lightness += (lightness * percent) / 100.0;
            }
            else
            {
                lightness += (100.0 - lightness) * (percent / 100.0);
            }
            return new HSL(this.H, this.S, lightness, 255);
        }

        #endregion // Lighter

        #region FromColor

        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="c"></param>
        public static HSL FromColor(Color c)
        {
            return new HSL(c);
        }
        #endregion // FromColor

        #region FromHSL
        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="hue"></param>
        /// <param name="lightness"></param>
        /// <param name="saturation"></param>
        public static HSL FromHSL(double hue, double saturation, double lightness, byte alpha)
        {
            return new HSL(hue, saturation, lightness, alpha);
        }

        #endregion // FromHSL


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