using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.WorkSurface;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.DataList
{
    /// <summary>
    /// Interaction logic for DataListView.xaml
    /// </summary>
    public partial class DataListView
    {
        readonly IEventAggregator _eventPublisher;

        public DataListView()
            : this(EventPublishers.Aggregator)
        {
        }

        public DataListView(IEventAggregator eventPublisher)
        {
            InitializeComponent();

            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            DataContextChanged += OnDataContextChanged;
        }


        #region Events

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                vm.AddRecordsetNamesIfMissing();
            }
        }

        private void NametxtTextChanged(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {

                TextBox txtbox = sender as TextBox;
                if(txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    if(itemThatChanged != null && itemThatChanged.IsRecordset)
                    {
                        itemThatChanged.IsExpanded = true;
                    }
                    if(itemThatChanged != null)
                    {
                        vm.AddBlankRow(itemThatChanged);
                        vm.ValidateNames(itemThatChanged);
                    }
                }
            }
        }

        private void NametxtFocusLost(object sender, RoutedEventArgs e)
        {
            DoDataListValidation(sender);
        }

        void DoDataListValidation(object sender)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                TextBox txtbox = sender as TextBox;
                if(txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    vm.RemoveBlankRows(itemThatChanged);
                    vm.AddRecordsetNamesIfMissing();
                    vm.ValidateNames(itemThatChanged);
                }
            }
        }

        private void UserControlLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            WriteToResourceModel();
        }

        private void Inputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }

            WriteToResourceModel();
        }

        private void Outputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }

            WriteToResourceModel();
        }

        #endregion Events

        #region Private Methods

        private void WriteToResourceModel()
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                vm.WriteToResourceModel();
                _eventPublisher.Publish(new UpdateIntellisenseMessage());
            }
        }

        #endregion Private Methods

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = DataContext as IDataListViewModel;
            if(vm != null)
            {

                Button btn = sender as Button;
                if(btn != null)
                {
                    IDataListItemModel itemThatChanged = btn.DataContext as IDataListItemModel;
                    vm.RemoveDataListItem(itemThatChanged);
                    WriteToResourceModel();
                }
            }
        }

        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var vm = DataContext as IDataListViewModel;
            if(vm != null)
            {
                var model = vm.Parent as WorkSurfaceContextViewModel;
                if(model != null)
                {
                    model.FindMissing();
                }

            }
        }

    }
}
