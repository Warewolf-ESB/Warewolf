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
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Represents a set of axis stripes.
    /// </summary>
    public class StripeGroupBase : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StripeGroupBase"/> class.
        /// </summary>
        protected StripeGroupBase()
        {

        }

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(StripeGroupBase), new PropertyMetadata(new SolidColorBrush(Colors.Gray), new PropertyChangedCallback(OnPropertyChanged)));

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
                return (Brush)this.GetValue(StripeGroupBase.StrokeProperty);
            }
            set
            {
                this.SetValue(StripeGroupBase.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(StripeGroupBase), new PropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (double)this.GetValue(StripeGroupBase.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(StripeGroupBase.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness

        #region Style
        /// <summary>
        /// Identifies the <see cref="Style"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(StripeGroupBase), new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the style to be applied to the label during rendering.
        /// </summary>
        public Style Style
        {
            get { return (Style)this.GetValue(StripeGroupBase.StyleProperty); }
            set { this.SetValue(StripeGroupBase.StyleProperty, value); }
        }
        #endregion

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(StripeGroupBase), new PropertyMetadata(new SolidColorBrush(Colors.LightGray), new PropertyChangedCallback(OnPropertyChanged)));

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
                return (Brush)this.GetValue(StripeGroupBase.FillProperty);
            }
            set
            {
                this.SetValue(StripeGroupBase.FillProperty, value);
            }
        }

        #endregion Fill

        #region Unit

        /// <summary>
        /// Identifies the <see cref="Unit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit",
            typeof(double), typeof(StripeGroupBase), new PropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (double)this.GetValue(StripeGroupBase.UnitProperty);
            }
            set
            {
                this.SetValue(StripeGroupBase.UnitProperty, value);
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

        #region Width

        /// <summary>
        /// Identifies the <see cref="Width"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width",
            typeof(double), typeof(StripeGroupBase), new PropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a the stripe width
        /// </summary>
        /// <seealso cref="WidthProperty"/>
        public double Width
        {
            get
            {
                return (double)this.GetValue(StripeGroupBase.WidthProperty);
            }
            set
            {
                this.SetValue(StripeGroupBase.WidthProperty, value);
            }
        }
        #endregion Width

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
        internal protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StripeGroupBase stripeGroup = d as StripeGroupBase;
            stripeGroup.OnPropertyChanged(e);
        }

        /// <summary>
        /// Method invoked when a property is changed on this stripe group.
        /// </summary>
        /// <param name="e">The DependencyPropertyChangedEventArgs in context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e")]
        protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

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