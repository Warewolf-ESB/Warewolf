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
                    if (Items != null)
                    {
                        Items.Add(String.Empty); //Dummy item
                    }
                }
                else
                {
                    if (Items != null)
                    {
                        Items.Clear();
                    }
                }

                SetValue(HasChildrenProperty, value);
            }
        }
    }
}
