using System;
using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

// ReSharper disable once CheckNamespace
namespace Dev2.UI
{
    public class ConnectControlViewModel : DependencyObject, IHandle<UpdateSelectedServer>, IHandle<UpdateActiveEnvironmentMessage>
    {
        #region Fields

        readonly IEnvironmentModel _activeEnvironment;
        readonly IEventAggregator _eventPublisher;

        #endregion

        #region Ctor

        public ConnectControlViewModel(IEnvironmentModel activeEnvironment, IEventAggregator eventAggregator)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventAggregator);
            _eventPublisher = eventAggregator;
            _eventPublisher.Subscribe(this);
            if(activeEnvironment != null)
            {
                _activeEnvironment = activeEnvironment;
            }
            Servers = new ObservableCollection<IEnvironmentModel>();
        }

        #endregion

        #region Properties

        public bool IsSelectionFromTree { get; set; }

        public ObservableCollection<IEnvironmentModel> Servers { get; set; }

        public IEnvironmentModel ActiveEnvironment
        {
            get
            {
                return _activeEnvironment;
            }
        }

        #endregion

        #region Dependancy Properties

        #region IsDropDownEnabled

        public bool IsDropDownEnabled
        {
            get { return (bool)GetValue(IsDropDownEnabledProperty); }
            set { SetValue(IsDropDownEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownEnabledProperty =
            DependencyProperty.Register("IsDropDownEnabled", typeof(bool), typeof(ConnectControlViewModel), new PropertyMetadata(true));


        #endregion

        #region LabelText

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string),
                typeof(ConnectControlViewModel), new PropertyMetadata("Server "));

        #endregion

        #region IsEditEnabled

        public bool? IsEditEnabled
        {
            get { return (bool?)GetValue(IsEditEnabledProperty); }
            set { SetValue(IsEditEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEditEnabledProperty =
            DependencyProperty.Register("IsEditEnabled", typeof(bool?), typeof(ConnectControlViewModel));

        #endregion

        #region Selected Server

        public IEnvironmentModel SelectedServer
        {
            get { return (IEnvironmentModel)GetValue(SelectedServerProperty); }
            set { SetValue(SelectedServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedServer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerProperty =
            DependencyProperty.Register("SelectedServer", typeof(IEnvironmentModel), typeof(ConnectControlViewModel), new PropertyMetadata(null, OnSelectedServerChanged));

        static void OnSelectedServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ConnectControlViewModel)d;
            var newValue = e.NewValue as IEnvironmentModel;
            if(viewModel.IngoreSelectionChangedMessage)
            {
                viewModel._eventPublisher.Publish(new ServerSelectionChangedMessage(viewModel.SelectedServer));
            }

            if(newValue != null)
            {
                if(!viewModel.IsSelectionFromTree)
                {
                    if(viewModel.BindToActiveEnvironment)
                    {
                        viewModel._eventPublisher.Publish(new AddServerToExplorerMessage(viewModel.SelectedServer, viewModel.Context, true));
                        viewModel._eventPublisher.Publish(new SetSelectedItemInExplorerTree(newValue.Name));
                        viewModel._eventPublisher.Publish(new SetActiveEnvironmentMessage(newValue, true));
                    }
                    else
                    {
                        var isSourceServer = viewModel.IsSourceServer;
                        viewModel._eventPublisher.Publish(new AddServerToDeployMessage(viewModel.SelectedServer, viewModel.Context) { IsDestination = !isSourceServer, IsSource = isSourceServer });
                    }
                }
            }
            else
            {
                viewModel.IsEditEnabled = false;
            }
        }


        #endregion

        #region BindToActiveEnvironment

        public bool BindToActiveEnvironment
        {
            get { return (bool)GetValue(BindToActiveEnvironmentProperty); }
            set { SetValue(BindToActiveEnvironmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindToActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindToActiveEnvironmentProperty =
            DependencyProperty.Register("BindToActiveEnvironment", typeof(bool), typeof(ConnectControlViewModel), new PropertyMetadata(false));

        #endregion

        #region Context

        public Guid? Context
        {
            get { return (Guid?)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(Guid?), typeof(ConnectControlViewModel));

        #endregion

        #region IngoreSelectionChangedMessage

        public bool IngoreSelectionChangedMessage
        {
            get { return (bool)GetValue(IngoreSelectionChangedMessageProperty); }
            set { SetValue(IngoreSelectionChangedMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IngoreSelectionChangedMessageProperty =
            DependencyProperty.Register("IngoreSelectionChangedMessage", typeof(bool),
                typeof(ConnectControlViewModel), new PropertyMetadata(false));

        #endregion

        #endregion

        #region Handlers

        #region Implementation of IHandle<SetActiveEnvironmentMessage>

        public void Handle(UpdateActiveEnvironmentMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(message.EnvironmentModel != null && BindToActiveEnvironment)
            {
                IsSelectionFromTree = true;
                SelectedServer = message.EnvironmentModel;
                IsSelectionFromTree = false;
            }
        }

        #endregion

        #region Implementation of IHandle<UpdateSelectedServer>

        public void Handle(UpdateSelectedServer updateSelectedServer)
        {
            if(updateSelectedServer.IsSourceServer && IsSourceServer)
            {
                SelectedServer = updateSelectedServer.EnvironmentModel;
            }
            else if(!updateSelectedServer.IsSourceServer && !IsSourceServer)
            {
                SelectedServer = updateSelectedServer.EnvironmentModel;
            }
        }

        public bool IsSourceServer { get { return LabelText.Contains("Source"); } }

        #endregion

        #endregion

        #region Public Methods

        public void LoadServers(IEnvironmentModel envModel = null)
        {
            IsEditEnabled = false;
            Servers.Clear();
            var servers = ServerProvider.Instance.Load();
            foreach(var server in servers)
            {
                Servers.Add(server);
                if(envModel != null && server.Name == envModel.Name)
                {
                    SelectedServer = server;
                    IsEditEnabled = (SelectedServer != null && !SelectedServer.IsLocalHost);
                }
            }
        }

        #endregion
    }
}