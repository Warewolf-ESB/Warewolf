#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Xml;


namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Static class with methods relating to binding
    /// </summary>
    public static class BindingHelper
    {
        #region Member Variables

        static bool _xmlLoaded;

        #endregion //Member Variables

        #region Methods

        #region BindPath
        /// <summary>
        /// Binds the specified target property to the specified path property.
        /// </summary>
        /// <param name="item">The underlying data item</param>
        /// <param name="container">The element associated with the item</param>
        /// <param name="path">The path property to provide the value for the specified <paramref name="targetProperty"/></param>
        /// <param name="targetProperty">The property whose value is to come from the specified path of the underlying data item.</param>
        public static void BindPath(DependencyObject container, object item, string path, DependencyProperty targetProperty)
        {
            if(string.IsNullOrEmpty(path))
            {
                return;
            }

            var b = new Binding();

            if (IsXmlNode(item))
            {
                b.XPath = path;
            }
            else
            {
                b.Path = new PropertyPath(path);
            }

            BindingOperations.SetBinding(container, targetProperty, b);
        }
        #endregion //BindPath

        #region IsXmlNode
        /// <summary>
        /// Returns a boolean indicating if the specified object is an xml node object without forcing the Xml assembly to be loaded
        /// </summary>
        /// <param name="item">The object to evaluate</param>
        /// <returns>True if the object is or derives from System.Xml.XmlNode</returns>
        public static bool IsXmlNode(object item)
        {
            if(null != item && (_xmlLoaded || item.GetType().FullName.StartsWith("System.Xml")))
            {
                return IsXmlNodeHelper(item);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool IsXmlNodeHelper(object item)
        {
            _xmlLoaded = true;
            return item is XmlNode;
        }
        #endregion //IsXmlNode

        #endregion //Methods
    }
}
