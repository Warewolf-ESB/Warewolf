using System.Windows.Controls;
using System.Windows.Data;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MappingsView.xaml
    /// </summary>
    public partial class MappingsView : UserControl
    {
        public MappingsView()
        {
            InitializeComponent();
        }

        public ItemCollection GetInputMappings()
        {
            BindingExpression be = InputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return InputsMappingDataGrid.ItemsSource as ItemCollection;
        }

        public ItemCollection GetOutputMappings()
        {
            BindingExpression be = OutputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return OutputsMappingDataGrid.ItemsSource as ItemCollection;
        }
    }
}
