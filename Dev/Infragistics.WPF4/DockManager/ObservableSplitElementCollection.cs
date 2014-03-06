using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	#region ObservableSplitElementCollection<T>
	/// <summary>
	/// Observable collection for managing a collection of elements that has splitters before/after elements.
	/// </summary>
	/// <typeparam name="T">Type of child element allowed.</typeparam>
	internal class ObservableSplitElementCollection<T> :
		ObservableCollectionExtended<T> where T : FrameworkElement
	{
		#region Member Variables

		private PaneSplitterMode _splitterMode;
		private ISplitElementCollectionOwner _owner;
		private IList<T> _elements;
		private IList<PaneSplitter> _splitters;

		#endregion //Member Variables

		#region Constructor
		internal ObservableSplitElementCollection(ISplitElementCollectionOwner owner, PaneSplitterMode splitterMode)
		{
			this._owner = owner;
			this._splitterMode = splitterMode;
			this._elements = new List<T>();
			this._splitters = new List<PaneSplitter>();
		}
		#endregion //Constructor

		#region Properties

		#region SplitterMode
		public PaneSplitterMode SplitterMode
		{
			get { return this._splitterMode; }
		}
		#endregion //SplitterMode

		#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children including the splitters
		/// </summary>
		public int VisualChildrenCount
		{
			get 
			{
				return this._elements.Count + this._splitters.Count;
			}
		} 
		#endregion //VisualChildrenCount

		#endregion //Properties

		#region Methods

		#region GetSplitterEnumerator
		internal IEnumerator<PaneSplitter> GetSplitterEnumerator()
		{
			return this._splitters.GetEnumerator();
		} 
		#endregion //GetSplitterEnumerator

		#region GetVisualChild
		/// <summary>
		/// Returns the visual (element or panesplitter) at the specified index
		/// </summary>
		public Visual GetVisualChild(int index)
		{
			if (index < this._elements.Count)
				return this._elements[index];

			index -= this._elements.Count;

			return this._splitters[index];
		}
		#endregion //GetVisualChild

		#region RefreshSplitterVisibility
		/// <summary>
		/// Updates the Visibility of the splitters
		/// </summary>
		internal void RefreshSplitterVisibility()
		{
			if (this._splitterMode == PaneSplitterMode.BetweenPanes)
			{
				T next = null;

				for (int i = this.Count - 1; i >= 0; i--)
				{
					T previous = this[i];
					PaneSplitter splitter = PaneSplitter.GetSplitter(previous);

					// if the element is collapsed (or is the last visible item)
					// then hide its splitter if it has one
					if (previous.Visibility == Visibility.Collapsed)
					{
						if (null != splitter)
							splitter.Visibility = Visibility.Collapsed;
					}
					else if (next == null)
					{
						next = previous;

						if (null != splitter)
							splitter.Visibility = Visibility.Collapsed;
					}
					else
					{
						Debug.Assert(null != splitter);

						if (null != splitter)
							splitter.Visibility = Visibility.Visible;
					}
				}
			}
		} 
		#endregion // RefreshSplitterVisibility

		#endregion //Methods

		#region Base class overrides

		#region NotifyItemsChanged
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}
		#endregion //NotifyItemsChanged

		#region OnCollectionChanged
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			// rebuild the element and splitter list
			#region Cache Old Element/Splitters

			Dictionary<T, PaneSplitter> oldElements = new Dictionary<T, PaneSplitter>();

			// build a list of the previous elements
			for (int i = 0, count = this._elements.Count; i < count; i++)
			{
				PaneSplitter splitter = PaneSplitter.GetSplitter(this._elements[i]);
				Debug.Assert(null == splitter || this._splitters.Contains(splitter));
				oldElements.Add(this._elements[i], splitter);
			}

			#endregion //Cache Old Element/Splitters

			#region Build the Element/Splitter List
			bool splitterPerPane = this._splitterMode == PaneSplitterMode.SinglePane;

			// reuse what we can
			for (int i = 0, last = this.Count - 1; i <= last; i++)
			{
				T element = this[i];
				PaneSplitter splitter;

				Debug.Assert(element is PaneSplitter == false);

				// if this element existed before...
				if (oldElements.TryGetValue(element, out splitter))
				{
					// remove the reference so we know not to clean it up
					oldElements.Remove(element);

					// move the element to the right position
					if (this._elements[i] != element)
					{
						this._elements.Remove(element);
						this._elements.Insert(i, element);
					}
				}
				else
				{
					this._elements.Insert(i, element);

					this._owner.OnElementAdded(element);
				}

				if (splitterPerPane || i < last)
				{
					// if we need a splitter and don't have one...
					if (null == splitter)
					{
						// create a new splitter and associate it with this element
						splitter = this._owner.CreateSplitter();
						splitter.ElementBeforeSplitter = element;
						element.SetValue(PaneSplitter.SplitterPropertyKey, splitter);

						// make sure its in the collection before letting the owner know
						this._splitters.Insert(i, splitter);

						this._owner.OnSplitterAdded(splitter);
					}
					else
					{
						Debug.Assert(VisualTreeHelper.GetParent(splitter) == this._owner);

						// make sure its in the right spot
						// move the element to the right position
						if (this._splitters[i] != splitter)
						{
							this._splitters.Remove(splitter);
							this._splitters.Insert(i, splitter);
						}
					}
				}
				else if (splitter != null)
				{
					this._splitters.Remove(splitter);

					// notify the owner about the removal before we remove it from the list
					this._owner.OnSplitterRemoved(splitter);
					element.ClearValue(PaneSplitter.SplitterPropertyKey);
					splitter.ElementBeforeSplitter = null;
				}
			}
			#endregion //Build the Element/Splitter List

			#region Remove Unused Elements/Splitters
			// remove any elements and splitters for those elements
			// if elements were removed
			if (oldElements.Count > 0)
			{
				foreach (KeyValuePair<T, PaneSplitter> pair in oldElements)
				{
					this._elements.Remove(pair.Key);

					this._owner.OnElementRemoved(pair.Key);
					pair.Key.ClearValue(PaneSplitter.SplitterPropertyKey);

					PaneSplitter splitter = pair.Value;

					if (splitter != null)
					{
						this._splitters.Remove(splitter);

						this._owner.OnSplitterRemoved(splitter);
						splitter.ElementBeforeSplitter = null;
					}
				}
			}
			#endregion //Remove Unused Elements/Splitters

			this.RefreshSplitterVisibility();

			// now raise the event
			base.OnCollectionChanged(e);
		}
		#endregion //OnCollectionChanged

		#region OnItemAdding
		protected override void OnItemAdding(T itemAdded)
		{
			this._owner.OnElementAdding(itemAdded);

			base.OnItemAdding(itemAdded);
		}
		#endregion //OnItemAdding

		#endregion //Base class overrides
	} 
	#endregion //ObservableSplitElementCollection<T>

	#region ISplitElementCollectionOwner
	/// <summary>
	/// Interface implemented by a class using a <see cref="ObservableSplitElementCollection&lt;T&gt;"/>
	/// </summary>
	internal interface ISplitElementCollectionOwner
	{
		void OnElementAdding(FrameworkElement newElement);
		void OnElementAdded(FrameworkElement newElement);
		void OnElementRemoved(FrameworkElement oldElement);

		/// <summary>
		/// Creates a pane splitter
		/// </summary>
		PaneSplitter CreateSplitter();

		/// <summary>
		/// Invoked when a splitter has been added.
		/// </summary>
		/// <param name="splitter">Splitter that was added</param>
		void OnSplitterAdded(PaneSplitter splitter);

		/// <summary>
		/// Invoked when a splitter has been removed.
		/// </summary>
		/// <param name="splitter">Splitter that was removed</param>
		void OnSplitterRemoved(PaneSplitter splitter);
	} 
	#endregion //ISplitElementCollectionOwner
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