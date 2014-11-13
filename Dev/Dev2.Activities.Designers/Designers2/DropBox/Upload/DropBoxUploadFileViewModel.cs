
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
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
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
        IEnvironmentModel _environmentModel;
        private readonly string[] _operations = { "Read File", "Write File" };
        readonly IEventAggregator _eventPublisher;
        public DropBoxUploadFileViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), o => IsDropboxSourceSelected);
        }

        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource!= null ;
                
            }
            
        }

        public DropBoxUploadFileViewModel(ModelItem modelItem)
            : this(modelItem, EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
          

        }

        public override void Validate()
        {
        }

        public IEnumerable<OauthSource> Sources
        {
            get { return _environmentModel.ResourceRepository.FindSourcesByType<OauthSource>(_environmentModel, enSourceType.OauthSource); }
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
                SetProperty(value);
                OnPropertyChanged("IsDropboxSourceSelected");
                OnPropertyChanged("SelectedSource");
            }
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
