
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.CustomControls
{
    public class SplitterPanel : Control
    {
        static SplitterPanel()
        {
            //This style is defined in Resources\SplitterPanel.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitterPanel), new FrameworkPropertyMetadata(typeof(SplitterPanel)));
        }
        const double SplitterExpandedSize = 250;
        const double SplitterCollapsedSize = 15; // Change TheCollapsibleColumn.Width in xaml if you change this!

        double _previousWidth = SplitterExpandedSize;
        Storyboard _contentLeftFadeOut;
        Storyboard _contentLeftFadeIn;
        FrameworkElement _layoutRoot;
        ColumnDefinition _collapsibleColumn;
        bool _isInitialized;
        bool _isDragStart;

        #region Header

        public string Header { get { return (string)GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(SplitterPanel));

        #endregion

        #region HeaderForeground

        public Brush HeaderForeground { get { return (Brush)GetValue(HeaderForegroundProperty); } set { SetValue(HeaderForegroundProperty, value); } }

        // Using a DependencyProperty as the backing store for HeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(SplitterPanel));

        #endregion

        #region HeaderBackground

        public Brush HeaderBackground { get { return (Brush)GetValue(HeaderBackgroundProperty); } set { SetValue(HeaderBackgroundProperty, value); } }

        // Using a DependencyProperty as the backing store for HeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(SplitterPanel));

        #endregion

        #region HeaderCollapsedForeground

        public Brush HeaderCollapsedForeground { get { return (Brush)GetValue(HeaderCollapsedForegroundProperty); } set { SetValue(HeaderCollapsedForegroundProperty, value); } }

        // Using a DependencyProperty as the backing store for HeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderCollapsedForegroundProperty =
            DependencyProperty.Register("HeaderCollapsedForeground", typeof(Brush), typeof(SplitterPanel));

        #endregion

        #region HeaderCollapsedBackground

        public Brush HeaderCollapsedBackground { get { return (Brush)GetValue(HeaderCollapsedBackgroundProperty); } set { SetValue(HeaderCollapsedBackgroundProperty, value); } }

        // Using a DependencyProperty as the backing store for HeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderCollapsedBackgroundProperty =
            DependencyProperty.Register("HeaderCollapsedBackground", typeof(Brush), typeof(SplitterPanel));

        #endregion

        #region SplitterBackground

        public Brush SplitterBackground { get { return (Brush)GetValue(SplitterBackgroundProperty); } set { SetValue(SplitterBackgroundProperty, value); } }

        // Using a DependencyProperty as the backing store for SplitterBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SplitterBackgroundProperty =
            DependencyProperty.Register("SplitterBackground", typeof(Brush), typeof(SplitterPanel));

        #endregion

        #region IsSplitterExpanded

        public bool IsSplitterExpanded { get { return (bool)GetValue(IsSplitterExpandedProperty); } set { SetValue(IsSplitterExpandedProperty, value); } }

        // Using a DependencyProperty as the backing store for IsSplitterExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSplitterExpandedProperty =
            DependencyProperty.Register("IsSplitterExpanded", typeof(bool), typeof(SplitterPanel), new PropertyMetadata(false, OnIsSplitterExpandedPropertyChanged));

        static void OnIsSplitterExpandedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var isExpanded = (bool)args.NewValue;
            var panel = (SplitterPanel)obj;
            panel.UpdateState(isExpanded ? SplitterState.Expanded : SplitterState.Collapsed);
        }

        #endregion

        #region ContentLeft

        public object ContentLeft { get { return ContentLeftProperty; } set { SetValue(ContentLeftProperty, value); } }

        // Using a DependencyProperty as the backing store for ContentLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentLeftProperty =
            DependencyProperty.Register("ContentLeft", typeof(object), typeof(SplitterPanel), new PropertyMetadata(null));

        #endregion

        #region ContentRight

        public object ContentRight { get { return GetValue(ContentRightProperty); } set { SetValue(ContentRightProperty, value); } }

        // Using a DependencyProperty as the backing store for ContentRight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentRightProperty =
            DependencyProperty.Register("ContentRight", typeof(object), typeof(SplitterPanel), new PropertyMetadata(null));

        #endregion

        #region OnApplyTemplate

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            try
            {
                _layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
                if(_layoutRoot != null)
                {
                    _contentLeftFadeOut = _layoutRoot.Resources["TheContentLeftFadeOut"] as Storyboard;
                    _contentLeftFadeIn = _layoutRoot.Resources["TheContentLeftFadeIn"] as Storyboard;
                }

                _collapsibleColumn = GetTemplateChild("TheCollapsibleColumn") as ColumnDefinition;
                if(_collapsibleColumn != null)
                {
                    _collapsibleColumn.MinWidth = SplitterCollapsedSize;
                }

                var gridSplitter = GetTemplateChild("TheSplitter") as GridSplitter;
                if(gridSplitter != null)
                {
                    gridSplitter.DragCompleted += OnSplitterDragCompleted;
                    gridSplitter.DragStarted += OnSplitterDragStarted;
                }

                var expansionButton = GetTemplateChild("TheExpansionButton") as Button;
                if(expansionButton != null)
                {
                    expansionButton.Click += OnToggleSplitterClick;
                }

                var collapsedHeader = GetTemplateChild("TheCollapsedHeader") as Border;
                if(collapsedHeader != null)
                {
                    collapsedHeader.MouseLeftButtonUp += OnHeaderMouseLeftButtonUp;
                }

                var expandedHeader = GetTemplateChild("ExpandedHearderText") as TextBlock;
                if(expandedHeader != null)
                {
                    expandedHeader.MouseLeftButtonUp += OnHeaderMouseLeftButtonUp;
                }
            }
            finally
            {
                _isInitialized = true;
                UpdateState(IsSplitterExpanded ? SplitterState.Expanded : SplitterState.Collapsed);
            }
        }

        #endregion

        #region OnHeaderMouseLeftButtonUp

        void OnHeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 1)
            {
                OnToggleSplitterClick(sender, e);
            }
        }

        #endregion

        #region OnSplitterDragStarted/Completed

        void OnSplitterDragStarted(object sender, DragStartedEventArgs e)
        {
            if(!IsSplitterExpanded)
            {
                _isDragStart = true;
                try
                {
                    IsSplitterExpanded = true;
                }
                finally
                {
                    _isDragStart = false;
                }
            }
        }

        void OnSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsSplitterExpanded = !(_collapsibleColumn.ActualWidth <= SplitterCollapsedSize);
        }

        #endregion

        #region OnToggleSplitterClick

        void OnToggleSplitterClick(object sender, RoutedEventArgs e)
        {
            IsSplitterExpanded = !IsSplitterExpanded;
        }

        #endregion

        #region UpdateState

        void UpdateState(SplitterState state)
        {
            if(!_isInitialized)
            {
                return;
            }

            switch(state)
            {
                case SplitterState.Collapsed:
                    if(!_isDragStart)
                    {
                        _previousWidth = _collapsibleColumn.ActualWidth > SplitterCollapsedSize ? _collapsibleColumn.ActualWidth : SplitterExpandedSize;
                        _collapsibleColumn.Width = new GridLength(SplitterCollapsedSize, GridUnitType.Pixel);
                    }
                    _contentLeftFadeOut.Begin(_layoutRoot);
                    break;

                case SplitterState.Expanded:
                    if(!_isDragStart)
                    {
                        _collapsibleColumn.Width = new GridLength(_previousWidth, GridUnitType.Pixel);
                    }
                    _contentLeftFadeIn.Begin(_layoutRoot);
                    break;
            }
        }

        #endregion
        #region SplitterState Enum

        enum SplitterState
        {
            Collapsed,
            Expanded
        }

        #endregion

    }
}
