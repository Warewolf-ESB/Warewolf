using System;



using System.Linq;

using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Data;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal class AxisLabelInfo
    {
        public string LabelString { get; set; }
        public double LabelPosition { get; set; }
    }

    internal class AxisLabelManager
    {
        public List<object> LabelDataContext { get; set; }
        public List<LabelPosition> LabelPositions { get; set; }
        //public object BindingSource { get; set; }
        public AxisLabelPanelBase TargetPanel { get; set; }

        [Weak]
        public Axis Axis { get; set; }
        public Action<double> FloatPanelAction { get; set; }

        public AxisLabelManager()
        {
            FloatPanelAction = (crossing) => { };
        }

        public virtual void Clear(Rect windowRect, Rect viewportRect)
        {
            LabelDataContext.Clear();
            LabelPositions.Clear();
            TargetPanel.Axis = Axis;
            //foreach (TextBlock tb in 
            //    TargetPanel.Children.OfType<TextBlock>())
            //{
            //    UnbindLabel(tb);
            //}
            //TargetPanel.Children.Clear();
            TargetPanel.WindowRect = windowRect;
            TargetPanel.ViewportRect = viewportRect;
            if (viewportRect.IsEmpty || windowRect.IsEmpty)
            {
                SetTextBlockCount(0);
            }
            if (Axis.TextBlocks.Count == 0)
            {
                TargetPanel.Children.Clear();
            }
        }

        public virtual void AddLabelObject(object labelObject, LabelPosition position)
        {
            LabelDataContext.Add(labelObject);
            LabelPositions.Add(position);
        }

        public virtual void UpdateLabelPanel()
        {
            TargetPanel.LabelDataContext = LabelDataContext;
            TargetPanel.LabelPositions = LabelPositions;
        }

//        public static void UnbindLabel(FrameworkElement label)
//        {
//#if !WINDOWS_PHONE
//            label.ClearValue(TextBlock.EffectProperty);
//#endif
//            label.ClearValue(TextBlock.ForegroundProperty);
//            label.ClearValue(TextBlock.FontFamilyProperty);
//            label.ClearValue(TextBlock.FontSizeProperty);
//            label.ClearValue(TextBlock.FontStretchProperty);
//            label.ClearValue(TextBlock.FontStyleProperty);
//            label.ClearValue(TextBlock.FontWeightProperty);
//            label.ClearValue(TextBlock.HorizontalAlignmentProperty);
//            label.ClearValue(TextBlock.IsHitTestVisibleProperty);
//            label.ClearValue(TextBlock.OpacityMaskProperty);
//            label.ClearValue(TextBlock.OpacityProperty);
//            label.ClearValue(TextBlock.PaddingProperty);
//#if SILVERLIGHT
//            label.ClearValue(TextBlock.ProjectionProperty);
//#endif
//            label.ClearValue(TextBlock.StyleProperty);
//            label.ClearValue(TextBlock.TextAlignmentProperty);
//            label.ClearValue(TextBlock.TextDecorationsProperty);
//            label.ClearValue(TextBlock.TextWrappingProperty);
//            label.ClearValue(TextBlock.VerticalAlignmentProperty);
//            label.ClearValue(TextBlock.VisibilityProperty);
//            label.ClearValue(TextBlock.TextProperty);
//        }

        //public virtual void BindLabel(FrameworkElement label)
        //{
        //    BindLabel(label, BindingSource);
        //}

        public static void BindLabel(FrameworkElement label)
        {


            label.SetBinding(TextBlock.EffectProperty,
                new Binding("Effect"));

            label.SetBinding(TextBlock.ForegroundProperty,
                new Binding("Foreground"));
            label.SetBinding(TextBlock.FontFamilyProperty,
                new Binding("FontFamily"));
            label.SetBinding(TextBlock.FontSizeProperty,
                new Binding("FontSize"));
            label.SetBinding(TextBlock.FontStretchProperty,
                new Binding("FontStretch"));
            label.SetBinding(TextBlock.FontStyleProperty,
                new Binding("FontStyle"));
            label.SetBinding(TextBlock.FontWeightProperty,
                new Binding("FontWeight"));
            label.SetBinding(TextBlock.HorizontalAlignmentProperty,
                new Binding("HorizontalAlignment"));
            label.SetBinding(TextBlock.IsHitTestVisibleProperty,
                new Binding("IsHitTestVisible"));
            label.SetBinding(TextBlock.OpacityMaskProperty,
                new Binding("OpacityMask"));
            label.SetBinding(TextBlock.OpacityProperty,
                new Binding("Opacity"));
            label.SetBinding(TextBlock.PaddingProperty,
                new Binding("Padding"));




            label.SetBinding(TextBlock.TextAlignmentProperty,
                new Binding("TextAlignment"));
            label.SetBinding(TextBlock.TextDecorationsProperty,
                new Binding("TextDecorations"));
            label.SetBinding(TextBlock.TextWrappingProperty,
                new Binding("TextWrapping"));
            label.SetBinding(TextBlock.VerticalAlignmentProperty,
                new Binding("VerticalAlignment"));

        }

        public virtual void AddLabel(FrameworkElement label)
        {
            TargetPanel.Children.Add(label);
        }

        internal virtual void SetLabelInterval(double p)
        {
            TargetPanel.Interval = p;
        }

        internal virtual void FloatPanel(double crossingValue)
        {
            FloatPanelAction(crossingValue);
        }

        internal FrameworkElement GetTextBlock(int i)
        {
            TextBlock tb = Axis.TextBlocks[i];
            return tb;
        }

        internal void SetTextBlockCount(int p)
        {
            Axis.TextBlocks.Count = p;
        }

        public bool LabelsHidden
        {
            get
            {
                if (Axis.LabelSettings == null)
                {
                    return false;
                }
                return Axis.LabelSettings.Visibility != Visibility.Visible;
            }
        }

        internal void ResetLabels()
        {
            Axis.TextBlocks.Count = 0;
            Axis.LabelPanel.TextBlocks.Clear();
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