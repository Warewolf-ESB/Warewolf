using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Markup;
using Infragistics.Windows.Controls;
using Infragistics.Windows;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// Abstract base class for Infragistics controls
    /// </summary>
    public abstract class IGControlBase : System.Windows.Controls.Control
    {
        #region Private Members

        private SelectionController _selectionController;
		private ResizeController _resizeController;

        #endregion //Private Members	
    
        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="IGControlBase"/> class
        /// </summary>
        protected IGControlBase()
        {
			// Call ThemeManager's VerifyThemeInformationRegistered method
			// to ensure that this assemblies theme are registered
			ThemeManager.VerifyThemeInformationRegistered(this.GetType().Assembly);

            if (this is ISelectionHost)
            {
                this._selectionController = new SelectionController((ISelectionHost)this);

                // AS 8/19/08 ActiveStrategy
                // The XamMonthCalendar mimics the common controls calendar behavior of
                // bringing the group for a leading/trailing item into view when a leading/
                // trailing item is activated. The group however should not be brought into
                // view while the mouse is being used to activate the item. In order to know 
                // when this happens (because the mouse may or may not be captured at the 
                // time and may or may not be down on the element) I'm adding a virtual 
                // method for a derived class to know when the selection strategy used 
                // by the selection controller is changed.
                //
                this._selectionController.ActiveStrategyChanged += new EventHandler(OnActiveSelectionStrategyChanged);
            }
            if (this is IResizeHost)
                this._resizeController = ((IResizeHost)this).Controller;
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
            IResizeHost resizeHost = this as IResizeHost;

            if (resizeHost != null &&
                 resizeHost.Controller.ResizerBar != null)
                resizeHost.Controller.PositionResizerBar();

            return base.ArrangeOverride(finalSize);
        }

            #endregion //ArrangeOverride	

            #region GetVisualChild

        /// <summary>
        /// Gets the parent child at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the specific child parent.</param>
        /// <returns>The parent child at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            IResizeHost resizeHost = this as IResizeHost;

            if ( resizeHost != null &&
                 resizeHost.Controller.ResizerBar != null &&
				 // AS 6/16/09
				 // This assumes that the derived class doesn't override the 
				 // GetVisualChild/VisualChildrenCount and manipulate the index.
				 // It should really only consider the base index.
				 //
                 //index == this.VisualChildrenCount - 1)
                 index == base.VisualChildrenCount)
            {
                return resizeHost.Controller.ResizerBar;
            }

            return base.GetVisualChild(index);
        }

            #endregion //GetVisualChild	
    
            #region OnKeyDown

        /// <summary>
        /// Called when a key is pressed
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
			if (e.Key == Key.Escape)
				e.Handled = this.CancelPendingMouseOperations();

			base.OnKeyDown(e);

			if ( e.Handled == true )
				return;

			// Pass this key along to our commands class which will check to see if a command
			// needs to be executed.  If so, the commands class will execute the command and
			// return true.
			if (e.Handled == false && 
				this.Commands != null && 
				this.Commands.ProcessKeyboardInput(e, this as ICommandHost) == true)
				e.Handled = true;
        }

            #endregion //OnKeyDown	

            #region OnLostMouseCapture

        /// <summary>
        /// Called when mouse capture is lost
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
        {
			// SSP 5/20/09 TFS17816
			// LostMouseCapture is a bubble event. If some other element inside the data presenter
			// had capture (for example the label presenter) and the capture is given to the data 
			// presenter, the OnLostMouseCapture will get called on the data presenter. However 
			// this doesn't mean that the data presenter has lost capture, especially if the 
			// capture is being given to it currently.
			// 
			// ------------------------------------------------------------------------------------
            //this.CancelPendingMouseOperations();
			if ( e.OriginalSource == this || ! this.IsMouseCaptured )
				this.CancelPendingMouseOperations( );
			// ------------------------------------------------------------------------------------

            base.OnLostMouseCapture(e);
        }

            #endregion //OnLostMouseCapture	

			#region OnMouseLeftButtonDown

        /// <summary>
        /// Called when the left mouse button is pressed
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

			if (this._selectionController != null && !e.Handled)
			{
				this._selectionController.OnMouseLeftButtonDown(e);

				if (e.Handled)
					return;
			}
        }

            #endregion //OnMouseLeftButtonDown	
            
            #region OnMouseMove

        /// <summary>
        /// Called when the mouse is moved
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

			// if we are in the middle of a resizing operation then just return
			if (this._resizeController != null &&
				 this._resizeController.IsResizing)
				return;

			if (this._selectionController != null && !e.Handled)
            {
                this._selectionController.OnMouseMove(e);

                if (e.Handled)
                    return;
            }
        }

            #endregion //OnMouseMove	
            
            #region OnMouseLeftButtonUp

        /// <summary>
        /// Called when the left mouse button is released
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
			base.OnMouseLeftButtonUp(e);

			// if we are in the middle of a resizing operation then just return
			if (this._resizeController != null &&
				 this._resizeController.IsResizing)
				return;

			if (this._selectionController != null && !e.Handled)
			{
				this._selectionController.OnMouseLeftButtonUp(e);

				if (e.Handled)
					return;
			}
         }

            #endregion //OnMouseLeftButtonUp	

            #region OnPreviewMouseLeftButtonDown

        /// <summary>
        /// Called when the left mouse button is pressed
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

			// give the control focus if it doesn't already have it
			if (!e.Handled &&
				!this.IsKeyboardFocused &&
				!this.IsKeyboardFocusWithin &&
				this.Focusable &&
				this.IsEnabled
				// AS 9/2/09 TFS19928
				// IsKeyboardFocusWithin may be false but focus may logically be within 
				// the control and so we shouldn't steal focus from the focused element.
				//
				&& !Utilities.IsDescendantOf(this, Keyboard.FocusedElement as DependencyObject, true)				
				)
				this.Focus();
        }

            #endregion //OnPreviewMouseLeftButtonDown	
    
            #region VisualChildrenCount

        /// <summary>
        /// Returns the total number of parent children (read-only)
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                int count = base.VisualChildrenCount;

                IResizeHost resizeHost = this as IResizeHost;

                if ( resizeHost != null &&
                     resizeHost.Controller.ResizerBar != null )
                    count++;

                return count;
            }
        }

            #endregion //VisualChildrenCount	
    
        #endregion //Base class overrides

        #region Properties

			#region Protected Properties

				#region Commands

		/// <summary>
		/// Returns a collection of commands this control exposes (read-only).
		/// </summary>
		protected internal abstract CommandsBase Commands { get; }

				#endregion //Commands	
    
			#endregion //Protected Properties	
    
            #region Private Properties

                #region ResizeController

		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


                #endregion //ResizeController	
    
            #endregion //Private Properties	
    
        #endregion //Properties

        #region Methods

            #region Protected Methods

                // AS 8/19/08 ActiveStrategy
                #region OnActiveSelectionStrategyChanged
                /// <summary>
                /// Invoked when the active <see cref="SelectionStrategyBase"/> used by the control has been changed.
                /// </summary>
                /// <param name="strategy">The new strategy</param>
                protected virtual void OnActiveSelectionStrategyChanged(SelectionStrategyBase strategy)
                {
                }
                #endregion //OnActiveSelectionStrategyChanged 

            #endregion //Protected Methods

            #region Private Methods

                #region CancelPendingMouseOperations

        private bool CancelPendingMouseOperations()
        {
            IResizeHost resizeHost = this as IResizeHost;

			bool handled = false;

			if (resizeHost != null &&
				 resizeHost.Controller.IsResizing)
			{
				resizeHost.Controller.CancelResize();
				handled = true;
			}

            if (this._selectionController != null)
                handled = handled || this._selectionController.CancelPendingOperations();

			return handled;
        }

                #endregion //CancelPendingMouseOperations	
    
                // AS 8/19/08 ActiveStrategy
                #region OnActiveSelectionStrategyChanged
                private void OnActiveSelectionStrategyChanged(object sender, EventArgs e)
                {
                    this.OnActiveSelectionStrategyChanged(this._selectionController.ActiveStrategy);
                } 
                #endregion //OnActiveSelectionStrategyChanged

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