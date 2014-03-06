using System;
using System.Collections.Generic;
using System.Windows;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Provides data for the <see cref="XamMenuBase.ItemClicked"></see>, events of a <see cref="XamMenuBase"/> control.
    /// </summary>
    public class ItemClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="XamMenuItem"/> that was clicked.
        /// </summary>
        public XamMenuItem Item
        {
            get;
            internal set;
        }
    }
    
    #region OpenedEventArgs

    /// <summary>
    /// Provides data for <see cref="XamContextMenu.Opened"/> event of <see cref="XamContextMenu"/>.
    /// </summary>
    public class OpenedEventArgs : EventArgs
    {
        #region Members
        private readonly XamContextMenu _contextMenu;
        #endregion //Members

        #region Constructor

        internal OpenedEventArgs(XamContextMenu contextMenu, Point mouseLocation, Point menuLocation)
        {
            this.MouseClickLocation = mouseLocation;
            this.ContextMenuLocation = menuLocation;
            this._contextMenu = contextMenu;
        }

        #endregion //Constructor

        #region Properties

        #region MouseClickLocation

        /// <summary>
        /// Gets the mouse click location.
        /// </summary>
        /// <remarks>The point is valid only when <see cref="XamContextMenu"/> opens with a mouse click.</remarks>
        public Point MouseClickLocation
        {
            get;
            internal set;
        }

        #endregion //MouseClickLocation	
    
        #region ContextMenuLocation

        /// <summary>
        /// Gets the location of the <see cref="XamContextMenu"/>.
        /// </summary>
        public Point ContextMenuLocation
        {
            get;
            internal set;
        }

        #endregion //ContextMenuLocation	
    
        #endregion //Properties

        #region Methods

        #region Public Methods

        #region GetClickedElements

        /// <summary>
        /// Returns the elements of the specified type at the coordinates of the mouse when the menu was opened.
        /// </summary>
        /// <typeparam name="T">The type of elements</typeparam>
        /// <returns>The list of elements at the coordinates of the mouse when the menu was opened.</returns>
        public List<T> GetClickedElements<T>() where T : UIElement
        {
            return this._contextMenu.GetClickedElements<T>();
        }

        #endregion //GetClickedElements

        #endregion //Public Methods

        #endregion //Methods
    }

    #endregion //OpenedEventArgs

    #region OpeningEventArgs

    /// <summary>
    /// Provides data for <seealso cref="XamContextMenu.Opening"/> event of <see cref="XamContextMenu"/>
    /// </summary>
    public class OpeningEventArgs : CancellableEventArgs
    {
        #region Members
        private readonly XamContextMenu _contextMenu;
        #endregion //Members

        #region Constructor

        internal OpeningEventArgs(XamContextMenu contextMenu, Point mouseLocation)
        {
            this.MouseClickLocation = mouseLocation;
            this._contextMenu = contextMenu;
        }

        #endregion //Constructor

        #region Properties

        #region MouseClickLocation

        /// <summary>
        /// Gets the mouse click location.
        /// </summary>
        /// <remarks>The point is valid only when <see cref="XamContextMenu"/> opens with a mouse click.</remarks>
        public Point MouseClickLocation
        {
            get;
            internal set;
        }

        #endregion //MouseClickLocation	
    
        #endregion //Properties

        #region Methods

        #region Public Methods

        #region GetClickedElements

        /// <summary>
        /// Returns the elements of the specified type at the coordinates of the mouse when the menu is opening.
        /// </summary>
        /// <typeparam name="T">The type of elements</typeparam>
        /// <returns>List of elements of the specified type at the coordinates of the mouse when the menu is opening.</returns>
        public List<T> GetClickedElements<T>() where T : UIElement
        {
            return this._contextMenu.GetClickedElements<T>();
        }

        #endregion //GetClickedElements

        #endregion //Public Methods

        #endregion //Methods
    }

    #endregion //OpeningEventArgs
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