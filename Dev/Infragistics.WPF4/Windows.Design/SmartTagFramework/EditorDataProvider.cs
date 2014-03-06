using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Provides data required by different UI editors.
    /// </summary>
    public static class EditorDataProvider
    {
        #region Constants

        private const string			_nullValueString = "{x:Null}";

        #endregion //Constants

        #region Member Variables

        private static ICollection<FontStyle> _defaultFontStyles;
        private static ICollection<FontWeight> _defaultFontWeights;
        private static ICollection<FontStretch> _defaultFontStretches;
        private static ICollection<string> _defaultCursorNames;
        private static ICollection<NamedColor> _defaultColorNames;
        private static ICollection<XmlLanguage> _defaultLanguages;

        #endregion //Member Variables

        #region Properties

        #region Public Properties

        #region DefaultFontStyles

        /// <summary>
        /// Gets the default font styles.
        /// </summary>
        /// <value>The default font styles.</value>
        public static ICollection<FontStyle> DefaultFontStyles
        {
            get
            {
                if (_defaultFontStyles == null)
                    _defaultFontStyles = GetDefaultFontStyles();

                return _defaultFontStyles;
            }
        }

        #endregion //DefaultFontStyles

        #region DefaultFontWeights

        /// <summary>
        /// Gets the default font weights.
        /// </summary>
        /// <value>The default font weights.</value>
        public static ICollection<FontWeight> DefaultFontWeights
        {
            get
            {
                if (_defaultFontWeights == null)
                    _defaultFontWeights = GetDefaultFontWeights();

                return _defaultFontWeights;
            }
        }

        #endregion //DefaultFontWeights

        #region DefaultFontStretches

        /// <summary>
        /// Gets the default font stretches.
        /// </summary>
        /// <value>The default font stretches.</value>
        public static ICollection<FontStretch> DefaultFontStretches
        {
            get
            {
                if (_defaultFontStretches == null)
                    _defaultFontStretches = GetDefaultFontStretches();

                return _defaultFontStretches;
            }
        }

        #endregion //DefaultFontStretches

        #region DefaultCursorNames

        /// <summary>
        /// Gets the default cursor names.
        /// </summary>
        /// <value>The default cursor names.</value>
        public static ICollection<string> DefaultCursorNames
        {
            get
            {
                if (_defaultCursorNames == null)
                    _defaultCursorNames = GetDefaultCursorNames();

                return _defaultCursorNames;
            }
        }

        #endregion //DefaultCursorNames

        #region DefaultColorNames

        /// <summary>
        /// Gets the default color names.
        /// </summary>
        /// <value>The default color names.</value>
        public static ICollection<NamedColor> DefaultColorNames
        {
            get
            {
                if (_defaultColorNames == null)
                    _defaultColorNames = GetDefaultColorNames();

                return _defaultColorNames;
            }
        }

        #endregion //DefaultColorNames

        #region DefaultLanguages

        /// <summary>
        /// Gets the default languages.
        /// </summary>
        /// <value>The default languages.</value>
        public static ICollection<XmlLanguage> DefaultLanguages
        {
            get
            {
                if (_defaultLanguages == null)
                    _defaultLanguages = GetDefaultLanguages();

                return _defaultLanguages;
            }
        }

        #endregion //DefaultLanguages

        #endregion //Public Properties

        #endregion //Properties

        #region Methods

        #region Private Methods

        #region GetDefaultFontStyles

        private static ICollection<FontStyle> GetDefaultFontStyles()
        {
            return new FontStyle[]
                                      {
                                        FontStyles.Italic,
                                        FontStyles.Normal,
                                        FontStyles.Oblique        
                                      };
        }

        #endregion //GetDefaultFontStyles

        #region GetDefaultFontWeights

        private static ICollection<FontWeight> GetDefaultFontWeights()
        {
            return new FontWeight[]
                                      {
                                        FontWeights.Thin,
                                        FontWeights.ExtraLight,
                                        FontWeights.Light,
                                        FontWeights.Normal,
                                        FontWeights.Medium,
                                        FontWeights.SemiBold,
                                        FontWeights.Bold,
                                        FontWeights.ExtraBold,
                                        FontWeights.Black,
                                        FontWeights.ExtraBlack
                                      };
        }

        #endregion //GetDefaultFontWeights

        #region GetDefaultFontStretches

        private static ICollection<FontStretch> GetDefaultFontStretches()
        {
            return new FontStretch[]
                                      {
                                        FontStretches.UltraCondensed,
                                        FontStretches.ExtraCondensed,
                                        FontStretches.Condensed,
                                        FontStretches.SemiCondensed,
                                        FontStretches.Normal,
                                        FontStretches.SemiExpanded,
                                        FontStretches.Expanded,
                                        FontStretches.ExtraExpanded,        
                                        FontStretches.UltraExpanded                                        
                                      };
        }

        #endregion //GetDefaultFontStretches

        #region GetDefaultCursorNames

        private static ICollection<string> GetDefaultCursorNames()
        {
            List<string> names = new List<string>(Enum.GetNames(typeof(CursorType)));
            names.Add(_nullValueString);
            return names;
        }

        #endregion //GetDefaultCursorNames

        #region GetDefaultColorNames

        private static ICollection<NamedColor> GetDefaultColorNames()
        {
            List<NamedColor> names = new List<NamedColor>();

            NamedColor nc;
            MethodAttributes inclusiveAttributes = MethodAttributes.Static | MethodAttributes.Public;

            foreach (PropertyInfo pi in typeof(Colors).GetProperties())
            {
                if (pi.PropertyType == typeof(Color))
                {
                    MethodInfo mi = pi.GetGetMethod();
                    if ((mi != null) && ((mi.Attributes & inclusiveAttributes) == inclusiveAttributes))
                    {
                        nc = new NamedColor(pi.Name, (Color)pi.GetValue(null, null));
                        names.Add(nc);
                    }
                }
            }

            return names;
        }

        #endregion //GetDefaultColorNames

        #region GetDefaultLanguages

        private static ICollection<XmlLanguage> GetDefaultLanguages()
        {
            List<XmlLanguage> results = new List<XmlLanguage>();
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (string.IsNullOrEmpty(ci.IetfLanguageTag))
                {
                    continue;
                }

                results.Add(XmlLanguage.GetLanguage(ci.IetfLanguageTag));
            }
            return results;
        }

        #endregion //GetDefaultLanguages

        #endregion //Private Methods

        #endregion //Methods
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