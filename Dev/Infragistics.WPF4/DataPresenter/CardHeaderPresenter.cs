using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// An element used to represent the header area of a <see cref="CardViewCard"/>
	/// </summary>
    //[Description("An element used to represent the header area of a Card.")]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_CardView)]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,        GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,           GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,         GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateChanged,          GroupName = VisualStateUtilities.GroupChange)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnchanged,        GroupName = VisualStateUtilities.GroupChange)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateExpanded,         GroupName = VisualStateUtilities.GroupExpansion)]
    [TemplateVisualState(Name = VisualStateUtilities.StateCollapsed,        GroupName = VisualStateUtilities.GroupExpansion)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecord,       GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilterRecord,     GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAddRecord,        GroupName = VisualStateUtilities.GroupRecord)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,         GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,       GroupName = VisualStateUtilities.GroupSelection)]

	[DesignTimeVisible(false)]
    public class CardHeaderPresenter : ContentControl,
									   ISelectableElement
	{
		#region Member Variables


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CardHeaderPresenter"/>
		/// </summary>
		public CardHeaderPresenter()
		{
		}

		static CardHeaderPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CardHeaderPresenter), new FrameworkPropertyMetadata(typeof(CardHeaderPresenter)));

			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CardHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CardHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

			UIElement.FocusableProperty.OverrideMetadata(typeof(CardHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
        }
		#endregion //Constructor

		#region Resource Keys

		#region CollapseEmptyCellsButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the 'Collapse Empty Cells' <see cref="Button"/> within the header of a <see cref="CardViewCard"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.CardViewSettings.CollapseEmptyCellsButtonVisibility"/>
		public static readonly ResourceKey CollapseEmptyCellsButtonStyleKey = new StaticPropertyResourceKey(typeof(CardHeaderPresenter), "CollapseEmptyCellsButtonStyleKey");

		#endregion //CollapseEmptyCellsButtonStyleKey

		#region CollapseCardButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the 'Collapse Card' <see cref="Button"/> within the header of a <see cref="CardViewCard"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.CardViewSettings.CollapseCardButtonVisibility"/>
		public static readonly ResourceKey CollapseCardButtonStyleKey = new StaticPropertyResourceKey(typeof(CardHeaderPresenter), "CollapseCardButtonStyleKey");

		#endregion //CollapseCardButtonStyleKey

		#endregion //Resource Keys

		#region Base class overrides

			#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }
			#endregion //OnApplyTemplate

			#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CardHeaderPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.CardHeaderPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CardHeaderPresenterAutomationPeer(this);
		}

			#endregion //OnCreateAutomationPeer

			#region OnMouseDoubleClick

		/// <summary>
		/// Raises the MouseDoubleClick routed event.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			if (e.ChangedButton == MouseButton.Left)
			{
				// If there are any buttons in the visual tree between the original source and the CardHeaderPresenter
				// don't process the double click.
				if (e.OriginalSource is FrameworkElement || e.OriginalSource.GetType().IsAssignableFrom(typeof(FrameworkElement)))
				{
					ButtonBase button = Utilities.GetAncestorFromType((DependencyObject)e.OriginalSource, typeof(ButtonBase), true, this) as ButtonBase;
					if (button != null)
						return;
				}

				Record record = this.DataContext as Record;
				if (record != null)
				{
					DataPresenterBase dp = record.DataPresenter;
					if (dp != null && dp.CurrentViewInternal is CardView)
					{
						((CardView)dp.CurrentViewInternal).ViewSettings.CardHeight = double.NaN;
						((CardView)dp.CurrentViewInternal).ViewSettings.CardWidth = double.NaN;
					}
				}
			}
		}

			#endregion //OnMouseDoubleClick
    
            #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseEnter

            #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
            #endregion //OnMouseLeave

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region Card

		/// <summary>
        /// Identifies the <see cref="Card"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card",
            typeof(CardViewCard), typeof(CardHeaderPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCardChanged)));

        private static void OnCardChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CardHeaderPresenter php = target as CardHeaderPresenter;

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            if (php != null)
            {
                CardViewCard oldcard = e.OldValue as CardViewCard;

                if (oldcard != null)
                    oldcard.ClearHeader(php);

                CardViewCard newcard = e.NewValue as CardViewCard;

                if (newcard != null)
                    newcard.InitializeHeader(php);
            }
        }

        /// <summary>
        /// Gets/sets the associated <see cref="CardViewCard"/> that contains the header.
        /// </summary>
        /// <remarks>
		/// <para class="note"><b>Note:</b> this property needs to be set inside the template of the <see cref="CardViewCard"/> .</para>
        /// </remarks>
        /// <seealso cref="CardProperty"/>
        //[Description("Gets/sets the CardViewCard that contains the header.")]
        //[Category("CardsControl Properties")]
		public CardViewCard Card
        {
            get
            {
				return (CardViewCard)this.GetValue(CardHeaderPresenter.CardProperty);
            }
            set
            {
                this.SetValue(CardHeaderPresenter.CardProperty, value);
            }
        }

				#endregion //Card

			#endregion //Public Properties

		#endregion //Properties
        
        #region Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            CardViewCard card = this.Card;

            if ( card != null )
                CardViewCard.SetVisualStateHelper(card, this, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CardHeaderPresenter hdr = target as CardHeaderPresenter;

            if ( hdr != null )
                hdr.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
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

			#endregion //Protected methods

        #endregion //Methods

        #region ISelectableElement Members

        ISelectableItem ISelectableElement.SelectableItem
		{
			get 
			{
			    RecordPresenter rp = this.Card.Content as RecordPresenter;
			    if (rp != null)
			        return rp.Record as ISelectableItem;

				return null;
			}
		}

		#endregion
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