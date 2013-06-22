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
    public abstract class DsfSendEmailActivityViewModelBase : SimpleBaseViewModel
    {
        #region Fields

        private DsfSendEmailActivity _activity;
        private ICommand _editEmailSourceCommand;
        ObservableCollection<EmailSource> _emailSourceList;
        private readonly EmailSource _baseEmailSource = new EmailSource();
        bool _canEditSource;
        bool _hasClickedSave;

        #endregion

        #region Ctor

        public DsfSendEmailActivityViewModelBase(DsfSendEmailActivity activity)
        {
            _activity = activity ?? new DsfSendEmailActivity();
            _baseEmailSource.ResourceName = "New Email Source...";
            EmailSourceList.Add(_baseEmailSource);
            UpdateEnvironmentResources();
        }

        #endregion

        #region Properties

        public bool CanEditSource
        {
            get
            {
                return _canEditSource;
            }
            set
            {
                _canEditSource = value;
            }
        }

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
                if (value == _baseEmailSource)
                {
                    CreateNewEmailSource();                    
                }
                else
                {                                 
                    _activity.SelectedEmailSource = value;
                    if(value != null)
                    {
                        FromAccount = value.UserName;
                    }
                    
                  NotifyOfPropertyChange(() => SelectedEmailSource);
                }
                
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

        public ICommand EditEmailSourceCommand
        {
            get
            {
                return _editEmailSourceCommand ??
                       (_editEmailSourceCommand = new RelayCommand(param => EditEmailSource()));
            }
        }

        #endregion

        #region Methods

        public void UpdateEnvironmentResourcesCallback(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null)
            {
                List<EmailSource> tmpSourceList = EmailSourceList.ToList();
                var sourceList = GetSources(environmentModel);
                EmailSourceList.Clear();
                _baseEmailSource.ResourceName = "New Email Source...";
                EmailSourceList.Add(_baseEmailSource);
                foreach (var unlimitedObject in sourceList)
                {
                    var emailSource = new EmailSource(unlimitedObject.xmlData);
                    EmailSourceList.Add(emailSource);
                }
                if (EmailSourceList.Count > tmpSourceList.Count)
                {
                    foreach (EmailSource source in EmailSourceList)
                    {
                        if (tmpSourceList.FirstOrDefault(c => c.ResourceName == source.ResourceName) == null)
                        {
                            SelectedEmailSource = source;
                            break;
                        }
                    }
                }
                else
                {
                    SelectedEmailSource = null;
                }
            }
        }

        public void CreateNewEmailSource()
        {
            _hasClickedSave = true;
            EventAggregator.Publish(new ShowNewResourceWizard("EmailSource"));
            UpdateEnvironmentResources();
        }

        public void EditEmailSource()
        {
            Action<IEnvironmentModel> callback = EditEmailSource;
            EventAggregator.Publish(new GetActiveEnvironmentCallbackMessage(callback));
        }


        public abstract List<UnlimitedObject> GetSources(IEnvironmentModel environmentModel);

        #endregion       

        #region Private Methods


        private void UpdateEnvironmentResources()
        {
            Action<IEnvironmentModel> callback = UpdateEnvironmentResourcesCallback;
            EventAggregator.Publish(new GetActiveEnvironmentCallbackMessage(callback));
        }

        private void EditEmailSource(IEnvironmentModel env)
        {
            if (SelectedEmailSource != null)
            {
                IResourceModel resourceModel = env.ResourceRepository.FindSingle(c => c.ResourceName == SelectedEmailSource.ResourceName);
                if (resourceModel != null)
                {
                    EventAggregator.Publish(new ShowEditResourceWizardMessage(resourceModel));
                }
            }
        }

        #endregion
    }
}
