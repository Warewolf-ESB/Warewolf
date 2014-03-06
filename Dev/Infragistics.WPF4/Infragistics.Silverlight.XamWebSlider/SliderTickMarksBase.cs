using System.Windows;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A base class that implements tick marks maintenance
    /// </summary>
    public class SliderTickMarksBase : DependencyObjectNotifier // INotifyPropertyChanged
    {
        #region Members

        private const int DefaultnumberOfTickMarks = 5;

        #endregion Members

        #region Properties

        #region Public Properties

        #region HorizontalTickMarksTemplate

        /// <summary>
        /// Identifies the <see cref="HorizontalTickMarksTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalTickMarksTemplateProperty = DependencyProperty.Register("HorizontalTickMarksTemplate", typeof(DataTemplate), typeof(SliderTickMarksBase), new PropertyMetadata(new PropertyChangedCallback(SliderTickMarksChanged)));

        /// <summary>
        /// Gets or sets the DataTemplate for horizontal tick marks.
        /// </summary>
        /// <value>The horizontal tick marks template.</value>
        public DataTemplate HorizontalTickMarksTemplate
        {
            get { return (DataTemplate)this.GetValue(HorizontalTickMarksTemplateProperty); }
            set { this.SetValue(HorizontalTickMarksTemplateProperty, value); }
        }

        private static void SliderTickMarksChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SliderTickMarksBase tickmarks = obj as SliderTickMarksBase;
            if (tickmarks != null && e.OldValue != e.NewValue)
            {
                tickmarks.OnChangeSliderTickMarks();
            }
        }

        #endregion // HorizontalTickMarksTemplate 
        
        #region IncludeSliderEnds

        /// <summary>
        /// Identifies the <see cref="IncludeSliderEnds"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IncludeSliderEndsProperty = DependencyProperty.Register("IncludeSliderEnds", typeof(bool), typeof(SliderTickMarksBase), new PropertyMetadata(false, SliderTickMarksChanged));

        /// <summary>
        /// Gets or sets a value indicating whether include slider ends tickmarks
        /// </summary>
        /// <value>
        /// <c>true</c> There are tickmarks of the ends of the slider; otherwise, <c>false</c> There are not tickmarks of the ends of the slider.
        /// </value>
        public bool IncludeSliderEnds
        {
            get { return (bool)this.GetValue(IncludeSliderEndsProperty); }
            set { this.SetValue(IncludeSliderEndsProperty, value); }
        }

        #endregion // IncludeSliderEnds 

        #region NumberOfTickMarks

        /// <summary>
        /// Identifies the <see cref="NumberOfTickMarks"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NumberOfTickMarksProperty = DependencyProperty.Register("NumberOfTickMarks", typeof(int), typeof(SliderTickMarksBase), new PropertyMetadata(DefaultnumberOfTickMarks, SliderTickMarksChanged));

        /// <summary>
        /// Gets or sets the number of tick marks for 
        /// specified <see cref="SliderTickMarksBase"/> instance.
        /// </summary>
        /// <value>The number of tick marks.</value>
        public int NumberOfTickMarks
        {
            get { return (int)this.GetValue(NumberOfTickMarksProperty); }
            set
            {
                int ticks = value;
                if (ticks < 0)
                    ticks = 0;

                this.SetValue(NumberOfTickMarksProperty, ticks);
            }
        }

        #endregion // NumberOfTickMarks 

        #region UseFrequency

        /// <summary>
        /// Identifies the <see cref="UseFrequency"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UseFrequencyProperty = DependencyProperty.Register("UseFrequency", typeof(bool), typeof(SliderTickMarksBase), new PropertyMetadata(true, SliderTickMarksChanged));

        /// <summary>
        /// Gets or sets a value indicating whether arranging the
        /// tick marks will be based of its frequency.
        /// </summary>
        /// <value><c>true</c> if frequency is used to generate tickmarks; otherwise, <c>false</c>.</value>
        public bool UseFrequency
        {
            get { return (bool)this.GetValue(UseFrequencyProperty); }
            set { this.SetValue(UseFrequencyProperty, value); }
        }

        #endregion // UseFrequency 

        #region VerticalTickMarksTemplate

        /// <summary>
        /// Identifies the <see cref="VerticalTickMarksTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VerticalTickMarksTemplateProperty = DependencyProperty.Register("VerticalTickMarksTemplate", typeof(DataTemplate), typeof(SliderTickMarksBase), new PropertyMetadata(new PropertyChangedCallback(SliderTickMarksChanged)));

        /// <summary>
        /// Gets or sets the DataTemplate for the tick marks when 
        /// orientation of the slider is vertical.
        /// </summary>
        /// <value>The vertical tick marks template.</value>
        public DataTemplate VerticalTickMarksTemplate
        {
            get { return (DataTemplate)this.GetValue(VerticalTickMarksTemplateProperty); }
            set { this.SetValue(VerticalTickMarksTemplateProperty, value); }
        }

        #endregion // VerticalTickMarksTemplate 

        #endregion Public Properties

        #endregion Properties

        #region Methods

        #region Protected Methods

        #region OnChangeSliderTickMarks
        /// <summary>
        /// Called when SliderTickMarks is changed and there is need to 
        /// update tickmarks inside the TickMarksPanel.
        /// </summary>
        protected virtual void OnChangeSliderTickMarks()
        {
        }
        #endregion //OnChangeSliderTickMarks

        #endregion Protected Methods

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