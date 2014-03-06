using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a base class for brush scales.
    /// </summary>
    [DontObfuscate]
    [WidgetModule("ScatterChart")]
    public class BrushScale:DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Creates a new instance of BrushScale class.
        /// </summary>
        public BrushScale()
        {
            Series = new List<Series>();
            Brushes = new BrushCollection();
            // this is redundant.  the Brushes setter attaches the same event handler... which probably means it's getting called twice.
            Brushes.CollectionChanged += Brushes_CollectionChanged;
            PropertyUpdated += (o, e) => PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Called when the members of the brushes collection change.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void Brushes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var series in Series)
            {
                series.RenderSeries(false);
                series.NotifyThumbnailAppearanceChanged();
            }
        }

        /// <summary>
        /// Gets the brushes collection used by this scale.
        /// </summary>
        public BrushCollection Brushes 
        {
            get { return _brushes; }
            set
            {
                if (_brushes != null) _brushes.CollectionChanged -= Brushes_CollectionChanged;
                _brushes=value;
                if (_brushes != null) _brushes.CollectionChanged += Brushes_CollectionChanged;

                // this is redundant.  the PropertyUpdatedOverride method has the same block.
                foreach (var series in Series)
                {
                    series.RenderSeries(false);
                    series.NotifyThumbnailAppearanceChanged();
                }
            }
        }
        private BrushCollection _brushes = null;

        internal List<Series> Series { get; set; }

        /// <summary>
        /// Gets a brush from the brushes collection by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Brush for a given index.</returns>
        public virtual Brush GetBrush(int index)
        {
            if (Brushes == null || index < 0 || index >= Brushes.Count)
            {
                return null;
            }

            return Brushes[index];
        }

        /// <summary>
        /// Returns an interpolated brush value based on index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>The interpolated brush.</returns>
        protected internal Brush GetInterpolatedBrush(double index)
        {
            if (Brushes == null || Brushes.Count == 0 || index < 0)
            {
                return null;
            }




            return Brushes[index];

        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a property (including "effective" and non-dependency property) value changes.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises the property changed and updated events.
        /// </summary>
        /// <param name="name">The name of the property being changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string name, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(name, oldValue, newValue));
            }
        }
        #endregion

        #region PropertyUpdated Handler
        /// <summary>
        /// Called when a property is updated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            foreach (var series in Series)
            {
                series.RenderSeries(false);
                series.NotifyThumbnailAppearanceChanged();
            }
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