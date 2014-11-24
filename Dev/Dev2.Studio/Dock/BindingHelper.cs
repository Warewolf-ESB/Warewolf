
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Static class with methods relating to binding
    /// </summary>
    public static class BindingHelper
    {
        #region Member Variables

        private static bool _xmlLoaded;

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
                return;

            Binding b = new Binding();

            if(IsXmlNode(item))
                b.XPath = path;
            else
                b.Path = new PropertyPath(path);

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
        private static bool IsXmlNodeHelper(object item)
        {
            _xmlLoaded = true;
            return item is System.Xml.XmlNode;
        }
        #endregion //IsXmlNode

        #endregion //Methods
    }
}
