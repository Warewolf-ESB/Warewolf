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
using System.Collections.Generic;
using System.Windows.Data;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls
{
	internal static class PresentationUtilities
	{
		#region Member Variables

		#region RelativeSourceSelf
		internal static RelativeSource RelativeSourceSelf =



 RelativeSource.Self

;
		#endregion //RelativeSourceSelf

		#region RelativeSourceTemplatedParent
		internal static RelativeSource RelativeSourceTemplatedParent =



 RelativeSource.TemplatedParent

;
		#endregion //RelativeSourceTemplatedParent

		#region AllModifiers

		internal const ModifierKeys AllModifiers = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows;



		#endregion // AllModifiers

		#endregion //Member Variables

		#region Internal methods

		#region CreateBinding
		// silverlight doesn't handle creating a binding to an attached property or a complex binding well
		// so I needed a helper routine to create xaml for silverlight that would create the binding
		internal static Binding CreateBinding(params BindingPart[] parts)
		{
			StringBuilder sb = new StringBuilder();


			#region WPF

			List<object> pathParams = new List<object>();
			for (int i = 0; i < parts.Length; i++)
			{
				if (i > 0)
					sb.Append('.');

				BindingPart part = parts[i];

				Debug.Assert(part.PathParameter is DependencyProperty || part.PathParameter is PropertyDescriptor || part.PathParameter is MemberInfo);

				sb.AppendFormat("({0})", i);
				sb.Append(part.Path);
				pathParams.Add(part.PathParameter);
			}

			PropertyPath path = new PropertyPath(sb.ToString(), pathParams.ToArray());

			return new Binding { Path = path };

			#endregion //WPF


#region Infragistics Source Cleanup (Region)

































































#endregion // Infragistics Source Cleanup (Region)

		} 

		// JJD 08/17/11
		// Created an overload that took a string that would support a complex binding as long
		// as it didn't contain any attached properties that weren't defined on the source type
		internal static Binding CreateBinding(string pathWithoutAttachedProperties)
		{

			return new Binding(pathWithoutAttachedProperties);


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion //CreateBinding

		// AS 4/11/11 TFS71618
		#region Focus
		/// <summary>
		/// Attempts to focus the first focusable element within the specified element (including the element itself).
		/// </summary>
		/// <param name="element">The element to which focus should be shifted.</param>
		/// <returns>Returns the results of the call to Focus</returns>
		internal static bool Focus(UIElement element)
		{
			return FocusImpl(element) ?? false;
		}

		private static bool? FocusImpl(UIElement element)
		{
			if (element == null || element.Visibility == Visibility.Collapsed)
				return null;


			if (element.Visibility == Visibility.Hidden)
				return null;

			if (element.IsEnabled == false)
				return null;

			if (element.Focusable)
				return element.Focus();



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)





			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(element); i < count; i++)
			{
				UIElement child = VisualTreeHelper.GetChild(element, i) as UIElement;

				bool? result = PresentationUtilities.FocusImpl(child);

				if (result != null)
					return result.Value;
			}

			return null;
		} 
		#endregion //Focus

		#region ForceLostFocusBindingUpdate
		internal static void ForceLostFocusBindingUpdate(DependencyObject ancestorScope, DependencyObject element)
		{






			ForceLostFocusBindingUpdateImpl(ancestorScope, element);

		}

		#region ForceLostFocusBindingUpdateImpl WPF

		private static void ForceLostFocusBindingUpdateImpl(DependencyObject ancestorScope, DependencyObject descendant)
		{
			while (descendant != null)
			{
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(descendant))
				{
					DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(property);

					if (null != dpd)
					{
						var expression = BindingOperations.GetBindingExpressionBase(descendant, dpd.DependencyProperty);

						if (null != expression && IsLostFocusBinding(expression, dpd.Metadata))
							expression.UpdateSource();
					}
				}

				if (descendant is Visual || descendant is System.Windows.Media.Media3D.Visual3D)
					descendant = VisualTreeHelper.GetParent(descendant);
				else
					break;

				if (descendant == ancestorScope)
					break;
			}
		}

		private static bool IsLostFocusBinding(BindingExpressionBase expression, PropertyMetadata metadata)
		{
			if (expression == null)
				return false;

			UpdateSourceTrigger trigger;
			BindingMode mode;

			Binding b = expression.ParentBindingBase as Binding;

			if (null != b)
			{
				trigger = b.UpdateSourceTrigger;
				mode = b.Mode;
			}
			else
			{
				MultiBinding mb = expression.ParentBindingBase as MultiBinding;

				if (mb == null)
					return false;

				trigger = mb.UpdateSourceTrigger;
				mode = mb.Mode;
			}

			if (trigger == UpdateSourceTrigger.Default)
			{
				FrameworkPropertyMetadata fpMetadata = metadata as FrameworkPropertyMetadata;

				if (fpMetadata != null)
					trigger = fpMetadata.DefaultUpdateSourceTrigger;
			}

			if (trigger != UpdateSourceTrigger.LostFocus)
				return false;

			return mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay;
		}

		#endregion // ForceLostFocusBindingUpdateImpl WPF

		#region ForceLostFocusBindingUpdateImpl SL


#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

		#endregion // ForceLostFocusBindingUpdateImpl SL

		#endregion //ForceTextBindingUpdate

		#region GetElementWithKeyboardFocus
		internal static DependencyObject GetElementWithKeyboardFocus()
		{

			return Keyboard.FocusedElement as DependencyObject;



		}
		#endregion //GetElementWithKeyboardFocus

		#region GetElementWithFocus
		internal static DependencyObject GetElementWithFocus(UIElement referenceElement)
		{

			DependencyObject focusScope = FocusManager.GetFocusScope(referenceElement);
			return null != focusScope ? FocusManager.GetFocusedElement(focusScope) as DependencyObject : null;



		}
		#endregion //GetElementWithFocus

		#region GetKey
		internal static Key GetKey(KeyEventArgs e)
		{

			if (e.Key == Key.System)
				return e.SystemKey;


			return e.Key;
		} 
		#endregion // GetKey

		#region GetRootVisual
		internal static DependencyObject GetRootVisual(DependencyObject element)
		{

			while (element is ContentElement)
				element = LogicalTreeHelper.GetParent(element);


			while (element != null)
			{
				var parent = VisualTreeHelper.GetParent(element);

				if (null == parent)
					return element;

				element = parent;
			}

			return null;
		} 
		#endregion //GetRootVisual

		#region GetScrollThumb
		internal static Thumb GetScrollThumb(ScrollBar scrollBar)
		{

			var track = scrollBar.Track;
			return null != track ? track.Thumb : null;


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion // GetScrollThumb

		#region GetTemplatedParent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


		internal static DependencyObject GetTemplatedParent(FrameworkElement fe)
		{


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			return fe.TemplatedParent;

		}
		#endregion // GetTemplatedParent

		#region GetVisualAncestor
		
		
		
		internal static T GetVisualAncestor<T>(DependencyObject element, Predicate<T> match, DependencyObject ceiling = null )
			where T : DependencyObject
		{
			CoreUtilities.ValidateNotNull(element, "element");
			DependencyObject ancestor = element;

			do
			{
				ancestor = VisualTreeHelper.GetParent(ancestor);

				T ancestorT = ancestor as T;

				if (ancestorT != null)
				{
					if (match == null || match(ancestorT))
						return ancestorT;
				}
			} while (ancestor != null
				
				
				
				&& ( null == ceiling || ancestor != ceiling ) );

			return null;
		} 
		#endregion // GetVisualAncestor

		#region GetVisualDescendant
		/// <summary>
		/// Helper method to enumerate into the visual descendants of an element.
		/// </summary>
		/// <typeparam name="T">The type of element being sought</typeparam>
		/// <param name="element">The element whose descendants are to be traversed</param>
		/// <param name="match">The method invoked when a <typeparamref name="T"/> is found to determine if that is the one being sought</param>
		/// <param name="skipDescend">The method invoked when an element is about to be traversed to determine if the descendants should be checked</param>
		/// <returns>The element located or null if one was not found</returns>
		internal static T GetVisualDescendant<T>(DependencyObject element, Predicate<T> match, Predicate<DependencyObject> skipDescend = null)
			where T : DependencyObject
		{
			CoreUtilities.ValidateNotNull(element, "element");
			DependencyObject descendant = element;

			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(element); i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(element, i);

				if (null != child)
				{
					T descendantT = child as T;

					// if this is not a T or the match indicates its not an acceptable match
					// then check the descendants
					if (null == descendantT || match != null && !match(descendantT))
					{
						if (skipDescend == null || !skipDescend(child))
							descendantT = GetVisualDescendant<T>(child, match, skipDescend);
					}

					if (null != descendantT)
						return descendantT;
				}
			}

			return null;
		}
		#endregion // GetVisualAncestor

		#region GetVisualDescendantFromPoint

		internal static T GetVisualDescendantFromPoint<T>(UIElement subTree, Point pt, Predicate<T> match)
			where T : DependencyObject
		{
			CoreUtilities.ValidateNotNull(subTree, "subTree");

			// JJD 07/11/12 - TFS107595
			// Moved logic into helper method to work around a bug in the SL5 framework where calling
			// VisualTreeHelper.FindElementsInHostCoordinates method returned null in the
			// case where an effect (e.g. a blur effect) is being used on a page and a 
			// Popup contained in a template on a control on that page is passed in as the subtree



#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

			// JJD 07/11/12 - TFS107595
			// In WPF we can call the helper method once and reliably return its result
			return GetVisualDescendantFromPointHelper<T>(subTree, pt, match);

		}
		

		// JJD 07/11/12 - TFS107595
		// Added helper method to work around a bug in the SL5 framework where calling
		// VisualTreeHelper.FindElementsInHostCoordinates method returned null in the
		// case where an effect (e.g. a blur effect) is being used on a page and a 
		// Popup contained in a template on a control on that page is passed in as the subtree
		private static T GetVisualDescendantFromPointHelper<T>(UIElement subTree, Point pt, Predicate<T> match)
			where T : DependencyObject
		{
			CoreUtilities.ValidateNotNull(subTree, "subTree");




			DependencyObject htResult = null;

			HitTestResultCallback resultCallback = delegate(HitTestResult result)
			{
				htResult = result.VisualHit;

				return HitTestResultBehavior.Stop;
			};
		
			HitTestFilterCallback filterCallback = delegate(DependencyObject target)
			{
				HitTestFilterBehavior behavior = HitTestFilterBehavior.Continue;

				UIElement elem = target as UIElement;

				if (elem != null)
				{
					if (elem.IsVisible == false || elem.IsHitTestVisible == false)
						behavior = HitTestFilterBehavior.ContinueSkipSelfAndChildren;
				}

				return behavior;
			};
			
			VisualTreeHelper.HitTest(subTree, filterCallback, resultCallback, new PointHitTestParameters(pt));

			if (htResult == null)
				return null;

			UIElement element = htResult as UIElement;

			if (element == null && htResult != null)
				element = PresentationUtilities.GetVisualAncestor<UIElement>(htResult, null);

			{
				DependencyObject ancestor = element;

				do
				{
					ancestor = VisualTreeHelper.GetParent(ancestor);

					T ancestorT = ancestor as T;

					if (ancestorT != null)
					{
						if (match == null || match(ancestorT))
							return ancestorT;
					}
				} while (ancestor != null);
			}

			return null;
		}




#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

		
		#endregion // GetVisualDescendantFromPoint

		#region GetImmediateDescendant
		/// <summary>
		/// Helper routine similar to find ancestor except that it returns the direct child that 
		/// contains the descendant.
		/// </summary>
		/// <param name="descendant">The nested descendant from which to start searching the ancestors</param>
		/// <param name="ancestor">The ancestor whose direct child is to be returned</param>
		internal static DependencyObject GetImmediateDescendant(DependencyObject descendant, DependencyObject ancestor)
		{

			while (descendant is ContentElement)
				descendant = LogicalTreeHelper.GetParent(descendant);





			while (descendant != null)

			{
				DependencyObject parent = VisualTreeHelper.GetParent(descendant);

				if (parent == ancestor)
					return descendant;

				descendant = parent;
			}

			return null;
		}
		#endregion //GetImmediateDescendant

		#region HasFocus
		internal static bool HasFocus(UIElement element)
		{
			DependencyObject focusedObject = GetElementWithFocus(element);

			if (focusedObject != null && IsAncestorOf(element, focusedObject))
				return true;

			return false;
		} 
		#endregion //HasFocus

		#region HasNoOtherModifiers
		/// <summary>
		/// Helper method to ensure that no modifier keys except optionally the ones specified are pressed
		/// </summary>
		/// <param name="modifiersToIgnore">The modifier keys which can be ignored</param>
		/// <returns></returns>
		internal static bool HasNoOtherModifiers(ModifierKeys modifiersToIgnore = ModifierKeys.Shift)
		{
			return IsModifierPressed(ModifierKeys.None, modifiersToIgnore);
		}
		#endregion // HasNoOtherModifiers

		#region IsAncestorOf



		internal static bool IsAncestorOf(Visual ancestor, DependencyObject descendant)

		{
			// AS 11/17/11 TFS96225
			if (ancestor == null)
				return false;


			// AS 11/17/11 TFS96225
			// We should be handling content elements since you can only pass a visual or visual3d 
			// to isancestorof. Also like the SL version we should just return false if the descendant 
			// is null.
			//
			while (descendant is ContentElement)
				descendant = LogicalTreeHelper.GetParent(descendant);

			if (descendant == null)
				return false;

			return ancestor.IsAncestorOf(descendant);


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		} 
		#endregion //IsAncestorOf

		#region IsModifierPressed
		/// <summary>
		/// Helper method to see if only the specified key is pressed
		/// </summary>
		/// <param name="modifier">The modifier key to check for</param>
		/// <param name="modifiersToIgnore">The modifiers that may be ignore. For example this could be Shift if you are concerned with Ctrl or Ctrl-Shift.</param>
		/// <returns></returns>
		internal static bool IsModifierPressed(ModifierKeys modifier, ModifierKeys modifiersToIgnore = ModifierKeys.None)
		{
			Debug.Assert((modifier & modifiersToIgnore) == 0, "Ignore the key your looking for?");

			// strip out the modifiers we can ignore
			ModifierKeys combination = (Keyboard.Modifiers & ~modifiersToIgnore);

			return combination == modifier;
		} 
		#endregion // IsModifierPressed

		// AS 10/29/10 TFS56705
		// Refactored this from some code used in the activitydragcontroller.
		//
		#region MayBeVisible
		/// <summary>
		/// Used to eliminate elements that are definitely not in view.
		/// </summary>
		/// <param name="element">Element to evaluate</param>
		/// <returns>Returns false if the element is collapsed, has no visual parent, etc.; otherwise returns true.</returns>
		internal static bool MayBeVisible( FrameworkElement element )
		{
			if ( element == null || element.Visibility == Visibility.Collapsed )
				return false;

			
			
			
			


			if ( !element.IsVisible || !element.IsLoaded )
				return false;


			return true;
		} 
		#endregion // MayBeVisible

		#region ReparentElement
		/// <summary>
		/// Helper method for putting an element into or pulling out of a panel.
		/// </summary>
		/// <param name="panel">The panel to add/remove the child to/from</param>
		/// <param name="child">The element to add/remove</param>
		/// <param name="add">True to add the element assuming its current visual parent is null or false to remove the element assuming its current visual parent is the specified <paramref name="panel"/></param>
		internal static void ReparentElement(Panel panel, FrameworkElement child, bool add)
		{
			if (null != child && null != panel)
			{
				if (add)
				{
					if (VisualTreeHelper.GetParent(child) == null)
						panel.Children.Add(child);
				}
				else
				{
					if (VisualTreeHelper.GetParent(child) == panel)
						panel.Children.Remove(child);
				}
			}
		}
		#endregion // ReparentElement

		#region TryTransformToRoot
		internal static bool TryTransformToRoot( FrameworkElement source, FrameworkElement wpfTarget, out GeneralTransform transform )
		{
			// the wpfTarget is only used for wpf. null it out for sl rather than making the caller have to if/def and not pass it in



			try
			{
				// AS 10/29/10 TFS56705
				// If the elements are in separate visual trees then the TransformToVisual will fail.
				//
				//transform = source.TransformToVisual(wpfTarget);

				if ( source.FindCommonVisualAncestor(wpfTarget) == null )
				{
					Point pt = source.PointToScreen(new Point());
					pt = wpfTarget.PointFromScreen(pt);
					transform = new TranslateTransform(pt.X, pt.Y);
					transform.Freeze();
				}
				else

				{
					transform = source.TransformToVisual(wpfTarget);
				}
				return true;
			}
			catch ( Exception )
			{
				transform = null;
				return false;
			}
		}
		#endregion // TryTransformToRoot

		#endregion //Internal methods
	}

	#region BindingPart
	internal class BindingPart
	{
		/// <summary>
		/// This is only needed if indexing into a property
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// This should be a DP (for WPF), a FieldInfo (in SL this should only be for attached props), a PropertyInfo or a PropertyDescriptor (for WPF).
		/// </summary>
		public object PathParameter { get; set; }
	} 
	#endregion //BindingPart
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