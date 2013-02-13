using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Dev2.Studio.Feedback
{
    [Export(typeof(IFeedbackInvoker))]
    public class FeedbackInvoker : IFeedbackInvoker, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged Implementation

        #region Class Members

        private IFeedbackAction _currentFeedbackAction;

        #endregion Class Members

        #region Properties

        [Import]
        public IPopUp Popup { get; set; }

        public IFeedbackAction CurrentAction
        {
            get
            {
                return _currentFeedbackAction;
            }
            private set
            {
                _currentFeedbackAction = value;
                OnPropertyChanged("CurrentAction");
            }
        }

        #endregion Properties

        #region Methods

        public void InvokeFeedback(IFeedbackAction feedbackAction)
        {
            InvokeFeedbackImpl(feedbackAction);
        }

        public void InvokeFeedback(IAsyncFeedbackAction feedbackAction)
        {
            InvokeFeedbackImpl(feedbackAction);
        }

        public void InvokeFeedback(IFeedbackAction emailFeedbackAction, IAsyncFeedbackAction recordedFeedbackAction)
        {
            if (emailFeedbackAction == null)
            {
                throw new ArgumentNullException("emailFeedbackAction");
            }

            if (recordedFeedbackAction == null)
            {
                throw new ArgumentNullException("recordedFeedbackAction");
            }

            IFeedbackAction actionToInvoke = null;

            if (recordedFeedbackAction.CanProvideFeedback)
            {
                MessageBoxResult result = Popup.Show("The ability to give feedback by recording steps is available on your system, would you like to use it?", "Recorded Feedback", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    actionToInvoke = recordedFeedbackAction;
                }
                else if (result == MessageBoxResult.No)
                {
                    actionToInvoke = emailFeedbackAction;
                }
            }
            else
            {
                actionToInvoke = emailFeedbackAction;
            }

            if (actionToInvoke == null)
            {
                return;
            }

            InvokeFeedbackImpl(actionToInvoke);
        }

        #endregion Methods

        #region Private Methods

        private void InvokeFeedbackImpl(IFeedbackAction feedbackAction)
        {
            if (feedbackAction == null)
            {
                throw new ArgumentNullException("feedbackAction");
            }

            if (!feedbackAction.CanProvideFeedback)
            {
                Popup.Show("Unable to provide feedback at this time.", "Feedback Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string feedbackType = feedbackAction.ToString();
            feedbackType = feedbackType.Remove(0, feedbackType.LastIndexOf(".", System.StringComparison.Ordinal) + 1);
            if (!EnsureNoFeedbackSessionsInProgress(feedbackType))
            {
                return;
            }

            InvokeAction(feedbackAction);
        }

        private bool EnsureNoFeedbackSessionsInProgress(string feedbackType)
        {
            if(CurrentAction == null)
            {
                return true;
            }

            if (feedbackType == "RecorderFeedbackAction" || feedbackType == "IAsyncFeedbackActionProxy") // IAsyncFeedbackActionProxy required for Mocking
            {
                MessageBoxResult result = Popup.Show("Another feedback session is in progress, would you like to cancel it?", "Feedback in Progress", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.No)
                {
                    return false;
                }

                IAsyncFeedbackAction asyncFeedback = CurrentAction as IAsyncFeedbackAction;
                if (asyncFeedback != null)
                {
                    asyncFeedback.CancelFeedback();
                }
            }
            else // A feedback recording is in progress, but the user did not want to use the Feedback Recorder
            {
                IAsyncFeedbackAction asyncFeedback = CurrentAction as IAsyncFeedbackAction; // Get the current recording
                if (asyncFeedback != null)
                {
                    MessageBoxResult result = Popup.Show("As a previous feedback recording session was in progress, it has been cancelled. Would you like to attach its recording file to your feedback?", "Attach previous feedback recording", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // If the user wants to use the previous recording, use it
                        asyncFeedback.FinishFeedBack();
                        CurrentAction = null;
                        return false;
                    }
                    else
                    {
                        // If the user doesn't want to use the previous recording, discard it, and carry on as per usual
                        asyncFeedback.CancelFeedback();
                        return true;
                    }
                }
            }
            return true;
        }

        private void InvokeAction(IFeedbackAction feedbackAction)
        {
            IAsyncFeedbackAction asyncFeedbackAction = feedbackAction as IAsyncFeedbackAction;

            if (asyncFeedbackAction == null)
            {
                CurrentAction = feedbackAction;
                feedbackAction.StartFeedback();
                CurrentAction = null;
            }
            else
            {
                CurrentAction = asyncFeedbackAction;
                asyncFeedbackAction.StartFeedback(e =>
                {
                    CurrentAction = null;
                });
            }
        }

        #endregion Private Methods
    }
}
