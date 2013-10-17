using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.BaseConvert
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
            //SearchTypeUpdatedCommand = new RelayCommand(OnSearchTypeUpdated, o => true);
        }

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }

//        void OnSearchTypeUpdated(object obj)
//        {
//            var selectedIndex = (int)obj;
//            var viewModel = (FindRecordsMultipleCriteriaDesignerViewModel)DataContext;
//            viewModel.OnSearchTypeChanged(selectedIndex);
//        }
    }
}
