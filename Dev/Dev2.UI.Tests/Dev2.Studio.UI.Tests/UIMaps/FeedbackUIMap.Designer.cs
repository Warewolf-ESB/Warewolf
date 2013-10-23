namespace Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Dev2.CodedUI.Tests;
    
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
            var findFeedbackWindow = StudioWindow.GetParent().GetChildren();
            foreach (var child in findFeedbackWindow)
            {
                var getTitle = child.GetChildren()[0].Name;
                if (getTitle == "Feedback")
                {
                    return child as WpfWindow;
                }
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
            foreach (var child in children)
            {
                if (child.GetProperty("AutomationId").ToString() == "FeebackView_Send")
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
                if ((this.mUIRecordedFeedbackWindow == null))
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
