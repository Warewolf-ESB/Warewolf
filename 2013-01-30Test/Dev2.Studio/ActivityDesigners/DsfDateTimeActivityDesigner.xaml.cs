using System.Collections.Generic;
using Dev2.Converters.DateAndTime;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml
    public partial class DsfDateTimeActivityDesigner
    {
        public IList<string> ItemList { get; set; }

        public DsfDateTimeActivityDesigner()
        {
            InitializeComponent();
            ItemList = DateTimeFormatter.TimeModifierTypes;
            cbxTimeModifers.ItemsSource = ItemList;
        }
    }
}
