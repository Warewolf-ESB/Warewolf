
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.CodeDom.Compiler;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class FeedbackUIMap : UIMapBase
    {

        /// <summary>
        /// RecordedFeedbackWindowExists
        /// </summary>
        private WinWindow GetRecordedFeedbackWindow()
        {
            WinWindow uIRecordedFeedbackWindow = this.UIRecordedFeedbackWindow;
            uIRecordedFeedbackWindow.Find();
            return uIRecordedFeedbackWindow;
        }

        private Point StartRecordingButtonPoint()
        {
            // Get the Studio
            WpfWindow theStudio = new WpfWindow();

            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.Name, "Warewolf", PropertyExpressionOperator.Contains));

            theStudio.Find();

            return new Point(theStudio.BoundingRectangle.Right - 50, theStudio.Top + 35);

        }

        private WpfWindow GetFeedbackWindow()
        {
            var parentWindow = StudioWindow.GetParent();
            var parentChildern = parentWindow.GetChildren();

            var childCount = parentChildern.Count;

            childCount += 1;

            var findFeedbackWindow = parentChildern.FirstOrDefault(c => c.Name == "Feedback");

            if(findFeedbackWindow != null)
            {
                return findFeedbackWindow as WpfWindow;
            }

            throw new UITestControlNotFoundException("Cannot find feedback window");
        }

        private WpfButton FeedbackWindow_CancelButton()
        {
            WpfButton theButton = new WpfButton(GetFeedbackWindow());
            theButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "FeebackView_Cancel";
            theButton.Find();
            return theButton;
        }

        private WpfButton FeedbackWindow_OpenDefaultEmailButton()
        {
            var feedbackWindow = GetFeedbackWindow();
            var children = feedbackWindow.GetChildren();
            foreach(var child in children)
            {
                if(child.GetProperty("AutomationId").ToString() == "FeebackView_Send")
                {
                    return child as WpfButton;
                }
            }
            WpfButton theButton = new WpfButton(feedbackWindow);
            theButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "FeebackView_Send";
            theButton.Find();
            return theButton;
        }

        #region Properties
        public UIRecordedFeedbackWindow UIRecordedFeedbackWindow
        {
            get
            {
                if((this.mUIRecordedFeedbackWindow == null))
                {
                    this.mUIRecordedFeedbackWindow = new UIRecordedFeedbackWindow();
                }
                return this.mUIRecordedFeedbackWindow;
            }
        }
        #endregion

        #region Fields
        private UIRecordedFeedbackWindow mUIRecordedFeedbackWindow;
        #endregion
    }

    [GeneratedCode("Coded UITest Builder", "11.0.51106.1")]
    public class UIRecordedFeedbackWindow : WinWindow
    {

        public UIRecordedFeedbackWindow()
        {
            #region Search Criteria
            this.SearchProperties[WinWindow.PropertyNames.Name] = "Recorded Feedback";
            this.SearchProperties[WinWindow.PropertyNames.ClassName] = "#32770";
            this.WindowTitles.Add("Recorded Feedback");
            #endregion
        }
    }
}
