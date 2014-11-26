
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UITesting;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses
// ReSharper restore CheckNamespace
{
    using System.Drawing;
    using Mouse = Mouse;


    // ReSharper disable InconsistentNaming
    public partial class WebServiceWizardUIMap
    // ReSharper restore InconsistentNaming
    {
        public void ClickNewWebSource()
        {
            var control = StudioWindow.GetChildren()[0].GetChildren()[2];
            control.WaitForControlEnabled();
            Mouse.Click(control, new Point(410, 79));
        }

        public void CreateWebSource(string sourceUrl, string sourceName)
        {
            //Web Source Details
            KeyboardCommands.SendKey(sourceUrl);
            KeyboardCommands.SendTabs(3);
            Playback.Wait(100);
            KeyboardCommands.SendEnter();
            Playback.Wait(1000);
            WebSourceWizardUIMap.ClickSave();
            KeyboardCommands.SendTabs(3);
            KeyboardCommands.SendKey(sourceName);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
            Playback.Wait(1000);
        }

        public void SaveWebService(string serviceName)
        {
            //Web Service Details
            KeyboardCommands.SendTabs(8);
            Playback.Wait(500);
            KeyboardCommands.SendEnter();
            Playback.Wait(500);//wait for test
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
            Playback.Wait(500);
            KeyboardCommands.SendTabs(3);
            KeyboardCommands.SendKey(serviceName);
            KeyboardCommands.SendTab();
            KeyboardCommands.SendEnter();
        }
    }
}
