using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.CustomControls.Behavior
{
    public class EmptyListBoxWaterMarkBehavior : Behavior<ListBox>
    {
        private INotifyCollectionChanged _itemsCollection;
        private UIElement _oldContent;
        private UIElement _newContent;

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
            var resources = new ResourceDictionary
                {
                    Source = new Uri("/Dev2.CustomControls;component/Resources/ControlTemplates.xaml",
                                     UriKind.RelativeOrAbsolute)
                };

            //_emptyTemplate = resources["WaterMarkListBoxTemplate"] as ControlTemplate;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            UnsubscribeFromEvents();
        }

        #endregion Override Methods

        #region Dependency Properties

        #region WatermarkText

        #region WatermarkText Property
        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), 
            typeof(EmptyListBoxWaterMarkBehavior), new UIPropertyMetadata(string.Empty));
        #endregion WatermarkText Property


        #endregion WatermarkText

        #endregion
        
        #region Private Methods

        private void SubscribeToEvents()
        {
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        private void UnsubscribeFromEvents()
        {
            AssociatedObject.Unloaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            if (_itemsCollection != null)
            {
                _itemsCollection.CollectionChanged -= ItemsCollectionCollectionChanged;
            }

            _itemsCollection = AssociatedObject.ItemsSource as INotifyCollectionChanged;

            if (_itemsCollection != null)
            {
                _itemsCollection.CollectionChanged += ItemsCollectionCollectionChanged;
            }
            SetOrRemoveWatermark();
        }

        private void ItemsCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetOrRemoveWatermark();
        }

        private void SetOrRemoveWatermark()
        {
            if (AssociatedObject.Items.Count == 0)
            {
                if (_oldContent == null)
                {
                    var border = AssociatedObject.Descendents(1).OfType<Border>().First();
                    _oldContent = border.Child;
                    _newContent = new TextBlock
                        {
                            Text = WatermarkText, 
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Colors.Gray)
                        };
                    border.Child = _newContent;
                }
            }
            else
            {
                if (_oldContent != null)
                {
                    var border = AssociatedObject.Descendents(1).OfType<Border>().First();
                    border.Child = _oldContent;
                    _oldContent = null;
                }
            }
        }

        #endregion Private Methods

    }
}
