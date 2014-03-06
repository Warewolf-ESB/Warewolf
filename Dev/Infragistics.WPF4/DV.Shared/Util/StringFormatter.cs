using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Represents a string formatting object used to create strings
    /// based upon an object and a formatting string which dereferences
    /// properties from the object.
    /// </summary>
    public class StringFormatter
    {
        /// <summary>
        /// Delegate used to retreive named properties from an object.
        /// </summary>
        /// <param name="obj">Object containing named property.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Property value or null if object does not have the named property.</returns>
        public delegate object ValueDelegate(object obj, string propertyName);

        /// <summary>
        /// Predicate indicating that the current StringFormatter object refers
        /// to the named property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>true if the current StringFormatter object refers
        /// to the named property.</returns>
        public bool References(string propertyName)
        {
            return PropertyNames.Contains(propertyName);
        }

        /// <summary>
        /// Create a formatted a string according to the compiled format
        /// string and the properties on the specified object
        /// </summary>
        /// <param name="obj">The object in context.</param>
        /// <param name="propertyValue">The ValueDelegate in context.</param>
        /// <returns>Formatted string or null on error.</returns>
        public string Format(object obj, ValueDelegate propertyValue)
        {
            if (CompiledFormatString == null || PropertyNames == null)
            {
                return null;
            }

            object[] propertyValues = new object[PropertyNames.Count];

            for (int i = 0; i < PropertyNames.Count; ++i)
            {
                string propertyName = this.PropertyNames[i];
                
                propertyValues[i] = propertyValue == null ? GetPropertyValue(obj, propertyName) : propertyValue(obj, propertyName);

                if (propertyValues[i] == null)
                {
                    if (propertyName == "0")
                    {
                        propertyValues[i] = obj; // {0} in the format string is the object itself
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            try
            {
                return string.Format(CompiledFormatString, propertyValues);
            }
            catch (FormatException) // your format is wack.  you get nothing!!!
            {
                return CompiledFormatString;
            }
        }

        /// <summary>
        /// Gets or sets the raw formatting string for the current StringFormatter object. 
        /// </summary>
        /// <remarks>
        /// Setting the raw formatting string immediately compiles the format string and
        /// list of referenced properties.
        /// </remarks>
        public string FormatString
        {
            get { return formatString; }
            set
            {
                if (formatString != value)
                {
                    formatString = value;

                    int i = 0;
                    int cur = 0;

                    PropertyNames = new List<string>();
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int bgn = formatString.IndexOf('{', cur); bgn >= cur; bgn = formatString.IndexOf('{', cur))
                    {
                        stringBuilder.Append(formatString.Substring(cur, bgn - cur));

                        int end = formatString.IndexOf('}', bgn);

                        if (end <= bgn)
                        {
                            return; // that's a problem
                        }

                        string cmd = formatString.Substring(bgn + 1, end - bgn - 1).Trim();
                        int separator = cmd.IndexOf(':');

                        if (separator == -1)
                        {
                            PropertyNames.Add(cmd);
                            stringBuilder.Append("{" + i + "}");
                        }
                        else
                        {
                            PropertyNames.Add(cmd.Substring(0, separator).Trim());
                            stringBuilder.Append("{" + i + ":" + cmd.Substring(separator + 1).Trim() + "}");
                        }

                        ++i;
                        cur = end + 1;
                    }

                    stringBuilder.Append(formatString.Substring(cur));  // append trailing stuff
                    CompiledFormatString = stringBuilder.ToString();
                }
            }
        }
        private string formatString;

        /// <summary>
        /// Returns a String that represents the current StringFormatter.
        /// </summary>
        /// <returns>A String that represents the current StringFormatter.</returns>
        public override string ToString()
        {
            return FormatString ?? "";
        }

        /// <summary>
        /// Gets the compiled format string for the current StringFormatter object.
        /// </summary>
        public string CompiledFormatString { get; private set; }

        /// <summary>
        /// Gets the list of the property names referred to by the current StringFormatter object. 
        /// </summary>
        public List<string> PropertyNames { get; private set; }

        private static Regex regex = new Regex(@"(.*)\[(\d+)\]$");

        /// <summary>
        /// The default ValueDelegate
        /// </summary>
        /// <param name="obj">Object with the named property </param>
        /// <param name="name">Property name. A property name of '*' uses the object as the property.</param>
        /// <returns></returns>
        internal static object GetPropertyValue(Object obj, string name)
        {
            string[] names = name.Split('.');

            for (int i = 0; i < names.Length && obj != null; ++i)
            {
                Match match = regex.Match(names[i]);
                string propertyName = match.Success ? match.Groups[1].ToString() : names[i];
                int index = match.Success ? int.Parse(match.Groups[2].ToString()) : -1;

                Type type = obj.GetType();
                if (propertyName == "0" && type.IsValueType)
                {
                    return obj;
                }
                if (propertyName != "")
                {
                    PropertyInfo propertyInfo = type.GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        obj = propertyInfo != null && propertyInfo.CanRead ? propertyInfo.GetValue(obj, null) : null;
                    }

                    else if (typeof(ICustomTypeDescriptor).IsAssignableFrom(type))
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
                        if (properties != null && properties[propertyName] != null)
                        {
                            obj = properties[propertyName].GetValue(obj);
                        }
                        else
                        {
                            obj = null;
                        }
                    }

                    else
                    {
                        obj = null;
                    }
                }

                if (obj != null && index >= 0)
                {
                    PropertyInfo propertyInfo = type.GetProperty("Item");
                    object[] indexArgs = { index };

                    obj = propertyInfo != null && propertyInfo.CanRead ? propertyInfo.GetValue(obj, indexArgs) : null;
                }
            }

            return obj;
        }
    }

    /// <summary>
    /// Utility class for auto string formatting.
    /// </summary>
    public static class StringFormatUtil
    {
        /// <summary>
        /// Create and format a string according to the value of the current string
        /// and the properties on the specified object
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="obj">Object to format</param>
        /// <param name="propertyValue">Property value delegate</param>
        /// <returns>formatted string or null on error.</returns>
        public static string Format(this string format, object obj, StringFormatter.ValueDelegate propertyValue)
        {
            StringFormatter formatter = format != null ? new StringFormatter() { FormatString = format } : null;

            return formatter != null ? formatter.Format(obj, propertyValue) : null;
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