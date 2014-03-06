using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Infragistics
{
    /// <summary>
    /// Represents a control that supports interactive tools.
    /// </summary>
    public abstract class InteractiveControl : Control
    {
        private const int DoubleClickInterval = 300;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveControl"/> class.
        /// </summary>
        protected InteractiveControl()
        {
            this.DefaultStyleKey = typeof(InteractiveControl);

            this.VisualElements = new ObservableCollection<VisualElement>();
            this.VisualElements.CollectionChanged += new NotifyCollectionChangedEventHandler(VisualElements_CollectionChanged);

            this.LastInput = new InputContext();

            this.MouseDownTools = new Collection<ITool>();
            this.MouseMoveTools = new Collection<ITool>();

            this.DefaultTool = new DefaultTool(this);
            this.CurrentTool = this.DefaultTool;

            this.DoubleClickTimer = new DispatcherTimer();
            this.DoubleClickTimer.Interval = new TimeSpan(0, 0, 0, 0, DoubleClickInterval);
            this.DoubleClickTimer.Tick += new EventHandler(DoubleClickTimer_Tick);            
        }

        #region LastInput

        /// <summary>
        /// Gets or sets the last input.
        /// </summary>
        /// <value>The last input.</value>
        public InputContext LastInput
        {
            get { return (InputContext)GetValue(LastInputProperty); }
            set { SetValue(LastInputProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="LastInput"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LastInputProperty =
            DependencyProperty.Register("LastInput", typeof(InputContext), typeof(InteractiveControl),
              new PropertyMetadata(null));

        #endregion

        private ITool _defaultTool;
        private ITool DefaultTool
        {
            get { return this._defaultTool; }
            set { this._defaultTool = value; }
        }

        private Canvas _canvas;
        /// <summary>
        /// Canvas containing the content of this InteractiveControl.
        /// </summary>
        public virtual Canvas Canvas
        {
            get { return this._canvas; }
            protected set { this._canvas = value; }
        }

        private ITool _currentTool;
        /// <summary>
        /// The current tool being used to handle interaction.
        /// </summary>
        public ITool CurrentTool
        {
            get
            {
                return this._currentTool;
            }
            set
            {
                if (this._currentTool != value)
                {
                    if (this._currentTool != null)
                        this._currentTool.Stop();

                    if (value == null)
                        this._currentTool = this.DefaultTool;
                    else
                        this._currentTool = value;

                    if (this._currentTool != null)
                        this._currentTool.Start();
                }
            }
        }

        private Collection<ITool> _mouseDownTools;
        /// <summary>
        /// A Collection of tools to handle MouseDown actions.
        /// </summary>
        public Collection<ITool> MouseDownTools
        {
            get
            {
                return this._mouseDownTools;
            }
            private set
            {
                this._mouseDownTools = value;
            }
        }

        private Collection<ITool> _mouseMoveTools;
        /// <summary>
        /// A Collection of tools to handle MouseMove actions.
        /// </summary>
        public Collection<ITool> MouseMoveTools
        {
            get
            {
                return this._mouseMoveTools;
            }
            private set
            {
                this._mouseMoveTools = value;
            }
        }

        /// <summary>
        /// Returns the element under a given mouse point.
        /// </summary>
        /// <param name="point">The point under observation.</param>
        /// <returns>The topmost element that returns a positive hit test for the given point.</returns>
        internal virtual InteractiveElement HitTest(Point point)
        {
            Collection<InteractiveElement> elements = this.InteractiveElements;

            for (int i = elements.Count - 1; i >= 0; i--)
            {
                InteractiveElement element = elements[i];

                if (element.HitTest(point))
                {
                    return element;
                }
            }

            return null;
        }

        private ShapeElement _capturedElement;
        internal ShapeElement CapturedElement
        {
            get { return _capturedElement; }
            set { _capturedElement = value; }
        }

        internal bool IsManuallyReleasingMouseCapture { get; set; }

        internal void ReleaseMouseCaptures()
        {
            if (this.CapturedElement != null &&
                this.CapturedElement.ActiveFrameworkElement != null)
            {
                this.IsManuallyReleasingMouseCapture = true;
                this.CapturedElement.ActiveFrameworkElement.ReleaseMouseCapture();
            }
        }

        private bool IsCanvasValid()
        {
            if (this.Canvas == null ||
                this.Canvas.ActualHeight == 0 ||
                this.Canvas.ActualWidth == 0)
            {
                return false;
            }

            return true;
        }

        #region Protected Methods
        /// <summary>
        /// Method invoked when the mouse is moved over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsCanvasValid() == false)
            {
                return;
            }

            this.LastInput.ViewMousePosition = e.GetPosition(null);

            Point docMousePosition = e.GetPosition(this.Canvas);
            this.LastInput.DocMousePosition = docMousePosition;

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            foreach (InteractiveElement element in elements)
            {
                if (element.HitTest(docMousePosition))
                {
                    if (element.IsMouseOver)
                    {
                        element.OnMouseMove(e);
                    }
                    else
                    {
                        element.OnMouseEnter(e);
                    }
                }
                else
                {
                    if (element.IsMouseOver)
                    {
                        element.OnMouseLeave(e);
                    }
                }
            }

            this.CurrentTool.MouseMove();
        }
        /// <summary>
        /// Method invoked when the mouse pointer enters this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (IsCanvasValid() == false)
            {
                return;
            }

            this.LastInput.ViewMousePosition = e.GetPosition(null);
            this.LastInput.DocMousePosition = e.GetPosition(this.Canvas);

            this.CurrentTool.Start();
        }
        /// <summary>
        /// Method invoked when the mouse pointer leaves this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            foreach (InteractiveElement element in elements)
            {
                if (element.IsMouseOver)
                {
                    element.OnMouseLeave(e);
                }
            }

            // this.CurrentTool = null;
        }

        /// <summary>
        /// Method invoked when the control lost mouse capture.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (this.IsManuallyReleasingMouseCapture)
            {
                this.IsManuallyReleasingMouseCapture = false;
                return;
            }

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            foreach (InteractiveElement element in elements)
            {
                if (element.IsMouseOver)
                {
                    element.OnMouseLeave(e);
                }
            }

            this.CurrentTool = null;
        }

        /// <summary>
        /// Method invoked when the left mouse button is pressed over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (IsCanvasValid() == false)
            {
                return;
            }

            this.LastInput.ViewMousePosition = e.GetPosition(null);
            this.LastInput.DocMousePosition = e.GetPosition(this.Canvas);

            Debug.WriteLine("Mouse Left button down");

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            Point docMousePosition = this.LastInput.DocMousePosition;

            foreach (InteractiveElement element in elements)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnMouseLeftButtonDown(e);
                }
            }

            this.CurrentTool.MouseLeftButtonDown(e);

            CheckMouseDoubleClick(e);
        }
        /// <summary>
        /// Method invoked when the left mouse button is released over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsCanvasValid() == false)
            {
                return;
            }

            this.LastInput.ViewMousePosition = e.GetPosition(null);
            this.LastInput.DocMousePosition = e.GetPosition(this.Canvas);

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            foreach (InteractiveElement element in elements)
            {
                if (element.IsMouseCaptured)
                {
                    element.OnMouseLeftButtonUp(e);
                }
            }

            this.CurrentTool.MouseLeftButtonUp(e);
        }

        /// <summary>
        /// Method invoked when the left mouse button is double clicked over this control.
        /// </summary>
        /// <param name="e">The MouseEventArgs in context.</param>
        protected virtual void OnMouseLeftButtonDoubleClick(MouseButtonEventArgs e)
        {
            ICollection<InteractiveElement> elements = this.InteractiveElements;

            foreach (InteractiveElement element in elements)
            {
                if (element.IsMouseCaptured)
                {
                    element.OnMouseLeftButtonDoubleClick(e);
                }
            }

            this.CurrentTool.OnMouseLeftButtonDoubleClick();
        }

        /// <summary>
        /// Method invoked when a key is pressed while this control has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            this.LastInput.Key = e.Key;

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            Point docMousePosition = this.LastInput.DocMousePosition;

            foreach (InteractiveElement element in elements)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnKeyDown(e);
                }
            }

            this.CurrentTool.KeyDown();
        }
        /// <summary>
        /// Method invoked when a key is released while this control has focus.
        /// </summary>
        /// <param name="e">The KeyEventArgs in context.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            this.LastInput.Key = e.Key;

            ICollection<InteractiveElement> elements = this.InteractiveElements;

            Point docMousePosition = this.LastInput.DocMousePosition;

            foreach (InteractiveElement element in elements)
            {
                if (element.HitTest(docMousePosition))
                {
                    element.OnKeyUp(e);
                }
            }

            this.CurrentTool.KeyUp();
        }
        
        #endregion

        #region Event Handlers

        private void VisualElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (VisualElement element in e.NewItems)
                    {
                        element.SetView(this);
                        element.Render();
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (VisualElement element in e.OldItems)
                    {
                        element.Remove();
                    }
                    break;
            }
        }

        #endregion

        #region MouseDoubleClick Methods

        private Point _clickPoint;
        private Point ClickPoint
        {
            get { return _clickPoint; }
            set { _clickPoint = value; }
        }

        private DispatcherTimer _doubleClickTimer;
        /// <summary>
        /// Timer used to detect double-click actions.
        /// </summary>
        private DispatcherTimer DoubleClickTimer
        {
            get { return _doubleClickTimer; }
            set { _doubleClickTimer = value; }
        }

        private void CheckMouseDoubleClick(MouseButtonEventArgs e)
        {
            Point pt = this.LastInput.DocMousePosition;

            if (this.DoubleClickTimer.IsEnabled && IsMouseMoved(this.ClickPoint, pt) == false)
            {
                // a double click has occured     
                this.DoubleClickTimer.Stop();

                OnMouseLeftButtonDoubleClick(e);
            }
            else
            {
                this.DoubleClickTimer.Start();
                this.ClickPoint = pt;
            }
        }

        private static bool IsMouseMoved(Point pt1, Point pt2)
        {
            return System.Math.Abs(pt1.X - pt2.X) > 3 || System.Math.Abs(pt1.Y - pt2.Y) > 3;
        }

        private void DoubleClickTimer_Tick(object sender, EventArgs e)
        {
            this.DoubleClickTimer.Stop();
        }

        #endregion

        private ObservableCollection<VisualElement> _visualElements;
        /// <summary>
        /// The collection of visual elements within this control.
        /// </summary>
        internal ObservableCollection<VisualElement> VisualElements
        {
            get
            {
                return this._visualElements;
            }
            private set
            {
                this._visualElements = value;
            }
        }

        /// <summary>
        /// The collection of interactive elements within this control.
        /// </summary>
        internal Collection<InteractiveElement> InteractiveElements
        {
            get
            {
                Collection<InteractiveElement> elements = new Collection<InteractiveElement>();

                foreach (VisualElement visualElement in this.VisualElements)
                {
                    InteractiveElement interactiveElement = visualElement as InteractiveElement;
                    if (interactiveElement != null)
                    {
                        elements.Add(interactiveElement);
                    }
                }

                return elements;
            }
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