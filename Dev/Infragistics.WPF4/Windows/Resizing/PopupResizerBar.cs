using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Class created by a <see cref="PopupResizerDecorator"/> to allow resizing of a <see cref="Popup"/> control.
	/// </summary>
	/// <seealso cref="PopupResizerDecorator"/>
	/// <seealso cref="Location"/>
	/// <seealso cref="ResizeMode"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

    // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateBottom,               GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateTop,                  GroupName = VisualStateUtilities.GroupLocation)]

	public sealed class PopupResizerBar : Control
	{
		#region Private Members

        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Private Members

		#region Contructors

		/// <summary>
		/// Initializes a new instance of <see cref="PopupResizerBar"/>
		/// </summary>
		public PopupResizerBar()
		{
		}
		static PopupResizerBar()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupResizerBar), new FrameworkPropertyMetadata(typeof(PopupResizerBar)));
		}

		#endregion //Contructors

        #region Base class overrides

            #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template for the control has been changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns <see cref="PopupResizerBar"/> Automation Peer Class <see cref="PopupResizerBarAutomationPeer"/>
        /// </summary> 
        /// <returns>AutomationPeer</returns>

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PopupResizerBarAutomationPeer(this);
        }

            #endregion //OnCreateAutomationPeer

        // JJD 11/27/07 - BR27279
            #region OnMouseDown

        /// <summary>
        /// Invoked when the mouse is pressed down on the element. The panning operation is cancelled when the next mouse down occurs.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // JJD 11/27/07 - BR27279
            // If the mouse is captured by a child element mark the mousedown/up msgs
            // as handled. This is to prevent context menu from displaying during a
            // resize operation,
            if (this.IsMouseCaptureWithin)
                e.Handled = true;
        }

            #endregion //OnMouseDown

            // JJD 11/27/07 - BR27279
            #region OnMouseUp

        /// <summary>
        /// Invoked when a mouse button has been released. If a scroll operation has initiated, the panning operation will end. Otherwise, it will continue until the next mouse button is pressed.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            // JJD 11/27/07 - BR27279
            // If the mouse is captured by a child element mark the mousedown/up msgs
            // as handled. This is to prevent context menu from displaying during a
            // resize operation,
            if (this.IsMouseCaptureWithin)
                e.Handled = true;
        }

            #endregion //OnMouseUp

        #endregion //Base class overrides	
        
		#region Properties

			#region Public Properties

				#region ResizeMode

		/// <summary>
		/// Identifies the <see cref="ResizeMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeModeProperty = PopupResizerDecorator.ResizeModeProperty.AddOwner(typeof(PopupResizerBar), new FrameworkPropertyMetadata(PopupResizeMode.VerticalOnly));

		/// <summary>
		/// Gets/sets how the popup will be able to be resized.
		/// </summary>
		/// <seealso cref="ResizeModeProperty"/>
		//[Description("Gets/sets how the popup will be able to be resized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public PopupResizeMode ResizeMode
		{
			get
			{
				return (PopupResizeMode)this.GetValue(PopupResizerDecorator.ResizeModeProperty);
			}
			set
			{
				this.SetValue(PopupResizerDecorator.ResizeModeProperty, value);
			}
		}

				#endregion //ResizeMode

				#region Location

		/// <summary>
		/// Identifies the <see cref="Location"/> dependency property
		/// </summary>
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location",
            typeof(PopupResizerBarLocation), typeof(PopupResizerBar), new FrameworkPropertyMetadata(PopupResizerBarLocation.Bottom, FrameworkPropertyMetadataOptions.AffectsParentMeasure

            // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        , new PropertyChangedCallback(OnVisualStatePropertyChanged)

        ));

		/// <summary>
		/// Determines where the resizer bar is positioned.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this is determined based on where the popup is finally positioned relative to its PlacementTarget.</para></remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="PopupResizerBarLocation"/>
		/// <seealso cref="ResizeMode"/>
		//[Description("Determines where the resizer bar is positioned.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public PopupResizerBarLocation Location
		{
			get
			{
				return (PopupResizerBarLocation)this.GetValue(PopupResizerBar.LocationProperty);
			}
			set
			{
				this.SetValue(PopupResizerBar.LocationProperty, value);
			}
		}

				#endregion //Location

			#endregion //Public Properties

			#region Internal Properties

			#endregion //Internal Properties	
    
		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region GetThumb

		internal Thumb GetThumb()
		{
			return this.GetTemplateChild("PART_Thumb") as Thumb;
		}

				#endregion //GetThumb

                #region PerformMove
        internal void PerformMove(double x, double y)
        {
            Point clientPt = Utilities.PointFromScreenSafe(this, new Point(x, y));
            PopupResizerDecorator decorator = Utilities.GetAncestorFromType(this, typeof(PopupResizerDecorator), true) as PopupResizerDecorator;

            if (null != decorator)
                decorator.PerformMove(clientPt.X, clientPt.Y);
        } 
                #endregion //PerformMove

			#endregion //Internal Methods	

            #region Propected Methods

                #region VisualState... Methods


        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        private void SetVisualState(bool useTransitions)
        {
            switch (this.Location)
            {
                case PopupResizerBarLocation.Top:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateTop, useTransitions);
                    break;
                default:
                case PopupResizerBarLocation.Bottom:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateBottom, useTransitions);
                    break;
            }
        }

        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PopupResizerBar rb = target as PopupResizerBar;

            rb.UpdateVisualStates();
        }

        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods

            #endregion //Propected Methods	
            
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