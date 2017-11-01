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
        #region Member Variables

        private ICollectionView _currentView;
        private readonly Dictionary<object, DependencyObject> _generatedElements;
        private bool _isInitializing;
        private readonly ItemBindingCollection _itemBindings;

        #endregion //Member Variables

        #region Constructor

        protected ContainerFactoryBase()
        {
            _generatedElements = new Dictionary<object, DependencyObject>();
            _itemBindings = new ItemBindingCollection();
            _itemBindings.CollectionChanged += OnItemBindingsChanged;
        }

        #endregion //Constructor

        #region Base class overrides

        #region CreateInstanceCore

        protected override Freezable CreateInstanceCore()
        {
            return (ContainerFactoryBase)Activator.CreateInstance(GetType());
        }
        #endregion //CreateInstanceCore

        #region FreezeCore

        protected override bool FreezeCore(bool isChecking)
        {
            return false;
        }
        #endregion //FreezeCore

        #endregion //Base class overrides

        #region Properties

        #region Public

        #region ItemBindings

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public ItemBindingCollection ItemBindings => _itemBindings;

        #endregion //ItemBindings

        #region ItemsSource
        
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

        #endregion //ItemsSource

        #endregion //Public

        #region Protected

        #region IsInitializing

        protected bool IsInitializing => _isInitializing;

        #endregion //IsInitializing

        #endregion //Protected

        #region Private

        #region ItemForContainer

        private static readonly DependencyPropertyKey ItemForContainerPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ItemForContainer", typeof(object), typeof(ContainerFactoryBase),
                new FrameworkPropertyMetadata(null));
        
        private static readonly DependencyProperty ItemForContainerProperty = ItemForContainerPropertyKey.DependencyProperty;

        #endregion //ItemForContainer

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Public Methods

        #region GetItemForContainer

        public static object GetItemForContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }
        #endregion //GetItemForContainer

        #endregion //Public Methods

        #region Protected methods

        #region ApplyItemContainerStyle

        protected virtual void ApplyItemContainerStyle(DependencyObject container, object item)
        {

        }
        #endregion //ApplyItemContainerStyle

        #region ClearContainerForItem

        protected virtual void ClearContainerForItem(DependencyObject container, object item)
        {
        }
        #endregion //ClearContainerForItem

        #region GetContainerForItem

        protected abstract ContentControl GetContainerForItem(object item);
        #endregion //GetContainerForItem

        #region GetElements

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
        #endregion //GetElements

        #region GetItemFromContainer

        protected object GetItemFromContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }
        #endregion //GetItemFromContainer

        #region IsContainerInUse

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
        #endregion //IsContainerInUse

        #region IsItemItsOwnContainer

        protected virtual bool IsItemItsOwnContainer(object item)
        {
            return true;
        }
        #endregion //IsItemItsOwnContainer

        #region OnItemInserted

        protected abstract void OnItemInserted(DependencyObject container, object item, int index);

        #endregion //OnItemInserted

        #region OnItemMoved

        protected abstract void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex);

        #endregion //OnItemMoved

        #region OnItemRemoved

        protected abstract void OnItemRemoved(DependencyObject container, object oldItem);

        #endregion //OnItemRemoved

        #region PrepareContainerForItem

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
        #endregion //PrepareContainerForItem

        #region Reset

        protected void Reset()
        {
            ClearItems();

            ReinitializeElements();
        }
        #endregion //Reset

        #region VerifyItemIndex

        protected virtual void VerifyItemIndex(DependencyObject container, object item, int index)
        {
        }

        #endregion //VerifyItemIndex

        #endregion //Protected methods

        #region Private methods

        #region AttachContainerToItem
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
        #endregion //AttachContainerToItem

        #region ClearItems
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
        #endregion //ClearItems

        #region InsertItem
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
        #endregion //InsertItem

        #region IsItemItsOwnContainerImpl
        private bool IsItemItsOwnContainerImpl(object item)
        {
            if(!(item is DependencyObject))
            {
                return false;
            }

            return IsItemItsOwnContainer(item);
        }

        #endregion //IsItemItsOwnContainerImpl

        #region MoveItem
        private void MoveItem(object item, int oldIndex, int newIndex)
        {

            if (_generatedElements.TryGetValue(item, out DependencyObject container))
            {
                OnItemMoved(container, item, oldIndex, newIndex);
            }
        }
        #endregion //MoveItem

        #region OnItemBindingsChanged
        private void OnItemBindingsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Reset();
        }
        #endregion //OnItemBindingsChanged

        #region OnItemRemovedImpl
        private void OnItemRemovedImpl(DependencyObject container, object oldItem)
        {
            OnItemRemoved(container, oldItem);

            container.ClearValue(ItemForContainerPropertyKey);

            ClearContainerForItem(container, oldItem);
        }
        #endregion //OnItemRemovedImpl

        #region OnCollectionChanged
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
        #endregion //OnCollectionChanged

        #region ReinitializeElements
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
        #endregion //ReinitializeElements

        #region RemoveItem

        void RemoveItem(object oldItem)
        {
            Debug.Assert(_generatedElements.ContainsKey(oldItem));

            if (_generatedElements.TryGetValue(oldItem, out DependencyObject container))
            {
                _generatedElements.Remove(oldItem);
                OnItemRemovedImpl(container, oldItem);
            }
        }

        #endregion //RemoveItem

        #endregion //Private methods

        #endregion //Methods

        #region ISupportInitialize Members
        
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

        #endregion
    }

    public abstract class ContainerFactory : ContainerFactoryBase
    {
        #region Member Variables

        #endregion //Member Variables

        #region Constructor

        #endregion //Constructor

        #region Base class overrides

        #region ApplyItemContainerStyle

        protected override void ApplyItemContainerStyle(DependencyObject container, object item)
        {
            Style style = ContainerStyle;

            if(null == style && ContainerStyleSelector != null)
            {
                style = ContainerStyleSelector.SelectStyle(item, container);
            }

            if (null != style)
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
        #endregion //ApplyItemContainerStyle

        #region GetContainerForItem

        protected override ContentControl GetContainerForItem(object item)
        {
            return (ContentControl)Activator.CreateInstance(ContainerType);
        }
        #endregion //GetContainerForItem

        #endregion //Base class overrides

        #region Properties

        #region Public properties

        #region ContainerStyle
        
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

        #endregion //ContainerStyle

        #region ContainerStyleSelector
        
        public static readonly DependencyProperty ContainerStyleSelectorProperty = DependencyProperty.Register("ContainerStyleSelector",
            typeof(StyleSelector), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerStyleChanged));
        
        [Description("Returns or sets a StyleSelector that can be used to provide a Style for the items.")]
        [Category("Behavior")]
        [Bindable(true)]
        public StyleSelector ContainerStyleSelector
        {
            get
            {
                return (StyleSelector)GetValue(ContainerStyleSelectorProperty);
            }
            set
            {
                SetValue(ContainerStyleSelectorProperty, value);
            }
        }

        #endregion //ContainerStyleSelector

        #region ContainerType
        
        public static readonly DependencyProperty ContainerTypeProperty = DependencyProperty.Register("ContainerType",
            typeof(Type), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerTypeChanged, CoerceContainerType), ValidateContainerType);

        private static void OnContainerTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainerFactory ef = (ContainerFactory)d;
            ef.Reset();
        }

        private static object CoerceContainerType(DependencyObject d, object newValue)
        {
            ContainerFactory ef = (ContainerFactory)d;
            Type newType = (Type)newValue;

            if(null != newType)
            {
                ef.ValidateContainerType(newType);
            }

            return newValue;
        }

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

        #endregion //ContainerType

        #endregion //Public properties

        #region Private

        #region AppliedStyle
        
        private static readonly DependencyProperty AppliedStyleProperty =
            DependencyProperty.RegisterAttached("AppliedStyle", typeof(bool), typeof(ContainerFactory),
                new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        #endregion //AppliedStyle

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region RefreshContainerStyles
        private void RefreshContainerStyles()
        {
            foreach(DependencyObject container in GetElements())
            {
                ApplyItemContainerStyle(container, GetItemFromContainer(container));
            }
        }
        #endregion //RefreshContainerStyles

        #region ValidateContainerType

        protected virtual void ValidateContainerType(Type elementType)
        {
        }
        #endregion //ValidateContainerType

        #endregion //Methods
    }
}
