using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace Infragistics
{
    /// <summary>
    /// Defines the mode for color interpolation.
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// Interpolation in RGB space.
        /// </summary>
        RGB,

        /// <summary>
        /// Interpolation in HSV space.
        /// </summary>
        HSV
    }

    /// <summary>
    /// Utility class for color-based operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class ColorUtil
    {
        private static Random R = new Random();
        /// <summary>
        /// Returns a random color.
        /// </summary>
        /// <param name="alpha">The alpha level of the color to generate.</param>
        /// <returns>A random color with the specified alpha level.</returns>
        public static Color RandomColor(byte alpha)
        {
            return Color.FromArgb(
                alpha,
                (byte)ColorUtil.R.Next(0, 255),
                (byte)ColorUtil.R.Next(0, 255),
                (byte)ColorUtil.R.Next(0, 255));
        }

        /// <summary>
        /// Get a random color  
        /// </summary>
        /// <param name="color">Specifies the alpha, saturation and value for the returned color</param>
        /// <returns>New color</returns>
        public static Color RandomHue(Color color)
        {
            double[] ahsv = color.GetAHSV();

            return FromAHSV(ahsv[0], (double)ColorUtil.R.Next(0, 359), ahsv[2], ahsv[3]);
        }

        /// <summary>
        /// Get an interpolation from the current color to the specified color
        /// </summary>
        /// <param name="minimum">begin color, corresponding to p=0.0</param>
        /// <param name="interpolation_">interpolation parameter assumed to be in [0.0, 1.0]</param>
        /// <param name="maximum_">end color, corresponding to p=1.0</param>
        /// <param name="interpolationMode">Interpolation mode to use.</param>
        /// <returns>new color corresponding to the specified interpolation parameter</returns>
        /// <remarks>
        /// There are always two interpolation paths for the hue, and this function
        /// chooses the shortest one, so for example an interpolation from red to 
        /// blue runs through purple, not orange, yellow and green.
        /// </remarks>
        public static Color GetInterpolation(this Color minimum, double interpolation_, Color maximum_, InterpolationMode interpolationMode)
        {

            var min_ = minimum;

            switch(interpolationMode)
            {
                case InterpolationMode.HSV:
                    {
                        double[] b=minimum.GetAHSV();
                        double[] e=maximum_.GetAHSV();

                        double b1 = b[1] >= 0 ? b[1] : e[1];
                        double e1 = e[1] >= 0 ? e[1] : b[1];

                        if (b1 >= 0 && e1 >= 0 && System.Math.Abs(e1 - b1) > 180)
                        {
                            if (e1 > b1)
                            {
                                b1 += 360;
                            }
                            else
                            {
                                e1 += 360;
                            }
                        }

                        interpolation_ = System.Math.Max(0.0, System.Math.Min(1.0, interpolation_));
                        
                        return FromAHSV(b[0] + interpolation_ * (e[0] - b[0]), 
                                        b1 + interpolation_ * (e1 - b1),
                                        b[2] + interpolation_ * (e[2] - b[2]),
                                        b[3] + interpolation_ * (e[3] - b[3]));
                    }

                case InterpolationMode.RGB: 


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    return Color.FromArgb(  (byte)(minimum.A+interpolation_*(maximum_.A-minimum.A)),
                                            (byte)(minimum.R+interpolation_*(maximum_.R-minimum.R)),
                                            (byte)(minimum.G+interpolation_*(maximum_.G-minimum.G)), 
                                            (byte)(minimum.B+interpolation_*(maximum_.B-minimum.B)));

            }

            return minimum;
        }

        /// <summary>
        /// Get an interpolated color between two colors.
        /// </summary>
        /// <param name="minimum">begin color, corresponding to p=0.0</param>
        /// <param name="interpolation">interpolation parameter assumed to be in [0.0, 1.0]</param>
        /// <param name="maximum">end color, corresponding to p=1.0</param>
        /// <returns>new color corresponding to the specified interpolation parameter</returns>
        /// <remarks>
        /// There are always two interpolation paths for the hue, and this function
        /// chooses the shortest one, so for example an interpolation from red to 
        /// blue runs through purple, not orange, yellow and green.
        /// </remarks>
        public static Color GetAHSVInterpolation(double[] minimum, double interpolation, double[] maximum)
        {
            double b1 = minimum[1] >= 0 ? minimum[1] : maximum[1];
            double e1 = maximum[1] >= 0 ? maximum[1] : minimum[1];

            if (b1 >= 0 && e1 >= 0 && Math.Abs(e1 - b1) > 180)
            {
                if (e1 > b1)
                {
                    b1 += 360;
                }
                else
                {
                    e1 += 360;
                }
            }

            interpolation = Math.Max(0.0, Math.Min(1.0, interpolation));

            return FromAHSV(minimum[0] + interpolation * (maximum[0] - minimum[0]), b1 + interpolation * (e1 - b1), minimum[2] + interpolation * (maximum[2] - minimum[2]), minimum[3] + interpolation * (maximum[3] - minimum[3]));
        }

        /// <summary>
        /// Gets a new color corresponding to this color darkened or lightened by specified amount
        /// </summary>
        /// <param name="color"></param>
        /// <param name="interpolation">-1.0 for full darkening, to 1.0 for full lightening</param>
        /// <returns>new Color</returns>
        public static Color GetLightened(this Color color, double interpolation)
        {
            double[] ahsl = color.GetAHSL();

            if (interpolation < 0.0)
            {
                return FromAHSL(ahsl[0], ahsl[1], ahsl[2], ahsl[3] * (1.0 - MathUtil.Clamp(-interpolation, 0.0, 1.0)));
            }
            else
            {
                return FromAHSL(ahsl[0], ahsl[1], ahsl[2], ahsl[3] + MathUtil.Clamp(interpolation, 0.0, 1.0) * (1.0 - ahsl[3]));
            }
        }

        /// <summary>
        /// Gets the ahsl components of this color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static double[] GetAHSL(this Color color)
        {
            double[] ahsl = new double[4];

            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;

            ahsl[0] = color.A / 255.0;
            ahsl[3] = (max + min) / 2.0;

            if (delta == 0)
            {
                ahsl[1] = -1;
                ahsl[2] = 0;
            }
            else
            {
                ahsl[1] = H(max, delta, r, g, b);
                ahsl[2] = ahsl[3] < 0.5 ? delta / (max + min) : delta / (2 - max - min);
            }

            return ahsl;
        }

        /// <summary>
        /// Gets the ahsv components of this color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static double[] GetAHSV(this Color color)
        {
            double a = color.A / 255.0;
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double min = Math.Min(r, System.Math.Min(g, b));
            double max = Math.Max(r, System.Math.Max(g, b));
            double delta = max - min;

            double[] ahsv = new double[4];

            ahsv[0] = a;
            ahsv[3] = max;

            if (delta == 0)
            {
                ahsv[1] = -1;
                ahsv[2] = 0;
            }
            else
            {
                ahsv[1] = H(max, delta, r, g, b);
                ahsv[2] = delta / max;
            }

            return ahsv;
        }

        /// <summary>
        /// Gets a color from the specified ahsl components 
        /// </summary>
        /// <param name="alpha">The alpha (transparency), expressed as a value between 0 and 1, where 0 is transparent and 1 is opaque.</param>
        /// <param name="hue">The hue (color), expressed as a value between 0 and 1, where 0 is red, 1 is also red, and in between are orange, yellow, green, blue, indigo, and violet.</param>
        /// <param name="saturation">The saturation (colorfulness), expressed as a value between 0 and 1, where 0 is gray and 1 is fully saturated/colorful.</param>
        /// <param name="lightness">The lightness, expressed as a value between 0 and 1, where 0 is black and 1 is white.</param>
        public static Color FromAHSL(double alpha, double hue, double saturation, double lightness)
        {
            double r;
            double g;
            double b;

            if (saturation == 0)                       // HSL values = From 0 to 1
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q = lightness < 0.5 ? lightness * (1 + saturation) : lightness + saturation - (lightness * saturation);
                double p = 2 * lightness - q;
                double hk = hue / 360.0;

                r = C(p, q, hk + 1.0 / 3.0);
                g = C(p, q, hk);
                b = C(p, q, hk - 1.0 / 3.0);
            }

            return Color.FromArgb((byte)(alpha * 255.0), (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
        }

        /// <summary>
        /// Gets a color from the specified ahsv components
        /// </summary>
        public static Color FromAHSV(double alpha, double hue, double saturation, double value)
        {
            double r;
            double g;
            double b;

            while (hue >= 360)
            {
                hue -= 360.0;
            }

            if (saturation == 0)
            {
                r = value;
                g = value;
                b = value;
            }
            else
            {
                hue /= 60.0;

                double i = System.Math.Floor(hue);
                double f = hue - i;
                double p = value * (1 - saturation);
                double q = value * (1 - saturation * f);
                double t = value * (1 - saturation * (1 - f));

                switch ((int)i)
                {
                    case 0: r = value; g = t; b = p; break;
                    case 1: r = q; g = value; b = p; break;
                    case 2: r = p; g = value; b = t; break;
                    case 3: r = p; g = q; b = value; break;
                    case 4: r = t; g = p; b = value; break;
                    default: r = value; g = p; b = q; break;
                }
            }

            return Color.FromArgb((byte)(alpha * 255.0), (byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
        }

        private static double H(double max, double delta, double r, double g, double b)
        {
            double h = r == max ? (g - b) / delta :
                        g == max ? 2 + (b - r) / delta :
                        4 + (r - g) / delta;

            h *= 60.0;

            if (h < 0.0)
            {
                h += 360.0;
            }

            return h;
        }
        private static double C(double p, double q, double t)
        {
            t = t < 0 ? t + 1.0 : t > 1 ? t - 1.0 : t;

            if (t < 1.0 / 6.0)
            {
                return p + ((q - p) * 6.0 * t);
            }

            if (t < 1.0 / 2.0)
            {
                return q;
            }

            if (t < 2.0 / 3.0)
            {
                return p + ((q - p) * 6 * (2.0 / 3.0 - t));
            }

            return p;
        }

        private static Color[] _RandomColors;
        private static Color[] RandomColors
        {
            get
            {
                if (ColorUtil._RandomColors == null)
                {
                    ColorUtil._RandomColors = new Color[100];

                    ColorUtil._RandomColors[0] = Color.FromArgb(255, 0x46, 0x82, 0xb4);
                    ColorUtil._RandomColors[1] = Color.FromArgb(255, 0x41, 0x69, 0xe1);
                    ColorUtil._RandomColors[2] = Color.FromArgb(255, 0x64, 0x95, 0xed);
                    ColorUtil._RandomColors[3] = Color.FromArgb(255, 0xb0, 0xc4, 0xde);
                    ColorUtil._RandomColors[4] = Color.FromArgb(255, 0x7b, 0x68, 0xee);
                    ColorUtil._RandomColors[5] = Color.FromArgb(255, 0x6a, 0x5a, 0xcd);
                    ColorUtil._RandomColors[6] = Color.FromArgb(255, 0x48, 0x3d, 0x8b);
                    ColorUtil._RandomColors[7] = Color.FromArgb(255, 0x19, 0x19, 0x70);
                    
                    for (int colorIndex = 8; colorIndex < 100; colorIndex++)
                    {
                        // Create random color
                        ColorUtil._RandomColors[colorIndex] = Color.FromArgb(255, (Byte)ColorUtil.R.Next(255), (Byte)ColorUtil.R.Next(255), (Byte)ColorUtil.R.Next(255));
                    }

                    
                }
                return ColorUtil._RandomColors;
            }
        }

        /// <summary>
        /// Get a random color from generated array of colors.
        /// </summary>
        /// <param name="index">The index of the color to get in the generated array.</param>
        /// <returns>The color in the RandomColors array at the specified index.</returns>
        public static Color GetRandomColor(int index)
        {
            index %= 100;
            return ColorUtil.RandomColors[index];
        }
        /// <summary>
        /// Converts a color to its integer representation for use in bitmaps.
        /// </summary>
        /// <param name="color">The color to convert to an integer.</param>
        /// <returns>The integer representation for the given color.</returns>
        public static int ColorToInt(Color color)
        {
            double aa = color.A / 255.0;
            int rr = (int)(color.R * aa);
            int gg = (int)(color.G * aa);
            int bb = (int)(color.B * aa);

            return color.A << 24 | rr << 16 | gg << 8 | bb;
        }


        /// <summary>
        /// Returns the main color of the given brush.
        /// </summary>
        /// <param name="brush">The brush under observation.</param>
        /// <returns>The main color of the given brush.</returns>
        public static Color GetColor(Brush brush)
        {
            if (brush is SolidColorBrush)
            {
                return ((SolidColorBrush)brush).Color;
            }
            else if (brush is GradientBrush)
            {
                GradientBrush garbo = brush as GradientBrush;
                if (garbo.GradientStops != null && garbo.GradientStops.Count > 0)
                {
                    return garbo.GradientStops[0].Color;
                }
            }
            else
            {
                Debug.Assert(false, "unknown brush type");
            }
            return Colors.Transparent;
        }






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