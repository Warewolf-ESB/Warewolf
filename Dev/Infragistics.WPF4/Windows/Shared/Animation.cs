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
using System.Windows.Threading;
using System.Reflection;

namespace Infragistics
{
	/// <summary>
	/// An object that can help to animate anything that needs animating via events.
	/// </summary>
    public class Animation
    {
        #region Members

        DispatcherTimer _timer;
        bool _isPlaying;
        double _prevValue;

        #endregion

        #region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Animation"/> class.
		/// </summary>
        public Animation()
        {
			this.EasingFunction = new SineEase();
            this.Duration = 8;
            this._timer = new DispatcherTimer();
            this._timer.Tick += new EventHandler(timer_Tick);
            this._timer.Interval = TimeSpan.FromMilliseconds(1);
        }

        #endregion

        #region Properties

        #region Public 

		/// <summary>
		/// Gets/sets how many steps should be taken for the Start value to reach the End value.
		/// </summary>
        public int Duration
        {
            get;
            set;
        }

		/// <summary>
		/// Gets whether or not the Animation is currently running.
		/// </summary>
        public bool IsPlaying
        {
            get { return this._isPlaying; }
        }

		/// <summary>
		/// Gets/ sets the current step the animation is in.
		/// </summary>
		public int Time
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/ sets the easing function that will be used to calculate the animation.
		/// </summary>
		public EasingFunctionBase EasingFunction
		{
			get;
			set;
		}

        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Public

		/// <summary>
		/// Resets the Time to 1 and starts the Animation.
		/// </summary>
        public void Play()
        {
            this.Time = 0;
            this._prevValue = 0;
            this._timer.Start();
            this._isPlaying = true;
        }

		/// <summary>
		/// Resumes the animation if it was stopped before finishing. 
		/// </summary>
        public void Continue()
        {
			if (this.Time < this.Duration)
			{
				this._timer.Start();
				this._isPlaying = true;
			}
        }

		/// <summary>
		/// Ends the currently running animation.
		/// </summary>
        public void Stop()
        {
            this._timer.Stop();
            this._isPlaying = false;
        }

        #endregion // Public

		#endregion // Methods

		#region EventHandlers

		void timer_Tick(object sender, EventArgs e)
        {
            this.Time++;

            this.OnTick();

			if (this.Time == this.Duration)
			{
				this.Stop();
				this.OnComplete();
			}
        }

        #endregion

		#region Events

		/// <summary>
		/// Fired for each step in the Animation.
		/// </summary>
		public event EventHandler<AnimationEventArgs> Tick;

		/// <summary>
		/// Called each step in the Animation.
		/// </summary>
		protected virtual void OnTick()
		{
			double val = this.EasingFunction.Ease((double)this.Time / (double)this.Duration);
            //In SL5, if Time == Duration, the Easing Function returns 0.9999999999999.  For sanities sake, we're going to just add an extra check.
            if (this.Time == this.Duration)
                val = 1;

			if (this.Tick != null)
                this.Tick(this, new AnimationEventArgs() { Value = val, Difference = val - this._prevValue });

            this._prevValue = val;
		}

		/// <summary>
		/// Fired when the animation has reached the end.
		/// </summary>
		public event EventHandler Complete;

		/// <summary>
		/// Called when the animation has reached its end.
		/// </summary>
		protected virtual void OnComplete()
		{
			if (this.Complete != null)
				this.Complete(this, EventArgs.Empty);
		}

		#endregion // Events

    }

	/// <summary>
	/// The EventArgs that are passed into the <see cref="Animation.Tick"/> event.
	/// </summary>
	public class AnimationEventArgs : EventArgs
	{
		/// <summary>
		/// The value for the next step in the <see cref="Animation"/>.
		/// </summary>
		public double Value
		{
			get;
			internal set;
		}

        public double Difference
        {
            get;
            internal set;
        }

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