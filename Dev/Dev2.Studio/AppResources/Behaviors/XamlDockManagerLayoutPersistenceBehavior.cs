using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Infragistics.Windows.DockManager;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class XamlDockManagerLayoutPersistenceBehavior : Behavior<XamDockManager>, IHandle<IResetLayoutMessage>
    {
        #region Class Members

        private UserInterfaceLayoutModel _userInterfaceLayoutModel;

        #endregion Class Members

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            EventPublishers.Aggregator.Subscribe(this);

            if(Application.Current != null)
            {
                Application.Current.Exit -= AppOnExit;
                Application.Current.Exit += AppOnExit;
            }

            AssociatedObject.Initialized -= AssociatedObjectOnInitialized;
            AssociatedObject.Initialized += AssociatedObjectOnInitialized;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            EventPublishers.Aggregator.Unsubscribe(this);

            Application.Current.Exit -= AppOnExit;
            AssociatedObject.Initialized -= AssociatedObjectOnInitialized;
        }

        #endregion Override Methods

        #region Dependency Properties

        #region UserInterfaceLayoutRepository

        public IFrameworkRepository<UserInterfaceLayoutModel> UserInterfaceLayoutRepository
        {
            get { return (IFrameworkRepository<UserInterfaceLayoutModel>)GetValue(UserInterfaceLayoutRepositoryProperty); }
            set { SetValue(UserInterfaceLayoutRepositoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserInterfaceLayoutRepositoryProperty =
            DependencyProperty.Register("UserInterfaceLayoutRepository", typeof(IFrameworkRepository<UserInterfaceLayoutModel>), typeof(XamlDockManagerLayoutPersistenceBehavior), new PropertyMetadata(null, UserInterfaceLayoutRepositoryChangedCallback));

        private static void UserInterfaceLayoutRepositoryChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            XamlDockManagerLayoutPersistenceBehavior xamlDockManagerLayoutPersistenceBehavior = dependencyObject as XamlDockManagerLayoutPersistenceBehavior;
            if(xamlDockManagerLayoutPersistenceBehavior == null) return;
            if(xamlDockManagerLayoutPersistenceBehavior.UserInterfaceLayoutRepository == null) return;
            xamlDockManagerLayoutPersistenceBehavior.UserInterfaceLayoutRepository.Load();
            xamlDockManagerLayoutPersistenceBehavior.LoadLayout(xamlDockManagerLayoutPersistenceBehavior.LayoutName);
        }

        #endregion UserInterfaceLayoutRepository

        #region LayoutName

        public string LayoutName
        {
            get { return (string)GetValue(LayoutNameProperty); }
            set { SetValue(LayoutNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayoutName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayoutNameProperty =
            DependencyProperty.Register("LayoutName", typeof(string), typeof(XamlDockManagerLayoutPersistenceBehavior), new PropertyMetadata(default(string), LayoutNameyChangedCallback));

        private static void LayoutNameyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            XamlDockManagerLayoutPersistenceBehavior xamlDockManagerLayoutPersistenceBehavior = dependencyObject as XamlDockManagerLayoutPersistenceBehavior;
            if(xamlDockManagerLayoutPersistenceBehavior == null) return;

            if(e.NewValue == null) return;
            xamlDockManagerLayoutPersistenceBehavior.LoadLayout(e.NewValue.ToString());
        }

        #endregion LayoutName

        #region LayoutDataPropertyName

        public string LayoutDataPropertyName
        {
            get { return (string)GetValue(LayoutDataPropertyNameProperty); }
            set { SetValue(LayoutDataPropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayoutDataPropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayoutDataPropertyNameProperty =
            DependencyProperty.Register("LayoutDataPropertyName", typeof(string), typeof(XamlDockManagerLayoutPersistenceBehavior), new PropertyMetadata(default(string), LayoutDataPropertyNameChangedCallback));

        private static void LayoutDataPropertyNameChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            XamlDockManagerLayoutPersistenceBehavior xamlDockManagerLayoutPersistenceBehavior = dependencyObject as XamlDockManagerLayoutPersistenceBehavior;
            if(xamlDockManagerLayoutPersistenceBehavior == null) return;

            xamlDockManagerLayoutPersistenceBehavior.LoadLayout(xamlDockManagerLayoutPersistenceBehavior.LayoutName);
        }

        #endregion LayoutDataPropertyName

        #endregion Dependency Properties

        #region Static Properties

        #endregion Static Properties

        #region Private Methods

        private void LoadLayout(string layoutName)
        {
            if(AssociatedObject == null) return;

            if(UserInterfaceLayoutRepository == null)
            {
                AssociatedObject.LoadLayout(StringResources.XmlOriginalLayout);
                return;
            }

            _userInterfaceLayoutModel = UserInterfaceLayoutRepository.FindSingle(model => model.LayoutName == layoutName);
            if(_userInterfaceLayoutModel == null) return;

            PropertyInfo pi = _userInterfaceLayoutModel.GetType().GetProperty(LayoutDataPropertyName);
            if(pi == null) return;

            string layoutData = pi.GetValue(_userInterfaceLayoutModel, null) as string;
            if(layoutData == null) return;

            try
            {
                AssociatedObject.LoadLayout(layoutData);
            }
            catch(Exception)
            {
                this.TraceInfo("Invalid user interface layout file encountered, reverting to default layout.");
            }
        }

        private void SaveLayout()
        {
            if(UserInterfaceLayoutRepository == null || string.IsNullOrWhiteSpace(LayoutName)) return;

            if(_userInterfaceLayoutModel == null)
            {
                _userInterfaceLayoutModel = new UserInterfaceLayoutModel()
                {
                    LayoutName = LayoutName
                };
            }

            using(MemoryStream stream = new MemoryStream())
            {
                AssociatedObject.SaveLayout(stream);

                stream.Position = 0;
                using(StreamReader sr = new StreamReader(stream))
                {
                    _userInterfaceLayoutModel.MainViewDockingData = sr.ReadToEnd();

                    UserInterfaceLayoutRepository.Save(_userInterfaceLayoutModel);
                }
            }
        }

        #endregion Private Methods

        #region Event Handlers

        private void AssociatedObjectOnInitialized(object sender, EventArgs eventArgs)
        {
            LoadLayout(LayoutName);
        }

        private void AppOnExit(object sender, ExitEventArgs exitEventArgs)
        {
            SaveLayout();
        }

        #endregion Event Handlers

        #region Event Aggregator Handlers

        public void Handle(IResetLayoutMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(!AssociatedObject.Equals(message.Context))
            {
                return;
            }

            AssociatedObject.LoadLayout(StringResources.XmlOriginalLayout);

        }

        #endregion
    }
}
