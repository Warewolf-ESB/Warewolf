using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;

namespace Infragistics.Controls.Primitives
{
    /// <summary>
    /// Class used to route mouse messages to an appropriate <see cref="SelectionStrategyBase"/>
    /// </summary>
    internal sealed class SelectionController
    {
        #region Private Members

        private ISelectionHost _host;
        private SelectionStrategyBase _currentStrategy;
        private ISelectableItem _currentItem;

        #endregion //Private Members	
    
        #region Constructor







		internal SelectionController(ISelectionHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            this._host = host;
        }

        #endregion //Constructor	
    
        #region Properties

        // AS 8/19/08 ActiveStrategy
        #region ActiveStrategy
        internal SelectionStrategyBase ActiveStrategy
        {
            get { return this._currentStrategy; }
            set
            {
                if (value != this._currentStrategy)
                {
                    this._currentStrategy = value;
                    this.OnActiveStrategyChanged();
                }
            }
        } 
        #endregion //ActiveStrategy 
    
        #endregion //Properties

        #region Methods

            #region Public Methods

                #region CancelPendingOperations

        /// <summary>
        /// Called to cancel any pending mouse operations
        /// </summary>
		/// <returns>Returns true if successful.</returns>
        public bool CancelPendingOperations()
        {
            if (this._currentStrategy != null)
            {
				// ignore the cancel if it is being triggered internally
				// by the selection strategy which toggles capture on and
				// off in order to force a synchronous mouse move message
				// to be sent
				if (!this._currentStrategy.IsReleasingMouseCapture)
				{
					this._currentStrategy.CancelPendingOperations();
                    // AS 8/19/08 ActiveStrategy
					//this._currentStrategy = null;
                    this.ActiveStrategy = null;
					return true;
				}
            }

			return false;
        }

                #endregion //CancelPendingOperations

                #region OnMouseLeftButtonDown

        /// <summary>
        /// Called when the left mouse button is pressed
        /// </summary>
        /// <param name="e">The event arguments</param>
        public void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement root = this._host.RootElement;

            Debug.Assert(root != null);

            if (root == null)
                return;

            // get the selectable item under the mouse
            this._currentItem = GetSelectableItem(e, root);

            // if the mouse was down over an area that wasn't selectable then return
            if (this._currentItem == null)
                return;

            // get the selection strategy for the selectable item
            // AS 8/19/08 ActiveStrategy
            //this._currentStrategy = this._host.GetSelectionStrategyForItem(this._currentItem);
            this.ActiveStrategy = this._host.GetSelectionStrategyForItem(this._currentItem);

			// AS 8/11/08   +   JM 09-10-08 TFS5972 BR32616
			// Hold a reference to the strategy in case it should change. As part of this
			// I changed the references below from the member to the local.
			//
			//if (this._currentStrategy == null)
			//    return;
			SelectionStrategyBase strategy = this._currentStrategy;

			if (strategy == null)
				return;

            // see if the strategy is interested in the left mouse down
			// AS 3/25/10 TFS28679
			// We also want to exit if the mouse button was released. Just to be safe
			// we'll tell the selection strategy to cancel any pending operations 
			// (like its timers) if it thought it was successful.
			//
            //if (!strategy.OnMouseLeftButtonDown(this._currentItem, e))
			bool wasSuccessful = strategy.OnMouseLeftButtonDown(this._currentItem, e);

			if (!wasSuccessful )
            {
				if (wasSuccessful)
					strategy.CancelPendingOperations();

                // it returned false so it wasn't interested
                // AS 8/19/08 ActiveStrategy
                //this._currentStrategy = null;
                Debug.Assert(this.ActiveStrategy == null || this.ActiveStrategy == strategy, "The strategy was changed but we're going to clear it out.");
                this.ActiveStrategy = null;
                return;
            }

            // capture the mouse
			// AS 8/11/08   +   JM 09-10-08 TFS5972 BR32616
            // The capture call can result in a mouse move event but the 
            // elements might have been repositioned as a result so ignore
            // the move in that case. Otherwise when the strategy is a range
            // the mouse down will result in a range of items being selected
            // instead of just the item that we pressed the mouse down upon.
            //
            //root.CaptureMouse();
            // mark the event as handled
			strategy.CaptureMouseForRoot();

			e.Handled = true;
        }

                #endregion //OnMouseLeftButtonDown	
    
                #region OnMouseLeftButtonUp

        /// <summary>
        /// Called when the left mouse button is released
        /// </summary>
        /// <param name="e">The event arguments</param>
        public void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
	           if (this._currentStrategy == null)
					return;

                // get the selectable item under the mouse
                this._currentItem = GetSelectableItem(e, this._host.RootElement);

                this._currentStrategy.OnMouseLeftButtonUp(this._currentItem, e);

				// JJD 4/8/11 - TFS67003 
				// make sure the above processing didn't cause the _currentStrategy to be set to null
				if (this._currentStrategy != null)
					this._currentStrategy.ReleaseCapture();

                this._currentItem = null;
                this._currentStrategy = null;
            }
            finally
            {
                // release the mouse capture
                this.ReleaseCapture();
            }

            // AS 8/19/08 ActiveStrategy
            // I could have set the property above instead of the field but I thought
            // it might be better to release the capture first.
            //
            this.OnActiveStrategyChanged();

            e.Handled = true;
        }

                #endregion //OnMouseLeftButtonUp	
    
                #region OnMouseMove

        /// <summary>
        /// Called when the mouse is moved
        /// </summary>
        /// <param name="e">The event arguments</param>
        public void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (this._currentStrategy == null)
                return;

			//Debug.WriteLine("MuseMove X: " + e.MouseDevice.GetPosition((IInputElement)this._host).X.ToString() + " Y: " + e.MouseDevice.GetPosition((IInputElement)this._host).Y.ToString());

            // get the selectable item under the mouse
            this._currentItem = GetSelectableItem(e, this._host.RootElement);

			// JJD 9/19/06
			// We shouldn't release capture during a mouse selection even if 
			// the strategy's OnMouseMove returns false
			//if (!this._currentStrategy.OnMouseMove(this._currentItem, e))
			//{
			//    // Since ProcessMouseMove returned false then clear the stratgey an release the capture 
			//    this._currentStrategy = null;
			//    this.ReleaseCapture();
			//    return;
			//}
			this._currentStrategy.OnMouseMove(this._currentItem, e);


            e.Handled = true;


        }

                #endregion //OnMouseMove	
    
            #endregion //Public Methods	
    
            #region Private Methods

                // AS 8/19/08 ActiveStrategy
                #region OnActiveStrategyChanged
        private void OnActiveStrategyChanged()
        {
            if (null != this.ActiveStrategyChanged)
                this.ActiveStrategyChanged(this, EventArgs.Empty);
        } 
                #endregion //OnActiveStrategyChanged

                #region ReleaseCapture

        private void ReleaseCapture()
        {
            if (this._currentStrategy != null && this._currentStrategy.IsMouseCaptured)
                this._host.RootElement.ReleaseMouseCapture();
        }

                #endregion //ReleaseCapture	
    
            #endregion //Private Methods	
    
            #region Internal Methods

                #region GetSelectableItem

        static internal ISelectableItem GetSelectableItem(System.Windows.Input.MouseEventArgs e, DependencyObject root)
        {

			if (!(root is IInputElement))
				return null;

			if (!(root is Visual))
				return null;

			if (!(root is UIElement))
				return null;


			Point pt = e.GetPosition((UIElement)root);






#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			UIElement element = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(root as UIElement, pt, null);


			if (element == null)
				return null;

#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            // look for an element in the parent chain that implements ISelectableElement
			ISelectableElement selectableElement = GetSelectableElement(element, root);

			
			

            if (selectableElement == null)
                return null;

//			Debug.WriteLine(selectableElement.SelectableItem.ToString());

            return selectableElement.SelectableItem;
        }

		


#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)


                #endregion //GetSelectableItem	
    
                #region GetSelectableElement

        static internal ISelectableElement GetSelectableElement(DependencyObject element, DependencyObject root)
        {
            // if the element implements ISelectableElement then return it
            if (element is ISelectableElement)
                return (ISelectableElement)element;

            // if the element is the root then stop the parent chain walk and return null
            if (element == root)
                return null;

            // get the parent parent
			DependencyObject parent = VisualTreeHelper.GetParent(element);

            if (parent == null)
                return null;

            // call this method recursively to walk up the parent chain
            return GetSelectableElement(parent, root);
        }

                #endregion //GetSelectableElement	
    
            #endregion //Internal Methods	
    
        #endregion //Methods

        #region Events

        // AS 8/19/08 ActiveStrategy
        /// <summary>
        /// Raised when the <see cref="ActiveStrategy"/> has been changed.
        /// </summary>
        public event EventHandler ActiveStrategyChanged;

        #endregion //Events

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