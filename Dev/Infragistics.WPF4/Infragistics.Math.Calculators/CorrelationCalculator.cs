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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Infragistics.Collections;
using System.Diagnostics;
using Infragistics.Math;

namespace Infragistics.Math.Calculators
{
    /// <summary>
    /// Represents the class that calculates the correlation between two sets of data
    /// </summary>
    public class CorrelationCalculator : DependencyObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationCalculator"/> class.
        /// </summary>
        public CorrelationCalculator()
        {
            this.FastItemsSourceEventHandlerX = new EventHandler<FastItemsSourceEventArgs>(FastItemsSourceX_Event);
            this.FastItemsSourceEventHandlerY = new EventHandler<FastItemsSourceEventArgs>(FastItemsSourceY_Event);
        }
        #endregion //Constructor

        #region Properties

        #region Public

        #region ItemsSource

        /// <summary>
        /// The items source for the calculator.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CorrelationCalculator),
            new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CorrelationCalculator)d).OnItemsSourceChanged(e);
        }

        protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            this.FastItemsSourceX = new FastItemsSource() { ItemsSource = this.ItemsSource };
            this.FastItemsSourceY = new FastItemsSource() { ItemsSource = this.ItemsSource };

            this.UpdateCalculation();
        }

        private void UpdateCalculation()
        {
            if (this.ValueColumnX.Count == 0 || this.ValueColumnY.Count == 0 || this.ValueColumnX.Count != this.ValueColumnY.Count)
            {
                this.Value = double.NaN;
            }
            else
            {
                var x = this.ValueColumnX;
                var y = this.ValueColumnY;

                // clean NaN values, which would make the result undefined
                double[] arrayX = new double[x.Count];
                double[] arrayY = new double[y.Count];
                int count = 0;
                for (int i = 0; i < x.Count && i < y.Count; i++)
                {
                    if (!double.IsNaN(x[i]) && !double.IsNaN(y[i]))
                    {
                        arrayX[count] = x[i];
                        arrayY[count] = y[i];
                        count++;
                    }
                }
                
                // initialize vectors based on cleaned values
                Vector v1 = new Vector(count);
                Vector v2 = new Vector(count);
                for (int i = 0; i < count; i++)
                {
                    v1[i] = arrayX[i];
                    v2[i] = arrayY[i];
                }

                // compute correlation
                this.Value = Compute.Correlation(v1, v2);
            }
        }

        #endregion
        
        #region XMemberPath

        /// <summary>
        /// The X member path.
        /// </summary>
        public string XMemberPath
        {
            get { return (string)GetValue(XMemberPathProperty); }
            set { SetValue(XMemberPathProperty, value); }
        }
        public static readonly DependencyProperty XMemberPathProperty =
            DependencyProperty.Register("XMemberPath", typeof(string), typeof(CorrelationCalculator),
            new PropertyMetadata("", new PropertyChangedCallback(OnXMemberPathChanged)));

        private static void OnXMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CorrelationCalculator)d).OnXMemberPathChanged(e);
        }

        protected virtual void OnXMemberPathChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.FastItemsSourceX != null)
            {
                this.FastItemsSourceX.DeregisterColumn(this.ValueColumnX);
                this.ValueColumnX = this.FastItemsSourceX.RegisterColumn(this.XMemberPath);
                this.UpdateCalculation();
            }
        }

        #endregion
        
        #region YMemberPath

        /// <summary>
        /// The Y member path.
        /// </summary>
        public string YMemberPath
        {
            get { return (string)GetValue(YMemberPathProperty); }
            set { SetValue(YMemberPathProperty, value); }
        }
        public static readonly DependencyProperty YMemberPathProperty =
            DependencyProperty.Register("YMemberPath", typeof(string), typeof(CorrelationCalculator),
            new PropertyMetadata("", new PropertyChangedCallback(OnYMemberPathChanged)));

        private static void OnYMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CorrelationCalculator)d).OnYMemberPathChanged(e);
        }

        protected virtual void OnYMemberPathChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.FastItemsSourceY != null)
            {
                this.FastItemsSourceY.DeregisterColumn(this.ValueColumnY);
                this.ValueColumnY = this.FastItemsSourceY.RegisterColumn(this.YMemberPath);
                this.UpdateCalculation();
            }
        }

        #endregion

        #region Value
        private const string ValuePropertyName = "Value";
        private double _value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public double Value
        {
            get { return _value; }
            private set 
            { 
                bool changed = this._value != value;
                _value = value;

                if (changed && this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(ValuePropertyName));
                }
            }
        }
        #endregion //Value

        #endregion //Public

        #region Private

        #region FastItemsSourceX

        /// <summary>
        /// A description of the property.
        /// </summary>
        private FastItemsSource FastItemsSourceX
        {
            get { return (FastItemsSource)GetValue(FastItemsSourceXProperty); }
            set { SetValue(FastItemsSourceXProperty, value); }
        }
        private static readonly DependencyProperty FastItemsSourceXProperty =
            DependencyProperty.Register("FastItemsSourceX", typeof(FastItemsSource), typeof(CorrelationCalculator),
            new PropertyMetadata(null, new PropertyChangedCallback(OnFastItemsSourceXChanged)));

        private static void OnFastItemsSourceXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CorrelationCalculator)d).OnFastItemsSourceXChanged(e);
        }

        protected virtual void OnFastItemsSourceXChanged(DependencyPropertyChangedEventArgs e)
        {
            FastItemsSource oldFastItemsSource = e.OldValue as FastItemsSource;
            if (oldFastItemsSource != null)
            {
                this.FastItemsSourceX.Event -= FastItemsSourceEventHandlerX;
                //oldFastItemsSource.Event -= this.HandleFastItemsSourceEvent;
                oldFastItemsSource.DeregisterColumn(this.ValueColumnX);
                this.ValueColumnX = null;
            }
            FastItemsSource newFastItemsSource = e.NewValue as FastItemsSource;
            if (newFastItemsSource != null)
            {
                this.FastItemsSourceX.Event += FastItemsSourceEventHandlerX;
                //newFastItemsSource.Event += this.HandleFastItemsSourceEvent;
                Debug.Assert(!(this.ItemsSource is AggregateValueCollection) || string.IsNullOrEmpty(this.XMemberPath));
                this.ValueColumnX = newFastItemsSource.RegisterColumn(this.XMemberPath);
            }
        }

        #endregion

        #region FastItemsSourceY

        /// <summary>
        /// A description of the property.
        /// </summary>
        private FastItemsSource FastItemsSourceY
        {
            get { return (FastItemsSource)GetValue(FastItemsSourceYProperty); }
            set { SetValue(FastItemsSourceYProperty, value); }
        }
        private static readonly DependencyProperty FastItemsSourceYProperty =
            DependencyProperty.Register("FastItemsSourceY", typeof(FastItemsSource), typeof(CorrelationCalculator),
            new PropertyMetadata(null, new PropertyChangedCallback(OnFastItemsSourceYChanged)));

        private static void OnFastItemsSourceYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CorrelationCalculator)d).OnFastItemsSourceYChanged(e);
        }

        protected virtual void OnFastItemsSourceYChanged(DependencyPropertyChangedEventArgs e)
        {
            FastItemsSource oldFastItemsSource = e.OldValue as FastItemsSource;
            if (oldFastItemsSource != null)
            {
                this.FastItemsSourceY.Event -= FastItemsSourceEventHandlerY;
                //oldFastItemsSource.Event -= this.HandleFastItemsSourceEvent;
                oldFastItemsSource.DeregisterColumn(this.ValueColumnY);
                this.ValueColumnY = null;
            }
            FastItemsSource newFastItemsSource = e.NewValue as FastItemsSource;
            if (newFastItemsSource != null)
            {
                this.FastItemsSourceY.Event += FastItemsSourceEventHandlerY;
                //newFastItemsSource.Event += this.HandleFastItemsSourceEvent;
                Debug.Assert(!(this.ItemsSource is AggregateValueCollection) || string.IsNullOrEmpty(this.YMemberPath));
                this.ValueColumnY = newFastItemsSource.RegisterColumn(this.YMemberPath);
            }
        }

        #endregion

        #region ValueColumnX

        private IFastItemColumn<double> ValueColumnX { get; set; }

        #endregion //ValueColumn

        #region ValueColumnY

        private IFastItemColumn<double> ValueColumnY { get; set; }

        #endregion //ValueColumn

        #endregion //Private

        #endregion //Properties

        #region Events

        private EventHandler<FastItemsSourceEventArgs> FastItemsSourceEventHandlerX { get; set; }
        private EventHandler<FastItemsSourceEventArgs> FastItemsSourceEventHandlerY { get; set; }

        private void FastItemsSourceX_Event(object sender, FastItemsSourceEventArgs e)
        {
            this.UpdateCalculation();
        }
        private void FastItemsSourceY_Event(object sender, FastItemsSourceEventArgs e)
        {
            this.UpdateCalculation();
        }

        #endregion //Events

        #region INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion //INotifyCollectionChanged

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion //PropertyChanged
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