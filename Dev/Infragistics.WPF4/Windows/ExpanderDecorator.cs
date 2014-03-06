using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers;
using System.Windows.Threading;

namespace Infragistics.Windows
{
	/// <summary>
	/// A Decorator derived class that expands/collapses its Child element in either a Vertical or Horizontal Orientation.
	/// </summary>
	/// <seealso cref="Orientation"/>
	/// <seealso cref="IsExpanded"/>
	/// <seealso cref="ToggleExpandedState"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ExpanderDecorator : Decorator
	{
		#region Member Variables

		private Size					_cachedChildActualSize = Size.Empty;
		private double					_fromAnimationValue;
		private double					_toAnimationValue;
		private DependencyProperty		_propertyToAnimate;
		private bool					_isAnimationActive;
		private Storyboard				_storyboard;

		#endregion //Member Variables

		#region Constructors

		static ExpanderDecorator()
		{
			FrameworkElement.ClipToBoundsProperty.OverrideMetadata(typeof(ExpanderDecorator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));


			// Register commands.
			//
			// ToggleExpandedState
			ExpanderDecorator.ToggleExpandedState
				= new RoutedCommand("ToggleExpandedState", typeof(ExpanderDecorator));

			CommandManager.RegisterClassCommandBinding(typeof(ExpanderDecorator),
				new CommandBinding(ExpanderDecorator.ToggleExpandedState,
									 new ExecutedRoutedEventHandler(ExpanderDecorator.OnToggleExpandedState),
									 new CanExecuteRoutedEventHandler(ExpanderDecorator.OnQueryToggleExpandedState)));
		}

		#endregion //Constructors

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			// Let the base arrange the Child.
			Size s = base.ArrangeOverride(arrangeSize);


			// If we have a Child and we are Expanded (but not animating), then cache out Child's actual size.
			if (this.Child != null)
			{
				if (this.IsExpanded && this._isAnimationActive == false)
					this._cachedChildActualSize = new Size(((FrameworkElement)this.Child).ActualWidth, ((FrameworkElement)this.Child).ActualHeight);
			}

			return s;
		}

			#endregion //ArrangeOverride

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size childSize = new Size(0, 0);

			if (this.Child != null)
			{
				Size adjustedConstraint = constraint;
				if (this.IsExpanded)
				{
					if (this.Orientation == Orientation.Vertical)
						adjustedConstraint = new Size(constraint.Width, double.PositiveInfinity);
					else
						adjustedConstraint = new Size(double.PositiveInfinity, constraint.Height);
				}

				this.Child.Measure(adjustedConstraint);
				childSize = this.Child.DesiredSize;
			}

			return childSize;

		}

			#endregion //MeasureOverride

			// AS 9/1/09 TFS21721
			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns <see cref="ExpanderDecorator"/> Automation Peer Class <see cref="ExpanderDecoratorAutomationPeer"/>
		/// </summary> 
		/// <returns>AutomationPeer</returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new ExpanderDecoratorAutomationPeer(this);
		}

			#endregion //OnCreateAutomationPeer

		#endregion //Base Class Overrides

		#region Commands

		/// <summary>
		/// Toggles the ExpanderDecorator's <see cref="IsExpanded"/> property.
		/// </summary>
		/// <seealso cref="IsExpanded"/>
		public static readonly RoutedCommand ToggleExpandedState;

		#endregion Commands

		#region Properties

			#region IsExpanded

		/// <summary>
		/// Identifies the <see cref="IsExpanded"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
			typeof(bool), typeof(ExpanderDecorator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
																					new PropertyChangedCallback(OnIsExpandedChanged), 
																					new CoerceValueCallback(OnCoerceIsExpanded)));

		private static object OnCoerceIsExpanded(DependencyObject target, object value)
		{
			ExpanderDecorator ed = target as ExpanderDecorator ;

			if (ed != null && ed.IsLoaded == false)
			{
				if ((bool)value == true)
				{
					if (ed.Orientation == Orientation.Vertical)
						ed.ClearValue(ExpanderDecorator.HeightProperty);
					else
						ed.ClearValue(ExpanderDecorator.WidthProperty);
				}
			}

			return value;
		}

		private static void OnIsExpandedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ExpanderDecorator ed = target as ExpanderDecorator;
			if (ed != null)
			{
				// If we are loaded and visible and IsExpanded is true then doa BeginInvoke
				// so we don't call AnimateSize synchronously. This is because when those 3 conditions
				// are true the AnimateSize method needs to call UpdateLayout which in certain
				// circumstances can cause a Cyclic Reference exception to be thrown
				if ((bool)e.NewValue == false || ed.IsLoaded == false || ed.IsVisible == false)
					ed.AnimateSize((bool)e.NewValue);
				else
					ed.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Utilities.MethodDelegate(ed.BeginAnimateSizeForExpand));
			}
		}

		/// <summary>
		/// Returns/sets whether the ExpanderDecorator is expanded.
		/// </summary>
		/// <seealso cref="IsExpandedProperty"/>
		//[Description("Returns/sets whether the ExpanderDecorator is expanded.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsExpanded
		{
			get
			{
				return (bool)this.GetValue(ExpanderDecorator.IsExpandedProperty);
			}
			set
			{
				this.SetValue(ExpanderDecorator.IsExpandedProperty, value);
			}
		}

			#endregion //IsExpanded

			#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(ExpanderDecorator), new FrameworkPropertyMetadata(KnownBoxes.OrientationHorizontalBox, null, new CoerceValueCallback(OnCoerceOrientation)));

		private static object OnCoerceOrientation(DependencyObject target, object value)
		{
			ExpanderDecorator ed = target as ExpanderDecorator ;

			if (ed != null && ed.IsLoaded == true && ed.IsExpanded == false)
				ed.IsExpanded = true;
			else
			if (ed != null && ed.IsLoaded == false && ed.IsExpanded == false)
			{
				if ((Orientation)value == Orientation.Vertical)
				{
					ed.Height = 0;
					ed.ClearValue(ExpanderDecorator.WidthProperty);
				}
				else
				{
					ed.Width = 0;
					ed.ClearValue(ExpanderDecorator.HeightProperty);
				}
			}

			return value;
		}

		/// <summary>
		/// Returns/sets the dimension in which the expanding and collapsing of the Child content occurs.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns/sets the dimension in which the expanding and collapsing of the Child content occurs.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(ExpanderDecorator.OrientationProperty);
			}
			set
			{
				this.SetValue(ExpanderDecorator.OrientationProperty, value);
			}
		}

			#endregion //Orientation

		#endregion Properties

		#region Methods

			#region Private Methods

				#region AnimateSize

		private void AnimateSize(bool newIsExpandedValue)
		{
			if (this.Child == null)
				return;


			// If we are collapsed and not loaded, explicitly set our Height to zero
			// AS 10/18/10 TFS49492
			// The problem was that we are calling UpdateLayout while an ancestor's template 
			// was invalidated (the DocumentContentHost) and during the UpdateLayout call, an 
			// ancestor pulled the grid out of the logical tree which caused another style 
			// change to percolate which resulted in an exception because this animation 
			// was performed during a style change.
			//
			//if (this.IsLoaded == false) 
			if (this.IsLoaded == false || this.IsVisible == false) 
			{
				if (newIsExpandedValue == false)
				{
					if (this.Orientation == Orientation.Vertical)
						this.Height = 0;
					else
						this.Width = 0;
				}
				else
				{
					if (this.Orientation == Orientation.Vertical)
					{
						this.ClearValue(ExpanderDecorator.HeightProperty);
						this.BeginAnimation(ExpanderDecorator.HeightProperty, null);
					}
					else
					{
						this.ClearValue(ExpanderDecorator.WidthProperty);
						this.BeginAnimation(ExpanderDecorator.WidthProperty, null);
					}
				}

				return;
			}


			// If we are expanding, force an UpdateLayout so that we recalculate our cached child size.
			if (newIsExpandedValue == true)
			{
				this.InvalidateMeasure();
				this.InvalidateArrange();
				this.UpdateLayout();
			}


			// Normalize the child dimensions we will use for our animation from/to values.
			double childWidth	= this._cachedChildActualSize.Width;
			double childHeight	= this._cachedChildActualSize.Height;
			if (double.IsInfinity(childWidth))
				childWidth = this.Child.DesiredSize.Width;
			if (double.IsInfinity(childHeight))
				childHeight = this.Child.DesiredSize.Height;


			// Setup our from/to animation values and the property to animate (i.e., width or height).
			if (newIsExpandedValue == false)	// Setup for collapsing
			{ 
				this._toAnimationValue = 0;
				if (this.Orientation == Orientation.Vertical)
				{
					this._fromAnimationValue	= childHeight;
					this._propertyToAnimate		= ExpanderDecorator.HeightProperty;
				}
				else
				{
					this._fromAnimationValue	= childWidth;
					this._propertyToAnimate		= ExpanderDecorator.WidthProperty;
				}
			}
			else // Setup for expanding
			{ 
				if (this.Orientation == Orientation.Vertical)
				{
					this._fromAnimationValue	= 0;
					this._toAnimationValue		= childHeight;
					this._propertyToAnimate		= ExpanderDecorator.HeightProperty;
				}
				else
				{
					this._fromAnimationValue	= 0;
					this._toAnimationValue		= childWidth;
					this._propertyToAnimate		= ExpanderDecorator.WidthProperty;
				}
			}


			// Verify from/to values.
			Debug.Assert(false == double.IsInfinity(this._fromAnimationValue), "ExpanderDecorator 'from' animation value is Infinity!!");
			Debug.Assert(false == double.IsInfinity(this._toAnimationValue), "ExpanderDecorator 'to' animation value is Infinity!!");
			if (double.IsInfinity(this._fromAnimationValue) ||
				double.IsInfinity(this._toAnimationValue))
				return;


			// Create the animations + storyboard and kickoff the Storyboard.
			DoubleAnimation da	= this.CreateDoubleAnimation(this._fromAnimationValue, this._toAnimationValue, new Duration(TimeSpan.FromSeconds(.3)));
			this._storyboard	= new Storyboard();
			this._storyboard.Children.Add(da);

			string expanderDecoratorName = this.Name;
			if (string.IsNullOrEmpty(expanderDecoratorName))
			{
				NameScope.SetNameScope(this, new NameScope());

				expanderDecoratorName = "ExpDec";
				this.RegisterName(expanderDecoratorName, this);

			}

			Storyboard.SetTargetName(da, expanderDecoratorName);
			Storyboard.SetTargetProperty(da, new PropertyPath(this._propertyToAnimate));
			this._storyboard.Completed += new EventHandler(StoryboardCompleted);

			this._isAnimationActive = true;
			this._storyboard.Begin(this);
		}

				#endregion //AnimateSize
		
				// JJD 1/17/11 - TFS36978 - added
				#region BeginAnimateSizeForExpand

		private void BeginAnimateSizeForExpand()
		{
			// JJD 1/17/11 - TFS36978
			// Since this method is called asynchronously and only when the IsExpanded
			// state has gone to true we only call AnimateSize if the state is still true.
			// This avoids situations where it is set to tru and back to false synchronously.
			if (this.IsExpanded)
				this.AnimateSize(true);
		}
				#endregion //BeginAnimateSizeForExpand	
    
				#region CreateDoubleAnimation

		private DoubleAnimation CreateDoubleAnimation(double from, double to, Duration duration)
		{
			DoubleAnimation da = new DoubleAnimation(from, to, duration);

			return da;
		}

				#endregion //CreateDoubleAnimation

				#region StoryboardCompleted

		void StoryboardCompleted(object sender, EventArgs e)
		{

			// Unhook this 'Storyboard.Completed' event handler.
            // JJD 5/29/09 - TFS18041
            // Check for null before unhooking
            if (this._storyboard != null)
			    this._storyboard.Completed -= new EventHandler(StoryboardCompleted);

			this._storyboard			= null;

			// Reset our animation flag.
			this._isAnimationActive = false;

			// If we have animated to a NON-ZERO value, remove the animation, clear the appropriate property
			// value.
			if (this._toAnimationValue != 0)
			{
				this.BeginAnimation(this._propertyToAnimate, null);
				this.ClearValue(this._propertyToAnimate);
			}
		}

				#endregion //StoryboardCompleted

			#endregion // Private Methods

			#region Static Methods

				#region OnToggleExpandedState

        private static void OnToggleExpandedState(object target, ExecutedRoutedEventArgs args)
        {
            ExpanderDecorator ed = target as ExpanderDecorator;
            if (ed != null)
                ed.IsExpanded = !ed.IsExpanded;
        }

				#endregion //OnToggleExpandedState

				#region OnQueryToggleExpandedState

        private static void OnQueryToggleExpandedState(object target, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

				#endregion //OnQueryToggleExpandedState

			#endregion Static Methods

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