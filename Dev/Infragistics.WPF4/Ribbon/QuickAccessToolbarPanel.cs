using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.Windows.Media;
using Infragistics.Shared;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Arranges tools for the <see cref="QuickAccessToolbar"/>.
	/// </summary>
	//[ToolboxItem(false)]	// JM BR28203 11-06-07 - added this here for documentation but commented out and added ToolboxBrowsableAttribute directly to DesignMetadata for the XamRibbon assembly.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class QuickAccessToolbarPanel : Panel
	{
		#region Member Variables

		private QuickAccessToolbar								_qat;

		private bool											_generateChildrenCalled = false;
		private List<UIElement>									_generatedChildren;
		private ItemContainerGenerator							_itemContainerGenerator;
		private UIElementCollection								_privateChildren;
		private Size											_lastMeasureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

		#endregion //Member Variables	

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="QuickAccessToolbarOverflowPanel"/> class.
		/// </summary>
		public QuickAccessToolbarPanel()
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Arranges and sizes the tools contained in the QuickAccessToolbarOverflowPanel.
		/// </summary>
		/// <param name="finalSize">The size available to arrange the contained tools.</param>
		/// <returns>The size used to arrange the contained tools.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// JM BR28203 11-06-07 
			if (this._privateChildren == null)
				return finalSize;

			UIElementCollection children				= this._privateChildren;
			int					childrenCount			= children.Count;
			double				remainingWidth			= Math.Round(finalSize.Width);
			double				defaultQatToolHeight	= XamRibbon.DEFAULT_QAT_TOOL_SIZE.Height;
			Rect				arrangeRect				= new Rect(0, 0, defaultQatToolHeight, XamRibbon.DEFAULT_QAT_TOOL_SIZE.Width);

			for (int i = 0; i < childrenCount; i++)
			{
				UIElement	child				= children[i];
				double		roundedDesiredWidth	= Math.Round(child.DesiredSize.Width);

				remainingWidth					-= roundedDesiredWidth;

				arrangeRect.Width				= roundedDesiredWidth;
				child.Arrange(arrangeRect);
				arrangeRect.X					+= roundedDesiredWidth;

				if (remainingWidth <= 0)
					break;
			}

			return finalSize;
		}

			#endregion //ArrangeOverride	

			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this.VisualChildrenCount)
				throw new ArgumentOutOfRangeException("index");

			if (this._privateChildren == null)
				return base.GetVisualChild(index);

			return this._privateChildren[index];
		}

			#endregion //GetVisualChild	
     
			#region MeasureOverride

		/// <summary>
		/// Measures the QuickAccessToolbarOverflowPanel and all its contained tools.
		/// </summary>
		/// <param name="availableSize">The size available to the QuickAccessToolbarOverflowPanel.</param>
		/// <returns>The size desired by the QuickAccessToolbarOverflowPanel.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this._lastMeasureSize = availableSize;
			return this.DoMeasureProcessing(availableSize);
		}

			#endregion //MeasureOverride	

            #region OnIsItemsHostChanged
        /// <summary>
        /// Invoked when the panel becomes or is no longer the items host of an ItemsControl.
        /// </summary>
        /// <param name="oldIsItemsHost">Old state</param>
        /// <param name="newIsItemsHost">New state</param>
        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            // AS 11/5/08 TFS9049
            // Instead of waiting until the next qat applytemplate, we should remove these
            // as soon as this panel is no longer the itemshost for the qat. Also, we weren't
            // unhooking from the ItemsChanged.
            //
            if (newIsItemsHost == false)
            {
                if (null != this._itemContainerGenerator)
                    this._itemContainerGenerator.ItemsChanged -= new ItemsChangedEventHandler(this.OnItemsChanged);

                QuickAccessToolbarOverflowPanel overflow = this.QAT != null ? this.QAT.ToolbarOverflowPanel : null;
                if (null != overflow)
                    overflow.Children.Clear();

                this.RemoveVisualChildren();
            }

            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
        } 
            #endregion //OnIsItemsHostChanged

			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				if (this._privateChildren == null)
					return base.VisualChildrenCount;

				return this._privateChildren.Count;
			}
		}

			#endregion //VisualChildrenCount	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Internal Properties

				#region GeneratedChildren

		internal List<UIElement> GeneratedChildren
		{
			get
			{
				if (this._generatedChildren == null)
					this._generatedChildren = new List<UIElement>();

				return this._generatedChildren;
			}
		}

				#endregion //GeneratedChildren	
    
				#region QAT

		internal QuickAccessToolbar QAT
		{
			get { return this._qat; }
			set { this._qat = value; }
		}

				#endregion //QAT
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region GenerateChildren

		private void GenerateChildren()
		{
			if (this._generateChildrenCalled)
				return;

			if (base.IsItemsHost && (this.QAT != null))
			{
				if (this._itemContainerGenerator == null)
				{
					this._itemContainerGenerator = ((IItemContainerGenerator)this.QAT.ItemContainerGenerator).GetItemContainerGeneratorForPanel(this);
					this._itemContainerGenerator.ItemsChanged += new ItemsChangedEventHandler(this.OnItemsChanged);
				}

				IItemContainerGenerator generator = this._itemContainerGenerator;
				generator.RemoveAll();
				if (this._privateChildren == null)
					this._privateChildren = this.CreateUIElementCollection(null);
				else
					this._privateChildren.Clear();

																									
				this.GeneratedChildren.Clear();
				QuickAccessToolbarOverflowPanel qatOverflowPanel = this.QAT.ToolbarOverflowPanel;
				if (qatOverflowPanel != null)
					qatOverflowPanel.Children.Clear();


				using (IDisposable disposable = generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward))
				{
					UIElement generatedElement;
					while ((generatedElement = generator.GenerateNext() as UIElement) != null)
					{
						this.GeneratedChildren.Add(generatedElement);
						this._privateChildren.Add(generatedElement);
						generator.PrepareItemContainer(generatedElement);
					}
				}

				this._generateChildrenCalled = true;
			}
			else
				this._privateChildren = base.InternalChildren;
		}

				#endregion //GenerateChildren	
    
				#region OnItemsChanged

		private void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Reset:
					this._generateChildrenCalled = false;
					this.GenerateChildren();

					break;
			}

			base.InvalidateMeasure();
		}

				#endregion //OnItemsChanged	

			#endregion //Private Methods

			#region Internal Methods

				#region DoMeasureProcessing

		internal Size DoMeasureProcessing()
		{
			return this.DoMeasureProcessing(this._lastMeasureSize);
		}

		internal Size DoMeasureProcessing(Size availableSize)
		{
			// JM BR28203 11-06-07 
			if (this.QAT == null)
				return availableSize;

			this.GenerateChildren();

			if (this.QAT.ToolbarOverflowPanel == null)
				throw new InvalidOperationException(XamRibbon.GetString("LE_QatOverflowPanelMissing"));

			this.QAT.ToolbarOverflowPanel.Children.Clear();

			List<UIElement>		generatedChildren		= this.GeneratedChildren;
			UIElementCollection privateChildren			= this._privateChildren;
			UIElementCollection overflowChildren		= this.QAT.ToolbarOverflowPanel.Children;
			int					generatedChildrenCount	= generatedChildren.Count;
			double				availableWidth			= Math.Round(availableSize.Width);
			double				remainingWidth			= availableWidth;
			double				widthUsed				= 0;
			double				defaultQatToolHeight	= XamRibbon.DEFAULT_QAT_TOOL_SIZE.Height;
			QuickAccessToolbarOverflowPanel	
								overflowPanel			= this.QAT.ToolbarOverflowPanel;

			for (int i = 0; i < generatedChildrenCount; i++)
			{ 
				UIElement child = generatedChildren[i];
				child.Measure(new Size(double.PositiveInfinity, defaultQatToolHeight));

				double childDesiredWidth = Math.Round(child.DesiredSize.Width);

				// If the child requires more width than we have available, the child needs to be in the overflow panel.
				// If it is not already there, remove it from this panel and add it to the overflow panel.
				if (childDesiredWidth >= remainingWidth)
				{
					if (overflowChildren.Contains(child) == false)
					{
						privateChildren.Remove(child);

						// If the QuickCustomizeMenu tool is already on the overflow panel, insert the child before the QuickCustomizeMenuTool.
						// Otherwise just add it at the end.
						if (overflowChildren.Contains(this.QAT.QuickCustomizeMenu))
							overflowChildren.Insert(overflowChildren.IndexOf(this.QAT.QuickCustomizeMenu), child);
						else
							overflowChildren.Add(child);
					}

					remainingWidth = 0;
				}
				else
				{
					// If the child is not on this panel (i.e. it was previously 'bumped' to the overflow panel), add it.
					if (privateChildren.Contains(child) == false)
						privateChildren.Add(child);

					remainingWidth	-= childDesiredWidth;
					widthUsed		+= childDesiredWidth;
				}
			}


			// If we ended up with any tools on the overflow panel, make the overflow button element visible and make sure the
			// QUickCustomizeMenu tool is on the overflow panel.
			if (overflowChildren.Count > 0)
			{
				this.QAT.SetValue(QuickAccessToolbar.OverflowButtonVisibilityPropertyKey, Visibility.Visible);

				if (this.QAT.QuickCustomizeMenuSite.Content == this.QAT.QuickCustomizeMenu)
					this.QAT.QuickCustomizeMenuSite.Content = null;

				if (overflowChildren.Contains(this.QAT.QuickCustomizeMenu) == false)
					overflowChildren.Add(this.QAT.QuickCustomizeMenu);
			}
			else
			{
				this.QAT.SetValue(QuickAccessToolbar.OverflowButtonVisibilityPropertyKey, Visibility.Collapsed);

				if (overflowChildren.Contains(this.QAT.QuickCustomizeMenu))
					overflowChildren.Remove(this.QAT.QuickCustomizeMenu);

				if (this.QAT.QuickCustomizeMenuSite.Content != this.QAT.QuickCustomizeMenu)
					this.QAT.QuickCustomizeMenuSite.Content = this.QAT.QuickCustomizeMenu;
			}

			Size requiredSize = new Size(widthUsed, defaultQatToolHeight);

			return requiredSize;
		}

				#endregion //DoMeasureProcessing

				// JM 05-09-08
				#region RemoveVisualChildHelper

        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //RemoveVisualChildHelper

				// JM 05-09-08
				#region RemoveVisualChildren

		internal void RemoveVisualChildren()
		{
			UIElementCollection privateChildren	= this._privateChildren;

			// AS 5/3/10 TFS31821
			// Added null check. At design time, the IsItemsHost property went to 
			// false before the collection was set.
			//
			if (null != _privateChildren)
			{
				int privateChildrenCount = privateChildren.Count;

				for (int i = 0; i < privateChildrenCount; i++)
					this.RemoveVisualChild(privateChildren[i]);
			}
		}

				#endregion //RemoveVisualChildren

			#endregion //Internal Methods

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