/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.CustomControls.Utils;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class DataGridWatermarkModifier : Behavior<DataGrid>
    {
        #region Ctor

        public DataGridWatermarkModifier()
        {
            WatermarkIndexes = new List<int>();
            WatermarkText = new List<string>();
            WatermarkPropertyName = string.Empty;
        }

        #endregion Ctor

        #region Class Memebers

        private ItemCollection dataGridItems;
        private INotifyCollectionChanged observable;

        #endregion Class Memebers

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            SubscribeToEvents();


            dataGridItems = AssociatedObject.Items;
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

        // Using a DependencyProperty as the backing store for WatermarkText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof (List<string>), typeof (DataGridWatermarkModifier),
                new UIPropertyMetadata(null));

        public List<string> WatermarkText
        {
            get { return (List<string>) GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        #endregion WatermarkText Property

        #region WatermarkIndexes Property

        // Using a DependencyProperty as the backing store for WatermarkIndexes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkIndexesProperty =
            DependencyProperty.Register("WatermarkIndexes", typeof (List<int>), typeof (DataGridWatermarkModifier),
                new UIPropertyMetadata(null));

        public List<int> WatermarkIndexes
        {
            get { return (List<int>) GetValue(WatermarkIndexesProperty); }
            set { SetValue(WatermarkIndexesProperty, value); }
        }

        #endregion WatermarkIndexes Property

        #endregion WatermarkText

        #region WatermarkPropertyNames

        // Using a DependencyProperty as the backing store for WatermarkPropertyNames.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkPropertyNameProperty =
            DependencyProperty.Register("WatermarkPropertyName", typeof (string), typeof (DataGridWatermarkModifier),
                new PropertyMetadata(""));

        public string WatermarkPropertyName
        {
            get { return (string) GetValue(WatermarkPropertyNameProperty); }
            set { SetValue(WatermarkPropertyNameProperty, value); }
        }

        #endregion WatermarkPropertyNames

        #endregion Dependency Properties

        #region Private Methods

        private void UpdateWatermarks()
        {
            if (dataGridItems != null && !string.IsNullOrWhiteSpace(WatermarkPropertyName) && WatermarkText != null)
            {
                if (WatermarkIndexes == null)
                {
                    WatermarkIndexes = new List<int>();
                }

                for (int i = 0; i < WatermarkText.Count; i++)
                {
                    if (i == WatermarkIndexes.Count)
                    {
                        WatermarkIndexes.Add(i);
                    }
                }
                for (int i = 0; i < dataGridItems.Count; i++)
                {
                    List<object> list = dataGridItems.SourceCollection.Cast<object>().ToList();
                    var mi = list[i] as ModelItem;

                    if (mi != null)
                    {
                        int watermarkIndex = WatermarkIndexes.IndexOf(i);
                        if (watermarkIndex != -1)
                        {
                            WatermarkSential.IsWatermarkBeingApplied = true;
                            ModelProperty modelProperty = mi.Properties[WatermarkPropertyName];
                            if (modelProperty != null)
                            {
                                modelProperty.SetValue(WatermarkText[watermarkIndex]);
                            }
                        }
                        else
                        {
                            WatermarkSential.IsWatermarkBeingApplied = true;
                            ModelProperty modelProperty = mi.Properties[WatermarkPropertyName];
                            if (modelProperty != null)
                            {
                                modelProperty.SetValue("");
                            }
                        }
                    }
                    else
                    {
                        PropertyInfo pi = dataGridItems[i].GetType().GetProperty(WatermarkPropertyName);

                        if (pi != null)
                        {
                            if (WatermarkText.Count > i && dataGridItems.Count > i)
                            {
                                pi.SetValue(dataGridItems[i], WatermarkText[i], null);
                            }
                            else if (i == dataGridItems.Count - 1)
                            {
                                pi.SetValue(dataGridItems[i], "", null);
                            }
                        }
                    }
                }
            }
        }

        private void SubscribeToEvents()
        {
            observable = AssociatedObject.Items;
            if (observable != null)
            {
                observable.CollectionChanged -= observable_CollectionChanged;
                observable.CollectionChanged += observable_CollectionChanged;
            }

            var notifyPropertyChangedImplimentor = AssociatedObject as INotifyPropertyChanged;
            if (notifyPropertyChangedImplimentor != null)
            {
                notifyPropertyChangedImplimentor.PropertyChanged -= notifyPropertyChangedImplimentor_PropertyChanged;
            }

            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        private void UnsubscribeFromEvents()
        {
            observable = AssociatedObject.Items;
            if (observable != null)
            {
                observable.CollectionChanged -= observable_CollectionChanged;
            }

            var notifyPropertyChangedImplimentor = AssociatedObject as INotifyPropertyChanged;
            if (notifyPropertyChangedImplimentor != null)
            {
                notifyPropertyChangedImplimentor.PropertyChanged -= notifyPropertyChangedImplimentor_PropertyChanged;
            }

            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
        }

        #endregion Private Methods

        #region Event Handlers

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UnsubscribeFromEvents();
            routedEventArgs.Handled = true;
        }

        private void observable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateWatermarks();
        }

        private void notifyPropertyChangedImplimentor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Items" || e.PropertyName == "ItemsSource")
            {
                UpdateWatermarks();
            }
        }

        #endregion Event Handlers
    }
}