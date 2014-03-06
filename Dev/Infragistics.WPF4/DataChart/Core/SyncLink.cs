using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Infragistics.Controls.Charts.Util;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// SyncLink class.
    /// </summary>
    public class SyncLink : FrameworkElement, INotifyPropertyChanged, IFastItemsSourceProvider
    {
        #region Constructor and Initialisation

        /// <summary>
        /// Initializes a new instance of the SyncLink class.
        /// </summary>
        public SyncLink()
        {
            PropertyUpdated += (o, e) => { PropertyUpdatedOverride(o, e.PropertyName, e.OldValue, e.NewValue); };

            this.ChartsInternal = new ChartCollection();
            this.ChartsInternal.CollectionChanged += new NotifyCollectionChangedEventHandler(Charts_CollectionChanged);

            DefaultWindowRect = new Rect(0.0, 0.0, 1.0, 1.0);
        }

        #endregion Constructor and Initialisation

        /// <summary>
        /// The name of the associated synchronization channel.
        /// </summary>
        public string SyncChannel { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets called whenever a property value is updated
        /// on the series or owning chart. Gives the series a chance to respond to the various property updates.
        /// </summary>
        /// <param name="sender">The object being updated.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected virtual void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
        }

        
        FastItemsSource IFastItemsSourceProvider.ReleaseFastItemsSource(



            IEnumerable 

            itemsSource)
        {
            if (itemsSource != null)
            {
                FastItemsSourceReference itemsSourceReference = null;

                if (fastItemsSources.TryGetValue(itemsSource, out itemsSourceReference))
                {
                    --itemsSourceReference.References;

                    if (itemsSourceReference.References == 0)
                    {
                        fastItemsSources.Remove(itemsSource);
                    }
                }
            }

            return null;
        }

        FastItemsSource IFastItemsSourceProvider.GetFastItemsSource(
            


            IEnumerable 

            itemsSource)
        {
            FastItemsSource fastItemsSource = null;

            if (itemsSource != null)
            {
                FastItemsSourceReference itemsSourceReference = null;

                if (!fastItemsSources.TryGetValue(itemsSource, out itemsSourceReference))
                {
                    fastItemsSource = new FastItemsSource() { ItemsSource = itemsSource };
                    itemsSourceReference = new FastItemsSourceReference(fastItemsSource);

                    fastItemsSources.Add(itemsSource, itemsSourceReference);
                }

                itemsSourceReference.References++;
                fastItemsSource = itemsSourceReference.FastItemsSource;
            }

            return fastItemsSource;
        }

        /// <summary>
        /// Returns the fast items source for the given items source.
        /// </summary>
        /// <param name="itemsSource">The items source to get the fast items source for.</param>
        /// <returns>The fast items source, if found.</returns>
        public FastItemsSource PeekItemsSource(



            IEnumerable 

            itemsSource)
        {
            FastItemsSource fastItemsSource = null;

            if (itemsSource != null)
            {
                FastItemsSourceReference itemsSourceReference = null;

                if (!fastItemsSources.TryGetValue(itemsSource, out itemsSourceReference))
                {
                    return null;
                }

                fastItemsSource = itemsSourceReference.FastItemsSource;
            }

            return fastItemsSource;
        }

        private Dictionary<IEnumerable, FastItemsSourceReference> fastItemsSources = new Dictionary<IEnumerable, FastItemsSourceReference>();

        /// <summary>
        /// The default window rect is applied to chart areas as they're added to the chart.
        /// </summary>
        internal Rect DefaultWindowRect
        {
            get;
            private set;
        }

        /// <summary>
        /// Calculate a rectangle for a chart area by synchronizing according to the
        /// chart area's window settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chart"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        private Rect SynchroniseRect(SeriesViewer sender, SeriesViewer chart, Rect rect)
        {
            if (!rect.IsEmpty)
            {
                double cx = 0.5 * (rect.Left + rect.Right);
                double cy = 0.5 * (rect.Top + rect.Bottom);

                double minWidth = sender.WindowRectMinWidth;

                if (chart == sender)
                {
                    if (!chart.HorizontalZoomable)
                    {
                        rect.X = chart.WindowRect.X;
                        rect.Width = chart.WindowRect.Width;
                    }

                    if (!chart.VerticalZoomable)
                    {
                        rect.Y = chart.WindowRect.Y;
                        rect.Height = chart.WindowRect.Height;
                    }
                }
                else
                {
                    SyncSettings settings = SyncManager.GetSyncSettings(chart);
                    if (settings == null ||
                        !settings.SynchronizeHorizontally)
                    {
                        rect.X = chart.WindowRect.X;
                        rect.Width = chart.WindowRect.Width;
                    }

                    if (settings == null ||
                        !settings.SynchronizeVertically)
                    {
                        rect.Y = chart.WindowRect.Y;
                        rect.Height = chart.WindowRect.Height;
                    }
                }

                double width = MathUtil.Clamp(rect.Width, minWidth, 1.0);
                double height = MathUtil.Clamp(rect.Height, minWidth, 1.0);

                if (sender.UseFixedAspectZoom() && !sender.ViewportRect.IsEmpty)
                {
                    Rect viewport = sender.ViewportRect;
                    double matchAspect = viewport.Width / viewport.Height;
                    double projectedWidth = width * viewport.Width;
                    double projectedHeight = height * viewport.Height;
                    double projectedAspect = projectedWidth / projectedHeight;

                    if (projectedAspect != matchAspect)
                    {
                        if (projectedHeight * matchAspect > projectedWidth)
                        {
                            projectedWidth = projectedHeight * matchAspect;
                            width = projectedWidth / viewport.Width;
                        }
                        else 
                        {
                            projectedHeight = projectedWidth / matchAspect;
                            height = projectedHeight / viewport.Height;
                        }
                    }
                }
                else if (sender.EffectiveIsSquare())
                {
                    bool widthChanging = false;
                    bool heightChanging = false;
                    if (Math.Abs(width - sender.ActualWindowRect.Width) > minWidth)
                    {
                        widthChanging = true;
                    }
                    if (Math.Abs(height - sender.ActualWindowRect.Height) > minWidth)
                    {
                        heightChanging = true;
                    }
                    sender.MatchRatio(ref width, ref height, widthChanging, heightChanging);

                    width = MathUtil.Clamp(width, minWidth, 1.0);
                    height = MathUtil.Clamp(height, minWidth, 1.0);
                }            

                double left = cx - 0.5 * width;
                double top = cy - 0.5 * height;
                double right = cx + 0.5 * width;
                double bottom = cy + 0.5 * height;

                if (left < 0)
                {
                    left = 0;
                    right = left + width;
                }

                if (right > 1)
                {
                    right = 1;
                    left = right - width;
                }

                if (top < 0)
                {
                    top = 0;
                    bottom = top + height;
                }

                if (bottom > 1)
                {
                    bottom = 1;
                    top = bottom - height;
                }

                rect = new Rect(left, top, right - left, bottom - top);
            }

            return rect;
        }

        /// <summary>
        /// Gets a collection of chart areas known to the current chart.
        /// </summary>
        /// <remarks>
        /// The chart areas property is maintained intenally and should not be modified by users.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]



        public IList<SeriesViewer> Charts

        {
            get { return this.ChartsInternal; }
        }

        internal ChartCollection ChartsInternal { get; set; }

        private void Charts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {





                foreach (SeriesViewer chart in e.NewItems)
                {

                    //changing to only synchronize new charts in the group if they have
                    //no window rect.
                    if (chart.WindowRect.IsEmpty)
                    {
                        chart.WindowRect = SynchroniseRect(null, chart, DefaultWindowRect);
                    }
                }
            }
        }

        internal void CrosshairNotify(SeriesViewer sender, Point crossHairPoint)
        {





            foreach (SeriesViewer chart in Charts)
            {

                if (chart == sender)
                {
                    chart.CrosshairPoint = crossHairPoint;
                }
                else
                {
                    Point pt = crossHairPoint;
                    SyncSettings settings = SyncManager.GetSyncSettings(chart);

                    if (settings == null ||
                        !settings.SynchronizeHorizontally)
                    {
                        pt.X = double.NaN;
                    }

                    if (settings == null ||
                        !settings.SynchronizeVertically)
                    {
                        pt.Y = double.NaN;
                    }

                    chart.CrosshairPoint = pt;
                }
            }
        }

        /// <summary>
        /// Called internally by chart areas to request a window preview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="previewRect"></param>
        internal void PreviewNotify(SeriesViewer sender, Rect previewRect)
        {





            foreach (SeriesViewer chart in Charts)
            {

                chart.PreviewRect = SynchroniseRect(sender, chart, previewRect);
            }
        }

        /// <summary>
        /// Called internally by chart areas to request a window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowRect"></param>
        internal void WindowNotify(SeriesViewer sender, Rect windowRect)
        {
            bool invalid = windowRect.IsEmpty || double.IsNaN(windowRect.X) || double.IsNaN(windowRect.Y) || double.IsNaN(windowRect.Width) || double.IsNaN(windowRect.Height);
            Debug.Assert(!invalid, "SyncLink attempted to apply an invalid WindowRect");
            if (!invalid && !sender.DontNotify)
            {





                foreach (SeriesViewer chart in Charts)
                {

                    //avoid recursing notifications.
                    chart.DontNotify = true;
                    chart.WindowRect = SynchroniseRect(sender, chart, windowRect);
                    chart.DontNotify = false;
                }
            }
        }

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Notifies clients that a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies clients that a property value has changed.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
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

        #endregion INotifyPropertyChanged Implementation
    }

    internal class FastItemsSourceReference
    {
        public FastItemsSourceReference(FastItemsSource fastItemsSource)
        {
            FastItemsSource = fastItemsSource;
            References = 0;
        }
        public FastItemsSource FastItemsSource;
        public int References;
    }

    /// <summary>
    /// Represents the synchronization settings for a chart including which synchronization channel it is
    /// part of.
    /// </summary>
    [TypeConverter(typeof(SyncSettingsConverter))]

    [DesignTimeVisible(false)]

    [DontObfuscate]
    public class SyncSettings
        : FrameworkElement, INotifyPropertyChanged
    {
        #region SyncChannel Property

        /// <summary>
        /// Gets or sets the channel with which to synchronize.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public string SyncChannel
        {
            get
            {
                return (string)GetValue(SyncChannelProperty);
            }
            set
            {
                SetValue(SyncChannelProperty, value);
            }
        }

        internal const string SyncChannelPropertyName = "SyncChannel";

        /// <summary>
        /// Identifies the SyncChannel dependency property.
        /// </summary>
        public static readonly DependencyProperty SyncChannelProperty = DependencyProperty.Register(
            SyncChannelPropertyName, typeof(string), typeof(SyncSettings),
            new PropertyMetadata(null,
                (o, e) =>
                {
                    (o as SyncSettings).RaisePropertyChanged(SyncChannelPropertyName, e.OldValue, e.NewValue);
                }));

        #endregion SyncChannel Property

        #region SynchronizeVertically Property

        /// <summary>
        /// Gets or sets the bool used to display the window preview shadow.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool SynchronizeVertically
        {
            get
            {
                return (bool)GetValue(SynchronizeVerticallyProperty);
            }
            set
            {
                SetValue(SynchronizeVerticallyProperty, value);
            }
        }

        internal const string SynchronizeVerticallyPropertyName = "SynchronizeVertically";

        /// <summary>
        /// Identifies the SynchronizeVertically dependency property.
        /// </summary>
        public static readonly DependencyProperty SynchronizeVerticallyProperty = DependencyProperty.Register(
            SynchronizeVerticallyPropertyName, typeof(bool), typeof(SyncSettings),
            new PropertyMetadata(true,
                 (o, e) =>
                 {
                     (o as SyncSettings).RaisePropertyChanged(SynchronizeVerticallyPropertyName, e.OldValue, e.NewValue);
                 }));

        #endregion SynchronizeVertically Property

        #region SynchronizeHorizontally Property

        /// <summary>
        /// Gets or sets the bool used to display the window preview shadow.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public bool SynchronizeHorizontally
        {
            get
            {
                return (bool)GetValue(SynchronizeHorizontallyProperty);
            }
            set
            {
                SetValue(SynchronizeHorizontallyProperty, value);
            }
        }

        internal const string SynchronizeHorizontallyPropertyName = "SynchronizeHorizontally";

        /// <summary>
        /// Identifies the SynchronizeHorizontally dependency property.
        /// </summary>
        public static readonly DependencyProperty SynchronizeHorizontallyProperty = DependencyProperty.Register(
            SynchronizeHorizontallyPropertyName, typeof(bool), typeof(SyncSettings),
            new PropertyMetadata(true,
                 (o, e) =>
                 {
                     (o as SyncSettings).RaisePropertyChanged(SynchronizeHorizontallyPropertyName, e.OldValue, e.NewValue);
                 }));

        #endregion SynchronizeHorizontally Property

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        protected void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            if (PropertyUpdated != null)
            {
                PropertyUpdated(this, new PropertyUpdatedEventArgs(propertyName, oldValue, newValue));
            }
        }

        /// <summary>
        /// Raised when a value of the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raised when the value of a property is updated.
        /// </summary>
        public event PropertyUpdatedEventHandler PropertyUpdated;
    }

    /// <summary>
    /// Conveniently converts a string value into some default SyncSettings.
    /// </summary>
    public class SyncSettingsConverter

        : TypeConverter
    {
        /// <summary>
        /// Returns if the converts is able to convert from the specified source type.
        /// </summary>
        /// <param name="context">The type descriptor context.</param>
        /// <param name="sourceType">The source type to convert from.</param>
        /// <returns>True if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given value from the source type to a SyncSettings instance.
        /// </summary>
        /// <param name="context">The type descriptor context.</param>
        /// <param name="culture">The applicable culture settings.</param>
        /// <param name="value">The source value.</param>
        /// <returns>The SyncSettings instance.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            object value)
        {
            if (value != null &&
                value is string)
            {
                return new SyncSettings()
                {
                    SyncChannel = value as string,
                    SynchronizeVertically = true,
                    SynchronizeHorizontally = true
                };
            }
            return base.ConvertFrom(context, culture, value);
        }



    }

    [DontObfuscate]
    internal class SyncLinkManager
    {
        private static SyncLinkManager _instance;

        [DontObfuscate]
        public static SyncLinkManager Instance()
        {
            if (_instance == null)
            {
                _instance = new SyncLinkManager();
            }
            return _instance;
        }

        private Dictionary<string, SyncLink> _links;

        internal int LinkCount { get { return _links.Count; } }

        private SyncLinkManager()
        {
            _links = new Dictionary<string, SyncLink>();
        }

        /// <summary>
        /// Gets the synchonization link that matches the synchchannel name provided.
        /// </summary>
        /// <param name="name">The synchchannel name to search for.</param>
        /// <returns>The SyncLink found.</returns>
        [DontObfuscate]
        public SyncLink GetLink(string name)
        {
            SyncLink link;
            if (_links.TryGetValue(name, out link))
            {
                return link;
            }
            link = new SyncLink();
            link.SyncChannel = name;

            _links.Add(name, link);
            return link;
        }

        /// <summary>
        /// Releases a reference to a SyncLink.
        /// </summary>
        /// <param name="link">The SyncLink to release.</param>
        [DontObfuscate]
        public void ReleaseLink(SyncLink link)
        {
            if (link.Charts.Count == 0 &&
                link.SyncChannel != null &&
                _links.ContainsKey(link.SyncChannel))
            {
                _links.Remove(link.SyncChannel);
            }
        }
    }

    /// <summary>
    /// SyncManager helps to manage SyncSettings of the SeriesViewer control.
    /// </summary>
    public class SyncManager
    {
        internal static void SuspendSyncChannel(SeriesViewer chart)
        {
            var settings = GetSyncSettings(chart);
            if (settings == null)
            {
                return;
            }
            if (!chart.IsSyncReady)
            {
                return;
            }
            if (chart.SyncChannel != settings.SyncChannel)
            {
                return;
            }
            ChangeSyncChannel(chart, chart.SyncChannel, null);
        }

        internal static void EnsureSyncChannel(SeriesViewer chart)
        {
            var settings = GetSyncSettings(chart);
            if (settings == null)
            {
                return;
            }
            if (chart.IsSyncReady &&
                chart.SyncChannel == settings.SyncChannel)
            {
                return;
            }
            string lastChannel = null;
            if (chart.IsSyncReady)
            {
                lastChannel = chart.SyncChannel;
            }
            ChangeSyncChannel(chart, lastChannel, settings.SyncChannel);
        }

        internal static void ChangeSyncChannel(SeriesViewer chart, string oldSyncChannel, string newSyncChannel)
        {
            if (chart != null)
            {
                if (string.IsNullOrEmpty(newSyncChannel))
                {
                    SyncLink oldLink = chart.ActualSyncLink;
                    chart.ActualSyncLink = null;
                    chart.ActualSyncLink = new SyncLink();
                    if (oldLink != null)
                    {
                        SyncLinkManager.Instance().ReleaseLink(oldLink);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(oldSyncChannel))
                    {
                        SyncLink oldLink = chart.ActualSyncLink;
                        chart.ActualSyncLink = null;
                        if (oldLink != null)
                        {
                            SyncLinkManager.Instance().ReleaseLink(oldLink);
                        }
                    }
                    chart.ActualSyncLink = SyncLinkManager.Instance().GetLink(newSyncChannel);
                }
            }
        }


        /// <summary>
        /// Identifies the SyncSettings attached property.
        /// </summary>
        public static readonly DependencyProperty SyncSettingsProperty =
            DependencyProperty.RegisterAttached("SyncSettings", typeof(SyncSettings),
            typeof(SyncManager), new PropertyMetadata(null,
                (o, e) =>
                {
                    OnSyncSettingsChanged(o as SeriesViewer, e);
                }));

        private static void OnSyncSettingsChanged(SeriesViewer chart, DependencyPropertyChangedEventArgs e)
        {
            string oldSyncChannel = null;
            string newSyncChannel = null;

            if (e.OldValue != null)
            {
                (e.OldValue as SyncSettings).PropertyUpdated -= chart.UpdateSyncSettings;
                oldSyncChannel = (e.OldValue as SyncSettings).SyncChannel;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as SyncSettings).PropertyUpdated += chart.UpdateSyncSettings;
                newSyncChannel = (e.NewValue as SyncSettings).SyncChannel;
            }
            ChangeSyncChannel(chart, oldSyncChannel, newSyncChannel);
        }


        /// <summary>
        /// Sets the SyncSettings for a target chart.
        /// </summary>
        /// <param name="target">The target chart to set the sync settings for.</param>
        /// <param name="syncSettings">The SyncSettings to set for the chart.</param>
        public static void SetSyncSettings(DependencyObject target, SyncSettings syncSettings)
        {



            target.SetValue(SyncSettingsProperty, syncSettings);

        }

        /// <summary>
        /// Gets the SyncSettings for a target chart.
        /// </summary>
        /// <param name="target">The chart to get the SyncSettings for.</param>
        /// <returns>The SyncSettings for the chart.</returns>
        public static SyncSettings GetSyncSettings(DependencyObject target)
        {



            return (SyncSettings)target.GetValue(SyncSettingsProperty);

        }
    }

    /// <summary>
    /// Implementors are providers of FastItemsSource instances.
    /// </summary>
    public interface IFastItemsSourceProvider
    {
        /// <summary>
        /// Gets a fast item source for the target enumerable.
        /// </summary>
        /// <param name="target">The enumerable to get the FastItemsSource for.</param>
        /// <returns>The FastItemsSource reference.</returns>
        FastItemsSource GetFastItemsSource(



            IEnumerable 

            target);

        /// <summary>
        /// Releases a FastItemsSource reference.
        /// </summary>
        /// <param name="itemsSource">The enumerable for which to release the FastItemsSource.</param>
        /// <returns>The FastItemsSource reference.</returns>
        FastItemsSource ReleaseFastItemsSource(



            IEnumerable 

            itemsSource);
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