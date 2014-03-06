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

namespace Infragistics
{
    /// <summary>
    /// Base class for mark objects.
    /// </summary>
    public abstract class MarkBase : Control
    {
        /// <summary>
        /// MarkBase constructor.
        /// </summary>
        protected MarkBase()
            : base()
        {
            this.DefaultStyleKey = typeof(MarkBase);
            //this.Stroke = this.Fill = new SolidColorBrush(Colors.Gray);
            //this.Visibility = Visibility.Visible;
            //this.StrokeThickness = 1;
            //this.TickMarkSize = 1;
        }

        #region Public Properties

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(MarkBase), new PropertyMetadata(null, OnPropertyChanged));

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
                return (Brush)this.GetValue(MarkBase.StrokeProperty);
            }
            set
            {
                this.SetValue(MarkBase.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(MarkBase), new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <seealso cref="FillProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        //[Category("Brushes")]
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(MarkBase.FillProperty);
            }
            set
            {
                this.SetValue(MarkBase.FillProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MarkBase), new PropertyMetadata(1.0, OnPropertyChanged));

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
                return (double)this.GetValue(MarkBase.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(MarkBase.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness

        #region TickMarkSize

        /// <summary>
        /// Identifies the <see cref="TickMarkSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TickMarkSizeProperty = DependencyProperty.Register("TickMarkSize", typeof(double), typeof(MarkBase), new PropertyMetadata(1.0, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the TickMark size. This is scaling factor applied to the default TickMark size. Value 1 is default size, a value between 0 and 1 decrease the default TickMark size and a value greater than 1 increase the default TickMark size. 
        /// </summary>
        /// <seealso cref="TickMarkSizeProperty"/>
        //[Description("Gets or sets the TickMark size. This is scaling factor applied to the default TickMark size. Value 1 is default size, a value between 0 and 1 decrease the default TickMark size and a value greater than 1 increase the default TickMark size.")]
        //[Category("Appearance")]
        public double TickMarkSize
        {
            get
            {
                return (double)this.GetValue(MarkBase.TickMarkSizeProperty);
            }
            set
            {
                this.SetValue(MarkBase.TickMarkSizeProperty, value);
            }
        }

        /// <summary>
        /// Represents a method used as a callback when registering a new dependency property or attached property. 
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if the value was validated; false if the submitted value was invalid.</returns>
        private static bool OnTickMarkSizeValidate(object value)
        {
            double tickMarkSize = (double)value;
            return (tickMarkSize >= 0);
        }

        #endregion TickMarkSize
        

        #endregion Public Properties

        #region OnPropertyChanged
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

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