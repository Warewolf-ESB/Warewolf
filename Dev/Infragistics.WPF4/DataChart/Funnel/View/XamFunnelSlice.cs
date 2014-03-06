using System;
using System.Collections.Generic;
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
using System.Windows.Data;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a slice in the funnel chart.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]

    [DesignTimeVisible(false)]

    [TemplatePart(Name="PART_LABEL", Type=typeof(FrameworkElement))]
    public class XamFunnelSlice : ContentControl
    {
        /// <summary>
        /// Initializes a default, empty XamFunnelSlice object. 
        /// </summary>
        public XamFunnelSlice()
        {
            this.DefaultStyleKey = typeof(XamFunnelSlice);
            SetBinding(ActualFillProperty, new Binding(FillPropertyName));
            SetBinding(ActualOutlineProperty, new Binding(OutlinePropertyName));
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            Detach();
            base.OnApplyTemplate();
            Attach();
        }

        /// <summary>
        /// The label control associated with the slice.
        /// </summary>
        protected FrameworkElement LabelControl { get; set; }

        /// <summary>
        /// The owner of the slice.
        /// </summary>
        public XamFunnelView Owner { get; set; }

        private void Attach()
        {
            LabelControl = (FrameworkElement)GetTemplateChild("PART_LABEL");
            if (LabelControl != null)
            {
                LabelControl.SizeChanged += LabelControl_SizeChanged;
            }
        }

        void LabelControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Owner != null)
            {
                if (e.PreviousSize.Width > 0 && e.PreviousSize.Height > 0 &&
                    e.NewSize.Width > 0 && e.NewSize.Height > 0 &&
                    (e.NewSize.Width != e.PreviousSize.Width ||
                    e.NewSize.Height != e.PreviousSize.Height))
                {
                    SliceAppearance sa = DataContext as SliceAppearance;
                    int index = -1;
                    if (sa != null)
                    {
                        index = sa.Index;
                    }
                    Owner.LabelSizeChanged(index, e.NewSize, e.PreviousSize, false);
                }
            }
        }

        private void Detach()
        {
            if (LabelControl != null)
            {
                LabelControl.SizeChanged -= LabelControl_SizeChanged;
            }
        }

        #region StrokeThickness Dependency Property
        /// <summary>
        /// Gets or sets the thickness to use for the stroke of the slice.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }
            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        internal const string StrokeThicknessPropertyName = "StrokeThickness";

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
            StrokeThicknessPropertyName,
            typeof(double),
            typeof(XamFunnelSlice),
            new PropertyMetadata(1.0));
        #endregion

        #region Fill Dependency Property
        /// <summary>
        /// Gets or sets the brush to use for the filled portion of the slice.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush Fill
        {
            get
            {
                return (Brush)GetValue(FillProperty);
            }
            set
            {
                SetValue(FillProperty, value);
            }
        }

        internal const string FillPropertyName = "Fill";

        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(
            FillPropertyName,
            typeof(Brush),
            typeof(XamFunnelSlice),
            new PropertyMetadata(null, 
                (o, e) => (o as XamFunnelSlice).OnPropertyChanged(FillPropertyName, e.OldValue, e.NewValue)));
        #endregion

        #region Outline Dependency Property
        /// <summary>
        /// Gets or sets the brush to use for the stroke portion of the slice.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush Outline
        {
            get
            {
                return (Brush)GetValue(OutlineProperty);
            }
            set
            {
                SetValue(OutlineProperty, value);
            }
        }

        internal const string OutlinePropertyName = "Outline";

        /// <summary>
        /// Identifies the Outline dependency property.
        /// </summary>
        public static readonly DependencyProperty OutlineProperty =
            DependencyProperty.Register(
            OutlinePropertyName,
            typeof(Brush),
            typeof(XamFunnelSlice),
            new PropertyMetadata(null,
                (o, e) => (o as XamFunnelSlice).OnPropertyChanged(OutlinePropertyName, e.OldValue, e.NewValue)));
        #endregion

        #region ActualFill Dependency Property
        /// <summary>
        /// Gets or sets the effective brush to use for the filled portion of the slice.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush ActualFill
        {
            get
            {
                return (Brush)GetValue(ActualFillProperty);
            }
            set
            {
                SetValue(ActualFillProperty, value);
            }
        }

        internal const string ActualFillPropertyName = "ActualFill";

        /// <summary>
        /// Identifies the ActualFill dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualFillProperty =
            DependencyProperty.Register(
            ActualFillPropertyName,
            typeof(Brush),
            typeof(XamFunnelSlice),
            new PropertyMetadata(null));
        #endregion

        #region ActualOutline Dependency Property
        /// <summary>
        /// Gets or sets the effective brush to use for the stroke portion of the slice.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Brush ActualOutline
        {
            get
            {
                return (Brush)GetValue(ActualOutlineProperty);
            }
            set
            {
                SetValue(ActualOutlineProperty, value);
            }
        }

        internal const string ActualOutlinePropertyName = "ActualOutline";

        /// <summary>
        /// Identifies the ActualOutline dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualOutlineProperty =
            DependencyProperty.Register(
            ActualOutlinePropertyName,
            typeof(Brush),
            typeof(XamFunnelSlice),
            new PropertyMetadata(null));
        #endregion

        #region LabelVisibility Dependency Property
        /// <summary>
        /// Gets or sets whether the inner label is visible or not. 
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public Visibility LabelVisibility
        {
            get
            {
                return (Visibility)GetValue(LabelVisibilityProperty);
            }
            set
            {
                SetValue(LabelVisibilityProperty, value);
            }
        }

        internal const string LabelVisibilityPropertyName = "LabelVisibility";

        /// <summary>
        /// Identifies the LabelVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelVisibilityProperty =
            DependencyProperty.Register(
            LabelVisibilityPropertyName,
            typeof(Visibility),
            typeof(XamFunnelSlice),
            new PropertyMetadata(Visibility.Visible));
        #endregion

        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case "Fill":
                    if (newValue == null)
                    {
                        SetBinding(ActualFillProperty, new Binding(FillPropertyName));
                    }
                    else
                    {
                        SetBinding(ActualFillProperty, new Binding(FillPropertyName) { Source = this });
                    }
                    break;
                case "Outline":
                    if (newValue == null)
                    {
                        SetBinding(ActualOutlineProperty, new Binding(OutlinePropertyName));
                    }
                    else
                    {
                        SetBinding(ActualOutlineProperty, new Binding(OutlinePropertyName) { Source = this });
                    }
                    break;
            }
        }


        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (LabelControl != null)
            {
                LabelControl.RenderTransform = new TranslateTransform()
                {
                    X = (finalSize.Width / 2.0) - (LabelControl.DesiredSize.Width / 2.0),
                    Y = (finalSize.Height / 2.0) - (LabelControl.DesiredSize.Height / 2.0)
                };
            }

            return base.ArrangeOverride(finalSize);
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