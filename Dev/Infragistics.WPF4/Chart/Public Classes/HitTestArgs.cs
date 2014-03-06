
#region Using

using System;
using System.Collections.Generic;
using System.Text;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class contains return arguments of the hit test function.
    /// </summary>
    public class HitTestArgs
    {
        #region Fields

        // Private fields
        private object _selectedObject;
        private Chart _chart;
        private int _seriesIndex;
        private int _pointIndex;
        private double _xValue = double.NaN;
        private double _yValue = double.NaN;
        private double _xValueSecondary = double.NaN;
        private double _yValueSecondary = double.NaN;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets index of a selected data point.
        /// </summary>
        public int PointIndex
        {
            get { return _pointIndex; }
            set { _pointIndex = value; }
        }

        /// <summary>
        /// Gets or sets value which indicates that hit test is enabled.
        /// </summary>
        public bool Enabled
        {
            get 
            {
                if (_chart != null)
                {
                    return _chart.HitTestInfoArray.Count > 0; 
                }
                return false;
            }
        }

        /// <summary>
        /// Gets or sets index of a series which contains 
        /// a selected data point.
        /// </summary>
        public int SeriesIndex
        {
            get { return _seriesIndex; }
            set { _seriesIndex = value; }
        }

        /// <summary>
        /// Gets or sets a chart which contains a selected data point.
        /// </summary>
        internal Chart Chart
        {
            get { return _chart; }
            set { _chart = value; }
        }

        /// <summary>
        /// Gets or sets a selected object
        /// </summary>
        public object SelectedObject
        {
            get { return _selectedObject; }
            set { _selectedObject = value; }
        }

        /// <summary>
        /// Gets or sets an X axis value where the mouse pointer is located.
        /// </summary>
        public double XValue
        {
            get { return _xValue; }
            set { _xValue = value; }
        }

        /// <summary>
        /// Gets or sets a Y axis value where the mouse pointer is located.
        /// </summary>
        public double YValue
        {
            get { return _yValue; }
            set { _yValue = value; }
        }

        /// <summary>
        /// Gets or sets an X Secondary axis value where the mouse pointer is located.
        /// </summary>
        public double XValueSecondary
        {
            get { return _xValueSecondary; }
            set { _xValueSecondary = value; }
        }

        /// <summary>
        /// Gets or sets a Y Secondary axis value where the mouse pointer is located.
        /// </summary>
        public double YValueSecondary
        {
            get { return _yValueSecondary; }
            set { _yValueSecondary = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the HitTestArgs class. 
        /// </summary>
        public HitTestArgs()
        {
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