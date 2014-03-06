using System.Collections.ObjectModel;
using System.Windows;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A generic class that implements tick marks maintenance
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class SliderTickMarks<T> : SliderTickMarksBase
    {
        #region Members
        private const double DefaultFrequency = 10;
        
        private XamSliderBase<T> _owner;
        private ReadOnlyObservableCollection<T> _resolvedTickMarks;
        private ObservableCollection<T> _tickMarks;
        #endregion Members

        #region Properties

        #region Public Properties
        #region Owner

        /// <summary>
        /// Gets or sets the <see cref="XamSliderBase&lt;T&gt;"/> owner.
        /// </summary>
        /// <value>The owner.</value>
        public XamSliderBase<T> Owner
        {
            
            get
            {
                return this._owner;
            }
            protected internal set
            {
                this._owner = value;
                this.OnChangeSliderTickMarks();
            }
        }

        #endregion // Owner

        #region TickMarksFrequency

        /// <summary>
        /// Identifies the <see cref="TickMarksFrequency"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TickMarksFrequencyProperty = DependencyProperty.Register("TickMarksFrequency", typeof(double), typeof(SliderTickMarks<T>), new PropertyMetadata(TickMarksChanged));

        /// <summary>
        /// Gets or sets the tick marks frequency - 
        /// equal double value that is the difference
        /// between two adjacent tick marks.
        /// </summary>
        /// <value>The tick marks frequency.</value>
        public virtual double TickMarksFrequency
        {
            get { return (double)this.GetValue(TickMarksFrequencyProperty); }
            set { this.SetValue(TickMarksFrequencyProperty, value); }
        }

        private static void TickMarksChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SliderTickMarks<T> tickmarks = obj as SliderTickMarks<T>;
            if (tickmarks != null && e.OldValue != e.NewValue)
            {
                if (!tickmarks.TickmarkGenerationInProcess)
                    tickmarks.OnChangeSliderTickMarks();
            }
        }

        #endregion // TickMarksFrequency 

        #region TickMarksValues

        /// <summary>
        ///Gets the collection of Tick Mark Values of Type T that are used to set the slider.
        /// </summary>
        /// <value>The tick marks.</value>
        public virtual ObservableCollection<T> TickMarksValues
        {
            get
            {
                if (this._tickMarks == null)
                {
                    this._tickMarks = new ObservableCollection<T>();
                    this._tickMarks.CollectionChanged += TickMarks_CollectionChanged;
                }

                return this._tickMarks;
            }
        }

        private static void TickMarks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SliderTickMarks<T> tickmarks = sender as SliderTickMarks<T>;
            if (tickmarks != null)
            {
                tickmarks.OnChangeSliderTickMarks();
            }
        }

        #endregion // TickMarksValues

        #endregion Public Properties

        #region Internal Properties

        #region ResolvedTickMarks

        internal ReadOnlyObservableCollection<T> ResolvedTickMarks
        {
            get
            {
                this.OnResolveTickMarks();

                return this._resolvedTickMarks;
            }
        }

        #endregion // ResolvedTickMarks 

        #region TickmarkGenerationInProcess

        internal bool TickmarkGenerationInProcess
        { get; set; }

        #endregion

        #endregion Internal Properties

        #endregion Properties

        #region Overrides

        #region OnChangeSliderTickMarks
        /// <summary>
        /// Called when SliderTickMarks is changed and there is need to
        /// update tickmarks inside the TickMarksPanel.
        /// </summary>
        protected override void OnChangeSliderTickMarks()
        {
            base.OnChangeSliderTickMarks();
            if (this.Owner != null)
            {
                this.Owner.GenerateTickMarks();
            }
        }
        #endregion //OnChangeSliderTickMarks

        #endregion Overrides

        #region Methods

        #region Protected Methods

        /// <summary>
        /// Gets the frequency value in double.
        /// </summary>
        /// <returns>value in double</returns>
        protected virtual double GetFrequencyValue()
        {
            return this.TickMarksFrequency;
        }

        #region GenerateTickMarksValues
        /// <summary>
        /// Generates the values of the  tick marks, 
        /// based on values of the slider that is owner of the 
        /// <see cref="SliderTickMarks&lt;T&gt;"/> and values 
        /// of the Frequency, NumberOfTickMarks and UseFrequency
        /// properties
        /// </summary>
        /// <returns>ObservableCollection with values of the tick marks</returns>
        protected virtual ObservableCollection<T> GenerateTickMarksValues()
        {
            ObservableCollection<T> values = new ObservableCollection<T>();
            if ((this.Owner != null) && (NumberOfTickMarks >= 0))
            {
                if (this.TickMarksValues.Count == 0)
                {
                    double value = this.Owner.ToDouble(this.Owner.MinValue);
                    double maxValue = this.Owner.ToDouble(this.Owner.MaxValue);
                    double frequency = this.GetFrequencyValue();
                    if (frequency <= 0)
                    {
                        this.TickMarksFrequency = frequency = DefaultFrequency;
                    }

                    if (!this.UseFrequency)
                    {
                        frequency = (maxValue - value) / (NumberOfTickMarks + 1);
                    }

                    while (value < maxValue)
                    {
                        value += frequency;
                        if (value < maxValue)
                        {
                            values.Add(this.Owner.ToValue(value));
                        }
                    }
                }
                else
                {
                    foreach (T tick in this.TickMarksValues)
                    {
                        values.Add(tick);
                    }
                }

                if (this.IncludeSliderEnds)
                {
                    values.Insert(0, this.Owner.MinValue);                    
                    values.Add(this.Owner.MaxValue);
                }
            }

            return values;
        }
        #endregion //GenerateTickMarksValues

        #region OnResolveTickMarks
        /// <summary>
        /// Returns collection from resolved tick marks based on 
        /// TickMarks , Frequency, NumberOfTickMarks and UseFrequency 
        /// properties
        /// </summary>
        protected virtual void OnResolveTickMarks()
        {
            this._resolvedTickMarks = new ReadOnlyObservableCollection<T>(this.GenerateTickMarksValues());

            this.OnPropertyChanged("ResolverTickMarks");
        }
        #endregion //OnResolveTickMarks

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