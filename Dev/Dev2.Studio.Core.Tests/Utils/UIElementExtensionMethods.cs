using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Serialization;

namespace Dev2.Core.Tests.Utils
{
    public static class UIElementExtensionMethods
    {
        /// <summary>
        /// Render a UIElement such that the visual tree is generated, 
        /// without actually displaying the UIElement
        /// anywhere
        /// </summary>
        public static void CreateVisualTree(this UIElement element)
        {
            var fixedDoc = new FixedDocument();
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();
            fixedPage.Children.Add(element);
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            var f = new XpsSerializerFactory();
            using(var s = new MemoryStream())
            {
                var w = f.CreateSerializerWriter(s);
                w.Write(fixedDoc);
            }

        }
    }
}
