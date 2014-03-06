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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Math.Calculators
{
    public abstract class ItemsSourceCalculator : DependencyObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Constructor
        public ItemsSourceCalculator()
        {
            this.FastItemsSourceEventHandler = new EventHandler<FastItemsSourceEventArgs>(FastItemsSource_Event);
            this.ValueMemberPathUpdatedAction = () => { };
            this.FastItemsSourceUpdatedAction = () => { };
        }
        #endregion // Constructor

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
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ItemsSourceCalculator),
            new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsSourceCalculator)d).OnItemsSourceChanged(e);
        }

        protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            this.FastItemsSource = new FastItemsSource() { ItemsSource = this.ItemsSource };
        }

        #endregion

        #region ValueMemberPath

        /// <summary>
        /// The ValueMemberPath for the ItemsSource.
        /// </summary>
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register("ValueMemberPath", typeof(string), typeof(ItemsSourceCalculator),
            new PropertyMetadata("", new PropertyChangedCallback(OnValueMemberPathChanged)));

        private static void OnValueMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsSourceCalculator)d).OnValueMemberPathChanged(e);
        }

        protected virtual void OnValueMemberPathChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.FastItemsSource != null)
            {
                this.FastItemsSource.DeregisterColumn(this.ValueColumn);
                this.ValueColumn = this.FastItemsSource.RegisterColumn(this.ValueMemberPath);
            }
            ValueMemberPathUpdatedAction();
        }

        protected Action ValueMemberPathUpdatedAction;

        #endregion ValueMemberPath

        #endregion // Public

        #region Protected

        protected IFastItemColumn<double> ValueColumn { get; set; }

        protected EventHandler<FastItemsSourceEventArgs> FastItemsSourceEventHandler { get; set; }
        
        #endregion Protected

        #region Private

        #region FastItemsSource

        /// <summary>
        /// FastItemsSource for the data.
        /// </summary>
        private FastItemsSource FastItemsSource
        {
            get { return (FastItemsSource)GetValue(FastItemsSourceProperty); }
            set { SetValue(FastItemsSourceProperty, value); }
        }
        private static readonly DependencyProperty FastItemsSourceProperty =
            DependencyProperty.Register("FastItemsSource", typeof(FastItemsSource), typeof(ItemsSourceCalculator),
            new PropertyMetadata(null, new PropertyChangedCallback(OnFastItemsSourceChanged)));

        private static void OnFastItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsSourceCalculator)d).OnFastItemsSourceChanged(e);
        }

        protected virtual void OnFastItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            FastItemsSource oldFastItemsSource = e.OldValue as FastItemsSource;
            if (oldFastItemsSource != null)
            {
                this.FastItemsSource.Event -= FastItemsSourceEventHandler;
                //oldFastItemsSource.Event -= this.HandleFastItemsSourceEvent;
                oldFastItemsSource.DeregisterColumn(this.ValueColumn);
                this.ValueColumn = null;
            }
            FastItemsSource newFastItemsSource = e.NewValue as FastItemsSource;
            if (newFastItemsSource != null)
            {
                this.FastItemsSource.Event += FastItemsSourceEventHandler;
                //newFastItemsSource.Event += this.HandleFastItemsSourceEvent;
                Debug.Assert(!(this.ItemsSource is AggregateValueCollection) || string.IsNullOrEmpty(this.ValueMemberPath));
                this.ValueColumn = newFastItemsSource.RegisterColumn(this.ValueMemberPath);
            }
            FastItemsSourceUpdatedAction();
        }

        protected Action FastItemsSourceUpdatedAction;

        #endregion

        #endregion Private

        #endregion // Properties

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // Events

        #region Methods

        #region Public

        #endregion //Public

        #region Protected
        protected void RaisePropertyChanged(ItemsSourceCalculator sender, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sender, e);
            }
        }
        #endregion

        #region Internal

        internal Infragistics.Math.Vector CreateInputVector(IList<double> input)
        {
            Infragistics.Math.Vector vector;
            if (InputContainsNan(input))
            {
                IList<double> clearedInput = ClearInputNan(input);

                if (clearedInput.Count == 0)
                {
                    return new Infragistics.Math.Vector();
                }
                vector = new Infragistics.Math.Vector(clearedInput);
            }
            else
            {
                vector = new Infragistics.Math.Vector(input);
            }

            return vector;
        }


        #endregion //Internal

        #region Private

        #region FastItemsSource_Event

        protected virtual void FastItemsSource_Event(object sender, FastItemsSourceEventArgs e)
        {
            switch (e.Action)
            {
                case FastItemsSourceEventAction.Remove:
                    break;
                case FastItemsSourceEventAction.Insert:
                    break;
                case FastItemsSourceEventAction.Replace:
                    break;
                case FastItemsSourceEventAction.Change:
                    break;
                case FastItemsSourceEventAction.Reset:
                    break;
            }
        }
        #endregion //FastItemsSource_Event

        #region InputContainsNan
        protected bool InputContainsNan(IList<double> input)
        {
            foreach (double value in input)
            {
                if (double.IsNaN(value))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion //InputContainsNan

        #region ClearInputNan
        protected IList<double> ClearInputNan(IList<double> input)
        {
            List<double> clearedInput = new List<double>();

            foreach (double value in input)
            {
                if (!double.IsNaN(value))
                {
                    clearedInput.Add(value);
                }
            }

            return clearedInput;
        }
        #endregion //ClearInputNan

        #endregion //Private

        #endregion //Methods

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

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