
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Represent a data collection which containes axis objects.
    /// </summary>
    public class AxisCollection : ObservableCollection<Axis>
    {
        #region Fields

        // Private fields
        private object _chartParent;

        #endregion Fields

        #region Internal Properties

        /// <summary>
        /// The parent object
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
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

        #endregion Internal Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the AxisCollection class. 
        /// </summary>
        public AxisCollection()
        {
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index. 
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, Axis item)
        {
            item.ChartParent = this;
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Raises the CollectionChanged event with the provided arguments. 
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Set Parent
            if (e.NewItems != null)
            {
                foreach (Axis item in e.NewItems)
                {
                    if (item.ChartParent == null)
                    {
                        item.ChartParent = this;
                    }
                }
            }

            // Refresh chart
            XamChart control = XamChart.GetControl(this);
            if (control != null)
            {
                control.RefreshProperty();
            }
        }

        internal Axis GetAxis(AxisType type)
        {
            // Check the number of axis with this axis type
            int numOfAxis = 0;
            foreach (Axis axis in this)
            {
                if (axis.AxisType == type)
                {
                    numOfAxis++;
                }
            }

            if (numOfAxis > 1)
            {
                // The bubble chart needs ValueX, ValueY and Radius. The values have to be set for every data point using ChartParameters collection.
                throw new InvalidOperationException(ErrorString.Exc63 + type.ToString());
            }

            foreach (Axis axis in this)
            {
                if (axis.AxisType == type)
                {
                    return axis;
                }
            }

            switch (type)
            {
                case AxisType.PrimaryX:
                    return XamChart.GetControl(this).DefaultAxisX;
                case AxisType.PrimaryY:
                    return XamChart.GetControl(this).DefaultAxisY;
                case AxisType.PrimaryZ:
                    return XamChart.GetControl(this).DefaultAxisZ;
            }

            return null;
        }

        /// <summary>
        /// Checks if Axis are Visible. 
        /// </summary>
        /// <param name="type">Axis type</param>
        /// <returns>True if axis are visible</returns>
        internal bool IsAxisVisible(AxisType type)
        {
            foreach (Axis axis in this)
            {
                if (axis.AxisType == type )
                {
                    if (axis.Visible)
                    {
                        if (axis.Label != null && !axis.Label.Visible)
                        {
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (type == AxisType.PrimaryX || type == AxisType.PrimaryY || type == AxisType.PrimaryZ)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find Major/Minor Gridlines/tickmarks.
        /// </summary>
        /// <param name="type">Axis type</param>
        /// <param name="isGrid">True if grid is requested.</param>
        /// <param name="isMajor">True if major gridlines or tickmarks are reqested.</param>
        /// <returns>Reqested Mark</returns>
        internal Mark GetMark(AxisType type, bool isGrid, bool isMajor)
        {
            Axis axis = GetAxis(type);
            Mark mark = null;

            if (axis != null)
            {
                if (isGrid)
                {
                    if (isMajor)
                    {
                        mark = axis.MajorGridline;
                    }
                    else
                    {
                        mark = axis.MinorGridline;
                    }
                }
                else
                {
                    if (isMajor)
                    {
                        mark = axis.MajorTickMark;
                    }
                    else
                    {
                        mark = axis.MinorTickMark;
                    }
                }
            }

            return mark;
        }

        #endregion Methods
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