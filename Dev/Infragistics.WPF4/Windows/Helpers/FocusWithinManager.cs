using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Markup;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Infragistics.Collections;

namespace Infragistics.Windows.Helpers
{
	#region FocusWithinManager Class

	/// <summary>
	/// Class for managing IsFocusedWithin state of ui elements of specific types.
	/// To have the FocusWithinManager manage IsFocusedWithin state for ui elements
	/// of a specific type, register that type using the <see cref="FocusWithinManager.RegisterType(Type)"/> 
	/// method.
	/// </summary>
	public class FocusWithinManager : DependencyObject
	{
		#region Nested Data Structures

		#region HashtableValueComparer Class

		
		
		
		
		private class DictionaryValueComparer<TKey, TValue> : IComparer<TKey>
		{
			
			
			
			
			private IDictionary<TKey, TValue> _table;

			private bool _reverseOrder;

			
			
			
			
			public DictionaryValueComparer( IDictionary<TKey, TValue> table, bool reverseOrder )
			{
				_table = table;
				_reverseOrder = reverseOrder;
			}

			
			
			
			
			public int Compare( TKey xxObj, TKey yyObj )
			{
				
				
				
				TValue xxVal = _table[xxObj];
				TValue yyVal = _table[yyObj];

				int rr = ( (IComparable)xxVal ).CompareTo( yyVal );

				if ( _reverseOrder )
					rr *= -1;

				return rr;
			}
		}

		#endregion // HashtableValueComparer Class

		#endregion // Nested Data Structures

		#region Vars

		[ThreadStatic( )]
		private static long time_stamp;

		
		
		
		[ThreadStatic( )]
		
		private static WeakDictionary<DependencyObject, long> _focusedElems;

		
		
		
		[ThreadStatic( )]
		
		private static WeakDictionary<DependencyObject, object> _hookedInElems;

		#endregion // Vars

		#region Constructor

		static FocusWithinManager( )
		{
		}

		private FocusWithinManager( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region CheckFocusWithinHelper

		/// <summary>
		/// A helper method for checking to see the specified element contains focus.
		/// It checks to see if the focused element is a descendant of the specified
		/// elem.
		/// </summary>
		/// <param name="elem"></param>
		/// <returns></returns>
		public static bool CheckFocusWithinHelper( DependencyObject elem )
		{
			if ( null == elem )
				return false;

			DependencyObject focusedElem, focusScope;
			GetFocusedElem( elem, out focusedElem, out focusScope );

			return null != focusedElem && IsAncestorOf( elem, focusedElem );
		}

		#endregion // CheckFocusWithinHelper

		#region IsAncestorOf

		/// <summary>
		/// Checks to see if the specified ancestor is ancestor of elem.
		/// </summary>
		/// <param name="ancestor"></param>
		/// <param name="elem"></param>
		/// <returns></returns>
		public static bool IsAncestorOf( DependencyObject ancestor, DependencyObject elem )
		{
			// SSP 8/27/07 BR26646
			// If visual parent of elem is null then traverse ancestors using logical parent.
			// This situation arises with ComboBox and its popup.
			// 
			// ----------------------------------------------------------------------------------
			return IsAncestorOfHelper( ancestor, elem );
			
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------------------------------
		}

		#endregion // IsAncestorOf

		#region RegisterType

		/// <summary>
		/// Registers the specified type. The IsFocusedWithin property will be managed on
		/// instances of classType classes, including derived classes.
		/// </summary>
		/// <param name="classType">The type of element for which the focus state will be managed</param>
		/// <exception cref="ArgumentException">The <paramref name="classType"/> is not a <see cref="UIElement"/> or <see cref="UIElement3D"/></exception>
		/// <exception cref="ArgumentNullException">The <paramref name="classType"/> cannot be null.</exception>
		public static void RegisterType( Type classType )
		{
			RegisterType( classType, null );
		}

		// AS 3/21/08 NA 2008 Vol 1 - XamDockManager
		// Added an overload to take a propertychangecallback to avoid having to override OnPropertyChanged to
		// find out when the IsFocusWithin property has changed.
		//
		/// <summary>
		/// Registers the specified type. The IsFocusedWithin property will be managed on
		/// instances of classType classes, including derived classes.
		/// </summary>
		/// <param name="classType">The type of element for which the focus state will be managed</param>
		/// <param name="callback">An instance of a PropertyChangeCallback that should be invoked when the <see cref="IsFocusWithinProperty"/> changes or null to not register a callback.</param>
		/// <exception cref="ArgumentException">The <paramref name="classType"/> is not a <see cref="UIElement"/> or <see cref="UIElement3D"/></exception>
		/// <exception cref="ArgumentNullException">The <paramref name="classType"/> cannot be null.</exception>
		public static void RegisterType( Type classType, PropertyChangedCallback callback )
		{
			EventManager.RegisterClassHandler( classType, UIElement.GotFocusEvent, new RoutedEventHandler( OnGotFocusClassHandler ), true );
			EventManager.RegisterClassHandler( classType, UIElement.LostFocusEvent, new RoutedEventHandler( OnLostFocusClassHandler ), true );

			if ( null != callback )
			{
				IsFocusWithinPropertyKey.OverrideMetadata( classType, new FrameworkPropertyMetadata( callback ) );
			}
		}

		#endregion // RegisterType

		#endregion // Public Methods

		#region Private Methods

		#region GetFocusedElem

		private static void GetFocusedElem( DependencyObject elem, out DependencyObject focusedElem, out DependencyObject focusScope )
		{
			focusScope = null != elem ? FocusManager.GetFocusScope( elem ) : null;
			focusedElem = null != focusScope
				? focusScope.GetValue( FocusManager.FocusedElementProperty ) as DependencyObject : null;
		}

		#endregion // GetFocusedElem

		#region HookOrUnhookLostFocus

		private static void HookOrUnhookLostFocus( DependencyObject obj, bool hook )
		{
			if ( null == obj )
				return;

			if ( hook )
			{
				if ( !HookedInElems.ContainsKey( obj ) )
				{
					bool hooked = true;
					if ( obj is UIElement )
						( (UIElement)obj ).LostFocus += new RoutedEventHandler( OnLostFocusHandler );
					else if ( obj is ContentElement )
						( (ContentElement)obj ).LostFocus += new RoutedEventHandler( OnLostFocusHandler );
					else
						hooked = false;

					if ( hooked )
						HookedInElems.Add( obj, null );
				}
			}
			else
			{
				if ( obj is UIElement )
					( (UIElement)obj ).LostFocus -= new RoutedEventHandler( OnLostFocusHandler );
				else if ( obj is ContentElement )
					( (ContentElement)obj ).LostFocus -= new RoutedEventHandler( OnLostFocusHandler );

				HookedInElems.Remove( obj );
			}
		}

		#endregion // HookOrUnhookLostFocus

		#region IsAncestorOfHelper

		// SSP 8/27/07 BR26664
		// 
		/// <summary>
		/// Checks to see if the specified ancestor is ancestor of elem.
		/// </summary>
		/// <param name="ancestor"></param>
		/// <param name="elem"></param>
		/// <returns></returns>
		private static bool IsAncestorOfHelper( DependencyObject ancestor, DependencyObject elem )
		{
			// JJD 3/01/07 - BR20680
			// If the ancestor and the element are the same then return true
			if ( ancestor == elem )
				return true;

			DependencyObject parent = null;
			if ( elem is Visual || elem is Visual3D )
				parent = VisualTreeHelper.GetParent( elem );

			if ( null == parent )
			{
				parent = LogicalTreeHelper.GetParent( elem );
			}

			if ( null != parent )
				return IsAncestorOfHelper( ancestor, parent );

			return false;
		}

		#endregion // IsAncestorOfHelper

		#region OnGotFocusClassHandler

		private static void OnGotFocusClassHandler( object sender, RoutedEventArgs e )
		{
			OnGotLostFocusHelper( true, sender, e );
		}

		#endregion // OnGotFocusClassHandler

		#region OnGotLostFocusHelper

		private static void OnGotLostFocusHelper( bool gotFocus, object sender, RoutedEventArgs e )
		{
			DependencyObject elem = sender as DependencyObject;

			Debug.Assert( null != elem );
			if ( null == elem )
				return;

			VerifyFocusWithin( elem, gotFocus );
		}

		#endregion // OnGotLostFocusHelper

		#region OnLostFocusClassHandler

		private static void OnLostFocusClassHandler( object sender, RoutedEventArgs e )
		{
			OnGotLostFocusHelper( false, sender, e );
		}

		#endregion // OnLostFocusClassHandler

		#region OnLostFocusHandler

		private static void OnLostFocusHandler( object sender, RoutedEventArgs e )
		{
			DependencyObject d = sender as DependencyObject;
			Debug.Assert( null != d );
			if ( null == d )
				return;

			HookOrUnhookLostFocus( d, false );

			DependencyObject focusScope, focusedElem;
			GetFocusedElem( d, out focusedElem, out focusScope );

			VerifyFocusWithin( focusedElem, focusScope );
		}

		#endregion // OnLostFocusHandler

		#region SetFocusWithin

		private static void SetFocusWithin( DependencyObject elem, bool value, DependencyObject focusedElem )
		{
			if ( value )
			{
				elem.SetValue( IsFocusWithinPropertyKey, KnownBoxes.TrueBox );
				HookOrUnhookLostFocus( focusedElem, true );

				if ( !FocusedElems.ContainsKey( elem ) )
					FocusedElems[elem] = time_stamp++;
			}
			else
			{
				elem.ClearValue( IsFocusWithinPropertyKey );

				FocusedElems.Remove( elem );
			}
		}

		#endregion // SetFocusWithin

		#region VerifyFocusWithin

		private static void VerifyFocusWithin( DependencyObject elem, bool fromGotFocusHandler )
		{
			if ( null == elem )
				return;

			DependencyObject focusScope, focusedElem;
			GetFocusedElem( elem, out focusedElem, out focusScope );

			if ( fromGotFocusHandler )
				VerifyFocusWithin( focusedElem, focusScope );

			bool isFocusWithin = fromGotFocusHandler;
			if ( !isFocusWithin && null != focusedElem )
			{
				if ( IsAncestorOf( elem, focusedElem ) )
					isFocusWithin = true;
			}

			SetFocusWithin( elem, isFocusWithin, focusedElem );
		}

		private static void VerifyFocusWithin( DependencyObject focusedElem, DependencyObject focusScope )
		{
			
			
			
			
			List<DependencyObject> removeList = new List<DependencyObject>( );

			foreach ( DependencyObject item in FocusedElems.Keys )
			{
				DependencyObject scope = FocusManager.GetFocusScope( item );
				if ( focusScope != scope )
					continue;

				if ( null == focusedElem || !IsAncestorOf( item, focusedElem ) )
					removeList.Add( item );
			}

			
			
			
			
			removeList.Sort( new DictionaryValueComparer<DependencyObject, long>( FocusedElems, true ) );

			foreach ( DependencyObject item in removeList )
				FocusedElems.Remove( item );

			foreach ( DependencyObject item in removeList )
				SetFocusWithin( item, false, focusedElem );
		}

		#endregion // VerifyFocusWithin

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region IsFocusWithin

		private static readonly DependencyPropertyKey IsFocusWithinPropertyKey = DependencyProperty.RegisterReadOnly(
				"IsFocusWithin",
				typeof( bool ),
				typeof( FocusWithinManager ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// This property returns a value indicating whether the object has focus within or not.
		/// Only objects of types that are registered via <see cref="RegisterType(Type)"/> method will
		/// return a valid value from this property.
		/// </summary>
		public static readonly DependencyProperty IsFocusWithinProperty = IsFocusWithinPropertyKey.DependencyProperty;

		#endregion // IsFocusWithin

		#endregion // Public Properties

		#region Private Properties

		#region FocusedElems

		
		
		
		
		private static WeakDictionary<DependencyObject, long> FocusedElems
		{
			get
			{
				if ( null == _focusedElems )
					
					
					
					
					_focusedElems = new WeakDictionary<DependencyObject, long>( true, false );

				return _focusedElems;
			}
		}

		#endregion // FocusedElems

		#region HookedInElems

		
		
		
		
		private static WeakDictionary<DependencyObject, object> HookedInElems
		{
			get
			{
				if ( null == _hookedInElems )
					
					
					
					
					_hookedInElems = new WeakDictionary<DependencyObject, object>( true, false );

				return _hookedInElems;
			}
		}

		#endregion // HookedInElems

		#endregion // Private Properties

		#endregion // Properties
	}

	#endregion // FocusWithinManager Class
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