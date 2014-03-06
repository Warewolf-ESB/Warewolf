using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Windows.Internal
{
    // JJD 8/20/08 - BR35341
    // Added abstract base class to handle adornerlayer re-creations based on template changes from higher level elements 
    /// <summary>
    /// Abstract class for internal use only
    /// </summary>
	[System.ComponentModel.DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!! (AS 5/27/10)
	public abstract class AdornerEx : Adorner
    {
        #region Constructor

        /// <summary>
        /// Creates an instance of an AdornerEx derived class
        /// </summary>
        /// <param name="adornedElement"></param>
        protected AdornerEx(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            // Wire up the Unloaded event the adorned element
            this.WireLoadedUnloaded(false);
        }

        #endregion //Constructor	
    
        #region Methods

            #region OnAdornedElementLoaded

        private void OnAdornedElementLoaded(object sender, RoutedEventArgs e)
        {
            // Unhook the Loaded event
            ((FrameworkElement)this.AdornedElement).Loaded -= new RoutedEventHandler(OnAdornedElementLoaded);

            // wire up the new layer
            this.VerifyAdornerLayer(true);
        }

            #endregion //OnAdornerLayerUnloaded

            #region OnAdornerLayerUnloaded

        private void OnAdornerLayerUnloaded(object sender, RoutedEventArgs e)
        {
            // wire up the new layer
            this.VerifyAdornerLayer(false);
        }

            #endregion //OnAdornerLayerUnloaded

            #region VerifyAdornerLayer

        private void VerifyAdornerLayer(bool calledFromLoaded)
        {
            AdornerLayer oldParent = VisualTreeHelper.GetParent(this) as AdornerLayer;

            if (oldParent != null)
            {
                // if our parent is the same as the old adorned element then return
                if (oldParent == AdornerLayer.GetAdornerLayer(this.AdornedElement))
                {
                    // Wire up the Unloaded event of the adorned element if we are called from
                    // the Loaded event handler
                    if (calledFromLoaded)
                        ((FrameworkElement)this.AdornedElement).Unloaded += new RoutedEventHandler(OnAdornerLayerUnloaded);

                    return;
                }

                // Unwire the event since it will get rewired below
                ((FrameworkElement)this.AdornedElement).Unloaded -= new RoutedEventHandler(OnAdornerLayerUnloaded);

                // remove this element from the old layer
                oldParent.Remove(this);
            }

            // wire up the new layer
            this.WireLoadedUnloaded(true);
        }

            #endregion //VerifyAdornerLayer

            #region WireLoadedUnloaded

        private void WireLoadedUnloaded(bool addToLayer)
        {
            // get the adorned element's adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);

            if (adornerLayer != null)
            {
                // wire up the Unloaded event

                if (addToLayer == false && ((FrameworkElement)this.AdornedElement).IsLoaded == false)
                    ((FrameworkElement)this.AdornedElement).Loaded += new RoutedEventHandler(OnAdornedElementLoaded);
                else
                    ((FrameworkElement)this.AdornedElement).Unloaded += new RoutedEventHandler(OnAdornerLayerUnloaded);

                // Add this adorner to the layer if requested
                if (addToLayer && adornerLayer != VisualTreeHelper.GetParent(this))
                    adornerLayer.Add(this);
            }
        }

            #endregion //WireLoadedUnloaded

        #endregion //Methods

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