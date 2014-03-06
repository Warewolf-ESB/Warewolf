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
using System.Diagnostics.CodeAnalysis;

namespace Infragistics
{
    /// <summary>
    /// Base class for axes.
    /// </summary>
    public abstract class AxisBase : Control, IDisposable
    {
        #region Ctor
        /// <summary>
        /// AxisBase constructor.
        /// </summary>
        protected AxisBase()
            : base()
        {

        }
        #endregion

        #region Public Properties
       
        #region AutoRange

        /// <summary>
        /// Identifies the <see cref="AutoRange"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoRangeProperty = DependencyProperty.Register("AutoRange",
            typeof(bool), typeof(AxisBase), new PropertyMetadata(true, OnPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the Minimum, Maximum and Unit are automatically calculated. To create manual range AutoRange property has to be set to false and Minimum, Maximum and Unit have to be set.
        /// </summary>
        /// <remarks>
        /// By default, chart determines the minimum and maximum range values of the axis. 
        /// You can customize the scale to better meet your needs. The default value for AutoRange is true.
        /// </remarks>
        /// <seealso cref="AutoRangeProperty"/>
        public bool AutoRange
        {
            get
            {
                return (bool)this.GetValue(AutoRangeProperty);
            }
            set
            {
                this.SetValue(AutoRangeProperty, value);
            }
        }

        #endregion AutoRange       

        #region ScrollScale
        /// <summary>
        /// Identifies the ScrollScale dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollScaleProperty = DependencyProperty.Register("ScrollScale", typeof(double), typeof(AxisBase), new PropertyMetadata(1.0, OnPropertyChanged));
        /// <summary>
        /// Gets or sets the scale of the scrollable area, expressed as a value between 0 and 1.
        /// </summary>
        /// <value>The scale of the scrollable area.</value>        
        public double ScrollScale
        {
            get { return (double)this.GetValue(AxisBase.ScrollScaleProperty); }
            set { this.SetValue(AxisBase.ScrollScaleProperty, value); }
        }
        #endregion

        #region ScrollPosition
        /// <summary>
        /// Identifies the ScrollPosition dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollPositionProperty = DependencyProperty.Register("ScrollPosition", typeof(double), typeof(AxisBase), new PropertyMetadata(0.0, OnPropertyChanged));
        /// <summary>
        /// Gets or sets the position of the current view inside the scrollable area, expressed as a value between 0 and 1.
        /// </summary>
        /// <value>The position of the current view inside the scrollable area.</value>        
        public double ScrollPosition
        {
            get { return (double)this.GetValue(AxisBase.ScrollPositionProperty); }
            set { this.SetValue(AxisBase.ScrollPositionProperty, value); }
        }
        #endregion

        #endregion

        #region OnPropertyChanged
        /// <summary>
        /// Method invoked when a property is changed on an AxisBase object.
        /// </summary>
        /// <param name="d">The axis for which a property has changed.</param>
        /// <param name="e">The DependencyPropertyChangedEventArgs in context.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AxisBase axis = d as AxisBase;
            if (d != null)
            {
                axis.OnPropertyChanged(e);
            }
        }
        /// <summary>
        /// Method invoked when a property is changed on this axis.
        /// </summary>
        /// <param name="e">The DependencyPropertyChangedEventArgs in context.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "e is the standard name for arguments deriving from EventArgs.")]
        protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;
        /// <summary>
        /// True if this object has been disposed, otherwise False.
        /// </summary>
        protected bool Disposed
        {
            get { return _disposed; }
            set { _disposed = value; }
        }
        /// <summary>
        /// AxisBase finalizer.
        /// </summary>
        ~AxisBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.Disposed = true;
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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