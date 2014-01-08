using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.TriggerActions
{
    public class SetCaretIndexAction : TriggerAction<TextBox>
    {
        public int IndexPosition
        {
            get { return (int)GetValue(IndexPositionProperty); }
            set { SetValue(IndexPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IndexPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexPositionProperty =
            DependencyProperty.Register("IndexPosition", typeof(int), typeof(SetCaretIndexAction), new PropertyMetadata(0));

        protected override void Invoke(object parameter)
        {
            AssociatedObject.CaretIndex = IndexPosition;
            AssociatedObject.Focus();
        }
    }
}