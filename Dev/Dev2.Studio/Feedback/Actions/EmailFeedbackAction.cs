
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Help;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Feedback.Actions
{
    public class EmailFeedbackAction : IFeedbackAction
    {
        readonly Dictionary<string, string> _attachmentPath;
        readonly IEnvironmentModel _environmentModel;

        public EmailFeedbackAction(Dictionary<string, string> attachedFiles, IEnvironmentModel activeEnvironment)
        {
            VerifyArgument.IsNotNull("activeEnvironment", activeEnvironment);
            WindowManager = CustomContainer.Get<IWindowManager>();
            _attachmentPath = attachedFiles;
            _environmentModel = activeEnvironment;
        }

        public IWindowManager WindowManager { get; private set; }

        public bool CanProvideFeedback
        {
            get { return true; }
        }

        public int Priority
        {
            get { return 2; }
        }

        public void StartFeedback()
        {
            if(CanProvideFeedback)
                DisplayFeedbackWindow();
        }

        public void DisplayFeedbackWindow()
        {
            var feedbackVm = new FeedbackViewModel(_attachmentPath);
            if(string.IsNullOrEmpty(feedbackVm.ServerLogAttachmentPath) && _environmentModel != null)
            {
                feedbackVm.ServerLogAttachmentPath = _environmentModel.ResourceRepository.GetServerLogTempPath(_environmentModel);
            }
            WindowManager.ShowWindow(feedbackVm);
        }
    }
}
