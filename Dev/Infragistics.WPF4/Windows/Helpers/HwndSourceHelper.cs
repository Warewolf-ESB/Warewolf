using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Infragistics.Collections;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace Infragistics.Windows
{
	internal class HwndSourceHelper
	{
		#region Member Variables

		private WeakDictionary<HwndSource, HwndSourceWrapper> _sourceElements;
		private WeakList<UIElement> _noSourceList;
		private WeakDictionary<UIElement, int> _elementRefCount;
		private HwndSourceHook _hook;

		private static UIPermission _permission = new UIPermission(UIPermissionWindow.AllWindows);

		#endregion

		#region Constructor
		[SecurityCritical]
		public HwndSourceHelper(HwndSourceHook hook)
		{
			CoreUtilities.ValidateNotNull(hook, "hook");

			_permission.Demand();

			_hook = hook;

			_sourceElements = new WeakDictionary<HwndSource, HwndSourceWrapper>(true, false);
			_noSourceList = new WeakList<UIElement>();
			_elementRefCount = new WeakDictionary<UIElement, int>(true, false);
		}
		#endregion //Constructor

		#region Private Methods

		#region OnElementSourceChanged
		private void OnElementSourceChanged(object sender, SourceChangedEventArgs e)
		{
			var element = sender as UIElement;

			// when the hwndsource changes remove this element from the list and add it to the new one
			RemoveElementFromSource(element, e.OldSource as HwndSource);
			AddElementToSource(element, e.NewSource as HwndSource);
		}
		#endregion //OnElementSourceChanged

		#region AddElementToSource
		private void AddElementToSource(UIElement element, HwndSource source)
		{
			if (source == null)
				_noSourceList.Add(element);
			else
			{
				HwndSourceWrapper wrapper;
				if (!_sourceElements.TryGetValue(source, out wrapper))
				{
					_sourceElements[source] = wrapper = new HwndSourceWrapper(source, _hook);
				}

				wrapper.AddElement(element);
			}
		}
		#endregion //AddElementToSource

		#region RemoveElementFromSource
		private void RemoveElementFromSource(UIElement element, HwndSource source)
		{
			if (source == null)
				_noSourceList.Remove(element);
			else
			{
				HwndSourceWrapper wrapper;
				if (_sourceElements.TryGetValue(source, out wrapper))
				{
					wrapper.RemoveElement(element);
				}
			}
		}
		#endregion //RemoveElementFromSource

		#endregion //Private Methods

		#region Public Methods

		#region AddElement
		public void AddElement(UIElement element)
		{
			int refCount;
			if (!_elementRefCount.TryGetValue(element, out refCount) || refCount == 0)
			{
				// listen for source changes
				PresentationSource.AddSourceChangedHandler(element, new SourceChangedEventHandler(this.OnElementSourceChanged));

				// and store the current source info
				this.AddElementToSource(element, HwndSource.FromVisual(element) as HwndSource);
			}

			_elementRefCount[element] = refCount + 1;
			_elementRefCount.Compact(true);
		}
		#endregion //AddElement

		#region RemoveElement
		public void RemoveElement(UIElement element)
		{
			int refCount;
			if (!_elementRefCount.TryGetValue(element, out refCount))
			{
				Debug.Fail("The element was never added?");
				return;
			}

			if (refCount == 1)
			{
				PresentationSource.RemoveSourceChangedHandler(element, new SourceChangedEventHandler(this.OnElementSourceChanged));
				_elementRefCount.Remove(element);
				this.RemoveElementFromSource(element, HwndSource.FromVisual(element) as HwndSource);
			}
			else
			{
				_elementRefCount[element] = refCount - 1;
			}

			_elementRefCount.Compact(true);
		}
		#endregion //RemoveElement

		#endregion //Public Methods

		#region HwndSourceWrapper class
		private class HwndSourceWrapper
		{
			#region Member Variables

			private WeakReference _hs;
			private HwndSourceHook _hook;
			private WeakList<UIElement> _elementList;
			private bool _isEnabled;

			#endregion //Member Variables

			#region Constructor
			internal HwndSourceWrapper(HwndSource hs, HwndSourceHook hook)
			{
				CoreUtilities.ValidateNotNull(hs);
				CoreUtilities.ValidateNotNull(hook);

				Debug.Assert(!hs.IsDisposed, "Associated HwndSource is already disposed?");

				_hs = new WeakReference(hs);
				_hook = hook;
				_elementList = new WeakList<UIElement>();
			}
			#endregion //Constructor

			#region Public Methods

			#region AddElement
			public void AddElement(UIElement element)
			{
				_elementList.Add(element);
				_elementList.Compact();

				this.VerifyIsEnabled();
			}
			#endregion //AddElement

			#region RemoveElement
			public void RemoveElement(UIElement element)
			{
				_elementList.Remove(element);
				_elementList.Compact();

				this.VerifyIsEnabled();
			}
			#endregion //RemoveElement

			#endregion //Public Methods

			#region Private Methods

			#region VerifyIsEnabled
			private void VerifyIsEnabled()
			{
				bool isEnabled = _elementList.Count > 0;

				if (_isEnabled != isEnabled)
				{
					// this should never happen but as a precaution we don't want 
					// to root the hwndsource
					var hs = CoreUtilities.GetWeakReferenceTargetSafe(_hs) as HwndSource;

					if (hs == null)
						return;

					// if somehow we're trying to add a hook and this is already 
					// disposed then don't try to change the enabled state and 
					// don't hook since that will result in an exception
					if (hs.IsDisposed && isEnabled)
						return;

					_isEnabled = isEnabled;

					if (isEnabled)
						hs.AddHook(_hook);
					else
						hs.RemoveHook(_hook);
				}
			}
			#endregion //VerifyIsEnabled

			#endregion //Private Methods
		}
		#endregion //HwndSourceWrapper class
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