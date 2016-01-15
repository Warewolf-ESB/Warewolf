using System;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.CustomControls.Controls
{
    public class TreeViewCustom : TreeViewItem
    {
        static TreeViewCustom()
        {
            HasChildrenProperty = DependencyProperty.Register("HasChildren", typeof(Boolean), typeof(TreeViewCustom));
        }

        static readonly DependencyProperty HasChildrenProperty;

        public Boolean HasChildren
        {
            get
            {
                return (Boolean)base.GetValue(HasChildrenProperty);
            }
            set
            {
                if (value)
                {
                    if (this.Items != null)
                    {
                        this.Items.Add(String.Empty); //Dummy item
                    }
                }
                else
                {
                    if (this.Items != null)
                    {
                        this.Items.Clear();
                    }
                }

                base.SetValue(HasChildrenProperty, value);
            }
        }
    }
}
