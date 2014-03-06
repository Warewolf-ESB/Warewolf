using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Ribbon.Internal;
using System.Diagnostics;
using Infragistics.Windows.Editors.Events;
using System.Collections;
using System.Security;
using System.Windows.Threading;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a RibbonTool inside a MenuTool
	/// </summary>
	/// <seealso cref="MenuToolBase"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ToolMenuItem : MenuItem,
								IRibbonToolHost,
								IKeyTipContainer
	{
		#region Private Members

		private ToolMenuItem		_parentMenuItem;
		private FrameworkElement	_elementTreeToCapture;
		// AS 5/11/09 TFS17066
		//private ArrayList			_extraLogicalChildren;
		private LogicalContainer	_logicalContainer;

		static private Style		_itemOnlyStyle;

		// AS 12/19/07 BR29199
		private PopupOpeningReason	_openingReason;

		#endregion //Private Members	
    
		#region Constructors

		static ToolMenuItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(typeof(ToolMenuItem)));

			ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(MenuToolPanel)));
			template.Seal();
			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(template));

			// add a coerce callback for IsSubmenuOpen so we can cancel during the opening event
			MenuItem.IsSubmenuOpenProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceIsSubmenuOpen)));

			// AS 11/9/07
			// We also need to handle the coerce so we can prevent a non-activatable tool from being selected.
			//
			//Selector.IsSelectedProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSelectedChanged)));
			Selector.IsSelectedProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSelectedChanged), new CoerceValueCallback(CoerceIsSelected)));

			// JJD 10/26/07 - BR27371
			// Register for the PreviewMouseDownOutsideCapturedElement Event
			EventManager.RegisterClassHandler(typeof(ToolMenuItem), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnPreviewMouseDownOutsideCapturedElement));

			// AS 11/7/07 BR27990
			CommandManager.RegisterClassCommandBinding(typeof(ToolMenuItem), new CommandBinding(MenuTool.SegmentedButtonCommand, new ExecutedRoutedEventHandler(ToolMenuItem.OnExecuteCommand), new CanExecuteRoutedEventHandler(ToolMenuItem.OnCanExecuteCommand)));

			// AS 6/3/08 BR32772
			// Several overriden methods on MenuItem have been decorated with a uipermission attribute
			// so we cannot override them and still be able to run in an xbap. Register a separate handler
			// instead.
			//
			EventManager.RegisterClassHandler(typeof(ToolMenuItem), Keyboard.KeyDownEvent, new KeyEventHandler(OnClassKeyDown));

			// JM 10-17-08 [TFS9222]
			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			// AS 1/11/12 TFS30681
			MenuItem.IsCheckableProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsCheckableChanged)));
			MenuItem.IsCheckedProperty.OverrideMetadata(typeof(ToolMenuItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsCheckedChanged)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ToolMenuItem"/> class.
		/// </summary>
		public ToolMenuItem()
		{
		}

		#endregion //Constructors	

		#region Base class overrides

			#region ArrangeOverride

		/// <summary>
		/// Arranges and sizes the tools contained in the QuickAccessToolbarOverflowPanel.
		/// </summary>
		/// <param name="arrangeBounds">The size available to arrange the contained tools.</param>
		/// <returns>The size used to arrange the contained tools.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			Size size = base.ArrangeOverride(arrangeBounds);

			ToolMenuItem parentMenuItem = this.ParentMenuItem;

			if (parentMenuItem != null)
			{
				if (parentMenuItem.ResizeMode == PopupResizeMode.None ||
					this.RibbonTool is GalleryTool)
					this.ClearValue(PopupResizerDecorator.ResizeConstraintsProperty);
				else
				{
					PopupResizerDecorator.Constraints constraints = PopupResizerDecorator.GetResizeConstraints(this);

					if (constraints == null)
					{
						constraints = new PopupResizerDecorator.Constraints();

						PopupResizerDecorator.SetResizeConstraints(this, constraints);
					}

					Size desiredSize = this.DesiredSize;

					constraints.MinimumWidth = desiredSize.Width;
					constraints.MinimumHeight = desiredSize.Height;
				}
			}

			return size;
		}

			#endregion //ArrangeOverride	
    
			#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes the effects of PrepareContainerForItemOverride.
		/// </summary>
		/// <param name="element">The container element</param>
		/// <param name="item">The item.</param>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			MenuToolBase menuTool = this.RibbonTool as MenuToolBase;

			if (menuTool != null)
			{
				menuTool.ClearContainerForItemInternal(this, element, item);
				return;
			}

			base.ClearContainerForItemOverride(element, item);
		}

		internal void ClearContainerForItemBase(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);
		}

			#endregion //ClearContainerForItemOverride	

			#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			MenuToolBase menuTool = this.RibbonTool as MenuToolBase;
			
			if ( menuTool != null )
				return menuTool.GetContainerForItemInternal(this);

			return new ToolMenuItem();
		}

			#endregion //GetContainerForItemOverride	
    
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			MenuToolBase menuTool = this.RibbonTool as MenuToolBase;

			if (menuTool != null)
				return menuTool.IsItemItsOwnContainerInternal(item);

			return item is ToolMenuItem;
		}

			#endregion //IsItemItsOwnContainerOverride	

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// AS 5/11/09 TFS17066
				//if (this._extraLogicalChildren == null)
				//    return base.LogicalChildren;
				//
				//return new MultiSourceEnumerator(new IEnumerator[] { base.LogicalChildren, this._extraLogicalChildren.GetEnumerator() });
				if (null == _logicalContainer)
					return base.LogicalChildren;

				return new MultiSourceEnumerator(base.LogicalChildren, new SingleItemEnumerator(_logicalContainer));
			}
		}

			#endregion //LogicalChildren

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="ToolMenuItem"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.ToolMenuItemAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToolMenuItemAutomationPeer(this);
        }
            #endregion

            // JJD 11/20/07 - BR27066
            #region OnGotKeyboardFocus

        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Keyboard.GotKeyboardFocus�attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.KeyboardFocusChangedEventArgs that contains the event data.</param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            // JJD 11/20/07 - BR27066
            // since this is a bubble event we want to bypass notifications
            // of focus change for our children
            if (e.NewFocus != this)
                return;

			// AS 11/21/07
			// Do not shift focus within if it came from within.
			//
			// AS 11/30/07
			// IsAncestorOf will not account for crossing over popups, etc. or other breaks in the visual tree
			//
			//if (e.OldFocus is DependencyObject && this.IsAncestorOf((DependencyObject)e.OldFocus))
			if (Utilities.IsDescendantOf(this, e.OldFocus as DependencyObject))
				return;

            IRibbonTool tool = this.RibbonTool as IRibbonTool;

            FrameworkElement feTool = tool as FrameworkElement;

            // check to see if the tool occupies the entire area
            if (feTool == null ||
                tool.ToolProxy.GetMenuItemDisplayMode(feTool) != RibbonToolProxy.ToolMenuItemDisplayMode.UseToolForEntireArea)
                return;

            // JJD 11/21/07 - BR27066
            // If we didn't get focus via a keyboard then bail.
            // Otherwise, figure out which item should be focused
            if ((Keyboard.GetKeyStates(Key.Up)      & KeyStates.Down) == 0 &&
                (Keyboard.GetKeyStates(Key.Down)    & KeyStates.Down) == 0 &&
                (Keyboard.GetKeyStates(Key.Right)   & KeyStates.Down) == 0 &&
                (Keyboard.GetKeyStates(Key.Left)    & KeyStates.Down) == 0 &&
                (Keyboard.GetKeyStates(Key.Tab)     & KeyStates.Down) == 0)
                return;

            FocusNavigationDirection direction = FocusNavigationDirection.First;

            Point pt = new Point();

            FrameworkElement feOldFocus = e.OldFocus as FrameworkElement;

            if (feOldFocus == null &&
                e.OldFocus is DependencyObject)
                feOldFocus = Utilities.GetAncestorFromType(e.OldFocus as DependencyObject, typeof(FrameworkElement), true) as FrameworkElement;

            if (feOldFocus != null)
                pt = this.TranslatePoint(pt, feOldFocus);

            // if the old focus element is below us then get the last element 
            if (pt.Y < 0)
                direction = FocusNavigationDirection.Last;
            else
            {
                // JJD 11/21/07 - BR27066
                // The first time arrowing down into a gallery we need to check
                // if there is a state button selected. If so we want to focus
                // it. Otherwise focus the first galleryitem
                GalleryTool galleryTool = tool as GalleryTool;

                if (galleryTool != null &&
                    galleryTool.ItemBehavior == GalleryToolItemBehavior.StateButton &&
                    galleryTool.SelectedItem != null &&
                    galleryTool.ItemActivatedEventFiredOnceDuringDropdown == false)
                {
                    GalleryToolDropDownPresenter ddp = Utilities.GetDescendantFromType(this, typeof(GalleryToolDropDownPresenter), true ) as GalleryToolDropDownPresenter;

                    Debug.Assert(ddp != null);

                    if (ddp != null)
                    {
                        GalleryItemPresenter itemPresnter = ddp.GetFirstPresenterForItem(galleryTool.SelectedItem);

                        Debug.Assert(itemPresnter != null);

                        if (itemPresnter != null)
                        {
                            itemPresnter.Focus();
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            TraversalRequest request = new TraversalRequest(direction);

            if (feTool.MoveFocus(request))
                e.Handled = true;
        }

            #endregion //OnGotKeyboardFocus	
    
			#region OnKeyDown

		
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


		// AS 12/19/07 BR29199
		// Moved from OnKeyDown to a helper method.
		//
		private void OnKeyDownImpl(KeyEventArgs e)
		{
			// AS 11/29/07 BR28761
			ToolMenuItem parentItem = this.ParentMenuItem;

			if (null != parentItem)
			{
				parentItem.OnChildKeyDown(this, e);

				if (e.Handled)
					return;
			}

            // JJD 11/21/07 - BR27066
            //if (e.Key == Key.Enter)
            switch (e.Key)
            {
                case Key.Enter:
                    #region Process the Enter key

                    {
                        ValueEditor editor = this.RibbonTool as ValueEditor;

                        if (editor != null)
                        {
                            if (editor.IsInEditMode)
                                editor.EndEditMode(true, false);
                            else
                                editor.StartEditMode();

                            e.Handled = true;
                            return;
                        }

                        // AS 11/8/07 BR27990
                        if (this.IsSegmented)
                        {
                            MenuTool menu = this.RibbonTool as MenuTool;

                            if (menu != null && menu.CanExecuteSegmentedButton())
                            {
                                menu.OnSegmentedButtonClick();
                                e.Handled = true;
                            }
                        }
                        break;
                    }

                    #endregion //Process the Enter key

                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    #region Process the directional navigation keys
                    {
                        // JJD 11/21/07 - BR27066
                        // We need to handle the directional navigating keys for ToolMenuItems
                        // whose IsTabStop is true (which we set for embedded tools in Prepare).
                        if (this.IsFocused && this.IsTabStop == false)
                        {
							// AS 5/5/10 TFS30526
							// Moved to helper method
							//
							//FocusNavigationDirection direction;
							//
							//#region Map key to direction enum
							//
							//switch (e.Key)
							//{
							//    default:
							//    case Key.Down:
							//        direction = FocusNavigationDirection.Down;
							//        break;
							//    case Key.Up:
							//        direction = FocusNavigationDirection.Up;
							//        break;
							//    case Key.Left:
							//        direction = FocusNavigationDirection.Left;
							//        break;
							//    case Key.Right:
							//        direction = FocusNavigationDirection.Right;
							//        break;
							//}
							//
							//#endregion //Map key to direction enum
                            FocusNavigationDirection direction = ToolMenuItem.GetDirection(e).Value;
                            TraversalRequest request = new TraversalRequest(direction);

                            if (this.MoveFocus(request))
                            {
                                e.Handled = true;
                                return;
                            }
                        }
						// AS 5/5/10 TFS30526
						// The base menuitem class has an issue whereby if focus is not on a 
						// specific item it will shift focus to the root item. To get around this 
						// we will take over the focus if the menu item contains focus but is itself 
						// not keyboard focused.
						// 
						else if (!this.IsKeyboardFocused && this.IsKeyboardFocusWithin)
						{
							FrameworkElement fe = Keyboard.FocusedElement as FrameworkElement;

							if (null != fe)
							{
								DependencyObject target = fe.PredictFocus(ToolMenuItem.GetDirection(e).Value);

								if (target != null)
									target = GetElementToFocus(target);

								IInputElement focusElement = target as IInputElement;

								if (null != focusElement)
								{
									focusElement.Focus();
									e.Handled = true;
									return;
								}
							}
						}

                        break;
                    }

                    #endregion //Process the directional navigation keys
            }

			// AS 6/3/08 BR32772
			// This is no longer called from the OnKeyDown override
			//
			//base.OnKeyDown(e);
		}

			#endregion //OnKeyDown	

			#region OnClick

		/// <summary>
		/// Called when the menu item is clicked.
		/// </summary>
		protected override void OnClick()
		{
			IRibbonTool ribbonTool = this.RibbonTool;

			// bypass click processing on all but standard menu items
			if (ribbonTool != null && 
				ribbonTool.ToolProxy.GetMenuItemDisplayMode(ribbonTool as FrameworkElement) != RibbonToolProxy.ToolMenuItemDisplayMode.Standard )
				return;

			// JM 11-1-07 BR28000 - Do not call the base click processing if the tool is a RadioButtonTool and IsChecked == true.
			// The user should not be able to uncheck a RadioButton by clicking on it.
			if (ribbonTool is RadioButton && ((RadioButton)ribbonTool).IsChecked == true)
			{
				// AS 11/7/07
				// The menu is staying open when clicking the already checked item. The closest analogy
				// in Office is that of the Switch Windows dropdown. This menu will close if you click the 
				// selected item.
				//
				if (this.StaysOpenOnClick == false)
				{
					XamRibbon ribbon = XamRibbon.GetRibbon(this);

					if (ribbon != null)
						ribbon.TransferFocusOutOfRibbon();
				}
			}
			else
				base.OnClick();

			if (ribbonTool != null)
				ribbonTool.ToolProxy.OnMenuItemClick(ribbonTool as FrameworkElement);
		}

			#endregion //OnClick	
		
			// JJD 10/26/07 - BR27371
			#region OnMouseLeave

		/// <summary>
		/// Called when the mouse leaves the element.
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			ValueEditor editor = this.Header as ValueEditor;

			// JJD 10/26/07 - BR27371
			// If an editor is in edit mode prevent the default processing
			// by not calling the base and marking the event as handled.
			// Otherwise focus will be taken away and which will cause us
			// to exit edit mode
			if (editor != null && editor.IsInEditMode)
			{
				e.Handled = true;
				return;
			}

			base.OnMouseLeave(e);
		}

			#endregion //OnMouseLeave	

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property value chas changed
		/// </summary>
		/// <param name="e">The arguments containing the property and its new and old values.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			DependencyProperty dp = e.Property;

            // JJD 11/29/07 - BR28763
            // if the IsSuppmenuOpen is being changed to false (i.e. closing up)
            // and the keyboard focus is in an element nested within this element then set focus
            // to this element. This has to be done before we call the base implementation.
            // Otherwise the MenuItem base logic throws a null ref exception when trying to close up
            // because it tries to set focus which clears the CurrentSelectedItem at an inopportune time
            // and their code is not robust enough to handle that.
            if (dp == IsSubmenuOpenProperty && 
                false.Equals(e.NewValue))
            {
                if (this.IsKeyboardFocusWithin && !this.IsKeyboardFocused && this.IsMouseCaptureWithin)
                    this.Focus();
            }

			base.OnPropertyChanged(e);

			if (dp == ToolMenuItem.HeaderProperty)
			{
				#region Header

				if (this.Role != MenuItemRole.TopLevelHeader)
				{
					FrameworkElement oldHeader = e.OldValue as FrameworkElement;
					FrameworkElement newHeader = e.NewValue as FrameworkElement;

					if (oldHeader != null)
						this.UnwireHeader(oldHeader);

					if (newHeader != null)
					{
                        // JJD 12/4/07 - BR28873
                        // Initialize IsSeparator property
                        if ( newHeader is SeparatorTool ||
                             newHeader is Separator )
                            this.SetValue(IsSeparatorPropertyKey, KnownBoxes.TrueBox);
                        else
                            this.ClearValue(IsSeparatorPropertyKey);

						ValueEditor editor = this.RibbonTool as ValueEditor;

						if (editor != null)
						{
							//hook the edit mode events
                            // JJD 2/5/09 - TFS5879/BR30928
                            // Wire the event handler on the ToolMenuItem instaed of the editor because otherwise
                            // when the toll gets cloned onto the QAT this event handler will get cloned
                            // and we end up capturing the mouse and not releasing it
                            //editor.EditModeStarted += new EventHandler<EditModeStartedEventArgs>(OnEditModeStarted);
                            //editor.EditModeEnded += new EventHandler<EditModeEndedEventArgs>(OnEditModeEnded);
                            this.AddHandler(ValueEditor.EditModeStartedEvent, new EventHandler<EditModeStartedEventArgs>(OnEditModeStarted));
                            this.AddHandler(ValueEditor.EditModeEndedEvent, new EventHandler<EditModeEndedEventArgs>(OnEditModeEnded));
                        }
						else
						{
							// hook the IsKeyboardFocusWithinChanged event
							newHeader.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(OnHeaderIsKeyboardFocusWithinChanged);
						}
					}
                    else
                    {
                        // JJD 12/4/07 - BR28873
                        // Clear IsSeparator property
                        this.ClearValue(IsSeparatorPropertyKey);
                    }
				}

				#endregion //Header
			}
			else
			if (dp == IsPopupConstrainedVerticallyProperty ||
				dp == ResizeModeProperty)
				this.RefreshPopupVerticalScrollBarVisibility();
			else
			if (dp == IsSubmenuOpenProperty)
			{
				this.ClearValue(IsPopupConstrainedVerticallyProperty);

				// AS 11/1/07 BR27964
				if (true.Equals(e.NewValue))
				{
					// AS 12/19/07 BR29199
					KeyTipManager.NotifyPopupOpened(this.OpeningReasonResolved, this);

					this.ActivateKeyTipProviderAfterDelay();
				}
				else
					this.DeactivateKeyTipProviderIfNeeded();
			}
			else
			if (dp == MenuToolBase.MenuItemDescriptionProperty)
			{
				// JM 11-05-07 - Since MenuItemDescription is now of type object we need to change this compare.
				//this.SetValue(MenuToolBase.HasMenuItemDescriptionPropertyKey, KnownBoxes.FromValue(string.IsNullOrEmpty((string)e.NewValue) == false));
				this.SetValue(MenuToolBase.HasMenuItemDescriptionPropertyKey, KnownBoxes.FromValue(e.NewValue != null));
			}
		}

			#endregion //OnPropertyChanged	
    
			#region OnToolTipOpening
		/// <summary>
		/// Invoked when the tooltip could be shown for the element.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			base.OnToolTipOpening(e);

			// AS 7/10/09 TFS18398
			// Since a tool may not be within the visual tree (e.g. when the menu item 
			// is used to represent a buttontool), the toolmenuitem has its tooltip 
			// bound to the tooltip of the tool. This can be a problem however when that 
			// tooltip was binding to the datacontext because the datacontext of the menu 
			// item is not the same as the datacontext of the tool (since the menu item is 
			// a "container" generated by the items control and therefore its datacontext is 
			// the underlying item (i.e. the tool). Since it would be a potentially dangerous 
			// change to change the datacontext of the menuitem after the itemscontrol 
			// explicitly set it to the underlying item, we will instead potentially 
			// tweak the tooltip. If the tooltip was a ToolTip instance and didn't have a 
			// datacontext explicitly set, we will set it to the datacontext of the tool. If 
			// it was something else then we will create a tooltip and set its datacontext to 
			// that of the tool on the assumption that the elements in that tooltip or those 
			// hydrated for the tooltip instance (i.e. via the datatemplate) could rely on the 
			// datacontext.
			//
			e.Handled = ToolTipHelper.ShowToolTip(this, e);
		} 
			#endregion //OnToolTipOpening


			#region PrepareContainerForItemOverride

		internal void PrepareContainerForItemBase(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			FrameworkElement fe = element as FrameworkElement;

			if (fe != null)
				fe.DataContext = item;
		}

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// AS 6/9/08 BR32242
			ToolMenuItem container = element as ToolMenuItem;

			if (null != container)
				container.SetValue(ToolPropertyKey, item as IRibbonTool);

			var ribbonTool = this.RibbonTool;
			MenuToolBase menuTool = ribbonTool as MenuToolBase;

			// AS 1/10/12 TFS99109
			// We need to get into here when the menuitem was not associated with a tool but came 
			// from something like the parent menuitem having its itemssource set.
			//
			//if (menuTool != null)
			if (menuTool != null || ribbonTool == null)
			{
				ToolMenuItem menuItem = element as ToolMenuItem;
				FrameworkElement feItem = item as FrameworkElement;

				// JJD 10/12/07 set UseLargeImages property on ToolMenuItem and tool
				if (null != menuItem)
				{
					object useLargeImages = KnownBoxes.FromValue(this.UseLargeImagesForItems);

					menuItem.SetValue(ToolMenuItem.UseLargeImagePropertyKey, useLargeImages);

					// AS 10/17/07 - MenuItemDescriptionMinWidth
					menuItem.SetValue(ToolMenuItem.MenuItemDescriptionMinWidthPropertyKey, this.MenuItemDescriptionMinWidthForItems);

					// AS 11/21/07
					// We currently have some duplicate code within the if blocks below but it was only
					// one line of code. Now that we need to do more for the embedded tool/control situation
					// I'm moving this to a separate block after this if block.
					//
					bool isEmbeddedItem = false;

                    if (feItem != null && item is IRibbonTool)
                    {
						feItem.SetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty, useLargeImages);

                        // JJD 11/20/07 - BR27066
                        // If the tool occupies the entire area then set the DirectionalNavigationProperty to Continue so
                        // the user can navigate into descendant elements
						if (((IRibbonTool)item).ToolProxy.GetMenuItemDisplayMode(feItem) == RibbonToolProxy.ToolMenuItemDisplayMode.UseToolForEntireArea)
						{
							// AS 11/21/07
							// Moved to an if block outside this one.
							//
							//menuItem.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Continue);
							isEmbeddedItem = true;
						}
                    }
                    else
                    {
                        // JJD 11/20/07 - BR27066
                        // If the item is not a menu item but is a Framework element (e.g. a Panel)
                        // then set the DirectionalNavigationProperty to Continue so the user can navigate into descendant elements
						if (!(item is MenuItem) && item is FrameworkElement)
						{
							// AS 11/21/07
							// Moved to an if block outside this one.
							//
							//menuItem.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Continue);
							isEmbeddedItem = true;
						}

						// AS 1/10/12 TFS99109
						// We have to set this on the menu item in the case where the itemssource of the applicationmenu was set.
						//
						if (container != null)
							container.SetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty, useLargeImages);
                    }

					// AS 11/21/07
					// This code was in 2 places in the if block above. Centralizing it here.
					//
					if (isEmbeddedItem)
					{
						menuItem.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Continue);

						// AS 11/21/07
						// We don't want the menu item to be a tab stop if it is fully obscured by the control/tool
						// it contains.
						//
						menuItem.IsTabStop = false;
					}

					// AS 11/5/07
					if (null != MenuToolBase.MenuToolMenu.ShowKeyboardCuesProperty)
						menuItem.SetValue(MenuToolBase.MenuToolMenu.ShowKeyboardCuesProperty, KnownBoxes.TrueBox);

					// JM 11-07-07 BR27028
					if (feItem != null && menuItem != feItem)
						menuItem.SetBinding(FrameworkElement.ToolTipProperty, Utilities.CreateBindingObject(FrameworkElement.ToolTipProperty, BindingMode.OneWay, feItem));
				}

				// AS 1/10/12 TFS99109
				// Added if since now we can get in here even when the menu tool is null.
				//
				if (menuTool != null)
					menuTool.PrepareContainerForItemInternal(this, element, item);

				// AS 11/20/07 ApplicationMenuRecentItem to ToolMenuItem
				// The location wasn't known above so initialize the use large image property now.
				//
				if (null != menuItem && menuItem.Location == ToolLocation.ApplicationMenuRecentItems)
				{
					menuItem.SetValue(ToolMenuItem.UseLargeImagePropertyKey, KnownBoxes.FalseBox);

					if (feItem != null && item is IRibbonTool)
						feItem.SetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty, KnownBoxes.FalseBox);
				}

				return;
			}

			base.PrepareContainerForItemOverride(element, item);
			
		}

			#endregion //PrepareContainerForItemOverride	

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region CanPopupContentScroll
		
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

				#endregion //CanPopupContentScroll

				#region IsSegmented

		internal static readonly DependencyPropertyKey IsSegmentedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsSegmented",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsSegmented"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSegmentedProperty =
			IsSegmentedPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets whether this element represents a MenuTool with a Segmented ButtonType.
		/// </summary>
		/// <value>Returns true if this element represents a <see cref="MenuTool"/> with a <b>Segmented</b>Segmented <see cref="MenuTool.ButtonType"/>ButtonType</value>
		/// <seealso cref="IsSegmentedProperty"/>
		/// <seealso cref="MenuTool"/>
		/// <seealso cref="MenuTool.ButtonType"/>
		/// <seealso cref="MenuToolButtonType"/>
		//[Description("Gets whether this element represents a MenuTool with a Segmented ButtonType.")]
		//[Category("Ribbon Properties")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsSegmented
		{
			get
			{
				return (bool)this.GetValue(ToolMenuItem.IsSegmentedProperty);
			}
		}

				#endregion //IsSegmented

                // JJD 12/4/07 - BR28873 - added IsSeparator property
				#region IsSeparator

		internal static readonly DependencyPropertyKey IsSeparatorPropertyKey =
			DependencyProperty.RegisterReadOnly("IsSeparator",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsSeparator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSeparatorProperty =
			IsSeparatorPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets whether this element represents a SeparatorTool (read-only).
		/// </summary>
        /// <value>Returns true if this element represents a <see cref="SeparatorTool"/>.</value>
		/// <seealso cref="IsSeparatorProperty"/>
        /// <seealso cref="SeparatorTool"/>
        //[Description("Gets whether this element represents a SeparatorTool.")]
		//[Category("Ribbon Properties")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsSeparator
		{
			get
			{
				return (bool)this.GetValue(ToolMenuItem.IsSeparatorProperty);
			}
		}

				#endregion //IsSeparator

				#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(ToolMenuItem));

		/// <summary>
		/// Returns an enumeration that indicates the location of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible tool locations include: Ribbon, Menu, QuickAccessToolbar, ApplicationMenu, ApplicationMenuFooterToolbar, ApplicationMenuRecentItems.</p>
		/// </remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		//[Description("Returns an enumeration that indicates the location of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(ButtonTool.LocationProperty);
			}
		}

				#endregion //Location

				#region MenuItemDescriptionMinWidth

		private static readonly DependencyPropertyKey MenuItemDescriptionMinWidthPropertyKey =
			DependencyProperty.RegisterReadOnly("MenuItemDescriptionMinWidth",
			typeof(double), typeof(ToolMenuItem), new FrameworkPropertyMetadata(0d));

		/// <summary>
		/// Identifies the <see cref="MenuItemDescriptionMinWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MenuItemDescriptionMinWidthProperty =
			MenuItemDescriptionMinWidthPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns/sets the minimum width of the <see cref="Infragistics.Windows.Ribbon.MenuToolBase.MenuItemDescriptionProperty"/>s (if any) associated with the MenuTool's menu items.
		/// </summary>
		/// <seealso cref="MenuItemDescriptionMinWidthProperty"/>
		//[Description("Description")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public double MenuItemDescriptionMinWidth
		{
			get
			{
				return (double)this.GetValue(ToolMenuItem.MenuItemDescriptionMinWidthProperty);
			}
		}

				#endregion //MenuItemDescriptionMinWidth

				#region PopupVerticalScrollBarVisibility

		private static readonly DependencyPropertyKey PopupVerticalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("PopupVerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(ToolMenuItem), new FrameworkPropertyMetadata(ScrollBarVisibility.Disabled));

		/// <summary>
		/// Identifies the <see cref="PopupVerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PopupVerticalScrollBarVisibilityProperty =
			PopupVerticalScrollBarVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility to be used by the ScrollViever inside the Popup (read-only)
		/// </summary>
		/// <seealso cref="PopupVerticalScrollBarVisibilityProperty"/>
		//[Description("Returns the visibility to be used by the ScrollViever inside the Popup (read-only)")]
		//[Category("Ribbon Properties")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
		public ScrollBarVisibility PopupVerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(ToolMenuItem.PopupVerticalScrollBarVisibilityProperty);
			}
		}

				#endregion //PopupVerticalScrollBarVisibility

				#region ResizeMode

		/// <summary>
		/// Identifies the <see cref="ResizeMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register("ResizeMode",
			typeof(PopupResizeMode), typeof(ToolMenuItem), new FrameworkPropertyMetadata(PopupResizeMode.None));

		/// <summary>
		/// Gets/sets whether the popup can be resized
		/// </summary>
		/// <seealso cref="ResizeModeProperty"/>
		//[Description("Gets/sets whether the popup can be resized")]
		//[Category("Ribbon Properties")]
		public PopupResizeMode ResizeMode
		{
			get
			{
				return (PopupResizeMode)this.GetValue(ToolMenuItem.ResizeModeProperty);
			}
			set
			{
				this.SetValue(ToolMenuItem.ResizeModeProperty, value);
			}
		}

				#endregion //ResizeMode

				// AS 6/9/08 BR32242
				// Previously we relied on the DataContext being the item and used
				// that to access the IRibbonTool. Now that we are carrying down the 
				// ribbon's datacontext, we need a separate property. This property
				// has to be set from the PrepareContainerForItemOverride.
				//
				#region Tool

		internal static readonly DependencyPropertyKey ToolPropertyKey =
			DependencyProperty.RegisterReadOnly("Tool",
			typeof(IRibbonTool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Tool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ToolProperty =
			ToolPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated ribbon tool.
		/// </summary>
		/// <seealso cref="ToolProperty"/>
		//[Description("Returns the associated ribbon tool.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public IRibbonTool Tool
		{
			get
			{
				return (IRibbonTool)this.GetValue(ToolMenuItem.ToolProperty);
			}
		}

				#endregion //Tool

				#region UseLargeImage

		internal static readonly DependencyPropertyKey UseLargeImagePropertyKey =
			DependencyProperty.RegisterReadOnly("UseLargeImage",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="UseLargeImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseLargeImageProperty =
			UseLargeImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the menu item should display a large image.
		/// </summary>
		/// <seealso cref="UseLargeImageProperty"/>
		//[Description("Returns a boolean indicating whether the menu item should display a large image.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool UseLargeImage
		{
			get
			{
				return (bool)this.GetValue(ToolMenuItem.UseLargeImageProperty);
			}
		}

				#endregion //UseLargeImage

			#endregion //Public Properties

			#region Internal Properties

				#region ImageSource

		internal static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
			typeof(ImageSource), typeof(ToolMenuItem), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnImageSourceChanged)));

		private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolMenuItem menuItem = (ToolMenuItem)d;

			Image newImage = menuItem.Icon as Image;

			if (e.NewValue == null)
				newImage = null;
			else
			{
				if (newImage == null)
				{
					newImage = new AutoDisabledImage();

                    // AS 10/1/08 TFS5939/BR32114
                    Utilities.SetSnapElementToDevicePixels(newImage, true);

					
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


					// AS 11/9/07
					// In the case where we embed the tool within the menu item, the menu item will not be 
					// disabled when the tool is disabled but we want the image to be rendered as disabled.
					//
					if (menuItem.RibbonTool != null)
						newImage.SetBinding(UIElement.IsEnabledProperty, Utilities.CreateBindingObject(UIElement.IsEnabledProperty, BindingMode.OneWay, menuItem.RibbonTool));
				}

				newImage.Source = (ImageSource)e.NewValue;
			}

			menuItem.Icon = newImage;
		}

				#endregion //ImageSource

				// AS 1/11/12 TFS30681
				#region IsCheckableInternal

		internal static readonly DependencyProperty IsCheckableInternalProperty = DependencyProperty.Register("IsCheckableInternal",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceIsCheckableInternal)));

		internal bool IsCheckableInternal
		{
			get { return (bool)this.GetValue(IsCheckableInternalProperty); }
		}

		private static object CoerceIsCheckableInternal(DependencyObject d, object newValue)
		{
			ToolMenuItem tmi = d as ToolMenuItem;

			if (tmi.IsCheckable)
				return KnownBoxes.TrueBox;

			return newValue;
		}
				#endregion //IsCheckableInternal

				#region IsPopupConstrainedVertically

		internal static readonly DependencyProperty IsPopupConstrainedVerticallyProperty = DependencyProperty.Register("IsPopupConstrainedVertically",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal bool IsPopupConstrainedVertically
		{
			get
			{
				return (bool)this.GetValue(ToolMenuItem.IsPopupConstrainedVerticallyProperty);
			}
			set
			{
				this.SetValue(ToolMenuItem.IsPopupConstrainedVerticallyProperty, value);
			}
		}

				#endregion //IsPopupConstrainedVertically

				#region IsSegmentedInternal

		internal static readonly DependencyProperty IsSegmentedInternalProperty = DependencyProperty.Register("IsSegmentedInternal",
			typeof(bool), typeof(ToolMenuItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSegmentedInternalChanged)));

		private static void OnIsSegmentedInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ToolMenuItem tmi = target as ToolMenuItem;

			// synchronize the internal and public read-only property values
			if ( tmi != null )
			{
				tmi.SetValue(IsSegmentedPropertyKey, e.NewValue);

				// AS 11/13/07 BR27990
				// The command also needs to be bound if its within a menu.
				//
				if (tmi.RibbonTool is MenuTool)
					MenuTool.BindCommandProperties(tmi, (MenuTool)tmi.RibbonTool);
			}
		}

				#endregion //IsSegmentedInternal
	
				#region ItemOnlyStyle

		static internal Style ItemOnlyStyle
		{
			get
			{
				if (_itemOnlyStyle == null)
				{
					// Create a style that just has a ContentPresenter with is content bound to the 
					// DataContext (which contains the tool
					_itemOnlyStyle = new Style(typeof(ToolMenuItem));

                    FrameworkElementFactory ef = new FrameworkElementFactory(typeof(ContentPresenter));

					Binding binding = new Binding();
					binding.Mode = BindingMode.OneTime;

					// AS 6/9/08 BR32242
					// The datacontext isn't the tool anymore so we need to bind to the 
					// Header property which is the tool itself.
					//
					binding.Path = new PropertyPath(ToolMenuItem.HeaderProperty);
					binding.RelativeSource = RelativeSource.TemplatedParent;

					ef.SetBinding(ContentPresenter.ContentProperty, binding);

					ControlTemplate template = new ControlTemplate(typeof(ToolMenuItem));
					template.VisualTree = ef;

					// set the Template property
					Setter setter = new Setter();
					setter.Property = ToolMenuItem.TemplateProperty;
					setter.Value = template;
					_itemOnlyStyle.Setters.Add(setter);

					// set the OverridesDefaultStyleProperty
					setter = new Setter();
					setter.Property = ToolMenuItem.OverridesDefaultStyleProperty;
					setter.Value = KnownBoxes.TrueBox;
					_itemOnlyStyle.Setters.Add(setter);
				}

				return _itemOnlyStyle;
			}
		}

				#endregion //ItemOnlyStyle	
    
				// AS 10/17/07 - MenuItemDescriptionMinWidth
				#region MenuItemDescriptionMinWidthForItems
		private object MenuItemDescriptionMinWidthForItems
		{
			get
			{
				MenuToolBase menu = this.RibbonTool as MenuToolBase;

				if (menu != null && this.Location != ToolLocation.ApplicationMenu)
					return menu.GetValue(MenuToolBase.MenuItemDescriptionMinWidthProperty);

				return RibbonKnownBoxes.DoubleZeroBox;
			}
		} 
				#endregion //MenuItemDescriptionMinWidthForItems

				// AS 12/19/07 BR29199
				#region OpeningReason
		internal PopupOpeningReason OpeningReason
		{
			get { return this._openingReason; }
		}

		internal PopupOpeningReason OpeningReasonResolved
		{
			get
			{
				PopupOpeningReason reason = this._openingReason;

				if (reason == PopupOpeningReason.None)
				{
					MenuToolBase tool = this.RibbonTool as MenuToolBase;

					if (null != tool)
						reason = tool.OpeningReasonResolved;
				}

				return reason;
			}
		}

				#endregion //OpeningReason

				#region ParentMenuItem

		internal ToolMenuItem ParentMenuItem 
		{ 
			get 
			{
				if (this._parentMenuItem == null && this.GetType() == typeof(ToolMenuItem))
					this._parentMenuItem = Utilities.GetAncestorFromType(this, typeof(ToolMenuItem), true) as ToolMenuItem;

				return this._parentMenuItem; 
			} 
		}

				#endregion //ParentMenuTool	
     
				#region RibbonTool

		// AS 6/9/08 BR32242
		// We are overriding the DataContext to pick up that of the ribbon so
		// we cannot use that to get to the tool/item any longer.
		//
		//internal IRibbonTool RibbonTool { get { return this.DataContext as IRibbonTool; } }
		internal IRibbonTool RibbonTool
		{
			get { return this.Tool; }
		}

				#endregion //RibbonTool	
    
				// AS 10/12/07 UseLargeImages
				#region UseLargeImagesForItems
		/// <summary>
		/// Determines if the child tools should render using large images.
		/// </summary>
		internal bool UseLargeImagesForItems
		{
			get 
			{
				// AS 6/9/08 BR32242
				// We should not assume that the tool is the datacontext - use the
				// previously existing RibbonTool property instead.
				//
				//MenuToolBase menuTool = this.DataContext as MenuToolBase;
				// AS 1/10/12 TFS99109
				// Allow the UseLargeImagesOnMenuProperty to come from and be set on the 
				// menu item itself when it is not associated with a tool (e.g. when the 
				// parent menu item had its itemsource set).
				//
				//MenuToolBase menuTool = this.RibbonTool as MenuToolBase;
				var ribbonTool = this.RibbonTool;

				if (ribbonTool == null)
					return (bool)this.GetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty);

				MenuToolBase menuTool = ribbonTool as MenuToolBase;

				return menuTool != null && menuTool.UseLargeImages; 
			}
		} 
				#endregion //UseLargeImagesForItems

			#endregion //Internal Properties	
    
		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region AddLogicalChildInternal

			internal void AddLogicalChildInternal(FrameworkElement child)
			{
				// AS 5/11/09 TFS17066
				// In the MenuToolBase.PrepareContainerForItemInternal, we essentially reparent
				// the tools into the logical tree of the menuitems to avoid a bug in WPF where
				// when commands are propogated an exception can result if the logical parent 
				// is an ancestor of the focus scope of the visual parent. However, since the 
				// ToolMenuItems are created by the ItemsControl's ItemsGenerator, the DataContext
				// of the ToolMenuItem is the item for which the item container was generated. 
				// Therefore when the tool becomes a logical child of the parent menu's menu item
				// its datacontext is inherited from the logical parent (the menu item) and its 
				// datacontext is now the tool that the menu item represents. Rather than try to 
				// change that since the WPF framework could be relying upon it, we will create 
				// a layer between the menu item and the tool. We can then set the DataContext
				// of that element to something up the ancestor chain so the datacontext is 
				// propogated.
				//
				//if (this._extraLogicalChildren == null)
				//    this._extraLogicalChildren = new ArrayList(2);
				//
				//this._extraLogicalChildren.Add(child);
				//
				//this.AddLogicalChild(child);
				if (null == _logicalContainer)
				{
					_logicalContainer = new LogicalContainer();
					// AS 9/17/09 TFS19492
					// Instead of picking up the datacontext of the ribbon, get it from the owning tool.
					//
					//Binding b = new Binding();
					//b.Path = new PropertyPath("(0).(1)", XamRibbon.RibbonProperty, FrameworkElement.DataContextProperty);
					//b.RelativeSource = RelativeSource.Self;
					Debug.Assert(this.RibbonTool != null);
					Binding b = Utilities.CreateBindingObject(FrameworkElement.DataContextProperty, BindingMode.OneWay, this.RibbonTool);
					_logicalContainer.SetBinding(DataContextProperty, b);
					this.AddLogicalChild(_logicalContainer);
				}

				_logicalContainer.AddLogicalChildInternal(child);
			}

				#endregion //AddLogicalChildInternal

				#region GetChildKeyTipProviders
		/// <summary>
		/// Returns an array of <see cref="IKeyTipProvider"/> that represent the menu items in the Items collection.
		/// </summary>
		internal IKeyTipProvider[] GetChildKeyTipProviders()
		{
			List<IKeyTipProvider> providers = new List<IKeyTipProvider>();
			ItemContainerGenerator generator = this.ItemContainerGenerator;

			if (null != generator)
			{
				foreach (object item in this.Items)
				{
					ToolMenuItem menuItem = item as ToolMenuItem;

					if (null == menuItem)
						menuItem = generator.ContainerFromItem(item) as ToolMenuItem;

					if (null != menuItem)
						providers.AddRange(menuItem.GetKeyTipProviders());
				}
			}

			return providers.ToArray();
		} 
				#endregion //GetChildKeyTipProviders

				// AS 5/5/10 TFS30526
				#region GetDirection
		internal static FocusNavigationDirection? GetDirection(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Down:
					return FocusNavigationDirection.Down;
				case Key.Up:
					return FocusNavigationDirection.Up;
				case Key.Left:
					return FocusNavigationDirection.Left;
				case Key.Right:
					return FocusNavigationDirection.Right;
				case Key.Tab:
					// AS 12/4/07 BR28887
					// can't predict focus but we still need to handle it because the highlight
					// might get messed up in the process
					return (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0 ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous;
				default:
					return null;
			}
		}
				#endregion //GetDirection

				// AS 5/5/10 TFS30526
				#region GetElementToFocus
		internal static DependencyObject GetElementToFocus(DependencyObject predictedTarget)
		{
			DependencyObject ancestor = predictedTarget;

			while (ancestor != null && ancestor is ToolMenuItem == false)
			{
				ancestor = VisualTreeHelper.GetParent(ancestor);
			}

			// if we found a containing menu item and it doesn't contain the keyboard 
			// but is focusable then focus the menu item first
			if (ancestor != null
				&& false.Equals(ancestor.GetValue(FrameworkElement.IsKeyboardFocusWithinProperty))
				&& true.Equals(ancestor.GetValue(FrameworkElement.FocusableProperty)))
				predictedTarget = ancestor;

			return predictedTarget;
		}
				#endregion //GetElementToFocus

				// AS 10/15/07 BR27181
				#region GetKeyTipProviders
		/// <summary>
		/// Returns the keytip providers that represent the item itself.
		/// </summary>
		internal IKeyTipProvider[] GetKeyTipProviders()
		{
			IRibbonTool tool = this.RibbonTool;

			if (tool != null)
			{
				RibbonToolProxy proxy = tool != null ? tool.ToolProxy : null;

				if (null != proxy)
					return proxy.GetKeyTipProviders((FrameworkElement)tool, this);
			}
			else
				return new IKeyTipProvider[1] { new ToolMenuItemKeyTipProvider(this) };

			return new IKeyTipProvider[0];
		} 
				#endregion //GetKeyTipProviders

				// AS 11/29/07 BR28761
				#region OnChildKeyDown
		internal virtual void OnChildKeyDown(ToolMenuItem childMenuItem, KeyEventArgs e)
		{

		} 
				#endregion //OnChildKeyDown

				// AS 12/19/07 BR29199
				#region OpenMenu
		internal void OpenMenu(PopupOpeningReason reason)
		{
			try
			{
				Debug.Assert(this._openingReason == PopupOpeningReason.None);
				this._openingReason = reason;
				this.IsSubmenuOpen = true;
			}
			finally
			{
				this._openingReason = PopupOpeningReason.None;
			}
		} 
				#endregion //OpenMenu

				#region RemoveLogicalChildInternal

			internal void RemoveLogicalChildInternal(FrameworkElement child)
			{
				// AS 5/11/09 TFS17066
				//Debug.Assert(this._extraLogicalChildren != null);
				//
				//if ( this._extraLogicalChildren != null )
				//    this._extraLogicalChildren.Remove(child);
				//
				//this.RemoveLogicalChild(child);
				Debug.Assert(null != _logicalContainer);
				if (null != _logicalContainer)
				{
					_logicalContainer.RemoveLogicalChildInternal(child);
				}
			}

				#endregion //RemoveLogicalChildInternal

				#region SimulateMouseLeave

		// JJD 10/12/07
		// Added hack to simulate a mouse leave because that is the only way to remove reset
		// the IsHighlighted read-only property
		internal void SimulateMouseLeave()
		{
			base.OnMouseLeave(new MouseEventArgs(InputManager.Current.PrimaryMouseDevice, Environment.TickCount));
		}

				#endregion //SimulateMouseLeave

			#endregion //Internal Methods	
        
			#region Private Methods

				// AS 11/1/07 BR27964
				#region ActivateKeyTipProvider
		private void ActivateKeyTipProviderAfterDelay()
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, new XamRibbon.MethodInvoker(this.ActivateKeyTipProviderIfNeeded));
		}

		private void ActivateKeyTipProviderIfNeeded()
		{
			if (this.IsSubmenuOpen)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

				if (null != ribbon && ribbon.HasKeyTipManager)
				{
					IKeyTipContainer container = this.RibbonTool as IKeyTipContainer ?? this;
					ribbon.KeyTipManager.ActivateKeyTipProviderIfNotPending(container);
				}
			}
		} 
				#endregion //ActivateKeyTipProvider

				#region CaptureMouseForElementTree

		private void CaptureMouseForElementTree(FrameworkElement element)
		{
			IInputElement captured = Mouse.Captured;

			this._elementTreeToCapture = element;

			if (captured is DependencyObject)
			{
				// AS 11/30/07
				// IsAncestorOf will not account for crossing over popups, etc. or other breaks in the visual tree
				//
				//if (this._elementTreeToCapture.IsAncestorOf(captured as DependencyObject))
				if (Utilities.IsDescendantOf(this._elementTreeToCapture, captured as DependencyObject))
				{
					captured.AddHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));
					return;
				}
			}

			if (Mouse.Capture(element, CaptureMode.SubTree) && this._elementTreeToCapture != null)
			{
				// track lost capture changes
				((IInputElement)this._elementTreeToCapture).AddHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));
			}

		}

				#endregion //CaptureMouseForElementTree	
    
				#region CoerceIsSelected
		// AS 11/9/07
		private static object CoerceIsSelected(DependencyObject d, object newValue)
		{
			ToolMenuItem menuItem = d as ToolMenuItem;

			// when the mouse is moved over a disabled embedded tool, we want to prevent it from being
			// highlighted. to this end, we can prevent it from being selected which will prevent it
			// from being highlighted.
			if (true.Equals(newValue) &&
				menuItem.RibbonTool != null &&
				false == menuItem.RibbonTool.ToolProxy.IsActivateable(menuItem.RibbonTool as FrameworkElement))
			{
				return KnownBoxes.FalseBox;
			}

			return newValue;
		} 
				#endregion //CoerceIsSelected

				#region CoerceIsSubmenuOpen

		private static object CoerceIsSubmenuOpen(DependencyObject target, object value)
		{
			ToolMenuItem menuItem = target as ToolMenuItem;

			if (menuItem != null)
			{
				if (value is bool && (bool)value == true)
				{
					MenuToolBase menuTool = menuItem.RibbonTool as MenuToolBase;

					// AS 11/16/07 BR28480
					// Its ok if the toolmenuitem is used by itself.
					//
					//Debug.Assert(menuTool != null);

					MenuToolPresenter mtp = menuItem as MenuToolPresenter;

					// for MenuTool's prevent the menu from opening if the MenuButtonArea
					// is not enabled
					if (mtp != null && !mtp.IsMenuButtonAreaEnabled)
						return KnownBoxes.FalseBox;

					// JJD 10/10/07 - BR26870
					// The Opening event is no longer cancelable
					//if (menuTool != null && menuTool.RaiseOpeningEvent() == false)
					//    return KnownBoxes.FalseBox;
					if (menuTool != null )
						menuTool.RaiseOpeningEvent();

					menuItem.OnSubMenuOpening();
				}
			}

			return value;
		}

				#endregion //CoerceIsSubmenuOpen	

				// AS 11/1/07 BR27964
				#region DeactivateKeyTipProviderIfNeeded
		private void DeactivateKeyTipProviderIfNeeded()
		{
			if (this.IsSubmenuOpen == false)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

				if (null != ribbon && ribbon.HasKeyTipManager)
				{
					IKeyTipContainer container = this.RibbonTool as IKeyTipContainer ?? this;
					ribbon.KeyTipManager.DeactivateKeyTipContainer(container);
				}
			}
		} 
				#endregion //DeactivateKeyTipProviderIfNeeded

				// JJD 10/26/07 - BR27371
				#region IsLogicalAncestor

		// Added the IsLogicalAncestor method which will account for logical ancestors as well as visual ancestors
		// AS 11/16/07 BR28512
		// Changed to internal so we don't duplicate this code in the ApplicationMenuRecentItem.
		//
		//private static bool IsLogicalAncestor(DependencyObject ancestor, DependencyObject descendant)
		internal static bool IsLogicalAncestor(DependencyObject ancestor, DependencyObject descendant)
		{
			while (descendant != null)
			{
				if (descendant == ancestor)
					return true;

				descendant = Utilities.GetParent(descendant, true);
			}

			return false;
		}

				#endregion //IsLogicalAncestor	

				// AS 11/7/07 BR27990
				#region OnCanExecuteCommand

		private static void OnCanExecuteCommand(object target, CanExecuteRoutedEventArgs args)
		{
			ToolMenuItem mi = target as ToolMenuItem;

			if (mi != null)
			{
				if (args.Command == MenuTool.SegmentedButtonCommand)
				{
					MenuTool menuTool = mi.RibbonTool as MenuTool;

					if (null != menuTool)
					{
						args.CanExecute = menuTool.CanExecuteSegmentedButton();
						args.Handled = true;
					}
				}
			}
		}

				#endregion //OnCanExecuteCommand

				// AS 6/3/08 BR32772
				#region OnClassKeyDown
		private static void OnClassKeyDown(object sender, KeyEventArgs e)
		{
			ToolMenuItem mi = sender as ToolMenuItem;

			// AS 12/19/07 BR29199
			// Moved the impl of OnKeyDown to OnKeyDownImpl.
			// We need to keep track of being in a keydown in 
			// case we open the popup via the keyboard.
			//
			PopupOpeningReason oldReason = mi._openingReason;

			try
			{
				mi._openingReason = PopupOpeningReason.Keyboard;

				mi.OnKeyDownImpl(e);

				// AS 6/10/08 BR33757
				// Previously we were overriding OnKeyDown and therefore the 
				// _openingReason was set and we knew the menu was opening
				// via the keyboard. Since we cannot override the method
				// lets call the method and handle it here.
				//
				if (false == e.Handled)
				{
					try
					{
						mi.OnKeyDown(e);
					}
					catch (SecurityException)
					{
					}
				}
			}
			finally
			{
				mi._openingReason = oldReason;
			}
		}
				#endregion //OnClassKeyDown

				#region OnEditModeEnded

		private void OnEditModeEnded(object sender, EditModeEndedEventArgs e)
		{
            // AS 3/17/09 TFS15446
            // When TFS5879/BR30928 was fixed, we started hooking the event on the 
            // menu item so the sender is the menu item and not the editor so we 
            // need to deal with the original source.
            //
			//FrameworkElement fe = sender as FrameworkElement;
            FrameworkElement fe = e.OriginalSource as FrameworkElement;

			Debug.Assert(fe != null);

			if (fe == null)
				return;

			// AS 11/21/07 BR28630
			// We need to know if we still had focus by this point.
			//
			bool wasFocusWithin = fe.IsKeyboardFocusWithin;

			this.ReleaseMouseCaptureForElementTree();

			fe.PreviewMouseDown -= new MouseButtonEventHandler(OnHeaderPreviewMouseDown);

			// AS 11/21/07 BR28630
			// If the user presses enter, etc. to accept the changes and this menu item
			// contains the focus then close the menu.
			//
			if (e.ChangesAccepted && wasFocusWithin)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

				if (null != ribbon)
					ribbon.TransferFocusOutOfRibbon();
			}
		}

				#endregion //OnEditModeEnded	
    
				#region OnEditModeStarted

		private void OnEditModeStarted(object sender, EditModeStartedEventArgs e)
		{
            // JJD 2/5/09 - TFS5879/BR30928
            // Use the original source instead of the sender because we are wiring the event
            // handler on the ToolMenuItem instaed of the editor because otherwise
            // when the toll gets cloned onto the QAT this event handler will get cloned
            // and we end up capturing the mouse and not releasing it
			//FrameworkElement fe = sender as FrameworkElement;
			FrameworkElement fe = e.OriginalSource as FrameworkElement;

			Debug.Assert(fe != null);

			if (fe == null)
				return;

			this.CaptureMouseForElementTree(fe);

			fe.PreviewMouseDown += new MouseButtonEventHandler(OnHeaderPreviewMouseDown);
		}

				#endregion //OnEditModeStarted	

				#region OnElementLostMouseCapture

		private void OnElementLostMouseCapture(object sender, MouseEventArgs e)
		{
			IInputElement oldCaptured = sender as IInputElement;

			oldCaptured.RemoveHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));

			IInputElement captured = Mouse.Captured;

			// AS 11/4/11 TFS88222
			// We only capture the mouse for the element or header if we are entering edit mode or 
			// have focus within but if focus goes elsewhere then we shouldn't try to recapture the 
			// the mouse. In this case we were trying to shift focus elsewhere and the comboeditortool 
			// was getting mouse capture when the combobox within it was being forced to release 
			// the capture.
			//
			if (captured != null || !Utilities.IsDescendantOf(this, Keyboard.FocusedElement as DependencyObject))
			{
				if (captured is DependencyObject)
				{
					// JJD 10/26/07 - BR27371
					// Call the IsLogicalAncestor method which will acount for logical ancestors as well
					//if (this._elementTreeToCapture.IsAncestorOf(captured as DependencyObject))
					if (IsLogicalAncestor(this._elementTreeToCapture, captured as DependencyObject))
					{
						captured.AddHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));
						return;
					}
					
				}

				this._elementTreeToCapture = null;
				return;
			}

			// AS 11/16/07 BR28512
			// I don't think we should be trying to retake capture if we just had it and lost it.
			// That would cause us to get into a loop if the menu is being closed, etc.
			//
			//if (this._elementTreeToCapture != null)
			if (this._elementTreeToCapture != null && this._elementTreeToCapture != oldCaptured)
			{
				Mouse.Capture(this._elementTreeToCapture, CaptureMode.SubTree);

				// JJD 10/26/07 - BR27371
				// Check to make sure the this._elementTreeToCapture hasn't been nulled out 
				if (this._elementTreeToCapture != null)
					((IInputElement)this._elementTreeToCapture).AddHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));
			}
		}

				#endregion //OnElementLostMouseCapture
       
				// AS 11/7/07 BR27990
				#region OnExecuteCommand

		private static void OnExecuteCommand(object target, ExecutedRoutedEventArgs args)
		{
			ToolMenuItem mi = target as ToolMenuItem;

			if (mi != null)
			{
				if (args.Command == MenuTool.SegmentedButtonCommand)
				{
					MenuTool menuTool = mi.RibbonTool as MenuTool;

					if (null != menuTool)
					{
						menuTool.OnSegmentedButtonClick();
						args.Handled = true;
					}
				}
			}
		}

				#endregion //OnExecuteCommand

				#region OnHeaderIsKeyboardFocusWithinChanged

		private void OnHeaderIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement header = sender as FrameworkElement;

			Debug.Assert(header != null);

			if (header == null)
				return;

			if ((bool)e.NewValue == true)
			{
				// AS 11/21/07 BR28629
				// If the mouse is captured by something in another focus scope (e.g. a context menu) then
				// we do not want to steal the mouse capture. I found this when fixing BR28629. Basically it
				// caused the context menu to lose capture and close itself.
				//
				DependencyObject capturedElement = Mouse.Captured as DependencyObject;
				DependencyObject capturedFocusScope = capturedElement != null ? FocusManager.GetFocusScope(capturedElement) : null;
				DependencyObject tmiFocusScope = FocusManager.GetFocusScope(this);

				if (capturedFocusScope == null || tmiFocusScope == capturedFocusScope)
				{
					this.CaptureMouseForElementTree(header);

					header.PreviewMouseDown += new MouseButtonEventHandler(OnHeaderPreviewMouseDown);
				}
			}
			else
			{
				header.PreviewMouseDown -= new MouseButtonEventHandler(OnHeaderPreviewMouseDown);

				// AS 11/21/07
				// When we release mouse capture (which we are doing because we don't contain the input focus
				// any more), this menu item is getting a mouse leave which causes it to focus the parent
				// menu item. In addition, the menu item that is actually under the mouse is getting a mouse
				// enter which is causing it to get focused. We really don't want to interfere with the current
				// focused item while we are releasing capture (e.g. if we're tabbing to another menu item) 
				// so we'll prevent it from losing focus while we release the mouse capture.
				//
				//this.ReleaseMouseCaptureForElementTree();
				DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;
				KeyboardFocusChangedEventHandler previewLostFocusHandler = new KeyboardFocusChangedEventHandler(OnPreventLostKeyboardFocus);

				if (null != focusedElement)
					Keyboard.AddPreviewLostKeyboardFocusHandler(focusedElement, previewLostFocusHandler);

				try
				{
					this.ReleaseMouseCaptureForElementTree();
				}
				finally
				{
					if (null != focusedElement)
						Keyboard.RemovePreviewLostKeyboardFocusHandler(focusedElement, previewLostFocusHandler);
				}
			}
		}

				#endregion //OnHeaderIsKeyboardFocusWithinChanged	
    
				#region OnHeaderPreviewMouseDown

		private void OnHeaderPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement header = sender as FrameworkElement;

			Debug.Assert(header != null);

			if (header == null)
				return;

			HitTestResult result = VisualTreeHelper.HitTest(header, e.MouseDevice.GetPosition(header));

			if (result == null ||
				 result.VisualHit == null)
			{
				// JJD 10/26/07 - BR27371
				// Call the IsLogicalAncestor method which will account for logical ancestors as well
				// as visual ancestors. If this method returns true then return
				if (e.OriginalSource is DependencyObject &&
					IsLogicalAncestor(header, e.OriginalSource as DependencyObject))
					return;

				IInputElement captured = Mouse.Captured;

				if (captured != null)
					captured.ReleaseMouseCapture();

				if (header.IsKeyboardFocusWithin)
					this.Focus();
			}
		}

				#endregion //OnHeaderPreviewMouseDown	
 
				// AS 1/11/12 TFS30681
				#region OnIsCheckableChanged
		private static void OnIsCheckableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tmi = d as ToolMenuItem;

			tmi.CoerceValue(ToolMenuItem.IsCheckableInternalProperty);
		} 
				#endregion //OnIsCheckableChanged

				// AS 1/11/12 TFS30681
				#region OnIsCheckedChanged
		private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tmi = d as ToolMenuItem;
			var peer = UIElementAutomationPeer.FromElement(tmi) as ToolMenuItemAutomationPeer;

			if (null != peer)
				peer.RaiseToggleStatePropertyChangedEvent((bool)e.OldValue, (bool)e.NewValue);
		} 
				#endregion //OnIsCheckedChanged

				#region OnPreventLostKeyboardFocus
		private void OnPreventLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// AS 11/21/07
			// This is only used while we are releasing mouse capture as a result of losing keyboard focus
			// because we don't want to change the focused element as a result of releasing capture.
			//
			e.Handled = true;
		} 
				#endregion //OnPreventLostKeyboardFocus

				// JJD 10/26/07 - BR27371
				// Register for the PreviewMouseDownOutsideCapturedElement Event
				#region OnPreviewMouseDownOutsideCapturedElement

		private static void OnPreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
		{
			ToolMenuItem tmi = sender as ToolMenuItem;
			if (tmi != null)
			{
				FrameworkElement header = tmi.Header as FrameworkElement;

				if (header != null)
				{
                    // AS 2/20/09 TFS5876
                    // Capture is outside the captured element but if that element with capture
                    // is within us and the mouse is within the that element then we shouldn't do
                    // anything. Previously we would have released capture which would have 
                    // closed up the contained comboeditor and this menu item.
                    //
                    if (Mouse.Captured is Visual && 
                        header.IsAncestorOf((DependencyObject)Mouse.Captured) &&
                        header.InputHitTest(e.GetPosition(header)) != null)
                        return;

					// AS 11/16/07 BR28512
					// This seems to have been broken by the fix for BR27371. Basically, if we don't call the 
					// helper method then we will retake the mouse capture.
					//
					//IInputElement captured = Mouse.Captured;
					//
					//if (captured != null)
					//	captured.ReleaseMouseCapture();
					tmi.ReleaseMouseCaptureForElementTree();

					if (header.IsKeyboardFocusWithin)
					    tmi.Focus();

					// AS 11/16/07 BR28512
					// We should be marking this event as handled.
					//
					e.Handled = true;
				}
			}
		}

				#endregion //OnPreviewMouseDownOutsideCapturedElement	
        
				#region OnIsSelectedChanged

		private static void OnIsSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ToolMenuItem tmi = target as ToolMenuItem;

			if (tmi != null)
			{
				ToolMenuItem tmiParent = tmi.ParentMenuItem;

				if (tmiParent != null)
				{
					MenuToolBase menuTool = tmiParent.RibbonTool as MenuToolBase;
					// AS 11/16/07 BR28480
					// Its ok if the toolmenuitem is used by itself.
					//
					//Debug.Assert(menuTool != null);

					if (menuTool != null)
						menuTool.OnMenuItemSelected(tmi, (bool)e.NewValue);
				}
			}
		}

				#endregion //OnIsSelectedChanged	
    
				#region OnSubMenuOpening

		private void OnSubMenuOpening()
		{
			// JJD 10/26/07 - BR27906
			// Refresh the VerticalScrollBarVisibility for the child items
			this.RefreshPopupVerticalScrollBarVisibility();

			MenuToolBase menuTool = this.RibbonTool as MenuToolBase;

			// AS 11/16/07 BR28480
			// Its ok if the toolmenuitem is used by itself.
			//
			//Debug.Assert(menuTool != null);

			if (menuTool != null)
			{
				// establish this menu as the owner of the repparentred child tools
				menuTool.SetReparentedToolsOwner(this);

				// Set the ToolMenuItem to the new owner
				menuTool.SetValue(MenuToolBase.ToolMenuItemProperty, this);
			}

			#region Get templated parent

			
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)


			#endregion //Get templated parent

			#region Get the Popup element

			// get the popup for this item's children
			Popup popup = this.GetTemplateChild("PART_Popup") as Popup;

			Debug.Assert(popup != null);

			if (popup == null)
				return;

			#endregion //Get the Popup element	

			#region Reinitialize pre-existing child menu items

			// AS 10/12/07 UseLargeImages
			ItemCollection items = this.Items;
			ItemContainerGenerator generator = this.ItemContainerGenerator;
			object useLargeImages = KnownBoxes.FromValue(this.UseLargeImagesForItems);

			// AS 10/17/07 - MenuItemDescriptionMinWidth
			object minWidth = this.MenuItemDescriptionMinWidthForItems;

			for (int i = 0, count = items.Count; i < count; i++)
			{
				ToolMenuItem menuItem = generator.ContainerFromIndex(i) as ToolMenuItem;

				// AS 11/16/07 BR28480
				// Do not use the element if it has not been prepared. If its been prepared then we 
				// should get it from the generator above. The reason is that if the item is a ToolMenuItem
				// and it hasn't been prepared then its DataContext hasn't been set so its gets the inherited
				// value which in this case means that it gets its parent menu's parent menu. Basically, we
				// end up getting a RibbonTool from the DataContext when this menu item doesn't have a menu 
				// item and when we incorrectly go to change its logical parent, we create a circular
				// reference.
				//
				//if (null == menuItem)
				//	menuItem = items[i] as ToolMenuItem;
				

				if (null != menuItem)
				{
					// AS 11/20/07 ApplicationMenuRecentItem to ToolMenuItem
					// Recent items don't use large images.
					//
					if (this is ApplicationMenuPresenter && menuItem.Location == ToolLocation.ApplicationMenuRecentItems)
						useLargeImages = KnownBoxes.FalseBox;

					var ribbonTool = menuItem.RibbonTool;
					FrameworkElement element = ribbonTool as FrameworkElement;
					if (element != null)
					{
						// Set UseLargeImages property
						element.SetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty, useLargeImages);

						// JJD 10/25/07
						// reset the menuitem's focusable property to the tool's activatable state
						menuItem.Focusable = ((IRibbonTool)element).ToolProxy.IsActivateable(element);

						// AS 11/7/07 BR28308
						// If the menu tool was added to the qat and dropped down then its
						// itemsource was cleared so we need to reestablish them when its 
						// parent is opened.
						//
						if (element is MenuToolBase)
							((MenuToolBase)element).SetReparentedToolsOwner(menuItem);
					}
					// AS 1/10/12 TFS99109
					// Set the value on the toolmenuitem itself if we are not associated with a ribbontool.
					//
					else if (null == ribbonTool)
					{
						menuItem.SetValue(RibbonToolHelper.UseLargeImagesOnMenuProperty, useLargeImages);
					}

					menuItem.SetValue(UseLargeImagePropertyKey, useLargeImages);

					// AS 10/17/07 - MenuItemDescriptionMinWidth
					menuItem.SetValue(MenuItemDescriptionMinWidthPropertyKey, minWidth);
				}
			}

			#endregion //Reinitialize pre-existing child menu items

			// if the templated parent is not an ApplicationMenuPresenter then we know
			// that this is not tool directly on the application menu
			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			// This really wasn't correct because we would initialize the reside mode for the
			// application menu presenter itself which should not be resizable.
			//
			//if (appMenuPresenter == null || this.Role == MenuItemRole.TopLevelHeader)
			if (false == this is ApplicationMenuPresenter && this.Location != ToolLocation.ApplicationMenu)
			{
				#region Set ResizeMode

				PopupResizeMode resizeMode = PopupResizeMode.None;

				// loop over the sub items and if there are GalleryTools and all of their
				// AllowResizeDropDown property values return true then we want to allow
				// resizing
				foreach (object item in this.Items)
				{
					GalleryTool galleryTool = item as GalleryTool;

					if (galleryTool != null)
					{
						if (galleryTool.AllowResizeDropDown)
						{
							if (galleryTool.MaxDropDownColumns == galleryTool.MinDropDownColumns)
								resizeMode = PopupResizeMode.VerticalOnly;
							else
								resizeMode = PopupResizeMode.Both;
						}

						break;
					}
				}

				this.ResizeMode = resizeMode;

				#endregion //Set ResizeMode	
				return;
			}

			#region Position popup over submenu area

			// get the location of the recent items from the application menu parent

			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			ApplicationMenuPresenter appMenuPresenter = this.ParentMenuItem as ApplicationMenuPresenter;

			if (null == appMenuPresenter)
				return;

			FrameworkElement subMenuArea = appMenuPresenter.GetTemplateChild("PART_SubMenuArea") as FrameworkElement;

			Debug.Assert(subMenuArea != null);

			if (subMenuArea == null)
				return;

            // JJD 11/06/07 - Call PointToScreenSafe so we don't get an exception throw
            // in XBAP semi-trust applications
            //Point screenPoint = subMenuArea.PointToScreen(new Point());
			Point screenPoint = Utilities.PointToScreenSafe( subMenuArea, new Point());

			double width = subMenuArea.ActualWidth;
			double height = subMenuArea.ActualHeight;

			popup.Placement = PlacementMode.Absolute;
			popup.PlacementTarget = null;
			popup.PlacementRectangle = new Rect(screenPoint, new Size(width, height));
			popup.Width = width;
			popup.Height = height;
			#endregion //Position popup over submenu area	
    
		}

				#endregion //OnSubMenuOpening	

				// AS 1/11/12 TFS30990
				#region RaiseMenuItemOnKeyDown
		internal void RaiseMenuItemOnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		} 
				#endregion //RaiseMenuItemOnKeyDown

				#region RefreshPopupVerticalScrollBarVisibility

		private void RefreshPopupVerticalScrollBarVisibility()
		{
			bool isDisabled = false;

			PopupResizeMode mode = this.ResizeMode;

			if (mode != PopupResizeMode.None &&
				!this.IsPopupConstrainedVertically)
			{
				isDisabled = true;
			}

			if (isDisabled)
			{
				// AS 12/19/07 BR27906
				//this.ClearValue(CanPopupContentScrollPropertyKey);
				this.ClearValue(PopupVerticalScrollBarVisibilityPropertyKey);
			}
			else
			{
				// AS 12/19/07 BR27906
				// We should not be controlling the CanContentScroll property since the 
				// menu item cannot know what is within the scrollviewer. Setting CanContentSroll
				// to true tells the ScrollContentPresenter that it should use the content
				// as the iscrollinfo - and therefore that it will scroll.
				//
				//this.SetValue(PopupVerticalScrollBarVisibilityPropertyKey, ScrollBarVisibility.Auto);
				//this.SetValue(CanPopupContentScrollPropertyKey, true);
				this.SetValue(PopupVerticalScrollBarVisibilityPropertyKey, KnownBoxes.ScrollBarVisibilityAutoBox);
			}
		}

				#endregion //RefreshPopupVerticalScrollBarVisibility	
    
				#region ReleaseMouseCaptureForElementTree

		private void ReleaseMouseCaptureForElementTree()
		{
			// AS 11/16/07 BR28512
			// Cache the old one and release the element now so we don't
			// try to recapture the mouse when we call ReleaseMouseCapture
			// below.
			//
			FrameworkElement oldElementTreeToCapture = this._elementTreeToCapture;
			this._elementTreeToCapture = null;

			IInputElement captured = Mouse.Captured;

			if (captured != null)
			{
				captured.RemoveHandler(FrameworkElement.LostMouseCaptureEvent, new MouseEventHandler(OnElementLostMouseCapture));

				// AS 11/16/07 BR28512
				// There were 2 issues here. First, we needed to release the capture for the subtree
				// even if something within the element had capture not not just if it had capture itself.
				// Also, we need to reference the cached element from above since we nulled out the member
				// now.
				//
				//if (captured == this._elementTreeToCapture)
				if (IsLogicalAncestor(oldElementTreeToCapture, captured as DependencyObject))
					captured.ReleaseMouseCapture();
			}

			// AS 11/16/07 BR28512
			// We need to do this before calling ReleaseMouseCapture above.
			//
			//this._elementTreeToCapture = null;
		}

				#endregion //ReleaseMouseCaptureForElementTree	
    
				#region UnwireHeader

		private void UnwireHeader(FrameworkElement header)
		{
			header.PreviewMouseDown -= new MouseButtonEventHandler(OnHeaderPreviewMouseDown);
			
			ValueEditor editor = header as ValueEditor;

			if (editor != null)
			{
				//unhook the edit mode events

                // JJD 2/5/09 - TFS5879/BR30928
                // Wire the event handler on the ToolMenuItem instaed of the editor because otherwise
                // when the toll gets cloned onto the QAT this event handler will get cloned
                // and we end up capturing the mouse and not releasing it
                //editor.EditModeStarted -= new EventHandler<EditModeStartedEventArgs>(OnEditModeStarted);
                //editor.EditModeEnded -= new EventHandler<EditModeEndedEventArgs>(OnEditModeEnded);
				this.RemoveHandler(ValueEditor.EditModeStartedEvent, new EventHandler<EditModeStartedEventArgs>(OnEditModeStarted));
				this.RemoveHandler(ValueEditor.EditModeEndedEvent, new EventHandler<EditModeEndedEventArgs>(OnEditModeEnded));
			}
			else
				header.IsKeyboardFocusWithinChanged -= new DependencyPropertyChangedEventHandler(OnHeaderIsKeyboardFocusWithinChanged);
		}

				#endregion //UnwireHeader	
            
			#endregion //Private Methods	
    
		#endregion //Methods

		#region IRibbonToolHost Members

		IRibbonTool IRibbonToolHost.Tool
		{
			get { return this.RibbonTool; }
		}

		#endregion

		// AS 10/15/07 BR27181
		#region ToolMenuItemKeyTipProvider
		private class ToolMenuItemKeyTipProvider : IKeyTipProvider
		{
			#region Member Variables

			private ToolMenuItem _menuItem;

			#endregion //Member Variables

			#region Constructor
			internal ToolMenuItemKeyTipProvider(ToolMenuItem menuItem)
			{
				this._menuItem = menuItem;
			}
			#endregion //Constructor

			#region IKeyTipProvider Members

			public bool Activate()
			{
				switch (this._menuItem.Role)
				{
					case MenuItemRole.SubmenuHeader:
					case MenuItemRole.TopLevelHeader:
						// AS 12/19/07 BR29199
						//this._menuItem.IsSubmenuOpen = true;
						this._menuItem.OpenMenu(PopupOpeningReason.KeyTips);
						return this._menuItem.IsSubmenuOpen;
					case MenuItemRole.SubmenuItem:
					case MenuItemRole.TopLevelItem:
						this._menuItem.OnClick();

						// this seems to be needed or else the item will come up as highlighted 
						// when its parent menu item is expanded again
						this._menuItem.SimulateMouseLeave();
						return true;
					default:
						Debug.Fail("Unrecognized role!");
						return false;
				}
			}

			public bool Equals(IKeyTipProvider provider)
			{
				return provider is ToolMenuItemKeyTipProvider &&
					this._menuItem == ((ToolMenuItemKeyTipProvider)provider)._menuItem;
			}

			public IKeyTipContainer GetContainer()
			{
				switch (this._menuItem.Role)
				{
					default:
					case MenuItemRole.TopLevelItem:
					case MenuItemRole.SubmenuItem:
						return null;
					case MenuItemRole.SubmenuHeader:
					case MenuItemRole.TopLevelHeader:
						return this._menuItem;
				}
			}

			public KeyTipAlignment Alignment
			{
				get { return ToolKeyTipProvider.GetToolAlignment(ToolLocation.Menu, RibbonToolSizingMode.ImageAndTextNormal); }
			}

			public string AutoGeneratePrefix
			{
				get { return null; }
			}

			public string Caption
			{
				get { return this._menuItem.Header != null ? this._menuItem.Header.ToString() : string.Empty; }
			}

			public bool DeactivateParentContainersOnActivate
			{
				get { return true; }
			}

			public bool IsEnabled
			{
				get { return this._menuItem.IsEnabled; }
			}

			public bool IsVisible
			{
				get { return this._menuItem.IsVisible; }
			}

			public string KeyTipValue
			{
				get { return RibbonToolHelper.GetKeyTip(this._menuItem); }
			}

			public Point Location
			{
				get
				{
					return ToolKeyTipProvider.GetToolKeyTipLocation(this._menuItem, ToolLocation.Menu, RibbonToolSizingMode.ImageAndTextNormal);
				}
			}

			public char Mnemonic
			{
				get { return Utilities.GetFirstMnemonicChar(this.Caption); }
			}

			public UIElement AdornedElement
			{
				get { return this._menuItem; }
			}

			public char DefaultPrefix
			{
				get { return 'M'; }
			}

			// AS 1/5/10 TFS25626
			public bool CanAutoGenerateKeyTip 
			{
				get { return true; }
			}

			#endregion // ToolMenuItemKeyTipProvider
		} 
		#endregion //ToolMenuItemKeyTipProvider

		// AS 10/15/07 BR27181
		#region IKeyTipContainer Members

		void IKeyTipContainer.Deactivate()
		{
			this.IsSubmenuOpen = false;
		}

		IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
		{
			return this.GetChildKeyTipProviders();
		}

		bool IKeyTipContainer.ReuseParentKeyTips
		{
			get { return false; }
		}

		#endregion //IKeyTipContainer

		// AS 5/11/09 TFS17066
		#region LogicalContainer
		// AS 6/8/09 TFS17066
		//private class LogicalContainer : FrameworkElement
		internal class LogicalContainer : FrameworkElement
		{
			private IList<object> _children;

			internal LogicalContainer()
			{
				_children = new List<object>();
			}

			protected override IEnumerator LogicalChildren
			{
				get
				{
					return _children.GetEnumerator();
				}
			}

			internal void AddLogicalChildInternal(object child)
			{
				if (null != child)
				{
					_children.Add(child);
					this.AddLogicalChild(child);
				}
			}

			internal void RemoveLogicalChildInternal(object child)
			{
				if (null != child)
				{
					_children.Remove(child);
					this.RemoveLogicalChild(child);
				}
			}
		} 
		#endregion //LogicalContainer

		// AS 7/10/09 TFS18398
		// See OnToolTipOpening for details.
		//
		#region ToolTipHelper class
		private class ToolTipHelper
		{
			#region Member Variables

			private static object ToolTipId = new object();
			private GroupTempValueReplacement _originalValues;
			private DispatcherOperation _cancelOperation;

			#endregion //Member Variables

			#region Constructor
			private ToolTipHelper()
			{
			}
			#endregion //Constructor

			#region Methods

			#region CancelToolTip
			private object CancelToolTip(object param)
			{
				this.OnToolTipClosed(param, EventArgs.Empty);
				return null;
			}
			#endregion //CancelToolTip

			#region OnToolTipOpened
			private void OnToolTipOpened(object sender, EventArgs e)
			{
				// if we got here then the tooltip was opened before it was cancelled so just unhook the opened
				// and cancel the dispatcher operation
				Debug.Assert(null != _cancelOperation);

				if (null != _cancelOperation)
				{
					_cancelOperation.Abort();
					_cancelOperation = null;
				}

				((ToolTip)sender).Opened -= new RoutedEventHandler(OnToolTipOpened);
			}
			#endregion //OnToolTipOpened

			#region OnToolTipClosed
			private void OnToolTipClosed(object sender, EventArgs e)
			{
				ToolTip tt = (ToolTip)sender;

				if (null != _cancelOperation)
				{
					// if the tooltip closed before the operation completed then abort it
					if (_cancelOperation.Status == DispatcherOperationStatus.Pending)
						_cancelOperation.Abort();

					// make sure we're unhooked from the opened
					tt.Opened -= new RoutedEventHandler(OnToolTipOpened);

					_cancelOperation = null;
				}

				tt.Closed -= new RoutedEventHandler(this.OnToolTipClosed);

				// if this was a tooltip we created then we don't have to restore
				// the data context but we do need to release the content
				if (tt.Tag == ToolTipId)
					tt.Content = null;

				if (null != _originalValues)
				{
					// if the tooltip property was set to a tooltip instance and we got here
					// then we had manipulated its datacontext so we need to restore that
					_originalValues.Dispose();
					_originalValues = null;
				}
			}
			#endregion //OnToolTipClosed

			#region ShowToolTip
			internal static bool ShowToolTip(ToolMenuItem menuItem, ToolTipEventArgs e)
			{
				Debug.Assert(null != menuItem && null != e);

				// if the menu item tooltip was explicitly set to null then do not do anything
				if (e.Handled || menuItem.ToolTip == null)
					return e.Handled;

				// we're binding the tooltip of the menuitem to that of the underlying item 
				// (if its a frameworkelement) so only proceed if the tooltip values are the same
				FrameworkElement tool = menuItem.DataContext as FrameworkElement;

				if (tool == null || menuItem.ToolTip != tool.ToolTip)
					return false;

				// we only need to have special logic when there is a datacontext on the 
				// underlying tool
				if (tool.DataContext == null)
					return false;

				// if we got here then we're going to do some manipulation
				// so we need a tooltiphelper instance which will be used to 
				// clean up the tooltip/menuitem
				ToolTipHelper ttHelper = new ToolTipHelper();
				ttHelper._originalValues = new GroupTempValueReplacement();

				ToolTip tt = tool.ToolTip as ToolTip;

				if (tt == null)
				{
					tt = new ToolTip();
					tt.Tag = ToolTipId; // this is used to know during cleanup that its our created tooltip
					tt.DataContext = tool.DataContext;
					tt.Content = tool.ToolTip;

					// change the tooltip to our custom tooltip instance so we can let the framework show it
					ttHelper._originalValues.Add(menuItem, ToolTipProperty, tt);
				}
				else
				{
					ValueSource vs = DependencyPropertyHelper.GetValueSource(tt, DataContextProperty);

					// if the datacontext was already set on the tooltip then we should not interfere with that
					if (vs.BaseValueSource != BaseValueSource.Inherited &&
						vs.BaseValueSource != BaseValueSource.Default)
					{
						return false;
					}

					// store the original datacontext and update the tooltip to use the tool's datacontext explicitly
					ttHelper._originalValues.Add(tt, DataContextProperty, tool.DataContext);
				}

				// in case someone else handles the message and this tooltip doesn't get shown then 
				// we should act as if it was closed and clean up the tooltip
				ttHelper._cancelOperation = menuItem.Dispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(ttHelper.CancelToolTip), tt);

				// hook into the opened since we want to cancel the abort operation (i.e. the implicit close
				// operation from the begininvoke above)
				tt.Opened += new RoutedEventHandler(ttHelper.OnToolTipOpened);

				// finally hook into the closed in case it is opened and closed
				tt.Closed += new RoutedEventHandler(ttHelper.OnToolTipClosed);
				return false;
			}
			#endregion //ShowToolTip

			#endregion //Methods
		}
		#endregion //ToolTipHelper class
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