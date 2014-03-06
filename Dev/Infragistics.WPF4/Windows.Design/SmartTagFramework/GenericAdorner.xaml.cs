using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using Microsoft.Windows.Design.Model;
using Microsoft.Windows.Design;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Design.SmartTagFramework
{

    /// <summary>
    /// Represents an Adorner used as a smart tag panel
    /// </summary>
    public partial class GenericAdorner : UserControl
	{


		#region Member Variables

		private List<ActionListPage>				_actionListPages;
		private int									_currentActionListPageIndex = -1;
		private ModelItem							_modelItem;
		private EditingContext						_context;
		private ActionListPage						_actionListPageToRemove;

		// JM 01-17-11 TFS32484, TFS33069 
		private Popup								_containingPopup;
		private double								_popupVerticalOffsetBeforeDrag;
		private double								_popupHorizontalOffsetBeforeDrag;
		private bool								_pageIsAnimating;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
        /// Initializes a new instance of GenericAdorner
        /// </summary>
		// JM 01-17-11 TFS32484, TFS33069 
		//public GenericAdorner(DesignerActionList actionList, ModelItem modelItem, EditingContext context)
		public GenericAdorner(DesignerActionList actionList, ModelItem modelItem, EditingContext context, Popup containingPopup)
        {
            InitializeComponent();

			this._modelItem			= modelItem;
			this._context			= context;
			this._containingPopup	= containingPopup;   // JM 01-17-11 TFS32484, TFS33069

			this._actionListPages		= new List<ActionListPage>(3);
			this.AddActionListPage(actionList, this._modelItem, false);

			// Bind the ExecuteDesignerActionObjectPropertyItemCommand
			CommandBinding commandBinding = new CommandBinding(SmartTagFrameworkCommands.ExecuteDesignerActionObjectPropertyItemCommand, ActionObjectPropertyItemCommand_Executed, ActionObjectPropertyItemCommand_CanExecute);
			CommandManager.RegisterClassCommandBinding(typeof(GenericAdorner), commandBinding);

			// Bind the NavigateToPreviousActionListPageCommand
			commandBinding = new CommandBinding(SmartTagFrameworkCommands.NavigateToPreviousActionListPageCommand, NavigateToPreviousActionListPageCommand_Executed, NavigateToPreviousActionListPageCommand_CanExecute);
			CommandManager.RegisterClassCommandBinding(typeof(GenericAdorner), commandBinding);
		}

		#endregion //Constructor

		#region Base Class Overrides

			// JM 01-17-11 TFS32484, TFS33069 Added.
			#region OnIsKeyboardFocusWithinChanged
		/// <summary>
		/// Called when the keyboard focus in a child element changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);

			bool		isOverSmartButtonAdorner	= false;
			UIElement	element						= Mouse.DirectlyOver as UIElement;
			while (element != null)
			{
				element = VisualTreeHelper.GetParent(element) as UIElement;
				if (element is SmartButtonAdorner)
				{
					isOverSmartButtonAdorner = true;
					break;
				}
			}

			if (isOverSmartButtonAdorner && ((Mouse.LeftButton & MouseButtonState.Pressed) == MouseButtonState.Pressed)) 
				return;

			if (false == this._pageIsAnimating && (bool)e.NewValue == false)
				RaiseCloseGenericAdornerEvent();
		}
			#endregion //OnIsKeyboardFocusWithinChanged

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region CurrentActionList

		/// <summary>
        /// Gets the DesignerActionList for the current ActionListPage.
        /// </summary>
        public DesignerActionList CurrentActionList
        {
			get 
			{ 
				if (this._currentActionListPageIndex < 0)
					return null;

				return this._actionListPages[this._currentActionListPageIndex].ActionList; 
			}
		}

				#endregion //CurrentActionList

			#endregion //Public Properties

			#region Private Properties

				#region CurrentActionListPage

		private ActionListPage CurrentActionListPage
		{
			get { return this._actionListPages[this._currentActionListPageIndex]; }
		}

				#endregion //CurrentActionListPage

			#endregion //PrivateProperties

		#endregion //Properties

		#region Events

			#region CloseGenericAdorner

		/// <summary>
        /// Event ID for the <see cref="CloseGenericAdornerEvent"/> routed event
        /// </summary>
        public static readonly RoutedEvent CloseGenericAdornerEvent =
            EventManager.RegisterRoutedEvent("CloseGenericAdornerEvent", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(GenericAdorner));

        /// <summary>
        /// Occurs when the popup for has closed up.
        /// </summary>
        public event RoutedEventHandler CloseGenericAdorner
        {
            add { AddHandler(CloseGenericAdornerEvent, value); }
            remove { RemoveHandler(CloseGenericAdornerEvent, value); }
        }

        /// <summary>
        /// This method raises the CloseGenericAdornerEvent
        /// </summary>
        protected void RaiseCloseGenericAdornerEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(GenericAdorner.CloseGenericAdornerEvent);
            RaiseEvent(newEventArgs);
        }

			#endregion //CloseGenericAdorner

		#endregion //Events

		#region Event Handlers

			#region ActionObjectPropertyItemCommand_Executed

		private static void ActionObjectPropertyItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GenericAdorner genericAdorner = sender as GenericAdorner;

			// Find the DesignerActionObjectPropertyItem instance that was clicked on by looking through all the items in our
			// CustomActionList for a DesignerActionObjectPropertyItem with a name that matches the command parameter.
            foreach (DesignerActionItem item in genericAdorner.CurrentActionList.Items)
            {
				DesignerActionObjectPropertyItem actionObjectPropertyItem = item as DesignerActionObjectPropertyItem;

				if (actionObjectPropertyItem != null && actionObjectPropertyItem.Id.Equals(e.Parameter.ToString()))
                {
					// If the object property is null, create an instance and corresponding ModelItem.
					ModelItem objectModelItem = genericAdorner.CurrentActionListPage.ModelItem.Properties[actionObjectPropertyItem.Name].Value;
					if (objectModelItem == null)
					{
						// Create a ModelItem & object instance for the object property.
						objectModelItem = ModelFactory.CreateItem(genericAdorner._context, actionObjectPropertyItem.PropertyType, null);

						// Set the value of the property to the newly create ModelItem
						genericAdorner.CurrentActionListPage.ModelItem.Properties[actionObjectPropertyItem.Name].SetValue(objectModelItem);
					}

					// Set the ModelItem property on the DesignerActionObjectPropertyItem
					actionObjectPropertyItem.ModelItem = objectModelItem;

					genericAdorner.AddActionListPage(actionObjectPropertyItem.ActionList, objectModelItem, true);

                    break;
                }
            }
		}

			#endregion //ActionObjectPropertyItemCommand_Executed

			#region ActionObjectPropertyItemCommand_CanExecute

		private static void ActionObjectPropertyItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
		}

			#endregion //ActionObjectPropertyItemCommand_CanExecute

			#region btnClose_Click

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            RaiseCloseGenericAdornerEvent();
        }

			#endregion //btnClose_Click

			// JM 01-17-11 TFS32484, TFS33069 Added.
			#region headerThumb_DragStarted
		private void headerThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
		{
			Debug.WriteLine("DragStarted - Popup.VerticalOffset: " + this._containingPopup.VerticalOffset.ToString());
			Debug.WriteLine("DragStarted - Popup.HorizontalOffset: " + this._containingPopup.HorizontalOffset.ToString());

			this._popupHorizontalOffsetBeforeDrag	= this._containingPopup.HorizontalOffset;
			this._popupVerticalOffsetBeforeDrag		= this._containingPopup.VerticalOffset;
		}
			#endregion //headerThumb_DragStarted

			// JM 01-17-11 TFS32484, TFS33069 Added.
			#region headerThumb_DragDelta
		private void headerThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			Debug.WriteLine("DragDelta - e.HorizontalChange: " + e.HorizontalChange.ToString());
			Debug.WriteLine("DragDelta - e.VerticalChange: " + e.VerticalChange.ToString());

			this._containingPopup.VerticalOffset	+= e.VerticalChange;
			this._containingPopup.HorizontalOffset	+= e.HorizontalChange;
		}
			#endregion //headerThumb_DragDelta

			#region NavigateToPreviousActionListPageCommand_Executed

		private static void NavigateToPreviousActionListPageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			GenericAdorner genericAdorner = sender as GenericAdorner;

			int currentActionListPageIndex = genericAdorner._currentActionListPageIndex;
			Debug.Assert(currentActionListPageIndex != 0, "Trying to navigate backwards from first page!");
			if (currentActionListPageIndex == 0)
				return;

			// Update the IsEnabled status of all pages.
			ActionListPage newActionListPage = genericAdorner._actionListPages[currentActionListPageIndex - 1];
			genericAdorner.EnableActionListPage(newActionListPage);

			// Apply action list settings (if any) to GenericAdorner and page area.
			genericAdorner.ApplyActionListSettings(newActionListPage.ActionList, false);

			// Update the page area's layout
			genericAdorner.pageArea.InvalidateMeasure();
			genericAdorner.pageArea.UpdateLayout();

			// Animate to the previous page
			genericAdorner.AnimatePageTransition(currentActionListPageIndex, false);
		}

			#endregion //NavigateToPreviousActionListPageCommand_Executed

			#region NavigateToPreviousActionListPageCommand_CanExecute

		private static void NavigateToPreviousActionListPageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

			#endregion //NavigateToPreviousActionListPageCommand_CanExecute

		#endregion //Event Handlers

		#region Methods

			#region AddActionListPage

		private void AddActionListPage(DesignerActionList actionList, ModelItem modelItem, bool animate)
		{
			// Create an ActionListPage and initialize it.
			ActionListPage actionListPage = new ActionListPage();
			actionListPage.Initialize(modelItem, actionList);

			// Add the page to the list of pages.
			this._actionListPages.Add(actionListPage);
			this._currentActionListPageIndex = this._actionListPages.Count - 1;

			// Add the page as a child of the pageArea (i.e., the ActionListCanvas)
			this.pageArea.Children.Add(actionListPage);

			// Update the IsEnabled status of all pages.
			this.EnableActionListPage(actionListPage);

			// Apply action list settings (if any) to GenericAdorner and page area.
			this.ApplyActionListSettings(actionList, (this._currentActionListPageIndex == 0));

			// Update the page area's layout
			this.pageArea.UpdateLayout();

			// Animate the page transition if asked to.
			if (animate && this._actionListPages.Count > 1)
				this.AnimatePageTransition(this._actionListPages.Count - 2, true);
			else
				this.PositionPage(this._actionListPages[this._actionListPages.Count - 1]);
		}

			#endregion // AddActionListPage

			#region AnimatePageTransition

		private void AnimatePageTransition(int fromPageIndex, bool forward)
		{
			// Edit parameters.
			Debug.Assert(	fromPageIndex	>= 0 && 
							fromPageIndex	< this._actionListPages.Count);

			// Calculate the animation start/end values for the Canvas.Left property of the from and to pages.
			bool			animatingForward	= forward;
			int				toPageIndex			= animatingForward ? fromPageIndex + 1 : fromPageIndex - 1;
			ActionListPage	toPage				= animatingForward ? this._actionListPages[toPageIndex] : this._actionListPages[toPageIndex];
			ActionListPage	fromPage			= this._actionListPages[fromPageIndex];

			double			fromPageStartValueLeft, fromPageEndValueLeft, toPageStartValueLeft, toPageEndValueLeft;
			if (animatingForward)
			{
				fromPageStartValueLeft	= 0;
				fromPageEndValueLeft	= -fromPage.ActualWidth;
				toPageStartValueLeft	= fromPage.ActualWidth;
				toPageEndValueLeft		= 0;
			}
			else
			{
				fromPageStartValueLeft	= 0;
				fromPageEndValueLeft	= fromPage.ActualWidth;
				toPageStartValueLeft	= -toPage.ActualWidth;
				toPageEndValueLeft		= 0;

				this._actionListPageToRemove	= fromPage;
			}

			// Create the animations for the Canvas.Left proeprty of the from and to pages.
			Duration		duration			= new Duration(TimeSpan.FromMilliseconds(300));
			DoubleAnimation	fromPageAnimation	= new DoubleAnimation(fromPageStartValueLeft, fromPageEndValueLeft, duration);
			DoubleAnimation toPageAnimation		= new DoubleAnimation(toPageStartValueLeft, toPageEndValueLeft, duration);

			// Hook the animation Completed event so we can do some cleanup at the end of the animation.
			fromPageAnimation.Completed += new EventHandler(OnPageAnimationCompleted);

			// Create the animations for title and back button TranslateTransforms
			double translateOffsetValueLeft;
			if (animatingForward)
				translateOffsetValueLeft = this.ActualWidth / 2;
			else
				translateOffsetValueLeft = -(this.ActualWidth / 2);

			DoubleAnimation translateAnimation = new DoubleAnimation(translateOffsetValueLeft, 0, duration);

			// Create the animations for title and back button Opacity animation
			DoubleAnimation titleOpacityAnimation		= new DoubleAnimation(0.0, 1.0, duration);
			DoubleAnimation backButtonOpacityAnimation	= new DoubleAnimation(0.0, 1.0, duration);

			this.btnBack.Visibility = toPageIndex == 0 ? Visibility.Collapsed : Visibility.Visible;

			// Kick off all animations
			this._pageIsAnimating = true;	// JM 01-17-11 TFS32484, TFS33069
			fromPage.BeginAnimation(Canvas.LeftProperty, fromPageAnimation);
			toPage.BeginAnimation(Canvas.LeftProperty, toPageAnimation);

			this.transformTitle.BeginAnimation(TranslateTransform.XProperty, translateAnimation);
			this.lblTitle.BeginAnimation(OpacityProperty, titleOpacityAnimation);

			this.transformBackButton.BeginAnimation(TranslateTransform.XProperty, translateAnimation);
			this.btnBack.BeginAnimation(OpacityProperty, backButtonOpacityAnimation);
		}

			#endregion //AnimatePageTransition

			#region ApplyActionListSettings

		private void ApplyActionListSettings(DesignerActionList actionList, bool shouldApplyPageDimensions)
		{
			// Set the title, if it is specified
			if (!string.IsNullOrEmpty(actionList.GenericAdornerTitle))
				this.lblTitle.Content = actionList.GenericAdornerTitle;

			// Set the page area dimensions if specified
			if (shouldApplyPageDimensions)
			{
				if (0 != actionList.GenericAdornerMinHeight)
					this.pageArea.MinHeight = actionList.GenericAdornerMinHeight;

				if (0 != actionList.GenericAdornerMaxHeight)
					this.pageArea.MaxHeight = actionList.GenericAdornerMaxHeight;

				if (0 != actionList.GenericAdornerMinWidth)
					this.pageArea.MinWidth = actionList.GenericAdornerMinWidth;

				if (0 != actionList.GenericAdornerMaxWidth)
					this.pageArea.MaxWidth = actionList.GenericAdornerMaxWidth;
			}
		}

			#endregion //ApplyActionListSettings

			#region EnableActionListPage

		private void EnableActionListPage(ActionListPage actionListPage)
		{
			// Enable the specified ActionListPage and disable all others.
			foreach (ActionListPage page in this._actionListPages)
			{
				page.IsEnabled = page == actionListPage;
			}
		}

			#endregion //EnableActionListPage

			// JM 01-17-11 TFS32484, TFS33069 Added.
			#region InitializeKeyboardFocus
		internal void InitializeKeyboardFocus()
		{
			Keyboard.Focus(this.btnClose);
		}
			#endregion //InitializeKeyboardFocus

			#region OnPageAnimationCompleted

		void OnPageAnimationCompleted(object sender, EventArgs e)
		{
			// If we have a page to remove, remove it now from the page area and our List of pages.
			if (this._actionListPageToRemove != null)
			{
				this.pageArea.Children.Remove(this._actionListPageToRemove);
				this._actionListPages.Remove(this._actionListPageToRemove);
				this._currentActionListPageIndex = this._actionListPages.Count - 1;

				this._actionListPageToRemove = null;
			}

			// Make sure the page area is properly sized.
			this.pageArea.InvalidateMeasure();
			this.pageArea.UpdateLayout();

			// JM 01-17-11 TFS32484, TFS33069.
			this._pageIsAnimating = false;
			this.InitializeKeyboardFocus();
		}

			#endregion //OnPageAnimationCompleted

			#region PositionPage

		private void PositionPage(ActionListPage actionListPage)
		{
			Canvas.SetLeft(actionListPage, 0);
			Canvas.SetTop(actionListPage, 0);
		}

			#endregion //PositionPage

		#endregion // Methods
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