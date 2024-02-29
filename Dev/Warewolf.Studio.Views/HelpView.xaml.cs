#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Media;
using Dev2.Common.Interfaces;
using Infragistics.Documents.RichText;
#if !NETFRAMEWORK
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : IHelpView
    {
        public HelpView()
        {
            InitializeComponent();
            var richTextDocument = XamRichTextEditor.Document;

            var color = (Color)ColorConverter.ConvertFromString("#FFF4F2EE");

            var colorInfo = new ColorInfo(color);
            richTextDocument.RootNode.Settings = new DocumentSettings
            {
                Background = colorInfo,
            };
            XamRichTextEditor.CaretColor = colorInfo;
        }

#if !NETFRAMEWORK
		public string Path => throw new System.NotImplementedException();

		public Task RenderAsync(ViewContext context)
		{
			throw new System.NotImplementedException();
		}
#endif
	}
}
