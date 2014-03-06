using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Windows.Design;

namespace Infragistics.Windows.Design.OutlookBar
{
    /// <summary>
    /// Interaction logic for animated SmartButton control
    /// </summary>
	[ToolboxBrowsable(false)]	// JM 10-20-08
	public partial class SmartButton : UserControl
    {
        #region Member Variables

        private bool _isRightArrow = false; //the arrow points the right side (the pop up is closed)
        private readonly double _angleFrom = 0;
        private readonly double _angleTo = -180;
        private readonly double _duration = 300; //milliseconds

        #endregion //Member Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="SmartButton"/>
        /// </summary>
        public SmartButton()
        {
            InitializeComponent();
        }

        #endregion //Constructor
        
        #region Properties

        #region Public Properties

        /// <summary>
        /// Returns/sets a value that indicates if the arrow points to the right
        /// </summary>
        public bool IsRightArrow
        {
            get
            {
                return _isRightArrow;
            }

            set
            {
                double aFrom = 0;
                double aTo = 0;

                if (_isRightArrow)
                {
                    aFrom = _angleTo;
                    aTo = _angleFrom;
                }
                else
                {
                    aFrom = _angleFrom;
                    aTo = _angleTo;
                }
                _isRightArrow = value;

                DoubleAnimation da;
                da = new DoubleAnimation(aFrom, aTo, new Duration(TimeSpan.FromMilliseconds(_duration)));
                RotateTransform rt = new RotateTransform();
                btnSmart.RenderTransform = rt;
                btnSmart.RenderTransformOrigin = new Point(0.5, 0.5);
                rt.BeginAnimation(RotateTransform.AngleProperty, da);
            }
        }

        #endregion //Public Properties

        #endregion //Properties	

        #region Event Handlers

        private void btnSmart_Click(object sender, RoutedEventArgs e)
        {
            if (_isRightArrow)
            {
                this.IsRightArrow = false;
            }
            else
            {
                this.IsRightArrow = true;
            }
        }

        #endregion //Event Handlers	    
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