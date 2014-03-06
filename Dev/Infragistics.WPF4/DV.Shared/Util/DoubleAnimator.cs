using System;

using System.Windows;
using System.Windows.Media.Animation;
using System.ComponentModel;





namespace Infragistics
{
    /// <summary>
    /// Class for facilitating the animation of Double values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DoubleAnimator

 : DependencyObject, INotifyPropertyChanged



    {


        #region TransitionProgress Dependency Property
        /// <summary>
        /// Sets or gets the value mapping property for the current series object.
        /// <para>
        /// This is a dependency property.
        /// </para>
        /// </summary>
        public double TransitionProgress
        {
            get
            {
                return (double)GetValue(TransitionProgressProperty);
            }
            set
            {
                SetValue(TransitionProgressProperty, value);
            }
        }

        internal const string TransitionProgressPropertyName = "TransitionProgress";

        /// <summary>
        /// Identifies the TransitionProgress dependency property.
        /// </summary>
        public static readonly DependencyProperty TransitionProgressProperty =
            DependencyProperty.Register(
            TransitionProgressPropertyName,
            typeof(double),
            typeof(DoubleAnimator),
            new PropertyMetadata(
                0.0,
                (o, e) => (o as DoubleAnimator)
                    .OnPropertyChanged(
                    TransitionProgressPropertyName,
                    e.OldValue,
                    e.NewValue)));

        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(
                    this,
                    new PropertyChangedEventArgs(
                        "propertyName"));
            }
        }
        #endregion


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        private int _intervalMilliseconds;
        /// <summary>
        /// The total number of milliseconds in the interval.
        /// </summary>
        public int IntervalMilliseconds
        {
            get { return _intervalMilliseconds; }
            set
            {
                _intervalMilliseconds = value;

                _animation.Duration = TimeSpan.FromMilliseconds(_intervalMilliseconds);

            }
        }

        private double _from = 0.0;
        private double _to = 0.0;
        /// <summary>
        /// DoubleAnimator constructor.
        /// </summary>
        /// <param name="from">The double value to start the animation at.</param>
        /// <param name="to">The double value to end the animation at.</param>
        /// <param name="intervalMilliseconds">The span of time it should take for the animation to complete.</param>
        public DoubleAnimator(double from, double to, int intervalMilliseconds)
        {
            _from = from;
            _to = to;
            _intervalMilliseconds = intervalMilliseconds;


            _animation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(intervalMilliseconds),
                From = _from,
                To = _to
            };

            Storyboard.SetTarget(_animation, this);
            Storyboard.SetTargetProperty(_animation, new PropertyPath("TransitionProgress"));
            _storyBoard.Children.Add(_animation);


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

        }


        private Storyboard _storyBoard = new Storyboard();
        private DoubleAnimation _animation = new DoubleAnimation();




        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void Start()
        {

            _storyBoard.Stop();
            _storyBoard.Seek(TimeSpan.FromSeconds(0));
            _storyBoard.Begin();


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        }
        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {

            _storyBoard.Stop();
            _storyBoard.Seek(TimeSpan.FromSeconds(0));





        }



#region Infragistics Source Cleanup (Region)









































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Method which returns a boolean indicating whether or not the animation is active.
        /// </summary>
        /// <returns>True if the animation is active, otherwise False.</returns>
        public bool AnimationActive()
        {



            return GetClockState(_storyBoard) == ClockState.Active;

        }


        internal ClockState GetClockState(Storyboard transitionStoryboard)
        {

            try
            {
                return _storyBoard.GetCurrentState();
            }
            catch
            {
                return ClockState.Stopped;
            }



        }

        /// <summary>
        /// Event raised any time a property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
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