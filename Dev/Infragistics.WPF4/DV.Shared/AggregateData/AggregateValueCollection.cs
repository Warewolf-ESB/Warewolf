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
using System.Collections.ObjectModel;

namespace Infragistics.Collections
{
    /// <summary>
    /// Represents a collection of AggregateValueSource
    /// </summary>
    public class AggregateValueCollection : DependencyObject, IEnumerable, INotifyCollectionChanged
    {
        #region Contructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateValueCollection"/> class.
        /// </summary>
        public AggregateValueCollection()
            : base()
        {
            this.ItemsSources = new ObservableCollection<AggregateValueSource>();
            this.ItemsSources.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ItemSourcesCollectionChanged);
        }
        #endregion //Contructor

        #region Properties

        #region Public

        #region ItemsSources
        /// <summary>
        /// Gets or sets the items sources.
        /// </summary>
        /// <value>The items sources.</value>
        public ObservableCollection<AggregateValueSource> ItemsSources { get; set; }
        #endregion //ItemsSources

        #endregion Public

        #endregion //Properties

        #region Methods

        #region Private

        #region ItemSourcesCollectionChanged
        private void ItemSourcesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object oldItem in e.OldItems)
                {
                    AggregateValueSource oldValueSource = oldItem as AggregateValueSource;
                    oldValueSource.CollectionChanged -= new NotifyCollectionChangedEventHandler(ValueSourceCollectionChanged);
                }
            }
            if (e.NewItems != null)
            {
                foreach (object newItem in e.NewItems)
                {
                    AggregateValueSource newValueSource = newItem as AggregateValueSource;
                    newValueSource.CollectionChanged += new NotifyCollectionChangedEventHandler(ValueSourceCollectionChanged);
                }
            }

            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion //ItemSourcesCollectionChanged

        #region ValueSourceCollectionChanged
        private void ValueSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #endregion //ValueSourceCollectionChanged

        #endregion //Private

        #endregion //Methods

        #region INotifyCollectionChanged Members
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Gets the enumerator for iterating through the collection.
        /// </summary>
        /// <returns>An AggregateValueEnumerator for this AggregateValueCollection.</returns>
        public IEnumerator GetEnumerator()
        {
            return new AggregateValueEnumerator(this);
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