using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Diagnostics;

namespace Infragistics.Windows
{
	internal static class RoutedEventManager
	{
		#region Member Variables

		private static bool _hasReflectionFailed;
		private static bool _hasCopySingleEventFailed;
		private static bool _hasCopyAllEventsFailed;
		private static MethodInfo _getEventFromIdMethod;
		private static PropertyInfo _eventHandlerProperty;
		private static FieldInfo _eventHandlerEntriesField;

		// AS 10/15/07 BR27177
		private static MethodInfo _frugalCallbackMethod;
		#endregion //Member Variables

		#region Constructor

		static RoutedEventManager()
		{
			try
			{
				Type elementType = typeof(UIElement);

				// the handlers for all events routed through an element are stored in
				// the EventHandlersStore so if we want to copy over the handlers
				// from one element to another, we need to access this store. it would
				// have been better if we could have just overridden the On(Add|Remove)Handler
				// to maintain our own list but that is internal virtual
				RoutedEventManager._eventHandlerProperty = elementType.GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);

				Debug.Assert(RoutedEventManager._eventHandlerProperty != null, "There is no EventHandlerStore property!");

				if (RoutedEventManager._eventHandlerProperty == null)
					_hasReflectionFailed = true;
			}
			catch
			{
				// if we can't get to the event store then this class cannot get/clear the routed events
				_hasReflectionFailed = true;
			}

			if (_hasReflectionFailed == false)
			{
				try
				{
					// the key passed into the method is the GlobalIndex of the routed event. in order to
					// get the routed event, we need to use the internal GlobalEventManager - specifically
					// its EventFromGlobalIndex static method
					Type globalManagerType = Type.GetType("System.Windows.GlobalEventManager, " + typeof(RoutedEvent).Assembly.FullName, false);
					RoutedEventManager._getEventFromIdMethod = globalManagerType.GetMethod("EventFromGlobalIndex", BindingFlags.Static | BindingFlags.NonPublic);

					// get the entries field which is needed to get the list of hooked events...
					RoutedEventManager._eventHandlerEntriesField = RoutedEventManager._eventHandlerProperty.PropertyType.GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic);

					if (RoutedEventManager._getEventFromIdMethod == null || RoutedEventManager._eventHandlerEntriesField == null)
						_hasCopyAllEventsFailed = true;

					// AS 10/15/07 BR27177
					// Do not use the string name in case the class is obfuscated.
					//
					Type callBackProviderType = typeof(RoutedEventManager.FrugalMapCallbackProvider);
					MethodInfo[] methods = callBackProviderType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
					MethodInfo callbackMethod = null;

					if (null != methods)
					{
						foreach (MethodInfo method in methods)
						{
							ParameterInfo[] parameters = method.GetParameters();

							if (parameters.Length == 3 &&
								parameters[0].ParameterType == typeof(System.Collections.ArrayList) &&
								parameters[1].ParameterType == typeof(int) &&
								parameters[2].ParameterType == typeof(object))
							{
								callbackMethod = method;
								break;
							}
						}
					}

					Debug.Assert(callbackMethod != null, "We were unable to find the callback method we should use for the frugal map iteration!");

					if (callbackMethod == null)
						_hasCopyAllEventsFailed = true;

					_frugalCallbackMethod = callbackMethod;
				}
				catch
				{
					// if we can't get to the global event manager then we can't perform the copy all event handlers routine
					_hasCopyAllEventsFailed = true;
				}
			}
		}
		#endregion //Constructor

		#region Methods

		#region Internal Methods

		#region ClearAllEventHandlers

		internal static bool ClearAllEventHandlers(UIElement element)
		{
			return AddOrRemoveAllEventHandlers(element, null);
		}

		#endregion //ClearAllEventHandlers

		#region ClearEventHandlers
		internal static bool ClearEventHandlers(UIElement element, RoutedEvent[] routedEvents)
		{
			return AddOrRemoveEventHandlers(element, null, routedEvents);
		}

		#endregion //ClearEventHandlers

		#region CopyEventHandlers

		internal static bool CopyEventHandlers(UIElement sourceElement, UIElement targetElement, RoutedEvent routedEvent)
		{
			RoutedEvent[] events = routedEvent == null ? null : new RoutedEvent[] { routedEvent };
			return CopyEventHandlers(sourceElement, targetElement, events);
		}

		internal static bool CopyEventHandlers(UIElement sourceElement, UIElement targetElement, RoutedEvent[] routedEvents)
		{
			return AddOrRemoveEventHandlers(sourceElement, targetElement, routedEvents);
		}

		#endregion //CopyEventHandlers

		#region CopyAllEventHandlers

		internal static bool CopyAllEventHandlers(UIElement sourceElement, UIElement targetElement)
		{
			return AddOrRemoveAllEventHandlers(sourceElement, targetElement);
		}

		#endregion //CopyAllEventHandlers

		#endregion //Internal Methods

		#region Private Methods

		#region AddOrRemoveAllEventHandlers

		/// <summary>
		/// Either clears all the elements of the source element (if the targetelement is null) or copies all the 
		/// handlers from the sourceelement to the handlers for the target element.
		/// </summary>
		/// <param name="sourceElement">The element with the events</param>
		/// <param name="targetElement">The element to get a copy of the sourceelement's handlers or null to clear the handlers for the source element.</param>
		private static bool AddOrRemoveAllEventHandlers(UIElement sourceElement, UIElement targetElement)
		{
			if (RoutedEventManager._hasReflectionFailed == false && sourceElement != null)
			{
				bool copyHandlers = targetElement != null;

				if (RoutedEventManager._hasCopyAllEventsFailed == false)
				{
					try
					{
						AddOrRemoveAllEventHandlersImpl(sourceElement, targetElement);
						return true;
					}
					catch (MethodAccessException)
					{
						RoutedEventManager._hasReflectionFailed = true;
					}
					catch (Exception ex)
					{
						Debug.Fail("Unexpected exception while copying all event handlers!" + ex.Message);
						RoutedEventManager._hasCopyAllEventsFailed = true;
					}
				}
				else if (RoutedEventManager._hasCopySingleEventFailed == false)
				{
					RoutedEvent[] events = EventManager.GetRoutedEvents();

					bool copy = targetElement != null;

					if (copy)
						return CopyEventHandlers(sourceElement, targetElement, events);
					else
						return ClearEventHandlers(sourceElement, events);
				}
			}

			return false;
		}

		/// <summary>
		/// Either clears all the elements of the source element (if the targetelement is null) or copies all the 
		/// handlers from the sourceelement to the handlers for the target element.
		/// </summary>
		/// <param name="sourceElement">The element with the events</param>
		/// <param name="targetElement">The element to get a copy of the sourceelement's handlers or null to clear the handlers for the source element.</param>
		private static void AddOrRemoveAllEventHandlersImpl(UIElement sourceElement, UIElement targetElement)
		{
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


			// get the store for the specified element
			object eventHandlerStore = RoutedEventManager._eventHandlerProperty.GetValue(sourceElement, null);

			// if we can't get an event handler store, there must be no events hooked
			if (null != eventHandlerStore)
			{
				// we need the frugal map of the event store since that maintains
				// a keyed list of all the event handlers keyed by the global index
				// of each routed event
				object frugalMap = RoutedEventManager._eventHandlerEntriesField.GetValue(eventHandlerStore);

				if (null != frugalMap)
				{
					// get the iterate method which takes a callback and a list
					MethodInfo iterateMethod = frugalMap.GetType().GetMethod("Iterate", BindingFlags.Instance | BindingFlags.Public);

					if (null == iterateMethod)
						throw new InvalidOperationException();

					// get the type of the delegate
					ParameterInfo[] parameters = iterateMethod.GetParameters();

					// if the signature is wrong, we'll let it blow up and we just won't get in here again
					Type delegateType = iterateMethod.GetParameters()[1].ParameterType;

					// create the delegate around our static callback method
					bool addHandlers = targetElement != null;

					FrugalMapCallbackProvider provider = new FrugalMapCallbackProvider(addHandlers);

					// AS 10/15/07 BR27177
					//Delegate iterateDelegate = Delegate.CreateDelegate(delegateType, provider, "FrugalMapCallback");
					Delegate iterateDelegate = Delegate.CreateDelegate(delegateType, provider, RoutedEventManager._frugalCallbackMethod);

					// the list seems to be an opaque piece of info that just gets passed along to 
					// the callback although they do verify that it is non-null
					System.Collections.ArrayList list = new System.Collections.ArrayList();

					// we'll use the list to pass the elements on which to hook the events from this source element's events
					if (addHandlers)
						list.Add(targetElement);
					else
						list.Add(sourceElement);

					// call the iterate method. this will call us back in our callback method
					// with each event in the list
					iterateMethod.Invoke(frugalMap, new object[] { list, iterateDelegate });

					// since we could not remove the handlers for the element whose events were
					// being iterated, it made more sense to just add or remove here after
					// building the list
					foreach(KeyValuePair<RoutedEvent, RoutedEventHandlerInfo[]> item in provider.Handlers)
					{
						foreach (RoutedEventHandlerInfo info in item.Value)
						{
							if (addHandlers)
								targetElement.AddHandler(item.Key, info.Handler, info.InvokeHandledEventsToo);
							else
								sourceElement.RemoveHandler(item.Key, info.Handler);
						}
					}
				}
			}
		}
		#endregion //AddOrRemoveAllEventHandlers

		#region AddOrRemoveEventHandlers
		internal static bool AddOrRemoveEventHandlers(UIElement sourceElement, UIElement targetElement, RoutedEvent[] routedEvents)
		{
			if (RoutedEventManager._hasReflectionFailed == false &&
				RoutedEventManager._hasCopySingleEventFailed == false &&
				sourceElement != null && routedEvents != null && routedEvents.Length > 0)
			{
				try
				{
					AddOrRemoveEventHandlersImpl(sourceElement, targetElement, routedEvents);
					return true;
				}
				catch (MethodAccessException)
				{
					RoutedEventManager._hasReflectionFailed = true;
				}
				catch (Exception ex)
				{
					Debug.Fail("Unexpected exception while copying single event handler!" + ex.Message);
					RoutedEventManager._hasCopySingleEventFailed = true;
				}
			}

			return false;
		}

		private static void AddOrRemoveEventHandlersImpl(UIElement sourceElement, UIElement targetElement, RoutedEvent[] routedEvents)
		{
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


			// get the store for the specified element
			object eventHandlerStore = RoutedEventManager._eventHandlerProperty.GetValue(sourceElement, null);

			// if we can't get an event handler store, there must be no events hooked
			if (null != eventHandlerStore)
			{
				// the store has a method for obtaining a list of RoutedEventHandlerInfo for a particular event. 
				MethodInfo getEventMethod = RoutedEventManager._eventHandlerProperty.PropertyType.GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public);

				Debug.Assert(getEventMethod != null, "There is no GetRoutedEventHandlers method!");

				if (null == getEventMethod)
					throw new InvalidOperationException();

				bool addHandler = targetElement != null;

				for (int i = 0; i < routedEvents.Length; i++)
				{
					RoutedEvent routedEvent = routedEvents[i];

					if (null != routedEvents)
					{
						object handlerInfos = getEventMethod.Invoke(eventHandlerStore, new object[] { routedEvent });

						if (null != handlerInfos)
						{
							Debug.Assert(handlerInfos is RoutedEventHandlerInfo[], "The return value of GetRoutedEventHandlers is no longer an array!");

							RoutedEventHandlerInfo[] handlerInfoArray = handlerInfos as RoutedEventHandlerInfo[];

							if (handlerInfoArray == null)
								throw new InvalidOperationException();



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


							for(int j = handlerInfoArray.Length - 1; j >= 0; j--)
							{
								RoutedEventHandlerInfo handler = handlerInfoArray[j];

								if (addHandler)
									targetElement.AddHandler(routedEvent, handler.Handler, handler.InvokeHandledEventsToo);
								else
									sourceElement.RemoveHandler(routedEvent, handler.Handler);
							}
						}
					}
				}
			}
		}
		#endregion //AddOrRemoveEventHandlers

		#endregion //Private Methods

		#endregion //Methods

		#region FrugalMapCallbackProvider





		private class FrugalMapCallbackProvider
		{
			#region Member Variables

			private bool _addHandlers = false;
			private Dictionary<RoutedEvent, RoutedEventHandlerInfo[]> _handlers;

			#endregion //Member Variables

			#region Constructor
			internal FrugalMapCallbackProvider(bool addHandlers)
			{
				this._addHandlers = addHandlers;
				this._handlers = new Dictionary<RoutedEvent, RoutedEventHandlerInfo[]>();
			}
			#endregion //Constructor

			#region Properties

			#region Handlers
			internal Dictionary<RoutedEvent, RoutedEventHandlerInfo[]> Handlers
			{
				get { return this._handlers; }
			}
			#endregion //Handlers

			#endregion //Properties

			#region Methods

			#region FrugalMapCallback
			public void FrugalMapCallback(System.Collections.ArrayList list, int key, object value)
			{
				// the key passed into the method is the GlobalIndex of the routed event. in order to
				// get the routed event, we need to use the internal GlobalEventManager - specifically
				// its EventFromGlobalIndex static method

				// get the routed event for which the callback is being called
				RoutedEvent routedEvent = RoutedEventManager._getEventFromIdMethod.Invoke(null, new object[] { key }) as RoutedEvent;

				if (null != routedEvent)
				{
					Type valueType = value.GetType();

					// the value is a FrugalObjectList<RoutedEventHandlerInfo>. we need to get its array
					// of the RoutedEventHandlerInfos using its ToArray method
					MethodInfo toArrayMethod = valueType.GetMethod("ToArray", BindingFlags.Instance | BindingFlags.Public);

					RoutedEventHandlerInfo[] handlers = toArrayMethod.Invoke(value, Type.EmptyTypes) as RoutedEventHandlerInfo[];

					// now we can add the handlers to everything we put in the list
					if (null != handlers && handlers.Length > 0)
					{
						for (int i = 0, count = list.Count; i < count; i++)
						{
							UIElement element = list[i] as UIElement;

							Debug.Assert(element != null, "This is expected to be a uielement whose events we are propogating!");

							if (element != null)
							{


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


								// note:do not do the remove in the callback or we won't get 
								// called with the rest of the handlers
								this._handlers.Add(routedEvent, handlers);
							}
						}
					}
				}
			}
			#endregion //FrugalMapCallback

			#endregion //Methods
		}
		#endregion //FrugalMapCallbackProvider
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