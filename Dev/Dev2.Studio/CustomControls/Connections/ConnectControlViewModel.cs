using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.UI
{
    // Moved code incorrectly put into ConnectViewModel here
    public class ConnectControlViewModel:DependencyObject,INotifyPropertyChanged
    {
        readonly IEnvironmentModel _activeEnvironment;

        public ConnectControlViewModel(IEnvironmentModel activeEnvironment)
        {
            if(activeEnvironment == null)
            {
                throw new ArgumentNullException("activeEnvironment");
            }
            _activeEnvironment = activeEnvironment;
            Servers = new ObservableCollection<IEnvironmentModel>();
        }

        public IEnvironmentModel ActiveEnvironment
        {
            get
            {

                return _activeEnvironment;
            }
        }

        public void ChangeSelected(IEnvironmentModel activeEnvironment)
        {
            IsSelectedFromDropDown = false;
            LoadServers(activeEnvironment);
            IsSelectedFromDropDown = true;
        }

        public IEnvironmentModel GetSelectedServer(ObservableCollection<IEnvironmentModel> servers, string labelText)
        {
            if(servers == null || servers.Count == 0)
            {
                return null;
            }

            if(string.IsNullOrEmpty(labelText) || !labelText.Contains("Destination"))
            {
                IEnvironmentModel firstOrDefault = servers.FirstOrDefault(s => s.Name == _activeEnvironment.Name);
                if (firstOrDefault != null)
                {
                    return firstOrDefault;
                }

                IEnvironmentModel first = servers.First(s => s.IsLocalHost());

                return first;
            }

            if(_activeEnvironment.IsLocalHost() && servers.Count(itm => !itm.IsLocalHost() && itm.IsConnected) == 1)
            {
                //Select the only other connected server
                var otherServer = servers.FirstOrDefault(itm => !itm.IsLocalHost() && itm.IsConnected);
                if(otherServer != null)
                {
                    return otherServer;
                }
            }

            if(_activeEnvironment.IsLocalHost() && servers.Count(itm => !itm.IsLocalHost()) == 1)
            {
                //Select and connect to the only other server
                var otherServer = servers.FirstOrDefault(itm => !itm.IsLocalHost());
                if(otherServer != null)
                {
                    otherServer.Connect();
                    otherServer.ForceLoadResources();
                    return otherServer;
                }
            }

            if(!_activeEnvironment.IsLocalHost())
            {
                //Select localhost
                return servers.FirstOrDefault(itm => itm.IsLocalHost());
            }

            return null;
        }
        #region Servers

        public ObservableCollection<IEnvironmentModel> Servers { get; set; }

        // Using a DependencyProperty as the backing store for Servers.  This enables animation, styling, binding, etc...
        #endregion
        public bool? IsEditEnabled
        {
            get { return (bool?)GetValue(IsEditEnabledProperty); }
            set { SetValue(IsEditEnabledProperty, value); }
        }

        

        public static readonly DependencyProperty IsEditEnabledProperty =
            DependencyProperty.Register("IsEditEnabled", typeof(bool?), typeof(ConnectControlViewModel));

        #region Selected Server

        public IEnvironmentModel SelectedServer
        {
            get
            {
                return (IEnvironmentModel)GetValue(SelectedServerProperty);
            }
            set
            {
                SetValue(SelectedServerProperty, value);
                OnPropertyChanged();
            }
        }

        public bool IsSelectedFromDropDown { get; set; }

        // Using a DependencyProperty as the backing store for SelectedServer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerProperty =
            DependencyProperty.Register("SelectedServer", typeof(IEnvironmentModel), typeof(ConnectControlViewModel),
                new PropertyMetadata(null, ServerChangedCallBack));

        static void ServerChangedCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = (ConnectControlViewModel)dependencyObject;
            var selectedServer = e.NewValue as IEnvironmentModel;
            if(selectedServer != null)
            {
                var server = control.Servers.FirstOrDefault(s => s.ID == selectedServer.ID);
                control.SelectedServer = server;
            }
        }

        #endregion

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
                    if(!IsSelectedFromDropDown)
                    {
                        SelectedServer = server;
                    }
                    IsEditEnabled = (SelectedServer != null && !SelectedServer.IsLocalHost());
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}