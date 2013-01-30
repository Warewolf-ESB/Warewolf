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

            if (!EnsureNoFeedbackSessionsInProgress())
            {
                return;
            }

            InvokeAction(feedbackAction);
        }

        private bool EnsureNoFeedbackSessionsInProgress()
        {
            if(CurrentAction == null)
            {
                return true;
            }

            MessageBoxResult result = Popup.Show("Another feedback session is in progress, would you like to cancel it?", "Feedback in Progress", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if(result != MessageBoxResult.Yes)
            {
                return false;
            }

            IAsyncFeedbackAction asyncFeedback = CurrentAction as IAsyncFeedbackAction;
            if(asyncFeedback != null)
            {
                asyncFeedback.CancelFeedback();
            }

            CurrentAction = null;
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
