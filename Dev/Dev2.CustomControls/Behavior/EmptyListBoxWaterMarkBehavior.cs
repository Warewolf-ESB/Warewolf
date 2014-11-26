/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using System.Linq;
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
        private UIElement _newContent;
        private UIElement _oldContent;

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
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

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof (string),
                typeof (EmptyListBoxWaterMarkBehavior), new UIPropertyMetadata(string.Empty));

        public string WatermarkText
        {
            get { return (string) GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

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
                    Border border = AssociatedObject.Descendents(1).OfType<Border>().First();
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
                    Border border = AssociatedObject.Descendents(1).OfType<Border>().First();
                    border.Child = _oldContent;
                    _oldContent = null;
                }
            }
        }

        #endregion Private Methods
    }
}