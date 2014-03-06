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
using System.ComponentModel;
using System.Collections.Specialized;

namespace Infragistics.Collections
{
    /// <summary>
    /// Represents the objects build AggregateValueCollection. Each AggregateValueSource has its own items source.
    /// </summary>
    public class AggregateValueSource : DependencyObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Contructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateValueSource"/> class.
        /// </summary>
        public AggregateValueSource()
        {
            
        }
        #endregion //Contructor

        #region Properties

        #region Public

        #region ItemsSource
        private const string ItemsSourcePropertyName = "ItemsSource";
        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(ItemsSourcePropertyName, typeof(IEnumerable), typeof(AggregateValueSource), new PropertyMetadata(null, (sender, e) =>
            {
                (sender as AggregateValueSource).OnPropertyChanged(ItemsSourcePropertyName, e.OldValue, e.NewValue);
            }));

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                return this.GetValue(ItemsSourceProperty) as IEnumerable;
            }
            set
            {
                this.SetValue(ItemsSourceProperty, value);
            }
        }
        #endregion //ItemsSource

        #region ValueMemberPath
        private const string ValueMemberPathPropertyName = "ValueMemberPath";
        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(ValueMemberPathPropertyName, typeof(string), typeof(AggregateValueSource), new PropertyMetadata(null, (sender, e) =>
        {
            (sender as AggregateValueSource).OnPropertyChanged(ValueMemberPathPropertyName, e.OldValue, e.NewValue);
        }));

        /// <summary>
        /// Gets or sets the value member path.
        /// </summary>
        public string ValueMemberPath
        {
            get
            {
                return this.GetValue(ValueMemberPathProperty) as string;
            }
            set
            {
                this.SetValue(ValueMemberPathProperty, value);
            }
        }

        #endregion //ValueMemberPath

        #endregion //Public

        #region Internal

        #region ValueColumn
        internal IFastItemColumn<double> ValueColumn { get; private set; }
        #endregion //ValueColumn

        #endregion //Internal

        #region Private

        #region FastItemsSource

        private const string FastItemsSourcePropertyName = "FastItemsSource";
        private static readonly DependencyProperty FastItemsSourceProperty = DependencyProperty.Register(FastItemsSourcePropertyName, typeof(FastItemsSource), typeof(AggregateValueSource), new PropertyMetadata(null, (sender, e) =>
        {
            (sender as AggregateValueSource).OnPropertyChanged(FastItemsSourcePropertyName, e.OldValue, e.NewValue);
        }));
        private FastItemsSource FastItemsSource
        {
            get
            {
                return this.GetValue(FastItemsSourceProperty) as FastItemsSource;
            }
            set
            {
                this.SetValue(FastItemsSourceProperty, value);
            }
        }
        #endregion //FastItemsSource

        #endregion //Private

        #endregion //Properties

        #region Events
        
        #region HandleFastItemsSourceEvent
        private void HandleFastItemsSourceEvent(object sender, FastItemsSourceEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion //HandleFastItemsSourceEvent

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised whenever a property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            switch (propertyName)
            {
                case ItemsSourcePropertyName:
                    this.FastItemsSource = new FastItemsSource() { ItemsSource = this.ItemsSource };
                    break;
                case FastItemsSourcePropertyName:
                    FastItemsSource oldFastItemsSource = oldValue as FastItemsSource;
                    if (oldFastItemsSource != null)
                    {
                        oldFastItemsSource.Event -= this.HandleFastItemsSourceEvent;
                        oldFastItemsSource.DeregisterColumn(this.ValueColumn);
                        this.ValueColumn = null;
                    }
                    FastItemsSource newFastItemsSource = newValue as FastItemsSource;
                    if (newFastItemsSource != null)
                    {
                        newFastItemsSource.Event += this.HandleFastItemsSourceEvent;
                        this.ValueColumn = newFastItemsSource.RegisterColumn(this.ValueMemberPath);
                    }
                    break;
                case ValueMemberPathPropertyName:
                    if (this.FastItemsSource != null)
                    {
                        this.FastItemsSource.DeregisterColumn(this.ValueColumn);
                        this.ValueColumn = this.FastItemsSource.RegisterColumn(this.ValueMemberPath);
                    }
                    break;
            }
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }


        }


        #endregion

        #region INotifyCollectionChanged Members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #endregion Events
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