
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Studio.Core.AppResources.Converters;

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

        private void CboRounding_DropDownClosed(object sender, System.EventArgs e)
        {
            ComboBox RoundingComboBox = sender as ComboBox;

            enRoundingType roundingType;
            if (Enum.TryParse<enRoundingType>(RoundingComboBox.SelectedItem.ToString(), out roundingType))
            {
                if (roundingType == enRoundingType.None)
                {
                    ModelItemUtils.SetProperty<string>("RoundingDecimalPlaces", string.Empty, ModelItem);
                    TxtRounding.IsEnabled = false;
                }
                else
                {
                    TxtRounding.IsEnabled = true;   
                }
            }
        }
    }
}
