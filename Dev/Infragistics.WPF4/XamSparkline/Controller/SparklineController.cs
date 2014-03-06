using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Charts.Messaging;
using System.Collections;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    internal class SparklineController : DependencyObject, IFastItemsSourceProvider
    {
        internal SparklineController(ServiceProvider provider)
        {
            ConfigurationMessages = (MessageChannel)provider.GetService("ConfigurationMessages");
            RenderingMessages = (MessageChannel)provider.GetService("RenderingMessages");
            InteractionMessages = (MessageChannel)provider.GetService("InteractionMessages");

            FastItemsSource_Event = (o, e) =>
            {
                DataUpdatedOverride(e.Action, e.Position, e.Count, e.PropertyName);
            };            
            
            Model = (XamSparkline)provider.GetService("Model");
            View = (XamSparklineView)provider.GetService("View");

            ConfigurationMessages.AttachTarget(ConfigurationMessageReceived);
            InteractionMessages.AttachTarget(InteractionMessageReceived);
        }

        #region Properties
        private MessageChannel _configurationMessages;
        internal MessageChannel ConfigurationMessages
        {
            get { return _configurationMessages; }
            set { _configurationMessages = value; }
        }

        private MessageChannel _renderingMessages;
        internal MessageChannel RenderingMessages
        {
            get { return _renderingMessages; }
            set { _renderingMessages = value; }
        }

        private MessageChannel _interactionMessages;
        internal MessageChannel InteractionMessages
        {
            get { return _interactionMessages; }
            set { _interactionMessages = value; }
        }

        private XamSparkline _model;
        internal XamSparkline Model
        {
            get { return _model; }
            set
            {
                bool changed = this._model != value;
                if (changed)
                {
                    _model = value;
                    this.UpdateInternalData();
                }
            }
        }

        private XamSparklineView _view;
        internal XamSparklineView View
        {
            get { return _view; }
            set { _view = value; }
        }

        internal IEnumerable ItemsSource
        {
            get 
            {
                return this.Model != null ? this.Model.ItemsSource : null;
            }
        }

        internal static readonly DependencyProperty FastItemsSourceProperty = DependencyProperty.Register("FastItemsSource", typeof(FastItemsSource), typeof(SparklineController),
            new PropertyMetadata((sender, e) =>
            {
                (sender as SparklineController).UpdateFastItemsSource(e.OldValue, e.NewValue);
            }));

        internal FastItemsSource FastItemsSource
        {
            get { return (FastItemsSource)GetValue(FastItemsSourceProperty); }
            set { SetValue(FastItemsSourceProperty, value); }
        }

        private IFastItemColumn<double> _valueColumn = null;
        internal IFastItemColumn<double> ValueColumn
        {
            get { return _valueColumn; }
            set { _valueColumn = value; }
        }

        private IFastItemColumn<object> _labelColumn = null;
        internal IFastItemColumn<object> LabelColumn
        {
            get { return _labelColumn; }
            set { _labelColumn = value; }
        }

        private SparkFramePreparer _framePreparer = new SparkFramePreparer();
        internal SparkFramePreparer FramePreparer
        {
            get { return _framePreparer; }
            set { _framePreparer = value; }
        }

        private SparkFrame _currentFrame = new SparkFrame();
        internal SparkFrame CurrentFrame
        {
            get { return _currentFrame; }
            set { _currentFrame = value; }
        }

        internal string ValueMemberPath
        {
            get 
            {
                return this.Model != null ? this.Model.ValueMemberPath : null;
            }
        }

        internal string LabelMemberPath
        {
            get 
            {
                return this.Model != null ? this.Model.LabelMemberPath : null;
            }
        }

        internal double ViewportWidth { get; private set; }
        internal double ViewportHeight { get; private set; }

        protected object ToolTip
        {
            get 
            {
                return this.Model != null ? this.Model.ToolTip : null;
            }
        }
        #endregion

        internal EventHandler<FastItemsSourceEventArgs> FastItemsSource_Event;

        internal double GetScaledXValue(double unscaledValue)
        {
            int itemCount = ValueColumn.Count;
            double width = ViewportWidth;

            if (Model.DisplayType == SparklineDisplayType.Area
                || Model.DisplayType == SparklineDisplayType.Line)
            {
                itemCount--;
            }

            if (itemCount < 0)
                itemCount = 0;

            double scaledValue =
                itemCount > 0 ? unscaledValue / itemCount
                : itemCount == 0 ? 0.5f
                : double.NaN;

            scaledValue = scaledValue * width;

            return scaledValue;
        }

        internal double GetScaledYValue(double unscaledValue)
        {
           return ViewportHeight - (((unscaledValue - Model.ActualMinimum) / (Model.ActualMaximum - Model.ActualMinimum)) * ViewportHeight);
        }

        protected virtual bool Valid()
        {
            return
                this.ItemsSource != null &&
                this.ValueMemberPath != null &&
                this.ViewportWidth > 0.0 &&
                this.ViewportHeight > 0.0 &&
                this.Model != null &&
                this.Model.ActualMinimum != this.Model.ActualMaximum;
        }

        private bool RequiresRefresh(PropertyChangedMessage propertyChangedMessage)
        {
            if (propertyChangedMessage.OldValue != propertyChangedMessage.NewValue)
            {
                return true;
            }

            return false;
        }

        private void UpdateFastItemsSource(object oldValue, object newValue)
        {
            if (FastItemsSource_Event == null)
            {
                FastItemsSource_Event = (o, args) =>
                {
                    DataUpdatedOverride(args.Action, args.Position, args.Count, args.PropertyName);
                };
            }

            FastItemsSource oldFastItemsSource = oldValue as FastItemsSource;
            if (oldFastItemsSource != null)
            {
                oldFastItemsSource.Event -= FastItemsSource_Event;
            }

            FastItemsSource newFastItemsSource = newValue as FastItemsSource;
            if (newFastItemsSource != null)
            {
                newFastItemsSource.Event += FastItemsSource_Event;
            }
        }

        private void UpdateMinMax()
        {
            if (Model != null)
            {
                if (this.ValueColumn == null)
                {
                    this.Model.ActualMinimum = this.Model.ActualMaximum = double.NaN;
                }
                else
                {
                    if (double.IsNaN(this.Model.Minimum))
                    {
                        Model.ActualMinimum = ValueColumn.Minimum;
                    }
                    else
                    {
                        Model.ActualMinimum = Model.Minimum;
                    }

                    if (double.IsNaN(this.Model.Maximum))
                    {
                        Model.ActualMaximum = ValueColumn.Maximum;
                    }
                    else
                    {
                        Model.ActualMaximum = Model.Maximum;
                    }
                }
            }
        }

        private void UpdateValueColumn()
        {
            if (this.FastItemsSource != null)
            {
                FastItemsSource.DeregisterColumn(ValueColumn);
                ValueColumn = FastItemsSource.RegisterColumn(ValueMemberPath);
            }
            else
            {
                this.ValueColumn = null;
            }
        }

        private void UpdateLabelColumn()
        {
            if (this.FastItemsSource != null)
            {
                FastItemsSource.DeregisterColumn(LabelColumn);
                LabelColumn = FastItemsSource.RegisterColumnObject(LabelMemberPath);
                Model.LabelColumn = LabelColumn;
            }
            else
            {
                this.Model.LabelColumn = this.LabelColumn = null;
            }
        }

        private void UpdateAxes()
        {
            AxisRenderMessage message = new AxisRenderMessage();
            RenderingMessages.SendMessage(message);
        }

        private void UpdateInternalData()
        {
            FastItemsSource = GetFastItemsSource(ItemsSource);

            UpdateValueColumn();
            UpdateLabelColumn();
            UpdateMinMax();
            UpdateAxes();
        }

        internal void DataUpdatedOverride(FastItemsSourceEventAction action, int position, int count, string propertyName)
        {
            UpdateValueColumn();
            UpdateLabelColumn();
            UpdateMinMax();
            UpdateAxes();

            Refresh();
        }

        private void UpdateToolTip()
        {
            ToolTipTemplateMessage m = new ToolTipTemplateMessage();
            m.Template = ToolTip;
            RenderingMessages.SendMessage(m);
        }

        internal virtual void ConfigurationMessageReceived(Message m)
        {
            ConfigurationMessage cm = (ConfigurationMessage)m;
            if (cm is PropertyChangedMessage)
            {
                PropertyChangedMessageReceived(cm as PropertyChangedMessage);
            }
        }

        internal virtual void InteractionMessageReceived(Message m)
        {
            InteractionMessage im = (InteractionMessage)m;

            if (im is ViewportChangedMessage)
            {
                ViewportChangedMessageReceived(im as ViewportChangedMessage);
            }
            else if (im is MouseLeaveMessage)
            {
                MouseLeaveMessageReceived(im as MouseLeaveMessage);
            }
            else if (im is MouseMoveMessage)
            {
                MouseMoveMessageReceived(im as MouseMoveMessage);
            }
        }

        internal virtual void PropertyChangedMessageReceived(PropertyChangedMessage propertyChangedMessage)
        {
            switch (propertyChangedMessage.PropertyName)
            {
                case XamSparkline.ItemsSourcePropertyName:
                    //ItemsSource = (IEnumerable)propertyChangedMessage.NewValue;
                    UpdateInternalData();
                    break;
                case XamSparkline.ValueMemberPathPropertyName:
                    //ValueMemberPath = (string)propertyChangedMessage.NewValue;
                    UpdateInternalData();
                    break;
                case XamSparkline.LabelMemberPathPropertyName:
                    //LabelMemberPath = (string)propertyChangedMessage.NewValue;
                    UpdateInternalData();
                    break;
                case XamSparkline.ToolTipPropertyName:
                    //ToolTip = propertyChangedMessage.NewValue;
                    UpdateToolTip();
                    break;
                case XamSparkline.MinimumPropertyName:
                case XamSparkline.MaximumPropertyName:
                    this.UpdateMinMax();
                    this.UpdateAxes();
                    break;
                case XamSparkline.HorizontalAxisLabelPropertyName:
                case XamSparkline.VerticalAxisLabelPropertyName:
                    this.UpdateAxes();
                    break;
            }

            if (RequiresRefresh(propertyChangedMessage))
            {
                Refresh();
            }
        }

        internal virtual void ViewportChangedMessageReceived(ViewportChangedMessage viewportChangedMessage)
        {
            ViewportWidth = viewportChangedMessage.NewWidth;
            ViewportHeight = viewportChangedMessage.NewHeight;

            Refresh();
        }

        private void MouseMoveMessageReceived(MouseMoveMessage mouseMoveMessage)
        {
            ClearMessage cm = new ClearMessage();
            cm.Layer = SparkLayerType.ToolTipLayer;
            RenderingMessages.SendMessage(cm);

            ToolTipMessage tm = new ToolTipMessage();
            tm.Layer = SparkLayerType.ToolTipLayer;
            tm.XOffset = mouseMoveMessage.Position.X + 10;
            tm.YOffset = mouseMoveMessage.Position.Y + 10;
            if (this.ValueColumn != null && this.ValueColumn.Count > 0)
            {
                tm.Context = new SparklineToolTipContext
                {
                    High = ValueColumn.Maximum,
                    Low = ValueColumn.Minimum,
                    First = ValueColumn[0],
                    Last = ValueColumn[ValueColumn.Count - 1]
                };
            }
            RenderingMessages.SendMessage(tm);
        }

        private void MouseLeaveMessageReceived(MouseLeaveMessage mouseLeaveMessage)
        {
            ClearMessage cm = new ClearMessage();
            cm.Layer = SparkLayerType.ToolTipLayer;
            
            RenderingMessages.SendMessage(cm);
        }

        private void ClearLayer(SparkLayerType layerType)
        {
            ClearMessage cm = new ClearMessage();
            cm.Layer = layerType;
            this.RenderingMessages.SendMessage(cm);
        }
        private void ClearData()
        {
            this.ClearLayer(SparkLayerType.SparkLayer);
            this.ClearLayer(SparkLayerType.MarkerLayer);
            this.ClearLayer(SparkLayerType.TrendLayer);
            this.ClearLayer(SparkLayerType.RangeLayer);
        }
        protected virtual void Refresh()
        {
            this.ClearData();
            
            if (!Valid())
            {
                return;
            }

            FramePreparer.Controller = this;
            FramePreparer.PrepareFrame(CurrentFrame);

            switch (Model.DisplayType)
            {
                case SparklineDisplayType.Column:
                    RefreshColumns();
                    break;

                case SparklineDisplayType.WinLoss:
                    RefreshWinLoss();
                    break;

                case SparklineDisplayType.Line:
                case SparklineDisplayType.Area:
                    RefreshPolygon();
                    break;
            }

            NormalRangeMessage rangeMessage = new NormalRangeMessage
            {
                X = 0,
                Y = GetScaledYValue(Model.NormalRangeMaximum),
                Width = ViewportWidth,
                Height = Math.Abs(GetScaledYValue(Model.NormalRangeMaximum) - GetScaledYValue(Model.NormalRangeMinimum))
            };

            RenderingMessages.SendMessage(rangeMessage);

            TrendLineMessage trendMessage = new TrendLineMessage
            {
                Points = CurrentFrame.TrendPoints.ToArray()
            };

            RenderingMessages.SendMessage(trendMessage);
        }

        private void RefreshColumns()
        {
            ColumnMessage columnMessage = new ColumnMessage();
            columnMessage.Columns = CurrentFrame.Markers;
            columnMessage.NegativeColumns = CurrentFrame.NegativeMarkers;
            columnMessage.LowColumns = CurrentFrame.LowPoints;
            columnMessage.HighColumns = CurrentFrame.HighPoints;
            columnMessage.FirstColumn = CurrentFrame.FirstPoint;
            columnMessage.LastColumn = CurrentFrame.LastPoint;

            columnMessage.BucketCount = CurrentFrame.Buckets.Count;
            columnMessage.Crossing = FramePreparer.Crossing;
            columnMessage.Offset = FramePreparer.Offset;
            columnMessage.DisplayType = Model.DisplayType;

            RenderingMessages.SendMessage(columnMessage);

            RefreshMarkers();
        }

        private void RefreshWinLoss()
        {
            WinLossColumnMessage message = new WinLossColumnMessage();
            message.Columns = CurrentFrame.Markers;
            message.NegativeColumns = CurrentFrame.NegativeMarkers;

            message.LowColumns = CurrentFrame.LowPoints;
            message.HighColumns = CurrentFrame.HighPoints;
            message.FirstColumn = CurrentFrame.FirstPoint;
            message.LastColumn = CurrentFrame.LastPoint;

            message.BucketCount = CurrentFrame.Buckets.Count;
            message.Crossing = FramePreparer.Crossing;
            message.Offset = FramePreparer.Offset;
            message.DisplayType = Model.DisplayType;

            RenderingMessages.SendMessage(message);
        }

        private void RefreshPolygon()
        {
            if (CurrentFrame.Buckets.Count == 0)
            {
                return;
            }

            //break up into sequences of valid buckets.
            //each segment is a list of consecutive non-null buckets.
            List<List<double[]>> segments = new List<List<double[]>>();
            List<double[]> segment = new List<double[]>();

            bool lastBucketIsNull = true;

            foreach (var bucket in CurrentFrame.Buckets)
            {
                if (double.IsNaN(bucket[1]) || double.IsInfinity(bucket[1]))
                {
                    //this bucket is invalid.
                    if (!lastBucketIsNull)
                    {
                        //there existed a previous sequence of valid buckets.
                        //we just reached the first null bucket after a valid sequence.
                        //add the sequence to the segments.
                        segments.Add(segment);
                    }

                    //skip the null bucket.
                    lastBucketIsNull = true;
                    continue;
                }

                if (lastBucketIsNull)
                {
                    //this is the first non-null bucket, start a new segment.
                    segment = new List<double[]>();
                    lastBucketIsNull = false;
                }

                segment.Add(bucket);
            }

            if (!lastBucketIsNull)
            {
                //only add the segment if the last bucket is valid, otherwise this has already been done.
                segments.Add(segment);
            }

            PolygonMessage polygonMessage = new PolygonMessage();
            polygonMessage.Points = new List<Point[]>();

            if (Model.UnknownValuePlotting == UnknownValuePlotting.LinearInterpolate)
            {
                List<double[]> InterpolatedBuckets = new List<double[]>();
                foreach (var validSegment in segments)
                {
                    InterpolatedBuckets.AddRange(validSegment);
                }

                Point[] pts = CreatePolygonSegment(InterpolatedBuckets);
                polygonMessage.Points.Add(pts);
            }
            else
            {
                //iterate through the segments and create a polygon
                foreach (var validSegment in segments)
                {
                    Point[] pts = CreatePolygonSegment(validSegment);
                    polygonMessage.Points.Add(pts);
                }
            }

            RenderingMessages.SendMessage(polygonMessage);
            RefreshMarkers();
        }

        private Point[] CreatePolygonSegment(List<double[]> buckets)
        {
            List<Point> points = new List<Point>();

            foreach (var bucket in buckets)
            {
                points.Add(new Point(bucket[0], bucket[1]));
            }

            //for the line, create a list of bottom points of the polygon
            if (Model.DisplayType == SparklineDisplayType.Line)
            {
                List<Point> points2 = new List<Point>();

                foreach (var bucket in buckets)
                {
                    points2.Add(new Point(bucket[0], bucket[2]));
                }

                points2.Reverse();
                points.AddRange(points2);
            }

            //for the area, add first and last points at the Y crossing
            if (Model.DisplayType == SparklineDisplayType.Area)
            {
                Point firstPoint = points[0];
                Point lastPoint = points[points.Count - 1];

                points.Add(new Point(lastPoint.X, FramePreparer.Crossing));
                points.Add(new Point(firstPoint.X, FramePreparer.Crossing));
            }

            return points.ToArray();
        }

        private void RefreshMarkers()
        {
            MarkerMessage markerMessage = new MarkerMessage();

            markerMessage.MarkerPoints = CurrentFrame.Markers;
            markerMessage.NegativeMarkerPoints = CurrentFrame.NegativeMarkers;
            markerMessage.LowPoints = CurrentFrame.LowPoints;
            markerMessage.HighPoints = CurrentFrame.HighPoints;
            markerMessage.FirstPoint = CurrentFrame.FirstPoint;
            markerMessage.LastPoint = CurrentFrame.LastPoint;

            markerMessage.MarkerSize = Model.MarkerSize;
            markerMessage.FirstMarkerSize = Model.FirstMarkerSize;
            markerMessage.LastMarkerSize = Model.LastMarkerSize;
            markerMessage.HighMarkerSize = Model.HighMarkerSize;
            markerMessage.LowMarkerSize = Model.LowMarkerSize;
            markerMessage.NegativeMarkerSize = Model.NegativeMarkerSize;

            RenderingMessages.SendMessage(markerMessage);
        }

        #region IFastItemsSource implementation
        public FastItemsSource GetFastItemsSource(IEnumerable target)
        {
            FastItemsSource fastItemsSource;

            if (ItemsSource != null)
            {
                FastItemsSourceReference itemsSourceReference = null;
                fastItemsSource = new FastItemsSource() { ItemsSource = ItemsSource };
                itemsSourceReference = new FastItemsSourceReference(fastItemsSource);

                itemsSourceReference.References++;
                fastItemsSource = itemsSourceReference.FastItemsSource;
            }
            else
            {
                fastItemsSource = null;
            }

            return fastItemsSource;
        }

        public FastItemsSource ReleaseFastItemsSource(IEnumerable itemsSource)
        {
            if (itemsSource != null)
            {
                FastItemsSourceReference itemsSourceReference = null;
                --itemsSourceReference.References;
            }

            return null;
        }
        #endregion
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