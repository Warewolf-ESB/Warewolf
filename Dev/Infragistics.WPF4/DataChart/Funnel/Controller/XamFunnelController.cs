using System;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

using System.Collections;
using System.Net;
using Infragistics.Controls.Charts.Messaging;
using Infragistics.Controls.Charts.Util;
using System.Windows.Threading;
using System.Threading;



namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the main logic of the funnel chart. Determines what should be rendered but not the how.
    /// </summary>
    internal class XamFunnelController
    {
        public XamFunnelController()
        {
            DoubleAnimator = new DoubleAnimator(0, 1, 2000);
            DoubleAnimator.PropertyChanged += DoubleAnimator_PropertyChanged;
            PreviousFrame = new FunnelFrame();
            InterpolatedFrame = new FunnelFrame();
            CurrentFrame = new FunnelFrame();
            SliceSelectionManager = new SliceSelectionManager();

            ValueColumn = new DoubleColumn();
            InnerLabelColumn = new ObjectColumn();
            OuterLabelColumn = new ObjectColumn();

            RenderingMessages = new MessageChannel();
            ModelUpdateMessages = new MessageChannel();

            MessageHandler = new MessageHandler();
            MessageHandler.AddHandler(typeof(PropertyChangedMessage), PropertyChangedMessageReceived);
            MessageHandler.AddHandler(typeof(DataUpdatedMessage), DataUpdatedMessageReceived);
            MessageHandler.AddHandler(typeof(ViewportChangedMessage), ViewportChangedMessageReceived);
            MessageHandler.AddHandler(typeof(ViewPropertyChangedMessage), ViewPropertyChangedMessageReceived);
            //MessageHandler.AddHandler(typeof(SliceClickedMessage), SliceClickedMessageReceived);
            MessageHandler.AddHandler(typeof(MouseMoveMessage), MouseMoveMessageReceived);
            MessageHandler.AddHandler(typeof(MouseButtonMessage), MouseButtonMessageReceived);
            MessageHandler.AddHandler(typeof(MouseLeaveMessage), MouseLeaveMessageReceived);
            MessageHandler.AddHandler(typeof(UserSelectedItemsChangedMessage), UserSelectedItemsChangedMessageReceived);
            MessageHandler.AddHandler(typeof(LabelSizeChangedMessage), LabelSizeChangedMessageReceived);
        }

        /// <summary>
        /// Represents an animation tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleAnimator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TransitionProgress = DoubleAnimator.TransitionProgress;

            if (TransitionProgress == 1.0)
            {
                AnimationInProgress = false;
                DisplayFrame(CurrentFrame);
            }

            if (AnimationInProgress)
            {
                double q = 1.0 - TransitionProgress;
                FunnelFrame.Interpolate(InterpolatedFrame, PreviousFrame, CurrentFrame, TransitionProgress);
                DisplayFrame(InterpolatedFrame);
            }
        }

        #region "Properties"

        private DoubleAnimator _doubleAnimator;
        /// <summary>
        /// Handles the animation concerns of the funnel chart.
        /// </summary>
        protected DoubleAnimator DoubleAnimator
        {
            get { return _doubleAnimator; }
            set { _doubleAnimator = value; }
        }

        private Bezier _bezier;
        protected Bezier Bezier
        {
            get { return _bezier; }
            set { _bezier = value; }
        }

        private MessageHandler _messageHandler;
        protected MessageHandler MessageHandler
        {
            get { return _messageHandler; }
            set { _messageHandler = value; }
        }


        private DoubleColumn _valueColumn;
        protected DoubleColumn ValueColumn
        {
            get { return _valueColumn; }
            set { _valueColumn = value; }
        }

        private ObjectColumn _innerLabelColumn;
        protected ObjectColumn InnerLabelColumn
        {
            get { return _innerLabelColumn; }
            set { _innerLabelColumn = value; }
        }

        private ObjectColumn _outerLabelColumn;
        protected ObjectColumn OuterLabelColumn
        {
            get { return _outerLabelColumn; }
            set { _outerLabelColumn = value; }
        }

        private string _valueMemberPath;
        protected string ValueMemberPath
        {
            get { return _valueMemberPath; }
            set { _valueMemberPath = value; }
        }

        private string _outerLabelMemberPath;
        protected string OuterLabelMemberPath
        {
            get { return _outerLabelMemberPath; }
            set { _outerLabelMemberPath = value; }
        }

        private string _innerLabelMemberPath;
        protected string InnerLabelMemberPath
        {
            get { return _innerLabelMemberPath; }
            set { _innerLabelMemberPath = value; }
        }

        private bool _refreshRequired;
        internal bool RefreshRequired
        {
            get { return _refreshRequired; }
            set { _refreshRequired = value; }
        }

        private double _viewportWidth;
        protected double ViewportWidth
        {
            get { return _viewportWidth; }
            set { _viewportWidth = value; }
        }

        private double _viewportHeight;
        protected double ViewportHeight
        {
            get { return _viewportHeight; }
            set { _viewportHeight = value; }
        }

        private MessageChannel _renderingMessages;
        protected MessageChannel RenderingMessages
        {
            get { return _renderingMessages; }
            set { _renderingMessages = value; }
        }

        private MessageChannel _modelUpdateMessages;
        protected MessageChannel ModelUpdateMessages
        {
            get { return _modelUpdateMessages; }
            set { _modelUpdateMessages = value; }
        }

        private IOuterLabelWidthDecider _widthDecider;
        protected IOuterLabelWidthDecider WidthDecider
        {
            get { return _widthDecider; }
            set { _widthDecider = value; }
        }

        private IFunnelLabelSizeDecider _sizeDecider;
        protected IFunnelLabelSizeDecider SizeDecider
        {
            get { return _sizeDecider; }
            set { _sizeDecider = value; }
        }

        private bool _innerLabelsVisibility;
        protected bool InnerLabelVisibility
        {
            get { return _innerLabelsVisibility; }
            set { _innerLabelsVisibility = value; }
        }

        private bool _outerLabelsVisibility;
        protected bool OuterLabelVisibility
        {
            get { return _outerLabelsVisibility; }
            set { _outerLabelsVisibility = value; }
        }

        private double _transitionProgress;
        public double TransitionProgress
        {
            get { return _transitionProgress; }
            set { _transitionProgress = value; }
        }

        private bool _isSplined;
        protected bool IsSplined
        {
            get { return _isSplined; }
            set { _isSplined = value; }
        }

        private double _bottomEdgeWidth;
        protected double BottomEdgeWidth
        {
            get { return _bottomEdgeWidth; }
            set { _bottomEdgeWidth = value; }
        }

        private Brush[] _brushes;
        protected Brush[] Brushes
        {
            get { return _brushes; }
            set { _brushes = value; }
        }

        private Brush[] _outlines;
        protected Brush[] Outlines
        {
            get { return _outlines; }
            set { _outlines = value; }
        }

        private OuterLabelAlignment _outerLabelAlignment;
        protected OuterLabelAlignment OuterLabelAlignment
        {
            get { return _outerLabelAlignment; }
            set { _outerLabelAlignment = value; }
        }

        private FunnelSliceDisplay _funnelSliceDisplay;
        protected FunnelSliceDisplay FunnelSliceDisplay
        {
            get { return _funnelSliceDisplay; }
            set { _funnelSliceDisplay = value; }
        }

        private bool _animationInProgress;
        protected bool AnimationInProgress
        {
            get { return _animationInProgress; }
            set { _animationInProgress = value; }
        }

        private double _transitionDuration;
        protected double TransitionDuration
        {
            get { return _transitionDuration; }
            set { _transitionDuration = value; }
        }

        private bool _useBezierCurve;
        protected bool UseBezierCurve
        {
            get { return _useBezierCurve; }
            set { _useBezierCurve = value; }
        }

        private Point _upperBezierControlPoint;
        protected Point UpperBezierControlPoint
        {
            get { return _upperBezierControlPoint; }
            set { _upperBezierControlPoint = value; }
        }

        private Point _lowerBezierControlPoint;
        protected Point LowerBezierControlPoint
        {
            get { return _lowerBezierControlPoint; }
            set { _lowerBezierControlPoint = value; }
        }

        private bool _allowSliceSelection;
        protected bool AllowSliceSelection
        {
            get { return _allowSliceSelection; }
            set { _allowSliceSelection = value; }
        }

        private int _hoveredSliceIndex;
        protected int HoveredSliceIndex
        {
            get { return _hoveredSliceIndex; }
            set { _hoveredSliceIndex = value; }
        }

        private int _pressedSliceIndex;
        protected int PressedSliceIndex
        {
            get { return _pressedSliceIndex; }
            set { _pressedSliceIndex = value; }
        }

        private IItemProvider _itemProvider;
        protected IItemProvider ItemProvider
        {
            get { return _itemProvider; }
            set { _itemProvider = value; }
        }


        private ServiceProvider _serviceProvider;
        public ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                ServiceProvider oldValue = _serviceProvider;
                _serviceProvider = value;
                OnServiceProviderChanged(oldValue, _serviceProvider);
            }
        }

        private SliceSelectionManager _sliceSelectionManager;
        internal SliceSelectionManager SliceSelectionManager
        {
            get { return _sliceSelectionManager; }
            set { _sliceSelectionManager = value; }
        }

        private FunnelFrame _previousFrame;
        protected FunnelFrame PreviousFrame
        {
            get { return _previousFrame; }
            set { _previousFrame = value; }
        }

        private FunnelFrame _currentFrame;
        protected FunnelFrame CurrentFrame
        {
            get { return _currentFrame; }
            set { _currentFrame = value; }
        }

        private FunnelFrame _interpolatedFrame;
        protected FunnelFrame InterpolatedFrame
        {
            get { return _interpolatedFrame; }
            set { _interpolatedFrame = value; }
        }

        private Style _normalSliceStyle;
        protected Style NormalSliceStyle
        {
            get { return _normalSliceStyle; }
            set { _normalSliceStyle = value; }
        }

        private Style _selectedSliceStyle;
        protected Style SelectedSliceStyle
        {
            get { return _selectedSliceStyle; }
            set { _selectedSliceStyle = value; }
        }

        private Style _unselectedSliceStyle;
        protected Style UnselectedSliceStyle
        {
            get { return _unselectedSliceStyle; }
            set { _unselectedSliceStyle = value; }
        }

        private bool _useUnselectedStyle;
        protected bool UseUnselectedStyle
        {
            get { return _useUnselectedStyle; }
            set { _useUnselectedStyle = value; }
        }

        private bool _isInverted;
        protected bool IsInverted
        {
            get { return _isInverted; }
            set { _isInverted = value; }
        }

        private bool _userOuterLabelsForLegend;
        protected bool UserOuterLabelsForLegend
        {
            get { return _userOuterLabelsForLegend; }
            set { _userOuterLabelsForLegend = value; }
        }

        private bool _hasTooltip;
        protected bool HasTooltip
        {
            get { return _hasTooltip; }
            set { _hasTooltip = value; }
        }



        #endregion

        private void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                MessageChannel channel =
                    oldValue.GetService("ConfigurationMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.DetachTarget(MessageReceived);
                }
                channel =
                    oldValue.GetService("InteractionMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.DetachTarget(MessageReceived);
                }
                RenderingMessages.DetachFromNext();
                ModelUpdateMessages.DetachFromNext();
            }
            if (newValue != null)
            {
                MessageChannel channel =
                    newValue.GetService("ConfigurationMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.AttachTarget(MessageReceived);
                }
                channel =
                    newValue.GetService("InteractionMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.AttachTarget(MessageReceived);
                }

                MessageChannel rendering =
                    newValue.GetService("RenderingMessages") as MessageChannel;
                RenderingMessages.ConnectTo(rendering);
                MessageChannel modelUpdates =
                    newValue.GetService("ModelUpdateMessages") as MessageChannel;
                ModelUpdateMessages.ConnectTo(modelUpdates);
            }
            RefreshRequired = true;
        }

        /// <summary>
        /// Main message entry point.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void MessageReceived(Message m)
        {
            MessageHandler.MessageReceived(m);
            RefreshVisuals();
        }

        /// <summary>
        /// Determines if the size is valid, to allow for rendering.
        /// </summary>
        /// <returns></returns>
        internal bool SizeValid()
        {




            return ViewportHeight > 0 &&
               !double.IsNaN(ViewportHeight) &&
               !double.IsInfinity(ViewportHeight) &&
               ViewportWidth > 0 &&
               !double.IsNaN(ViewportWidth) &&
               !double.IsInfinity(ViewportWidth);

        }

        /// <summary>
        /// Determines if the internal state is valid, to allow for rendering.
        /// </summary>
        /// <returns></returns>
        internal bool IsValid()
        {
            return ValueColumn != null &&
                ValueColumn.Values != null &&
                ValueColumn.Values.Count > 0 &&
                WidthDecider != null &&
                SizeDecider != null &&
                Brushes != null &&
                Brushes.Length > 0 &&
                Outlines != null &&
                Outlines.Length > 0 &&
                SizeValid();
        }

        /// <summary>
        /// Refresh the visual components of the chart if necessary.
        /// </summary>
        private void RefreshVisuals()
        {
            if (!RefreshRequired)
            {
                return;
            }

            if (!IsValid())
            {
                ClearRendering();
                return;
            }

            RefreshRequired = false;

            double outerLabelWidth = WidthDecider.DecideWidth(GetOuterLabels());
            if (outerLabelWidth > ViewportWidth)
            {
                outerLabelWidth = 0;
            }
            if (!OuterLabelVisibility)
            {
                outerLabelWidth = 0;
            }
            double plotAreaWidth = ViewportWidth - outerLabelWidth;

            bool plotOuterLabels = outerLabelWidth > 0;
            bool plotInnerLabels = InnerLabelVisibility;
            bool plotSlices = plotAreaWidth > 0;
            if (plotSlices || plotOuterLabels)
            {
                Plot(plotSlices, plotOuterLabels, plotInnerLabels, plotAreaWidth, outerLabelWidth);
            }
        }

        private void ClearRendering()
        {
            if (ShouldAnimate() && TransitionProgress > 0)
            {
                DoubleAnimator.Stop();
            }
            SendClearMessage("LeftLabels");
            SendClearMessage("RightLabels");
            SendClearMessage("SliceArea");
            CleanupUnused();
            if (Legend != null)
            {
                while (Legend.Children.Count > 0)
                {
                    Legend.Children.RemoveAt(0);
                }
            }
            CurrentFrame.Slices.Clear();
            InterpolatedFrame.Slices.Clear();
            PreviousFrame.Slices.Clear();
        }

        private void CleanupUnused()
        {
            FrameRenderCompleteMessage frc = new FrameRenderCompleteMessage();
            RenderingMessages.SendMessage(frc);
        }

        /// <summary>
        /// Plots the current frame, and triggers animation if necessary.
        /// </summary>
        /// <param name="plotSlices">Indicates if the slices should be rendered.</param>
        /// <param name="plotOuterLabels">Indicates if the outer labels should be rendered.</param>
        /// <param name="plotInnerLabels">Indicates if the inner labels should be rendered.</param>
        /// <param name="plotAreaWidth">Indicates the available width in which to render.</param>
        /// <param name="outerLabelWidth">Indicates width in which the outer labels should be rendered.</param>
        private void Plot(
            bool plotSlices,
            bool plotOuterLabels,
            bool plotInnerLabels,
            double plotAreaWidth,
            double outerLabelWidth)
        {
            if (AnimationInProgress)
            {
                FunnelFrame frame = PreviousFrame;
                PreviousFrame = InterpolatedFrame;
                InterpolatedFrame = frame;
            }
            else 
            {
                FunnelFrame frame = PreviousFrame;
                PreviousFrame = CurrentFrame;
                CurrentFrame = frame;
            }

            PlotFrame(CurrentFrame,
                plotSlices,
                plotOuterLabels,
                plotInnerLabels,
                plotAreaWidth,
                outerLabelWidth);

            DoubleAnimator.Stop();
            TransitionProgress = 0;
            if (ShouldAnimate())
            {
                FunnelFrame.Interpolate(InterpolatedFrame, PreviousFrame, CurrentFrame, TransitionProgress);
                DisplayFrame(InterpolatedFrame);
                AnimationInProgress = true;
                DoubleAnimator.Start();
            }
            else
            {
                DisplayFrame(CurrentFrame);
            }
        }

        private double SafeGetValue(double value)
        {
            if (double.IsNaN(value))
            {
                return 0.0;
            }

            return Math.Abs(value);
        }

        private bool _effectiveUseBezierCurve = false;

        /// <summary>
        /// Plots the current frame.
        /// </summary>
        /// <param name="frame">The frame to update with the current render.</param>
        /// <param name="plotSlices">Indicates if the slices should be plotted.</param>
        /// <param name="plotOuterLabels">Indicates if the outer labels should be plotted.</param>
        /// <param name="plotInnerLabels">Indicates if the inner labels should be plotted.</param>
        /// <param name="plotAreaWidth">Indicates the width in which to render the slices.</param>
        /// <param name="outerLabelWidth">Indicates the width in which to render the outer labels.</param>
        private void PlotFrame(
            FunnelFrame frame,
            bool plotSlices,
            bool plotOuterLabels,
            bool plotInnerLabels,
            double plotAreaWidth,
            double outerLabelWidth)
        {
            double plotAreaCenter = plotAreaWidth / 2.0;
            _effectiveUseBezierCurve = UseBezierCurve;

            if (_effectiveUseBezierCurve)
            {
                InitializeBezier(plotAreaWidth, plotAreaCenter);
            }
            else
            {
                Bezier = null;
            }

            bool doWeighting = FunnelSliceDisplay == FunnelSliceDisplay.Weighted;
            IntColumn indices = GetSortedIndices();
            double totalValue = 0;
            foreach (int index in indices)
            {
                totalValue += SafeGetValue((double)ValueColumn.Values[index]);
            }
            if (totalValue == 0)
            {
                doWeighting = false;
            }

            double unweightedHeight =
                ViewportHeight / indices.Values.Count;

            double currentTop = 0.0;

            frame.OuterLabelWidth = outerLabelWidth;
            frame.InnerLabelsShown = plotInnerLabels;
            frame.OuterLabelsShown = plotOuterLabels;
            frame.OuterAlignedLeft = OuterLabelAlignment == OuterLabelAlignment.Left;
            frame.Slices = new SliceInfoList();

            int i = 0;
            foreach (int index in indices)
            {
                double currentHeight;
                if (doWeighting)
                {
                    currentHeight = (SafeGetValue((double)ValueColumn.Values[index]) / totalValue) * ViewportHeight;
                }
                else
                {
                    currentHeight = unweightedHeight;
                }

                double topWidth = GetWidthAt(plotAreaWidth, currentTop);
                double currentBottom = currentTop + currentHeight;
                double bottomWidth = GetWidthAt(plotAreaWidth, currentBottom);
                double halfTopWidth = topWidth / 2.0;
                double halfBottomWidth = bottomWidth / 2.0;

                SliceInfo info = new SliceInfo();

                double offsetX = 0.0;
                if (halfBottomWidth > halfTopWidth)
                {
                    offsetX = plotAreaCenter - halfBottomWidth;
                }
                else
                {
                    offsetX = plotAreaCenter - halfTopWidth;
                }
                
                double offsetY = currentTop;

                info.Slice.Fill = GetSliceFill(index);
                info.Slice.Outline = GetSliceOutline(index);
                info.Slice.Style = GetSliceStyle(index);

                if (plotSlices)
                {
                    info.HasSlice = true;
                    
                    info.Slice.UpperLeft = new Point(
                        plotAreaCenter - halfTopWidth - offsetX,
                        currentTop - offsetY);
                    info.Slice.UpperRight = new Point(
                        plotAreaCenter + halfTopWidth - offsetX,
                        currentTop - offsetY);
                    info.Slice.LowerLeft = new Point(
                        plotAreaCenter - halfBottomWidth - offsetX,
                        currentBottom - offsetY);
                    info.Slice.LowerRight = new Point(
                        plotAreaCenter + halfBottomWidth - offsetX,
                        currentBottom - offsetY);

                    if (_effectiveUseBezierCurve)
                    {
                        AddBezierPoints(info.Slice, currentTop, currentBottom, plotAreaCenter, offsetX, offsetY);
                    }
                }

                if (plotInnerLabels)
                {
                    info.Slice.HasInnerLabel = true;
                    info.Slice.InnerLabelPosition = new Point(
                        plotAreaCenter - offsetX,
                        (currentTop + currentBottom) / 2.0 - offsetY);
                    info.Slice.InnerLabel = GetInnerLabel(index);
                    var innerLabelSize = SizeDecider.DecideLabelSize(info, true);
                    if (innerLabelSize.Width > plotAreaWidth)
                    {
                        info.Slice.HasInnerLabel = false;
                    }
                    if (innerLabelSize.Height > currentHeight)
                    {
                        info.Slice.HasInnerLabel = false;
                    }
                }
                else
                {
                    info.Slice.HasInnerLabel = false;
                    info.Slice.InnerLabel = GetInnerLabel(index);
                }

                if (plotInnerLabels || plotSlices)
                {
                    info.Slice.Offset = new Point(offsetX, offsetY);
                    info.Slice.Item = ValueColumn.Values[index];
                    info.Slice.Index = index;
                }

                if (plotOuterLabels)
                {
                    info.HasOuterLabel = true;
                    info.OuterLabelPosition = new Point(
                        0.0,
                        (currentTop + currentBottom) / 2.0);
                    info.OuterLabel = GetOuterLabel(index);
                    Size outerLabelSize = SizeDecider.DecideLabelSize(info, false);
                    if (outerLabelSize.Height > currentHeight)
                    {
                        info.HasOuterLabel = false;
                    }
                }
                else
                {
                    info.HasOuterLabel = false;
                    info.OuterLabel = GetOuterLabel(index);
                }

                info.Index = index;

                frame.Slices.Add(info);

                currentTop += currentHeight;
                i++;
            }

            RenderLegend(frame);
            frame.Slices.IndexSort();
        }

        /// <summary>
        /// Determines the bezier points to add for a slice.
        /// </summary>
        /// <param name="sliceAppearance">The appearance parameters for the slice.</param>
        /// <param name="currentTop">The top of the current slice.</param>
        /// <param name="currentBottom">The bottom of the current slice.</param>
        /// <param name="plotAreaCenter">The center of the plot area.</param>
        /// <param name="offsetx">The x offset of the top left of the slice area.</param>
        /// <param name="offsety">The y offset of the top left of the slice area.</param>
        private void AddBezierPoints(SliceAppearance sliceAppearance, double currentTop, double currentBottom, double plotAreaCenter, double offsetx, double offsety)
        {
            BezierPoint top = Bezier.GetPointAt(currentTop);
            BezierPoint bottom = Bezier.GetPointAt(currentBottom);
            PointList points = new PointList();
            PointList rightPoints = new PointList();
            int startIndex = top.Index;
            int endIndex = bottom.Index;

            for (int i = startIndex; i <= endIndex; i++)
            {
                points.Add(
                    new Point(
                    ((BezierPoint)Bezier.Points[i]).Point.X - offsetx,
                    ((BezierPoint)Bezier.Points[i]).Point.Y - offsety));
            }

            for (int i = endIndex; i >= startIndex; i--)
            {
                Point p = ((BezierPoint)Bezier.Points[i]).Point;
                double dist = plotAreaCenter - p.X;
                Point rightPoint = new Point(plotAreaCenter + dist - offsetx, p.Y - offsety);
                rightPoints.Add(rightPoint);
            }

            sliceAppearance.BezierPoints = points;
            sliceAppearance.RightBezierPoints = rightPoints;
        }

        /// <summary>
        /// Determines if two points differ.
        /// </summary>
        /// <param name="p1">The first point to compare</param>
        /// <param name="p2">The second point to compare</param>
        /// <returns>True if the points differ.</returns>
        private bool PointDiffers(Point p1, Point p2)
        {
            if (p1.X != p2.X ||
                p1.Y != p2.Y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes the parameters of the bezier curve that defines the edge of the funnel.
        /// </summary>
        /// <param name="plotAreaWidth">The width of the slice plot area.</param>
        /// <param name="plotAreaCenter">The center of the slice plot area.</param>
        private void InitializeBezier(double plotAreaWidth, double plotAreaCenter)
        {
            Point p0;
            Point p3;
            if (IsInverted)
            {
                p0 = new Point(plotAreaCenter - (plotAreaWidth * BottomEdgeWidth / 2.0), 0);
                p3 = new Point(plotAreaCenter - (plotAreaWidth / 2.0), ViewportHeight);
            }
            else
            {
                p0 = new Point(plotAreaCenter - (plotAreaWidth / 2.0), 0);
                p3 = new Point(plotAreaCenter - (plotAreaWidth * BottomEdgeWidth / 2.0), ViewportHeight);
            }

            Point upperBezier = UpperBezierControlPoint.Y < LowerBezierControlPoint.Y ? UpperBezierControlPoint : LowerBezierControlPoint;
            Point lowerBezier = LowerBezierControlPoint.Y > UpperBezierControlPoint.Y ? LowerBezierControlPoint : UpperBezierControlPoint;
            if (upperBezier.Y < 0)
            {
                upperBezier.Y = 0;
            }
            if (lowerBezier.Y > 1.0)
            {
                lowerBezier.Y = 1.0;
            }
            if (IsInverted)
            {
                double swap = lowerBezier.X;
                lowerBezier.X = upperBezier.X;
                upperBezier.X = swap;

                swap = upperBezier.Y;
                upperBezier.Y = 1.0 - lowerBezier.Y;
                lowerBezier.Y = 1.0 - swap;
            }

            Point p1 = new Point(
                plotAreaWidth * upperBezier.X,
                ViewportHeight * upperBezier.Y);

            Point p2 = new Point(
                plotAreaWidth * lowerBezier.X,
                ViewportHeight * lowerBezier.Y);

            if (Bezier == null ||
                PointDiffers(p0, Bezier.P0) ||
                PointDiffers(p1, Bezier.P1) ||
                PointDiffers(p2, Bezier.P2) ||
                PointDiffers(p3, Bezier.P3))
            {
                Bezier = new Bezier(
                    p0, p1, p2, p3, 2.0, plotAreaCenter);
            }

            if (Bezier == null)
            {
                _effectiveUseBezierCurve = false;
            }
            else
            {
                _effectiveUseBezierCurve = Bezier.Valid;
            }
        }

        /// <summary>
        /// Gets the outer label at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private object GetOuterLabel(int index)
        {
            if (OuterLabelColumn == null ||
                OuterLabelColumn.Values == null ||
                index > OuterLabelColumn.Values.Count - 1)
            {
                return null;
            }
            return OuterLabelColumn.Values[index];
        }

        /// <summary>
        /// Gets the inner label at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private object GetInnerLabel(int index)
        {
            if (InnerLabelColumn == null ||
                InnerLabelColumn.Values == null ||
                index > InnerLabelColumn.Values.Count - 1)
            {
                return null;
            }
            return InnerLabelColumn.Values[index];
        }

        /// <summary>
        /// Gets the style for the slice at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Style GetSliceStyle(int index)
        {
            if (SliceSelectionManager.IsSelected(index))
            {
                return SelectedSliceStyle;
            }
            if (SliceSelectionManager.IsUnselected(index)
                && UseUnselectedStyle)
            {
                return UnselectedSliceStyle;
            }

            return NormalSliceStyle;
        }

        /// <summary>
        /// Gets the outline brush for the slice at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Brush GetSliceOutline(int index)
        {
            return Outlines[index % Outlines.Length];
        }

        /// <summary>
        /// Gets the fill brush for the slice at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Brush GetSliceFill(int index)
        {
            return Brushes[index % Brushes.Length];
        }

        /// <summary>
        /// Gets the width of the funnel at the specified height.
        /// </summary>
        /// <param name="plotAreaWidth">The width of the entire slice area.</param>
        /// <param name="currentTop">The y value to get the width for.</param>
        /// <returns>The width at the specified height.</returns>
        private double GetWidthAt(double plotAreaWidth, double currentTop)
        {
            double bottomWidth = plotAreaWidth * BottomEdgeWidth;
            if (_effectiveUseBezierCurve)
            {
                double x = Bezier.GetPointAt(currentTop).Point.X;
                return ((plotAreaWidth / 2.0) - x) * 2.0;
            }
            else
            {
                if (IsInverted)
                {
                    return plotAreaWidth - ((plotAreaWidth - bottomWidth) * ((ViewportHeight - currentTop) / ViewportHeight));
                }
                else
                {
                    return plotAreaWidth - ((plotAreaWidth - bottomWidth) * (currentTop/ViewportHeight));
                }
            }
        }

        /// <summary>
        /// Gets the sorted indexes for the slices.
        /// </summary>
        /// <returns></returns>
        private IntColumn GetSortedIndices()
        {
            IntColumn column = new IntColumn();
            column.Populate(ValueColumn.Values.Count);

            column.Sort(delegate(int i1, int i2)
            {
                if ((double)ValueColumn.Values[i1] < (double)ValueColumn.Values[i2])
                {
                    return IsInverted ? -1 : 1;
                }
                if ((double)ValueColumn.Values[i1] > (double)ValueColumn.Values[i2])
                {
                    return IsInverted ? 1 : -1;
                }
                return 0;
            });

            return column;
        }

        /// <summary>
        /// Sends a frame to be rendered.
        /// </summary>
        /// <param name="frame">The frame to be rendered.</param>
        private void DisplayFrame(FunnelFrame frame)
        {
            SendClearMessage("LeftLabels");
            SendClearMessage("RightLabels");
            SendClearMessage("SliceArea");

            if (frame.OuterAlignedLeft)
            {
                UpdatePanelWidth("LeftPanel", frame.OuterLabelWidth);
                UpdatePanelWidth("RightPanel", 0);
            }
            else
            {
                UpdatePanelWidth("LeftPanel", 0);
                UpdatePanelWidth("RightPanel", frame.OuterLabelWidth);
            }

            foreach (SliceInfo info in frame.Slices)
            {
                if (info.HasSlice)
                {
                    RenderSliceMessage m = new RenderSliceMessage();
                    m.AreaID = "SliceArea";
                    m.Slice = info.Slice;
                    RenderingMessages.SendMessage(m);
                }
            }

            foreach (SliceInfo info in frame.Slices)
            {
                if (info.HasOuterLabel)
                {
                    RenderOuterLabelMessage m = new RenderOuterLabelMessage();
                    if (frame.OuterAlignedLeft)
                    {
                        m.AreaID = "LeftLabels";
                    }
                    else
                    {
                        m.AreaID = "RightLabels";
                    }
                    m.Label = info.OuterLabel;
                    m.Position = info.OuterLabelPosition;
                    m.OuterLabelWidth = frame.OuterLabelWidth;
                    m.SliceInfo = info;
                    RenderingMessages.SendMessage(m);
                }
            }

            FrameRenderCompleteMessage frc = new FrameRenderCompleteMessage();
            RenderingMessages.SendMessage(frc);
        }

        /// <summary>
        /// Sends that a panel should be resized in the view.
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="width"></param>
        private void UpdatePanelWidth(string panelName, double width)
        {
            SetAreaSizeMessage sasm = new SetAreaSizeMessage();
            sasm.AreaID = panelName;
            sasm.SettingWidth = true;
            sasm.Width = width;
            RenderingMessages.SendMessage(sasm);
        }

        /// <summary>
        /// Determines if animation should be performed.
        /// </summary>
        /// <returns></returns>
        private bool ShouldAnimate()
        {
            return TransitionDuration > 0.0;
        }

        /// <summary>
        /// Sends that the view should clear the contents of a content area.
        /// </summary>
        /// <param name="areaID">The content area to clear the contents of.</param>
        private void SendClearMessage(string areaID)
        {
            ClearMessage cm = new ClearMessage();
            cm.AreaID = areaID;

            RenderingMessages.SendMessage(cm);
        }

        /// <summary>
        /// Returns the outer labels for the chart.
        /// </summary>
        /// <returns></returns>
        private ObjectColumn GetOuterLabels()
        {
            return OuterLabelColumn;
        }

        /// <summary>
        /// The data model has been updated.
        /// </summary>
        /// <param name="m"></param>
        private void DataUpdatedMessageReceived(Message m)
        {
            RefreshRequired = true;
        }

        /// <summary>
        /// Called when a property on the model has changed.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void PropertyChangedMessageReceived(Message m)
        {
            PropertyChangedMessage pm = (PropertyChangedMessage)m;
            switch (pm.PropertyName)
            {
                case "ValueColumn":
                    ValueColumn.SetValues(pm.NewValue);
                    RefreshRequired = true;
                    break;
                case "InnerLabelColumn":
                    InnerLabelColumn.SetValues(pm.NewValue);
                    RefreshRequired = true;
                    break;
                case "InnerLabelVisibility":
                    InnerLabelVisibility = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "OuterLabelVisibility":
                    OuterLabelVisibility = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "OuterLabelColumn":
                    OuterLabelColumn.SetValues(pm.NewValue);
                    RefreshRequired = true;
                    break;
                case "ValueMemberPath":
                    ValueMemberPath = pm.NewValue as string;
                    break;
                case "OuterLabelMemberPath":
                    OuterLabelMemberPath = pm.NewValue as string;
                    break;
                case "InnerLabelMemberPath":
                    InnerLabelMemberPath = pm.NewValue as string;
                    break;
                case "BottomEdgeWidth":
                    BottomEdgeWidth = (double)pm.NewValue;
                    if (BottomEdgeWidth > 1.0)
                    {
                        BottomEdgeWidth = 1.0;
                    }
                    if (BottomEdgeWidth < 0.001)
                    {
                        BottomEdgeWidth = 0.001;
                    }
                    RefreshRequired = true;
                    break;
                case "Brushes":
                    Brushes = pm.NewValue as Brush[];
                    RefreshRequired = true;
                    break;
                case "Outlines":
                    Outlines = pm.NewValue as Brush[];
                    RefreshRequired = true;
                    break;
                case "OuterLabelAlignment":
                    OuterLabelAlignment = (OuterLabelAlignment)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "FunnelSliceDisplay":
                    FunnelSliceDisplay = (FunnelSliceDisplay)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "InnerLabelTemplate":
                case "OuterLabelTemplate":
                    TemplateChangedMessage tm = new TemplateChangedMessage();
                    tm.TemplateName = pm.PropertyName;
                    tm.Template = (DataTemplate)pm.NewValue;
                    RenderingMessages.SendMessage(tm);
                    RefreshRequired = true;
                    break;
                case "IsInverted":
                    IsInverted = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "TransitionDuration":
                    DoubleAnimator.IntervalMilliseconds = Convert.ToInt32(pm.NewValue);
                    TransitionDuration = (double)pm.NewValue;
                    break;
                case "AllowSliceSelection":
                    AllowSliceSelection = (bool)pm.NewValue;
                    break;
                case "SelectedSliceStyle":
                    SelectedSliceStyle = (Style)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "UnselectedSliceStyle":
                    UnselectedSliceStyle = (Style)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "UseUnselectedStyle":
                    UseUnselectedStyle = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "UseBezierCurve":
                    UseBezierCurve = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "UpperBezierControlPoint":
                    UpperBezierControlPoint = (Point)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "LowerBezierControlPoint":
                    LowerBezierControlPoint = (Point)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "ItemProvider":
                    ItemProvider = (IItemProvider)pm.NewValue;
                    break;
                case "ToolTip":
                    TooltipValueChangedMessage tvm = new TooltipValueChangedMessage();
                    if (pm.NewValue != null)
                    {
                        HasTooltip = true;
                    }
                    else
                    {
                        HasTooltip = false;
                        ClearTooltipMessage ctm = new ClearTooltipMessage();
                        RenderingMessages.SendMessage(ctm);
                    }
                    tvm.Value = pm.NewValue;
                    RenderingMessages.SendMessage(tvm);
                    break;
                case "Legend":
                    OnLegendChanged(pm.NewValue);
                    RefreshRequired = true;
                    break;
                case "LegendItemTemplate":
                    LegendItemTemplate = (DataTemplate)pm.NewValue;
                    RefreshRequired = true;
                    break;
                case "UseOuterLabelsForLegend":
                    UserOuterLabelsForLegend = (bool)pm.NewValue;
                    RefreshRequired = true;
                    break;





            }
        }



        internal ItemLegend Legend { get; set; }

        private DataTemplate _legendItemTemplate;
        internal DataTemplate LegendItemTemplate
        {
            get { return _legendItemTemplate; }
            set { _legendItemTemplate = value; }
        }

        private void OnLegendChanged(object p)
        {

            if (Legend != null)
            {
                Legend.ClearLegendItems((System.Windows.Controls.Control)ServiceProvider.GetService("Model"));
            }
            Legend = (ItemLegend)p;

        }

        private void RenderLegend(FunnelFrame frame)
        {

            List<UIElement> legendItems = new List<UIElement>();
            foreach (var slice in frame.Slices)
            {
                ContentControl item = new ContentControl();

                string itemLabel = "";
                if (slice.Slice.InnerLabel != null)
                {
                    if (UserOuterLabelsForLegend && slice.OuterLabel != null)
                    {
                        itemLabel = slice.OuterLabel.ToString();
                    }
                    else
                    {
                        itemLabel = slice.Slice.InnerLabel.ToString();
                    }
                }
                Brush itemBrush = null;
                Brush itemOutline = null;
                if (slice.Slice.Fill != null)
                {
                    itemBrush = slice.Slice.Fill;
                }
                if (slice.Slice.Outline != null)
                {
                    itemOutline = slice.Slice.Outline;
                }
                
                object dataItem = null;
                if (ItemProvider != null)
                {
                    dataItem = ItemProvider.GetItem(slice.Slice.Index);
                }

                item.Content = new FunnelSliceDataContext { 
                    Series = (Control)ServiceProvider.GetService("Model"), 
                    Item = dataItem, 
                    ItemBrush = itemBrush, 
                    ItemLabel = itemLabel,
                    ItemOutline = itemOutline
                };

                item.ContentTemplate = LegendItemTemplate;

                legendItems.Add(item);
            }

            if (Legend != null)
            {
                Legend.CreateLegendItems(legendItems, (Control)ServiceProvider.GetService("Model"));
            }

        }

        /// <summary>
        /// Called when a property changes in the view, that we need to know about.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void ViewPropertyChangedMessageReceived(Message m)
        {
            ViewPropertyChangedMessage pm = (ViewPropertyChangedMessage)m;
            switch (pm.PropertyName)
            {
                case "OuterLabelWidthDecider":
                    WidthDecider = pm.NewValue as IOuterLabelWidthDecider;
                    RefreshRequired = true;
                    break;
                case "FunnelLabelSizeDecider":
                    SizeDecider = pm.NewValue as IFunnelLabelSizeDecider;
                    RefreshRequired = true;
                    break;
            }
        }

        /// <summary>
        /// Called when the viewport changed in the view.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void ViewportChangedMessageReceived(
            Message m)
        {
            ViewportChangedMessage vm = (ViewportChangedMessage)m;
            ViewportWidth = vm.NewWidth;
            ViewportHeight = vm.NewHeight;

            RefreshRequired = true;
        }

        /// <summary>
        /// Called when a mouse button interaction occurs.
        /// </summary>
        /// <param name="m"></param>
        private void MouseButtonMessageReceived(
            Message m)
        {
            MouseButtonMessage mbm = (MouseButtonMessage)m;

            if (mbm.Type == MouseButtonType.Right)
            {
                return;
            }





            if (mbm.Action == MouseButtonAction.Down)
            {
                PressedSliceIndex = HoveredSliceIndex;
            }
            else if (mbm.Action == MouseButtonAction.Up)
            {
                int pressed = PressedSliceIndex;
                PressedSliceIndex = -1;
                if (pressed == HoveredSliceIndex)
                {
                    OnSliceClicked(pressed);
                }
            }
        }

        /// <summary>
        /// Called when a slice is clicked.
        /// </summary>
        /// <param name="index"></param>
        private void OnSliceClicked(int index)
        {
            if (index < 0 || index > ValueColumn.Values.Count - 1)
            {
                return;
            }

            if (AllowSliceSelection)
            {
                SliceSelectionManager.ToggleSelection(index, ValueColumn.Values[index]);
                int[] selectedItems = SliceSelectionManager.GetSelectedItems();
                SelectedItemsChangedMessage m = new SelectedItemsChangedMessage();
                m.SelectedItems = selectedItems;
                ModelUpdateMessages.SendMessage(m);
                RefreshRequired = true;
            }

            SliceClickedMessage scm = new SliceClickedMessage();
            scm.Index = index;
            scm.Item = ValueColumn.Values[index];
            ModelUpdateMessages.SendMessage(scm);
        }

        /// <summary>
        /// Called when the mouse moves over the chart.
        /// </summary>
        /// <param name="m"></param>
        private void MouseMoveMessageReceived(
            Message m)
        {
            MouseMoveMessage mm = (MouseMoveMessage)m;

            HoveredSliceIndex = GetHoveredSliceIndex(mm.Position);

            if (HasTooltip)
            {
                UpdateToolTip(mm.Position);
            }
        }

        /// <summary>
        /// Is called when the mouse leaves the controls view.
        /// </summary>
        /// <param name="m"></param>
        private void MouseLeaveMessageReceived(
           Message m)
        {
            //remove any tooltips.
            ClearTooltipMessage ctm = new ClearTooltipMessage();
            RenderingMessages.SendMessage(ctm);
        }

        /// <summary>
        /// Updates the tooltip to have new position and content.
        /// </summary>
        /// <param name="position">The new position for the tooltip.</param>
        private void UpdateToolTip(Point position)
        {
            FunnelDataContext context = new FunnelDataContext();
            if (ItemProvider != null && 
                HoveredSliceIndex >= 0 && 
                HoveredSliceIndex < ItemProvider.Count)
            {
                context.Item = ItemProvider.GetItem(HoveredSliceIndex);
            }
            context.Index = HoveredSliceIndex;

            if (context.Index < 0)
            {
                ClearTooltipMessage cm = new ClearTooltipMessage();
                RenderingMessages.SendMessage(cm);
                return;
            }

            TooltipUpdateMessage tm = new TooltipUpdateMessage();
            tm.Context = context;
            tm.Position = GetTooltipPosition(position, context);



            RenderingMessages.SendMessage(tm);
        }

        /// <summary>
        /// Gets the correct position to display the tooltip.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private Point GetTooltipPosition(Point position, object context)
        {




            Point placement =
                new Point(position.X + 10, position.Y + 10);


            return placement;
        }

        /// <summary>
        /// Gets the index of the slice that is currently being hovered.
        /// </summary>
        /// <param name="point">The mouse position.</param>
        /// <returns>The hovered slice index.</returns>
        private int GetHoveredSliceIndex(Point point)
        {
            if (!IsValid())
            {
                return -1;
            }

            double yVal = point.Y;
            double xVal = point.X;
            FunnelFrame currentFrame = GetCurrentFrame();
            if (currentFrame.OuterAlignedLeft)
            {
                xVal -= currentFrame.OuterLabelWidth;
            }

            SliceInfo slice = GetSliceByYValue(yVal);
            if (slice == null)
            {
                return -1;
            }

            double plotAreaWidth = GetPlotAreaWidth();
            double width = GetWidthAt(plotAreaWidth, yVal);
            double halfWidth = width / 2.0;
            double center =  plotAreaWidth / 2.0;

            if (xVal >= (center - halfWidth) &&
                xVal <= (center + halfWidth))
            {
                return slice.Index;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the width of the plot area.
        /// </summary>
        /// <returns></returns>
        private double GetPlotAreaWidth()
        {
            FunnelFrame current = GetCurrentFrame();
            return ViewportWidth - current.OuterLabelWidth;
        }

        /// <summary>
        /// Gets the slice that appears at the given y value.
        /// </summary>
        /// <param name="yVal">The y value to get the slice for.</param>
        /// <returns>The slice at the given y value.</returns>
        private SliceInfo GetSliceByYValue(double yVal)
        {
            FunnelFrame current = GetCurrentFrame();

            int index = current.Slices.GetByYValue(yVal);
            if (index >= 0 && index < current.Slices.Count)
            {
                return (SliceInfo)current.Slices[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the current displayed frame for mouse interaction purposes.
        /// </summary>
        /// <returns></returns>
        private FunnelFrame GetCurrentFrame()
        {
            FunnelFrame current = null;
            if (AnimationInProgress)
            {
                current = InterpolatedFrame;
            }
            else
            {
                current = CurrentFrame;
            }
            return current;
        }

        /// <summary>
        /// Called when the user indicates which slices should be selected.
        /// </summary>
        /// <param name="m"></param>
        private void UserSelectedItemsChangedMessageReceived(Message m)
        {
            UserSelectedItemsChangedMessage um = (UserSelectedItemsChangedMessage)m;
            SliceSelectionManager.SetSelectedItems(um.SelectedItems, ValueColumn);
            RefreshRequired = true;
        }

        private void LabelSizeChangedMessageReceived(Message m)
        {
            CheckLabelSizes((LabelSizeChangedMessage)m);
        }

        private void CheckLabelSizes(LabelSizeChangedMessage m)
        {
            FunnelFrame current = GetCurrentFrame();
            if (m.SliceIndex == -1)
            {
                RefreshRequired = true;
            }
            else
            {
                SliceInfo si = current.Slices[m.SliceIndex];
                if (m.IsOuter)
                {
                    if (m.NewSize.Width > current.OuterLabelWidth)
                    {
                        RefreshRequired = true;
                    }

                    if (m.NewSize.Height != m.OldSize.Height)
                    {
                        RefreshRequired = true;
                    }

                    if (m.NewSize.Height > si.Slice.LowerRight.Y - si.Slice.UpperRight.Y)
                    {
                        RefreshRequired = true;
                    }
                }
                else
                {
                    if (m.NewSize.Height > si.Slice.LowerRight.Y - si.Slice.UpperRight.Y)
                    {
                        RefreshRequired = true;
                    }
                }
            }

            //if (RefreshRequired)
            //{
            //    RefreshRequired = false;
            //    MakePending();
            //}
        }

//        bool _pending = false;
//        private void MakePending()
//        {
//            if (!_pending)
//            {
//                _pending = true;
//                Dispatcher d = (ServiceProvider.GetService("Model") as FrameworkElement).Dispatcher;
//#if SILVERLIGHT
//                d.BeginInvoke(DoRefresh);
//#else
//                d.BeginInvoke(new ThreadStart(DoRefresh), null);
//#endif
//            }
//        }

//        private void DoRefresh()
//        {
//            _pending = false;
//            RefreshRequired = true;
//            RefreshVisuals();
//        }

    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved