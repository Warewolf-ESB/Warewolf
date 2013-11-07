using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;

namespace Dev2.Activities.Designers2.DataSplit
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
            SplitTypeUpdatedCommand = new RelayCommand(OnSplitTypeChanged, o => true);
        }

        public ICommand SplitTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }

        void OnSplitTypeChanged(object obj)
        {
            var selectedIndex = (int)obj;
            var viewModel = (DataSplitDesignerViewModel)DataContext;
            if(viewModel != null)
            {
                viewModel.OnSplitTypeChanged(selectedIndex);
            }
        }
    }
}
