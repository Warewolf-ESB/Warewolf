using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Media;
using System.Collections.Generic;
namespace Infragistics
{
    /// <summary>
    /// Utility class for converting strings to colors.
    /// </summary>
    public static class ColorConverter
    {
        static ColorConverter()
        {
            standardColors.Add("aliceblue", new Color() {A=255, R=240, G=248,B=255});
            standardColors.Add("antiquewhite", new Color() {A=255, R=250, G=235,B=215});
            standardColors.Add("aqua", new Color() {A=255, R=0, G=255,B=255});
            standardColors.Add("aquamarine", new Color() {A=255, R=127, G=255,B=212});
            standardColors.Add("azure", new Color() {A=255, R=240, G=255,B=255});
            standardColors.Add("beige", new Color() {A=255, R=245, G=245,B=220});
            standardColors.Add("bisque", new Color() {A=255, R=255, G=228,B=196});
            standardColors.Add("black", new Color() {A=255, R=0, G=0,B=0});
            standardColors.Add("blanchedalmond", new Color() {A=255, R=255, G=235,B=205});
            standardColors.Add("blue", new Color() {A=255, R=0, G=0,B=255});
            standardColors.Add("blueviolet", new Color() {A=255, R=138, G=43,B=226});
            standardColors.Add("brown", new Color() {A=255, R=165, G=42,B=42});
            standardColors.Add("burgesswood", new Color() {A=255, R=222, G=184,B=135});
            standardColors.Add("cadetblue", new Color() {A=255, R=95, G=158,B=160});
            standardColors.Add("chartreuse", new Color() {A=255, R=127, G=255,B=0});
            standardColors.Add("chocolate", new Color() {A=255, R=210, G=105,B=30});
            standardColors.Add("coral", new Color() {A=255, R=255, G=127,B=80});
            standardColors.Add("cornflowerblue", new Color() {A=255, R=100, G=149,B=237});
            standardColors.Add("cornsilk", new Color() {A=255, R=255, G=248,B=220});
            standardColors.Add("crimson", new Color() {A=255, R=220, G=20,B=60});
            standardColors.Add("cyan", new Color() {A=255, R=0, G=255,B=255});
            standardColors.Add("darkblue", new Color() {A=255, R=0, G=0,B=139});
            standardColors.Add("darkcyan", new Color() {A=255, R=0, G=139,B=139});
            standardColors.Add("darkgoldenrod", new Color() {A=255, R=184, G=134,B=11});
            standardColors.Add("darkgray", new Color() {A=255, R=169, G=169,B=169});
            standardColors.Add("darkgreen", new Color() {A=255, R=0, G=100,B=0});
            standardColors.Add("darkkhaki", new Color() {A=255, R=189, G=183,B=107});
            standardColors.Add("darkmagenta", new Color() {A=255, R=139, G=0,B=139});
            standardColors.Add("darkolivegreen", new Color() {A=255, R=85, G=107,B=47});
            standardColors.Add("darkorange", new Color() {A=255, R=255, G=140,B=0});
            standardColors.Add("darkorchid", new Color() {A=255, R=153, G=50,B=204});
            standardColors.Add("darkred", new Color() {A=255, R=139, G=0,B=0});
            standardColors.Add("darksalmon", new Color() {A=255, R=233, G=150,B=122});
            standardColors.Add("darkseagreen", new Color() {A=255, R=143, G=188,B=143});
            standardColors.Add("darkslateblue", new Color() {A=255, R=72, G=61,B=139});
            standardColors.Add("darkslategray", new Color() {A=255, R=47, G=79,B=79});
            standardColors.Add("darkturquoise", new Color() {A=255, R=0, G=206,B=209});
            standardColors.Add("darkviolet", new Color() {A=255, R=148, G=0,B=211});
            standardColors.Add("deeppink", new Color() {A=255, R=255, G=20,B=147});
            standardColors.Add("deepskyblue", new Color() {A=255, R=0, G=191,B=255});
            standardColors.Add("dimgray", new Color() {A=255, R=105, G=105,B=105});
            standardColors.Add("dodgerblue", new Color() {A=255, R=30, G=144,B=255});
            standardColors.Add("firebrick", new Color() {A=255, R=178, G=34,B=34});
            standardColors.Add("floralwhite", new Color() {A=255, R=255, G=250,B=240});
            standardColors.Add("forestgreen", new Color() {A=255, R=34, G=139,B=34});
            standardColors.Add("fuchsia", new Color() {A=255, R=255, G=0,B=255});
            standardColors.Add("gainsboro", new Color() {A=255, R=220, G=220,B=220});
            standardColors.Add("ghostwhite", new Color() {A=255, R=248, G=248,B=255});
            standardColors.Add("gold", new Color() {A=255, R=255, G=215,B=0});
            standardColors.Add("goldenrod", new Color() {A=255, R=218, G=165,B=32});
            standardColors.Add("gray", new Color() {A=255, R=128, G=128,B=128});
            standardColors.Add("green", new Color() {A=255, R=0, G=128,B=0});
            standardColors.Add("greenyellow", new Color() {A=255, R=173, G=255,B=47});
            standardColors.Add("honeydew", new Color() {A=255, R=240, G=255,B=240});
            standardColors.Add("hotpink", new Color() {A=255, R=255, G=105,B=180});
            standardColors.Add("indianred", new Color() {A=255, R=205, G=92,B=92});
            standardColors.Add("indigo", new Color() {A=255, R=75, G=0,B=130});
            standardColors.Add("ivory", new Color() {A=255, R=255, G=255,B=240});
            standardColors.Add("khaki", new Color() {A=255, R=240, G=230,B=140});
            standardColors.Add("lavender", new Color() {A=255, R=230, G=230,B=250});
            standardColors.Add("lavenderblush", new Color() {A=255, R=255, G=240,B=245});
            standardColors.Add("lawngreen", new Color() {A=255, R=124, G=252,B=0});
            standardColors.Add("lemonchiffon", new Color() {A=255, R=255, G=250,B=205});
            standardColors.Add("lightblue", new Color() {A=255, R=173, G=216,B=230});
            standardColors.Add("lightcoral", new Color() {A=255, R=240, G=128,B=128});
            standardColors.Add("lightcyan", new Color() {A=255, R=224, G=255,B=255});
            standardColors.Add("lightgoldenrodyellow", new Color() {A=255, R=250, G=250,B=210});
            standardColors.Add("lightgreen", new Color() {A=255, R=144, G=238,B=144});
            standardColors.Add("lightgrey", new Color() {A=255, R=211, G=211,B=211});
            standardColors.Add("lightpink", new Color() {A=255, R=255, G=182,B=193});
            standardColors.Add("lightsalmon", new Color() {A=255, R=255, G=160,B=122});
            standardColors.Add("lightseagreen", new Color() {A=255, R=32, G=178,B=170});
            standardColors.Add("lightskyblue", new Color() {A=255, R=135, G=206,B=250});
            standardColors.Add("lightslategray", new Color() {A=255, R=119, G=136,B=153});
            standardColors.Add("lightsteelblue", new Color() {A=255, R=176, G=196,B=222});
            standardColors.Add("lightyellow", new Color() {A=255, R=255, G=255,B=224});
            standardColors.Add("lime", new Color() {A=255, R=0, G=255,B=0});
            standardColors.Add("limegreen", new Color() {A=255, R=50, G=205,B=50});
            standardColors.Add("linen", new Color() {A=255, R=250, G=240,B=230});
            standardColors.Add("magenta", new Color() {A=255, R=255, G=0,B=255});
            standardColors.Add("maroon", new Color() {A=255, R=128, G=0,B=0 });
            standardColors.Add("mediumaquamarine", new Color() {A=255, R=102, G=205,B=170});
            standardColors.Add("mediumblue", new Color() {A=255, R=0, G=0,B=205});
            standardColors.Add("mediumorchid", new Color() {A=255, R=186, G=85,B=211});
            standardColors.Add("mediumpurple", new Color() {A=255, R=147, G=112,B=219});
            standardColors.Add("mediumseagreen", new Color() {A=255, R=60, G=179,B=113});
            standardColors.Add("mediumslateblue", new Color() {A=255, R=123, G=104,B=238});
            standardColors.Add("mediumspringgreen", new Color() {A=255, R=0, G=250,B=154});
            standardColors.Add("mediumturquoise", new Color() {A=255, R=72, G=209,B=204});
            standardColors.Add("mediumvioletred", new Color() {A=255, R=199, G=21,B=133});
            standardColors.Add("midnightblue", new Color() {A=255, R=25, G=25,B=112});
            standardColors.Add("mintcream", new Color() {A=255, R=245, G=255,B=250});
            standardColors.Add("mistyrose", new Color() {A=255, R=255, G=228,B=225});
            standardColors.Add("moccasin", new Color() {A=255, R=255, G=228,B=181});
            standardColors.Add("navajowhite", new Color() {A=255, R=255, G=222,B=173});
            standardColors.Add("navy", new Color() {A=255, R=0, G=0,B=128});
            standardColors.Add("oldlace", new Color() {A=255, R=253, G=245,B=230});
            standardColors.Add("olive", new Color() {A=255, R=128, G=128,B=0});
            standardColors.Add("olivedrab", new Color() {A=255, R=107, G=142,B=35});
            standardColors.Add("orange", new Color() {A=255, R=255, G=165,B=0});
            standardColors.Add("orangered", new Color() {A=255, R=255, G=69,B=0});
            standardColors.Add("orchid", new Color() {A=255, R=218, G=112,B=214});
            standardColors.Add("palegoldenrod", new Color() {A=255, R=238, G=232,B=170});
            standardColors.Add("palegreen", new Color() {A=255, R=152, G=251,B=152});
            standardColors.Add("paleturquoise", new Color() {A=255, R=175, G=238,B=238});
            standardColors.Add("palevioletred", new Color() {A=255, R=219, G=112,B=147});
            standardColors.Add("papayawhip", new Color() {A=255, R=255, G=239,B=213});
            standardColors.Add("peachpuff", new Color() {A=255, R=255, G=218,B=185});
            standardColors.Add("peru", new Color() {A=255, R=205, G=133,B=63});
            standardColors.Add("pink", new Color() {A=255, R=255, G=192,B=203});
            standardColors.Add("plum", new Color() {A=255, R=221, G=160,B=221});
            standardColors.Add("powderblue", new Color() {A=255, R=176, G=224,B=230});
            standardColors.Add("purple", new Color() {A=255,  R = 128, G = 0, B = 128 });
            standardColors.Add("purwablue", new Color() {A=255, R=155, G=225,B=255});
            standardColors.Add("red", new Color() {A=255, R=255, G=0,B=0});
            standardColors.Add("rosybrown", new Color() {A=255, R=188, G=143,B=143});
            standardColors.Add("royalblue", new Color() {A=255, R=65, G=105,B=225});
            standardColors.Add("saddlebrown", new Color() {A=255, R=139, G=69,B=19});
            standardColors.Add("salmon", new Color() {A=255, R=250, G=128,B=114});
            standardColors.Add("sandybrown", new Color() {A=255, R=244, G=164,B=96});
            standardColors.Add("seagreen", new Color() {A=255, R=46, G=139,B=87});
            standardColors.Add("seashell", new Color() {A=255, R=255, G=245,B=238});
            standardColors.Add("sienna", new Color() {A=255, R=160, G=82,B=45});
            standardColors.Add("silver", new Color() {A=255, R=192, G=192,B=192});
            standardColors.Add("skyblue", new Color() {A=255, R=135, G=206,B=235});
            standardColors.Add("slateblue", new Color() {A=255, R=106, G=90,B=205});
            standardColors.Add("slategray", new Color() {A=255, R=112, G=128,B=144});
            standardColors.Add("snow", new Color() {A=255, R=255, G=250,B=250});
            standardColors.Add("springgreen", new Color() {A=255, R=0, G=255,B=127});
            standardColors.Add("steelblue", new Color() {A=255, R=70, G=130,B=180});
            standardColors.Add("tan", new Color() {A=255, R=210, G=180,B=140});
            standardColors.Add("teal", new Color() {A=255, R=0, G=128,B=128});
            standardColors.Add("thistle", new Color() {A=255, R=216, G=191,B=216});
            standardColors.Add("tomato", new Color() {A=255, R=255, G=99,B=71});
            standardColors.Add("turquoise", new Color() {A=255, R=64, G=224,B=208});
            standardColors.Add("violet", new Color() {A=255, R=238, G=130,B=238});
            standardColors.Add("wheat", new Color() {A=255, R=245, G=222,B=179});
            standardColors.Add("white", new Color() {A=255, R=255, G=255,B=255});
            standardColors.Add("whitesmoke", new Color() {A=255, R=245, G=245,B=245});
            standardColors.Add("yellow", new Color() {A=255, R=255, G=255,B=0});
            standardColors.Add("yellowgreen", new Color() {A=255, R=154, G=205,B=50});	
        }
        private static Dictionary<string, Color> standardColors = new Dictionary<string, Color>();

        private static bool IsHexits(string value)
        {
            if (value == null)
            {
                return false;
            }
            bool result = true;
            char[] chars = value.ToLower(CultureInfo.InvariantCulture).ToCharArray();
            foreach (char c in chars)
            {
                result &= char.IsDigit(c) || c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f';

            }
            return result;
        }
        /// <summary>
        /// Gets a color matching the input string.
        /// </summary>
        /// <param name="value">The string to convert to a color.</param>
        /// <returns>A color matching the input string.</returns>
        public static Color FromString(string value)
        {
            if (value == null)
            {
                return Colors.Transparent;
            }

            string valueLower = value.ToLower(CultureInfo.InvariantCulture);

            if (standardColors.ContainsKey(valueLower))
            {
                return standardColors[valueLower];
            }

            if (value[0] == '#')
            {
                value = value.Remove(0, 1);
            }

            Color color;
            if (FromHexitsString(value, out color))
            {
                return color;
            }
            if (FromTokenizedString(value, out color))
            {
                return color;
            }
           
            PropertyInfo namedColorProperty = typeof(Colors).GetProperty(value, BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public);
            if (namedColorProperty != null)
            {
                object result = namedColorProperty.GetValue(null, BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
                if (result is Color)
                {
                    return (Color)result;
                }
            }

            return Colors.Transparent;
        }

        private static bool FromHexitsString(string value, out Color color)
        {
            if (ColorConverter.IsHexits(value))
            {
                if (value.Length == 8)
                {
                    color = Color.FromArgb(
                        byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    return true;
                }
                else if (value.Length == 6)
                {
                    
                    color = Color.FromArgb(255,
                        byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    return true;
                }
            }
            
            color = Color.FromArgb(255, 0, 0, 0);
            return false;
        }

        private static bool FromTokenizedString(string value, out Color color)
        {
            string[] tokenizedValueString = value.Split(',');
            if (tokenizedValueString != null)
            {
                if (tokenizedValueString.Length == 4)
                {
                    color = Color.FromArgb(
                        byte.Parse(tokenizedValueString[0], CultureInfo.InvariantCulture),
                        byte.Parse(tokenizedValueString[1], CultureInfo.InvariantCulture),
                        byte.Parse(tokenizedValueString[2], CultureInfo.InvariantCulture),
                        byte.Parse(tokenizedValueString[3], CultureInfo.InvariantCulture));
                    return true;
                }
                else if (tokenizedValueString.Length == 3)
                {
                    color = Color.FromArgb(255,
                        byte.Parse(tokenizedValueString[0], CultureInfo.InvariantCulture),
                        byte.Parse(tokenizedValueString[1], CultureInfo.InvariantCulture),
                        byte.Parse(tokenizedValueString[2], CultureInfo.InvariantCulture));
                    return true;
                }
            }
            color = Color.FromArgb(255, 0, 0, 0);
            return false;

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