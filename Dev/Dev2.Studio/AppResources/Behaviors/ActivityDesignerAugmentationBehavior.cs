
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
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Activities.Adorners;
using Dev2.CustomControls.Behavior;
using Dev2.CustomControls.Converters;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class ActivityDesignerAugmentationBehavior : Behavior<ActivityDesigner>
    {
        #region Class Members

        private bool _beginInt;
        private AdornerLayer _rootAdornerLayer;
        private IList<AdornerToggleButton> _adornerToggleButtons;

        private IList<AdornerToggleButton> AdornerToggleButtons
        {
            get { return _adornerToggleButtons ?? 
                (_adornerToggleButtons = new List<AdornerToggleButton>()); }
        }

        #endregion Class Members

        #region Attached Properties

        #region TitleBarBorderBrush

        public static Brush GetTitleBarBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TitleBarBackgroundProperty);
        }

        public static void SetTitleBarBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(TitleBarBackgroundProperty, value);
        }

        public static readonly DependencyProperty TitleBarBorderBrushProperty =
            DependencyProperty.RegisterAttached("TitleBarBorderBrush", typeof(Brush), 
            typeof(ActivityDesignerAugmentationBehavior), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        #endregion TitleBarBorderBrush

        #region TitleBarBackground

        public static Brush GetTitleBarBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TitleBarBackgroundProperty);
        }

        public static void SetTitleBarBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(TitleBarBackgroundProperty, value);
        }

        public static readonly DependencyProperty TitleBarBackgroundProperty =
            DependencyProperty.RegisterAttached("TitleBarBackground", typeof(Brush), 
            typeof(ActivityDesignerAugmentationBehavior), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        #endregion TitleBarBackground

        #endregion Attached Properties

        #region Dependency Properties

        #region TopTemplate

        public DataTemplate TopTemplate
        {
            get { return (DataTemplate)GetValue(TopTemplateProperty); }
            set { SetValue(TopTemplateProperty, value); }
        }

        public static readonly DependencyProperty TopTemplateProperty =
            DependencyProperty.Register("TopTemplate", typeof(DataTemplate), 
            typeof(ActivityDesignerAugmentationBehavior), new PropertyMetadata(null));

        #endregion TopTemplate

        #region BottomTemplate

        public DataTemplate BottomTemplate
        {
            get { return (DataTemplate)GetValue(BottomTemplateProperty); }
            set { SetValue(BottomTemplateProperty, value); }
        }

        public static readonly DependencyProperty BottomTemplateProperty =
            DependencyProperty.Register("BottomTemplate", typeof(DataTemplate), 
            typeof(ActivityDesignerAugmentationBehavior), new PropertyMetadata(null));

        #endregion BottomTemplate

        #region SupressConnectorNodes

        public bool SupressConnectorNodes
        {
            get { return (bool)GetValue(SupressConnectorNodesProperty); }
            set { SetValue(SupressConnectorNodesProperty, value); }
        }

        public static readonly DependencyProperty SupressConnectorNodesProperty =
            DependencyProperty.Register("SupressConnectorNodes", typeof(bool),
            typeof(ActivityDesignerAugmentationBehavior), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o,
            DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool)
            {
                var suppres = (bool)args.NewValue;
                var behavior = (ActivityDesignerAugmentationBehavior)o;

                if (suppres)
                {
                    behavior.RemoveConnectorNodeAdorners();
                }
                else
                {
                    behavior.AddConnectorNodeAdorners();
                }
            }
        }

        #endregion SupressConnectorNodes

        #region DataContext

        public object DataContext
        {
            get
            {
                return GetValue(DataContextProperty);
            }
            set
            {
                SetValue(DataContextProperty, value);
            }
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(object), 
            typeof(ActivityDesignerAugmentationBehavior), new PropertyMetadata(null));

        #endregion

        #endregion Dependency Properties

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            DataContext = AssociatedObject;
            SubscribeToEvents();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            UnsubscribeFromEvents();
        }

        #endregion Override Methods

        #region Event Handlers        

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            InsertAdorners();
            e.Handled = true;
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UnsubscribeFromEvents();
            routedEventArgs.Handled = true;
        }

        private void AdornerLayerOnMouseLeave(object sender, EventArgs eventArgs)
        {
            RemoveConnectorNodeAdorners();
        }

        #endregion Event Handlers

        #region Private Methods

        private void RemoveConnectorNodeAdorners()
        {
            UIElement uIElement = AssociatedObject.Parent as UIElement;
            if (uIElement == null || _rootAdornerLayer == null)
            {
                return;
            }

            Adorner[] adorners = _rootAdornerLayer.GetAdorners(uIElement);
            if (SupressConnectorNodes && adorners != null)
            {
                foreach (var adorner in adorners)
                {                    
                    adorner.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddConnectorNodeAdorners()
        {
            UIElement uIElement = AssociatedObject.Parent as UIElement;
            if (uIElement == null || _rootAdornerLayer == null)
            {
                return;
            }

            Adorner[] adorners = _rootAdornerLayer.GetAdorners(uIElement);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    adorner.Visibility = Visibility.Visible;
                }
            }
        }

        private void AssociatedObjectLayoutUpdated(object sender, EventArgs eventArgs)
        {
            InsertAdorners();
        }

        private void SubscribeToEvents()
        {            
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.LayoutUpdated -= AssociatedObjectLayoutUpdated;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.PreviewDragEnter -= AssociatedObjectOnPreviewDragEnter;

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.LayoutUpdated += AssociatedObjectLayoutUpdated;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.PreviewDragEnter += AssociatedObjectOnPreviewDragEnter;
        }


        void AssociatedObjectOnPreviewDragEnter(object sender, DragEventArgs dragEventArgs)
        {
            AddConnectorNodeAdorners();
        }

        private void UnsubscribeFromEvents()
        {
            AssociatedObject.LayoutUpdated -= AssociatedObjectLayoutUpdated;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.PreviewDragEnter -= AssociatedObjectOnPreviewDragEnter;
            
            if (_rootAdornerLayer != null)
            {
               
                _rootAdornerLayer.LayoutUpdated -= AdornerLayerOnMouseLeave;
                
                _rootAdornerLayer = null;
            }
        }

        private Border GetColoursBorder(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                Border childBorder = child as Border;
                if (childBorder != null && childBorder.Name != "debuggerVisuals")
                {
                    return childBorder;
                }
            }

            return null;
        }

        private void InsertAdorners()
        {
            if (VisualTreeHelper.GetChildrenCount(AssociatedObject) == 0)
            {
                return;
            }
            AssociatedObject.LayoutUpdated -= AssociatedObjectLayoutUpdated;

            _beginInt = AssociatedObject.IsInitialized;

            if (!_beginInt)
            {
                AssociatedObject.BeginInit();
            }

            //
            // Create visual for adorners
            //
            FrameworkElement topVisuals = TopTemplate.LoadContent() as FrameworkElement;
            if (topVisuals != null)
            {
                var topBorder = topVisuals.FindName("AdornerBorder") as FrameworkElement;
                if (topBorder != null)
                {
                     var behaviors = Interaction.GetBehaviors(AssociatedObject);
                     var actualSizeBehavior =
                     behaviors.FirstOrDefault(b => b.GetType() == typeof (ActualSizeBindingBehavior))
                         as ActualSizeBindingBehavior;
                    if (actualSizeBehavior != null)
                    {
                        var widthBinding = new Binding
                            {
                                Source = actualSizeBehavior,
                                Path = new PropertyPath("ActualWidth"),
                                Converter = new DoubleToMarginLeftConverter()
                            };
                        topBorder.SetBinding(FrameworkElement.MarginProperty, widthBinding);
                    }
                }
            }

            if (topVisuals != null && DataContext != null)
            {
                topVisuals.DataContext = DataContext ;
            }
            //
            // Setup intercept for supressing the connector nodes
            //
            _rootAdornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            if (_rootAdornerLayer != null)
            {
                _rootAdornerLayer.LayoutUpdated += AdornerLayerOnMouseLeave;
            }

            //
            // Get controls from visual tree
            //
            Border childBorder = VisualTreeHelper.GetChild(AssociatedObject, 0) as Border;

            if (childBorder == null) return;

            Grid hostGrid = childBorder.Child as Grid;

            if (hostGrid == null) return;

            Border titleBarBorder = GetColoursBorder(hostGrid);

            if (titleBarBorder == null) return;

            //
            // Set colour attached properties
            //
            Binding backgroundBinding = new Binding("Background");
            backgroundBinding.Source = titleBarBorder;

            Binding borderBrushBinding = new Binding("BorderBrush");
            borderBrushBinding.Source = titleBarBorder;
            
            if (topVisuals != null)
            {
                topVisuals.BeginInit();
                topVisuals.SetBinding(TitleBarBackgroundProperty, backgroundBinding);
                topVisuals.SetBinding(TitleBarBorderBrushProperty, borderBrushBinding);
                topVisuals.EndInit();
            }

            //
            // Insert adorners
            //
            AdornerDecorator decorator = new AdornerDecorator();
            childBorder.Child = null;
            decorator.Child = hostGrid;
            childBorder.Child = decorator;

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(hostGrid);

            if (!_beginInt)
            {
                AssociatedObject.BeginInit();
            }

            TopVisualsAdornerWrapper topVisualsAdornerWrapper = 
                new TopVisualsAdornerWrapper(hostGrid, topVisuals, AssociatedObject);

            adornerLayer.Add(topVisualsAdornerWrapper);

            if (!_beginInt)
            {
                adornerLayer.EndInit();
                AssociatedObject.EndInit();
            }

            AdornerToggleButtons.Clear();
            topVisualsAdornerWrapper.Descendents().OfType<AdornerToggleButton>().ToList()
                .ForEach(d =>
                    {
                        AdornerToggleButtons.Add(d);
                        d.Click += d_Click;
                    });
        }

        void d_Click(object sender, RoutedEventArgs e)
        {
            SupressConnectorNodes = AdornerToggleButtons.Any(t => t.IsChecked == true);
        }

        #endregion Private Methods

        #region Private Classes

        private class TopVisualsAdornerWrapper : Adorner
        {
            private readonly VisualCollection _visualChildren;

            public TopVisualsAdornerWrapper(UIElement target, FrameworkElement adornerContent, ActivityDesigner activityDesigner)
                : base(target)
            {
                _visualChildren = new VisualCollection(this) 
                {
                    adornerContent
                };
            }

            protected override int VisualChildrenCount { get { return _visualChildren.Count; } }
            protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

            protected override Size MeasureOverride(Size constraint)
            {
                FrameworkElement fe = _visualChildren[0] as FrameworkElement;
                if (fe != null)
                {
                    fe.Measure(constraint);
                    return fe.DesiredSize;
                }

                return new Size();
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                FrameworkElement fe = _visualChildren[0] as FrameworkElement;
                if (fe != null)
                {
                    fe.Arrange(new Rect(new Point(0, 0), finalSize));
                    return new Size(fe.ActualWidth, fe.ActualHeight);
                }

                return new Size();
            }
        }

        private class BottomVisualsAdornerWrapper : Adorner
        {
            private readonly VisualCollection _visualChildren;
            private readonly ActivityDesigner _activity;

            public BottomVisualsAdornerWrapper(UIElement target, FrameworkElement adornerContent, ActivityDesigner activityDesigner)
                : base(target)
            {
                _activity = activityDesigner;
                _visualChildren = new VisualCollection(this);
                _visualChildren.Add(adornerContent);
            }

            protected override int VisualChildrenCount { get { return _visualChildren.Count; } }
            protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

            protected override Size MeasureOverride(Size constraint)
            {
                FrameworkElement fe = _visualChildren[0] as FrameworkElement;

                if (fe != null)
                {
                    fe.Measure(constraint);

                    Size size = new Size(_activity.ActualWidth, fe.DesiredSize.Height);
                    return size;
                }

                return new Size();
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                FrameworkElement ae = AdornedElement as FrameworkElement;
                FrameworkElement fe = _visualChildren[0] as FrameworkElement;
                if (fe != null && ae != null)
                {
                    fe.Arrange(new Rect(new Point(ae.ActualWidth - fe.ActualWidth, ae.ActualHeight), finalSize));
                    return new Size(fe.ActualWidth, fe.ActualHeight);
                }

                return new Size();
            }
        }

        #endregion Private Classes
    }
}
