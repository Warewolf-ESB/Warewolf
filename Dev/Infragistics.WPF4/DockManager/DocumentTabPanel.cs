//#define DEBUG_ADD_REMOVE

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;

namespace Infragistics.Windows.DockManager
{
	
	
	
	/// <summary>
	/// Custom panel for positioning elements similar to that of the mdi tabs within Visual Studio where only as many tabs that can be displayed in view will be positioned.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DocumentTabPanel : VirtualizingPanel
	{
		#region Member Variables

		private int _previousLastIndex = -1;

		// AS 2/1/10 TFS27032
		// We need to generate the element for the selected tab even if its out 
		// of view or else its children will not be in the automation tree.
		//
		private int _outOfViewItems = 0;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DocumentTabPanel"/>
		/// </summary>
		public DocumentTabPanel()
		{
			
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			Rect tabRect = new Rect(finalSize);
			UIElementCollection children = this.Children;
			Dock dock = this.TabStripPlacement;
			bool isHorizontal = dock == Dock.Top || dock == Dock.Bottom;
			// AS 6/24/08 BR34248
			//bool isLeft = dock == Dock.Left;

			// AS 6/24/08 BR34248
			//if (isLeft)
			//	tabRect.Y = tabRect.Height;

			for (int i = children.Count - 1; i >= 0; i--)
			{
				UIElement element = children[i];

				// AS 2/1/10 TFS27032
				// For items out of view (which should only be the selected item),
				// we'll force it to be clipped by using a width/height of 0.
				//
				bool isOutOfView = i < _outOfViewItems;

				if (isHorizontal)
				{
					// AS 2/1/10 TFS27032
					//tabRect.Width = element.DesiredSize.Width;
					tabRect.Width = isOutOfView ? 0 : element.DesiredSize.Width;
				}
				else // vertical
				{
					// AS 2/1/10 TFS27032
					//tabRect.Height = element.DesiredSize.Height;
					tabRect.Height = isOutOfView ? 0 : element.DesiredSize.Height;
				}

				// AS 6/24/08 BR34248
				//if (isLeft)
				//	tabRect.Y -= tabRect.Height;

				element.Arrange(tabRect);

				if (isHorizontal)
					tabRect.X += tabRect.Width;
				// AS 6/24/08 BR34248
				//else if (isLeft == false)
				else
					tabRect.Y += tabRect.Height;
			}

			// AS 5/8/12 TFS108266
			var handler = new EventHandler(OnLayoutUpdated);
			this.LayoutUpdated -= handler;
			this.LayoutUpdated += handler;

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override System.Windows.Size MeasureOverride(Size availableSize)
		{
			#region Setup

			bool isHorizontal = this.IsHorizontal;
			UIElementCollection children = this.Children;
			IItemContainerGenerator generator = this.ItemContainerGenerator;
			ItemsControl owner = ItemsControl.GetItemsOwner(this);
			int count = owner != null ? owner.Items.Count : 0;
			Size childMeasureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			int elementsAdded = 0;
			int lastIndex = 0;

			// AS 2/1/10 TFS27032
			// We need to make sure that we hydrate the container for the selected index. Otherwise 
			// its visual children (which are displayed in the selectedcontent area of the tab 
			// control) will not be available to automation clients.
			//
			int selectedIndex = owner is TabControl ? ((TabControl)owner).SelectedIndex : -1;

			// AS 5/20/08 BR32996
			// This panel (like the virtualizingstackpanel) needs to be in an items control.
			//
			if (null == generator || null == owner)
				return new Size();

			if (this._previousLastIndex < 0)
				this._previousLastIndex = count - 1;

			double desiredExtent = 0;
			double desiredHeight = 0;

			double availableExtent = isHorizontal ? availableSize.Width : availableSize.Height;
			double availableHeight = isHorizontal ? availableSize.Height : availableSize.Width;

			int elementIndex = children.Count;
			GeneratorPosition position = GetGeneratorPositionFromItemIndex(0);

			#endregion //Setup

			#region Generate/Measure Elements
			using (IDisposable disposable = generator.StartAt(position, GeneratorDirection.Forward, true))
			{
				for (int i = 0; i < count; i++)
				{
					bool isNewlyRealized;
					UIElement child = generator.GenerateNext(out isNewlyRealized) as UIElement;

					if (child == null)
						break;

					if (isNewlyRealized)
					{
						this.InsertInternalChild(elementIndex, child);

						generator.PrepareItemContainer(child);




					}
					else
						elementIndex--;

					child.Measure(childMeasureSize);
					Size childSize = child.DesiredSize;

					double childExtent = isHorizontal ? childSize.Width : childSize.Height;
					double childHeight = isHorizontal ? childSize.Height : childSize.Width;

					// if the element can't fit then we should remove it
					if (elementsAdded > 0 && childExtent + desiredExtent > availableExtent)
						break;

					lastIndex = i;
					elementsAdded++;
					desiredExtent += childExtent;
					desiredHeight = Math.Max(childHeight, desiredHeight);
				}
			}
			#endregion //Generate/Measure Elements

			// AS 2/1/10 TFS27032
			_outOfViewItems = 0;
			bool createSelected = selectedIndex >= 0 && selectedIndex > elementsAdded;

			#region Remove Items From the End
			for (int i = children.Count - 1; i >= elementsAdded; i--)
			{
				position = new GeneratorPosition(i, 0);

				// AS 2/1/10 TFS27032
				// Do not remove the selected tab item.
				//
				if (generator.IndexFromGeneratorPosition(position) == selectedIndex)
				{
					createSelected = false;
					_outOfViewItems++;
					continue;
				}

				if (position.Offset == 0)
				{



					generator.Remove(position, 1);

					// AS 2/1/10 TFS27032
					// We cannot assume its the first child since the selected item may have been
					// hydrated and be positioned before it.
					//
					//this.RemoveInternalChildRange(0, 1);
					this.RemoveInternalChildRange(_outOfViewItems, 1);
				}
			}
			#endregion //Remove Items From the End

			#region Create Selected TabItem
			// AS 2/1/10 TFS27032
			// If we didn't generate the tab for the selected tab item do it now.
			//
			if (createSelected)
			{
				position = GetGeneratorPositionFromItemIndex(selectedIndex);

				using (IDisposable disposable = generator.StartAt(position, GeneratorDirection.Forward, true))
				{
					bool isNewlyRealized;
					UIElement child = generator.GenerateNext(out isNewlyRealized) as UIElement;

					if (child != null)
					{
						if (isNewlyRealized)
						{
							this.InsertInternalChild(0, child);

							generator.PrepareItemContainer(child);
						}
					}
				}

				_outOfViewItems++;
			} 
			#endregion //Create Selected TabItem

			#region Calculations

			this._previousLastIndex = lastIndex;
			Size desiredSize;

			// if we scroll then return only as much as provided
			if (isHorizontal)
			{
				desiredSize = new Size(desiredExtent, desiredHeight);
			}
			else
			{
				desiredSize = new Size(desiredHeight, desiredExtent);
			}
			#endregion //Calculations

			#region HasItemsOutOfView

			if (null != owner)
				owner.SetValue(HasItemsOutOfViewPropertyKey, children.Count != count ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);

			#endregion //HasItemsOutOfView

			return desiredSize;
		}

		#endregion //MeasureOverride

		#region OnItemsChanged
		/// <summary>
		/// Invoked when the Items collection of the associated <see cref="ItemsControl"/> has been changed.
		/// </summary>
		/// <param name="sender">The object that raised the event</param>
		/// <param name="args">Provides information about the change</param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{




			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					break;
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Replace:
					// AS 2/1/10 TFS27032
					// These were handled the same as remove but really they should
					// have dealt with the old position.
					//
					if (args.OldPosition.Offset == 0)
						this.RemoveInternalChildRange((this.Children.Count - 1) - args.OldPosition.Index, 1);
					break;
				case NotifyCollectionChangedAction.Remove:
					Debug.Assert(args.ItemCount == 1);
					// only remove the item if it was hydrated
					if (args.Position.Offset == 0)
						this.RemoveInternalChildRange((this.Children.Count - 1) - args.Position.Index, 1);
					break;
				case NotifyCollectionChangedAction.Reset:
					// reset the stored pointers and clear the collection
					this.RemoveInternalChildRange(0, this.Children.Count);
					this._previousLastIndex = -1;
					break;
			}

			base.OnItemsChanged(sender, args);

			this.InvalidateMeasure();
		}
		#endregion //OnItemsChanged

		#endregion //Base class overrides

		#region Properties

		#region Private Properties

		#region IsHorizontal
		private bool IsHorizontal
		{
			get
			{
				switch (this.TabStripPlacement)
				{
					default:
					case Dock.Bottom:
					case Dock.Top:
						return true;
					case Dock.Left:
					case Dock.Right:
						return false;
				}
			}
		}
		#endregion //IsHorizontal

		#endregion //Private Properties

		#region Public Properties

		#region HasItemsOutOfView

		private static readonly DependencyPropertyKey HasItemsOutOfViewPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HasItemsOutOfView",
			typeof(bool), typeof(DocumentTabPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the HasItemsOutOfView" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetHasItemsOutOfView"/>
		public static readonly DependencyProperty HasItemsOutOfViewProperty =
			HasItemsOutOfViewPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'HasItemsOutOfView' attached readonly property
		/// </summary>
		/// <seealso cref="HasItemsOutOfViewProperty"/>
		public static bool GetHasItemsOutOfView(DependencyObject d)
		{
			return (bool)d.GetValue(DocumentTabPanel.HasItemsOutOfViewProperty);
		}

		#endregion //HasItemsOutOfView

		#region TabStripPlacement

		/// <summary>
		/// Identifies the <see cref="TabStripPlacement"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabStripPlacementProperty = TabControl.TabStripPlacementProperty.AddOwner(typeof(DocumentTabPanel), new FrameworkPropertyMetadata(KnownBoxes.DockTopBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Determines the orientation and placement of the tab items.
		/// </summary>
		/// <seealso cref="TabStripPlacementProperty"/>
		//[Description("Determines the orientation and placement of the tab items.")]
		//[Category("DockManager Properties")] // Layout
		[Bindable(true)]
		public Dock TabStripPlacement
		{
			get
			{
				return (Dock)this.GetValue(DocumentTabPanel.TabStripPlacementProperty);
			}
			set
			{
				this.SetValue(DocumentTabPanel.TabStripPlacementProperty, value);
			}
		}

		#endregion //TabStripPlacement

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region GetGeneratorPositionFromItemIndex
		private GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex)
		{
			Debug.Assert(this.IsItemsHost == true, "GetGeneratorPositionFromItemIndex called but IsItemsHost == false");
			if (this.IsItemsHost != true)
			{
				return new GeneratorPosition(-1, itemIndex + 1);
			}

			// Must access the internal children collection before using the generator (this is probably a bug in the framework).
			UIElementCollection children = base.InternalChildren;

			IItemContainerGenerator generator = this.ItemContainerGenerator;

			GeneratorPosition generatorPosition = (generator != null) ? generator.GeneratorPositionFromIndex(itemIndex) :
																				new GeneratorPosition(-1, itemIndex + 1);
			return generatorPosition;
		} 
		#endregion //GetGeneratorPositionFromItemIndex

		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			this.LayoutUpdated -= this.OnLayoutUpdated;

			var tabGroupPane = ItemsControl.GetItemsOwner(this) as TabGroupPane;

			// AS 5/8/12 TFS108266
			// Note I'm doing this async because we might get in here while a 
			// call to UpdateLayout is made while we are still changing the selected item.
			//
			if (null != tabGroupPane)
			{
				this.Dispatcher.BeginInvoke(
					System.Windows.Threading.DispatcherPriority.DataBind, 
					new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(tabGroupPane.EnsureSelectedDocumentIsInView));
			}
		}
		#endregion //OnLayoutUpdated

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