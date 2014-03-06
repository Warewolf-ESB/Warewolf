using System;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// An struct representing a color using a HSB colorspace.
    /// </summary>
    public struct HSB
    {
        #region Members
        double _h;
        double _s;
        double _b;
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
        /// Gets / sets the B value for a HSB color.  The expected values are between 0 and 100.
        /// </summary>
        public double B
        {
            get
            {
                return this._b;
            }
            set
            {
                this._b = value;
            }
        }
        #endregion // L

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HSB"/> structure.
        /// </summary>
        /// <param name="c"></param>
        private HSB(Color c)
            : this(0, 0, 0, 0)
        {
            this._alpha = c.A;

            double red = c.R / 255.0;
            double blue = c.B / 255.0;
            double green = c.G / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));

            double deltaMaxMin = max - min;

            this._b = max;

            if (deltaMaxMin == 0)
            {
                this._h = 0;
                this._s = 0;
            }
            else
            {
                this._s = deltaMaxMin / max;


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
            this._b *= 100.0;
            this._s *= 100.0;

            this._h = Math.Round(this._h, 2);
            this._b = Math.Round(this._b, 2);
            this._s = Math.Round(this._s, 2);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="hue"></param>
        /// <param name="lightness"></param>
        /// <param name="saturation"></param>
        private HSB(double hue, double saturation, double lightness, byte alpha)
        {
            this._h = Math.Round(hue, 2);
            this._s = Math.Round(saturation, 2);
            this._b = Math.Round(lightness, 2);
            this._alpha = alpha;
        }

        #endregion // Constructor

        #region ToColor

        /// <summary>
        /// Converts the HSB to an RGB color.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            double h = this._h / 360.0;
            double s = this._s / 100.0;
            double b = this._b / 100.0;

            if (s == 0)
            {
                return Color.FromArgb(this._alpha, (byte)(b * 255), (byte)(b * 255), (byte)(b * 255));
            }

            double hue = h * 6;

            if (hue == 6)
                hue = 0;

            int intHue = (int)Math.Floor(hue);

            double x = b * (1 - s);
            double y = b * (1 - s * (hue - intHue));
            double z = b * (1 - s * (1 - (hue - intHue)));

            double red = 0, blue = 0, green = 0;
            switch (intHue)
            {
                case (0):
                    {
                        red = b;
                        green = z;
                        blue = x;
                        break;
                    }
                case (1):
                    {
                        red = y;
                        green = b;
                        blue = x;
                        break;
                    }
                case (2):
                    {
                        red = x;
                        green = b;
                        blue = z;
                        break;
                    }
                case (3):
                    {
                        red = x;
                        green = y;
                        blue = b;
                        break;
                    }
                case (4):
                    {
                        red = z;
                        green = x;
                        blue = b;
                        break;
                    }
                case (5):
                    {
                        red = b;
                        green = x;
                        blue = y;
                        break;
                    }
            }

            Color c = Color.FromArgb(this.Alpha, (byte)(255 * red), (byte)(255 * green), (byte)(255 * blue));

            return c;
        }

        #endregion // ToColor

        #region FromColor

        /// <summary>
        /// Initializes a new instance of the <see cref="HSL"/> structure.
        /// </summary>
        /// <param name="c"></param>
        public static HSB FromColor(Color c)
        {
            return new HSB(c);
        }
        #endregion // FromColor
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