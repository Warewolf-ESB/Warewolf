
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
using System.Windows.Media.Imaging;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// Contains helpers for the treeview nodes
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public static class TreeViewModelHelper
    {
        /// <summary>
        /// Gets an image from a uri
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public static BitmapImage GetImage(string uri)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        /// <summary>
        /// Gets the display name of the category, returns Unassigned if empty.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public static string GetCategoryDisplayName(string categoryName)
        {
            return string.IsNullOrEmpty(categoryName)
                       ? Warewolf.Studio.Resources.Languages.Core.Navigation_Category_Unassigned
                       : categoryName.ToUpper();
        }
    }
}
