#region

using System;
using System.Windows.Media.Imaging;
using Dev2.Studio.Core;

#endregion

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
                       ? StringResources.Navigation_Category_Unassigned
                       : categoryName.ToUpper();
        }
    }
}
