using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class WindowCloseBehviour : Behavior<Window>
    {
        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        #endregion Override Methods

        #region Dependency Properties

        #region CloseIndicator

        public string CloseIndicator
        {
            get { return (string)GetValue(CloseIndicatorProperty); }
            set { SetValue(CloseIndicatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseIndicatorProperty =
            DependencyProperty.Register("CloseIndicator", typeof(string), typeof(WindowCloseBehviour), new PropertyMetadata(CloseIndicatorChanged));

        private static void CloseIndicatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowCloseBehviour windowCloseBehviour = d as WindowCloseBehviour;

            if (windowCloseBehviour == null || windowCloseBehviour.AssociatedObject == null) return;

            bool value = Convert.ToBoolean(e.NewValue);
            if (value)
            {
                windowCloseBehviour.AssociatedObject.Close();
            }
        }

        #endregion CloseIndicator

        #endregion Dependency Properties
    }
}
