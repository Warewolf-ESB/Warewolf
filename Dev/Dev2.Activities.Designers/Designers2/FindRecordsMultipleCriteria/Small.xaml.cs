using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
            SearchTypeUpdatedCommand = new RelayCommand(OnSearchTypeUpdated, o => true);
        }

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }

        void OnSearchTypeUpdated(object obj)
        {
            var selectedIndex = (int)obj;
            var viewModel = (FindRecordsMultipleCriteriaDesignerViewModel)DataContext;
            viewModel.OnSearchTypeChanged(selectedIndex);
        }
    }
}
