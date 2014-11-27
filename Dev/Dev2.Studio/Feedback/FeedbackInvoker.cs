
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
using System.ComponentModel;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Feedback
{
    public class FeedbackInvoker : IFeedbackInvoker, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public FeedbackInvoker()
        {
            Popup = CustomContainer.Get<IPopupController>();
        }

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

        public IPopupController Popup { get; set; }

        public IFeedbackAction CurrentAction
        {
            get
            {
                return _currentFeedbackAction;
            }
            set
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

        // 14 Feb 2013 - Michael Cullen - Modified to fix Bug 8809
        public void InvokeFeedback(IFeedbackAction emailFeedbackAction, IAsyncFeedbackAction recordedFeedbackAction)
        {
            // The person clicked the Feedback button
            if(emailFeedbackAction == null)
            {
                throw new ArgumentNullException("emailFeedbackAction");
            }

            if(recordedFeedbackAction == null)
            {
                throw new ArgumentNullException("recordedFeedbackAction");
            }

            IFeedbackAction actionToInvoke = null;

            if(recordedFeedbackAction.CanProvideFeedback)
            {
                IAsyncFeedbackAction asyncFeedback = CurrentAction as IAsyncFeedbackAction;
                if(asyncFeedback != null) // If a recording session is already in progress, ask the user if he wants to stop it.
                {

                    MessageBoxResult result = Popup.Show("Another feedback session is in progress - Would you like to stop it?", "Feedback in Progress", MessageBoxButton.YesNo, MessageBoxImage.Question, null);
                    if(result == MessageBoxResult.Yes)
                    {
                        asyncFeedback.FinishFeedBack();
                    }
                }
                else // Else give him the normal message asking what he wants to do.
                {
                    MessageBoxResult result = Popup.Show("The ability to give feedback by recording steps is available on your system - Would you like to use it?", "Recorded Feedback", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, null);

                    if(result == MessageBoxResult.Yes)
                    {
                        actionToInvoke = recordedFeedbackAction;
                    }
                    else if(result == MessageBoxResult.No)
                    {
                        actionToInvoke = emailFeedbackAction;
                    }
                }
            }
            else
            {
                actionToInvoke = emailFeedbackAction;
            }

            if(actionToInvoke == null)
            {
                return;
            }

            InvokeFeedbackImpl(actionToInvoke);
        }

        #endregion Methods

        #region Private Methods

        private void InvokeFeedbackImpl(IFeedbackAction feedbackAction)
        {
            if(feedbackAction == null)
            {
                throw new ArgumentNullException("feedbackAction");
            }

            if(!feedbackAction.CanProvideFeedback)
            {
                Popup.Show("Unable to provide feedback at this time.", "Feedback Unavailable", MessageBoxButton.OK, MessageBoxImage.Error, null);
                return;
            }

            if(!EnsureNoFeedbackSessionsInProgress())
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

            MessageBoxResult result = Popup.Show("Another feedback session is in progress - Would you like to cancel it?", "Feedback in Progress", MessageBoxButton.YesNo, MessageBoxImage.Error, null);
            if(result == MessageBoxResult.No)
            {
                return false;
            }

            IAsyncFeedbackAction asyncFeedback = CurrentAction as IAsyncFeedbackAction;
            if(asyncFeedback != null)
            {
                asyncFeedback.CancelFeedback();
            }
            return true;
        }

        private void InvokeAction(IFeedbackAction feedbackAction)
        {
            IAsyncFeedbackAction asyncFeedbackAction = feedbackAction as IAsyncFeedbackAction;

            if(asyncFeedbackAction == null)
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
