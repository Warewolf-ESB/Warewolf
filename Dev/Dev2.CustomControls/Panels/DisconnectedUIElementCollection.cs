/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


/* Copyright (c) 2008, Dr. WPF
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   * Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 * 
 *   * Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 * 
 *   * The name Dr. WPF may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY Dr. WPF ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Dr. WPF BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Warewolf.Resource.Errors;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace WPF.JoshSmith.Panels
{
    internal class DisconnectedUIElementCollection : UIElementCollection, INotifyCollectionChanged
    {
        #region constructors

        /// <summary>
        ///     This collection can be used by a panel to maintain a collection of child elements
        ///     that are *not* connected to their owner as visual children or logical children.
        /// </summary>
        public DisconnectedUIElementCollection(UIElement owner)
            : this(owner, new SurrogateVisualParent())
        {
        }

        private DisconnectedUIElementCollection(UIElement owner, SurrogateVisualParent surrogateVisualParent)
            : base(surrogateVisualParent, null)
        {
            _ownerPanel = owner as Panel;
            surrogateVisualParent.InitializeOwner(this);
            _elements = new Collection<UIElement>();
        }

        #endregion

        #region UIElementCollection overrides

        public override int Capacity
        {
            get { return base.Capacity; }
            set
            {
                VerifyWriteAccess();
                base.Capacity = value;
            }
        }

        /// <summary>
        ///     Gets the number of elements in the collection.
        /// </summary>
        public override int Count
        {
            get { return _elements.Count; }
        }

        public override UIElement this[int index]
        {
            // ReSharper disable RedundantCast
            get { return _elements[index] as UIElement; }
            // ReSharper restore RedundantCast
            set
            {
                VerifyWriteAccess();
                base[index] = value;
            }
        }

        /// <summary>
        ///     Adds the element to the DisconnectedUIElementCollection
        /// </summary>
        public override int Add(UIElement element)
        {
            VerifyWriteAccess();
            return base.Add(element);
        }

        /// <summary>
        ///     Removes all elements from the DisconnectedUIElementCollection
        /// </summary>
        public override void Clear()
        {
            VerifyWriteAccess();
            base.Clear();
        }

        /// <summary>
        ///     Determines whether an element is in the DisconnectedUIElementCollection
        /// </summary>
        public override bool Contains(UIElement element)
        {
            return _elements.Contains(element);
        }

        /// <summary>
        ///     Copies the collection into the Array
        /// </summary>
        public override void CopyTo(Array array, int index)
        {
            ((ICollection) _elements).CopyTo(array, index);
        }

        /// <summary>
        ///     Strongly typed version of CopyTo
        /// </summary>
        public override void CopyTo(UIElement[] array, int index)
        {
            _elements.CopyTo(array, index);
        }

        /// <summary>
        ///     Returns an enumerator that can iterate through the collection.
        /// </summary>
        public override IEnumerator GetEnumerator()
        {
            return (_elements as ICollection).GetEnumerator();
        }

        /// <summary>
        ///     Returns the index of the element in the DisconnectedUIElementCollection
        /// </summary>
        public override int IndexOf(UIElement element)
        {
            return _elements.IndexOf(element);
        }

        /// <summary>
        ///     Inserts an element into the DisconnectedUIElementCollection at the specified index
        /// </summary>
        public override void Insert(int index, UIElement element)
        {
            VerifyWriteAccess();
            base.Insert(index, element);
        }

        /// <summary>
        ///     Removes the specified element from the DisconnectedUIElementCollection
        /// </summary>
        public override void Remove(UIElement element)
        {
            VerifyWriteAccess();
            base.Remove(_degenerateSiblings[element]);
        }

        /// <summary>
        ///     Removes the element at the specified index from the DisconnectedUIElementCollection
        /// </summary>
        public override void RemoveAt(int index)
        {
            VerifyWriteAccess();
            base.RemoveAt(index);
        }

        /// <summary>
        ///     Removes the specified number of elements starting at the specified index from the DisconnectedUIElementCollection
        /// </summary>
        public override void RemoveRange(int index, int count)
        {
            VerifyWriteAccess();
            base.RemoveRange(index, count);
        }

        #endregion

        #region public methods

        /// <summary>
        ///     The Initialize method is simply exposed as an accessible member that can
        ///     be called from the ConceptualPanel's Loaded event.  Accessing this member
        ///     via the Children property will implicitly cause CreateUIElementCollection
        ///     to be called to create the disconnected collection.  This method exists
        ///     because simple access of a property like Count might be optimized away by
        ///     an aggressive compiler.
        /// </summary>
        public void Initialize()
        {
            // do nothing
        }

        #endregion

        #region private methods

        private int BaseIndexOf(UIElement element)
        {
            return base.IndexOf(element);
        }

        private void BaseInsert(int index, UIElement element)
        {
            base.Insert(index, element);
        }

        private void BaseRemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        /// <summary>
        ///     If the owner is an items host, we need to enforce the rule that elements
        ///     cannot be explicitly added to the disconnected collection.  However, it is still
        ///     possible to modify the visual or logical "connected" children of a ConceptualPanel
        ///     while it is an items host by simply calling the AddVisualChild, RemoveVisualChild,
        ///     AddLogicalChild, or RemoveLogicalChild methods.  Logic within ConceptualPanel
        ///     ensures that any visual children added in this manner will be returned within
        ///     a GetVisualChild() enumeration.
        /// </summary>
        private void VerifyWriteAccess()
        {
            // if the owner is not a panel, just return
            if (_ownerPanel == null) return;

            // check whether the owner is an items host for an ItemsControl
            if (_ownerPanel.IsItemsHost && ItemsControl.GetItemsOwner(_ownerPanel) != null)
                throw new InvalidOperationException(ErrorResource.CannotImplicitlyAddDisconnectedChildren);
        }

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        ///     Since the owner of the collection is not the parent of the elements, it needs
        ///     a mechanism by which to monitor its collection of child visuals.
        ///     This class provides such notifications via INotifyCollectionChanged.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem, index));
        }

        #endregion

        #region private supporting classes

        #region DegenerateSibling

        private class DegenerateSibling : UIElement
        {
            private readonly UIElement _element;

            public DegenerateSibling(UIElement element)
            {
                _element = element;
            }

            public UIElement Element
            {
                get { return _element; }
            }

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        #endregion

        #region SurrogateVisualParent

        private class SurrogateVisualParent : UIElement
        {
            private bool _internalUpdate;
            private DisconnectedUIElementCollection _owner;

            internal void InitializeOwner(DisconnectedUIElementCollection owner)
            {
                _owner = owner;
            }

            protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
            {
                // avoid reentrancy during internal updates
                if (_internalUpdate) return;

                _internalUpdate = true;
                try
                {
                    // when a UIElement is added, replace it with its degenerate sibling
                    if (visualAdded != null)
                    {
                        Debug.Assert(!(visualAdded is DegenerateSibling),
                            "Unexpected addition of degenerate... All degenerates should be added during internal updates.");

                        var element = visualAdded as UIElement;
                        var sibling = new DegenerateSibling(element);
                        int index = _owner.BaseIndexOf(element);
                        _owner.BaseRemoveAt(index);
                        _owner.BaseInsert(index, sibling);
                        if (element != null)
                        {
                            _owner._degenerateSiblings[element] = sibling;
                            _owner._elements.Insert(index, element);
                            _owner.RaiseCollectionChanged(NotifyCollectionChangedAction.Add, element, index);
                        }
                    }

                    // when a degenerate sibling is removed, remove its corresponding element
                    if (visualRemoved != null)
                    {
                        Debug.Assert(visualRemoved is DegenerateSibling,
                            "Unexpected removal of UIElement... All non degenerates should be removed during internal updates.");

                        var sibling = (DegenerateSibling)visualRemoved;
                        int index = _owner._elements.IndexOf(sibling.Element);
                        _owner._elements.RemoveAt(index);
                        _owner.RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, sibling.Element, index);
                        _owner._degenerateSiblings.Remove(sibling.Element);
                    }
                }
                finally
                {
                    _internalUpdate = false;
                }
            }
        }

        #endregion

        #endregion

        #region fields

        private readonly Dictionary<UIElement, DegenerateSibling> _degenerateSiblings =
            new Dictionary<UIElement, DegenerateSibling>();

        private readonly Collection<UIElement> _elements;
        private readonly Panel _ownerPanel;

        #endregion
    }
}