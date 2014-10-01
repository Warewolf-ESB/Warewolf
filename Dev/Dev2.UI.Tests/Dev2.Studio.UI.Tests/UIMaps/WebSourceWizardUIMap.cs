
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    // ReSharper disable InconsistentNaming
    public class WebSourceWizardUIMap : WizardsUIMap
    // ReSharper restore InconsistentNaming
    {
        public void ClickSave()
        {
            var control = StudioWindow.GetChildren()[0].GetChildren()[2];
            control.WaitForControlEnabled();
            Mouse.Click(control, new Point(648, 501));
        }

        public void EnterTextForDefaultQueryIfFocusIsSet(string textToEnter)
        {
            Keyboard.SendKeys(textToEnter);
        }
    }
}
