/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2008

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using WPF.JoshSmith.Panels;

namespace Dev2.CustomControls.Panels
{
    /// <summary>
    ///     A Panel that displays its children in a Viewport3D.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/panel3d.aspx
    /// </remarks>
    public sealed class Panel3D : LogicalPanel
    {
        #region Data

        /// <summary>
        ///     (0, 0)
        /// </summary>
        private static readonly Point ORIGIN_POINT = new Point(0, 0);

        /// <summary>
        ///     A mapping between the 2D children of this panel and the 3D objects displayed in _viewport.
        /// </summary>
        private readonly Dictionary<UIElement, Viewport2DVisual3D> _elementTo3DModelMap =
            new Dictionary<UIElement, Viewport2DVisual3D>();

        /// <summary>
        ///     Viewport2DVisual3D objects for every item in this panel's Children collection.
        /// </summary>
        private readonly List<Viewport2DVisual3D> _models = new List<Viewport2DVisual3D>();

        /// <summary>
        ///     Ticks when the items have finished moving and it is time to clean up.
        /// </summary>
        private readonly DispatcherTimer _moveItemsCompletionTimer;

        /// <summary>
        ///     Holds requests to move the 3D models that arrived while the items were already being moved.
        /// </summary>
        private readonly Queue<MoveItemsRequest> _moveItemsRequestQueue = new Queue<MoveItemsRequest>();

        /// <summary>
        ///     The Viewport3D that hosts the Viewport2DVisual3D objects stored in the _models field.
        /// </summary>
        private readonly Viewport3DEx _viewport;

        /// <summary>
        ///     Keeps track of whether the current call to MoveItems() should execute the completion logic
        ///     or not, based on whether an item in the UI was selected during the item animations.
        /// </summary>
        private bool _abortMoveItems;

        /// <summary>
        ///     Keeps track of whether _viewport has been added to the panel's visual children collection yet.
        /// </summary>
        private bool _hasAddedViewport;

        /// <summary>
        ///     If this panel is the items host for a Selector-derived control, this references that control.
        /// </summary>
        private Selector _itemsOwner;

        /// <summary>
        ///     Stores data pertaining to a call to MoveItems().
        /// </summary>
        private struct MoveItemsRequest
        {
            public readonly TimeSpan AnimationLength;
            public readonly bool Forward;
            public readonly int ItemCount;

            public MoveItemsRequest(int itemCount, bool forward, TimeSpan animationLength)
            {
                ItemCount = itemCount;
                Forward = forward;
                AnimationLength = animationLength;
            }
        }

        #endregion // Data

        #region Initialization

        /// <summary>
        ///     Initializes a new instance.
        /// </summary>
        public Panel3D()
        {
            // Create the viewport that hosts the child elements.
            _viewport = CreateViewport();

            // Configure the timer that is used to clean up after the 3D models move.
            _moveItemsCompletionTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _moveItemsCompletionTimer.Tick += OnMoveItemsCompleted;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Setting the Camera property must be delayed until Loaded fires,
            // otherwise setting it from XAML has no effect.  Not sure why...
            if (Camera == null)
                Camera = _viewport.Camera;

            // We raise this custom routed event because the Loaded event of an items host
            // is not raised on the control that contains it.  This event will make it
            // to the outside world, so that consumers can get a reference to the panel.
            if (ItemsControl.GetItemsOwner(this) != null)
                RaiseEvent(new RoutedEventArgs(ItemsHostLoadedEvent));
        }

        private static Viewport3DEx CreateViewport()
        {
            var viewport = new Viewport3DEx
            {
                Camera = new PerspectiveCamera
                {
                    LookDirection = new Vector3D(2, 0, -10),
                    Position = new Point3D(-3.18, 2, 3),
                    UpDirection = new Vector3D(0, 1, 0)
                }
            };

            viewport.Children.Add(new ModelVisual3D
            {
                Content = new AmbientLight(Colors.White)
            });

            return viewport;
        }

        #endregion // Initialization

        #region Routed Events

        /// <summary>
        ///     Identifies the ItemsHostLoaded bubbling event.
        /// </summary>
        public static readonly RoutedEvent ItemsHostLoadedEvent = EventManager.RegisterRoutedEvent(
            "ItemsHostLoaded",
            RoutingStrategy.Bubble,
            typeof (RoutedEventHandler),
            typeof (Panel3D));

        /// <summary>
        ///     Raised when the Panel3D is loaded, only when acting as an items host for an ItemsControl.
        /// </summary>
        public event RoutedEventHandler ItemsHostLoaded
        {
            add { AddHandler(ItemsHostLoadedEvent, value); }
            remove { RemoveHandler(ItemsHostLoadedEvent, value); }
        }

        #endregion // Routed Events

        #region Properties

        #region AllowTransparency

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty AllowTransparencyProperty =
            DependencyProperty.Register(
                "AllowTransparency",
                typeof (bool),
                typeof (Panel3D),
                new UIPropertyMetadata(
                    false,
                    OnAllowTransparencyChanged
                    ));

        /// <summary>
        ///     Gets/sets whether the models in the scene support being truly translucent, such
        ///     that the models behind them are visible through the models in front.
        ///     The default value is false.
        ///     This is a dependency property.
        /// </summary>
        public bool AllowTransparency
        {
            get { return (bool) GetValue(AllowTransparencyProperty); }
            set { SetValue(AllowTransparencyProperty, value); }
        }

        private static void OnAllowTransparencyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var panel3D = depObj as Panel3D;
            if (panel3D != null)
            {
                Viewport2DVisual3D frontModel = panel3D._viewport.FrontModel;
                panel3D._viewport.AllowTransparency = (bool) e.NewValue;

                if (frontModel != null)
                    panel3D.BuildScene(frontModel);
            }
        }

        #endregion // AllowTransparency

        #region AutoAdjustOpacity

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty AutoAdjustOpacityProperty =
            DependencyProperty.Register(
                "AutoAdjustOpacity",
                typeof (bool),
                typeof (Panel3D),
                new UIPropertyMetadata(
                    true,
                    (depObj, e) => ((Panel3D) depObj).BuildScene()
                    ));

        /// <summary>
        ///     Gets/sets whether the Panel3D automatically adjusts each model's opacity based on its visual index.
        ///     The default value is true.
        ///     This is a dependency property.
        /// </summary>
        public bool AutoAdjustOpacity
        {
            get { return (bool) GetValue(AutoAdjustOpacityProperty); }
            set { SetValue(AutoAdjustOpacityProperty, value); }
        }

        #endregion // AutoAdjustOpacity

        #region Camera

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register(
                "Camera",
                typeof (Camera),
                typeof (Panel3D),
                new UIPropertyMetadata(null, OnCameraChanged));

        /// <summary>
        ///     Gets/sets a camera used to view the 3D scene.
        ///     The default camera is declared as:
        ///     &lt;PerspectiveCamera
        ///     LookDirection="2, 0, -10"
        ///     Position="-3.18, 2, 3"
        ///     UpDirection="0, 1, 0"
        ///     /&gt;
        /// </summary>
        public Camera Camera
        {
            get { return (Camera) GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        private static void OnCameraChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var panel3D = depObj as Panel3D;
            var camera = e.NewValue as Camera;
            if (panel3D._viewport.Camera != camera)
                panel3D._viewport.Camera = camera;
        }

        #endregion // Camera

        #region DefaultAnimationLength

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty DefaultAnimationLengthProperty =
            DependencyProperty.Register(
                "DefaultAnimationLength",
                typeof (TimeSpan),
                typeof (Panel3D),
                new UIPropertyMetadata(TimeSpan.FromMilliseconds(700)));

        /// <summary>
        ///     The default amount of time it takes to move items.
        ///     This value can be overridden when calling MoveItems().
        ///     The default value is 700 milliseconds.
        ///     This is a dependency property.
        /// </summary>
        public TimeSpan DefaultAnimationLength
        {
            get { return (TimeSpan) GetValue(DefaultAnimationLengthProperty); }
            set { SetValue(DefaultAnimationLengthProperty, value); }
        }

        #endregion // DefaultAnimationLength

        #region IsMovingItems

        private static readonly DependencyPropertyKey IsMovingItemsKey =
            DependencyProperty.RegisterReadOnly(
                "IsMovingItems",
                typeof (bool),
                typeof (Panel3D),
                new UIPropertyMetadata(false));

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty IsMovingItemsProperty = IsMovingItemsKey.DependencyProperty;

        /// <summary>
        ///     Returns whether or not the models displayed in this Panel3D are currently
        ///     animating to new locations as a result of calling the MoveItems method.
        ///     This is a read-only dependency property.
        /// </summary>
        public bool IsMovingItems
        {
            get { return (bool) GetValue(IsMovingItemsProperty); }
            private set { SetValue(IsMovingItemsKey, value); }
        }

        #endregion // IsMovingItems

        #region ItemLayoutDirection

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty ItemLayoutDirectionProperty =
            DependencyProperty.Register(
                "ItemLayoutDirection",
                typeof (Vector3D),
                typeof (Panel3D),
                new UIPropertyMetadata(
                    new Vector3D(-1.0, +1.3, -7.0),
                    (depObj, e) => (depObj as Panel3D).BuildScene()
                    ));

        /// <summary>
        ///     Gets/sets a Vector3D that describes the direction in which the items are positioned.
        ///     The default value is (-1, +1.3, -7).
        ///     This is a dependency property.
        /// </summary>
        public Vector3D ItemLayoutDirection
        {
            get { return (Vector3D) GetValue(ItemLayoutDirectionProperty); }
            set { SetValue(ItemLayoutDirectionProperty, value); }
        }

        #endregion // ItemLayoutDirection

        #region MaxVisibleModels

        /// <summary>
        ///     Readonly field.
        /// </summary>
        public static readonly DependencyProperty MaxVisibleModelsProperty =
            DependencyProperty.Register(
                "MaxVisibleModels",
                typeof (int),
                typeof (Panel3D),
                new UIPropertyMetadata(
                    10, // default value
                    (depObj, e) => (depObj as Panel3D).BuildScene() // apply new value
                    ),
                newValue => 1 < (int) newValue // validate new value
                );

        /// <summary>
        ///     Gets/sets the maximum number of 3D models that can be
        ///     displayed at once.  The default value is 10.  The minimum
        ///     value for this property is 2.  This is a dependency property.
        /// </summary>
        public int MaxVisibleModels
        {
            get { return (int) GetValue(MaxVisibleModelsProperty); }
            set { SetValue(MaxVisibleModelsProperty, value); }
        }

        #endregion // MaxVisibleModels

        #endregion // Properties

        #region MoveItems

        /// <summary>
        ///     Moves the items forward or backward over the default animation length.
        /// </summary>
        public void MoveItems(int itemCount, bool forward)
        {
            MoveItems(itemCount, forward, DefaultAnimationLength);
        }

        /// <summary>
        ///     Moves the items forward or backward over the specified animation length.
        /// </summary>
        public void MoveItems(int itemCount, bool forward, TimeSpan animationLength)
        {
            bool go = MoveItems_CanExecute(itemCount, forward, animationLength);
            if (!go)
                return;

            MoveItems_VerifyItemCount(itemCount);

            // Prepare some flags that control this algorithm.
            _abortMoveItems = false;
            IsMovingItems = true;

            // Move the 3D models to their new position in 
            // the Viewport3D's Children collection.
            MoveItems_RelocateModels(itemCount, forward);

            // If we are the items host of a Selector, select the first child element.
            MoveItems_SelectFrontItem();

            // Start moving the models to their new locations 
            // and apply the new opacity values.
            MoveItems_BeginAnimations(forward, animationLength);

            // Start the timer that ticks when the animations are finished.
            MoveItems_StartCleanupTimer(animationLength);
        }

        #region CanExecute

        private bool MoveItems_CanExecute(int itemCount, bool forward, TimeSpan animationLength)
        {
            if (IsMovingItems)
            {
                var req = new MoveItemsRequest(
                    itemCount,
                    forward,
                    animationLength);

                _moveItemsRequestQueue.Enqueue(req);
                return false;
            }

            // We cannot move less than two items.
            // The first item is a light source, so ignore it.
            if (_viewport.ModelCount < 2)
                return false;

            if (itemCount < 1)
                return false;

            return true;
        }

        #endregion // CanExecute

        #region VerifyItemCount

        private void MoveItems_VerifyItemCount(int itemCount)
        {
            // TODO: Make this smarter so that it does not throw an exception...
            if (IsVirtualizing && _models.Count < itemCount)
                throw new InvalidOperationException(
                    "ARTIFICAL LIMITATION: Cannot move more items than the Panel3D contains when it is virtualizing.");
        }

        #endregion // VerifyItemCount

        #region RelocateModels

        private void MoveItems_RelocateModels(int itemCount, bool forward)
        {
            // Move the first or last models to the opposite end of the list.
            if (forward)
            {
                if (IsVirtualizing)
                {
                    // There are more models than can be shown at once, so
                    // add some to the scene by appending them to the 
                    // viewport's list of child elements.  By the time
                    // the items stop moving, the MaxVisibleModels setting
                    // will be honored.
                    for (int i = 0; i < itemCount; ++i)
                    {
                        // Find an element to add to the back of the list.
                        Viewport2DVisual3D backModel = GetNextModel(_viewport.BackModel);

                        if (_viewport.Children.Contains(backModel))
                            break;

                        // Make sure the model is in the correct location, so that it 
                        // looks like it enters the 3D scene from far off in the distance.
                        int logicalIndex = MaxVisibleModels + i;
                        ConfigureModel(backModel, logicalIndex);

                        // Add the model to the back of the scene.
                        _viewport.AddToBack(backModel);
                    }
                }

                for (int i = 0; i < itemCount; ++i)
                {
                    Viewport2DVisual3D frontModel = _viewport.RemoveFrontModel();

                    // This model is removed from the scene once the
                    // animation completes, if we are virtualizing.
                    _viewport.AddToBack(frontModel);
                }
            }
            else
            {
                for (int i = 0; i < itemCount; ++i)
                {
                    Viewport2DVisual3D frontModel = _viewport.FrontModel;
                    frontModel = GetPreviousModel(frontModel);

                    if (_viewport.Children.Contains(frontModel))
                        _viewport.Children.Remove(frontModel);

                    _viewport.AddToFront(frontModel);

                    // Make it look like the new front item(s) 
                    // is coming from the back of the list.
                    if (IsVirtualizing)
                        ConfigureModel(frontModel, MaxVisibleModels - i);
                }
            }
        }

        #endregion // RelocateModels

        #region SelectFrontItem

        private void MoveItems_SelectFrontItem()
        {
            if (_itemsOwner == null || _viewport.ModelCount == 0)
                return;

            var container = _viewport.FrontModel.Visual as FrameworkElement;
            if (container != null)
            {
                // If the owner has an ItemsSource it will
                // contain the item's DataContext, otherwise
                // the item is the container itself.
                if (_itemsOwner.Items.Contains(container))
                    _itemsOwner.SelectedItem = container;
                else
                    _itemsOwner.SelectedItem = container.DataContext;
            }
        }

        #endregion // SelectFrontItem

        #region BeginAnimations

        private void MoveItems_BeginAnimations(bool forward, TimeSpan animationLength)
        {
            var duration = new Duration(animationLength);
            int index = 0;
            foreach (Viewport2DVisual3D model in _viewport.GetModels())
            {
                Vector3D offsets = GetModelOffsets(index);

                var xAnimation = new DoubleAnimation
                {
                    To = offsets.X,
                    AccelerationRatio = forward ? 0 : 1,
                    DecelerationRatio = forward ? 1 : 0,
                    Duration = duration
                };

                var yAnimation = new DoubleAnimation
                {
                    To = offsets.Y,
                    AccelerationRatio = forward ? 0.7 : 0.3,
                    DecelerationRatio = forward ? 0.3 : 0.7,
                    Duration = duration
                };

                var zAnimation = new DoubleAnimation
                {
                    To = offsets.Z,
                    AccelerationRatio = forward ? 0.3 : 0.7,
                    DecelerationRatio = forward ? 0.7 : 0.3,
                    Duration = duration
                };

                var transform = model.Transform as TranslateTransform3D;
                if (transform != null)
                {
                    transform.BeginAnimation(TranslateTransform3D.OffsetXProperty, xAnimation);
                    transform.BeginAnimation(TranslateTransform3D.OffsetYProperty, yAnimation);
                    transform.BeginAnimation(TranslateTransform3D.OffsetZProperty, zAnimation);
                }

                if (AutoAdjustOpacity)
                {
                    var opacityAnimation = new DoubleAnimation
                    {
                        To = GetElementOpacity(index),
                        AccelerationRatio = 0.2,
                        DecelerationRatio = 0.8,
                        Duration = duration
                    };

                    var element = model.Visual as UIElement;
                    if (element != null)
                    {
                        element.BeginAnimation(OpacityProperty, opacityAnimation);
                    }
                }

                ++index;
            }
        }

        #endregion // BeginAnimations

        #region StartCleanupTimer

        private void MoveItems_StartCleanupTimer(TimeSpan animationLength)
        {
            _moveItemsCompletionTimer.Interval = animationLength;
            _moveItemsCompletionTimer.Start();
        }

        #endregion // StartCleanupTimer

        #region OnMoveItemsCompleted

        /// <summary>
        ///     Invoked when the items stop moving, due to a call to MoveItems().
        /// </summary>
        private void OnMoveItemsCompleted(object sender, EventArgs e)
        {
            _moveItemsCompletionTimer.Stop();

            if (_abortMoveItems)
                return;

            // Remove any extra models from the scene.
            while (MaxVisibleModels < _viewport.ModelCount)
                _viewport.RemoveBackModel();

            IsMovingItems = false;

            if (0 < _moveItemsRequestQueue.Count)
            {
                MoveItemsRequest req = _moveItemsRequestQueue.Dequeue();
                MoveItems(req.ItemCount, req.Forward, req.AnimationLength);
            }
        }

        #endregion // OnMoveItemsCompleted

        #endregion // MoveItems

        #region GetVisibleIndexFromChildIndex

        /// <summary>
        ///     Returns the visible index of the 3D model that represents
        ///     the 2D element at the specified index in the panel's Children
        ///     collection.  Both index values are zero-based.  The visible
        ///     index of the front model is 0, and each successive model in the
        ///     3D scene has a visible index one higher than the previous model.
        ///     If the element at the specified index is not currently in the
        ///     viewport, the visible index is -1.
        /// </summary>
        /// <param name="childIndex">A zero-based index of an element in the Children collection.</param>
        public int GetVisibleIndexFromChildIndex(int childIndex)
        {
            if (childIndex < 0 || Children.Count <= childIndex)
                throw new IndexOutOfRangeException("childIndex is invalid");

            UIElement elem = Children[childIndex];
            if (elem == null)
                throw new InvalidOperationException("Cannot get visible index of null/missing element.");

            Viewport2DVisual3D model = _elementTo3DModelMap[elem];

            return _viewport.GetVisualIndex(model);
        }

        #endregion // GetVisibleIndexFromChildIndex

        #region Layout Overrides

        /// <summary>
        ///     Arranges the Viewport3D in the panel.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _viewport.Arrange(new Rect(ORIGIN_POINT, finalSize));
            return finalSize;
        }

        /// <summary>
        ///     Measures the size needed to display the Viewport3D.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Make sure the viewport is parented on first measure.
            if (!_hasAddedViewport)
            {
                _hasAddedViewport = true;
                AddVisualChild(_viewport);
            }

            _viewport.Measure(availableSize);
            return _viewport.DesiredSize;
        }

        #endregion  // Layout Overrides

        #region Build 3D Model

        /// <summary>
        ///     Adds or removes a model in the scene.
        /// </summary>
        /// <param name="elementAdded"></param>
        /// <param name="elementRemoved"></param>
        protected override void OnLogicalChildrenChanged(UIElement elementAdded, UIElement elementRemoved)
        {
            // Do not create a model for the Viewport3D.
            if (elementAdded == _viewport)
                return;

            bool add =
                elementAdded != null &&
                !_elementTo3DModelMap.ContainsKey(elementAdded);

            if (add)
                AddModelForElement(elementAdded);

            bool remove =
                elementRemoved != null &&
                _elementTo3DModelMap.ContainsKey(elementRemoved);

            if (remove)
                RemoveModelForElement(elementRemoved);
        }

        private void AddModelForElement(UIElement element)
        {
            Viewport2DVisual3D model = BuildModel(element);

            // Add the new model at the correct location in our list of models.
            int idx = Children.IndexOf(element);
            _models.Insert(idx, model);

            _elementTo3DModelMap.Add(element, model);

            // If the scene has more than just a light source, grab the first
            // element and use it as the front model.  Otherwise, the scene
            // does not have any of our models in it yet, so pass the new one.
            Viewport2DVisual3D frontModel =
                _viewport.ModelCount > 0
                    ? _viewport.FrontModel
                    : model;

            BuildScene(frontModel);
        }

        private void RemoveModelForElement(UIElement element)
        {
            IsMovingItems = false;

            Viewport2DVisual3D model = _elementTo3DModelMap[element];
            _models.Remove(model);
            _elementTo3DModelMap.Remove(element);

            if (_viewport.Children.Contains(model))
            {
                _viewport.Children.Remove(model);
                BuildScene();
            }
        }

        /// <summary>
        ///     Returns an interactive 3D model that hosts
        ///     the specified UIElement.
        /// </summary>
        private Viewport2DVisual3D BuildModel(UIElement element)
        {
            var model = new Viewport2DVisual3D
            {
                Geometry = new MeshGeometry3D
                {
                    TriangleIndices = new Int32Collection(
                        new[] {0, 1, 2, 2, 3, 0}),
                    TextureCoordinates = new PointCollection(
                        new[]
                        {
                            new Point(0, 1),
                            new Point(1, 1),
                            new Point(1, 0),
                            new Point(0, 0)
                        }),
                    Positions = new Point3DCollection(
                        new[]
                        {
                            new Point3D(-1, -1, 0),
                            new Point3D(+1, -1, 0),
                            new Point3D(+1, +1, 0),
                            new Point3D(-1, +1, 0)
                        })
                },
                Material = new DiffuseMaterial(),
                Transform = new TranslateTransform3D(),
                // Host the 2D element in the 3D model.
                Visual = element
            };

            Viewport2DVisual3D.SetIsVisualHostMaterial(model.Material, true);

            return model;
        }

        #endregion // Build 3D Model

        #region Build 3D Scene

        /// <summary>
        ///     Tears down the 3D scene and rebuilds it, so that newly added
        ///     or removed models are taken into account.
        /// </summary>
        private void BuildScene()
        {
            if (0 < _viewport.ModelCount)
                BuildScene(_viewport.FrontModel);
        }

        /// <summary>
        ///     Tears down the current 3D scene and constructs a new one
        ///     where the specified model is the front object in view.
        /// </summary>
        private void BuildScene(Viewport2DVisual3D frontModel)
        {
            _viewport.RemoveAllModels();

            // Add in some 3D models, starting with the one in front.
            Viewport2DVisual3D current = frontModel;
            for (int i = 0; _viewport.ModelCount < MaxVisibleModels; ++i)
            {
                ConfigureModel(current, i);

                _viewport.AddToBack(current);

                current = GetNextModel(current);
                if (_viewport.Children.Contains(current))
                    break;
            }
        }

        #endregion // Build 3D Scene

        #region Selected Item Synchronization

        /// <summary>
        ///     Invoked when the panel is added to or removed from a list control.
        /// </summary>
        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            if (oldIsItemsHost && _itemsOwner != null)
            {
                _itemsOwner.SelectionChanged -= OnItemsOwnerSelectionChanged;
                _itemsOwner = null;
            }

            if (newIsItemsHost)
            {
                _itemsOwner = ItemsControl.GetItemsOwner(this) as Selector;
                if (_itemsOwner != null)
                    _itemsOwner.SelectionChanged += OnItemsOwnerSelectionChanged;
            }
        }

        private void OnItemsOwnerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_itemsOwner == null ||
                _itemsOwner.SelectedIndex < 0 ||
                (_itemsOwner.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated &&
                 _itemsOwner.ItemContainerGenerator.Status != GeneratorStatus.GeneratingContainers))
                return;

            var elem = _itemsOwner.ItemContainerGenerator.ContainerFromIndex(_itemsOwner.SelectedIndex) as UIElement;
            if (elem == null)
                return;

            Debug.Assert(_elementTo3DModelMap.ContainsKey(elem), "Why does the map not contain this element?");

            Viewport2DVisual3D model = _elementTo3DModelMap[elem];

            bool isSelectedItemInFront = _viewport.FrontModel == model;

            if (!isSelectedItemInFront)
            {
                if (IsMovingItems)
                {
                    IsMovingItems = false;
                    _abortMoveItems = true;
                    _moveItemsRequestQueue.Clear();
                }

                // When the scene needs to be adjusted due to a 
                // selection change, we do not animate because
                // the selected item could change many times
                // in rapid succession.  Instead, we just rebuild
                // the scene and place the selected item in front.
                BuildScene(model);
            }
        }

        #endregion // Selected Item Synchronization

        #region Private Helpers

        private bool IsVirtualizing
        {
            get { return MaxVisibleModels < _models.Count; }
        }

        /// <summary>
        ///     Adjusts a 3D model's location and opacity.
        /// </summary>
        private void ConfigureModel(Viewport2DVisual3D model, int index)
        {
            // We begin "null animations" to unapply any active animations.

            var trans = model.Transform as TranslateTransform3D;
            trans.BeginAnimation(TranslateTransform3D.OffsetXProperty, null);
            trans.BeginAnimation(TranslateTransform3D.OffsetYProperty, null);
            trans.BeginAnimation(TranslateTransform3D.OffsetZProperty, null);

            Vector3D offsets = GetModelOffsets(index);
            trans.OffsetX = offsets.X;
            trans.OffsetY = offsets.Y;
            trans.OffsetZ = offsets.Z;

            var elem = model.Visual as UIElement;
            elem.BeginAnimation(OpacityProperty, null);
            elem.Opacity = AutoAdjustOpacity ? GetElementOpacity(index) : 1.0;
        }

        private double GetElementOpacity(int index)
        {
            int ordinalIndex = index + 1;

            bool isinView = 0 < ordinalIndex && ordinalIndex <= MaxVisibleModels;
            if (!isinView)
                return 0.0;

            return 1.0/Math.Max(ordinalIndex, 1) + 0.1;
        }

        private Viewport2DVisual3D GetNextModel(Viewport2DVisual3D current)
        {
            if (!_models.Contains(current))
                throw new ArgumentException("current");

            // Wrap to the start of the list if necessary.
            int idx = _models.IndexOf(current) + 1;
            if (idx == _models.Count)
                idx = 0;

            return _models[idx];
        }

        private Viewport2DVisual3D GetPreviousModel(Viewport2DVisual3D current)
        {
            if (!_models.Contains(current))
                throw new ArgumentException("current");

            // Wrap to the end of the list if necessary.
            int idx = _models.IndexOf(current) - 1;
            if (idx == -1)
                idx = _models.Count - 1;

            return _models[idx];
        }

        private Vector3D GetModelOffsets(int index)
        {
            int ordinalIndex = index + 1;
            return Vector3D.Multiply(ordinalIndex, ItemLayoutDirection);
        }

        #endregion // Private Helpers
    }
}