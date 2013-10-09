
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Activities.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Studio.Core.AppResources.Converters;
using Dev2.Utilities;

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
                return new List<string>(Dev2EnumConverter.ConvertEnumsTypeToStringList<enRoundingType>());
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

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfNumberFormatActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        void DsfNumberFormatActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfNumberFormatActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
