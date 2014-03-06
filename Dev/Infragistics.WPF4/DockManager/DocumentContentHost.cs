using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Windows.Markup;
using System.Collections;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Control used to display one or more <see cref="ContentPane"/> 
	/// instances as tabbed documents within a <see cref="XamDockManager"/>
	/// </summary>
	[TemplatePart(Name="PART_Panel", Type=typeof(DocumentContentHostPanel))]
	[ContentProperty("Panes")]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class DocumentContentHost : Control
		, IAddChild
		, IPaneContainer
	{
		// FUTURE do we want to create a command for showing a pane such that when it was in the document area, we would remove it from its current container and add it to the container of the currently active document?

		#region Member Variables

		private DocumentContentHostPanel _panel;
		private QueuedObservableCollection<SplitPane> _panes;
		// AS 5/28/08 BR33298
		// Not needed anymore.
		//
		//private IPaneContainer[] _containers = new IPaneContainer[0];

		// AS 5/27/08 BR33298
		// The split panes should be logical children of the host now so we need to maintain
		// a separate collection so we can handle adding/removing them from the logical tree.
		//
		private IList<SplitPane> _logicalChildren = new List<SplitPane>();

		// AS 4/28/11 TFS73532
		private bool _hasAppliedTemplate;

		#endregion //Member Variables

		#region Constructor
		static DocumentContentHost()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentContentHost), new FrameworkPropertyMetadata(typeof(DocumentContentHost)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(DocumentContentHost), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(DocumentContentHost), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(DocumentContentHost), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));
		}

		/// <summary>
		/// Initializes a new <see cref="DocumentContentHost"/>
		/// </summary>
		public DocumentContentHost()
		{
			this._panes = new QueuedObservableCollection<SplitPane>();

			// AS 5/27/08 BR33298
			this._panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnPanesCollectionChanged);

			// AS 4/28/11 TFS73532
			this._panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnPanesCollectionChangedAfterHost);
		}

		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
            // AS 10/15/08 TFS8068
            // Maintain the DataContext during the move.
            //
            using (DockManagerUtilities.CreateMoveReplacement(this._logicalChildren))
                this.OnApplyTemplateImpl();

			// AS 4/28/11 TFS73532
			_hasAppliedTemplate = true;
        }

        private void OnApplyTemplateImpl()
        {
			SplitPane[] children = null;

			// AS 4/28/11 TFS73532
			// We don't have to do this the first time the template is applied.
			//
			if (_hasAppliedTemplate)
			{
				// AS 5/27/08 BR33298
				// As we had to do with the XamDockManager, we need to remove and readd
				// the logical children so that the attached properties will propogate
				// down the chain.
				//
				children = new SplitPane[this._logicalChildren.Count];
				this._logicalChildren.CopyTo(children, 0);

				for (int i = children.Length - 1; i >= 0; i--)
				{
					this._logicalChildren.RemoveAt(i);
					this.RemoveLogicalChild(children[i]);
				}
			}

			// release the old panel
			if (null != this._panel)
				this._panel.Host = null;

			base.OnApplyTemplate();

			this._panel = this.GetTemplateChild("PART_Panel") as DocumentContentHostPanel;

			// initialize the new panel
			if (this._panel != null)
			{
				this._panel.Host = this;
				// AS 5/28/08 BR33298
				//this._containers = new IPaneContainer[] { this._panel };

				// AS 4/28/11 TFS73532
				// When adding children we want them to be made a logical child first and then a visual child 
				// but when removing we want the visual child removed first and then removed from the logical 
				// tree to avoid multiple traversals of the tree so we'll use a separate handler to remove the 
				// logical children.
				//
				this._panes.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnPanesCollectionChangedAfterHost);
				this._panes.CollectionChanged += new NotifyCollectionChangedEventHandler(OnPanesCollectionChangedAfterHost);
			}
			// AS 5/28/08 BR33298
			//else
			//	this._containers = new IPaneContainer[0];

			// AS 4/28/11 TFS73532
			if (children != null)
			{
				// AS 5/27/08 BR33298
				// As we had to do with the XamDockManager, we need to remove and readd
				// the logical children so that the attached properties will propogate
				// down the chain.
				//
				for (int i = 0; i < children.Length; i++)
				{
					SplitPane split = children[i];
					this._logicalChildren.Add(split);
					this.AddLogicalChild(split);
				}
			}
		}
		#endregion //OnApplyTemplate

		#region HitTestCore

		/// <summary>
		/// Invoked to determine if the specified point is within the element's bounds.
		/// </summary>
		/// <param name="hitTestParameters">Provides information about the hit test location</param>
		/// <returns>True if the point is within the bounds of the element.</returns>
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			Rect rect = new Rect(new Point(), this.RenderSize);
			if (rect.Contains(hitTestParameters.HitPoint))
				return new PointHitTestResult(this, hitTestParameters.HitPoint);

			return base.HitTestCore(hitTestParameters);
		}

		#endregion // HitTestCore

		// AS 5/27/08 BR33298
		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				if (null != base.LogicalChildren)
					return new MultiSourceEnumerator(this._logicalChildren.GetEnumerator(), base.LogicalChildren);
				else
					return this._logicalChildren.GetEnumerator();
			}
		}
		#endregion //LogicalChildren

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ActiveDocument

		/// <summary>
		/// Identifies the <see cref="ActiveDocument"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActiveDocumentProperty = ActivePaneManager.ActiveDocumentProperty.AddOwner(typeof(DocumentContentHost));

		/// <summary>
		/// Returns the currently active ContentPane within the DocumentContentHost
		/// </summary>
		/// <seealso cref="ActiveDocumentProperty"/>
		//[Description("Returns the currently active ContentPane within the DocumentContentHost")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public ContentPane ActiveDocument
		{
			get
			{
				return (ContentPane)this.GetValue(DocumentContentHost.ActiveDocumentProperty);
			}
		}

		#endregion //ActiveDocument

		#region Panes
		/// <summary>
		/// Returns a collection of the root level panes that are displayed with the <see cref="DocumentContentHost"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Panes collection represents the root <see cref="SplitPane"/> instances that contain 
		/// the <see cref="ContentPane"/> instances that represent the documents in the application.</p>
		/// </remarks>
		//[Description("The collection of split panes displayed within the document host. The split panes can only contain SplitPane or TabGroupPane instances.")]
		//[Category("DockManager Properties")] // Data
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ObservableCollectionExtended<SplitPane> Panes
		{
			get { return this._panes; }
		}
		#endregion //Panes

		#endregion //Public Properties 

		#region Internal Properties

		#region Panel
		internal DocumentContentHostPanel Panel
		{
			get { return this._panel; }
		} 
		#endregion //Panel

		#region RootSplitterOrientation

		/// <summary>
		/// Identifies the <see cref="RootSplitterOrientation"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty RootSplitterOrientationProperty = DependencyProperty.Register("RootSplitterOrientation",
			typeof(Orientation), typeof(DocumentContentHost), new FrameworkPropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Returns/sets the orientation of the root split panes.
		/// </summary>
		/// <seealso cref="RootSplitterOrientationProperty"/>
		internal Orientation RootSplitterOrientation
		{
			get
			{
				return (Orientation)this.GetValue(DocumentContentHost.RootSplitterOrientationProperty);
			}
			set
			{
				this.SetValue(DocumentContentHost.RootSplitterOrientationProperty, value);
			}
		}

		#endregion //RootSplitterOrientation

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region GetContainerForPane
		/// <summary>
		/// Helper method to find/create the container to which the specified pane should be added when being moved into the document area.
		/// </summary>
		/// <param name="pane">The pane that will be moved.</param>
		/// <param name="createIfNull">True if a group should be created and returned if there isn't one available</param>
		/// <returns>The container to which the pane should be added.</returns>
		internal IContentPaneContainer GetContainerForPane(ContentPane pane, bool createIfNull)
		{
			Debug.Assert(pane.IsDocument == false);

			if (pane.IsDocument)
				return pane.PlacementInfo.CurrentContainer;

			ContentPane activeDocument = this.ActiveDocument;
			IContentPaneContainer newContainer = null;

			if (null != activeDocument)
				newContainer = activeDocument.PlacementInfo.CurrentContainer;
			// AS 10/15/10 TFS57333
			// The activedocument may have been removed but the active document not yet 
			// updated. So if it doesn't have a container then evaluate the groups we have.
			//
			//else
			if (newContainer == null)
			{
				newContainer = this.GetFirstLeafTabGroup();

				// if there is no leaf tab group then create one
				if (null == newContainer && createIfNull)
				{
					Debug.Assert(this.Panes.Count == 0);

					SplitPane newSplit = DockManagerUtilities.CreateSplitPane(XamDockManager.GetDockManager(this));
					TabGroupPane newGroup = DockManagerUtilities.CreateTabGroup(XamDockManager.GetDockManager(this));
					newSplit.Panes.Add(newGroup);
					this.Panes.Add(newSplit);
					newContainer = newGroup;
				}
			}

			return newContainer;
		}
		#endregion //GetContainerForPane

		#endregion //Internal Methods

		#region Private Methods

		#region GetFirstLeafTabGroup
		/// <summary>
		/// Helper method that should be used when there is no active document (and therefore no visible tab group).
		/// </summary>
		/// <returns>The tab group to which a pane can be added</returns>
		private IContentPaneContainer GetFirstLeafTabGroup()
		{
			//Debug.Assert(this.ActiveDocument == null);

			TabGroupPane group = null;

			foreach (SplitPane pane in this.Panes)
			{
				group = DockManagerUtilities.GetTabGroupPane(pane);

				if (null != group)
					break;
			}

			return group;
		}
		#endregion //GetFirstLeafTabGroup

		// AS 5/27/08 BR33298
		#region OnPanesCollectionChanged
		private void OnPanesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs args = e as QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs;
			Debug.Assert(null != args);

			foreach (SplitPane added in args.ItemsAdded)
			{
				this._logicalChildren.Add(added);
				this.AddLogicalChild(added);
			}

			// AS 4/28/11 TFS73532
			// Moved to the OnPanesCollectionChangedAfterHost
			var dm = XamDockManager.GetDockManager( this );

			// AS 7/17/12 TFS115251
			// If we're loading a layout then we could end up reusing this pane and because of a
			// bug in the WPF framework, the IsLoaded state could be corrupted if the pane is 
			// still in the logical tree when it is removed from visual tree if it is then 
			// put back into the visual tree within an unloaded pane. To avoid this we'll 
			// remove the pane from the logical tree before removing it from the visual tree 
			// but only while loading a layout.
			//
			if ( dm != null && dm.IsLoadingLayout )
			{
				foreach ( SplitPane removed in args.ItemsRemoved )
				{
					this._logicalChildren.Remove( removed );
					this.RemoveLogicalChild( removed );
				}
			}
		}

		// AS 4/28/11 TFS73532
		// When a pane is removed we want it to be removed from the visual tree first and then 
		// from the logical tree so we need to let the host process the remove before we remove 
		// the items from the logical tree.
		//
		private void OnPanesCollectionChangedAfterHost(object sender, NotifyCollectionChangedEventArgs e)
		{
			QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs args = e as QueuedObservableCollection<SplitPane>.QueuedNotifyCollectionChangedEventArgs;
			Debug.Assert(null != args);

			// AS 7/17/12 TFS115251
			// Adjusted the previous logic to only call RemoveLogicalChild if it was 
			// still in the _logicalChildren collection since they might have been removed 
			// above in the OnPanesCollectionChanged.
			//
			foreach (SplitPane removed in args.ItemsRemoved)
			{
				if (this._logicalChildren.Remove(removed))
					this.RemoveLogicalChild(removed);
			}
		}
		#endregion //OnPanesCollectionChanged

		#endregion //Private Methods 

		#endregion //Methods

		#region Events

		#region ActiveDocumentChanged

		/// <summary>
		/// Event ID for the <see cref="ActiveDocumentChanged"/> routed event
		/// </summary>
		/// <seealso cref="ActiveDocument"/>
		/// <seealso cref="ActiveDocumentChanged"/>
		/// <seealso cref="OnActiveDocumentChanged"/>
		public static readonly RoutedEvent ActiveDocumentChangedEvent =
			EventManager.RegisterRoutedEvent("ActiveDocumentChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ContentPane>), typeof(DocumentContentHost));

		/// <summary>
		/// Occurs when the <see cref="ActiveDocument"/> property has been changed
		/// </summary>
		/// <seealso cref="ActiveDocumentChanged"/>
		/// <seealso cref="ActiveDocumentChangedEvent"/>
		protected virtual void OnActiveDocumentChanged(RoutedPropertyChangedEventArgs<ContentPane> args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseActiveDocumentChanged(RoutedPropertyChangedEventArgs<ContentPane> args)
		{
			args.RoutedEvent = DocumentContentHost.ActiveDocumentChangedEvent;
			args.Source = this;
			this.OnActiveDocumentChanged(args);
		}

		/// <summary>
		/// Occurs when the <see cref="ActiveDocument"/> property has been changed
		/// </summary>
		/// <seealso cref="OnActiveDocumentChanged"/>
		/// <seealso cref="ActiveDocument"/>
		/// <seealso cref="ActiveDocumentChangedEvent"/>
		//[Description("Occurs when the 'ActiveDocument' property has been changed")]
		//[Category("DockManager Events")] // Behavior
		public event RoutedPropertyChangedEventHandler<ContentPane> ActiveDocumentChanged
		{
			add
			{
				base.AddHandler(DocumentContentHost.ActiveDocumentChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(DocumentContentHost.ActiveDocumentChangedEvent, value);
			}
		}

		#endregion //ActiveDocumentChanged

		#endregion //Events

		#region IAddChild Members

		void IAddChild.AddChild(object value)
		{
			DockManagerUtilities.ThrowIfNull(value, "value");

			SplitPane pane = value as SplitPane;

			if (null == pane)
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidDocumentContentHostChild"));

			this.Panes.Add(pane);
		}

		void IAddChild.AddText(string text)
		{
			throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidDocumentContentHostChild"));
		}

		#endregion

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			// AS 5/27/08 BR33298
			// If the template hasn't been applied then return the 
			// split panes.
			//
			//get { return this._containers; }
			get 
			{
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				return this.Panes;
			}
		}

		bool IPaneContainer.RemovePane(object pane)
		{
			// AS 5/28/08 BR33298
			// Since the host is now the logical parent, the DockManagerUtilities.GetParentPane
			// will return this so we need to be able to remove the pane.
			//
			SplitPane split = pane as SplitPane;

			if (null != split)
			{
				int index = this.Panes.IndexOf(split);

				if (index >= 0)
				{
					this.Panes.RemoveAt(index);
					return true;
				}
			}

			return false;
		}

		bool IPaneContainer.CanBeRemoved
		{
			get { return false; }
		}
		#endregion //IPaneContainer Members
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