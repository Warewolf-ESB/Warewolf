
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Designers2.DropBox.Upload
{
    public class DropBoxUploadFileViewModel : ActivityDesignerViewModel,INotifyPropertyChanged
    {
        readonly IEnvironmentModel _environmentModel;
        private readonly string[] _operations = { "Read File", "Write File" };
        readonly IEventAggregator _eventPublisher;
        ObservableCollection<OauthSource> _sources;
        static readonly OauthSource NewOAuthSource = new OauthSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "New OAuth Source..."
        };
        static readonly OauthSource SelectOAuthSource = new OauthSource
        {
            ResourceID = Guid.NewGuid(),
            ResourceName = "Select a OAuth Source..."
        };
        bool _isRefreshing;

        public DropBoxUploadFileViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), o => IsDropboxSourceSelected);
            Sources = LoadOAuthSources();
            SetSelectedOAuthSource(SelectedSource);
        }

        ObservableCollection<OauthSource> LoadOAuthSources()
        {
            var oauthSources = _environmentModel.ResourceRepository.FindSourcesByType<OauthSource>(_environmentModel, enSourceType.OauthSource);
            oauthSources.Insert(0,NewOAuthSource);
            return oauthSources.ToObservableCollection();
        }

        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource!= null && SelectedSource!=SelectOAuthSource && SelectedSource!=NewOAuthSource;
            }
        }

        public DropBoxUploadFileViewModel(ModelItem modelItem)
            : this(modelItem, EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public override void Validate()
        {
        }

        public ObservableCollection<OauthSource> Sources
        {
            get
            {
                return _sources;
            }
            private set
            {
                _sources = value;
                OnPropertyChanged("Sources");
            }
        }

        void SetSelectedOAuthSource(OauthSource oAuthSource)
        {
            var selectOAuthSource = oAuthSource == null ? null : Sources.FirstOrDefault(d => d.ResourceID == oAuthSource.ResourceID);
            if (selectOAuthSource == null)
            {
                if (Sources.FirstOrDefault(d => d.Equals(SelectOAuthSource)) == null)
                {
                    Sources.Insert(0, SelectOAuthSource);
                }
                selectOAuthSource = SelectOAuthSource;
            }
            SelectedSource = selectOAuthSource;
        }

        public string[] Operations
        {
            get
            {
                return _operations;
                
            }
        }

        public string Operation
        {
            get {  return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                OnPropertyChanged("Operation");
            }
        }

        public OauthSource SelectedSource
        {
            get { return GetProperty<OauthSource>(); }
            // ReSharper restore ExplicitCallerInfoArgument
            set
            {
                if (value == NewOAuthSource && !_isRefreshing)
                {
                    CreateOAuthSource();
                    return;
                }
                SetProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDropboxSourceSelected");
                OnPropertyChanged("SelectedSource");
            }
        }

        void CreateOAuthSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            _isRefreshing = true;
            Sources = LoadOAuthSources();
            var newOAuthSource = Sources.FirstOrDefault(source => source.IsNewResource);
            SetSelectedOAuthSource(newOAuthSource);
            _isRefreshing = false;
        }

        public RelayCommand EditDropboxSourceCommand { get; private set; }
        private void EditDropBoxSource()
        {
            var resourceModel = _environmentModel.ResourceRepository.FindSingle(c => c.ID == SelectedSource.ResourceID);
            if (resourceModel != null)
            {
                _eventPublisher.Publish(new ShowEditResourceWizardMessage(resourceModel));
                OnPropertyChanged("Sources");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [ExcludeFromCodeCoverage]
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
