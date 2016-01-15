
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
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class FeedbackUIMap
    {
        public bool DoesRecordedFeedbackWindowExist()
        {
            WinWindow RecordedFeedbackWindow = GetRecordedFeedbackWindow();
            Point p;
            return RecordedFeedbackWindow.TryGetClickablePoint(out p);
        }

        public bool DoesFeedbackWindowExist()
        {
            WpfWindow theWindow = GetFeedbackWindow();
            Point p;
            return theWindow.TryGetClickablePoint(out p);
        }

        public void ClickStartStopRecordingButton()
        {
            Point theButtonPosition = StartRecordingButtonPoint();
            Mouse.Click(theButtonPosition);
            Playback.Wait(1500);
        }

        public void FeedbackWindow_ClickCancel()
        {
            Playback.Wait(500);
            WpfButton theButton = FeedbackWindow_CancelButton();
            Mouse.Click(theButton, new Point(5, 5));
        }

        public void FeedbackWindow_ClickOpenDefaultEmail()
        {
            WpfButton theButton = FeedbackWindow_OpenDefaultEmailButton();
            Mouse.Click(theButton, new Point(5, 5));
        }
    }
}
