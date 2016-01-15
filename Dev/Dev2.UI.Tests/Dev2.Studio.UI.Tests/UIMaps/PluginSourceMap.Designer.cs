
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses
{
    using System.CodeDom.Compiler;
    using System.Drawing;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


    [GeneratedCode("Coded UITest Builder", "11.0.51106.1")]
    public partial class PluginSourceMap : WizardsUIMap
    {

        /// <summary>
        /// ClickCancel
        /// </summary>
        public void ClickCancel()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{ENTER}");
        }


        /// <summary>
        /// ClickSave
        /// </summary>
        public void ClickSavePlugin()
        {
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}");
        }

        /// <summary>
        /// Click Save
        /// </summary>
        public void ClickSave()
        {
            var uIItemImage = StudioWindow.GetChildren()[0].GetChildren()[2];

            // Click image
            Mouse.Click(uIItemImage, new Point(523, 450));

            // Click image
            Mouse.Click(uIItemImage, new Point(488, 436));
        }
    }
}
