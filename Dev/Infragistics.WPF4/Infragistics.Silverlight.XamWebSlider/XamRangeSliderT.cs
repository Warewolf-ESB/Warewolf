using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides generic slider with
    /// multiple thumbs (handles) behavior.
    /// </summary>
    /// <typeparam name="T">The generic type</typeparam>
    [ContentProperty("Thumbs")]
    public abstract class XamRangeSlider<T> : XamSliderBase<T>
    {
        #region Properties

        #region Public

        #region ActiveThumb

        /// <summary>
        /// Gets or sets the active thumb.
        /// It specified thumb from slider thumbs,
        /// that is active at the moment
        /// </summary>
        /// <value>The active thumb.</value>

        [Browsable(false)]


        [EditorBrowsable(EditorBrowsableState.Never)]
        public new XamSliderThumb<T> ActiveThumb
        {
            get { return base.ActiveThumb; }
            set { base.ActiveThumb = value; }
        }

        #endregion ActiveThumb



        #region IsSelectionRangeEnabled

        /// <summary>
        /// Identifies the <see cref="IsSelectionRangeEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectionRangeEnabledProperty = DependencyProperty.Register("IsSelectionRangeEnabled", typeof(bool), typeof(XamRangeSlider<T>), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selection range enabled.
        /// When selection rabge is enabled draggung the track fill will drag also the closest thumb
        /// at equal distance
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selection range enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelectionRangeEnabled
        {
            get { return (bool)this.GetValue(IsSelectionRangeEnabledProperty); }
            set { this.SetValue(IsSelectionRangeEnabledProperty, value); }
        }

        #endregion IsSelectionRangeEnabled



        #region Thumbs

        /// <summary>
        /// Gets the thumbs - collection from <see cref="XamSliderThumb&lt;T&gt;"/>
        /// instances.
        /// </summary>
        /// <value>The thumbs.</value>
        public new ObservableCollection<XamSliderThumb<T>> Thumbs
        {
            get
            {
                return base.Thumbs;
            }
        }

        #endregion Thumbs

        #endregion Public

        #endregion Properties

        #region Methods

        #region Internal



        #region OnTrackFillDragCompleted

        /// <summary>
        /// Method to call the TrackFillDragCompleted Event
        /// </summary>
        /// <param name="thumb"></param>
        internal void OnTrackFillDragCompleted(XamSliderThumb<T> thumb1, XamSliderThumb<T> thumb2)
        {
            if (this.TrackFillDragCompleted != null)
            {
                var ea = new TrackFillChangedEventArgs<T>();

                if (thumb1.ResolveValue() < thumb2.ResolveValue())
                {
                    ea.LesserThumb = thumb1;
                    ea.GreaterThumb = thumb2;
                }
                else
                {
                    ea.LesserThumb = thumb2;
                    ea.GreaterThumb = thumb1;
                }

                TrackFillDragCompleted(this, ea);
            }
        }

        #endregion OnTrackFillDragCompleted



        #endregion Internal

        #endregion Methods

        #region Overrides



        #region OnTrackClick
        
        /// <summary>
        /// Overrides the <see cref="XamSliderBase{T}.OnTrackClick"/>.
        /// When click over the slider track, makes sure that we are not IsSelectionRangeEnabled.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected override void OnTrackClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.IsSelectionRangeEnabled)
                base.OnTrackClick(e);
        }

        #endregion



        #endregion

        #region Events


        /// <summary>
        /// Occurs when a TrackFill Drag has occurred.
        /// </summary>
        public event EventHandler<TrackFillChangedEventArgs<T>> TrackFillDragCompleted;


        #endregion Events
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