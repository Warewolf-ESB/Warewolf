using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;
using Infragistics.Windows.Themes;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A custom panel used to arrange the <see cref="QuickAccessToolbar"/>, <see cref="XamRibbon"/> caption and the caption of the <see cref="ContextualTabGroup"/> instances.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonCaptionPanel : FrameworkElement
	{
		#region Member Variables

		private QuickAccessToolbar _qat;
		private ContentControl _captionElement;
		private List<object> _logicalChildren;
		private List<Button> _tabGroupPresenters;
		private static IValueConverter _boolToVisConverter = new BooleanToVisibilityConverter();
		private bool _isInArrange = false;
		private static readonly Rect OutOfViewTabGroupRect = new Rect(-1000,0,0,0);
		private Rect _arrangeTabGroupRect = Rect.Empty;
		private RibbonTabItem _arrangeFirstTabItem = null;
		private RibbonTabItem _arrangeLastTabItem = null;
		private static readonly Style TabGroupButtonStyle;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RibbonCaptionPanel"/>
		/// </summary>
		public RibbonCaptionPanel()
		{
			this._logicalChildren = new List<object>();
			this._tabGroupPresenters = new List<Button>();

			// AS 10/25/07
			// We need to always hook the LayoutUpdated because something outside the tab items could cause the tab items
			// to shift without causing the tab items to get an arrange. This happened when maximizing the form because the 
			// tab control's margins are changed. Since this could possibly come up for other cases we'll just always use
			// layout updated. I thought this might root the element but apparantly the AdornerLayer does this - hooks and 
			// never unhooks - and it does not get rooted.
			//
			// AS 5/23/08
			// We do not want to be hooked into the layout updated when we are
			// not loaded since the elements could be out of the visual tree, etc.
			// In this case, it was causing an exception in the OnLayoutUpdate
			// because the tab item and the panel didn't share a common visual ancestor.
			// Instead, hook the loaded and hook the layout updated there. Unhook
			// that in the unloaded.
			//
			this.LayoutUpdated += new EventHandler(OnLayoutUpdate);
			//this.Loaded += new RoutedEventHandler(this.OnLoaded);
		}

		static RibbonCaptionPanel()
		{
			XamRibbon.RibbonProperty.OverrideMetadata(typeof(RibbonCaptionPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRibbonChanged)));
			
			CommandManager.RegisterClassCommandBinding(typeof(RibbonCaptionPanel), new CommandBinding(ContextualTabGroup.SelectFirstTabCommand, new ExecutedRoutedEventHandler(OnExecuteCommand)));

			Style tabGroupButtonStyle = new Style(typeof(Button));
			FrameworkElementFactory fef = new FrameworkElementFactory(typeof(ContentPresenter));
			ControlTemplate ct = new ControlTemplate(typeof(Button));
			ct.VisualTree = fef;
			tabGroupButtonStyle.Setters.Add(new Setter(Control.TemplateProperty, ct));
			tabGroupButtonStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
			tabGroupButtonStyle.Setters.Add(new Setter(Control.MarginProperty, new Thickness()));
			tabGroupButtonStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness()));
			tabGroupButtonStyle.Setters.Add(new Setter(Control.FocusableProperty, KnownBoxes.FalseBox));
			tabGroupButtonStyle.Seal();
			RibbonCaptionPanel.TabGroupButtonStyle = tabGroupButtonStyle;
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			bool wasInArrange = this._isInArrange;
			try
			{
				this._isInArrange = true;
				return this.ArrangeOverrideImpl(finalSize);
			}
			finally
			{
				this._isInArrange = wasInArrange;
			}
		}

		private Size ArrangeOverrideImpl(Size finalSize)
		{
			// start with a full rect
			Rect finalRectLeft = new Rect(finalSize);
			Rect finalRectRight = Rect.Empty;
			bool canShowTabGroups = this.AreTabItemsScrolling == false;

			// contextual tab groups get precedence
			Rect totalTabGroupRect = Rect.Empty;

			this._arrangeFirstTabItem = this._arrangeLastTabItem = null;
			this._arrangeTabGroupRect = Rect.Empty;

			// AS 10/25/07
			// Always hook the event.
			//
			//EventHandler layoutHandler = new EventHandler(OnLayoutUpdate);
			//this.LayoutUpdated -= layoutHandler;

			double reservedSpaceForQat = 0d;

			if (this._qat != null)
			{
				// AS 10/22/07 BR27591
				// Leave the minimum space of the qat available for itself.
				//
				reservedSpaceForQat = Math.Max(Math.Min(finalRectLeft.Width, this._qat.MinWidth), 0);
				finalRectLeft.X += reservedSpaceForQat;
				finalRectLeft.Width -= reservedSpaceForQat;
			}

			#region ContextualTabGroups
			for (int i = 0, count = this._tabGroupPresenters.Count; i < count; i++)
			{
				Button btn = this._tabGroupPresenters[i];

				// skip hidden groups
				if (btn.Visibility == Visibility.Collapsed)
					continue;

				// assume it cannot be positioned
				Rect tabGroupRect = Rect.Empty;

				if (canShowTabGroups)
				{
					ContextualTabGroup tabGroup = (ContextualTabGroup)btn.Content;

					RibbonTabItem firstTab = (RibbonTabItem)tabGroup.GetValue(ContextualTabGroup.FirstVisibleTabItemProperty);
					RibbonTabItem lastTab = (RibbonTabItem)tabGroup.GetValue(ContextualTabGroup.LastVisibleTabItemProperty);

					if (firstTab != null && lastTab != null)
					{
						// store the first tab of the first tab group positioned
						if (this._arrangeFirstTabItem == null)
							this._arrangeFirstTabItem = firstTab;

						// store the last tab of the last tab group positioned
						this._arrangeLastTabItem = lastTab;

						if (firstTab.IsArrangeValid && lastTab.IsArrangeValid)
						{
							// get the first/last tab relative to this element
							Rect firstTabRect = firstTab.TransformToVisual(this).TransformBounds(new Rect(firstTab.RenderSize));
							Rect lastTabRect = lastTab.TransformToVisual(this).TransformBounds(new Rect(lastTab.RenderSize));

							// build a union of both
							tabGroupRect = Rect.Union(firstTabRect, lastTabRect);

							if (this._arrangeTabGroupRect.IsEmpty)
								this._arrangeTabGroupRect = tabGroupRect;
							else
								this._arrangeTabGroupRect.Union(tabGroupRect);

							// adjust the top & height since we just want the horizontal positioning of the tab items
							tabGroupRect.Y = finalRectLeft.Y;
							tabGroupRect.Height = finalRectLeft.Height;

							// do not go outside what we've been given
							tabGroupRect.Intersect(finalRectLeft);

							// if this if the first contextual tab group then just store this as the tab group rect
							if (totalTabGroupRect.IsEmpty && tabGroupRect.Width > 0)
								totalTabGroupRect = tabGroupRect;
							else // otherwise expand the other rect
								totalTabGroupRect.Union(tabGroupRect);
						}
					}
				}

				btn.Measure(tabGroupRect.Size);

				if (tabGroupRect.IsEmpty)
					btn.Arrange(OutOfViewTabGroupRect);
				else
					btn.Arrange(tabGroupRect);
			}
			#endregion //ContextualTabGroups

			// if we positioned tab groups then split up the rects
			// for the remaining elements
			if (totalTabGroupRect.IsEmpty == false)
			{
				// the left rect ends where the tab groups start
				// AS 10/23/07 BR27591
				//finalRectLeft.Width = totalTabGroupRect.X;
				finalRectLeft.Width = totalTabGroupRect.X - reservedSpaceForQat;

				// the right rect starts after the group and uses the remaining space (if any)
				finalRectRight = finalRectLeft;
				finalRectRight.X = totalTabGroupRect.Right;
				finalRectRight.Width = finalSize.Width - finalRectRight.X;
			}

			// AS 10/25/07
			// Always hook the event.
			//
			//// if there were tab items then verify the positions after
			//if (this._arrangeFirstTabItem != null)
			//	this.LayoutUpdated += layoutHandler;

			#region QuickAccessToolbar
			if (null != this._qat)
			{
				// AS 10/22/07 BR27591
				// Reintroduce the space we reserved for the qat.
				//
				finalRectLeft.Width += reservedSpaceForQat;
				finalRectLeft.X -= reservedSpaceForQat;

				Rect qatRect = finalRectLeft;

				// leave at least 1 pixel for the caption
				if (this._captionElement != null && qatRect.Width > 1)
					qatRect.Width--;

				// if there is less room than the qat wanted measure it again
				// so we know how much space it really needs
				if (qatRect.Width < this._qat.DesiredSize.Width)
				{
					this._qat.Measure(qatRect.Size);
				}

				// give it the lesser of what it wants and what we have
				qatRect.Width = Math.Min(qatRect.Width, this._qat.DesiredSize.Width);

				this._qat.Arrange(qatRect);

				// get the rect ready for the remaining guys
				finalRectLeft.X += qatRect.Width;
				finalRectLeft.Width -= qatRect.Width;
			}
			#endregion //QuickAccessToolbar

			#region CaptionElement
			// use the greater of the rects - left or right - for the caption
			if (null != this._captionElement)
			{
				
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				Rect captionRect = finalRectLeft;

				if (finalRectRight.IsEmpty == false)
				{
					// get the full caption area ignoring the contextual tab groups
					Rect captionAreaRect = Rect.Union(finalRectRight, finalRectLeft);

					// figure out where the text element would be based on its preferred size
					captionRect = new Rect(captionAreaRect.X, captionAreaRect.Y, this._captionElement.DesiredSize.Width, captionAreaRect.Height);

					// center the caption
					captionRect.X += (captionAreaRect.Width - captionRect.Width) / 2;

					// if the element is fully in view on the right then we're done
					if (finalRectRight.Contains(captionRect) == false)
					{
						// if it fits within the left side then arrange it in the left area
						if (captionRect.Width <= finalRectLeft.Width || finalRectLeft.Width > finalRectRight.Width)
							captionRect = finalRectLeft;
						else
						{
							captionRect.X = finalRectRight.X + 1;

							// AS 6/14/11 TFS77156
							captionRect.Intersect(finalRectRight);
						}
					}
				}

				if (captionRect.Width < this._captionElement.DesiredSize.Width)
					this._captionElement.Measure(captionRect.Size);

				this._captionElement.Arrange(captionRect);
			}
			#endregion //CaptionElement

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region GetVisualChild
		/// <summary>
		/// Returns the visual child at the specified index.
		/// </summary>
		/// <param name="index">Integer position of the child to return.</param>
		/// <returns>The child element at the specified position.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is greater than the <see cref="VisualChildrenCount"/></exception>
		protected override System.Windows.Media.Visual GetVisualChild(int index)
		{
			if (this._qat != null)
			{
				if (index == 0)
					return this._qat;

				index--;
			}

			if (this._captionElement != null)
			{
				if (index == 0)
					return this._captionElement;

				index--;
			}

			if (index < this._tabGroupPresenters.Count)
				return this._tabGroupPresenters[index];

			index -= this._tabGroupPresenters.Count;

			throw new ArgumentOutOfRangeException();
		}
		#endregion //GetVisualChild

		#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				return this._logicalChildren.GetEnumerator();
			}
		}

		#endregion //LogicalChildren

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desiredSize = new Size();

			// if there is unlimited space then we want to just add the width of the qat and the
			// caption
			if (double.IsInfinity(availableSize.Width))
			{
				if (null != this._qat)
				{
					this._qat.Measure(availableSize);
					desiredSize.Width += this._qat.DesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, this._qat.DesiredSize.Height);
				}

				if (null != this._captionElement)
				{
					this._captionElement.Measure(availableSize);
					desiredSize.Width += this._captionElement.DesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, this._captionElement.DesiredSize.Height);
				}
			}
			else
			{
				// measure the qat with 50 pixels less than the available
				Size sizeForMeasure = availableSize;
				sizeForMeasure.Width = Math.Max(sizeForMeasure.Width - 50d, 0d);

				if (null != this._qat)
				{
					this._qat.Measure(sizeForMeasure);
					desiredSize.Width += this._qat.DesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, this._qat.DesiredSize.Height);

					// reset the size for the rest of the elements to be 
					// whatever is left
					sizeForMeasure.Width = Math.Max(availableSize.Width -= desiredSize.Width, 0);
				}

				if (null != this._captionElement)
				{
					this._captionElement.Measure(sizeForMeasure);
					desiredSize.Width += this._captionElement.DesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, this._captionElement.DesiredSize.Height);
				}
			}

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isInArrange)
				return;

			base.OnChildDesiredSizeChanged(child);
		} 
		#endregion //OnChildDesiredSizeChanged

		#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int count = 0;

				if (null != this._qat)
					count++;

				if (null != this._captionElement)
					count++;

				count += this._tabGroupPresenters.Count;

				return count;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region Internal Properties

		#region QuickAccessToolbar
		internal QuickAccessToolbar QuickAccessToolbar
		{
			get { return this._qat; }
			set
			{
				if (this._qat != value)
				{
					if (null != this._qat)
					{
                        // AS 2/5/09 TFS11796
                        // Do not add the qat as a logical child of the panel. The ribbon
                        // will make it its logical child. In this way the tools within
                        // will remain as part of the logical tree of the ribbon even 
                        // when the qat is being moved from the top to bottom.
                        //
                        //this.RemoveLogicalChild(this._qat);
						//this._logicalChildren.Remove(this._qat);
						this.RemoveVisualChild(this._qat);
					}

					this._qat = value;

					if (null != this._qat)
					{
                        // AS 2/5/09 TFS11796
                        // Do not add the qat as a logical child of the panel. The ribbon
                        // will make it its logical child. In this way the tools within
                        // will remain as part of the logical tree of the ribbon even 
                        // when the qat is being moved from the top to bottom.
                        //
                        //this.AddLogicalChild(this._qat);
						//this._logicalChildren.Add(this._qat);
						this.AddVisualChild(this._qat);
					}

					this.InvalidateMeasure();
				}
			}
		}
		#endregion //QuickAccessToolbar

		#region TabItemArrangeVersion 

		// AS 10/25/07
		// This is not needed since we're hooking the layout updated in the RibbonCaptionPanel.
		//
		//internal static DependencyProperty TabItemArrangeVersionProperty = XamRibbon.TabItemArrangeVersionProperty.AddOwner(typeof(RibbonCaptionPanel), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));

		#endregion //TabItemArrangeVersion 

		#endregion //Internal Properties

		#region Private Properties

		#region AreTabItemsScrolling

		private static readonly DependencyProperty AreTabItemsScrollingProperty = DependencyProperty.Register("AreTabItemsScrolling",
			typeof(bool), typeof(RibbonCaptionPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		private bool AreTabItemsScrolling
		{
			get
			{
				return (bool)this.GetValue(RibbonCaptionPanel.AreTabItemsScrollingProperty);
			}
		}

		#endregion //AreTabItemsScrolling

		#region CaptionElement
		private ContentControl CaptionElement
		{
			get
			{
				if (null == this._captionElement)
				{
					ContentControl cc = this._captionElement = new ContentControl();
					cc.IsHitTestVisible = false; // we need to be able to drag the form by the caption
					cc.SetResourceReference(Control.StyleProperty, RibbonCaptionPanel.CaptionStyleKey);
					this.AddVisualChild(this._captionElement);
				}

				return this._captionElement;
			}
		}
		#endregion //CaptionElement

		#region IsGlassActive

		private static readonly DependencyProperty IsGlassActiveProperty = DependencyProperty.Register("IsGlassActive",
			typeof(bool), typeof(RibbonCaptionPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsGlassActiveChanged)));

		private static void OnIsGlassActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonCaptionPanel panel = (RibbonCaptionPanel)d;

			if (panel._captionElement != null)
			{
				// AS 6/3/08 BR32772
				//panel._captionElement.SetValue(XamRibbonWindow.IsGlassActivePropertyKey, e.NewValue);
				panel._captionElement.SetValue(XamRibbon.IsGlassActivePropertyKey, e.NewValue);
			}
		}

		#endregion //IsGlassActive

		#region IsWithinRibbonWindow

		private static readonly DependencyProperty IsWithinRibbonWindowProperty = DependencyProperty.Register("IsWithinRibbonWindow",
			typeof(bool), typeof(RibbonCaptionPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWithinRibbonWindowChanged)));

		private static void OnIsWithinRibbonWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonCaptionPanel panel = (RibbonCaptionPanel)d;

			if (true.Equals(e.NewValue))
				panel.CaptionElement.SetBinding(ContentControl.ContentProperty, Utilities.CreateBindingObject("RibbonWindow.Title", BindingMode.OneWay, XamRibbon.GetRibbon(panel)));
			else
			{
				if (panel._captionElement != null)
					BindingOperations.ClearBinding(panel._captionElement, ContentControl.ContentProperty);
			}
		}

		#endregion //IsWithinRibbonWindow

		#endregion //Private Properties

		#region Public Properties

		#region CaptionStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> used by the <see cref="ContentControl"/> that displays the <see cref="Window.Title"/> of the <see cref="Window"/> that contains the <see cref="XamRibbon"/> when its <see cref="XamRibbon.IsWithinRibbonWindow"/> is true.
		/// </summary>
		public static readonly ResourceKey CaptionStyleKey = new StaticPropertyResourceKey(typeof(RibbonCaptionPanel), "CaptionStyleKey");

		#endregion //CaptionStyleKey

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region AddTabGroup
		private void AddTabGroup(ContextualTabGroup tabGroup)
		{
			Button btn = this.CreateButton(tabGroup);
			this.AddVisualChild(btn);

            // AS 8/19/08 Automation support
            tabGroup.CaptionElement = btn;

			// AS 10/12/07 BR27171
			this._tabGroupPresenters.Add(btn);
		} 
		#endregion //AddTabGroup

		#region CreateButton
		private Button CreateButton(ContextualTabGroup tabGroup)
		{
			Button btn = new Button();
			btn.Content = tabGroup;
			btn.Style = RibbonCaptionPanel.TabGroupButtonStyle;
			btn.Command = ContextualTabGroup.SelectFirstTabCommand;
			btn.SetBinding(FrameworkElement.VisibilityProperty, Utilities.CreateBindingObject("IsVisible", BindingMode.OneWay, tabGroup, _boolToVisConverter));

			// AS 1/10/08 BR29564
			btn.SetBinding(ToolTipService.ShowDurationProperty, Utilities.CreateBindingObject(ToolTipService.ShowDurationProperty, BindingMode.OneWay, tabGroup));

			// AS 10/2/07 BR27019
			btn.AddHandler(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(OnTabGroupDoubleClick));

			return btn;
		}
		#endregion //CreateButton

		#region OnContextualTabGroupChanged
		private void OnContextualTabGroupChanged(object sender, ItemPropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Tabs":
				case "IsVisible":
					this.InvalidateMeasure();
					break;
			}
		} 
		#endregion //OnContextualTabGroupChanged

		#region OnContextualTabGroupsChanged
		private void OnContextualTabGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// update the contextual tab groups collection
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ContextualTabGroup tabGroup in e.NewItems)
						this.AddTabGroup(tabGroup);
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (ContextualTabGroup tabGroup in e.OldItems)
						this.RemoveTabGroup(tabGroup);
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (ContextualTabGroup tabGroup in e.OldItems)
						this.RemoveTabGroup(tabGroup);
					foreach (ContextualTabGroup tabGroup in e.NewItems)
						this.AddTabGroup(tabGroup);
					break;
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset:
					this.RefreshTabGroups();
					break;
			}

			// whenever a tab group is added/remove we need to dirty our measure since
			// the qat may need to be resized.
			this.InvalidateMeasure();
		} 
		#endregion //OnContextualTabGroupsChanged

		#region OnExecuteCommand

		private static void OnExecuteCommand(object target, ExecutedRoutedEventArgs args)
		{
			RibbonCaptionPanel panel = (RibbonCaptionPanel)target;

			if (args.Command == ContextualTabGroup.SelectFirstTabCommand)
			{
				Button tabGroupButton = args.OriginalSource as Button;
				ContextualTabGroup group = tabGroupButton != null ? tabGroupButton.Content as ContextualTabGroup : null;

				if (null != group)
				{
                    
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                    group.SelectFirstTab();
				}

				args.Handled = true;
			}
		}

		#endregion //OnExecuteCommand

		#region OnLayoutUpdate
		private void OnLayoutUpdate(object sender, EventArgs e)
		{
			// AS 10/25/07
			// Always hook the event.
			//
			//this.LayoutUpdated -= new EventHandler(OnLayoutUpdate);

			if (this.IsArrangeValid)
			{
				RibbonTabItem firstTab = this._arrangeFirstTabItem;
				RibbonTabItem lastTab = this._arrangeLastTabItem;

				// AS 10/25/07
				// We're always hooking the event so the tabs will certainly be null at times.
				//
				//Debug.Assert(firstTab != null && lastTab != null);

				if (firstTab != null && lastTab != null)
				{
					try
					{
					// get the first/last tab relative to this element
					Rect firstTabRect = firstTab.TransformToVisual(this).TransformBounds(new Rect(firstTab.RenderSize));
					Rect lastTabRect = lastTab.TransformToVisual(this).TransformBounds(new Rect(lastTab.RenderSize));

					// build a union of both
					Rect tabGroupRect = Rect.Union(firstTabRect, lastTabRect);

					if (tabGroupRect != this._arrangeTabGroupRect)
						this.InvalidateArrange();
					}
					catch (InvalidOperationException)
					{
						// AS 5/23/08
						// See ctor for details. This is a safety catch in case the 
						// we get in here and the tabs are not in the tree with the
						// the panel.
						//
					}
				}
			}
		}
		#endregion //OnLayoutUpdate

		#region OnLoaded
		// AS 5/23/08
		// See the ctor for details.
		//
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(this.OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);
			this.LayoutUpdated += new EventHandler(OnLayoutUpdate);
		}
		#endregion //OnLoaded

		#region OnRibbonChanged
		private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonCaptionPanel panel = (RibbonCaptionPanel)d;

			XamRibbon oldRibbon = e.OldValue as XamRibbon;
			XamRibbon newRibbon = e.NewValue as XamRibbon;

			if (null != newRibbon)
			{
				// AS 11/6/07
				// Don't initialize the caption element unless we are within a ribbon window.
				//
				//panel.CaptionElement.SetBinding(ContentControl.ContentProperty, Utilities.CreateBindingObject("RibbonWindow.Title", BindingMode.OneWay, newRibbon));
				panel.SetBinding(IsWithinRibbonWindowProperty, Utilities.CreateBindingObject(XamRibbon.IsWithinRibbonWindowProperty, BindingMode.OneWay, newRibbon));

				// AS 6/3/08 BR32772
				//panel.SetBinding(IsGlassActiveProperty, Utilities.CreateBindingObject(XamRibbonWindow.IsGlassActiveProperty, BindingMode.OneWay, newRibbon));
				panel.SetBinding(IsGlassActiveProperty, Utilities.CreateBindingObject(XamRibbon.IsGlassActiveProperty, BindingMode.OneWay, newRibbon));

				panel.SetBinding(AreTabItemsScrollingProperty, Utilities.CreateBindingObject(XamTabControl.IsTabItemPanelScrollingProperty, BindingMode.OneWay, newRibbon.RibbonTabControl));
				// AS 10/25/07
				// This is not needed since we're hooking the layout updated in the RibbonCaptionPanel.
				//
				//panel.SetBinding(RibbonCaptionPanel.TabItemArrangeVersionProperty, Utilities.CreateBindingObject(XamRibbon.TabItemArrangeVersionProperty, BindingMode.OneWay, newRibbon));
				newRibbon.ContextualTabGroups.CollectionChanged += new NotifyCollectionChangedEventHandler(panel.OnContextualTabGroupsChanged);
				newRibbon.ContextualTabGroups.ItemPropertyChanged += new EventHandler<ItemPropertyChangedEventArgs>(panel.OnContextualTabGroupChanged);
			}
			else
			{
				// AS 11/6/07
				//if (panel._captionElement != null)
				//	BindingOperations.ClearBinding(panel._captionElement, ContentControl.ContentProperty);
				BindingOperations.ClearBinding(panel, IsWithinRibbonWindowProperty);

				BindingOperations.ClearBinding(panel, AreTabItemsScrollingProperty);
				BindingOperations.ClearBinding(panel, IsGlassActiveProperty);
				// AS 10/25/07
				// This is not needed since we're hooking the layout updated in the RibbonCaptionPanel.
				//
				//BindingOperations.ClearBinding(panel, TabItemArrangeVersionProperty);

				if (oldRibbon != null)
				{
					oldRibbon.ContextualTabGroups.CollectionChanged -= new NotifyCollectionChangedEventHandler(panel.OnContextualTabGroupsChanged);
					oldRibbon.ContextualTabGroups.ItemPropertyChanged -= new EventHandler<ItemPropertyChangedEventArgs>(panel.OnContextualTabGroupChanged);
				}
			}

			panel.RefreshTabGroups();
		}

		#endregion //OnRibbonChanged

		// AS 10/2/07 BR27019
		#region OnTabGroupDoubleClick
		private void OnTabGroupDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				XamRibbon ribbon = XamRibbon.GetRibbon(this);

				if (null != ribbon && ribbon.IsWithinRibbonWindow)
				{
					RoutedCommand command;
					
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

					command = ribbon.RibbonWindow.WindowState == WindowState.Maximized
						? RibbonWindowCommands.RestoreCommand
						: RibbonWindowCommands.MaximizeCommand;

					IInputElement target = sender as IInputElement;

					if (command.CanExecute(null, target))
						command.Execute(null, target);
				}
			}
		}
		#endregion //OnTabGroupDoubleClick

		#region OnUnloaded
		// AS 5/23/08
		// See the ctor for details.
		//
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.Loaded += new RoutedEventHandler(this.OnLoaded);
			this.Unloaded -= new RoutedEventHandler(OnUnloaded);
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdate);
		}
		#endregion //OnUnloaded

		#region RefreshTabGroups
		private void RefreshTabGroups()
		{
			// build a table of the current presenters
			Dictionary<ContextualTabGroup, Button> oldPresenters = new Dictionary<ContextualTabGroup, Button>();

			foreach (Button btn in this._tabGroupPresenters)
				oldPresenters.Add((ContextualTabGroup)btn.Content, btn);

			XamRibbon ribbon = XamRibbon.GetRibbon(this);
			ContextualTabGroupCollection tabGroups = ribbon != null ? ribbon.ContextualTabGroups : new ContextualTabGroupCollection();
			List<Button> newPresenters = new List<Button>();

			// create/reuse presenters for the existing tab groups
			for (int i = 0, count = tabGroups.Count; i < count; i++)
			{
				ContextualTabGroup group = tabGroups[i];
				Button btn;

				if (oldPresenters.TryGetValue(group, out btn))
				{
					// if it exists then reuse the presenter and remove the group from the old list
					oldPresenters.Remove(group);
				}
				else
				{
					btn = this.CreateButton(group);
					this.AddVisualChild(btn);

                    // AS 8/19/08 Automation support
                    group.CaptionElement = btn;
                }

				newPresenters.Add(btn);
			}

			// now remove any presenters for groups that were not still in the collection
			foreach (Button btn in oldPresenters.Values)
			{
                // AS 8/19/08 Automation support
                ContextualTabGroup oldGroup = btn.Content as ContextualTabGroup;

                if (null != oldGroup && oldGroup.CaptionElement == btn)
                    oldGroup.CaptionElement = null;

				this.RemoveVisualChild(btn);
			}

			this._tabGroupPresenters = newPresenters;
		} 
		#endregion //RefreshTabGroups

		#region RemoveTabGroup
		private void RemoveTabGroup(ContextualTabGroup tabGroup)
		{
			for (int i = 0, count = this._tabGroupPresenters.Count; i < count; i++)
			{
				Button btn = this._tabGroupPresenters[i];

				if (btn.Content == tabGroup)
				{
					this._tabGroupPresenters.RemoveAt(i);
					this.RemoveVisualChild(btn);

                    // AS 8/19/08 Automation support
                    tabGroup.CaptionElement = null;

					// JM BR27197 10-10-07
					break;
				}
			}
		} 
		#endregion //RemoveTabGroup

		#endregion //Private Methods 

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