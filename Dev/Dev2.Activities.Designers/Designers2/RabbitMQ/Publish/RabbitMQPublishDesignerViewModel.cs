/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Dev2.Activities.Designers2.RabbitMQ.Publish
{
    // ReSharper disable InconsistentNaming
    public class RabbitMQPublishDesignerViewModel : ActivityDesignerViewModel, INotifyPropertyChanged
    // ReSharper restore InconsistentNaming
    {
        // ReSharper disable InconsistentNaming
        private static readonly RabbitMQSource NewRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "New RabbitMQ Source..." };

        private static readonly RabbitMQSource SelectRabbitMQSource = new RabbitMQSource { ResourceID = Guid.NewGuid(), ResourceName = "Select a RabbitMQ Source..." };
        // ReSharper restore InconsistentNaming

        private readonly IEventAggregator _eventPublisher;
        private readonly IEnvironmentModel _environmentModel;

        //public Func<string> GetDatalistString = () => DataListSingleton.ActiveDataList.Resource.DataList;

        public RabbitMQPublishDesignerViewModel(ModelItem modelItem)
            : this(modelItem, EnvironmentRepository.Instance.ActiveEnvironment, EventPublishers.Aggregator)
        {
        }

        public RabbitMQPublishDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("environmentModel", environmentModel);

            _environmentModel = environmentModel;
            _eventPublisher = eventPublisher;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;

            EditRabbitMQSourceCommand = new RelayCommand(o => EditRabbitMQSource(), o => IsEditableRabbitMQSourceSelected);

            IsRefreshing = true;
            RabbitMQSources = LoadRabbitMQSources();
            SetSelectedRabbitMQSource(SelectedRabbitMQSource);
            IsRefreshing = false;
            AddTitleBarLargeToggle();
        }

        // ReSharper disable InconsistentNaming
        public ObservableCollection<RabbitMQSource> RabbitMQSources { get; private set; }

        public RelayCommand EditRabbitMQSourceCommand { get; private set; }

        private bool IsEditableRabbitMQSourceSelected
        {
            get
            {
                return SelectedRabbitMQSource != null && SelectedRabbitMQSource != SelectRabbitMQSource && SelectedRabbitMQSource != NewRabbitMQSource;
            }
        }

        public bool IsRefreshing { get { return (bool)GetValue(IsRefreshingProperty); } set { SetValue(IsRefreshingProperty, value); } }

        public static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.Register("IsRefreshing", typeof(bool), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedRabbitMQSourceProperty = DependencyProperty.Register("SelectedRabbitMQSource", typeof(RabbitMQSource), typeof(RabbitMQPublishDesignerViewModel), new PropertyMetadata(null, OnSelectedRabbitMQSourceChanged));

        private static void OnSelectedRabbitMQSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (RabbitMQPublishDesignerViewModel)d;
            if (viewModel.IsRefreshing)
            {
                return;
            }
            viewModel.OnRabbitMQSourceChanged();
            viewModel.EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
        }

        protected void OnRabbitMQSourceChanged()
        {
            if (SelectedRabbitMQSource == NewRabbitMQSource)
            {
                CreateRabbitMQSource();
                return;
            }

            if (SelectedRabbitMQSource != SelectRabbitMQSource && !IsRefreshing)
            {
                RabbitMQSources.Remove(SelectRabbitMQSource);
            }
            RabbitMQSourceResourceId = SelectedRabbitMQSource.ResourceID;
        }

        public RabbitMQSource SelectedRabbitMQSource
        {
            get
            {
                return (RabbitMQSource)GetValue(SelectedRabbitMQSourceProperty);
            }
            set
            {
                SetValue(SelectedRabbitMQSourceProperty, value);
                EditRabbitMQSourceCommand.RaiseCanExecuteChanged();
            }
        }

        // ReSharper restore InconsistentNaming

        public Guid RabbitMQSourceResourceId
        {
            get
            {
                return GetProperty<Guid>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public string QueueName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsDurable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExclusive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAutoDelete
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Message
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Result
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        // ReSharper disable InconsistentNaming
        private void CreateRabbitMQSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("RabbitMQSource"));
            IsRefreshing = true;
            RabbitMQSources = LoadRabbitMQSources();
            RabbitMQSource newRabbitMQSources = RabbitMQSources.FirstOrDefault(source => source.IsNewResource);
            SetSelectedRabbitMQSource(newRabbitMQSources);
            IsRefreshing = false;
        }

        private ObservableCollection<RabbitMQSource> LoadRabbitMQSources()
        {
            var rabbitMQSources = new[]
            {
                new RabbitMQSource()
                {
                    ResourceID = new Guid("00000000-0000-0000-0000-000000000001"),
                    ResourceType = ResourceType.RabbitMQSource,
                    ResourceName = "Test (localhost)",
                    Host = "localhost",
                    UserName = "guest",
                    Password = "guest"
                }
            }.ToObservableCollection();

            // TODO: Remove the above stub and uncomment below when new RabbitMQSource has been implemented WOLF-1523
            //var rabbitMQSources = _environmentModel.ResourceRepository.FindSourcesByType<RabbitMQSource>(_environmentModel, enSourceType.RabbitMQSource);
            rabbitMQSources.Insert(0, NewRabbitMQSource);
            return rabbitMQSources.ToObservableCollection();
        }

        private void SetSelectedRabbitMQSource(RabbitMQSource rabbitMQSource)
        {
            var selectRabbitMQSource = rabbitMQSource ?? RabbitMQSources.FirstOrDefault(d => d.ResourceID == RabbitMQSourceResourceId);
            if (selectRabbitMQSource == null)
            {
                if (RabbitMQSources.FirstOrDefault(d => d.Equals(SelectRabbitMQSource)) == null)
                {
                    RabbitMQSources.Insert(0, SelectRabbitMQSource);
                }
                selectRabbitMQSource = SelectRabbitMQSource;
            }
            SelectedRabbitMQSource = selectRabbitMQSource;
        }

        private void EditRabbitMQSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedRabbitMQSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);
        }

        // ReSharper restore InconsistentNaming

        public override void Validate()
        {
            //var result = new List<IActionableErrorInfo>();
            //result.AddRange(ValidateThis());
            //Errors = result.Count == 0 ? null : result;
        }

        //private IEnumerable<IActionableErrorInfo> ValidateThis()
        //{
        //    foreach (var error in GetRuleSet("RabbitMQSource", GetDatalistString()).ValidateRules("'RabbitMQ Source'", () => IsRabbitMQSourceFocused = true))
        //    {
        //        yield return error;
        //    }
        //    foreach (var error in GetRuleSet("QueueName", GetDatalistString()).ValidateRules("'Queue Name'", () => IsQueueNameFocused = true))
        //    {
        //        yield return error;
        //    }
        //    foreach (var error in GetRuleSet("Message", GetDatalistString()).ValidateRules("'Message'", () => IsMessageFocused = true))
        //    {
        //        yield return error;
        //    }
        //}

        //private IRuleSet GetRuleSet(string propertyName, string datalist)
        //{
        //    var ruleSet = new RuleSet();

        //    switch (propertyName)
        //    {
        //        case "RabbitMQSource":
        //            ruleSet.Add(new IsNullRule(() => SelectedRabbitMQSource));
        //            break;

        //        case "QueueName":
        //            ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => QueueName));
        //            break;

        //        case "Message":
        //            ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => Message));
        //            break;
        //    }
        //    return ruleSet;
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }
    }
}