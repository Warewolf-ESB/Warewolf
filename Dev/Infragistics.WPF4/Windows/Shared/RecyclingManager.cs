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
using System.Collections;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics
{
	/// <summary>
	/// An object that manages all recycling for an application.
	/// </summary>
    public class RecyclingManager
    {
        #region Static

        private static RecyclingManager _manager = new RecyclingManager();

		/// <summary>
		/// A global Manager that manages all recycling for an application.
		/// </summary>
        public static RecyclingManager Manager
        {
            get { return _manager; }
        }

        #endregion // Static

        #region Constructor

        private RecyclingManager()
        {			
        }

        #endregion // Constructor

        #region Properties

        #region Private

        #region RecycleInfo

        private static readonly DependencyProperty RecycleInfoProperty = DependencyProperty.RegisterAttached("RecycleInfo", typeof(PanelInfo), typeof(RecyclingManager), new PropertyMetadata((PanelInfo)null));

        private static PanelInfo GetRecycleInfo(Panel panel)
        {
            if (null == panel)
                throw new ArgumentNullException("panel");

            PanelInfo pi = (PanelInfo)panel.GetValue(RecycleInfoProperty);

            if (pi == null)
            {
                pi = new PanelInfo(panel);
                panel.SetValue(RecycleInfoProperty, pi);
            }

            return pi;
        }

        #endregion //RecycleInfo

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Public

        #region AttachElement

        /// <summary>
        /// Attaches an element to the given <see cref="ISupportRecycling"/> object. 
        /// If there aren't any available elements of the needed type, a new object will be generated. 
        /// If the object already has an attached element, then nothing will be done.
        /// Note: the element can only be attached to the specified panel. 
        /// </summary>
        /// <returns>True if the item was newly created and added to the children collection of the panel.</returns>
        public bool AttachElement(ISupportRecycling obj, Panel parent)
        {
            return GetRecycleInfo(parent).AttachElement(obj);
        }
        #endregion //AttachElement

        #region ReleaseElement

        /// <summary>
        /// Removes the attached element from the <see cref="ISupportRecycling"/> object, and marks the
        /// element as available, assuming the object wasn't marked dirty.
        /// Note: the element is only reusuable for the specified panel. 
        /// </summary>
        public bool ReleaseElement(ISupportRecycling obj, Panel parent)
        {
            return GetRecycleInfo(parent).ReleaseElement(obj);
        }

        #endregion // ReleaseElement

        #region ReleaseAll
        /// <summary>
        /// Releases all the elements from a given panel and from the cache.
        /// </summary>
        /// <param name="parent">The panel whose cached children are to be removed</param>
        public void ReleaseAll(Panel parent)
        {
            GetRecycleInfo(parent).ReleaseAll();
        }

        /// <summary>
        /// Removes all elements from the contain panel, and from the cache.
        /// </summary>
        /// <param name="parent">Panel whose children are to be released</param>
        /// <param name="id">The id under which the children were cached.</param>
        public void ReleaseAll(Panel parent, string id)
        {
            GetRecycleInfo(parent).ReleaseAll(id);
        }

        /// <summary>
        /// Removes all elements from the contain panel, and from the cache.
        /// </summary>
        /// <param name="parent">Panel whose children are to be released</param>
        /// <param name="t">The type under which the children were cached.</param>
        public void ReleaseAll(Panel parent, Type t)
        {
            GetRecycleInfo(parent).ReleaseAll(t);
        }
        #endregion // ReleaseAll

		#region ReleaseAllAvailable
		/// <summary>
		/// Removes all unused elements from the containing panel.
		/// </summary>
		/// <param name="parent">Panel whose children are to be released</param>
		internal void ReleaseAllAvailable(Panel parent)
		{
			GetRecycleInfo(parent).ReleaseAllAvailable();
		} 
		#endregion // ReleaseAllAvailable

        #region ItemFromElement
        /// <summary>
        /// Returns the <see cref="ISupportRecycling"/> instance associated with a given element.
        /// </summary>
        /// <param name="element">The element whose associated item is to be returned.</param>
        /// <returns>The associated item or null if the item was released or the element is not associated with an item</returns>
        public ISupportRecycling ItemFromElement(FrameworkElement element)
        {
            Panel panel = VisualTreeHelper.GetParent(element) as Panel;

            ISupportRecycling item = null;

            if (panel != null)
                item = GetRecycleInfo(panel).ItemFromElement(element);

            return item;
        }
        #endregion //ItemFromElement

        #region GetRecentlyAvailableElements

        /// <summary>
        /// Gets the unused elements of a specific panel. 
        /// </summary>
        /// <param name="panel">The panel whose recently released elements are to be returned</param>
        /// <param name="release">True to clear the list</param>
        /// <returns>A list of elements that have been released and are not currently associated with an item since the last call to GetRecentlyAvailableElements where <paramref name="release"/> was true</returns>
        public List<FrameworkElement> GetRecentlyAvailableElements(Panel panel, bool release)
        {
            return GetRecycleInfo(panel).GetRecentlyAvailableElements(release);
        }

        #endregion // GetRecentlyAvailableElements

        #region GetAvailableElements

        /// <summary>
        /// Gets the unused elements of a specific panel. 
        /// </summary>
        /// <param name="panel">The panel whose unused Children are to be returned.</param>
        /// <returns>A list of elements in the Children collection of the panel that have been marked for recycling.</returns>
        public List<FrameworkElement> GetAvailableElements(Panel panel)
        {
            return GetRecycleInfo(panel).GetAvailableElements();
        }

        #endregion // GetAvailableElements

        #endregion // Public

        #endregion // Methods

        #region PanelInfo class
        private class PanelInfo
        {
            #region Member Variables

            private Dictionary<Type, List<FrameworkElement>> _availableControlsSpecific;
            private Dictionary<Type, List<FrameworkElement>> _usedControlsSpecific;

            private Dictionary<string, List<FrameworkElement>> _availableIdentifierControlsSpecific;
            private Dictionary<string, List<FrameworkElement>> _usedIdentifierControlsSpecific;

            private List<FrameworkElement> _recentlyRecycled;

			// AS 5/28/10
			// Changed to a strong reference since the element may still be referenced but the 
			// item may have been collected in which case it will not be possible for the element 
			// to be released from the panelinfo.
			//
			//private WeakDictionary<FrameworkElement, ISupportRecycling> _itemForContainer;
			private Dictionary<FrameworkElement, ISupportRecycling> _itemForContainer;

            internal readonly Panel Panel;

            #endregion //Member Variables

            #region Constructor
            internal PanelInfo(Panel panel)
            {
                Panel = panel;

				// AS 5/28/10
				//_itemForContainer = new WeakDictionary<FrameworkElement, ISupportRecycling>(false, true);
				_itemForContainer = new Dictionary<FrameworkElement, ISupportRecycling>();
            }
            #endregion //Constructor

            #region Methods

            #region AttachElement

            public bool AttachElement(ISupportRecycling obj)
            {
                bool isNewlyRealized = false;
                if (obj.AttachedElement == null)
                {
                    FrameworkElement elem = null;
                    List<FrameworkElement> availableElements = null;
                    List<FrameworkElement> usedElements = null;
                    Type t = obj.RecyclingElementType;

                    if (t != null)
                    {
                        if (null != _availableControlsSpecific)
                            _availableControlsSpecific.TryGetValue(t, out availableElements);

                        if (_usedControlsSpecific == null)
                            _usedControlsSpecific = new Dictionary<Type, List<FrameworkElement>>();

                        if (!_usedControlsSpecific.TryGetValue(t, out usedElements))
                            _usedControlsSpecific[t] = usedElements = new List<FrameworkElement>();
                    }
                    else
                    {
                        string id = obj.RecyclingIdentifier;

                        Debug.Assert(null != id, "The object does not provide a RecyclingElementType nor a RecyclingIdentifier");

                        if (id == null)
                            return false;

                        if (null != _availableIdentifierControlsSpecific)
                            _availableIdentifierControlsSpecific.TryGetValue(id, out availableElements);

                        if (_usedIdentifierControlsSpecific == null)
                            _usedIdentifierControlsSpecific = new Dictionary<string, List<FrameworkElement>>();

                        if (!_usedIdentifierControlsSpecific.TryGetValue(id, out usedElements))
                            _usedIdentifierControlsSpecific[id] = usedElements = new List<FrameworkElement>();
                    }

                    isNewlyRealized = AttachElement(obj, availableElements, usedElements, out elem);

                    // keep track of the item that is associated with a given container
                    // to make it easier for a panel to get from the element to the item
                    _itemForContainer[elem] = obj;

                    obj.AttachedElement = elem;
                    obj.OnElementAttached(elem);

					// give the panel a chance to do some generic initialization
					IRecyclableElementHost elementHost = Panel as IRecyclableElementHost;

					if (null != elementHost)
						elementHost.OnElementAttached(obj, elem, isNewlyRealized);
                }
                else
                {
                    Debug.Assert(_itemForContainer.ContainsKey(obj.AttachedElement), "The item is associated with an element but that element is not a used element for the panel.");
                }

                return isNewlyRealized;
            }

            private bool AttachElement(ISupportRecycling item, List<FrameworkElement> availableElements, List<FrameworkElement> usedElements, out FrameworkElement element)
            {
                IRecyclableElement recycleElem;

                // first try to reuse an existing element
                if (null != availableElements)
                {
                    int lastAvailableIndex = availableElements.Count - 1;

                    for (int index = lastAvailableIndex; index >= 0; index--)
                    {
                        element = availableElements[index];

                        recycleElem = element as IRecyclableElement;

                        if (recycleElem == null || !recycleElem.DelayRecycling)
                        {
                            // now that we know we can use it we can remove it
                            // to avoid removing an item from the middle (or removing 
                            // items just to readd them as we used to), we'll just 
                            // replace this item with the one at the end of the collection 
                            // and remove this one
                            if (index < lastAvailableIndex)
                                availableElements[index] = availableElements[lastAvailableIndex];

                            availableElements.RemoveAt(lastAvailableIndex);

                            // ensure its no longer in the recently recycled list
                            if (null != _recentlyRecycled)
                                _recentlyRecycled.Remove(element);

                            usedElements.Add(element);
                            return false;
                        }
                    }
                }

                // if we didn't find one to reuse then create a new one
                element = item.CreateInstanceOfRecyclingElement();

                recycleElem = element as IRecyclableElement;

                if (recycleElem != null)
                    recycleElem.OwnerPanel = this.Panel;

                usedElements.Add(element);
                this.Panel.Children.Add(element);
                return true;
            }
            #endregion //AttachElement

            #region GetAvailableElements
            internal List<FrameworkElement> GetAvailableElements()
            {
                List<FrameworkElement> elements = new List<FrameworkElement>();

                if (null != _availableControlsSpecific)
                {
                    foreach (KeyValuePair<Type, List<FrameworkElement>> pair in _availableControlsSpecific)
                        elements.AddRange(pair.Value);
                }

                if (null != _availableIdentifierControlsSpecific)
                {
                    foreach (KeyValuePair<string, List<FrameworkElement>> pair in _availableIdentifierControlsSpecific)
                        elements.AddRange(pair.Value);
                }

                return elements;

            }
            #endregion //GetAvailableElements

            #region ItemFromElement
            public ISupportRecycling ItemFromElement(FrameworkElement element)
            {
                ISupportRecycling item;
                _itemForContainer.TryGetValue(element, out item);
                return item;
            }
            #endregion //ItemFromElement

            #region GetRecentlyAvailableElements
            internal List<FrameworkElement> GetRecentlyAvailableElements(bool release)
            {
                List<FrameworkElement> elements = new List<FrameworkElement>();

                if (_recentlyRecycled != null)
                {
                    elements.AddRange(_recentlyRecycled);

                    if (release)
                        _recentlyRecycled.Clear();
                }

                return elements;
            }
            #endregion //GetRecentlyAvailableElements

            #region OnElementReleased
            private static void OnElementReleased(ISupportRecycling item, FrameworkElement element)
            {
                if (null != item)
                {
                    Debug.Assert(item.AttachedElement == element);

                    item.IsDirty = false;
                    item.AttachedElement = null;
                    item.OnElementReleased(element);
                }
            }
            #endregion //OnElementReleased

            #region ReleaseAll

            public void ReleaseAll()
            {
                ReleaseAll(_availableControlsSpecific);
                ReleaseAll(_availableIdentifierControlsSpecific);
                ReleaseAll(_usedControlsSpecific);
                ReleaseAll(_usedIdentifierControlsSpecific);
            }

            public void ReleaseAll(string id)
            {
                ReleaseAll(_usedIdentifierControlsSpecific, id);
                ReleaseAll(_availableIdentifierControlsSpecific, id);
            }

            public void ReleaseAll(Type t)
            {
                ReleaseAll(_usedControlsSpecific, t);
                ReleaseAll(_availableControlsSpecific, t);
            }

            private void ReleaseAll<T>(Dictionary<T, List<FrameworkElement>> dictionary)
            {
                if (null != dictionary)
                {
                    foreach (KeyValuePair<T, List<FrameworkElement>> pair in dictionary)
                        ReleaseAll(pair.Value);
                }
            }

            private void ReleaseAll<T>(Dictionary<T, List<FrameworkElement>> dictionary, T key)
            {
                List<FrameworkElement> elements;
                if (null != dictionary && dictionary.TryGetValue(key, out elements))
                    ReleaseAll(elements);
            }

            private void ReleaseAll(List<FrameworkElement> elements)
            {
                UIElementCollection children = this.Panel.Children;

                // NZ 28 March 2012 - TFS104772 - Store off the elements list. This worksarounds an odd issue only
                // reproducible in our functional tests when MultiTouchVista driver is installed.
                List<FrameworkElement> elementsToRemove = new List<FrameworkElement>(elements);

                foreach (var element in elementsToRemove)
                {
                    ISupportRecycling oldItem;

                    // get the item associated with the container - this will only 
                    // be there if the item is a used item
                    if (_itemForContainer.TryGetValue(element, out oldItem))
                        _itemForContainer.Remove(element);

                    // make sure its out of the recently recycled list
                    if (null != _recentlyRecycled)
                        _recentlyRecycled.Remove(element);

                    elements.Remove(element);
                    children.Remove(element);

                    IRecyclableElement recycleElem = element as IRecyclableElement;
                    if (recycleElem != null)
                        recycleElem.OwnerPanel = null;

                    // if the item is still alive then we should detach it
                    if (oldItem != null)
                        OnElementReleased(oldItem, element);

					// let the panel know that an element was released
					IRecyclableElementHost elementHost = this.Panel as IRecyclableElementHost;

					if (null != elementHost)
						elementHost.OnElementReleased(oldItem, element, true);
                }
            }
            #endregion // ReleaseAll

			#region ReleaseAllAvailable
			internal void ReleaseAllAvailable()
			{
				ReleaseAll(_availableControlsSpecific);
				ReleaseAll(_availableIdentifierControlsSpecific);
			}
			#endregion // ReleaseAllAvailable

            #region ReleaseElement

            public bool ReleaseElement(ISupportRecycling obj)
            {
                FrameworkElement elem = obj.AttachedElement;
                if (elem != null)
                {
                    if (obj.OnElementReleasing(elem))
                    {
                        Type t = obj.RecyclingElementType;
                        List<FrameworkElement> usedElements = null;
                        List<FrameworkElement> availableElements = null;
                        bool removeFromPanel = obj.IsDirty;
						IRecyclableElementHost elementHost = this.Panel as IRecyclableElementHost;

						// if the element is going to be recycled, give the panel an option to 
						// force an element to be removed instead of recycled
						if (!removeFromPanel && elementHost != null)
							removeFromPanel = elementHost.ShouldRemove(obj, elem);

                        if (t != null)
                        {
                            if (null != _usedControlsSpecific && _usedControlsSpecific.TryGetValue(t, out usedElements))
                            {
                                // if we're going to reuse the element then make sure we have the available elements list
                                if (!removeFromPanel)
                                {
                                    Dictionary<Type, List<FrameworkElement>> availableControls = _availableControlsSpecific;

                                    if (null == availableControls)
                                        _availableControlsSpecific = availableControls = new Dictionary<Type, List<FrameworkElement>>();

                                    if (!availableControls.TryGetValue(t, out availableElements))
                                        availableControls[t] = availableElements = new List<FrameworkElement>();
                                }
                            }
                        }
                        else
                        {
                            string id = obj.RecyclingIdentifier;
                            Debug.Assert(null != id);

                            if (null != _usedIdentifierControlsSpecific && _usedIdentifierControlsSpecific.TryGetValue(id, out usedElements))
                            {
                                // if we're going to reuse the element then make sure we have the available elements list
                                if (!removeFromPanel)
                                {
                                    Dictionary<string, List<FrameworkElement>> availableControls = _availableIdentifierControlsSpecific;

                                    if (null == availableControls)
                                        _availableIdentifierControlsSpecific = availableControls = new Dictionary<string, List<FrameworkElement>>();

                                    if (!availableControls.TryGetValue(id, out availableElements))
                                        availableControls[id] = availableElements = new List<FrameworkElement>();
                                }
                            }
                        }

                        Debug.Assert(usedElements != null, "There were no used elements but we used to still do the clean up below. Is that right?");

                        if (usedElements != null)
                        {
                            int index = usedElements.IndexOf(elem);

                            if (index >= 0)
                            {
                                // since the usedElement indexes should not matter
                                // we'll move this item to the end and just remove
                                // the last entry to avoid moving any memory
                                int lastIndex = usedElements.Count - 1;

                                if (index < lastIndex)
                                    usedElements[index] = usedElements[lastIndex];

                                usedElements.RemoveAt(lastIndex);

                                if (!removeFromPanel)
                                {
                                    Debug.Assert(availableElements != null);
                                    availableElements.Add(elem);
                                }
                                else
                                {
                                    IRecyclableElement recycleElem = elem as IRecyclableElement;
                                    if (recycleElem != null)
                                        recycleElem.OwnerPanel = null;

                                    this.Panel.Children.Remove(elem);
                                }
                            }
                            else
                            {
                                Debug.Assert(false, "The specified element was not in the used elements list.");
                            }

                            _itemForContainer.Remove(elem);

                            OnElementReleased(obj, elem);

                            // added this if block - i don't think we meant to store the 
                            // elements that are no longer children of the panel
                            if (!removeFromPanel)
                            {
                                List<FrameworkElement> recentlyRecycled = _recentlyRecycled;

                                if (null == recentlyRecycled)
                                    _recentlyRecycled = recentlyRecycled = new List<FrameworkElement>();

                                recentlyRecycled.Add(elem);
                            }

							// let the panel know that an element was released.
							if (null != elementHost)
								elementHost.OnElementReleased(obj, elem, removeFromPanel);

                            return true;
                        }
                    }
                }
                return false;
            }

            #endregion // ReleaseElement

            #endregion //Methods
		}
        #endregion //PanelInfo class
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