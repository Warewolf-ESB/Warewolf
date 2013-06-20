
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Enums;

namespace Dev2.Studio.ActivityDesigners.Scripting
{
    // Interaction logic for DsfScriptingActivityDesigner.xaml
    public partial class DsfScriptingActivityDesigner
    {
        public DsfScriptingActivityDesigner()
        {
            InitializeComponent();
        }

        void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems != null && e.AddedItems.Count>0)
            {
                switch ((string)e.AddedItems[0])
                {                    
                    case "Ruby":
                        txtScript.DefaultText = "Ruby Syntax";
                        break;
                    case "Python":
                        txtScript.DefaultText = "Python Syntax";
                        break;
                    default:
                        txtScript.DefaultText = "JavaScript Syntax";
                        break;
                }                
            }            
        }

        void CbxScriptType_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
                {
                    cbx.ItemsSource = Dev2EnumConverter.ConvertEnumsTypeToStringList<enScriptType>();
                }
            }
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfScriptingActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
