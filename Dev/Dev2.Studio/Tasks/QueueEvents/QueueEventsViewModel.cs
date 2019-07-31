/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Tasks.QueueEvents
{
    public class QueueEventsViewModel : TasksItemViewModel, IUpdatesHelp
    {
        ICommand _newCommand;
        ICommand _deleteCommand;
        IResource _selectedQueueSource;
        string _selectedQueueEvent;
        string _queueName;
        string _workflowName;
        int _concurrency;
        private IServer _server;
        private IResourceRepository _resourceRepository;
        private ObservableCollection<INameValue> _queueNames;

        public QueueEventsViewModel(IServer server)
        {
            _server = server;
            _resourceRepository = server.ResourceRepository;
        }

        public ObservableCollection<string> QueueEvents { get; set; }

        public string SelectedQueueEvent
        {
            get => _selectedQueueEvent;
            set
            {
                _selectedQueueEvent = value;
                OnPropertyChanged(nameof(SelectedQueueEvent));
            }
        }

        public List<IResource> QueueSources => _resourceRepository.FindResourcesByType<IQueueSource>(_server);

        public IResource SelectedQueueSource
        {
            get => _selectedQueueSource;
            set
            {
                _selectedQueueSource = value;
                if (_selectedQueueSource != null)
                {
                    QueueNames = GetQueueNamesFromSource();
                }
                
                OnPropertyChanged(nameof(SelectedQueueSource));
            }
        }

        private ObservableCollection<INameValue> GetQueueNamesFromSource()
        {
            var queueNames = new ObservableCollection<INameValue>();

            var list = _resourceRepository.FindAutocompleteOptions(_server, SelectedQueueSource);

#pragma warning disable CC0021 // Use nameof
            foreach (var item in list["QueueNames"])
#pragma warning restore CC0021 // Use nameof
            {
                var nameValue = new NameValue(item, item);
                queueNames.Add(nameValue);
            }

            return queueNames;
        }

        public ObservableCollection<INameValue> QueueNames
        {
            get => _queueNames;
            set
            {
                _queueNames = value;
                OnPropertyChanged(nameof(QueueNames));
            }
        }

        public string QueueName
        {
            get => _queueName;
            set
            {
                _queueName = value;
                OnPropertyChanged(nameof(QueueName));
            }
        }

        public string WorkflowName
        {
            get => _workflowName;
            set
            {
                _workflowName = value;
                OnPropertyChanged(nameof(WorkflowName));
            }
        }

        public int Concurrency
        {
            get => _concurrency;
            set
            {
                _concurrency = value;
                OnPropertyChanged(nameof(Concurrency));
            }
        }

        public ICommand NewCommand => _newCommand ??
                       (_newCommand = new DelegateCommand(CreateNewQueueEvent));

        private void CreateNewQueueEvent(object queueObj)
        {
            QueueEvents.Add("");
        }

        public ICommand DeleteCommand => _deleteCommand ??
                       (_deleteCommand = new DelegateCommand(DeleteQueueEvent));

        private void DeleteQueueEvent(object queueObj)
        {
            QueueEvents.Remove("");
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        protected override void CloseHelp()
        {
            
        }

        public static bool Save()
        {
            return true;
        }
    }
}
