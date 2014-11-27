
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Abstract base class used to generate instances of elements based on a given source collection of items (<see cref="ItemsSource"/>).
    /// </summary>
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
        /// <summary>
        /// Initializes a new <see cref="ContainerFactoryBase"/>
        /// </summary>
        protected ContainerFactoryBase()
        {
            _generatedElements = new Dictionary<object, DependencyObject>();
            _itemBindings = new ItemBindingCollection();
            _itemBindings.CollectionChanged += OnItemBindingsChanged;
        }

        #endregion //Constructor

        #region Base class overrides

        #region CreateInstanceCore
        /// <summary>
        /// Creates an instance of the class
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return (ContainerFactoryBase)Activator.CreateInstance(GetType());
        }
        #endregion //CreateInstanceCore

        #region FreezeCore
        /// <summary>
        /// Invoked when the object is to be frozen.
        /// </summary>
        /// <param name="isChecking">True if the ability to freeze is being checked or false when the object is being attempted to be made frozen</param>
        /// <returns>Returns false since this object cannot be frozen.</returns>
        protected override bool FreezeCore(bool isChecking)
        {
            return false;
        }
        #endregion //FreezeCore

        #endregion //Base class overrides

        #region Properties

        #region Public

        #region ItemBindings
        /// <summary>
        /// Returns the collection of bindings that will be used to associated properties of the items from the <see cref="ItemsSource"/> with properties on the generated containers.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public ItemBindingCollection ItemBindings
        {
            get { return _itemBindings; }
        }
        #endregion //ItemBindings

        #region ItemsSource

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property
        /// </summary>
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

        /// <summary>
        /// Returns or sets the collection of items used to generate elements.
        /// </summary>
        /// <seealso cref="ItemsSourceProperty"/>
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
        /// <summary>
        /// Returns a boolean indicating if the object is being initialized.
        /// </summary>
        protected bool IsInitializing
        {
            get { return _isInitializing; }
        }
        #endregion //IsInitializing

        #endregion //Protected

        #region Private

        #region ItemForContainer

        private static readonly DependencyPropertyKey ItemForContainerPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ItemForContainer", typeof(object), typeof(ContainerFactoryBase),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// ItemForContainer Attached Dependency Property
        /// </summary>
        private static readonly DependencyProperty ItemForContainerProperty = ItemForContainerPropertyKey.DependencyProperty;

        #endregion //ItemForContainer

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Public Methods

        #region GetItemForContainer
        /// <summary>
        /// Returns the data item for which a given container is associated.
        /// </summary>
        /// <param name="container">The container to evaluate</param>
        /// <returns>The item associated with the specified container.</returns>
        public static object GetItemForContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }
        #endregion //GetItemForContainer

        #endregion //Public Methods

        #region Protected methods

        #region ApplyItemContainerStyle
        /// <summary>
        /// Used to apply a style to the container for an item
        /// </summary>
        /// <param name="container">The container associated with the item</param>
        /// <param name="item">The item from the source collection</param>
        protected virtual void ApplyItemContainerStyle(DependencyObject container, object item)
        {

        }
        #endregion //ApplyItemContainerStyle

        #region ClearContainerForItem
        /// <summary>
        /// Used to clear any settings applied to a container in the <see cref="PrepareContainerForItem"/>
        /// </summary>
        /// <param name="container">The container element </param>
        /// <param name="item">The item from the source collection</param>
        protected virtual void ClearContainerForItem(DependencyObject container, object item)
        {
        }
        #endregion //ClearContainerForItem

        #region GetContainerForItem
        /// <summary>
        /// Invoked when an element needs to be generated for a given item.
        /// </summary>
        /// <returns>The element to represent the item</returns>
        protected abstract ContentControl GetContainerForItem(object item);
        #endregion //GetContainerForItem

        #region GetElements
        /// <summary>
        /// Returns an enumerable list of elements that have been generated
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<DependencyObject> GetElements()
        {
            if(_currentView != null)
            {
                foreach(object item in _currentView)
                {
                    DependencyObject container;

                    if(_generatedElements.TryGetValue(item, out container))
                    {
                        yield return container;
                    }
                }
            }
        }
        #endregion //GetElements

        #region GetItemFromContainer
        /// <summary>
        /// Returns the item associated with a given container.
        /// </summary>
        /// <param name="container">The container whose underlying item is being requested</param>
        /// <returns>The underlying item</returns>
        protected object GetItemFromContainer(DependencyObject container)
        {
            return container.GetValue(ItemForContainerProperty);
        }
        #endregion //GetItemFromContainer

        #region IsContainerInUse
        /// <summary>
        /// Returns a boolean indicating if the specified container is currently in use. 
        /// </summary>
        /// <param name="container">The container to evaluate</param>
        /// <returns>This will return false if the container has not yet been added, has been removed or is in the process of being removed.</returns>
        protected bool IsContainerInUse(DependencyObject container)
        {
            object item = container.ReadLocalValue(ItemForContainerProperty);

            // if there is no item associated with the container then it cannot be in use
            if(item == DependencyProperty.UnsetValue)
                return false;

            DependencyObject actualContainer;

            // get the container we have for the item
            if(_generatedElements.TryGetValue(item, out actualContainer))
            {
                Debug.Assert(ReferenceEquals(actualContainer, container), "There shouldn't be 2 different containers associated with a given item");
                return ReferenceEquals(actualContainer, container);
            }

            Debug.Assert(!_generatedElements.ContainsValue(container));
            return false;
        }
        #endregion //IsContainerInUse

        #region IsItemItsOwnContainer
        /// <summary>
        /// Used to determine if the item from the source collection needs to have an container element generated for it.
        /// </summary>
        /// <param name="item">The item to evaluate</param>
        /// <returns>Returns true to indicate that a container is needed</returns>
        protected virtual bool IsItemItsOwnContainer(object item)
        {
            return true;
        }
        #endregion //IsItemItsOwnContainer

        #region OnItemInserted
        /// <summary>
        /// Invoked when an element for an item has been generated.
        /// </summary>
        /// <param name="item">The underlying item for which the element has been generated</param>
        /// <param name="container">The element that was generated</param>
        /// <param name="index">The index at which the item existed</param>
        protected abstract void OnItemInserted(DependencyObject container, object item, int index);

        #endregion //OnItemInserted

        #region OnItemMoved
        /// <summary>
        /// Invoked when an item has been moved in the source collection.
        /// </summary>
        /// <param name="item">The item that was moved</param>
        /// <param name="container">The associated element</param>
        /// <param name="oldIndex">The old index</param>
        /// <param name="newIndex">The new index</param>
        protected abstract void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex);

        #endregion //OnItemMoved

        #region OnItemRemoved
        /// <summary>
        /// Invoked when an element created for an item has been removed
        /// </summary>
        /// <param name="oldItem">The item associated with the element that was removed</param>
        /// <param name="container">The element that has been removed</param>
        protected abstract void OnItemRemoved(DependencyObject container, object oldItem);

        #endregion //OnItemRemoved

        #region PrepareContainerForItem
        /// <summary>
        /// Used to initialize a container for a given item.
        /// </summary>
        /// <param name="container">The container element </param>
        /// <param name="item">The item from the source collection</param>
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
        /// <summary>
        /// Removes all generated elements and rebuilds the elements.
        /// </summary>
        protected void Reset()
        {
            ClearItems();

            ReinitializeElements();
        }
        #endregion //Reset

        #region VerifyItemIndex
        /// <summary>
        /// Invoked during a verification of the source collection versus the elements generated to ensure the item is in the same location as that source item.
        /// </summary>
        /// <param name="item">The item being verified</param>
        /// <param name="container">The element associated with the item</param>
        /// <param name="index">The index in the source collection where the item exists</param>
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

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if(item != container)
                container.SetValue(FrameworkElement.DataContextProperty, item);
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

            if(IsItemItsOwnContainerImpl(newItem))
                container = newItem as ContentControl;
            else
                container = GetContainerForItem(newItem);

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
            if(item is DependencyObject == false)
                return false;

            return IsItemItsOwnContainer(item);
        }

        #endregion //IsItemItsOwnContainerImpl

        #region MoveItem
        private void MoveItem(object item, int oldIndex, int newIndex)
        {
            DependencyObject container;

            if(_generatedElements.TryGetValue(item, out container))
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
                return;

            // since its a freezable make sure its not frozen
            WritePreamble();

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for(int i = 0; i < e.NewItems.Count; i++)
                        InsertItem(i + e.NewStartingIndex, e.NewItems[i]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(object newItem in e.OldItems)
                        RemoveItem(newItem);
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach(object newItem in e.OldItems)
                        RemoveItem(newItem);
                    for(int i = 0; i < e.NewItems.Count; i++)
                        InsertItem(i + e.NewStartingIndex, e.NewItems[i]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReinitializeElements();
                    break;
            }
        }
        #endregion //OnCollectionChanged

        #region ReinitializeElements
        private void ReinitializeElements()
        {
            if(_currentView == null || _currentView.IsEmpty)
                ClearItems();
            else
            {
                if(IsInitializing)
                    return;

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
                    DependencyObject container;

                    if(!_generatedElements.TryGetValue(item, out container))
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

            DependencyObject container;
            if(_generatedElements.TryGetValue(oldItem, out container))
            {
                _generatedElements.Remove(oldItem);
                OnItemRemovedImpl(container, oldItem);
            }
        }

        #endregion //RemoveItem

        #endregion //Private methods

        #endregion //Methods

        #region ISupportInitialize Members

        /// <summary>
        /// Invoked when the object is about to be initialized
        /// </summary>
        public void BeginInit()
        {
            Debug.Assert(!_isInitializing);

            WritePreamble();
            _isInitializing = true;
        }

        /// <summary>
        /// Invoked when the object initialization is complete
        /// </summary>
        public void EndInit()
        {
            WritePreamble();
            _isInitializing = false;

            ReinitializeElements();
        }

        #endregion
    }

    /// <summary>
    /// Base class used to generate instances of objects of a specified type (<see cref="ContainerType"/>) based on a given source collection of items (<see cref="ContainerFactoryBase.ItemsSource"/>).
    /// </summary>
    public abstract class ContainerFactory : ContainerFactoryBase
    {
        #region Member Variables

        #endregion //Member Variables

        #region Constructor

        #endregion //Constructor

        #region Base class overrides

        #region ApplyItemContainerStyle
        /// <summary>
        /// Used to apply a style to the container for an item
        /// </summary>
        /// <param name="container">The container associated with the item</param>
        /// <param name="item">The item from the source collection</param>
        protected override void ApplyItemContainerStyle(DependencyObject container, object item)
        {
            Style style = ContainerStyle;

            if(null == style && ContainerStyleSelector != null)
                style = ContainerStyleSelector.SelectStyle(item, container);

            if(null != style)
            {
                container.SetValue(AppliedStyleProperty, KnownBoxes.FalseBox);
                container.SetValue(FrameworkElement.StyleProperty, style);
            }
            else if(true.Equals(container.GetValue(AppliedStyleProperty)))
            {
                // if we don't get a style now but we applied one previously clear it
                container.ClearValue(AppliedStyleProperty);
                container.ClearValue(FrameworkElement.StyleProperty);
            }
        }
        #endregion //ApplyItemContainerStyle

        #region GetContainerForItem
        /// <summary>
        /// Invoked when an element needs to be generated for a given item.
        /// </summary>
        /// <returns>The element to represent the item</returns>
        protected override ContentControl GetContainerForItem(object item)
        {
            return (ContentControl)Activator.CreateInstance(ContainerType);
        }
        #endregion //GetContainerForItem

        #endregion //Base class overrides

        #region Properties

        #region Public properties

        #region ContainerStyle

        /// <summary>
        /// Identifies the <see cref="ContainerStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContainerStyleProperty = DependencyProperty.Register("ContainerStyle",
            typeof(Style), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerStyleChanged));

        private static void OnContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContainerFactory ef = (ContainerFactory)d;
            ef.RefreshContainerStyles();
        }

        /// <summary>
        /// Returns the style to apply to the element created.
        /// </summary>
        /// <seealso cref="ContainerStyleProperty"/>
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

        /// <summary>
        /// Identifies the <see cref="ContainerStyleSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ContainerStyleSelectorProperty = DependencyProperty.Register("ContainerStyleSelector",
            typeof(StyleSelector), typeof(ContainerFactory), new FrameworkPropertyMetadata(null, OnContainerStyleChanged));

        /// <summary>
        /// Returns or sets a StyleSelector that can be used to provide a Style for the items.
        /// </summary>
        /// <seealso cref="ContainerStyleSelectorProperty"/>
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

        /// <summary>
        /// Identifies the <see cref="ContainerType"/> dependency property
        /// </summary>
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
                ef.ValidateContainerType(newType);

            return newValue;
        }

        private static bool ValidateContainerType(object newValue)
        {
            Type type = newValue as Type;

            if(type == null)
                return true;

            if(type.IsAbstract)
                throw new ArgumentException("ContainerType must be a non-abstract creatable type.");

            if(!typeof(DependencyObject).IsAssignableFrom(type))
                throw new ArgumentException("Element must be a DependencyObject derived type.");

            return true;
        }

        /// <summary>
        /// Returns or sets the type of element to create
        /// </summary>
        /// <seealso cref="ContainerTypeProperty"/>
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

        /// <summary>
        /// ItemForContainer Attached Dependency Property
        /// </summary>
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
        /// <summary>
        /// Invoked when the <see cref="ContainerType"/> is about to be changed to determine if the specified type is allowed.
        /// </summary>
        /// <param name="elementType">The new element type</param>
        protected virtual void ValidateContainerType(Type elementType)
        {
        }
        #endregion //ValidateContainerType

        #endregion //Methods
    }

    /// <summary>
    /// Custom <see cref="ContainerFactory"/> used to automatically add/remove elements for items to a given <see cref="CollectionContainerFactory&lt;T&gt;.TargetCollection"/>
    /// </summary>
    public class CollectionContainerFactory<T> : ContainerFactory
        where T : IList
    {
        #region Properties

        #region TargetCollection

        /// <summary>
        /// Identifies the <see cref="TargetCollection"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TargetCollectionProperty = DependencyProperty.Register("TargetCollection",
            typeof(T), typeof(CollectionContainerFactory<T>), new FrameworkPropertyMetadata(null, OnTargetCollectionChanged));

        private static void OnTargetCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CollectionContainerFactory<T> cef = (CollectionContainerFactory<T>)d;

            // if there are elements we need to move them from the old collection into the new one
            cef.OnTargetCollectionChanged((T)e.OldValue, (T)e.NewValue);
        }

        private void OnTargetCollectionChanged(T oldList, T newList)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(oldList != null)
            {
                foreach(DependencyObject item in GetElements())
                    oldList.Remove(item);
            }

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(newList != null)
            {
                foreach(DependencyObject item in GetElements())
                    newList.Add(item);
            }
        }

        /// <summary>
        /// Returns or sets the collection that will be updated with the elements generated.
        /// </summary>
        /// <remarks>
        /// <para>Note: This collection should only be modified by the element factory. I.e. you should not try to add or remove 
        /// items directly from the target collection.</para>
        /// </remarks>
        /// <seealso cref="TargetCollectionProperty"/>
        [Description("Returns or sets the collection that will be updated with the elements generated.")]
        [Category("Behavior")]
        [Bindable(true)]
        public T TargetCollection
        {
            get
            {
                return (T)GetValue(TargetCollectionProperty);
            }
            set
            {
                SetValue(TargetCollectionProperty, value);
            }
        }

        #endregion //TargetCollection

        #endregion //Properties

        #region Base class overrides

        #region OnItemInserted
        /// <summary>
        /// Invoked when an element for an item has been generated.
        /// </summary>
        /// <param name="item">The underlying item for which the element has been generated</param>
        /// <param name="container">The element that was generated</param>
        /// <param name="index">The index at which the item existed</param>
        protected override void OnItemInserted(DependencyObject container, object item, int index)
        {
            // add it to the target collection
            T target = TargetCollection;

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(null != target)
            {
                Debug.Assert(index >= 0 && index <= target.Count);
                target.Insert(index, container);
            }
        }
        #endregion //OnItemInserted

        #region OnItemMoved
        /// <summary>
        /// Invoked when an item has been moved in the source collection.
        /// </summary>
        /// <param name="item">The item that was moved</param>
        /// <param name="container">The associated element</param>
        /// <param name="oldIndex">The old index</param>
        /// <param name="newIndex">The new index</param>
        protected override void OnItemMoved(DependencyObject container, object item, int oldIndex, int newIndex)
        {
            T target = TargetCollection;

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(null != target)
            {
                int actualIndex = target.IndexOf(container);

                target.RemoveAt(actualIndex);
                target.Insert(newIndex + (actualIndex - oldIndex), container);
            }
        }
        #endregion //OnItemMoved

        #region OnItemRemoved
        /// <summary>
        /// Invoked when an element created for an item has been removed
        /// </summary>
        /// <param name="oldItem">The item associated with the element that was removed</param>
        /// <param name="container">The element that has been removed</param>
        protected override void OnItemRemoved(DependencyObject container, object oldItem)
        {
            T target = TargetCollection;

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(null != target)
                target.Remove(container);
        }
        #endregion //OnItemRemoved

        #region VerifyItemIndex
        /// <summary>
        /// Invoked during a verification of the source collection versus the elements generated to ensure the item is in the same location as that source item.
        /// </summary>
        /// <param name="item">The item being verified</param>
        /// <param name="container">The element associated with the item</param>
        /// <param name="index">The index in the source collection where the item exists</param>
        protected override void VerifyItemIndex(DependencyObject container, object item, int index)
        {
            T target = TargetCollection;

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if(null != target)
            {
                int oldIndex = target.IndexOf(item);

                if(oldIndex < 0)
                    target.Insert(index, container);
                else
                {
                    target.RemoveAt(oldIndex);
                    target.Insert(index, item);
                }
            }
        }
        #endregion //VerifyItemIndex

        #endregion //Base class overrides
    }
}
