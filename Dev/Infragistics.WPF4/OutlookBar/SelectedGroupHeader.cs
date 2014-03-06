using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// A ContentControl derived control used to present the selected group header in the <see cref="XamOutlookBar"/>
    /// </summary>

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,               GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,             GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,                 GroupName = VisualStateUtilities.GroupSplitterLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,                GroupName = VisualStateUtilities.GroupSplitterLocation)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SelectedGroupHeader : ContentControl
    {
        #region Members

        ToggleButton _tgButtonMinimize; // minimize button in XamOutlookBar

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Members

        #region Constructors

        static SelectedGroupHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectedGroupHeader), new FrameworkPropertyMetadata(typeof(SelectedGroupHeader)));


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(SelectedGroupHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		/// <summary>
		/// Initialize a new SelectedGroupHeader
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/>. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public SelectedGroupHeader()
		{
		}

        #endregion //Constructors

        #region Base Class Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
			_tgButtonMinimize = this.GetTemplateChild("MinimizeButton") as ToggleButton;

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate

		#region OnMouseEnter

		/// <summary>
        /// Invoked when an unhandled Mouse.MouseEnter attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseEnter	
    
        #region OnMouseLeave

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseLeave attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseLeave	

        #endregion //Base Class Overrides

        #region Methods

        #region Internal Methods






        internal void SetIsMinimized(bool isMinimized)
        {
            // Binding is not enough to keep IsChecked equal to XamOutlookBar.IsMinimized
            //      it is possible to cancel minimizing process ... 
            if (_tgButtonMinimize != null)
                _tgButtonMinimize.IsChecked = isMinimized;
        }

        #endregion //Internal Methods	

        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // Set Common states
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else 
            if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            XamOutlookBar olbar = XamOutlookBar.GetOutlookBar(this);

            // set minimized state
            if (olbar != null && olbar.IsMinimized)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateMinimized, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotMinimized, useTransitions);

            // set splitter location state
            if (olbar != null && olbar.VerticalSplitterLocation == VerticalSplitterLocation.Left)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateLeft, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateRight, useTransitions);

        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            SelectedGroupHeader groupHdr = target as SelectedGroupHeader;

            if (groupHdr != null)
                groupHdr.UpdateVisualStates();
        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        internal protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        internal protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Protected Methods
    
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