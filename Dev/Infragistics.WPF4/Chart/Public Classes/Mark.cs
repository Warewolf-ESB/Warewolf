
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Mark keeps appearance, animation and unit properties for 
    /// axis gridlines and tick marks.
    /// </summary>
    public class Mark : ChartFrameworkContentElement
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
        /// Initializes a new instance of the Mark class. 
        /// </summary>
        public Mark()
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
            XamChart control;
            if (e.Property == Mark.DashStyleProperty)
            {
                control = XamChart.GetControl(d);
                if (control != null)
                {
                    control.RefreshProperty();
                }
                return;
            }

            control = XamChart.GetControl(d);
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
            typeof(Animation), typeof(Mark), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the animation for Marks. This animation is used for gridlines and it draws lines 
        /// from minimum to maximum axis position. Gridlines could be animated one by one or all together.
        /// </summary>
        /// <remarks>
        /// This animation is only used to create growing effect for lines, but gridlines animation could be 
        /// also created using brush property and WPF animation.
        /// </remarks>
        /// <seealso cref="AnimationProperty"/>
        //[Description("Gets or sets the animation for Marks. This animation is used for gridlines and it draws lines from minimum to maximum axis position. Gridlines could be animated one by one or all together.")]
        public Animation Animation
        {
            get
            {
                Animation obj = (Animation)this.GetValue(Mark.AnimationProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(Mark.AnimationProperty, value);
            }
        }

        #endregion Animation

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(Mark), new FrameworkPropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (Brush)this.GetValue(Mark.StrokeProperty);
            }
            set
            {
                this.SetValue(Mark.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region Visible

        /// <summary>
        /// Identifies the <see cref="Visible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible",
            typeof(bool), typeof(Mark), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the Mark is visible.
        /// </summary>
        /// <seealso cref="VisibleProperty"/>
        //[Description("Gets or sets a value that indicates whether the Mark is visible.")]
        //[Category("Behavior")]
        public bool Visible
        {
            get
            {
                return (bool)this.GetValue(Mark.VisibleProperty);
            }
            set
            {
                this.SetValue(Mark.VisibleProperty, value);
            }
        }

        #endregion Visible

        #region DashStyle

        /// <summary>
        /// Identifies the <see cref="DashStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DashStyleProperty = DependencyProperty.Register("DashStyle",
            typeof(DashStyle), typeof(Mark), new FrameworkPropertyMetadata(DashStyles.Solid, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that describes the pattern of dashes generated for this Mark. Supported for 2D charts only.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        //[Description("Gets or sets a value that describes the pattern of dashes generated for this Mark. Supported for 2D charts only.")]
        //[Category("Appearance")]
        public DashStyle DashStyle
        {
            get
            {
                return (DashStyle)this.GetValue(Mark.DashStyleProperty);
            }
            set
            {
                this.SetValue(Mark.DashStyleProperty, value);
            }
        }

        #endregion DashStyle

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(Mark), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

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
                return (double)this.GetValue(Mark.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(Mark.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness

        #region TickMarkSize

        /// <summary>
        /// Identifies the <see cref="TickMarkSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TickMarkSizeProperty = DependencyProperty.Register("TickMarkSize",
            typeof(double), typeof(Mark), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(OnTickMarkSizeValidate));

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
                return (double)this.GetValue(Mark.TickMarkSizeProperty);
            }
            set
            {
                this.SetValue(Mark.TickMarkSizeProperty, value);
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

        #region Unit

        /// <summary>
        /// Identifies the <see cref="Unit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit",
            typeof(double), typeof(Mark), new FrameworkPropertyMetadata((double)0, new PropertyChangedCallback(OnPropertyChanged)), new ValidateValueCallback(AxisUnitValidateCallback));

        /// <summary>
        /// Gets or sets a distance between two neighboring gridlines or tickmarks. If value is 0 axis unit value is used. 
        /// </summary>
        /// <seealso cref="UnitProperty"/>
        //[Description("Gets or sets a distance between two neighboring gridlines or tickmarks. If value is 0 axis unit value is used. ")]
		//[Category("Range")]
        public double Unit
        {
            get
            {
                return (double)this.GetValue(Mark.UnitProperty);
            }
            set
            {
                this.SetValue(Mark.UnitProperty, value);
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