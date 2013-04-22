using System.Collections.Generic;
using Dev2.Converters.DateAndTime;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml 
    public partial class DsfDateTimeDifferenceActivityDesigner
    {
        public IList<string> ItemList { get; set; }

        public DsfDateTimeDifferenceActivityDesigner()
        {
            InitializeComponent();
            ItemList = DateTimeComparer.OutputFormatTypes;
            cbxOutputTypes.ItemsSource = ItemList;            
        }
    }
}
