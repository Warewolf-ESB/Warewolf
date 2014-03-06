using System;
using System.Windows;
using System.Windows.Controls;
using Infragistics.DragDrop;

namespace Infragistics.Controls.Menus
{
    #region NodeLayoutEventArgs
    /// <summary>
    /// A class listing the <see cref="NodeLayout"/> that was modified for an event.
    /// </summary>
    public class NodeLayoutEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="NodeLayout"/> that was modified.
        /// </summary>
        public NodeLayout NodeLayout
        {
            get;
            protected internal set;
        }
    }
    #endregion // NodeLayoutEventArgs

    #region NodeEventArgs
    /// <summary>
    /// A class listing the EventArgs for an event with a <see cref="XamDataTreeNode"/> input.
    /// </summary>
    public class NodeEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="XamDataTreeNode"/> that was modified.
        /// </summary>
        public XamDataTreeNode Node
        {
            get;
            protected internal set;
        }
    }

    #endregion // NodeEventArgs

    #region InitializeNodeEventArgs

    /// <summary>
    /// A class listing the information needed during the <see cref="XamDataTree.InitializeNode"/> event.
    /// </summary>
    public class InitializeNodeEventArgs : NodeEventArgs
    {
    }

    #endregion // InitializeNodeEventArgs

    #region CancellableNodeExpansionChangedEventArgs

    /// <summary>
    /// A class listing the information needed for node expanding events.
    /// </summary>
    public class CancellableNodeExpansionChangedEventArgs : CancellableNodeEventArgs
    {
    }

    #endregion // CancellableNodeExpansionChangedEventArgs

    #region NodeExpansionChangedEventArgs

    /// <summary>
    /// A class listing the information needed for node expanded events.
    /// </summary>
    public class NodeExpansionChangedEventArgs : NodeEventArgs
    {
    }

    #endregion // NodeExpansionChangedEventArgs

    #region NodeLayoutAssignedEventArgs

    /// <summary>
    /// A class listing the information needed for the <see cref="XamDataTree.NodeLayoutAssigned"/>  event.
    /// </summary>
    public class NodeLayoutAssignedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets / sets the <see cref="NodeLayout"/> which will be assinged to this <see cref="XamDataTreeNode"/>.
        /// </summary>
        public NodeLayout NodeLayout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the depth on the <see cref="XamDataTree"/> that <see cref="XamDataTreeNode"/> will appear.
        /// </summary>
        public int Level
        {
            get;
            protected internal set;
        }


        /// <summary>
        /// Gets the <see cref="NodeLayoutBase.Key"/> of the <see cref="NodeLayout"/> which will be applied.
        /// </summary>
        public string Key
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> which the <see cref="XamDataTreeNode"/> is bound to.
        /// </summary>
        public Type DataType
        {
            get;
            protected internal set;
        }
    }

    #endregion // NodeLayoutAssignedEventArgs

    #region ActiveNodeChangedEventArgs

    /// <summary>
    /// A class listing the information needed for the <see cref="XamDataTree.ActiveNodeChanged"/> event.
    /// </summary>
    public class ActiveNodeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="XamDataTreeNode"/> which was originally active.
        /// </summary>
        public XamDataTreeNode OriginalActiveTreeNode
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the <see cref="XamDataTreeNode"/> which is currently active.
        /// </summary>
        public XamDataTreeNode NewActiveTreeNode
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// This property is not used in this event.
        /// </summary>
        [Obsolete("The Cancel property on ActiveNodeChangedEventArgs will no longer be used.  To cancel the setting of the active node then the argument on the ActiveNodeChanging event should be modified.")]
        public bool Cancel { get; set; }
    }

    #endregion // ActiveNodeChangedEventArgs

    #region CancellableNodeEventArgs

    /// <summary>
    /// A class listing the information needed for <see cref="XamDataTreeNode"/> based events which may be cancelled by the developer.
    /// </summary>
    public abstract class CancellableNodeEventArgs : CancellableEventArgs
    {
        /// <summary>
        /// The <see cref="XamDataTreeNode"/> that was modified.
        /// </summary>
        public XamDataTreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion // CancellableNodeEventArgs

    #region BeginEditingNodeEventArgs

    /// <summary>
    /// A class listing the information needed for the <see cref="XamDataTree"/> to begin editing on a <see cref="XamDataTreeNode"/>.
    /// </summary>
    public class BeginEditingNodeEventArgs : CancellableNodeEventArgs
    {
    }

    #endregion // BeginEditingNodeEventArgs

    #region TreeEditingNodeEventArgs

    /// <summary>
    /// Provides information to editing events.
    /// </summary>
    public class TreeEditingNodeEventArgs : NodeEventArgs
    {
        /// <summary>
        /// Gets the editor that is being displayed in the <see cref="XamDataTreeNode"/>
        /// </summary>
        public FrameworkElement Editor
        {
            get;
            protected internal set;
        }
    }

    #endregion // TreeEditingNodeEventArgs

    #region TreeExitEditingEventArgs

    /// <summary>
    /// Provides information necessary when a <see cref="XamDataTreeNode"/> is exiting edit mode.
    /// </summary>
    public class TreeExitEditingEventArgs : CancellableNodeEventArgs
    {
        /// <summary>
        /// Gets the new value that was entered into the editor.
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Gets if the editing was canceled by the user.
        /// </summary>
        public bool EditingCanceled { get; protected internal set; }

        /// <summary>
        /// Gets the editor that is being displayed in the <see cref="XamDataTreeNode"/>
        /// </summary>
        public FrameworkElement Editor
        {
            get;
            protected internal set;
        }
    }
    #endregion // TreeExitEditingEventArgs

    #region NodeValidationErrorEventArgs

    /// <summary>
    /// Provides information when a validation error happens as the <see cref="XamDataTreeNode"/> exits edit mode.
    /// </summary>
    public class NodeValidationErrorEventArgs : NodeEventArgs
    {
        /// <summary>
        /// The actual <see cref="ValidationErrorEventArgs"/>.
        /// </summary>
        public ValidationErrorEventArgs ValidationErrorEventArgs { get; protected internal set; }

        /// <summary>
        /// Gets/sets whether the event is handled. If true, then the <see cref="XamDataTreeNode"/> will treat the validation as if it had passed.
        /// </summary>
        public bool Handled { get; set; }
    }

    #endregion // NodeValidationErrorEventArgs

    #region TreeDataObjectCreationEventArgs

    /// <summary>
    /// A class listing the information needed when a new object needs to be created.
    /// </summary>
    public class TreeDataObjectCreationEventArgs : NodeLayoutEventArgs
    {
        /// <summary>
        /// Gets / sets an object of the <see cref="ObjectType"/> which will be used as the newly created object.
        /// </summary>
        public object NewObject { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of object that the DataManager expects to be created.
        /// </summary>
        public Type ObjectType { get; protected internal set; }

        /// <summary>
        /// Gets the <see cref="XamDataTreeNode"/> object which is the parent for this object.  
        /// </summary>		
        public XamDataTreeNode ParentNode { get; protected internal set; }

        /// <summary>
        /// Gets the <see cref="Type"/> which is contained in the underlying collection.
        /// </summary>
        public Type CollectionType { get; protected internal set; }

    }
    #endregion // TreeDataObjectCreationEventArgs

    #region NodeSelectionEventArgs

    /// <summary>
    /// A class listing the information needed when the selected nodes change.
    /// </summary>
    public class NodeSelectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="SelectedNodesCollection"/> of nodes that were previously selected.
        /// </summary>
        public SelectedNodesCollection OriginalSelectedNodes { get; protected internal set; }

        /// <summary>
        /// Gets the <see cref="SelectedNodesCollection"/> of nodes that are selected.
        /// </summary>
        public SelectedNodesCollection CurrentSelectedNodes { get; protected internal set; }
    }

    #endregion // NodeSelectionEventArgs

    #region TreeDropEventArgs class
    /// <summary>
    /// Event arguments for <see cref="DragSource.Drop"/> event.
    /// </summary>
    public class TreeDropEventArgs : HandleableEventArgs
    {
        #region Constructors

        internal TreeDropEventArgs(DragDropEventArgs baseArgs)
        {
            this.DragDropEventArgs = baseArgs;

        }

        #endregion // Constructors

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the <see cref="Infragistics.DragDrop.DragDropEventArgs"/> object which were generated for this event.
        /// </summary>
        public DragDropEventArgs DragDropEventArgs
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the <see cref="TreeDropDestination"/> value which determines where the drop is perceived when a <see cref="XamDataTreeNode"/> is dropped in relation to it's drop target.
        /// </summary>
        public TreeDropDestination DropDestination 
        { 
            get; 
            protected internal set; 
        }

        #endregion // Public Properties

        #endregion // Properties
    }
    #endregion // DropEventArgs

    #region NodeDeletionEventArgs

    /// <summary>
    /// A class listing the EventArgs for deleting a node in the <see cref="XamDataTree"/>.
    /// </summary>
    public class CancellableNodeDeletionEventArgs : CancellableNodeEventArgs
    {
    }

    #endregion // NodeDeletionEventArgs

    #region NodeDeletedEventArgs

    /// <summary>
    /// A class listing the EventArgs after a node was deleted.
    /// </summary>
    public class NodeDeletedEventArgs : NodeEventArgs
    {
    }

#endregion // NodeDeletedEventArgs

    #region ActiveNodeChangingEventArgs

    /// <summary>
    /// A class listing the information needed for the <see cref="XamDataTree.ActiveNodeChanging"/> event.
    /// </summary>
    public class ActiveNodeChangingEventArgs : CancellableEventArgs
    {
        /// <summary>
        /// Gets the <see cref="XamDataTreeNode"/> which was originally active.
        /// </summary>
        public XamDataTreeNode OriginalActiveTreeNode
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the <see cref="XamDataTreeNode"/> which is currently active.
        /// </summary>
        public XamDataTreeNode NewActiveTreeNode
        {
            get;
            protected internal set;
        }
    }

    #endregion // ActiveNodeChangingEventArgs
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