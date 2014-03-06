
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// The chart legend box appears alongside the chart border. It is used 
    /// to give text description for every data point or series appearance in 
    /// the chart. Many qualities of the legend are dependent upon ChartType.
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class Legend : ChartContentControl
    {
        #region Fields

        // Private fields
        private object _chartParent;
        private LegendPane _legendPane;
        
        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or Sets the legend pane
        /// </summary>
        internal LegendPane LegendPane
        {
            get
            {
                return _legendPane;
            }
            set
            {
                _legendPane = value;
            }
        }

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
        /// Initializes a new instance of the Legend class. 
        /// </summary>
        public Legend()
        {
        }

        static Legend()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Legend), new FrameworkPropertyMetadata(typeof(Legend)));
        }

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this 
        /// FrameworkElement has been updated. The specific dependency property that changed 
        /// is reported in the arguments parameter. Overrides OnPropertyChanged. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(this);
            if (control != null && e.NewValue != e.OldValue)
            {
                if (XamChart.PropertyNeedRefresh(this,e))
                {
                    if (!control.ContentControlPropertyChanging)
                    {
                        control.RefreshProperty();
                    }
                }
            }

            base.OnPropertyChanged(e);
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

        #region Visible

        /// <summary>
        /// Identifies the <see cref="Visible"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible",
            typeof(bool), typeof(Legend), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the legend is visible.
        /// </summary>
        /// <seealso cref="VisibleProperty"/>
        //[Description("Gets or sets a value that indicates whether the legend is visible.")]
        //[Category("Behavior")]
        public bool Visible
        {
            get
            {
                return (bool)this.GetValue(Legend.VisibleProperty);
            }
            set
            {
                this.SetValue(Legend.VisibleProperty, value);
            }
        }

        #endregion Visible

        #region UseDataTemplate

        /// <summary>
        /// Identifies the <see cref="UseDataTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UseDataTemplateProperty = DependencyProperty.Register("UseDataTemplate",
            typeof(bool), typeof(Legend), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether legend items use data template.
        /// </summary>
        /// <seealso cref="UseDataTemplateProperty"/>
        //[Description("Gets or sets a value that indicates whether legend items use data template.")]
        //[Category("Behavior")]
        public bool UseDataTemplate
        {
            get
            {
                return (bool)this.GetValue(Legend.UseDataTemplateProperty);
            }
            set
            {
                this.SetValue(Legend.UseDataTemplateProperty, value);
            }
        }

        #endregion UseDataTemplate

        #region Margin

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public new static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin",
            typeof(Thickness), typeof(Legend), new FrameworkPropertyMetadata(new Thickness(0), new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.
        /// </summary>
        /// <remarks>
        /// The property uses percent as an unit, which means that the values for the left, top, right, and bottom of the bounding rectangle have to be values between 0 and 100. This relative layout will produce proportional resizing of the chart elements.
        /// </remarks>
        /// <seealso cref="MarginProperty"/>
        //[Description("Gets or sets the outer margin of an element using percent value from 0 to 100. This property doesn't have any effect if MarginType property is set to �Auto�.")]
        //[Category("Layout")]
        public new Thickness Margin
        {
            get
            {
                return (Thickness)this.GetValue(Legend.MarginProperty);
            }
            set
            {
                this.SetValue(Legend.MarginProperty, value);
            }
        }

        #endregion Margin
        
        #region MarginType

        /// <summary>
        /// Identifies the <see cref="MarginType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginTypeProperty = DependencyProperty.Register("MarginType",
            typeof(MarginType), typeof(Legend), new FrameworkPropertyMetadata(MarginType.Auto, new PropertyChangedCallback(OnPropertyChanged)));


        /// <summary>
        /// Gets or sets a margin type for the chart element. If MarginType property is �Percent�, the Margin property has to be set.
        /// </summary>
        /// <remarks>
        /// The margin type can be set to Auto or Percent. The default value is Auto, and the position of the chart element is automatically created. For manual positioning the MarginType has to be set to Percent.  
        /// </remarks>
        /// <seealso cref="Margin"/>
        /// <seealso cref="MarginTypeProperty"/>
        //[Description("Gets or sets a margin type for the chart element. If MarginType property is �Percent�, the Margin property has to be set.")]
        //[Category("Layout")]
        public MarginType MarginType
        {
            get
            {
                return (MarginType)this.GetValue(Legend.MarginTypeProperty);
            }
            set
            {
                this.SetValue(Legend.MarginTypeProperty, value);
            }
        }

        #endregion MarginType

        #region Items

        private LegendItemCollection _items = new LegendItemCollection();

        /// <summary>
        /// Gets the custom legend items. By default legend items are created using Series or DataPoint <see cref="Infragistics.Windows.Chart.DataPoint.Label"/>. 
        /// This collection should be used only if default appearance of the legend items has to be changed.
        /// </summary>
        /// <remarks>
        /// If the Items collection is empty legend items are automatically created from Series and DataPoints.
        /// </remarks>
        //[Description("Gets the custom legend items. By default legend items are created using Series or DataPoint Labels. This collection should be used only if default appearance of the legend items has to be changed.")]
        //[Category("Data")]
        public LegendItemCollection Items
        {
            get
            {
                if (_items.ChartParent == null)
                {
                    _items.ChartParent = this;
                }
                return _items;
            }
        }

        #endregion Items

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