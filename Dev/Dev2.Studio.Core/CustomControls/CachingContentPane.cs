using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Infragistics.Windows.DockManager;

namespace Dev2.Studio.Core.CustomControls
{
    public class CachingContentPane : ContentPane
    {
        private Grid _contentGrid;
        private UIElement _currentView;

        public CachingContentPane()
        {
            _contentGrid = new Grid();
            this.Content = _contentGrid;
        }

        public object CurrentItem
        {
            get { return (object)GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }

        public static readonly DependencyProperty CurrentItemProperty =
                DependencyProperty.Register("CurrentItem", typeof(object), typeof(CachingContentPane),
                new PropertyMetadata(null, (s, e) => ((CachingContentPane)s).OnCurrentItemChanged()));

        private void OnCurrentItemChanged()
        {
            var newView = EnsureItem(CurrentItem);
            SendToBack(_currentView);
            _currentView = newView;
        }

        private UIElement EnsureItem(object source)
        {
            if (source == null)
            {
                return null;
            }

            var view = GetView(source);

            if (!_contentGrid.Children.Contains(view))
            {
                SubscribeDeactivation(source);
                _contentGrid.Children.Add(view);
            }

            BringToFront(view);
            return view;
        }

        // logic from Caliburn.Micro
        private UIElement GetView(object viewModel)
        {
            var context = View.GetContext(this);
            var view = ViewLocator.LocateForModel(viewModel, this, context);

            ViewModelBinder.Bind(viewModel, view, context);
            return view;
        }

        private void SubscribeDeactivation(object source)
        {
            var sourceScreen = source as IScreen;
            if (sourceScreen != null)
            {
                sourceScreen.Deactivated += SourceScreen_Deactivated;
            }
        }

        private void SourceScreen_Deactivated(object sender, DeactivationEventArgs e)
        {
            if (e.WasClosed)
            {
                var sourceScreen = sender as IScreen;
                sourceScreen.Deactivated -= SourceScreen_Deactivated;

                var view = GetView(sourceScreen);
                _contentGrid.Children.Remove(view);
            }
        }

        private void BringToFront(UIElement control)
        {
            control.Visibility = System.Windows.Visibility.Visible;
        }

        private void SendToBack(UIElement control)
        {
            if (control != null)
            {
                control.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
