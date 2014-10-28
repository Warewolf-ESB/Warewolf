/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF.JoshSmith.Panels
{
    /// <summary>
    ///     This panel maintains a collection of conceptual children that are neither logical
    ///     children nor visual children of the panel.  This allows those visuals to be connected
    ///     to other parts of the UI, if necessary, or even to remain disconnected.
    /// </summary>
    public abstract class ConceptualPanel : Panel
    {
        private readonly List<Visual> _visualChildren = new List<Visual>();

        /// <summary>
        ///     Initializes a new instance.
        /// </summary>
        protected ConceptualPanel()
        {
            Loaded += OnLoaded;
        }

        /// <summary>
        ///     Returns the number of visual children.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return _visualChildren.Count; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            var disconnectedUiElementCollection = Children as DisconnectedUIElementCollection;
            if (disconnectedUiElementCollection != null)
            {
                disconnectedUiElementCollection.Initialize();
            }
        }

        /// <summary>
        ///     Creates a disconnected UIElement collection.
        /// </summary>
        protected override sealed UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            var children = new DisconnectedUIElementCollection(this);
            children.CollectionChanged += OnChildrenCollectionChanged;
            return children;
        }

        /// <summary>
        ///     Subclasses override this to perform an action when a conceptual child is added to the panel.
        /// </summary>
        protected virtual void OnChildAdded(UIElement child)
        {
        }

        /// <summary>
        ///     Subclasses override this to perform an action when a conceptual child is removed from the panel.
        /// </summary>
        protected virtual void OnChildRemoved(UIElement child)
        {
        }

        /// <summary>
        ///     For simplicity, this class will listen to change notifications on the DisconnectedUIElementCollection
        ///     and provide them to descendants through the OnChildAdded and OnChildRemoved members.
        /// </summary>
        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnChildAdded(e.NewItems[0] as UIElement);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnChildRemoved(e.OldItems[0] as UIElement);
                    break;
            }
        }

        /// <summary>
        ///     Gets the visual child at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualChildren.Count)
                throw new ArgumentOutOfRangeException();
            return _visualChildren[index];
        }

        /// <summary>
        ///     Invoked when the panel's children collection is modified.
        /// </summary>
        /// <param name="visualAdded"></param>
        /// <param name="visualRemoved"></param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is Visual)
            {
                _visualChildren.Add(visualAdded as Visual);
            }

            if (visualRemoved is Visual)
            {
                _visualChildren.Remove(visualRemoved as Visual);
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }
}