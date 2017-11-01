/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Infragistics;


namespace Dev2.Studio.Dock
{
    [ContentProperty("ItemBindings")]
    public abstract class ContainerFactoryBase : Freezable
        , ISupportInitialize
    {
        private ICollectionView _currentView;
        private readonly Dictionary<object, DependencyObject> _generatedElements;
        private bool _isInitializing;
        private readonly ItemBindingCollection _itemBindings;

        protected ContainerFactoryBase()
        {
            _generatedElements = new Dictionary<object, DependencyObject>();
            _itemBindings = new ItemBindingCollection();
            _itemBindings.CollectionChanged += OnItemBindingsChanged;
        }
        
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(IEnumerable), typeof(ContainerFactoryBase), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainerFactoryBase ef = (ContainerFactoryBase)d;
            ef.OnItemsSourceChanged((IEnumerable)e.NewValue);
        }

        private void OnItemsSourceChanged(IEnumerable newItems)
        {
            if(_currentView != null)
            {
                _currentView.CollectionChanged -= OnCollectionChanged;
                _currentView = null;
                ClearItems();
            }

            if(null != newItems)
            {
                _currentView = CollectionViewSource.GetDefaultView(newItems);
                Debug.Assert(_currentView != null);
                _currentView.CollectionChanged += OnCollectionChanged;
                ReinitializeElements();
            }
        }
        
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        protected bool IsInitializing => _isInitializing;
        
        private static readonly DependencyPropertyKey ItemForContainerPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ItemForContainer", typeof(object), typeof(ContainerFactoryBase),
                new FrameworkPropertyMetadata(null));
        
        private static readonly DependencyProperty ItemForContainerProperty = ItemForContainerPropertyKey.DependencyProperty;
        
        public static object GetItemForContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }
        
        protected virtual void ApplyItemContainerStyle(DependencyObject container, object item)
        {
        }

        protected virtual void ClearContainerForItem(DependencyObject container, object item)
        {
        }

        protected abstract ContentControl GetContainerForItem(object item);

        protected IEnumerable<DependencyObject> GetElements()
        {
            if(_currentView != null)
            {
                foreach(object item in _currentView)
                {
                    if (_generatedElements.TryGetValue(item, out DependencyObject container))
                    {
                        yield return container;
                    }
                }
            }
        }

        protected object GetItemFromContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }

        protected bool IsContainerInUse(DependencyObject container)
        {
            object item = container.ReadLocalValue(ItemForContainerProperty);

            // if there is no item associated with the container then it cannot be in use
            if(item == DependencyProperty.UnsetValue)
            {
                return false;
            }


            // get the container we have for the item
            if (_generatedElements.TryGetValue(item, out DependencyObject actualContainer))
            {
                Debug.Assert(ReferenceEquals(actualContainer, container), "There shouldn't be 2 different containers associated with a given item");
                return ReferenceEquals(actualContainer, container);
            }

            Debug.Assert(!_generatedElements.ContainsValue(container));
            return false;
        }

        protected abstract void OnItemInserted(DependencyObject container, object item, int index);

        protected abstract void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex);

        protected abstract void OnItemRemoved(DependencyObject container, object oldItem);

        protected virtual void PrepareContainerForItem(DependencyObject container, object item)
        {
            for(int i = 0, count = _itemBindings.Count; i < count; i++)
            {
                ItemBinding itemBinding = _itemBindings[i];

                if(itemBinding.CanApply(container, item))
                {
                    BindingOperations.SetBinding(container, itemBinding.TargetProperty, itemBinding.Binding);
                }
            }
        }

        protected void Reset()
        {
            ClearItems();

            ReinitializeElements();
        }

        protected virtual void VerifyItemIndex(DependencyObject container, object item, int index)
        {
        }

        private void AttachContainerToItem(DependencyObject container, object item)
        {
            // store a reference to the item on the container
            container.SetValue(ItemForContainerPropertyKey, item);
            container.SetValue(ContentControl.ContentProperty, item);

            
            if(item != container)
            {
                container.SetValue(FrameworkElement.DataContextProperty, item);
            }
        }

        private void ClearItems()
        {
            DependencyObject[] elements = new DependencyObject[_generatedElements.Count];
            _generatedElements.Values.CopyTo(elements, 0);
            _generatedElements.Clear();

            foreach(DependencyObject container in elements)
            {
                OnItemRemovedImpl(container, container.GetValue(ItemForContainerProperty));
            }
        }

        private void InsertItem(int index, object newItem)
        {
            Debug.Assert(!_generatedElements.ContainsKey(newItem));

            // create the element and associate it with the new item
            ContentControl container;

            container = IsItemItsOwnContainerImpl(newItem) ? newItem as ContentControl : GetContainerForItem(newItem);

            // keep a map between the new item and the element
            _generatedElements[newItem] = container;

            AttachContainerToItem(container, newItem);

            ApplyItemContainerStyle(container, newItem);

            PrepareContainerForItem(container, newItem);

            OnItemInserted(container, newItem, index);

        }

        private bool IsItemItsOwnContainerImpl(object item)
        {
            if(!(item is DependencyObject))
            {
                return false;
            }
            return true;
        }

        private void MoveItem(object item, int oldIndex, int newIndex)
        {

            if (_generatedElements.TryGetValue(item, out DependencyObject container))
            {
                OnItemMoved(container, item, oldIndex, newIndex);
            }
        }

        private void OnItemBindingsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Reset();
        }

        private void OnItemRemovedImpl(DependencyObject container, object oldItem)
        {
            OnItemRemoved(container, oldItem);

            container.ClearValue(ItemForContainerPropertyKey);

            ClearContainerForItem(container, oldItem);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(_isInitializing)
            {
                return;
            }

            // since its a freezable make sure its not frozen
            WritePreamble();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //Count should always be 1, but add the else in case of some inconsistant adding
                    if (e.NewItems.Count == 1)
                    {
                        InsertItem(e.NewStartingIndex, e.NewItems[0]);
                    }
                    else
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            InsertItem(i + e.NewStartingIndex, e.NewItems[i]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object newItem in e.OldItems)
                    {
                        RemoveItem(newItem);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object newItem in e.OldItems)
                    {
                        RemoveItem(newItem);
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        InsertItem(i + e.NewStartingIndex, e.NewItems[i]);
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReinitializeElements();
                    break;
                default:
                    break;
            }
        }

        private void ReinitializeElements()
        {
            if(_currentView == null || _currentView.IsEmpty)
            {
                ClearItems();
            }
            else
            {
                if(IsInitializing)
                {
                    return;
                }

                HashSet<object> oldItems = new HashSet<object>(_generatedElements.Keys);

                foreach(object item in _currentView)
                {
                    oldItems.Remove(item);
                }

                foreach(object oldItem in oldItems)
                {
                    DependencyObject container = _generatedElements[oldItem];
                    _generatedElements.Remove(oldItem);

                    OnItemRemovedImpl(container, oldItem);
                }

                int index = 0;
                foreach(object item in _currentView)
                {

                    if (!_generatedElements.TryGetValue(item, out DependencyObject container))
                    {
                        InsertItem(index, item);
                    }
                    else
                    {
                        VerifyItemIndex(container, item, index);
                    }

                    index++;
                }
            }
        }

        void RemoveItem(object oldItem)
        {
            Debug.Assert(_generatedElements.ContainsKey(oldItem));

            if (_generatedElements.TryGetValue(oldItem, out DependencyObject container))
            {
                _generatedElements.Remove(oldItem);
                OnItemRemovedImpl(container, oldItem);
            }
        }
                
        public void BeginInit()
        {
            Debug.Assert(!_isInitializing);

            WritePreamble();
            _isInitializing = true;
        }

        public void EndInit()
        {
            WritePreamble();
            _isInitializing = false;

            ReinitializeElements();
        }
    }

    public abstract class ContainerFactory : ContainerFactoryBase
    {
        protected override void ApplyItemContainerStyle(DependencyObject container, object item)
        {
            if (ContainerStyle != null)
            {
                container.SetValue(AppliedStyleProperty, KnownBoxes.FalseBox);
                container.SetValue(FrameworkElement.StyleProperty, style);
            }
            else
            {
                if (true.Equals(container.GetValue(AppliedStyleProperty)))
                {
                    // if we don't get a style now but we applied one previously clear it
                    container.ClearValue(AppliedStyleProperty);
                    container.ClearValue(FrameworkElement.StyleProperty);
                }
            }
        }

        protected override ContentControl GetContainerForItem(object item)
        {
            return (ContentControl)Activator.CreateInstance(ContainerType);
        }
        
        public static readonly DependencyProperty ContainerStyleProperty = DependencyProperty.Register("ContainerStyle",
            typeof(Style), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerStyleChanged));

        private static void OnContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainerFactory ef = (ContainerFactory)d;
            ef.RefreshContainerStyles();
        }
        
        [Description("Returns the style to apply to the element created.")]
        [Category("Behavior")]
        [Bindable(true)]
        public Style ContainerStyle
        {
            get
            {
                return (Style)GetValue(ContainerStyleProperty);
            }
            set
            {
                SetValue(ContainerStyleProperty, value);
            }
        }
        
        public static readonly DependencyProperty ContainerStyleSelectorProperty = DependencyProperty.Register("ContainerStyleSelector",
            typeof(StyleSelector), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerStyleChanged));
                
        public static readonly DependencyProperty ContainerTypeProperty = DependencyProperty.Register("ContainerType",
            typeof(Type), typeof(ContainerFactory), new FrameworkPropertyMetadata(), ValidateContainerType);
        
        private static bool ValidateContainerType(object newValue)
        {
            Type type = newValue as Type;

            if(type == null)
            {
                return true;
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException("ContainerType must be a non-abstract creatable type.");
            }

            if (!typeof(DependencyObject).IsAssignableFrom(type))
            {
                throw new ArgumentException("Element must be a DependencyObject derived type.");
            }

            return true;
        }
        
        [Description("Returns or sets the type of element to create")]
        [Category("Behavior")]
        [Bindable(true)]
        public Type ContainerType
        {
            get
            {
                return (Type)GetValue(ContainerTypeProperty);
            }
            set
            {
                SetValue(ContainerTypeProperty, value);
            }
        }
        
        private static readonly DependencyProperty AppliedStyleProperty =
            DependencyProperty.RegisterAttached("AppliedStyle", typeof(bool), typeof(ContainerFactory),
                new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
        
        private void RefreshContainerStyles()
        {
            foreach(DependencyObject container in GetElements())
            {
                ApplyItemContainerStyle(container, GetItemFromContainer(container));
            }
        }
    }
}
