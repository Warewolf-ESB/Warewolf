
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Dev2
{
    public static class ExtensionMethods
    {
        public static string AttributeSafe(this XElement elem, string name)
        {
            var attr = elem.Attribute(name);
            return attr == null ? string.Empty : attr.Value;
        }

        public static string ElementSafe(this XElement elem, string name)
        {
            var child = elem.Element(name);
            return child == null ? string.Empty : child.Value;
        }

        public static bool ContainsSafe(this string s, string filter)
        {
            if(string.IsNullOrEmpty(filter))
            {
                return true;
            }
            if(!string.IsNullOrEmpty(s))
            {
                return s.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
            }
            return false;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            var col = new ObservableCollection<T>();
            foreach(var cur in enumerable)
            {
                col.Add(cur);
            }
            return col;
        }

        public static string ToBase64String(this Stream stream)
        {
            return Convert.ToBase64String(GetByteArray(stream));
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            return GetByteArray(stream);
        }

        public static byte[] GetByteArray(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using(MemoryStream ms = new MemoryStream())
            {
                int read;
                while((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        ///  Returns the current string as a set of XML tags 
        /// </summary>
        /// <param name="tag">The string to be returned as tags</param>
        /// <returns>a set of tags in the form <tag>,</tag> as a string array</returns>
        public static string[] ReturnAsTagSet(this string tag)
        {
            return new string[] { "<" + tag + ">", "</" + tag + ">" };
        }

        public static string GetPropertyName<T, TReturn>(this Expression<Func<T, TReturn>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }
    }
}
