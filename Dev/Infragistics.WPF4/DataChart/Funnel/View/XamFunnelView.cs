using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Messaging;
using Infragistics.Controls.Charts.Util;





namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents the visual portion of a funnel chart.
    /// </summary>
    [TemplatePart(Name = "PART_LEFTOUTERLABELS", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_RIGHTOUTERLABELS", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_LEFTPANEL", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_RIGHTPANEL", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_PLOTAREA", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_SLICEAREA", Type = typeof(Canvas))]



    [TemplatePart(Name = "PART_TOOLTIP", Type = typeof(Popup))]

    [TemplatePart(Name = "PART_TOOLTIPCONTENT", Type = typeof(ContentControl))]
    [EditorBrowsable(EditorBrowsableState.Never)]

    [DesignTimeVisible(false)]

    public class XamFunnelView : Control, IOuterLabelWidthDecider, IFunnelLabelSizeDecider
    {
        /// <summary>
        /// Creates a new instance of the visual portion of a funnel chart.
        /// </summary>
        public XamFunnelView()
        {
            MessageHandler = new MessageHandler();
            MessageHandler.AddHandler(typeof(ClearMessage), ClearMessageReceived);
            MessageHandler.AddHandler(typeof(RenderSliceMessage), RenderSliceMessageReceived);
            MessageHandler.AddHandler(typeof(RenderOuterLabelMessage), RenderOuterLabelMessageReceived);
            MessageHandler.AddHandler(typeof(SetAreaSizeMessage), SetAreaSizeMessageReceived);
            MessageHandler.AddHandler(typeof(TemplateChangedMessage), TemplateChangedMessageReceived);
            MessageHandler.AddHandler(typeof(TooltipUpdateMessage), TooltipUpdateMessageReceived);
            MessageHandler.AddHandler(typeof(ClearTooltipMessage), ClearTooltipMessageReceived);
            MessageHandler.AddHandler(typeof(TooltipValueChangedMessage), TooltipValueChangedMessageReceived);
            MessageHandler.AddHandler(typeof(FrameRenderCompleteMessage), FrameRenderCompleteMessageReceived);

            InteractionMessages = new MessageChannel();
            //this.SizeChanged += XamFunnelView_SizeChanged;

            this.DefaultStyleKey = typeof(XamFunnelView);
        }

        /// <summary>
        /// Called when the mouse is moved over the plot area.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point mousePoint = e.GetPosition(PlotArea);
            MouseMoveMessage m = new MouseMoveMessage();
            m.Position = mousePoint;
            InteractionMessages.SendMessage(m);
        }

        /// <summary>
        /// Called when the left mouse button is depressed over the plot area.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Point mousePoint = e.GetPosition(PlotArea);
            MouseButtonMessage m = new MouseButtonMessage();
            m.Position = mousePoint;
            m.Action = MouseButtonAction.Down;
            m.Type = MouseButtonType.Left;
            InteractionMessages.SendMessage(m);
        }

        /// <summary>
        /// Called when the left mouse button is release over the plot area.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            Point mousePoint = e.GetPosition(PlotArea);
            MouseButtonMessage m = new MouseButtonMessage();
            m.Position = mousePoint;
            m.Action = MouseButtonAction.Up;
            m.Type = MouseButtonType.Left;
            InteractionMessages.SendMessage(m);
        }

        /// <summary>
        /// Called when the mouse leaves the plot area.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            Point mousePoint = e.GetPosition(PlotArea);
            MouseLeaveMessage mlm = new MouseLeaveMessage();
            mlm.Position = mousePoint;
            InteractionMessages.SendMessage(mlm);
        }

        private MessageHandler _messageHandler;

        /// <summary>
        /// Handles messages incoming from the controller.
        /// </summary>
        internal MessageHandler MessageHandler
        {
            get { return _messageHandler; }
            set { _messageHandler = value; }
        }

        private ServiceProvider _serviceProvider;

        /// <summary>
        /// Provides the necessary services to interact with the controller and the model.
        /// </summary>
        internal ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                ServiceProvider oldValue = _serviceProvider;
                _serviceProvider = value;
                OnServiceProviderChanged(oldValue, _serviceProvider);
            }
        }

        /// <summary>
        /// Used to send messages to the controller.
        /// </summary>
        internal MessageChannel InteractionMessages { get; set; }

        /// <summary>
        /// Represents an area to draw labels left of the plot area.
        /// </summary>
        protected Canvas LeftLabels { get; set; }

        /// <summary>
        /// Represents an area to draw labels right of the plot area.
        /// </summary>
        protected Canvas RightLabels { get; set; }

        /// <summary>
        /// Represents the container of the left label area.
        /// </summary>
        protected Panel LeftPanel { get; set; }

        /// <summary>
        /// Represents the container of the right label area.
        /// </summary>
        protected Panel RightPanel { get; set; }

        /// <summary>
        /// Represents the entire plot area for the chart.
        /// </summary>
        protected Panel PlotArea { get; set; }

        /// <summary>
        /// Represents the area in which to render slices.
        /// </summary>
        protected Canvas SliceArea { get; set; }

        /// <summary>
        /// Represents the root of the visual tree of the funnel chart.
        /// </summary>
        protected Grid Root { get; set; }

        /// <summary>
        /// Represents the template to use for inner labels.
        /// </summary>
        protected DataTemplate InnerLabelTemplate { get; set; }

        /// <summary>
        /// Represents the template to use for outer labels.
        /// </summary>
        protected DataTemplate OuterLabelTemplate { get; set; }



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Represents the popup to use as a tooltip.
        /// </summary>
        protected Popup ToolTip { get; set; }


        /// <summary>
        /// Represents the area to display tooltip content.
        /// </summary>
        protected ContentControl ToolTipContent { get; set; }

        /// <summary>
        /// Represents the current value to assign to the tooltip.
        /// </summary>
        protected object TooltipValue { get; set; }

        /// <summary>
        /// Called when the service provider changes.
        /// </summary>
        /// <param name="oldValue">The old service provider.</param>
        /// <param name="newValue">The new service provider.</param>
        private void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                MessageChannel channel =
                    oldValue.GetService("RenderingMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.DetachTarget(MessageReceived);
                }
                InteractionMessages.DetachFromNext();
            }
            if (newValue != null)
            {
                MessageChannel channel =
                    newValue.GetService("RenderingMessages") as MessageChannel;
                if (channel != null)
                {
                    channel.AttachTarget(MessageReceived);
                }

                MessageChannel rendering =
                    newValue.GetService("InteractionMessages") as MessageChannel;
                InteractionMessages.ConnectTo(rendering);
            }
        }

        /// <summary>
        /// Called when a message is received from the controller.
        /// </summary>
        /// <param name="m">The received message.</param>
        private void MessageReceived(Message m)
        {
            MessageHandler.MessageReceived(m);
        }

        /// <summary>
        /// Send the current available viewport to the controller.
        /// </summary>
        private void SendViewport()
        {
            InteractionMessages.SendMessage(
                new ViewportChangedMessage()
                {
                    NewWidth = PlotArea.ActualWidth,
                    NewHeight = PlotArea.ActualHeight
                });
        }

        private ContentControl sizeWatcher = null;

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (PlotArea != null)
            {
                PlotArea.SizeChanged -= PlotArea_SizeChanged;






            }

            if (Root != null)
            {
                if (sizeWatcher != null)
                {
                    sizeWatcher.SizeChanged -= OuterLabel_SizeChanged;
                }
                sizeWatcher = null;
            }

            _slicesByIndex.Clear();

            LeftLabels = GetTemplateChild("PART_LEFTOUTERLABELS") as Canvas;
            RightLabels = GetTemplateChild("PART_RIGHTOUTERLABELS") as Canvas;
            LeftPanel = GetTemplateChild("PART_LEFTPANEL") as Panel;
            RightPanel = GetTemplateChild("PART_RIGHTPANEL") as Panel;
            PlotArea = GetTemplateChild("PART_PLOTAREA") as Panel;
            SliceArea = GetTemplateChild("PART_SLICEAREA") as Canvas;
            Root = GetTemplateChild("PART_ROOT") as Grid;

            if (PlotArea != null)
            {
                PlotArea.SizeChanged += PlotArea_SizeChanged;






            }

            if (Root != null)
            {
                sizeWatcher = new ContentControl()
                {
                    Content = "...",
                    Foreground = new SolidColorBrush(Colors.Transparent),
                    IsHitTestVisible = false,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top
                };
                sizeWatcher.SizeChanged += OuterLabel_SizeChanged;

                Root.Children.Add(sizeWatcher);
            }



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

            ToolTip = GetTemplateChild("PART_TOOLTIP") as Popup;



            if (ToolTip != null)
            {
                if (SafeSetter.IsSafe)
                {
                    ToolTip.AllowsTransparency = true;
                }
                ToolTip.PlacementTarget = PlotArea;
                ToolTip.Placement = PlacementMode.Relative;
            }

            ToolTipContent = GetTemplateChild("PART_TOOLTIPCONTENT") as ContentControl;

            SendViewport();
            SendWidthDecider();
            SendSizeDecider();
        }



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Called when the render size of the plot area changes, necessitating notification of the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlotArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlotArea.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, PlotArea.ActualWidth, PlotArea.ActualHeight)
            };
            SendViewport();
        }

        /// <summary>
        /// Send our implementation fo teh width decider to the controller.
        /// </summary>
        private void SendWidthDecider()
        {
            ViewPropertyChangedMessage m = new ViewPropertyChangedMessage();
            m.PropertyName = "OuterLabelWidthDecider";
            m.OldValue = null;
            m.NewValue = this as IOuterLabelWidthDecider;
            InteractionMessages.SendMessage(m);
        }

        private void SendSizeDecider()
        {
            ViewPropertyChangedMessage m = new ViewPropertyChangedMessage();
            m.PropertyName = "FunnelLabelSizeDecider";
            m.OldValue = null;
            m.NewValue = this as IFunnelLabelSizeDecider;
            InteractionMessages.SendMessage(m);
        }

        /// <summary>
        /// Determine the amount of space needed to render the provided labels.
        /// </summary>
        /// <param name="labels">The labels to check.</param>
        /// <returns>The width required.</returns>
        public double DecideWidth(ObjectColumn labels)
        {
            if (labels == null || labels.Values == null)
            {
                return 0.0;
            }
            double largestWidth = double.MinValue;
            LeftLabels.Children.Clear();
            foreach (var label in labels.Values)
            {
                ContentPresenter cont = new ContentPresenter()
                {
                    ContentTemplate = OuterLabelTemplate,
                    Content = label
                };
                LeftLabels.Children.Add(cont);
                cont.InvalidateMeasure();
                cont.InvalidateArrange();
                cont.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                largestWidth = Math.Max(largestWidth, cont.DesiredSize.Width);
            }
            LeftLabels.Children.Clear();

            return largestWidth;
        }

        Size IFunnelLabelSizeDecider.DecideLabelSize(SliceInfo sliceInfo, bool inner)
        {
            if (sliceInfo == null)
            {
                return new Size(0.0, 0.0);
            }
            if (inner && sliceInfo.Slice.InnerLabel == null)
            {
                return new Size(0.0, 0.0);
            }
            if (!inner && sliceInfo.OuterLabel == null)
            {
                return new Size(0.0, 0.0);
            }

            DataTemplate template = OuterLabelTemplate;
            if (inner)
            {
                template = InnerLabelTemplate;
            }

            ContentControl cont = new ContentControl()
            {
                ContentTemplate = template,
                Content = inner ? sliceInfo.Slice.InnerLabel : sliceInfo.OuterLabel
            };

            this.PlotArea.Children.Add(cont);
            cont.InvalidateMeasure();
            cont.InvalidateArrange();
            cont.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var height = cont.DesiredSize.Height;
            var width = cont.DesiredSize.Width;
            this.PlotArea.Children.Remove(cont);
            return new Size(width, height);
        }

        /// <summary>
        /// Called when the controller indicates that an area should be cleared of content.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void ClearMessageReceived(Message m)
        {
            ClearMessage cm = (ClearMessage)m;
            Panel panel = GetTarget(cm) as Panel;
            if (panel == null)
            {
                return;
            }
            CleanupSlices(panel);
            CleanupLabels(panel);
            RemoveSlices(panel);
            RemoveLabels(panel);
        }

        private void RemoveLabels(Panel panel)
        {
            if (panel == LeftLabels || panel == RightLabels)
            {
                foreach (var child in panel.Children.OfType<FrameworkElement>())
                {
                    child.SizeChanged -= OuterLabel_SizeChanged;
                }
                panel.Children.Clear();
            }
        }

        private bool _mustClearUnused = true;

        private void RemoveSlices(Panel panel)
        {
            _mustClearUnused = true;
            foreach (var slice in SliceArea.Children.OfType<XamFunnelSlice>())
            {
                SetSliceUsed(slice, false);
            }
        }

        /// <summary>
        /// Clean up any resources used by the labels.
        /// </summary>
        /// <param name="panel"></param>
        private void CleanupLabels(Panel panel)
        {
        }

        /// <summary>
        /// Clean up any resources used by the slices.
        /// </summary>
        /// <param name="panel"></param>
        private void CleanupSlices(Panel panel)
        {
        }

        /// <summary>
        /// Determine the target of targettable rendering messages.
        /// </summary>
        /// <param name="m">The message received.</param>
        /// <returns>The target of the message.</returns>
        private FrameworkElement GetTarget(RenderingMessage m)
        {
            switch (m.AreaID)
            {
                case "RightPanel":
                    return RightPanel;
                case "LeftPanel":
                    return LeftPanel;
                case "LeftLabels":
                    return LeftLabels;
                case "RightLabels":
                    return RightLabels;
                case "SliceArea":
                    return SliceArea;
                case "Plot":
                    return PlotArea;
            }

            return null;
        }

        /// <summary>
        /// Called when the controller has indicated that we should change the size of one of our areas.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void SetAreaSizeMessageReceived(Message m)
        {
            SetAreaSizeMessage am = (SetAreaSizeMessage)m;
            Panel panel = GetTarget(am) as Panel;
            if (panel == null)
            {
                return;
            }
            bool changedSize = false;
            if (am.SettingHeight && panel.Height != am.Height)
            {
                panel.Height = am.Height;
                changedSize = true;
            }
            if (am.SettingWidth && panel.Width != am.Width)
            {
                panel.Width = am.Width;
                changedSize = true;
            }
            if (changedSize)
            {
                panel.UpdateLayout();
            }
        }

        /// <summary>
        /// Called when the controller has indicated we should render a funnel slice.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void RenderSliceMessageReceived(Message m)
        {
            RenderSliceMessage sm = (RenderSliceMessage)m;
            Canvas panel = GetTarget(sm) as Canvas;
            if (panel == null)
            {
                return;
            }

            if (!sm.Slice.Offset.X.IsPlottable() ||
                !sm.Slice.Offset.Y.IsPlottable())
            {
                return;
            }

            XamFunnelSlice slice = GetSlice(sm.Slice.Index, panel);
            sm.Slice.InnerLabelTemplate = InnerLabelTemplate;
            slice.Style = sm.Slice.Style;
            slice.DataContext = null;
            slice.DataContext = sm.Slice;
            slice.ContentTemplate = InnerLabelTemplate;
            slice.Content = sm.Slice.InnerLabel;
            slice.LabelVisibility = sm.Slice.HasInnerLabel ? Visibility.Visible : Visibility.Collapsed;

            Canvas.SetLeft(slice, sm.Slice.Offset.X);
            Canvas.SetTop(slice, sm.Slice.Offset.Y);
            SetSliceUsed(slice, true);

            if (!panel.Children.Contains(slice))
            {
                panel.Children.Add(slice);
            }
        }

        private XamFunnelSlice GetSlice(int index, Canvas panel)
        {
            XamFunnelSlice slice = null;
            if (!_slicesByIndex.TryGetValue(index, out slice))
            {
                slice = new XamFunnelSlice();
                _slicesByIndex.Add(index, slice);
            }
            slice.Owner = this;
            return slice;
        }

        private Dictionary<int, XamFunnelSlice> _slicesByIndex = new Dictionary<int, XamFunnelSlice>();

        private void FrameRenderCompleteMessageReceived(Message m)
        {
            CleanupUnused();
        }

        private void CleanupUnused()
        {
            if (!_mustClearUnused || SliceArea == null)
            {
                return;
            }

            _mustClearUnused = false;
            var toRemove = new List<XamFunnelSlice>();
            foreach (var slice in SliceArea.Children.OfType<XamFunnelSlice>())
            {
                if (!GetSliceUsed(slice))
                {
                    toRemove.Add(slice);
                }
            }

            foreach (var slice in toRemove)
            {
                SliceAppearance sa = (SliceAppearance)slice.DataContext;
                _slicesByIndex.Remove(sa.Index);
                SliceArea.Children.Remove(slice);
                slice.Owner = null;
            }
        }

        /// <summary>
        /// Called when the controller has indicated that we should render an outer label.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void RenderOuterLabelMessageReceived(Message m)
        {
            RenderOuterLabelMessage olm = (RenderOuterLabelMessage)m;
            Canvas panel = GetTarget(olm) as Canvas;
            if (panel == null)
            {
                return;
            }

            ContentPresenter control = new ContentPresenter()
            {
                ContentTemplate = OuterLabelTemplate,
                Content = olm.Label
            };
            control.Width = olm.OuterLabelWidth;
            panel.Children.Add(control);
            control.InvalidateMeasure();
            control.InvalidateArrange();
            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double desiredWidth = control.DesiredSize.Width;
            double desiredHeight = control.DesiredSize.Height;
            Canvas.SetTop(control, olm.Position.Y - (desiredHeight / 2.0));
            Canvas.SetLeft(control, 0);
            control.Tag = olm.SliceInfo;
            //control.SizeChanged += OuterLabel_SizeChanged;
        }

        void OuterLabel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            SliceInfo info = null;
            int index = -1;
            if (fe != null)
            {
                info = fe.Tag as SliceInfo;
                if (info != null)
                {
                    index = info.Index;
                }
            }
            if (e.PreviousSize.Width > 0 && e.PreviousSize.Height > 0 &&
                    (e.NewSize.Width != e.PreviousSize.Width ||
                    e.NewSize.Height != e.NewSize.Height))
            {
                LabelSizeChanged(index, e.NewSize, e.PreviousSize, true);
            }
        }

        /// <summary>
        /// Called when the controller has indicated that we need to update one of our templates.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void TemplateChangedMessageReceived(Message m)
        {
            TemplateChangedMessage tm = (TemplateChangedMessage)m;
            switch (tm.TemplateName)
            {
                case "InnerLabelTemplate":
                    InnerLabelTemplate = tm.Template;
                    break;
                case "OuterLabelTemplate":
                    OuterLabelTemplate = tm.Template;
                    break;
            }
        }

        /// <summary>
        /// Called when the controller has indicated that the tooltip value has changed.
        /// </summary>
        /// <param name="m">The message received.</param>
        private void TooltipValueChangedMessageReceived(Message m)
        {
            TooltipValueChangedMessage tm = (TooltipValueChangedMessage)m;
            TooltipValue = tm.Value;
        }

        /// <summary>
        /// Called when the controller has indicated that the positioning of the tooltip has changed.
        /// </summary>
        /// <param name="m"></param>
        private void TooltipUpdateMessageReceived(Message m)
        {
            TooltipUpdateMessage tum = (TooltipUpdateMessage)m;
            if (ToolTipContent != null)
            {
                ToolTipContent.DataContext = tum.Context;
                var fe = TooltipValue as FrameworkElement;

                if (fe != null)
                {
                    var panel = fe.Parent as Panel;
                    if (panel != null)
                    {
                        panel.Children.Remove(fe);
                    }
                    var cont = fe.Parent as ContentControl;
                    if (cont != null)
                    {
                        cont.Content = null;
                    }
                }

                if (ToolTipContent.Content != TooltipValue)
                {
                    ToolTipContent.Content = TooltipValue;
                }
            }



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            if (ToolTip != null)
            {
                ToolTip.HorizontalOffset = tum.Position.X;
                ToolTip.VerticalOffset = tum.Position.Y;
                if (!ToolTip.IsOpen)
                {
                    ToolTip.IsOpen = true;
                }
            }

        }

        /// <summary>
        /// Called when teh controller has indicated taht the tooltip should be removed from display.
        /// </summary>
        /// <param name="m"></param>
        private void ClearTooltipMessageReceived(Message m)
        {






            if (ToolTip != null)
            {
                ToolTip.IsOpen = false;
            }

        }

        /// <summary>
        /// Identifies the SliceUsed attached dependency property.
        /// </summary>
        public static readonly DependencyProperty SliceUsedProperty = DependencyProperty.RegisterAttached(
            "SliceUsed",
            typeof(bool),
            typeof(XamFunnelView),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets whether a slice is in use by the funnel view.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetSliceUsed(DependencyObject target)
        {
            return (bool)target.GetValue(SliceUsedProperty);
        }

        /// <summary>
        /// Sets whether a slice is in use by the funnel view.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetSliceUsed(DependencyObject target, bool value)
        {
            target.SetValue(SliceUsedProperty, value);
        }

        internal void LabelSizeChanged(int sliceIndex, Size newSize, Size oldSize, bool isOuter)
        {
            LabelSizeChangedMessage lscm = new LabelSizeChangedMessage();
            lscm.SliceIndex = sliceIndex;
            lscm.NewSize = newSize;
            lscm.OldSize = oldSize;
            lscm.IsOuter = isOuter;
            InteractionMessages.SendMessage(lscm);
        }
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