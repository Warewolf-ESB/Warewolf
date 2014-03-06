using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using System;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents an Adorner used as a smart tag button
    /// </summary>
    public partial class SmartButtonAdorner : UserControl
    {
		// JJD 08/08/12 - TFS118137 - added
		private bool _isLoaded;
		private AnimationClock _arrowOpacityClock;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of SmartButtonAdorner
        /// </summary>
        public SmartButtonAdorner()
        {
            InitializeComponent();

			// JJD 08/08/12 - TFS118137
			// Replaced event perpetual trigger animations with a binding to an attached ArrowOpacity property
			// that is animated in code. This gets around a timing issue in the framewrok when the control is
			// loaded and unloaded that occasionally will crash the designer with an exception because
			// the animation can't find the 'arrow' element
			// Wire the Loaded event and the ToggleButton's click event				
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.tbtnSmart.Click += new RoutedEventHandler(OnToggleButtonClick);
        }

        #endregion //Constructor

		#region Event Handlers

		// JJD 08/08/12 - TFS118137 - added
		#region OnToggleButtonClick

		private void OnToggleButtonClick(object sender, RoutedEventArgs e)
		{
			//animate the arrow animation
			this.AnimateArrowOpacity();
		}

		#endregion //OnToggleButtonClick	
    
		// JJD 08/08/12 - TFS118137 - added
		#region OnLoaded

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// unwire loaded event
			this.Loaded -= new RoutedEventHandler(OnLoaded);

			//wire loaded event
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

			_isLoaded = true;

			//animate the arrow animation
			this.AnimateArrowOpacity();

		}

		#endregion //OnLoaded	
    
		// JJD 08/08/12 - TFS118137 - added
		#region OnUnloaded

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// unwire unloaded event
			this.Unloaded -= new RoutedEventHandler(OnUnloaded);

			// wire loaded event
			this.Loaded += new RoutedEventHandler(OnLoaded);

			_isLoaded = false;

			//animate the arrow animation
			this.AnimateArrowOpacity();
		}

		#endregion //OnUnloaded	
    
		#endregion //Event Handlers	

		#region Properties

		// JJD 08/08/12 - TFS118137 - added
		#region ArrowOpacity

		/// <summary>
		/// Identifies the ArrowOpacity" attached dependency property
		/// </summary>
		/// <seealso cref="GetArrowOpacity"/>
		public static readonly DependencyProperty ArrowOpacityProperty =
			DependencyProperty.RegisterAttached("ArrowOpacity",
			typeof(double), typeof(SmartButtonAdorner), new FrameworkPropertyMetadata(1.0d));

		/// <summary>
		/// Gets the value of the 'ArrowOpacity' attached property
		/// </summary>
		/// <seealso cref="ArrowOpacityProperty"/>
		public static double GetArrowOpacity(DependencyObject d)
		{
			return (double)d.GetValue(SmartButtonAdorner.ArrowOpacityProperty);
		}

		#endregion //ArrowOpacity

		#endregion //Properties	
    
		#region Methods

		// JJD 08/08/12 - TFS118137 - added
		#region AnimateArrowOpacity

		private void AnimateArrowOpacity()
		{
			// if the contril is unloaded or the toggle button is checked we want
			// to stop the animation clock
			if (false == _isLoaded ||
				(this.tbtnSmart.IsChecked.HasValue && this.tbtnSmart.IsChecked.Value == true))
			{
				if (_arrowOpacityClock != null)
				{
					//stop the animation
					_arrowOpacityClock.Controller.Stop();
					_arrowOpacityClock = null;

					this.tbtnSmart.ClearValue(ArrowOpacityProperty);
				}
				return;
			}

			// create a perpertual animation for the ArrowOpacity property
			DoubleAnimation animation = new DoubleAnimation();
			animation.Duration = new Duration(TimeSpan.FromSeconds(0.7d));
			animation.From = 1.0d;
			animation.To = 0.3d;
			animation.AutoReverse = true;
			animation.RepeatBehavior = RepeatBehavior.Forever;

			// cache the clock so we can stop it later
			_arrowOpacityClock = animation.CreateClock();

			// apply the clock to the ArrowOpacity property on the ToggleButton
			this.tbtnSmart.ApplyAnimationClock(ArrowOpacityProperty, _arrowOpacityClock);

			// start the animation
			_arrowOpacityClock.Controller.Begin();


		}

		#endregion //AnimateArrowOpacity

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