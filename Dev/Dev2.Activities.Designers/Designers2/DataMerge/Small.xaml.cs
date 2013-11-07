using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;

namespace Dev2.Activities.Designers2.DataMerge
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
            MergeTypeUpdatedCommand = new RelayCommand(OnMergeTypeChanged, o => true);
        }

        public ICommand MergeTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
        
        void OnMergeTypeChanged(object obj)
        {
            var selectedIndex = (int)obj;
            var viewModel = (DataMergeDesignerViewModel)DataContext;
            if(viewModel != null)
            {
                viewModel.OnMergeTypeChanged(selectedIndex);
            }
        }
    }
}
