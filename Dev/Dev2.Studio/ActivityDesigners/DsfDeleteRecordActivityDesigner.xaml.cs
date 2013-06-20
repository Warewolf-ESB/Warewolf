using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfDeleteRecordActivityDesigner : IDisposable
    {
        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        public DsfDeleteRecordActivityDesigner()
        {
            InitializeComponent();
        }


        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (!_isRegistered)
            {
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }

            ModelItem item = newItem as ModelItem;


            ModelItem parent = item.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }
        }

        //private void Highlight(IDataListItemModel dataListItemViewModel) {
        //    List<string> containingFields = new List<string>();            
        //    border.Visibility = Visibility.Hidden;

        //    ForEverytxt.BorderBrush = Brushes.LightGray;
        //    ForEverytxt.BorderThickness = new Thickness(1.0);
        //    Tranformtxt.BorderBrush = Brushes.LightGray;
        //    Tranformtxt.BorderThickness = new Thickness(1.0);

        //    containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

        //    if (containingFields.Count > 0) {
        //        foreach (string item in containingFields) {
        //            if (item.Equals("foreachElementName")) {
        //                ForEverytxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
        //                ForEverytxt.BorderThickness = new Thickness(2.0);
        //            }
        //            else if (item.Equals("additionalData")) {
        //                Tranformtxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
        //                Tranformtxt.BorderThickness = new Thickness(2.0);
        //            }
        //        }

        //    }
        //}

        public void Dispose()
        {
            //Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        private void Recordsettxt_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (!String.IsNullOrEmpty(textBox.Text))
            {
                if (textBox.Text.EndsWith("]]"))
                {
                    if (!textBox.Text.Contains("(") && !textBox.Text.Contains(")"))
                    {
                        textBox.Text = textBox.Text.Insert(textBox.Text.IndexOf("]"), "()");
                    }
                    else
                    {
                        if (textBox.Text.Contains("."))
                        {
                            int startIndex = textBox.Text.IndexOf(".", StringComparison.Ordinal);
                            int endIndex = textBox.Text.IndexOf("]", StringComparison.Ordinal);
                            textBox.Text = textBox.Text.Remove(startIndex, (endIndex - startIndex));
                        }


                    }
                }
            }
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfDeleteRecordActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }        
    }
}
