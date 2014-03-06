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
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.ComponentModel;

namespace Infragistics.Controls.Primitives
{
    /// <summary>
    ///  Displays an indicator that an end-user can use to expand/collapse.
    /// </summary>
    [TemplatePart(Name = "ExpansionIndicator", Type = typeof(FrameworkElement))]
    [TemplateVisualState(GroupName = "ExpansionStates", Name = "Expanded")]
    [TemplateVisualState(GroupName = "ExpansionStates", Name = "Collapsed")]
    [TemplateVisualState(Name=ExpansionIndicator.StateMouseOver, GroupName="CommonStates")]
    [TemplateVisualState(Name=ExpansionIndicator.StateNormal, GroupName="CommonStates")]
    [TemplateVisualState(Name=ExpansionIndicator.StateDiabled, GroupName="CommonStates")]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!! (AS 5/27/10)

	public class ExpansionIndicator : ContentControl
    {
        #region Const

        const string StateMouseOver = "MouseOver";
        const string StateNormal = "Normal";
        const string StateDiabled = "Disabled";

        #endregion

        #region Members

        bool _isMouseOver;
        FrameworkElement _expansionIndicator;
        
        #endregion // Members

        #region Constructor


        static ExpansionIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(typeof(ExpansionIndicator)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpansionIndicator">DataTreeExpansionIndicator</see> Class.
        /// </summary>
        public ExpansionIndicator()
        {



            base.IsTabStop = false;



            this.IsEnabledChanged +=new DependencyPropertyChangedEventHandler(ExpansionIndicator_IsEnabledChanged);
        }

        #endregion // Constructor

        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Overrides the framework invocation when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            this._expansionIndicator = base.GetTemplateChild("ExpansionIndicator") as FrameworkElement;
            if (this._expansionIndicator != null)
            {
                this._expansionIndicator.MouseLeftButtonDown += new MouseButtonEventHandler(ExpansionIndicator_MouseDown);
                this._expansionIndicator.Visibility = this.IndicatorVisibility;
            }

            this.UpdateVisualState(false, this.IsExpanded);
        }
                
        #endregion // OnApplyTemplate

        #region OnCreateAutomationPeer
        /// <summary>
        /// Overrides the framework invocation when an automation peer needs to be created.
        /// </summary>
        /// <returns>Returns the <see cref="ExpansionIndicatorAutomationPeer"/>ExpansionIndicatorAutomationPeer</returns>.
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ExpansionIndicatorAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnMouseEnter
        /// <summary>
        /// Overrides the framework invocation of the mouse enter event.
        /// </summary>
        /// <param name="e">Data about the mouse enter event.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this._isMouseOver = true;
            this.UpdateVisualState(true, this.IsExpanded);
            base.OnMouseEnter(e);
        }
        #endregion

        #region OnMouseLeave
        /// <summary>
        /// Overrides the framework invocation of the mouse leave event.
        /// </summary>
        /// <param name="e">Data about the mouse leave event.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this._isMouseOver = false;
            this.UpdateVisualState(true, this.IsExpanded);
            base.OnMouseLeave(e);
        }
        #endregion

        #endregion // Overrides

        #region Properties

        #region Public

        #region IsExpanded

        /// <summary>
        /// DependencyProperty for if the <see cref="ExpansionIndicator">DataTreeExpansionIndicator</see> is expanded or collapsed.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ExpansionIndicator), new PropertyMetadata(new PropertyChangedCallback(IsExpandedChanged)));

        /// <summary>
        /// Returns/Sets if the <see cref="ExpansionIndicator">DataTreeExpansionIndicator</see> is expanded or collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)this.GetValue(IsExpandedProperty); }
            set 
			{ 
				
				if (value)
					VisualStateManager.GoToState(this, "Expanded", true);
				else
					VisualStateManager.GoToState(this, "Collapsed", true);

				this.SetValue(IsExpandedProperty, value);
			}
        }

        private static void IsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ExpansionIndicator control = (ExpansionIndicator)obj;
			control.UpdateVisualState(false, control.IsExpanded);
        }

        #endregion // IsExpanded

        #region IndicatorVisibility

        /// <summary>
        /// DependencyProperty for if the <see cref="ExpansionIndicator">DataTreeExpansionIndicator</see> is visible or collapsed.
        /// </summary>
        public static readonly DependencyProperty IndicatorVisibilityProperty = DependencyProperty.Register("IndicatorVisibility", typeof(Visibility), typeof(ExpansionIndicator), new PropertyMetadata(new PropertyChangedCallback(IndicatorVisibilityChanged)));

        /// <summary>
        /// Returns/Sets for if the <see cref="ExpansionIndicator">DataTreeExpansionIndicator</see> is visible or collapsed.
        /// </summary>
        public Visibility IndicatorVisibility
        {
            get { return (Visibility)this.GetValue(IndicatorVisibilityProperty); }
            set { this.SetValue(IndicatorVisibilityProperty, value); }
        }

        private static void IndicatorVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ExpansionIndicator control = (ExpansionIndicator)obj;
            if (control._expansionIndicator != null)
                control._expansionIndicator.Visibility = (Visibility)e.NewValue;
        }
        #endregion // IndicatorVisibility

        #endregion // Public

        #region Protected

        #region IndicatorElement

        /// <summary>
        /// Gets or sets the indicator element that a user can click to expand this. 
        /// </summary>
        protected FrameworkElement IndicatorElement
        {
            get
            {
                return this._expansionIndicator;
            }
            set
            {
                this._expansionIndicator = value;
            }
        }

        #endregion // IndicatorElement

        #endregion // Protected

        #region Private

        private void UpdateVisualState(bool useTransistions, bool isExpanded)
        {
            if(this.IsEnabled)
            {
                if(this._isMouseOver)
                {
                    VisualStateManager.GoToState(this,ExpansionIndicator.StateMouseOver, useTransistions);
                }
                else
                {
                    VisualStateManager.GoToState(this,ExpansionIndicator.StateNormal, useTransistions);
                }
            }
            else
            {
                VisualStateManager.GoToState(this,ExpansionIndicator.StateDiabled, useTransistions);
            }

			if (isExpanded)
                VisualStateManager.GoToState(this, "Expanded", useTransistions);
            else
                VisualStateManager.GoToState(this, "Collapsed", useTransistions);    
        }

        #endregion

        #endregion // Properties

        #region EventHandlers

        void ExpansionIndicator_MouseDown(object sender, MouseButtonEventArgs e)
        {
			this.UpdateVisualState(true, !this.IsExpanded);
			
            this.IsExpanded = !this.IsExpanded;
			e.Handled = true;
        }

        void  ExpansionIndicator_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
			this.UpdateVisualState(false, this.IsExpanded);
        }

        #endregion // EventHandlers
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