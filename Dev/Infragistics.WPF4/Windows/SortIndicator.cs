using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Internal;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// A Button derived element used display and/or change SortStatus. 
	/// </summary>
	//[Description("A Button derived element used display and/or change SortStatus. ")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

    // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateSortAscending,         GroupName = VisualStateUtilities.GroupSort)]
    [TemplateVisualState(Name = VisualStateUtilities.StateSortDescending,        GroupName = VisualStateUtilities.GroupSort)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnsorted,              GroupName = VisualStateUtilities.GroupSort)]

	public class SortIndicator : Button
	{

        #region Private Members


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Private Members	
    
        #region Constructor

        static SortIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SortIndicator), new FrameworkPropertyMetadata(typeof(SortIndicator)));
 
			// JJD 11/29/10 - TFS59556
			// By default the sort indicator shouldn't be hit test visible so that the mouse messages percolate up
			FrameworkElement.IsHitTestVisibleProperty.OverrideMetadata(typeof(SortIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
       }

        #endregion //Constructor	

        #region Base Class Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template for the control has been changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate	

        #endregion //Base Class Overrides

        #region SortStatus

        /// <summary>
		/// Identifies the 'SortStatus' dependency property
		/// </summary>
		public static readonly DependencyProperty SortStatusProperty = DependencyProperty.Register("SortStatus",
				typeof(SortStatus), typeof(SortIndicator), new FrameworkPropertyMetadata(SortStatus.NotSorted, new PropertyChangedCallback(OnSortStatusChanged)));

		/// <summary>
		/// Event ID for the 'SortStatusChanged' routed event
		/// </summary>
		public static readonly RoutedEvent SortStatusChangedEvent =
				EventManager.RegisterRoutedEvent("SortStatusChanged", RoutingStrategy.Bubble, typeof(EventHandler<SortStatusChangedEventArgs>), typeof(SortIndicator));

		private static void OnSortStatusChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			SortIndicator control = target as SortIndicator;

			if (control != null)
			{
				control.OnSortStatusChanged((SortStatus)e.OldValue, (SortStatus)e.NewValue);


                // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
                control.UpdateVisualStates();

			}
		}

		/// <summary>
		/// Gets/sets whether the status is ascending, descending or not sorted
		/// </summary>
		//[Description("Gets/sets whether the status is ascending, descending or not sorted")]
		//[Category("Behavior")]
		public SortStatus SortStatus
		{
			get
			{
				return (SortStatus)this.GetValue(SortIndicator.SortStatusProperty); ;
			}
			set
			{
				this.SetValue(SortIndicator.SortStatusProperty, value);
			}
		}

		/// <summary>
		/// Called when property 'SortStatus' changes
		/// </summary>
		protected virtual void OnSortStatusChanged(SortStatus previousValue, SortStatus currentValue)
		{
			SortStatusChangedEventArgs newEvent = new SortStatusChangedEventArgs(previousValue, currentValue);
			newEvent.RoutedEvent = SortIndicator.SortStatusChangedEvent;
			newEvent.Source = this;
			RaiseEvent(newEvent);
		}

		/// <summary>
		/// Occurs when property 'SortStatus' changes
		/// </summary>
		//[Description("Occurs when property 'SortStatus' changes")]
		//[Category("Behavior")]
		public event EventHandler<SortStatusChangedEventArgs> SortStatusChanged
		{
			add
			{
				base.AddHandler(SortIndicator.SortStatusChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(SortIndicator.SortStatusChangedEvent, value);
			}
		}

		#endregion //SortStatus

        #region Methods

        #region VisualState... Methods


        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            switch (this.SortStatus)
            {
                case SortStatus.Ascending:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateSortAscending, useTransitions);
                    break;
                case SortStatus.Descending:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateSortDescending, useTransitions);
                    break;
                default:
                case SortStatus.NotSorted:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateUnsorted, useTransitions);
                    break;
            }
        }

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            SortIndicator sd = target as SortIndicator;

            sd.UpdateVisualStates();
        }

        // JJD 4/023/10 - NA2010 Vol 2 - Added support for VisualStateManager
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