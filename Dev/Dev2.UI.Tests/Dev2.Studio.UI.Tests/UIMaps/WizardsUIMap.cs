
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
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    // ReSharper disable InconsistentNaming
    public class WizardsUIMap : UIMapBase
    // ReSharper restore InconsistentNaming
    {
        private const int DefaultTimeOut = 3000;
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForWizard(int timeOut = DefaultTimeOut, bool throwIfNotFound = true)
        {
            Playback.Wait(timeOut);
        }

        public bool TryWaitForWizard(int timeOut = 10000)
        {
            Playback.Wait(timeOut);
            var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[2];
            var type = tryGetDialog.GetType();
            return type == typeof(WpfImage);
        }

        public string GetLeftTitleText()
        {
            return GetTitleLabel("LeftTitle").DisplayText;
        }

        public string GetRightTitleText()
        {
            return GetTitleLabel("RightTitle").DisplayText;
        }

        private WpfText GetTitleLabel(string autoId)
        {
            UITestControlCollection uiTestControlCollection = StudioWindow.GetChildren();
            var tryGetDialog = uiTestControlCollection[0];

            UITestControl childByAutomationIDPath = VisualTreeWalker.GetChildByAutomationIdPath(tryGetDialog, autoId);
            WpfText wpfText = childByAutomationIDPath as WpfText;
            if(wpfText != null)
            {
                return wpfText;
            }
            throw new Exception("Could not find the " + autoId + " label.");
        }

    }
}
