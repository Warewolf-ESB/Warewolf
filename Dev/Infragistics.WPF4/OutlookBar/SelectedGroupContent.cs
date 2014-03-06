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
using Infragistics.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.OutlookBar
{

    /// <summary>
    /// A ContentControl derived control used to present selected group content area in the <see cref="XamOutlookBar"/>
    /// </summary>

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,                 GroupName = VisualStateUtilities.GroupSplitterLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,                GroupName = VisualStateUtilities.GroupSplitterLocation)]

    [TemplatePart(Name = "PART_ButtonShowPopup", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
	[TemplatePart(Name = "PART_SelectedGroupContentPresenter", Type = typeof(ContentPresenter))]	// JM 03-10-09 TFS 11436
	[TemplatePart(Name = "PART_PopupResizerDecorator", Type = typeof(PopupResizerDecorator))]	// JM 03-10-09 TFS 11436
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SelectedGroupContent : ContentControl
    {
        #region Private Members


        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Private Members

        #region Constructors

        static SelectedGroupContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectedGroupContent), new FrameworkPropertyMetadata(typeof(SelectedGroupContent)));
        }

		/// <summary>
		/// Initialize a new SelectedGroupContent
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/>. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public SelectedGroupContent()
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


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


        }

        #endregion //OnApplyTemplate	

        #endregion //Base Class Overrides

        #region Methods

        #region GetTemplateChildHelper

        internal DependencyObject GetTemplateChildHelper(string childName)
		{
			return this.GetTemplateChild(childName);
		}

			#endregion //GetTemplateChildHelper

        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {

            XamOutlookBar olbar = XamOutlookBar.GetOutlookBar( this );

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
            SelectedGroupContent groupContent = target as SelectedGroupContent;

            if (groupContent != null)
                groupContent.UpdateVisualStates();
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