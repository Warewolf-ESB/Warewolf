using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Data;
using Infragistics.Shared;
namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Represents a service that provides a <see cref="ToolTip"/> for a <see cref="TextBlock"/> when its 
	/// text cannot be fully rendered within the bounds it has been provided. It can also be used on a 
	/// <see cref="ContentPresenter"/> whose <see cref="ContentPresenter.Content"/> is a string represented 
	/// by a <see cref="TextBlock"/>.
	/// </summary>
	public static class ClippedTextToolTipService
	{
		#region Member Variables

		private static object ServiceToolTipId = new object();

		// AS 10/19/09 TFS23942
		private static bool UsingFindToolTipEvent;

		#endregion //Member Variables

		#region Constructor
		static ClippedTextToolTipService()
		{
			// AS 10/19/09 TFS23942
			// Previously we were hooking the ToolTipOpening. However that has the downside that you have 
			// to have set the ToolTip property and you only have the choice of let that tooltip be 
			// shown or not show its tooltip but in either case it never continued to ask the ancestors
			// if they had a tooltip to show. Now we will try to handle the internal FindToolTip event 
			// which is used when walking up the ancestor chain to determine if a tooltip is needed.
			//
			//EventManager.RegisterClassHandler(typeof(FrameworkElement), ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpening));
			RoutedEvent[] events = EventManager.GetRoutedEventsForOwner(typeof(ToolTipService));

			if (null != events)
			{
				foreach (RoutedEvent evt in events)
				{
					if (evt.Name == "FindToolTip")
					{
						EventManager.RegisterClassHandler(typeof(FrameworkElement), evt, new RoutedEventHandler(OnFindToolTip));
						UsingFindToolTipEvent = true;
						break;
					}
				}
			}

			if (!UsingFindToolTipEvent)
			{
				Debug.Fail("Unable to use FindToolTip");
				EventManager.RegisterClassHandler(typeof(FrameworkElement), ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpening));
			}
		}
		#endregion //Constructor

		#region Properties

		#region Public

		#region ShowToolTipWhenClipped

		/// <summary>
		/// Identifies the ShowToolTipWhenClipped attached dependency property
		/// </summary>
		/// <seealso cref="GetShowToolTipWhenClipped"/>
		/// <seealso cref="SetShowToolTipWhenClipped"/>
		public static readonly DependencyProperty ShowToolTipWhenClippedProperty = DependencyProperty.RegisterAttached("ShowToolTipWhenClipped",
			typeof(bool), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowToolTipWhenClippedChanged), new CoerceValueCallback(CoerceCanApplyPropertyToDependencyObject)));

		/// <summary>
		/// Returns a boolean indicating whether the service will provide a <see cref="ToolTip"/> for the <see cref="TextBlock"/> on which the property is set.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>ShowToolTipWhenClipped</b> can be used on either a <see cref="TextBlock"/> or a <see cref="ContentPresenter"/> that is using a 
		/// <b>TextBlock</b>. When set to <b>true</b>, a tooltip will be displayed for the element when the contents of the TextBlock are larger than the area 
		/// in which the TextBlock was provided.</p>
		/// </remarks>
		/// <seealso cref="ShowToolTipWhenClippedProperty"/>
		/// <seealso cref="SetShowToolTipWhenClipped"/>
		[AttachedPropertyBrowsableForType(typeof(TextBlock))]
		[AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
		public static bool GetShowToolTipWhenClipped(DependencyObject d)
		{
			return (bool)d.GetValue(ClippedTextToolTipService.ShowToolTipWhenClippedProperty);
		}

		/// <summary>
		/// Sets the value of the 'ShowToolTipWhenClipped' attached property
		/// </summary>
		/// <seealso cref="ShowToolTipWhenClippedProperty"/>
		/// <seealso cref="GetShowToolTipWhenClipped"/>
		public static void SetShowToolTipWhenClipped(DependencyObject d, bool value)
		{
			d.SetValue(ClippedTextToolTipService.ShowToolTipWhenClippedProperty, value);
		}

		#endregion //ShowToolTipWhenClipped

		#region AncestorTypeForToolTip

		/// <summary>
		/// Identifies the AncestorTypeForToolTip attached dependency property
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>AncestorTypeForToolTipProperty</b> is used to identify an ancestor on which the tooltip should be placed. 
		/// This allows that ancestor to show a tooltip when the <see cref="TextBlock"/> it contains has its text clipped but allow the 
		/// mouse to be anywhere over the ancestor to have that tooltip be displayed. By default, this property is null and the tooltip 
		/// will be set on the TextBlock itself.
		/// </p>
		/// </remarks>
		/// <seealso cref="GetAncestorTypeForToolTip"/>
		/// <seealso cref="SetAncestorTypeForToolTip"/>
		public static readonly DependencyProperty AncestorTypeForToolTipProperty = DependencyProperty.RegisterAttached("AncestorTypeForToolTip",
			typeof(Type), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAncestorTypeForToolTipChanged), new CoerceValueCallback(CoerceCanApplyPropertyToDependencyObject)));

		/// <summary>
		/// Returns the type of the ancestor element on which the tooltip will be set.
		/// </summary>
		/// <seealso cref="AncestorTypeForToolTipProperty"/>
		/// <seealso cref="SetAncestorTypeForToolTip"/>
		public static Type GetAncestorTypeForToolTip(DependencyObject d)
		{
			return (Type)d.GetValue(ClippedTextToolTipService.AncestorTypeForToolTipProperty);
		}

		/// <summary>
		/// Sets the value of the 'AncestorTypeForToolTip' attached property
		/// </summary>
		/// <seealso cref="AncestorTypeForToolTipProperty"/>
		/// <seealso cref="GetAncestorTypeForToolTip"/>
		public static void SetAncestorTypeForToolTip(DependencyObject d, Type value)
		{
			d.SetValue(ClippedTextToolTipService.AncestorTypeForToolTipProperty, value);
		}

		#endregion //AncestorTypeForToolTip

		#region ToolTipStyleKey

		/// <summary>
		/// Identifies the ToolTipStyleKey attached dependency property
		/// </summary>
		/// <seealso cref="GetToolTipStyleKey"/>
		/// <seealso cref="SetToolTipStyleKey"/>
		public static readonly DependencyProperty ToolTipStyleKeyProperty = DependencyProperty.RegisterAttached("ToolTipStyleKey",
			typeof(ResourceKey), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnToolTipStyleKeyChanged), new CoerceValueCallback(CoerceCanApplyPropertyToDependencyObject)));

		/// <summary>
		/// Returns the <see cref="ResourceKey"/> for a <see cref="FrameworkElement.Style"/> that will be used by the <see cref="ToolTip"/> created when the <see cref="ShowToolTipWhenClippedProperty"/> has been set to true.
		/// </summary>
		/// <seealso cref="ToolTipStyleKeyProperty"/>
		/// <seealso cref="SetToolTipStyleKey"/>
		[AttachedPropertyBrowsableForType(typeof(TextBlock))]
		[AttachedPropertyBrowsableForType(typeof(ContentPresenter))]
		public static ResourceKey GetToolTipStyleKey(DependencyObject d)
		{
			return (ResourceKey)d.GetValue(ClippedTextToolTipService.ToolTipStyleKeyProperty);
		}

		/// <summary>
		/// Sets the <see cref="ResourceKey"/> for a <see cref="FrameworkElement.Style"/> that will be used by the <see cref="ToolTip"/> created when the <see cref="ShowToolTipWhenClippedProperty"/> has been set to true.
		/// </summary>
		/// <seealso cref="ToolTipStyleKeyProperty"/>
		/// <seealso cref="GetToolTipStyleKey"/>
		public static void SetToolTipStyleKey(DependencyObject d, ResourceKey value)
		{
			d.SetValue(ClippedTextToolTipService.ToolTipStyleKeyProperty, value);
		}

		#endregion //ToolTipStyleKey

		#endregion //Public

		#region Internal

		#region ToolTipSource

		/// <summary>
		/// Identifies the ToolTipSource attached dependency property
		/// </summary>
		/// <seealso cref="GetToolTipSource"/>
		/// <seealso cref="SetToolTipSource"/>
		private static readonly DependencyProperty ToolTipSourceProperty = DependencyProperty.RegisterAttached("ToolTipSource",
			typeof(UIElement), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the value of the 'ToolTipSource' attached property
		/// </summary>
		/// <seealso cref="ToolTipSourceProperty"/>
		/// <seealso cref="SetToolTipSource"/>
		private static UIElement GetToolTipSource(DependencyObject d)
		{
			return (UIElement)d.GetValue(ClippedTextToolTipService.ToolTipSourceProperty);
		}

		/// <summary>
		/// Sets the value of the 'ToolTipSource' attached property
		/// </summary>
		/// <seealso cref="ToolTipSourceProperty"/>
		/// <seealso cref="GetToolTipSource"/>
		private static void SetToolTipSource(DependencyObject d, UIElement value)
		{
			d.SetValue(ClippedTextToolTipService.ToolTipSourceProperty, value);
		}

		#endregion //ToolTipSource

		#region ToolTipTarget

		/// <summary>
		/// Identifies the ToolTipTarget attached dependency property
		/// </summary>
		/// <seealso cref="GetToolTipTarget"/>
		/// <seealso cref="SetToolTipTarget"/>
		private static readonly DependencyProperty ToolTipTargetProperty = DependencyProperty.RegisterAttached("ToolTipTarget",
			typeof(UIElement), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the value of the 'ToolTipTarget' attached property
		/// </summary>
		/// <seealso cref="ToolTipTargetProperty"/>
		/// <seealso cref="SetToolTipTarget"/>
		private static UIElement GetToolTipTarget(DependencyObject d)
		{
			return (UIElement)d.GetValue(ClippedTextToolTipService.ToolTipTargetProperty);
		}

		/// <summary>
		/// Sets the value of the 'ToolTipTarget' attached property
		/// </summary>
		/// <seealso cref="ToolTipTargetProperty"/>
		/// <seealso cref="GetToolTipTarget"/>
		private static void SetToolTipTarget(DependencyObject d, UIElement value)
		{
			d.SetValue(ClippedTextToolTipService.ToolTipTargetProperty, value);
		}

		#endregion //ToolTipTarget

		// AS 1/11/12 TFS31806
		#region CurrentToolTipOfTarget

		private static readonly DependencyProperty CurrentToolTipOfTargetProperty = DependencyProperty.RegisterAttached("CurrentToolTipOfTarget",
			typeof(object), typeof(ClippedTextToolTipService), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCurrentToolTipOfTargetChanged)));

		private static void OnCurrentToolTipOfTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// if we just put a service tooltip on this...
			if (ClippedTextToolTipService.IsServiceToolTip(e.NewValue))
			{
				d.SetValue(ToolTipService.IsEnabledProperty, KnownBoxes.TrueBox);
			}
			else if (IsServiceToolTip(e.OldValue))
			{
				// remove our explicit IsEnabled setting
				d.ClearValue(ToolTipService.IsEnabledProperty);
			}
		}

		#endregion //CurrentToolTipOfTarget

		#endregion //Internal

		#endregion //Properties

		#region Methods

		#region CoerceCanApplyPropertyToDependencyObject
		private static object CoerceCanApplyPropertyToDependencyObject(DependencyObject d, object newValue)
		{
			if (d is TextBlock == false && d is ContentPresenter == false)
                throw new InvalidOperationException(SR.GetString("LE_CannotSetClippedTextToolTipService"));

			return newValue;
		} 
		#endregion //CoerceCanApplyPropertyToDependencyObject

		// AS 10/19/09 TFS23942
		#region HasServiceToolTip
		/// <summary>
		/// Helper method to determine if we have associated a service tooltip with the specified source textblock/contentpresenter element.
		/// </summary>
		/// <param name="toolTipTarget">Target element that would have the tooltip</param>
		private static bool HasServiceToolTip(DependencyObject toolTipTarget)
		{
			// AS 1/11/12 TFS31806
			//ToolTip tt = toolTipTarget.GetValue(FrameworkElement.ToolTipProperty) as ToolTip;
			//
			//return tt != null && tt.Tag == ServiceToolTipId;
			return toolTipTarget != null && IsServiceToolTip(toolTipTarget.GetValue(FrameworkElement.ToolTipProperty));
		}
		#endregion //HasServiceToolTip

		// AS 1/11/12 TFS31806
		#region IsServiceToolTip
		private static bool IsServiceToolTip(object tooltip)
		{
			ToolTip tt = tooltip as ToolTip;

			return tt != null && tt.Tag == ServiceToolTipId;
		}
		#endregion //IsServiceToolTip

		// AS 10/19/09 TFS23942
		// Refactored from the OnToolTipOpening implementation.
		//
		#region IsToolTipNeeded
		private static bool IsToolTipNeeded(FrameworkElement toolTipSource)
		{
			Debug.Assert(toolTipSource is ContentPresenter || toolTipSource is TextBlock);

			// get to the textblock to see if the text is clipped
			TextBlock tb = toolTipSource as TextBlock;

			if (null == tb)
			{
				ContentPresenter cp = toolTipSource as ContentPresenter;

				if (cp != null && VisualTreeHelper.GetChildrenCount(cp) == 1)
				{
					// AS 2/21/08
					// Support an access text within the text block.
					//
					//tb = VisualTreeHelper.GetChild(cp, 0) as TextBlock;
					DependencyObject child = VisualTreeHelper.GetChild(cp, 0);

					if (child is AccessText && VisualTreeHelper.GetChildrenCount(child) == 1)
						tb = VisualTreeHelper.GetChild(child, 0) as TextBlock;
					else
						tb = child as TextBlock;
				}
			}

			return IsToolTipNeeded(tb);
		}

		private static bool IsToolTipNeeded(TextBlock tb)
		{
			if (tb == null)
				return false;

			// do not show the tooltip by default;
			bool isToolTipNeeded = false;

			// AS 6/2/08 BR33525
			// Asking for the Content(Start|End) causes the textblock to lazily create its _complexContent
			// member variable. It seems to have some bad assumptions when that member variable is set.
			// It seems to assume that the text property was a locally set value because whenever a
			// subsequent change causes it to get a TextContainerChange notification, it sets its own
			// Text property to a deferredtextreference and since this is a locally set value it takes
			// precedence over the template binding so it doesn't pick up changes when that binding's
			// source value changes. To get around this, we need to not cause the tb to initialize that 
			// member variable. When that variable is set, it is used as its visual children so if there
			// are no visual children, we will use an alternate means of calculating whether the text
			// is clipped.
			//
			Rect startRect, endRect;
			if (VisualTreeHelper.GetChildrenCount(tb) == 0 || tb.ContentStart == null || tb.ContentEnd == null)
				startRect = endRect = Rect.Empty;
			else
			{
				startRect = tb.ContentStart.DocumentStart.GetCharacterRect(tb.ContentStart.LogicalDirection);
				endRect = tb.ContentEnd.DocumentEnd.GetCharacterRect(tb.ContentEnd.LogicalDirection);
			}

			if (startRect.IsEmpty == false && endRect.IsEmpty == false)
			{
				Rect fullRect = Rect.Union(startRect, endRect);
				Size layoutSize = LayoutInformation.GetLayoutSlot(tb).Size;

				Thickness margin = tb.Margin;

				if (null != margin)
				{
					fullRect.Width = Math.Max(0, fullRect.Width + margin.Left + margin.Right);
					fullRect.Height = Math.Max(0, fullRect.Height + margin.Top + margin.Bottom);
				}

				if ((Utilities.AreClose(fullRect.Width, layoutSize.Width) == false && fullRect.Width > layoutSize.Width)
					||
					(Utilities.AreClose(fullRect.Height, layoutSize.Height) == false && fullRect.Height > layoutSize.Height))
				{
					isToolTipNeeded = true;
				}
			}
			else
			{
				// AS 10/19/09 TFS23942
				// since we are evaluating the clip to determine if the element is clipped
				// by an ancestor we need to make sure the layout has been calculated. otherwise
				// i saw that sometimes the tooltip would not show
				tb.UpdateLayout();

				if (LayoutInformation.GetLayoutClip(tb) != null)
				{
					// the textblock may be positioned large enough to show the contents 
					// but its clipped by an ancestor
					isToolTipNeeded = true;
				}
				else
				{
					// AS 10/19/09 TFS23942
					// Reworked the original implementation so that we don't manipulate the actual
					// element in use. Note when using an AccessText that doesn't have an access key 
					// the text of the inner textblock was empty and so we needed to use the text of 
					// the access key. I'm assuming that the other properties have propogated to 
					// the textblock since they are inherited properties.
					//
					try
					{
						AccessText accessText = VisualTreeHelper.GetParent(tb) as AccessText;
						FrameworkElement textSource = accessText ?? (FrameworkElement)tb;
						string text = accessText != null ? accessText.Text : tb.Text;

						System.Globalization.CultureInfo ci = Utilities.GetNonNeutralCulture(textSource);
						Typeface typeFace = new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
						FormattedText ft = new FormattedText(text, ci, tb.FlowDirection, typeFace, tb.FontSize, tb.Foreground);
						ft.MaxTextWidth = tb.ActualWidth;
						ft.Trimming = TextTrimming.None;

						if (ft.Height > tb.ActualHeight)
						{
							isToolTipNeeded = true;
						}
					}
					catch (Exception ex)
					{
						Debug.Fail("Error while measuring for tooltip need:" + ex.Message);
					}
				}
			}

			return isToolTipNeeded;
		}
		#endregion //IsToolTipNeeded

		// AS 10/19/09 TFS23942
		// Moved here from the OnShowToolTipWhenClippedChanged so we can lazily
		// initialize it if possible (e.g. when able to handle the FindToolTip event.
		//
		#region InitializeToolTip
		private static void InitializeToolTip(DependencyObject toolTipSource)
		{
			ToolTip tt = new ToolTip();
			DependencyProperty sourceProperty = toolTipSource is TextBlock ? TextBlock.TextProperty : ContentPresenter.ContentProperty;
			// AS 2/19/09 TFS14268
			// The Content could be an element in which case we should not try to use that as the 
			// tooltip. The specific issue related to the fact that the content had a logical parent
			// but even if it were a visual we wouldn't want to take it since it would reparent
			// the element.
			//
			//tt.SetBinding(ToolTip.ContentProperty, Utilities.CreateBindingObject(sourceProperty, System.Windows.Data.BindingMode.OneWay, toolTipSource));
			tt.SetBinding(ToolTip.ContentProperty, Utilities.CreateBindingObject(sourceProperty, System.Windows.Data.BindingMode.OneWay, toolTipSource, ObjectToTextFilterConverter.Instance));
			tt.Tag = ServiceToolTipId; // id so we know its our tooltip

			ResourceKey key = toolTipSource.GetValue(ToolTipStyleKeyProperty) as ResourceKey;

			if (key != null)
			{
				tt.SetResourceReference(FrameworkElement.StyleProperty, key);
				tt.SetValue(FEClass.DefaultStyleKeyPropertyInternal, key);
			}

			// now that we have the tooltip, we need to see if this tooltip should be
			// put on a different ancestor element
			Type ancestorType = toolTipSource.GetValue(AncestorTypeForToolTipProperty) as Type;

			DependencyObject toolTipTarget = null;

			if (ancestorType != null)
				toolTipTarget = Utilities.GetAncestorFromType(toolTipSource, ancestorType, true);

			// if we couldn't find an ancestor of the specified type (or the ancestor type was not
			// specified) then use the textblock itself
			if (toolTipTarget == null)
				toolTipTarget = toolTipSource;

			Debug.Assert(toolTipTarget is FrameworkElement);

			toolTipTarget.SetValue(FrameworkElement.ToolTipProperty, tt);
			
			// AS 1/11/12 TFS31806
			// Watch the tooltip property in case the user changes it.
			//
			BindingOperations.SetBinding(toolTipTarget, CurrentToolTipOfTargetProperty, new Binding { Source = toolTipTarget, Path = new PropertyPath(ToolTipService.ToolTipProperty) });

			// hook up pointers between the source and target assuming they're different
			if (toolTipTarget != toolTipSource)
			{
				// AS 10/19/09 TFS23942
				// If the target we are about to use is already associated with 
				// a different source element then clear the target pointer 
				// of the old source since the target is now associated with a different
				// source.
				//
				UIElement oldSource = GetToolTipSource(toolTipTarget);

				if (null != oldSource)
					oldSource.ClearValue(ToolTipTargetProperty);

				toolTipTarget.SetValue(ToolTipSourceProperty, toolTipSource);
				toolTipSource.SetValue(ToolTipTargetProperty, toolTipTarget);
			}
		}
		#endregion //InitializeToolTip

		#region OnShowToolTipWhenClippedChanged
		private static void OnShowToolTipWhenClippedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement toolTipSource = d as FrameworkElement;

			// the source element must be a textblock or cp
			if (toolTipSource is TextBlock || toolTipSource is ContentPresenter)
			{
				if (false == (bool)e.NewValue)
				{
					#region Unhook

					
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

					RemoveToolTip(toolTipSource);

					#endregion //Unhook
				}
				else
				{
					
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

					// note we cannot lazily initialize it when we have an ancestortype because
					// the findtooltip could be raised for that element (e.g. if you move the 
					// mouse over it and not the text)
					if (!UsingFindToolTipEvent || GetAncestorTypeForToolTip(d) != null)
					{
						InitializeToolTip(toolTipSource);
					}
				}
			}
		}
		#endregion //OnShowToolTipWhenClippedChanged

		#region OnAncestorTypeForToolTipChanged
		private static void OnAncestorTypeForToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DependencyObject oldToolTipTarget = GetToolTipTarget(d);
			FrameworkElement tooltipSource = d as FrameworkElement;

			if (oldToolTipTarget == null)
				oldToolTipTarget = tooltipSource;

			ToolTip tt = oldToolTipTarget.GetValue(FrameworkElement.ToolTipProperty) as ToolTip;

			// we only need to do anything if there is a tooltip set. otherwise, we'll let
			// this wait until that happens.
			if (null != tt)
			{
				// make sure we're going to be changing the target
				DependencyObject newToolTipTarget = null;

				if (null != e.NewValue)
					newToolTipTarget = Utilities.GetAncestorFromType(tooltipSource, (Type)e.NewValue, true);

				if (null == newToolTipTarget)
					newToolTipTarget = tooltipSource;

				bool isOurToolTip = tt.Tag == ServiceToolTipId;

				// we only need to act if the target is changing
				if (newToolTipTarget != oldToolTipTarget)
				{
					// clear the tooltip property (as long as we put it there)
					if (isOurToolTip)
						oldToolTipTarget.ClearValue(FrameworkElement.ToolTipProperty);

					oldToolTipTarget.ClearValue(CurrentToolTipOfTargetProperty); // AS 1/11/12 TFS31806

					// this element is no longer the tooltip target so clear its reference to the source txtblock
					oldToolTipTarget.ClearValue(ToolTipSourceProperty);

					// the new target should be given a link to the source element (unless it is the
					// source element)
					if (newToolTipTarget != tooltipSource)
					{
						newToolTipTarget.SetValue(ToolTipSourceProperty, tooltipSource);
						tooltipSource.SetValue(ToolTipTargetProperty, newToolTipTarget);
					}
					else
						tooltipSource.ClearValue(ToolTipTargetProperty);

					// if the tooltip on the old target was our element then move it over
					if (isOurToolTip)
					{
						newToolTipTarget.SetValue(FrameworkElement.ToolTipProperty, tt);

						// AS 1/11/12 TFS31806
						// Watch the tooltip property in case the user changes it.
						//
						BindingOperations.SetBinding(newToolTipTarget, CurrentToolTipOfTargetProperty, new Binding { Source = newToolTipTarget, Path = new PropertyPath(ToolTipService.ToolTipProperty) });
					}
				}
			}
			else if (GetShowToolTipWhenClipped(tooltipSource) && e.NewValue != null)
			{
				// AS 10/19/09 TFS23942
				// We do have to create the tooltip if the ancestortype is specified 
				// since the findtooltip could be raised because the mouse goes over 
				// the ancestor element without going over the textblock.
				//
				InitializeToolTip(tooltipSource);
			}
		}
		#endregion //OnAncestorTypeForToolTipChanged

		// AS 10/19/09 TFS23942
		#region OnFindToolTip
		private static void OnFindToolTip(object sender, RoutedEventArgs e)
		{
			DependencyObject toolTipTarget = sender as DependencyObject;

			if (toolTipTarget != null)
			{
				// AS 3/17/09 TFS6239/BT34683
				// If the source is not specified then we can use the target. This would happen 
				// if the source and target were the same.
				//
				//FrameworkElement toolTipSource = toolTipTarget.GetValue(ToolTipSourceProperty) as FrameworkElement;
				FrameworkElement toolTipSource = (toolTipTarget.GetValue(ToolTipSourceProperty) ?? toolTipTarget) as FrameworkElement;

				if (toolTipSource is TextBlock || toolTipSource is ContentPresenter)
				{
					if (GetShowToolTipWhenClipped(toolTipSource))
					{
						var actualTarget = toolTipSource.GetValue(ToolTipTargetProperty) as FrameworkElement;

						// AS 1/11/12 TFS31806/TFS31807
						// We don't want to step on the user's ToolTip even if the tooltipsource needs a tooltip.
						//
						//if (IsToolTipNeeded(toolTipSource))
						if (!SkipTarget(actualTarget ?? toolTipTarget) && IsToolTipNeeded(toolTipSource))
						{
							// if we've already created/initialized it then we just
							// need to enable/disable it
							if (HasServiceToolTip(toolTipTarget))
								toolTipTarget.SetValue(ToolTipService.IsEnabledProperty, KnownBoxes.TrueBox);
							else // otherwise create it now
								InitializeToolTip(toolTipSource);
						}
						else
						{
							if (HasServiceToolTip(toolTipTarget))
								toolTipTarget.SetValue(ToolTipService.IsEnabledProperty, KnownBoxes.FalseBox);
						}
					}
				}
			}
		}
		#endregion //OnFindToolTip

		#region OnToolTipOpening
		private static void OnToolTipOpening(object sender, ToolTipEventArgs e)
		{
			// we set the tooltip on the target element so we need to get to
			// the source element to see if we should show the tooltip
			DependencyObject toolTipTarget = sender as DependencyObject;

			if (toolTipTarget != null)
			{
                // AS 3/17/09 TFS6239/BT34683
                // If the source is not specified then we can use the target. This would happen 
                // if the source and target were the same.
                //
				//FrameworkElement toolTipSource = toolTipTarget.GetValue(ToolTipSourceProperty) as FrameworkElement;
				FrameworkElement toolTipSource = (toolTipTarget.GetValue(ToolTipSourceProperty) ?? toolTipTarget) as FrameworkElement;

				// if we are in charge of only showing the tooltip for a textblock when
				// its caption is clipped, then check to see if it is clipped
				if (null != toolTipSource && (bool)toolTipSource.GetValue(ShowToolTipWhenClippedProperty) == true)
				{
					
#region Infragistics Source Cleanup (Region)


























































































































































#endregion // Infragistics Source Cleanup (Region)

					if (!IsToolTipNeeded(toolTipSource))
						e.Handled = true;
				}
			}
		}
		#endregion //OnToolTipOpening

		#region OnToolTipStyleKeyChanged
		private static void OnToolTipStyleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// get the element on which the tooltip will be set
			DependencyObject toolTipTarget = GetToolTipTarget(d);

			// if one hasn't been specified then look on the element itself
			if (toolTipTarget == null)
				toolTipTarget = d;

			if (toolTipTarget != null)
			{
				ToolTip tt = toolTipTarget.GetValue(FrameworkElement.ToolTipProperty) as ToolTip;

				if (null != tt && tt.Tag == ServiceToolTipId)
				{
					if (e.NewValue == null)
					{
						tt.ClearValue(FrameworkElement.StyleProperty);
						tt.ClearValue(FEClass.DefaultStyleKeyPropertyInternal);
					}
					else if (e.NewValue is Style)
					{
						// AS 2/21/08
						// If an explicit style is provided then use that as is.
						//
						tt.SetValue(FrameworkElement.StyleProperty, e.NewValue);
						tt.ClearValue(FEClass.DefaultStyleKeyPropertyInternal);
					}
					else
					{
						tt.SetResourceReference(FrameworkElement.StyleProperty, e.NewValue);
						tt.SetValue(FEClass.DefaultStyleKeyPropertyInternal, e.NewValue);
					}
				}
			}
		}
		#endregion //OnToolTipStyleKeyChanged

		#region RemoveToolTip
		private static void RemoveToolTip(DependencyObject toolTipSource)
		{
			// we need to update the target and the source
			DependencyObject targetElement = GetToolTipTarget(toolTipSource);

			// if there was a target element, clear it backwards pointer to the textblock
			if (targetElement != null)
				targetElement.ClearValue(ToolTipSourceProperty);
			else
				targetElement = toolTipSource;

			// get the tooltip from the target
			ToolTip currentTT = targetElement.GetValue(FrameworkElement.ToolTipProperty) as ToolTip;

			// if it has our tooltip, then clear it out
			if (currentTT != null && currentTT.Tag == ServiceToolTipId)
				targetElement.ClearValue(FrameworkElement.ToolTipProperty);

			targetElement.ClearValue(CurrentToolTipOfTargetProperty); // AS 1/11/12 TFS31806

			// make sure the textblock doesn't have a pointer back to the target any more
			toolTipSource.ClearValue(ToolTipTargetProperty);
		}
		#endregion //RemoveToolTip

		// AS 1/11/12 TFS31806/TFS31807
		#region SkipTarget
		private static bool SkipTarget(DependencyObject d)
		{
			object tt = d.ReadLocalValue(ToolTipService.ToolTipProperty);

			return tt != DependencyProperty.UnsetValue && !IsServiceToolTip(tt);
		}
		#endregion //SkipTarget

		#endregion //Methods

		#region FEClass
		private class FEClass : FrameworkElement
		{
			internal static DependencyProperty DefaultStyleKeyPropertyInternal = FEClass.DefaultStyleKeyProperty;
		}
		#endregion //FEClass

        // AS 2/19/09 TFS14268
        #region ObjectToTextFilterConverter
        private class ObjectToTextFilterConverter : IValueConverter
        {
            internal static readonly IValueConverter Instance = new ObjectToTextFilterConverter();

            private ObjectToTextFilterConverter()
            {
            }

            #region IValueConverter Members

            object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is ContentElement || value is Visual || value is System.Windows.Media.Media3D.Visual3D)
                    return Binding.DoNothing;

                if (value is DependencyObject && null != LogicalTreeHelper.GetParent((DependencyObject)value))
                    return Binding.DoNothing;

                return value;
            }

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return Binding.DoNothing;
            }

            #endregion //IValueConverter Members
        } 
        #endregion //ObjectToTextFilterConverter
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