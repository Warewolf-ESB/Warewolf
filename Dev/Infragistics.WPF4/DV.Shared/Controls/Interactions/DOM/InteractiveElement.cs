using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Base class for mouse and keyboard interactive objects.
    /// </summary>
    internal abstract class InteractiveElement : VisualElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveElement"/> class.
        /// </summary>
        protected InteractiveElement()
        {
   
        }

        #region Cursor

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>The cursor.</value>        
        [TypeConverter(typeof(CursorTypeConverter))]
        public Cursor Cursor
        {
            get { return (Cursor)GetValue(CursorProperty); }
            set { SetValue(CursorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cursor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CursorProperty =
            DependencyProperty.Register("Cursor", typeof(Cursor), typeof(InteractiveElement),
            new PropertyMetadata(Cursors.Arrow, OnCursorChanged));

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InteractiveElement)d).OnCursorChanged((Cursor)e.OldValue, (Cursor)e.NewValue);
        }
        /// <summary>
        /// Called when the value of the Cursor property is changed.
        /// </summary>
        /// <param name="oldValue">Old value of the Cursor property.</param>
        /// <param name="newValue">New value of the Cursor property.</param>
        protected virtual void OnCursorChanged(Cursor oldValue, Cursor newValue)
        {
        }

        #endregion

        private bool _isMouseOver;
        /// <summary>
        /// Indicates whether the mouse is currently over this element.
        /// </summary>
        public bool IsMouseOver
        {
            get { return this._isMouseOver; }
            private set { this._isMouseOver = value; }
        }

        private bool _isMouseCaptured;
        /// <summary>
        /// Indicates whether the mouse operation is being captured by this element.
        /// </summary>
        public bool IsMouseCaptured
        {
            get { return this._isMouseCaptured; }
            private set { this._isMouseCaptured = value; }
        }

        /// <summary>
        /// Method invoked when the mouse pointer is moved over this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseMove(MouseEventArgs e)
        {

        }
        /// <summary>
        /// Method invoked when the mouse pointer enters this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            this.IsMouseOver = true;
        }
        /// <summary>
        /// Method invoked whent he mouse pointer leaves this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            this.IsMouseOver = false;
        }
        /// <summary>
        /// Method invoked when the mouse left button is pressed over this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseLeftButtonDown(MouseEventArgs e)
        {
            this.IsMouseCaptured = true;
        }
        /// <summary>
        /// Method invoked when the mouse left button is released over this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseLeftButtonUp(MouseEventArgs e)
        {
            this.IsMouseCaptured = false;
        }
        
        /// <summary>
        /// Method invoked when the mouse left button is double clicked over this element.
        /// </summary>
        /// <param name="e">MouseEventArgs in context.</param>
        public virtual void OnMouseLeftButtonDoubleClick(MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Method invoked when a key is pressed over this element.
        /// </summary>
        /// <param name="e">KeyEventArgs in context.</param>
        public virtual void OnKeyDown(KeyEventArgs e)
        {

        }
        /// <summary>
        /// Method invoked when a key is released over this element.
        /// </summary>
        /// <param name="e">KeyEventArgs in context.</param>
        public virtual void OnKeyUp(KeyEventArgs e)
        {

        }
        /// <summary>
        /// Method invoked when this element is selected.
        /// </summary>
        /// <param name="e">EventArgs in context.</param>
        public virtual void OnGotSelection(EventArgs e)
        {

        }
        /// <summary>
        /// Method invoked when this element is unselected.
        /// </summary>
        /// <param name="e">EventArgs in context.</param>
        public virtual void OnLostSelection(EventArgs e)
        {

        }
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