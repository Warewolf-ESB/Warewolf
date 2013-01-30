
using Dev2.Common;
using Dev2.DataList.Contract;
using System.Collections.Generic;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfNumberFormatActivity.xaml
    public partial class DsfNumberFormatActivityDesigner
    {
        public DsfNumberFormatActivityDesigner()
        {
            InitializeComponent();
        }

        public List<string> RoundingTypes
        {
            get
            {
                return new List<string>(Dev2EnumConverter.ConvertEnumsToStringList<enRoundingType>());
            }
        }
    }
}
