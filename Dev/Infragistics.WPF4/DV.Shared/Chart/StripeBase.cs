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
    /// Stripe keeps appearance, animation and unit properties for 
    /// axis stripe.
    /// </summary>
    public abstract class StripeBase : Control
    {
        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeBase"/> class.
        /// </summary>
        protected StripeBase()
        {
            this.DefaultStyleKey = typeof(StripeBase);
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion Methods

        #region Public Properties     

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(StripeBase), new PropertyMetadata(new SolidColorBrush(Colors.Gray), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        public Brush Stroke
        {
            get
            {
                return (Brush)this.GetValue(StripeBase.StrokeProperty);
            }
            set
            {
                this.SetValue(StripeBase.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(StripeBase), new PropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        /// <seealso cref="StrokeThicknessProperty"/>
        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(StripeBase.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(StripeBase.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness        

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(StripeBase), new PropertyMetadata(new SolidColorBrush(Colors.LightGray), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        /// <seealso cref="FillProperty"/>
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(StripeBase.FillProperty);
            }
            set
            {
                this.SetValue(StripeBase.FillProperty, value);
            }
        }

        #endregion Fill

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