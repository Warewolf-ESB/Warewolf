using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Data.Enums;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Unlimited.Framework;

namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public abstract class DsfSendEmailActivityViewModelBase : SimpleBaseViewModel, IHandle<UpdateResourceMessage>
    {
        #region Fields

        private DsfSendEmailActivity _activity;       
        private ICommand _createNewEmailSourceCommand;
        ObservableCollection<EmailSource> _emailSourceList;

        #endregion        

        #region Ctor

        public DsfSendEmailActivityViewModelBase(DsfSendEmailActivity activity)
        {
            _activity = activity ?? new DsfSendEmailActivity();
            UpdateEnvironmentResources();            
        }

        #endregion

        #region Properties

        public ObservableCollection<EmailSource> EmailSourceList
        {
            get
            {
                return _emailSourceList ?? (_emailSourceList = new ObservableCollection<EmailSource>());
            }
        }

        public EmailSource SelectedEmailSource
        {
            get
            {
                return _activity.SelectedEmailSource;
            }
            set
            {                
                _activity.SelectedEmailSource = value;
                OnPropertyChanged("SelectedEmailSource");
            }
        }

        public string FromAccount
        {
            get
            {
                return _activity.FromAccount;
            }
            set
            {
                _activity.FromAccount = value;
                OnPropertyChanged("FromAccount");
            }
        }

        public string Password
        {
            get
            {
                return _activity.Password;
            }
            set
            {
                _activity.Password = value;
                OnPropertyChanged("Password");
            }
        }

        public string To
        {
            get
            {
                return _activity.To;
            }
            set
            {
                _activity.To = value;
                OnPropertyChanged("To");
            }
        }

        public string Cc
        {
            get
            {
                return _activity.Cc;
            }
            set
            {
                _activity.Cc = value;
                OnPropertyChanged("Cc");
            }
        }

        public string Bcc
        {
            get
            {
                return _activity.Bcc;
            }
            set
            {
                _activity.Bcc = value;
                OnPropertyChanged("Bcc");
            }
        }

        public enMailPriorityEnum Priority
        {
            get
            {
                return _activity.Priority;
            }
            set
            {
                _activity.Priority = value;
                OnPropertyChanged("Priority");
            }
        }

        public string Subject
        {
            get
            {
                return _activity.Subject;
            }
            set
            {
                _activity.Subject = value;
                OnPropertyChanged("Subject");
            }
        }

        public string Attachments
        {
            get
            {
                return _activity.Attachments;
            }
            set
            {
                _activity.Attachments = value;
                OnPropertyChanged("Attachments");
            }
        }

        public string Body
        {
            get
            {
                return _activity.Body;
            }
            set
            {
                _activity.Body = value;
                OnPropertyChanged("Body");
            }
        }

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        public new string Result
        {
            get
            {
                return _activity.Result;
            }
            set
            {
                _activity.Result = value;
                OnPropertyChanged("Result");
            }
        }

        #endregion

        #region Commands

        public ICommand CreateNewEmailSourceCommand
        {
            get
            {
                return _createNewEmailSourceCommand ??
                       (_createNewEmailSourceCommand = new RelayCommand(param => CreateNewEmailSource()));
            }
        }        

        #endregion

        #region Methods

        public void UpdateEnvironmentResourcesCallback(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null)
            {
                var sourceList = GetSources(environmentModel);
                EmailSourceList.Clear();
                foreach (var unlimitedObject in sourceList)
                {
                    var emailSource = new EmailSource(unlimitedObject.xmlData);
                    EmailSourceList.Add(emailSource);
                    if (SelectedEmailSource == null || emailSource.ResourceName == SelectedEmailSource.ResourceName)
                    {
                        SelectedEmailSource = emailSource;
                    }
                }
            }
        }

        public void CreateNewEmailSource()
        {
            EventAggregator.Publish(new ShowNewResourceWizard("EmailSource"));            
        }

        public abstract List<UnlimitedObject> GetSources(IEnvironmentModel environmentModel);        

        #endregion

        #region Implementation of IHandle<UpdateResourceMessage>

        public void Handle(UpdateResourceMessage message)
        {
            if (message.ResourceModel.ResourceType == ResourceType.Source)
            {
                UpdateEnvironmentResources();
                var newSource = EmailSourceList.FirstOrDefault(c=>c.ResourceName == message.ResourceModel.ResourceName);
                if(newSource != null)
                {
                    SelectedEmailSource = newSource;
                    UpdateEnvironmentResources();
                }               
            }
        }

        #endregion

        #region Private Methods

        private void UpdateEnvironmentResources()
        {
            Action<IEnvironmentModel> callback = UpdateEnvironmentResourcesCallback;
            EventAggregator.Publish(new GetActiveEnvironmentCallbackMessage(callback));
        }        

        #endregion        
    }
}
