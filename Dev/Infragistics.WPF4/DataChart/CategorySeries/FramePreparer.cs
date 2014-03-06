using System;
using System.Collections.Generic;



using System.Linq;

using System.Text;
using System.Windows;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    internal interface IProvidesViewport
    {
        void GetViewInfo(out Rect viewportRect, out Rect windowRect);
    }

    internal interface ISupportsMarkers
    {
        bool ShouldDisplayMarkers { get; }
        void UpdateMarkerCount(int markerCount);
        void UpdateMarkerTemplate(int markerCount, int itemIndex);
    }

    internal interface ISupportsErrorBars
    {

        bool ShouldDisplayErrorBars();
        bool ShouldSyncErrorBarsWithMarkers();
        ErrorBarSettingsBase ErrorBarSettings { get; }





        Axis XAxis { get; }
        Axis YAxis { get; }

    }

    internal class DefaultSupportsMarkers
        : ISupportsMarkers
    {
        public bool ShouldDisplayMarkers
        {
            get { return false; }
        }

        public void UpdateMarkerCount(int markerCount)
        {
        }

        public void UpdateMarkerTemplate(int markerCount, int itemIndex)
        {
        }
    }

    internal class DefaultProvidesViewport
        : IProvidesViewport
    {
        public void GetViewInfo(out Rect viewportRect, out Rect windowRect)
        {
            viewportRect = Rect.Empty;
            windowRect = Rect.Empty;
        }
    }

    internal class DefaultSupportsErrorBars
        : ISupportsErrorBars
    {

        bool ISupportsErrorBars.ShouldDisplayErrorBars()
        {
            return false;
        }

        bool ISupportsErrorBars.ShouldSyncErrorBarsWithMarkers()
        {
            return false;
        }


        ErrorBarSettingsBase ISupportsErrorBars.ErrorBarSettings
        {
            get
            {
                return null;
            }
        }




#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        Axis ISupportsErrorBars.XAxis
        {
            get
            {
                return null;
            }
        }

        Axis ISupportsErrorBars.YAxis
        {
            get
            {
                return null;
            }
        }

    }

    internal abstract class FramePreparer
    {





        protected FramePreparer(object host) : this(host as ISupportsMarkers, host as IProvidesViewport, host as ISupportsErrorBars)
        {
            
        }
        protected FramePreparer(ISupportsMarkers markersHost, IProvidesViewport viewportHost, ISupportsErrorBars errorBarsHost)
        {





            MarkersHost = new DefaultSupportsMarkers();
            ViewportHost = new DefaultProvidesViewport();
            ErrorBarsHost = new DefaultSupportsErrorBars();


            if (markersHost != null)
            {
                MarkersHost = markersHost;
            }
            if (viewportHost != null)
            {
                ViewportHost = viewportHost;
            }
            if (errorBarsHost != null)
            {
                ErrorBarsHost = errorBarsHost;
            }
        }

        [Weak]
        protected internal ISupportsErrorBars ErrorBarsHost { get; set; }
        [Weak]
        protected internal ISupportsMarkers MarkersHost { get; set; }
        [Weak]
        protected internal IProvidesViewport ViewportHost { get; set; }
        
        public abstract void PrepareFrame(Frame frame, SeriesView view); 
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