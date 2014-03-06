using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.Specialized;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Custom element for arranging the children of the <see cref="DocumentContentHost"/>
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DocumentContentHostPanel : FrameworkElement
		// AS 5/28/08 BR33298
		//, IPaneContainer
	{
		#region Member Variables

		private DocumentContentHost _host;
		private SplitPane _splitPane;
		// AS 5/28/08 BR33298
		//private IPaneContainer[] _containers = new IPaneContainer[0];

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DocumentContentHostPanel"/>
		/// </summary>
		public DocumentContentHostPanel()
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
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (null != this._splitPane)
				this._splitPane.Arrange(new Rect(finalSize));

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region GetVisualChild
		/// <summary>
		/// Returns the visual child at the specified index.
		/// </summary>
		/// <param name="index">Integer position of the child to return.</param>
		/// <returns>The child element at the specified position.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is greater than the <see cref="VisualChildrenCount"/></exception>
		protected override Visual GetVisualChild(int index)
		{
			if (this._splitPane != null)
			{
				if (index == 0)
					return this._splitPane;

				index--;
			}

			return base.GetVisualChild(index);
		}
		#endregion //GetVisualChild

		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				if (this._splitPane != null)
					return new SingleItemEnumerator(this._splitPane);

				return EmptyEnumerator.Instance;
			}
		}
		#endregion //LogicalChildren

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desiredSize = new Size();

			if (this._splitPane != null)
			{
				this._splitPane.Measure(availableSize);

				desiredSize.Width += this._splitPane.DesiredSize.Width;
				desiredSize.Height += this._splitPane.DesiredSize.Height;
			}

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region VisualChildrenCount
		/// <summary>
		/// Returns the number of visual children for the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int count = 0;

				if (this._splitPane != null)
					count++;

				return count;
			}
		}
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

		#region Internal Properties

		#region Host
		internal DocumentContentHost Host
		{
			get { return this._host; }
			set
			{
				if (value != this._host)
				{
					if (null != this._host)
						this._host.Panes.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnPanesChanged);

					this._host = value;

					this.Reinitialize();

					if (null != this._host)
						this._host.Panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnPanesChanged);
				}
			}
		}
		#endregion //Host

		#region SplitPane
		internal SplitPane SplitPane
		{
			get
			{
				if (this._splitPane == null)
				{
					this._splitPane = new TabbedDocumentSplitPane(this);
					Debug.Assert(this.Host != null);
					this._splitPane.SetBinding(SplitPane.SplitterOrientationProperty, Utilities.CreateBindingObject(DocumentContentHost.RootSplitterOrientationProperty, System.Windows.Data.BindingMode.OneWay, this.Host));
					// AS 5/28/08 BR33298
					//this._containers = new IPaneContainer[] { this._splitPane };

					this.AddVisualChild(this._splitPane);
					this.AddLogicalChild(this._splitPane);
				}

				return this._splitPane;
			}
		}
		#endregion //SplitPane 

		#endregion //Internal Properties 

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region OnPanesChanged
		private void OnPanesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.Reinitialize();
		}
		#endregion //OnPanesChanged

		#region Reinitialize
		private void Reinitialize()
		{
			if (this._host != null)
			{
				// copy the panes from the host
				FrameworkElement[] panes = new FrameworkElement[this._host.Panes.Count];
				SplitPane[] srcPanes = new SplitPane[panes.Length];
				this._host.Panes.CopyTo(srcPanes, 0);
				Array.Copy(srcPanes, panes, panes.Length);

				this.SplitPane.Panes.ReInitialize(panes);
			}
			else
				this.SplitPane.Panes.Clear();
		} 
		#endregion //Reinitialize

		#endregion //Private Methods

		#endregion //Methods

		#region IPaneContainer Members

		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

		#endregion //IPaneContainer Members

		#region TabbedDocumentSplitPane
		/// <summary>
		/// Custom split pane used to host the root <see cref="DocumentContentHost.Panes"/>
		/// </summary>
		private class TabbedDocumentSplitPane : SplitPane
			, IPaneContainer
		{
			#region Member Variables

			private DocumentContentHostPanel _owner;

			#endregion //Member Variables

			#region Constructor
			internal TabbedDocumentSplitPane(DocumentContentHostPanel owner)
			{
				this._owner = owner;
			}
			#endregion //Constructor

			#region Base class overrides

			// AS 5/27/08 BR33298
			#region IncludePanesAsLogicalChildren
			internal override bool IncludePanesAsLogicalChildren
			{
				get { return false; }
			}
			#endregion //IncludePanesAsLogicalChildren

			#endregion //Base class overrides

			#region IPaneContainer Members

			/// <summary>
			/// Returns the collection of items within the container
			/// </summary>
			IList IPaneContainer.Panes 
			{
				get 
				{
					Debug.Fail("The root DocumentContentHostPanel should not be used since the RootSplitPanes are now logical children of the DocumentContentHost.");

					return this.Panes; 
				}
			}

			bool IPaneContainer.CanBeRemoved
			{
				// this pane cannot be removed
				get 
				{
					Debug.Fail("The root DocumentContentHostPanel should not be used since the RootSplitPanes are now logical children of the DocumentContentHost.");

					return false; 
				}
			}

			bool IPaneContainer.RemovePane(object pane)
			{
				Debug.Fail("The root DocumentContentHostPanel should not be used since the RootSplitPanes are now logical children of the DocumentContentHost.");

				// when a pane is to be removed we need to remove it from
				// the ownign documentcontenthost
				DocumentContentHost host = this._owner.Host;
				Debug.Assert(null != host);

				SplitPane element = pane as SplitPane;
				Debug.Assert(null != element);

				int index = null != element ? host.Panes.IndexOf(element) : -1;

				if (index >= 0)
					host.Panes.RemoveAt(index);

				return index >= 0;
			}

			#endregion
		} 
		#endregion //TabbedDocumentSplitPane
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