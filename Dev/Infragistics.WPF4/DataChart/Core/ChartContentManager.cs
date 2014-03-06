using System;
using System.Collections.Generic;



using System.Linq;

using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    [WidgetIgnoreDepends("FragmentBase")]
    [WidgetIgnoreDepends("SplineFragmentBase")]
    [WidgetIgnoreDepends("Axis")]
    internal class ChartContentManager
        : DependencyObject
    {
        [Weak]
        private SeriesViewer _owner = null;

        private Dictionary<ChartContentType, Dictionary<DependencyObject, ContentInfo>> _content = 
            new Dictionary<ChartContentType, Dictionary<DependencyObject, ContentInfo>>();

        /// <summary>
        /// This list maintains the order of the series, where the content dictionary does not.
        /// </summary>


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private List<Series> _seriesList = new List<Series>();
        private List<Series> SeriesList 
        {
            get { return _seriesList; }
            set { _seriesList = value; }
        }


        public ChartContentManager(SeriesViewer owner)
        {
            FirstTime = true;
            FirstMeasure = true;
            _content.Add(ChartContentType.Series, new Dictionary<DependencyObject, ContentInfo>());
            _content.Add(ChartContentType.Axis, new Dictionary<DependencyObject, ContentInfo>());
            _content.Add(ChartContentType.Background, new Dictionary<DependencyObject, ContentInfo>());
            _owner = owner;
        }

        public ContentInfo Subscribe(ChartContentType type, DependencyObject obj, Action<bool> refresh)
        {
            var info = GetInfo(type, obj);
            info.Refresh = refresh;
            return info;
        }

        public void Unsubscribe(ChartContentType type, DependencyObject obj)
        {
            var ofType = _content[type];
            if (ofType.ContainsKey(obj))
            {
                ofType.Remove(obj);
            }

            Series s = obj as Series;






            if (s != null && SeriesList.Contains(s))
            {
                SeriesList.Remove(s);
            }

        }

        private bool _pending = false;

        private void MakePending()
        {
            if (!_pending)
            {
                _pending = true;






                Dispatcher.BeginInvoke(new ThreadStart(DoRefresh), null);


            }
        }

        public void Refresh(ChartContentType type, DependencyObject obj, ContentInfo info, bool animate)
        {
            var contentInfo = info;
            if (!contentInfo.IsDirty)
            {
                contentInfo.DoAnimation = animate;
            }
            else
            {
                if (!animate)
                {
                    contentInfo.DoAnimation = false;
                }
            }

            if (!contentInfo.IsDirty)
            {
                contentInfo.IsDirty = true;
                //RefreshPreview(false);
                MakePending();
            }
        }

        private bool PreviewRefreshPending { get; set; }

//        public void RefreshPreview(bool immediate)
//        {
//            if (!immediate)
//            {
//                if (!this.PreviewRefreshPending)
//                {
//                    this.PreviewRefreshPending = true;
//#if TINYCLR
//                    System.Html.Window.SetTimeout(() => RefreshPreview(true), 0);
//#else
//                    this.Dispatcher.BeginInvoke(new Action<bool>(RefreshPreview), true);
//#endif
//                }

//                return;
//            }

//            this.PreviewRefreshPending = false;

//            XamOverviewPlusDetailPane opd = _owner.OverviewPlusDetailPane;
//            if (opd == null)
//            {
//                return;
//            }

//            SeriesViewerSurfaceViewer  surfaceViewer = _owner.OverviewPlusDetailPane.SurfaceViewer as SeriesViewerSurfaceViewer;
//            if (surfaceViewer != null)
//            {
//                surfaceViewer.UpdatePreview();
//            }
//        }

        private ContentInfo GetInfo(ChartContentType type, DependencyObject obj)
        {
            var ofType = _content[type];
            ContentInfo info = null;
            if (!ofType.TryGetValue(obj, out info))
            {
                info = new ContentInfo();
                info.Content = obj;
                ofType.Add(obj, info);

                Series s = obj as Series;



                if (s != null && !SeriesList.Contains(s))

                {

                    if (s is FragmentBase || s is SplineFragmentBase)
                    {
                        FragmentBase fragmentSeries = s as FragmentBase;
                        SplineFragmentBase splineFragmentSeries = s as SplineFragmentBase;
                        StackedSeriesBase parentSeries = fragmentSeries != null ? fragmentSeries.ParentSeries : splineFragmentSeries.ParentSeries;
                        StackedFragmentSeries logicalSeries = fragmentSeries != null ? fragmentSeries.LogicalSeriesLink : splineFragmentSeries.LogicalSeriesLink;
                        int index = 0;

                        //when the chart has stacked series in it, the first series in the list is the parent series.
                        //we need to skip over it and start the index at 1.
                        if (SeriesList.IndexOf(parentSeries) == 0) index++;
                        index += parentSeries.Series.IndexOf(logicalSeries);

                        if (SeriesList.Count <= index 
                            || parentSeries.Series.Count == 0 
                            || index == -1)
                        {
                            SeriesList.Add(s);
                        }
                        else
                        {
                            SeriesList.Insert(index, s);
                        }
                    }
                    else

                    {



                        SeriesList.Add(s);

                    }
                }
            }

            return info;
        }

        public void Force()
        {
            //System.Diagnostics.Debug.WriteLine("forcing");
            DoRefresh();
            //System.Diagnostics.Debug.WriteLine("done forcing " + _pending.ToString());
        }

        private void DoRefresh()
        {
            if (_owner == null || _content == null)
            {
                return;
            }

            if (!_pending)
            {
                return;
            }
            _pending = false;
            foreach (var item in InOrder())
            {
                item.DoRefresh();
            }

            if (!_pending)
            {
                _owner.RaiseRefreshCompleted();
            }
        }

        private IEnumerable<ContentInfo> InOrder()
        {
            Dictionary<DependencyObject, ContentInfo> ofType = null;
            
            ofType = _content[ChartContentType.Background];
            foreach (var item in ofType.Values)
            {
                yield return item;
            }
            
            ofType = _content[ChartContentType.Axis];
            foreach (var item in ofType.Values)
            {
                yield return item;
            }

            ofType = _content[ChartContentType.Series];

            List<ContentInfo> contentList = new List<ContentInfo>();





            foreach (var series in SeriesList)
            {

                contentList.Add(ofType[series]);
            }

            foreach (var item in contentList)
            {
                yield return item;
            }
        }

        public void EnsureAxesRendered(Size plotSize)
        {
            foreach (var info in _content[ChartContentType.Axis].Values)
            {
                (info.Content as Axis).OverrideViewport();
            }
            foreach (var info in _content[ChartContentType.Axis].Values)
            {
                info.DoRefresh();
            }
            foreach (var info in _content[ChartContentType.Axis].Values)
            {
                (info.Content as Axis).ViewportOverride = Rect.Empty;
            }
        }

        public bool FirstMeasure { get; set; }
        public bool FirstTime { get; set; }

        internal void ViewportChanged(ChartContentType chartContentType, DependencyObject obj, ContentInfo info, Rect newViewportRect)
        {
            info.Viewport = newViewportRect;
            //System.Diagnostics.Debug.WriteLine("viewport changed: " + info.Viewport.Width.ToString() + "," + info.Viewport.Height.ToString());


            if (FirstTime)
            {
                var invalidCount = InOrder().Where((i) => !i.IsViewportValid).Count();
                if (invalidCount == 0)
                {
                    FirstTime = false;
                    Force();
                }
            }

        }

        internal void RangeDirty(Axis axis, ContentInfo info)
        {
            if (!info.RangeDirty)
            {
                info.RangeDirty = true;
                MakePending();
            }
        }
    }

    internal class ContentInfo
    {
        public DependencyObject Content { get; set; }
        public Action<bool> Refresh { get; set; }
        public bool DoAnimation { get; set; }
        public bool IsDirty { get; set; }
        public Rect Viewport { get; set; }
        public bool IsViewportValid { get 
        {
            if (Viewport.IsEmpty)
            {
                return false;
            }

            if (Viewport.Width == 0 && Viewport.Height == 0)
            {
                return false;
            }

            return true;
        }}

        public void UndirtyRange()
        {
            if (RangeDirty && Content is Axis)
            {
                RangeDirty = false;
                bool wasDirty = IsDirty;
                IsDirty = true;
                bool ret = (Content as Axis).UpdateRange(true);
                if (!ret)
                {
                    IsDirty = wasDirty;
                }
            }
        }

        public void DoRefresh()
        {
            UndirtyRange();

            
            //were refreshed, but this is the more targeted fix.
            if (Content is Axis)
            {
                var axis = Content as Axis;
                if (axis.CrossingAxis != null)
                {
                    var crossing = axis.CrossingAxis;
                    if (crossing.ContentInfo != null && crossing.ContentInfo.RangeDirty)
                    {
                        crossing.ContentInfo.UndirtyRange();
                    }
                }
            }

            if (IsDirty)
            {
                IsDirty = false;
                Refresh(DoAnimation);
                DoAnimation = false;
            }
        }

        public bool RangeDirty { get; set; }
    }

    internal enum ChartContentType
    {
        Series = 0,
        Axis = 1,
        Background = 2
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