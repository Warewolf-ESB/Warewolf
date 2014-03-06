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
using Infragistics.Controls.Schedules;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{

    /// <summary>
    /// Represents a specific tick mark in a <see cref="TimeslotHeader"/>
    /// </summary>
	[DesignTimeVisible(false)]
	public class TimeslotHeaderTickmark : Control
    {
        #region Constructor
        static TimeslotHeaderTickmark()
        {

			TimeslotHeaderTickmark.DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeslotHeaderTickmark), new FrameworkPropertyMetadata(typeof(TimeslotHeaderTickmark)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(TimeslotHeaderTickmark), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

        }

        /// <summary>
        /// Initializes a new <see cref="TimeslotHeaderTickmark"/>
        /// </summary>
        public TimeslotHeaderTickmark()
        {



        }
        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region Kind

        /// <summary>
        /// Identifies the <see cref="Kind"/> dependency property
        /// </summary>
        public static readonly DependencyProperty KindProperty = DependencyPropertyUtilities.Register(
      "Kind", typeof(TimeslotTickmarkKind), typeof(TimeslotHeaderTickmark),
            DependencyPropertyUtilities.CreateMetadata(TimeslotTickmarkKind.Minor)
            );

        /// <summary>
        /// Determines the kind of tickmark
        /// </summary>
        /// <seealso cref="KindProperty"/>
        public TimeslotTickmarkKind Kind
        {
            get
            {
                return (TimeslotTickmarkKind)this.GetValue(TimeslotHeaderTickmark.KindProperty);
            }
            set
            {
                this.SetValue(TimeslotHeaderTickmark.KindProperty, value);
            }
        }

        #endregion //Kind

        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register(
      "Orientation", typeof(Orientation), typeof(TimeslotHeaderTickmark),
            DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationHorizontalBox)
            );

        /// <summary>
        /// Gets/sets the orientation of the tickmark
        /// </summary>
        /// <seealso cref="OrientationProperty"/>
        public Orientation Orientation
        {
            get
            {
                return (Orientation)this.GetValue(TimeslotHeaderTickmark.OrientationProperty);
            }
            set
            {
                this.SetValue(TimeslotHeaderTickmark.OrientationProperty, value);
            }
        }

        #endregion //Orientation
        
        #endregion //Public Properties	
    
        #endregion //Properties	

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