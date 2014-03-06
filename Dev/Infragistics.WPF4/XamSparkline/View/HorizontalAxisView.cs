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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// View class for a horizontal axis in XamSparkline.
    /// </summary>
    public class HorizontalAxisView : Control
    {
        /// <summary>
        /// HorizontalAxisView constructor.
        /// </summary>
        public HorizontalAxisView()
        {
            DefaultStyleKey = typeof(HorizontalAxisView);

            MessageHandlers[typeof(AxisRenderMessage)] =
              new MessageEventHandler((m) => RenderAxisMessageReceived(m as AxisRenderMessage));

            SizeChanged += (o, e) =>
                {
                    if (e.NewSize == e.PreviousSize)
                        return;

                    RenderLabels();
                };

            Loaded += (o, e) =>
            {
                DependencyObject parent = this;
                while (!(parent is XamSparkline) && parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent == null)
                {
                    return;
                }

                Sparkline = parent as XamSparkline;
                Sparkline.HorizontalAxis = this;

                RenderLabels();
            };
            this.InitializeContent();
        }
        private void InitializeContent()
        {
            this.Content = new Grid();
            this.LabelContent = new Grid();
            this.Content.Children.Add(this.LabelContent);
        }
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ContentArea = (ContentPresenter)GetTemplateChild("HorizontalAxisContentArea");
            if (this.ContentArea != null)
            {
                this.ContentArea.Content = this.Content;
            }
        }

        ContentPresenter ContentArea { get; set; }
        XamSparkline Sparkline { get; set; }

        private ServiceProvider _serviceProvider;
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

        private Dictionary<Type, MessageEventHandler> _messageHandlers = new Dictionary<Type, MessageEventHandler>();
        internal Dictionary<Type, MessageEventHandler> MessageHandlers
        {
            get { return _messageHandlers; }
            set { _messageHandlers = value; }
        }

        private MessageChannel _renderingMessages;
        internal MessageChannel RenderingMessages
        {
            get { return _renderingMessages; }
            set { _renderingMessages = value; }
        }

        Grid _labelContent;
        Grid LabelContent
        {
            get { return _labelContent; }
            set { _labelContent = value; }
        }

        Grid _content;
        Grid Content
        {
            get { return _content; }
            set { _content = value; }
        }

        private void MessageReceived(Message m)
        {
            MessageEventHandler h;
            if (MessageHandlers.TryGetValue(m.GetType(), out h))
            {
                h.Invoke(m);
            }
        }

        private void OnServiceProviderChanged(ServiceProvider oldValue, ServiceProvider newValue)
        {
            if (oldValue != null)
            {
                RenderingMessages.DetachTarget(MessageReceived);
            }

            if (newValue != null)
            {
                RenderingMessages = (MessageChannel)newValue.GetService("RenderingMessages");
                RenderingMessages.AttachTarget(MessageReceived);
            }
        }

        private void RenderAxisMessageReceived(AxisRenderMessage message)
        {
            RenderLabels();
        }

        private void RenderLabels()
        {
            LabelContent.Children.Clear();

            if (Sparkline == null || Sparkline.LabelColumn == null || Sparkline.LabelColumn.Count == 0)
            {
                return;
            }

            FrameworkElement minLabel = LabelFormatter.GetLabelElement(this.Sparkline.LabelColumn[0], this.Sparkline.HorizontalAxisLabel);
            minLabel.HorizontalAlignment = HorizontalAlignment.Left;
            minLabel.VerticalAlignment = VerticalAlignment.Top;
            LabelContent.Children.Add(minLabel);

            FrameworkElement maxLabel = LabelFormatter.GetLabelElement(this.Sparkline.LabelColumn[this.Sparkline.LabelColumn.Count - 1], this.Sparkline.HorizontalAxisLabel);
            maxLabel.HorizontalAlignment = HorizontalAlignment.Right;
            maxLabel.VerticalAlignment = VerticalAlignment.Top;
            LabelContent.Children.Add(maxLabel);

        }

        private void InsertLabel(TextBlock leftLabel, TextBlock rightLabel, double left, double right)
        {
            double center = (left + right) / 2;
            int index = (int)Math.Floor(center / RenderSize.Width * Sparkline.LabelColumn.Count);

            TextBlock label = new TextBlock();
            label.Text = Sparkline.LabelColumn[index].ToString();

            if (label.ActualWidth > right - left || leftLabel.Text == label.Text || rightLabel.Text == label.Text)
            {
                return;
            }

            label.RenderTransform = new TranslateTransform { X = center - label.ActualWidth / 2 };

            LabelContent.Children.Add(label);

            InsertLabel(leftLabel, label, left, center - label.ActualWidth / 2);
            InsertLabel(label, rightLabel, center + label.ActualWidth / 2, right);
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