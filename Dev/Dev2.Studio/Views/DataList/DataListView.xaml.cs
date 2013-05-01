using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Views.DataList
{
    /// <summary>
    /// Interaction logic for DataListView.xaml
    /// </summary>
    public partial class DataListView : UserControl
    {
        public DataListView()
        {
            InitializeComponent();
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
        }

        #region Events

        private void Nametxt_TextChanged(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = this.DataContext as IDataListViewModel;
            if (vm != null)
            {

                TextBox txtbox = sender as TextBox;
                if (txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    if (itemThatChanged != null && itemThatChanged.IsRecordset)
                    {
                        itemThatChanged.IsExpanded = true;
                    }
                    vm.AddBlankRow(itemThatChanged);
                }
            }
        }

        private void Nametxt_FocusLost(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = this.DataContext as IDataListViewModel;
            if (vm != null)
            {                
                TextBox txtbox = sender as TextBox;
                if (txtbox != null)
                {
                    IDataListItemModel itemThatChanged = txtbox.DataContext as IDataListItemModel;
                    vm.RemoveBlankRows(itemThatChanged);
                    vm.AddRecordsetNamesIfMissing();
                    vm.ValidateNames(itemThatChanged);
                }
            }           
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            WriteToResourceModel();            
        }

        private void Inputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }

            WriteToResourceModel();
        }

        private void Outputcbx_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null || !checkBox.IsEnabled)
            {
                return;
            }

            WriteToResourceModel();
        }

        #endregion Events
        public IEventAggregator EventAggregator { get; set; }
        #region Private Methods

        private void WriteToResourceModel()
        {
            IDataListViewModel vm = this.DataContext as IDataListViewModel;
            if (vm != null)
            {
                vm.WriteToResourceModel();
                //Mediator.SendMessage(MediatorMessages.UpdateIntelisense, this);
                EventAggregator.Publish(new UpdateIntellisenseMessage());
            }
        }

        #endregion Private Methods

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IDataListViewModel vm = this.DataContext as IDataListViewModel;
            if (vm != null)
            {

                Button btn = sender as Button;
                if (btn != null)
                {
                    IDataListItemModel itemThatChanged = btn.DataContext as IDataListItemModel;
                    vm.RemoveDataListItem(itemThatChanged);
                    WriteToResourceModel();
                }
            }
        }

        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IDataListViewModel vm = this.DataContext as IDataListViewModel;
            if (vm != null)
            {
                vm.FindMissing();
            }
        }
    }
}
