
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Collections;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Stripe keeps appearance, animation and unit properties for 
    /// axis stripe.
    /// </summary>
    public class Stripe : ChartFrameworkContentElement
    {
        #region Fields

        // Private Fields
        private object _chartParent;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Stripe class. 
        /// </summary>
        public Stripe()
        {
        }

        /// <summary>
        /// Add child objects to logical tree
        /// </summary>
        internal void AddChildren()
        {
            AddChild(this.Animation);
        }

        /// <summary>
        /// Gets an enumerator for this element's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList _list = new ArrayList();

                _list.Add(this.Animation);

                return (IEnumerator)_list.GetEnumerator();
            }
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);
            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        #endregion Methods

        #region Public Properties

        #region Animation

        /// <summary>
        /// Identifies the <see cref="Animation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation",
            typeof(Animation), typeof(Stripe), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the animation for Stripes. This animation draws stripe rectangles 
        /// from minimum to maximum axis position. Stripes could be animated one by one or all together.
        /// </summary>
        /// <remarks>
        /// This animation is only used to create growing effect for rectangles, but stripes animation could be 
        /// also created using brush property and WPF animation.
        /// </remarks>
        /// <seealso cref="AnimationProperty"/>
        //[Description("Gets or sets the animation for Stripes. This animation draws stripe rectangles from minimum to maximum axis position. Stripes could be animated one by one or all together.")]
        public Animation Animation
        {
            get
            {
                Animation obj = (Animation)this.GetValue(Stripe.AnimationProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(Stripe.AnimationProperty, value);
            }
        }

        #endregion Animation

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(Stripe), new FrameworkPropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        //[Category("Brushes")]
        public Brush Stroke
        {
            get
            {
                return (Brush)this.GetValue(Stripe.StrokeProperty);
            }
            set
            {
                this.SetValue(Stripe.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(Stripe), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        /// <seealso cref="StrokeThicknessProperty"/>
        //[Description("Gets or sets the width of the Shape outline.")]
        //[Category("Appearance")]
        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(Stripe.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(Stripe.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness

        #region Unit

        /// <summary>
        /// Identifies the <see cref="Unit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit",
            typeof(double), typeof(Stripe), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(AxisUnitValidateCallback));

        /// <summary>
        /// Gets or sets a distance between two neighboring stripes. If value is 0 double axis unit value is used. 
        /// </summary>
        /// <seealso cref="UnitProperty"/>
        //[Description("Gets or sets a distance between two neighboring stripes. If value is 0, double axis unit value is used. ")]
        //[Category("Range")]
        public double Unit
        {
            get
            {
                return (double)this.GetValue(Stripe.UnitProperty);
            }
            set
            {
                this.SetValue(Stripe.UnitProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool AxisUnitValidateCallback(object value)
        {
            double unit = (double)value;

            return (unit >= 0);

        }

        #endregion Unit

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(Stripe), new FrameworkPropertyMetadata(Brushes.LightGray, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        /// <seealso cref="FillProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the shape. ")]
        //[Category("Brushes")]
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(Stripe.FillProperty);
            }
            set
            {
                this.SetValue(Stripe.FillProperty, value);
            }
        }

        #endregion Fill
               
        #region Width

        /// <summary>
        /// Identifies the <see cref="Width"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width",
            typeof(double), typeof(Stripe), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(WidthValidateCallback));

        /// <summary>
        /// Gets or sets a width between two neighboring stripes. If value is 0 automatic width is calculated from axis unit value. 
        /// </summary>
        /// <seealso cref="WidthProperty"/>
        //[Description("Gets or sets a width between two neighboring stripes. If value is 0 automatic width is calculated from axis unit value.")]
        //[Category("Range")]
        public double Width
        {
            get
            {
                return (double)this.GetValue(Stripe.WidthProperty);
            }
            set
            {
                this.SetValue(Stripe.WidthProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>true if the value was validated; false if the submitted value was invalid.</returns>
        private static bool WidthValidateCallback(object value)
        {
            double width = (double)value;

            return (width >= 0);

        }

        #endregion Width

        #endregion Public Properties
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