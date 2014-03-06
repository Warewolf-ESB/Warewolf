using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Documents;


namespace Infragistics.DragDrop
{
    #region DragDropManager class
    /// <summary>
    /// Registers and manages interaction between elements marked as drag sources and drop targets and triggers
    /// the raising of appropriate events about drag-and-drop operation to the user.
    /// </summary>
    public class DragDropManager
    {
        #region Members

        #region Private Members

        // private static Dictionary<WeakReference, IList<WeakReference>> _rootTargets = new Dictionary<WeakReference, IList<WeakReference>>();
        private static DragDropManager _instance;

        private List<WeakReference> _dropTargets = new List<WeakReference>();
        private List<WeakReference> _dragSources = new List<WeakReference>();

        private IList<WeakReference> _highlightedTargets = new List<WeakReference>();

        private bool _dragging;
        private bool _mouseCaptured;
        private bool _needReleaseMouse;
        private bool _waitDragStart;
        private bool _mouseMoving;

        private bool _gotCursor;

        private bool _endDrag;






        private UIElement _dragPopup;
        private UIElement _cursorPopup;
        private UIElement _targetMarkerPopup;

        private Image _snapshotImage;

        private UIElement _dropTargetElement;
        private UIElement _dragSourceElement;
        private UIElement _originalDragSourceElement;
        private UIElement _draggedElement;
        private UIElement _trackedElement;

        private DragSource _dragSource;
        private DropTarget _dropTarget;

        private Point _initialMousePosition = new Point(0, 0);
        private Point _mousePosition = new Point(0, 0);
        private Point _mouseOffset = new Point(0, 0);
        private Point _draggedOffset = new Point(0, 0);

        private DataTemplate _currentDropNotAllowedCursorTemplate;
        private DataTemplate _customDropNotAllowedCursorTemplate;
        private DataTemplate _defaultDropNotAllowedCursorTemplate;

        private DataTemplate _currentMoveCursorTemplate;
        private DataTemplate _customMoveCursorTemplate;
        private DataTemplate _defaultMoveCursorTemplate;

        private DataTemplate _currentCopyCursorTemplate;
        private DataTemplate _customCopyCursorTemplate;
        private DataTemplate _defaultCopyCursorTemplate;

        private DataTemplate _currentCursorTemplate;

        private DataTemplate _defaultDragTemplate;
        private DataTemplate _dragSourceDragTemplate;

        private DragDropEventArgs _dragDropEventArgs;
        private ContentControl _customCursorContentControl;
        private ContentControl _dragContentControl;

        private DragObject _dragObject;

        // private Style _targetStyle;

        private MouseEventArgs _mouseEventArgs;

        private Cursor _originalCursor;
        private Exception _error;


        private UIElement _rootVisual;
        private static List<WeakReference> registeredDragSources = new List<WeakReference>();
        private static int currentThreadId;
        private UIElement _target;
        private Popup _popupHost;

        private static bool isThreadIdInitialized;

        #endregion // Private Members

        #endregion // Members

        #region Properties

        #region Public Properties

        #region CurrentCopyCursorTemplate

        /// <summary>
        /// Gets data template that will be used for copy cursor when copy operation is performed.
        /// </summary>
        public static DataTemplate CurrentCopyCursorTemplate
        {
            get
            {
                return Instance._currentCopyCursorTemplate;
            }
        }

        #endregion // CurrentCopyCursorTemplate

        #region CurrentCursorTemplate

        /// <summary>
        /// Gets data template that is currently in usage as a mouse cursor.
        /// </summary>
        public static DataTemplate CurrentCursorTemplate
        {
            get
            {
                return Instance._currentCursorTemplate;
            }
        }

        #endregion // CurrentCursorTemplate

        #region CurrentDragTemplate

        /// <summary>
        /// Gets data template that will be applied to dragged item when drag-and-drop operation starts.
        /// </summary>
        public static DataTemplate CurrentDragTemplate
        {
            get
            {
                return Instance._dragging ?
                    Instance._dragDropEventArgs.DragTemplate :
                    null;
            }
        }

        #endregion // CurrentDragTemplate

        #region CurrentDropNotAllowedCursorTemplate

        /// <summary>
        /// Gets data template that will be used for drop operation not allowed 
        /// cursor if during drag-and-drop operation there is not found appropriate drop target.
        /// </summary>
        public static DataTemplate CurrentDropNotAllowedCursorTemplate
        {
            get
            {
                return Instance._currentDropNotAllowedCursorTemplate;
            }
        }

        #endregion // CurrentDropNotAllowedCursorTemplate

        #region CurrentMoveCursorTemplate

        /// <summary>
        /// Gets data template that will be used for move cursor during drag-and-drop operation.
        /// </summary>
        public static DataTemplate CurrentMoveCursorTemplate
        {
            get
            {
                return Instance._currentMoveCursorTemplate;
            }
        }

        #endregion // CurrentMoveCursorTemplate
        #region DragPopup

        /// <summary>
        /// Gets the UI element used to display dragged item on the screen.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        public static UIElement DragPopup
        {
            get
            {
                return Instance._dragPopup;
            }
        }


        #endregion // DragPopup
        #region IsDragging

        /// <summary>
        /// Gets a value indicating whether drag-and-drop operation is currently in progress.
        /// </summary>
        public static bool IsDragging
        {
            get
            {
                return Instance._dragging;
            }
        }

        #endregion // IsDragging

        #region DragSource

        /// <summary>
        /// Identifies the <see cref="DragSourceProperty"/> attached property.
        /// </summary>
        public static readonly DependencyProperty DragSourceProperty =
            DependencyProperty.RegisterAttached(
            "DragSource",
            typeof(DragSource),
            typeof(DragDropManager),
            new PropertyMetadata(new PropertyChangedCallback(OnDragSourceChanged)));

        private static void OnDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // unregister must occurs first
            if (e.OldValue != null)
            {
                DragSource dragSource = e.OldValue as DragSource;

                if (dragSource != null)
                {
                    if (dragSource.IsDraggable && dragSource.AssociatedObject != null)
                    {

                        if (registeredDragSources.Contains(dragSource))
                        {
                            registeredDragSources.Remove(dragSource);
                        }

                        UnregisterDragSource(dragSource.AssociatedObject);
                    }

                    dragSource.AssociatedObject = null;
                }
            }

            if (e.NewValue != null)
            {
                DragSource dragSource = e.NewValue as DragSource;

                if (dragSource != null)
                {


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                    // retain as associated object the top most visual only
                    if (!registeredDragSources.Contains(dragSource))
                    {
                        dragSource.AssociatedObject = d as UIElement;
                        registeredDragSources.Add(dragSource);

                        if (dragSource.IsDraggable && dragSource.AssociatedObject != null)
                        {
                            RegisterDragSource(d as UIElement);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="DragSourceProperty"/> attached property from a given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>The value of the <see cref="DragSourceProperty"/> attached property.</returns>
        public static DragSource GetDragSource(DependencyObject obj)
        {
            return (DragSource)obj.GetValue(DragSourceProperty);
        }

        /// <summary>
        /// Sets value for <see cref="DragSourceProperty"/> attached property to a given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object on which to set <see cref="DragSourceProperty"/> attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetDragSource(DependencyObject obj, DragSource value)
        {
            obj.SetValue(DragSourceProperty, value);
        }

        #endregion // DragSource

        #region DropTarget

        /// <summary>
        /// Identifies the <see cref="DropTargetProperty"/> attached property.
        /// </summary>
        public static readonly DependencyProperty DropTargetProperty =
            DependencyProperty.RegisterAttached(
            "DropTarget",
            typeof(DropTarget),
            typeof(DragDropManager),
            new PropertyMetadata(new PropertyChangedCallback(OnDropTargetChanged)));

        private static void OnDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // unregister must occurs first
            if (e.OldValue != null)
            {
                DropTarget dropTarget = e.OldValue as DropTarget;

                if (dropTarget != null)
                {
                    if (dropTarget.IsDropTarget && dropTarget.AssociatedObject != null)
                    {
                        UnregisterDropTarget(dropTarget.AssociatedObject);
                    }

                    dropTarget.AssociatedObject = null;
                }
            }

            if (e.NewValue != null)
            {
                DropTarget dropTarget = e.NewValue as DropTarget;
                UIElement associatedObject = d as UIElement;

                if (dropTarget != null)
                {
                    dropTarget.AssociatedObject = associatedObject;

                    if (dropTarget.IsDropTarget && associatedObject != null)
                    {
                        RegisterDropTarget(associatedObject);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="DropTargetProperty"/> attached property from a given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>The value of the <see cref="DropTargetProperty"/> attached property.</returns>
        public static DropTarget GetDropTarget(DependencyObject obj)
        {
            return (DropTarget)obj.GetValue(DropTargetProperty);
        }

        /// <summary>
        /// Sets value for <see cref="DropTargetProperty"/> attached property to a given <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object on which to set <see cref="DropTargetProperty"/> attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetDropTarget(DependencyObject obj, DropTarget value)
        {
            obj.SetValue(DropTargetProperty, value);
        }

        #endregion // DropTarget

        #region HighlightTargetsOnDragStart
        /// <summary>
        /// Gets or sets a value indicating whether drop target elements should be highlighted when
        /// drag source element with appropriate <see cref="DragSource.DragChannels"/> is dragged.
        /// </summary>
        public static bool HighlightTargetsOnDragStart
        {
            get;
            set;
        }

        #endregion // HighlightTargetsOnDragStart

        #endregion // Public Properties

        #region Private Properties

        #region Instance

        /// <summary>
        /// Gets the single instance of DragDropManager class
        /// </summary>
        public static DragDropManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DragDropManager();

                    Stream stream = typeof(DragDropManager).Assembly.GetManifestResourceStream("Infragistics.DragDrop.Templates.xaml");

                    if (stream != null)
                    {




                        ResourceDictionary rd = (ResourceDictionary)XamlReader.Load(stream);

                        _instance._defaultDropNotAllowedCursorTemplate = rd["dropNotAllowedDataTemplate"] as DataTemplate;
                        _instance._defaultMoveCursorTemplate = rd["moveDataTemplate"] as DataTemplate;
                        _instance._defaultCopyCursorTemplate = rd["copyDataTemplate"] as DataTemplate;
                        _instance._defaultDragTemplate = rd["dragDataTemplate"] as DataTemplate;
                    }

                    _instance._currentDropNotAllowedCursorTemplate = _instance._defaultDropNotAllowedCursorTemplate;
                    _instance._currentMoveCursorTemplate = _instance._defaultMoveCursorTemplate;
                    _instance._currentCopyCursorTemplate = _instance._defaultCopyCursorTemplate;
                    _instance._currentCursorTemplate = _instance._defaultMoveCursorTemplate;
                }

                return _instance;
            }
        }

        #endregion Instance

        private Exception Error
        {
            get { return this._error; }

            set
            {
                if (_error == null || value == null)
                {
                    // keep just the very first exception
                    _error = value;
                }
            }
        }

        #endregion // Private Properties

        #region Protected Properties

        #region CursorPopup

        /// <summary>
        /// Gets most top UI element where cursor data templates are applied.
        /// </summary>


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        protected static UIElement CursorPopup
        {
            get
            {
                return Instance._cursorPopup;
            }
        }

        #endregion // CursorPopup

        #endregion // Protected Properties

        #endregion // Class Properties

        #region Methods

        #region Public Methods

        #region EndDrag
        /// <summary>
        /// Ends drag operation if there is drag operation in progress.
        /// </summary>
        /// <param name="dragCancel"> When <paramref name="dragCancel"/> is set ot <b>true</b> drag is canceled.
        /// When it is set to <b>false</b> it is considered that drop is performed.</param>
        public static void EndDrag(bool dragCancel)
        {
            Instance._endDrag = true;
            Instance._mouseMoving = false;

            if (Instance._dragging)
            {




                UIElement dragHostChild = PlatformProxy.GetHostChild(Instance._dragPopup, Instance._rootVisual);


                // detach events of popup
                if (Instance._dragPopup != null)
                {
                    if (dragHostChild != null)
                    {
                        ((FrameworkElement)dragHostChild).SizeChanged -= DragPopupSizeChanged;
                    }
                }

                // close cursor popup
                if (Instance._cursorPopup != null)
                {





                    UIElement cursorHostChild = PlatformProxy.GetHostChild(Instance._cursorPopup, Instance._rootVisual);
                    PlatformProxy.CloseHost(Instance._cursorPopup, Instance._rootVisual, false, true);
                    //Instance._cursorPopup.Hide();

                    // remove cursor popup event handlers
                    FrameworkElement element = cursorHostChild as FrameworkElement;
                    if (element != null)
                    {
                        element.SizeChanged -= CursorPopupSizeChanged;
                    }
                }

                FrameworkElement trackedFrameworkElement = Instance._trackedElement as FrameworkElement;

                if (trackedFrameworkElement != null && Instance._gotCursor)
                {
                    // AS 5/23/11 TFS76472
                    // Added if - do not set the cursor if it was changed after we set it to None.				
                    if (trackedFrameworkElement.Cursor == Cursors.None)
                    {
                        trackedFrameworkElement.Cursor = Instance._originalCursor;
                    }

                    Instance._originalCursor = null;
                }

                // close target marker popup
                if (Instance._targetMarkerPopup != null)
                {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


                    PlatformProxy.CloseHost(Instance._targetMarkerPopup, Instance._rootVisual, true, true);
                    Instance._targetMarkerPopup = null;

                }

                // remove keyboard event handlers


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                Instance._rootVisual.KeyDown -= CursorPopupKeyDown;
                Instance._rootVisual.KeyUp -= CursorPopupKeyUp;
                Instance._trackedElement.MouseMove -= DragDropManager.OnMouseMove;
                Instance._trackedElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(DragDropManager.OnMouseLeftButtonUp));


                // check whether drag is canceled or drop is performed
                if (dragCancel)
                {
                    OnDragCancel();
                }
                else if (Instance._dropTargetElement != null)
                {
                    OnDrop(Instance._mouseEventArgs);
                }

                // raise the final event of event routine
                OnDragEnd();
            }

            // clean up the variables
            Instance._gotCursor = false;
            Instance._dragSourceElement = null;
            Instance._dragSource = null;
            Instance._originalDragSourceElement = null;
            Instance._dropTargetElement = null;
            Instance._dropTarget = null;
            Instance._draggedElement = null;

            Instance._dragDropEventArgs = null;
            Instance._dragSourceDragTemplate = null;

            Instance._mouseEventArgs = null;

            Instance._dragObject = null;
            Instance._snapshotImage = null;

            Instance._mousePosition = new Point();
            Instance._mouseOffset = new Point();
            Instance._initialMousePosition = new Point();

            // return cursors to default values
            Instance._customMoveCursorTemplate = null;
            Instance._customMoveCursorTemplate = Instance._defaultMoveCursorTemplate;

            Instance._customDropNotAllowedCursorTemplate = null;
            Instance._currentDropNotAllowedCursorTemplate = Instance._defaultDropNotAllowedCursorTemplate;

            Instance._customCopyCursorTemplate = null;
            Instance._currentCopyCursorTemplate = Instance._defaultCopyCursorTemplate;

            Instance._currentCursorTemplate = Instance._defaultMoveCursorTemplate;

            if (Instance._dragContentControl != null)
            {
                Instance._dragContentControl.ContentTemplate = null;
                Instance._dragContentControl.Content = null;
                Instance._dragContentControl = null;
            }

            if (Instance._customCursorContentControl != null)
            {
                Instance._customCursorContentControl.ContentTemplate = Instance._currentCursorTemplate;
            }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            if (Instance._dragPopup != null)
            {
                PlatformProxy.CloseHost(Instance._dragPopup, Instance._rootVisual, true, true);

                //Instance._dragPopup.Content = null;
                //Instance._dragPopup.Hide();
            }


            Instance._mouseCaptured = false;
            Instance._waitDragStart = false;
            Instance._dragging = false;

            if (Instance._needReleaseMouse)
            {
                if (Instance._trackedElement != null)
                {
                    Instance._trackedElement.ReleaseMouseCapture();
                }

                Instance._needReleaseMouse = false;
            }

            Instance._trackedElement = null;

            Instance._rootVisual = null;
            Instance._popupHost = null;

            if (Instance.Error != null)
            {
                Exception error = Instance.Error;
                Instance.Error = null;

                throw error;
            }
        }

        #endregion // EndDrag

        #region RefreshDragLayout
        /// <summary>
        /// Ensure that dragged item is on the top of layout.
        /// User can use this method to set dragged item as top most popup if there are
        /// other popups that are created after drag is already initiated.
        /// </summary>
        public static void RefreshDragLayout()
        {


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


            PlatformProxy.RefreshHostsLayout(Instance._rootVisual, Instance._dragPopup, Instance._cursorPopup, Instance._targetMarkerPopup);

        }

        #endregion // RefreshDragLayout

        #region GetDropTargets

        /// <summary>
        /// Gets the drop targets with the specified channels match.
        /// </summary>
        /// <param name="channels">The channels to match with.</param>
        /// <returns>The drop targets where at least one drop channel matches with one of the passed channels.</returns>
        public static IEnumerable<DropTarget> GetDropTargets(IEnumerable<string> channels)
        {
            IList<UIElement> targets = new List<UIElement>();
            for (int i = Instance._dropTargets.Count - 1; i >= 0; i--)
            {
                WeakReference weakReference = Instance._dropTargets[i];
                if (!weakReference.IsAlive)
                {
                    Instance._dropTargets.RemoveAt(i);
                    continue;
                }

                targets.Add((UIElement)weakReference.Target);
            }

            IEnumerable<DropTarget> dropTargets = targets.Select(t => GetDropTarget(t));
            if (channels == null)
            {
                return dropTargets;
            }

            return dropTargets.Where(dt => channels.Intersect(dt.DropChannels).Count() > 0);
        }

        #endregion // GetDropTargets

        #endregion // Public Methods

        #region Private Methods

        #region ApplyCursorTemplate







        private static void ApplyCursorTemplate()
        {
            if (Instance._dropTargetElement != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ApplyCursorTemplate(OperationType.Copy);
                }
                else
                {
                    ApplyCursorTemplate(OperationType.Move);
                }
            }
            else
            {
                ApplyCursorTemplate(OperationType.DropNotAllowed);
            }
        }

        #endregion // ApplyCursorTemplate

        #region ApplyCursorTemplate



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private static void ApplyCursorTemplate(OperationType? operationType)
        {
            if (Instance._cursorPopup != null)
            {
                DataTemplate cursorDataTemplate = null;
                switch (operationType)
                {
                    case OperationType.Move:
                        Instance._currentMoveCursorTemplate =
                            Instance._customMoveCursorTemplate ??
                            Instance._dragSource.MoveCursorTemplate ??
                            Instance._defaultMoveCursorTemplate;

                        cursorDataTemplate = Instance._currentMoveCursorTemplate;
                        break;

                    case OperationType.DropNotAllowed:
                        Instance._currentDropNotAllowedCursorTemplate =
                            Instance._customDropNotAllowedCursorTemplate ??
                            Instance._dragSource.DropNotAllowedCursorTemplate ??
                            Instance._defaultDropNotAllowedCursorTemplate;

                        cursorDataTemplate = Instance._currentDropNotAllowedCursorTemplate;
                        break;

                    case OperationType.Copy:
                        Instance._currentCopyCursorTemplate =
                            Instance._customCopyCursorTemplate ??
                            Instance._dragSource.CopyCursorTemplate ??
                            Instance._defaultCopyCursorTemplate;

                        cursorDataTemplate = Instance._currentCopyCursorTemplate;
                        break;
                }

                if (cursorDataTemplate != null)
                {
                    // do nothing if the same cursor is already set
                    if (cursorDataTemplate.Equals(Instance._currentCursorTemplate))
                    {
                        return;
                    }

                    // save the new cursor as a current cursor and apply it
                    Instance._currentCursorTemplate = cursorDataTemplate;
                    Instance._customCursorContentControl.ContentTemplate = cursorDataTemplate;
                }
            }
        }

        #endregion // ApplyCursorTemplate

        #region ApplyDragTemplate



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private static void ApplyDragTemplate(DataTemplate dataTemplate)
        {
            if (dataTemplate != null && dataTemplate != Instance._dragContentControl.ContentTemplate)
            {
                Instance._dragContentControl.Content = Instance._dragObject;
                Instance._dragContentControl.ContentTemplate = dataTemplate;
            }
        }

        #endregion // ApplyDragTemplate

        #region CatchDragImage



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private static void CatchDragImage(UIElement snapshotElement)
        {
            FrameworkElement snapshotFrameworkElement = snapshotElement as FrameworkElement;

            // set default width and height for UIElements if they are not FrameworkElements
            double width = 50;
            double height = 50;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            if (snapshotFrameworkElement == null)
            {
                return;
            }

            width = Math.Max(snapshotFrameworkElement.DesiredSize.Width, snapshotFrameworkElement.ActualWidth);
            height = Math.Max(snapshotFrameworkElement.DesiredSize.Height, snapshotFrameworkElement.ActualHeight);

            // PP: keep dpis with the same values in order to avoid sizing clipping issues.
            // different dpis produce resizing of bitmap in order to fit dpi for each axis and then clipping it to passed high and width
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);

            Rect arrangeRect = LayoutInformation.GetLayoutSlot(snapshotFrameworkElement);

            // Important - measure it and draw it with its desired size; unless desired size width/height is zero            
            Size desiredFixedSize = new Size(width, height);
            snapshotFrameworkElement.Measure(desiredFixedSize);
            snapshotFrameworkElement.Arrange(new Rect(desiredFixedSize));

            renderTarget.Render(snapshotFrameworkElement);

            WriteableBitmap writeableBitmap = new WriteableBitmap(renderTarget);

            // Important - arrange it back to its layout slot
            snapshotFrameworkElement.Arrange(arrangeRect);


            Instance._snapshotImage = new Image
            {
                Width = width,
                Height = height,
                Source = writeableBitmap
            };

            if (snapshotFrameworkElement.FlowDirection == FlowDirection.RightToLeft)
            {
                Instance._snapshotImage.LayoutTransform = new ScaleTransform(-1, 1);
            }

        }

        #endregion // CatchDragImage

        #region ChannelsMatch



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        private static bool ChannelsMatch(DependencyObject source, DependencyObject target)
        {
            bool result = false;

            DragSource dragSource = GetDragSource(source);
            DropTarget dropTarget = GetDropTarget(target);

            if (dragSource == null || dropTarget == null)
            {
                return false;
            }

            // when channels are not set we assume that channels match
            if (dragSource.DragChannels == null && dropTarget.DropChannels == null)
            {
                return true;
            }

            // when channel collections are empty we assume that channels match
            if (dragSource.DragChannels != null && dragSource.DragChannels.Count == 0 &&
                dropTarget.DropChannels != null && dropTarget.DropChannels.Count == 0)
            {
                return true;
            }

            if (dragSource.DragChannels != null && dropTarget.DropChannels != null)
            {
                foreach (string dropChannel in dropTarget.DropChannels)
                {
                    foreach (string dragChannel in dragSource.DragChannels)
                    {
                        if (dragChannel == dropChannel)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion // ChannelsMatch

        #region FindTargetInCoordinates



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private static UIElement FindTargetInRoot(Point position, UIElement reference)
        {
            UIElement result = null;


#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

            VisualTreeHelper.HitTest(reference, HitTestFilterInvisible, HitTestResultCallback, new PointHitTestParameters(position));
            result = Instance._target;
            Instance._target = null;

            if (result != null)
            {
                IsPopupHosted(result, out Instance._popupHost);
            }

            return result;
        }




        private static bool IsPopupHosted(UIElement instance, out Popup host)


        {
            Popup lookupPopup = null;

            host = null;

            //DependencyObject visualParent = VisualTreeHelper.GetParent(instance);
            //while (visualParent != null)
            //{
            //    instance = visualParent as UIElement;
            //    if (instance != null && Instance._dropTargets.Contains(instance))
            //    {
            //        // we hit another drop target so we break the verification
            //        return false;
            //    }

            //    visualParent = VisualTreeHelper.GetParent(visualParent);
            //}

            FrameworkElement frameworkElement = instance as FrameworkElement;
            if (frameworkElement != null)
            {
                lookupPopup = frameworkElement.Parent as Popup;
            }

            if (lookupPopup != null &&
                lookupPopup != Instance._dragPopup &&
                lookupPopup != Instance._cursorPopup &&
                lookupPopup != Instance._targetMarkerPopup)
            {

                host = lookupPopup;

                return true;
            }

            return false;
        }


        private static HitTestFilterBehavior HitTestFilterInvisible(DependencyObject potentialHitTestTarget)
        {
            bool isVisible = false;
            bool isHitTestVisible = false;

            var uiElement = potentialHitTestTarget as UIElement;
            if (uiElement != null)
            {
                isVisible = uiElement.IsVisible;
                if (isVisible)
                {
                    isHitTestVisible = uiElement.IsHitTestVisible;
                }
            }
            else
            {
                UIElement3D uiElement3D = potentialHitTestTarget as UIElement3D;
                if (uiElement3D != null)
                {
                    isVisible = uiElement3D.IsVisible;
                    if (isVisible)
                    {
                        isHitTestVisible = uiElement3D.IsHitTestVisible;
                    }
                }
            }

            if (isVisible)
            {
                return isHitTestVisible ? HitTestFilterBehavior.Continue : HitTestFilterBehavior.ContinueSkipSelf;
            }

            return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
        }

        private static HitTestResultBehavior HitTestResultCallback(HitTestResult result)
        {
            DependencyObject parent = result.VisualHit;
            while (parent != null)
            {
                UIElement uiElement = parent as UIElement;
                if (uiElement != null)
                {
                    if (Instance._dropTargets.Contains(uiElement))
                    {
                        if (ChannelsMatch(Instance._dragSourceElement, uiElement))
                        {
                            Instance._target = uiElement;
                            return HitTestResultBehavior.Stop;
                        }

                        if (Instance._dragSource.FindDropTargetMode == FindDropTargetMode.TopMostTargetOnly)
                        {
                            return HitTestResultBehavior.Stop;
                        }
                    }
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return HitTestResultBehavior.Continue;
        }


        #endregion // FindTargetInCoordinates

        #region FindTarget

        private static UIElement FindTarget(MouseEventArgs e)
        {
            UIElement target;

            Point position = e.GetPosition(null);
            if (!isThreadIdInitialized)
            {
                currentThreadId = PlatformProxy.GetCurrentThreadId();
                isThreadIdInitialized = true;
            }

            IList<FrameworkElement> enumeratedWindows = new List<FrameworkElement>();
            IList<FrameworkElement> popupRoots = new List<FrameworkElement>();

            PlatformProxy.EnumWindowsProc childCallback = delegate(IntPtr hwnd, IntPtr lParam)
            {
                HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
                if (hwndSource != null && hwndSource.RootVisual != null)
                {
                    Window window = hwndSource.RootVisual as Window;
                    if (window != null)
                    {
                        enumeratedWindows.Add(window);
                    }
                    else
                    {
                        if (hwndSource.RootVisual.GetType().Name == "PopupRoot")
                        {
                            popupRoots.Add((FrameworkElement)hwndSource.RootVisual);
                        }
                    }
                }

                return true;
            };

            PlatformProxy.EnumThreadWindows(currentThreadId, childCallback, IntPtr.Zero);

            // exclude popup root sources
            IList<PresentationSource> sourceList =
                PresentationSource.CurrentSources.Cast<PresentationSource>().
                    Where(ps => ps.RootVisual != null && ps.RootVisual.GetType().Name != "PopupRoot").ToList();

            foreach (PresentationSource rootSource in sourceList)
            {
                // check if it is used inside a xbap application
                if (rootSource.RootVisual != null && rootSource.RootVisual.GetType().Name == "RootBrowserWindow")
                {
                    if (!enumeratedWindows.Contains((FrameworkElement)rootSource.RootVisual))
                    {
                        enumeratedWindows.Add((FrameworkElement)rootSource.RootVisual);
                    }
                }

                // check if it is a win forms hosted
                if (rootSource.RootVisual != null)
                {
                    AdornerDecorator adornerDecorator = rootSource.RootVisual as AdornerDecorator;
                    if (adornerDecorator != null && adornerDecorator.Child.GetType().Name == "AvalonAdapter")
                    {
                        if (!enumeratedWindows.Contains((FrameworkElement)rootSource.RootVisual))
                        {
                            enumeratedWindows.Add((FrameworkElement)rootSource.RootVisual);
                        }
                    }
                }
            }

            foreach (FrameworkElement popupRoot in popupRoots)
            {
                position = e.GetPosition(popupRoot);
                target = FindTargetInRoot(position, popupRoot);
                if (target != null)
                {
                    return target;
                }
            }

            // when for a hit test point there are overlapped drop targets' root elements hosted in WinForms 
            // restrict the drag-drop operations to be avilable to drag source root only         
            bool restrictDragDropToSourceRoot = false;
            AdornerDecorator dragSourceRoot = Instance._trackedElement as AdornerDecorator;
            if (dragSourceRoot == null)
            {
                dragSourceRoot = PlatformProxy.GetRootVisual(Instance._trackedElement) as AdornerDecorator;
            }

            if (dragSourceRoot != null && dragSourceRoot.Child.GetType().Name == "AvalonAdapter")
            {
                int targetRootsCount = 0;
                foreach (FrameworkElement window in enumeratedWindows)
                {
                    // check if we still dragging into the root
                    HitTestResult result = VisualTreeHelper.HitTest(window, position);
                    if (result != null)
                    {
                        targetRootsCount++;
                    }
                }

                if (targetRootsCount > 1)
                {
                    restrictDragDropToSourceRoot = true;
                }
            }

            foreach (FrameworkElement window in enumeratedWindows)
            {
                if (window == Instance._dragPopup ||
                    window == Instance._cursorPopup ||
                    window == Instance._targetMarkerPopup)
                {
                    continue;
                }

                if (restrictDragDropToSourceRoot && window != dragSourceRoot)
                {
                    continue;
                }

                position = e.GetPosition(window);
                target = FindTargetInRoot(position, window);
                if (target != null)
                {
                    return target;
                }

                // check if we still dragging into the root
                HitTestResult result = VisualTreeHelper.HitTest(window, position);
                if (result != null)
                {
                    // if it is so then do nothing
                    return null;
                }
            }

            return null;






        }

        #endregion // FindTargetRoot

        #region GenerateTargetMarker







        private static void GenerateTargetMarker(FrameworkElement target)
        {




            UIElement mainWindow = null;
            if (Instance._popupHost != null)
            {
                mainWindow = Instance._popupHost.Child;
            }
            else if (target != null)
            {
                mainWindow = PlatformProxy.GetRootVisual(target);
            }

            if (mainWindow == null)
                return;

            Instance._targetMarkerPopup = PlatformProxy.GetHost(mainWindow, Instance._rootVisual, null);

            StackPanel marker = new StackPanel();
            Border markerChild = new Border
            {
                Background = Instance._dropTarget.DropTargetMarkerBrush,
                CornerRadius = new CornerRadius(2),
                Opacity = 0.4
            };

            // apply render transform parts to visual maker
            TransformGroup markerTransformGroup = new TransformGroup();
            if (target.RenderTransform != null)
            {
                TransformGroup transformGroup = target.RenderTransform as TransformGroup;
                if (transformGroup != null)
                {
                    foreach (Transform transform in transformGroup.Children)
                    {
                        if (transform is ScaleTransform || transform is RotateTransform)
                        {
                            markerTransformGroup.Children.Add(transform);
                        }
                    }
                }

                if (target.RenderTransform is ScaleTransform || target.RenderTransform is RotateTransform)
                {
                    markerTransformGroup.Children.Add(target.RenderTransform);
                }
            }

            if (markerTransformGroup.Children.Count > 0)
            {
                markerChild.RenderTransform = markerTransformGroup;
            }

            marker.Children.Add(markerChild);

            Point point;


            if (Instance._rootVisual.GetType().Name == "RootBrowserWindow")
            {
                point = target.PointToScreen(new Point(0, 0));
            }
            else
            {
                if (target.IsDescendantOf(mainWindow))
                {
                    GeneralTransform objGeneralTransform = target.TransformToVisual(mainWindow);
                    point = objGeneralTransform.Transform(new Point(0, 0));
                }
                else
                {
                    point = new Point(0, 0);
                }
            }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            double myObjTop = point.Y;
            double myObjLeft = point.X;



            // calculate the global coordinates
            if (target.FlowDirection == FlowDirection.RightToLeft)
            {
                myObjLeft -= target.RenderSize.Width;
            }


            // for XBAP application drag-drop uses a Window as a host which behaves
            // different that the WPF's popup; here we fix the popup left position 
            if (Instance._rootVisual.GetType().Name != "RootBrowserWindow")
            {
                // that's because Popup behavior in WPF when application root float direction is set to RightToLeft
                if (((FrameworkElement)mainWindow).FlowDirection == FlowDirection.RightToLeft ^ SystemParameters.IsMenuDropRightAligned)
                {
                    myObjLeft += target.RenderSize.Width;
                }
            }

            Geometry targetClip = LayoutInformation.GetLayoutClip(target);

            if (targetClip == null)
            {
                markerChild.Width = target.ActualWidth;
                markerChild.Height = target.ActualHeight;
            }
            else
            {
                double height;
                if (targetClip.Bounds.Top > 0)
                {
                    height = targetClip.Bounds.Height;
                    myObjTop += targetClip.Bounds.Top;
                }
                else
                {
                    height = Math.Min(target.RenderSize.Height, targetClip.Bounds.Height);
                }

                double clipOffset = 0;
                if (target.FlowDirection == FlowDirection.RightToLeft)
                {
                    clipOffset = target.RenderSize.Width - targetClip.Bounds.Right;
                }
                else
                {
                    clipOffset = targetClip.Bounds.Left;
                }

                myObjLeft += clipOffset;
                double width = Math.Min(target.RenderSize.Width, targetClip.Bounds.Width);

                markerChild.Width = width;
                markerChild.Height = height;
            }






            PlatformProxy.SetHostPosition(Instance._targetMarkerPopup, Instance._rootVisual, myObjTop, myObjLeft);
            PlatformProxy.SetHostSize(Instance._targetMarkerPopup, Instance._rootVisual, markerChild.Width, markerChild.Height);


            // Rearange popus so target marker is bottom most


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


            //Instance._targetMarkerPopup.Content = marker;
            PlatformProxy.OpenHost(Instance._targetMarkerPopup, Instance._rootVisual, marker);

            //Instance._targetMarkerPopup.Show();

            //Instance._dragPopup.Hide();
            PlatformProxy.CloseHost(Instance._dragPopup, Instance._rootVisual, false, false);
            if (Instance._cursorPopup != null)
            {
                //Instance._cursorPopup.Hide();
                PlatformProxy.CloseHost(Instance._cursorPopup, Instance._rootVisual, false, false);
            }

            //Instance._dragPopup.Show();
            PlatformProxy.OpenHost(Instance._dragPopup, Instance._rootVisual);

            if (Instance._cursorPopup != null)
            {
                //Instance._cursorPopup.Show();
                PlatformProxy.OpenHost(Instance._cursorPopup, Instance._rootVisual);
            }

        }

        #endregion // GenerateTargetMarker

        #region GetElementsInDropTarget







        private static ReadOnlyCollection<UIElement> GetElementsInDropTarget()
        {


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            List<UIElement> result = new List<UIElement>(1) { Instance._dropTargetElement };
            return new ReadOnlyCollection<UIElement>(result);
        }

        #endregion // GetElementsInDropTarget

        #region GetDragDropEventArgs







        private static DragDropEventArgs GetDragDropEventArgs()
        {
            // create the original state once
            if (Instance._dragDropEventArgs == null)
            {
                Instance._dragDropEventArgs = new DragDropEventArgs
                {
                    DragSource = Instance._dragSourceElement,
                    OriginalDragSource = Instance._originalDragSourceElement,
                    OperationType = null
                };

                // we need to keep values set from event handlers
                // when event arguments are generated for the first time 
                // the original source is checked first and then the parent source 
                DragSource dragSource = Instance._dragSource;

                if (dragSource != null)
                {
                    if (dragSource.DragTemplate != null)
                    {
                        Instance._dragDropEventArgs.DragTemplate =
                            dragSource.DragTemplate;
                    }

                    Instance._dragDropEventArgs.CopyCursorTemplate = dragSource.CopyCursorTemplate ??
                                                                     Instance._defaultCopyCursorTemplate;

                    Instance._dragDropEventArgs.MoveCursorTemplate = dragSource.MoveCursorTemplate ??
                                                                     Instance._defaultMoveCursorTemplate;

                    Instance._dragDropEventArgs.DropNotAllowedCursorTemplate = dragSource.DropNotAllowedCursorTemplate ??
                                                                               Instance._defaultDropNotAllowedCursorTemplate;

                    // force the binding re-evluation
                    BindingExpression be = BindingOperations.GetBindingExpression(dragSource, DragSource.DataObjectProperty);
                    if (dragSource.DataObject == null && be != null)
                    {
                        be.UpdateTarget();
                    }



                    Instance._dragDropEventArgs.Data =
                            dragSource.DataObject;




                }
            }

            return Instance._dragDropEventArgs;
        }

        #endregion // GetDragDropEventArgs

        #region ShowDragPopup

        private static void ShowDragPopup()
        {
            DataTemplate draggedElementTemplate =
                Instance._dragSourceDragTemplate ??
                Instance._defaultDragTemplate;

            Instance._dragObject = new DragObject
            {
                DragImage = Instance._snapshotImage,
                Data = Instance._dragDropEventArgs.Data
            };

            if (Instance._dragContentControl == null)
            {
                Instance._dragContentControl = new ContentControl
                {
                    ContentTemplate = draggedElementTemplate,
                    Content = Instance._dragObject
                };
            }
            else
            {
                Instance._dragContentControl.Content = Instance._dragObject;
                Instance._dragContentControl.ContentTemplate = draggedElementTemplate;
            }





            //UIElement popupHostChild = PlatformProxy.GetHostChild(Instance._dragPopup, Instance._rootVisual);

            // this handler is removed in DragDropManager.EndDrag(bool) method
            Instance._dragContentControl.SizeChanged += DragPopupSizeChanged;

            FrameworkElement trackedFrameworkElement = Instance._trackedElement as FrameworkElement;

            if (trackedFrameworkElement != null && !Instance._gotCursor)
            {
                Instance._originalCursor = trackedFrameworkElement.Cursor;
                trackedFrameworkElement.Cursor = Cursors.None;
                Instance._gotCursor = true;
            }




            //Instance._dragPopup.Show();
            PlatformProxy.OpenHost(Instance._dragPopup, Instance._rootVisual, Instance._dragContentControl);

        }

        #endregion // ShowDragPopup

        #region ShowCursorPopup

        private static void ShowCursorPopup()
        {
            // popup is created once and the same object is reused
            // for all drags and drops until the application is runing

            if (Instance._cursorPopup != null)
            {
                PlatformProxy.CloseHost(Instance._cursorPopup, Instance._rootVisual, true, true);
            }

            Grid hostChild = new Grid
                                 {
                                     Background = new SolidColorBrush(Colors.Transparent),
                                     Name = "popupChild"
                                 };


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            Instance._cursorPopup = PlatformProxy.GetHost(Instance._rootVisual, Instance._rootVisual, hostChild);

            Instance._customCursorContentControl = new ContentControl
            {
                Cursor = Cursors.None,
                Content = new object(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ContentTemplate = Instance._currentCursorTemplate
            };

            hostChild.Children.Add(Instance._customCursorContentControl);




            UIElement mainWindow = Instance._rootVisual;

            // set different event handlers for relevant events
            // event handlers are alive just for particular drag-and-drop operation

            // this handler is removed in DragDropManager.EndDrag(bool) method
            hostChild.SizeChanged += CursorPopupSizeChanged;

            // this handler is removed in DragDropManager.EndDrag(bool) method
            mainWindow.KeyDown += CursorPopupKeyDown;

            // this handler is removed in DragDropManager.EndDrag(bool) method
            mainWindow.KeyUp += CursorPopupKeyUp;




            //Instance._cursorPopup.Show();
            PlatformProxy.OpenHost(Instance._cursorPopup, Instance._rootVisual);

        }

        #endregion // ShowCursorPopup

        #region OnDragCancel

        private static void OnDragCancel()
        {
            try
            {
                DragSource dragSource = Instance._dragSource;

                if (dragSource != null)
                {
                    DragDropEventArgs args = GetDragDropEventArgs();

                    dragSource.OnDragCancel(args);
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
        }

        #endregion // OnDragCancel

        #region OnDragEnd

        private static void OnDragEnd()
        {
            try
            {
                if (Instance._highlightedTargets.Count > 0)
                {
                    foreach (DropTarget highlightedTarget in Instance._highlightedTargets.GetTargets<DropTarget>())
                    {
                        highlightedTarget.HighlightDropTarget(false);
                    }

                    Instance._highlightedTargets.Clear();
                }
                else
                {
                    if (Instance._dropTarget != null)
                    {
                        Instance._dropTarget.HighlightDropTarget(false);
                    }
                }

                DragSource ds = Instance._dragSource;

                if (ds != null)
                {
                    DragDropEventArgs args = GetDragDropEventArgs();

                    ds.OnDragEnd(args);

                    Instance._dragDropEventArgs = null;
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
        }

        #endregion // OnDragEnd

        #region OnDragEnter

        private static bool OnDragEnter()
        {
            try
            {
                DragSource ds = Instance._dragSource;

                if (ds != null)
                {
                    DragDropEventArgs baseArgs = GetDragDropEventArgs();
                    DragDropCancelEventArgs args = new DragDropCancelEventArgs(baseArgs)
                    {
                        DropTarget = Instance._dropTargetElement
                    };

                    ds.OnDragEnter(args);

                    if (args.Cancel)
                    {
                        //DragDropManager.EndDrag(true);
                        return false;
                    }

                    ApplyDragTemplate(args.DragTemplate);
                    SetCustomCursorTemplates(args);

                    Instance._dragDropEventArgs = args;
                }

                if (Instance._highlightedTargets.Count == 0 ||
                    !Instance._highlightedTargets.Contains(Instance._dropTarget))
                {
                    Instance._dropTarget.HighlightDropTarget(true);
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }

            return true;
        }

        #endregion // OnDragEnter

        #region OnDragLeave

        private static void OnDragLeave()
        {
            try
            {
                if (Instance._highlightedTargets.Count == 0 ||
                    !Instance._highlightedTargets.Contains(Instance._dropTarget))
                {
                    Instance._dropTarget.HighlightDropTarget(false);
                }

                DragSource ds = Instance._dragSource;

                if (ds != null)
                {
                    DragDropEventArgs args = GetDragDropEventArgs();
                    args.DropTarget = Instance._dropTargetElement;

                    ds.OnDragLeave(args);

                    args.DropTarget = null;

                    ApplyDragTemplate(args.DragTemplate);

                    SetCustomCursorTemplates(args);

                    Instance._dragDropEventArgs = args;
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
        }

        #endregion // OnDragLeave

        #region OnDragOver

        private static void OnDragOver(MouseEventArgs mouseArgs)
        {
            try
            {
                DragSource ds = Instance._dragSource;

                if (ds != null)
                {
                    DragDropEventArgs dragDropEventArgs = GetDragDropEventArgs();
                    DragDropMoveEventArgs args = new DragDropMoveEventArgs(dragDropEventArgs, mouseArgs)
                    {
                        DropTarget = Instance._dropTargetElement
                    };

                    ds.OnDragOver(args);

                    ApplyDragTemplate(args.DragTemplate);

                    SetCustomCursorTemplates(args);

                    Instance._dragDropEventArgs = args;
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
        }

        #endregion // OnDragOver

        #region OnDragStart

        private static bool OnDragStart()
        {
            try
            {
                DragSource ds = Instance._dragSource;

                DragDropEventArgs baseArgs = GetDragDropEventArgs();
                DragDropStartEventArgs args = new DragDropStartEventArgs(baseArgs);

                if (ds != null)
                {
                    ds.OnDragStart(args);

                    if (args.Cancel)
                    {
                        //DragDropManager.EndDrag(true);
                        return false;
                    }

                    // Get snapshot of dragged item
                    Instance._draggedElement = args.DragSnapshotElement ?? args.DragSource;
                    CatchDragImage(Instance._draggedElement);

                    SetCustomCursorTemplates(args);
                }

                Instance._dragSourceDragTemplate =
                    args.DragTemplate ??
                    Instance._dragSource.DragTemplate;

                Instance._dragDropEventArgs = args;

                TryHighlightDropTargets();
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
            return true;
        }

        #endregion // OnDragStart

        #region OnDrop

        private static void OnDrop(MouseEventArgs mouseEventArgs)
        {
            try
            {
                DragSource ds = Instance._dragSource;

                if (ds != null)
                {
                    DragDropEventArgs baseArgs = GetDragDropEventArgs();

                    DropEventArgs args = new DropEventArgs(baseArgs, mouseEventArgs)
                    {
                        DropTarget = Instance._dropTargetElement,
                        DropTargetElements = GetElementsInDropTarget()
                    };

                    ds.OnDrop(args);
                }
            }
            catch (Exception ex)
            {
                Instance.Error = ex;
            }
        }

        #endregion // OnDrop

        private static void TryHighlightDropTargets()
        {
            IEnumerable<DropTarget> dropTargets = GetDropTargets(Instance._dragSource.DragChannels);
            IEnumerable<DropTarget> highlitedTargets =
                dropTargets.Where(dt => dt.HighlightOnDragStart || HighlightTargetsOnDragStart).ToList();

            foreach (DropTarget highlitedTarget in highlitedTargets)
            {
                Instance._highlightedTargets.Add(new WeakReference(highlitedTarget));
            }

            foreach (DropTarget dropTarget in highlitedTargets)
            {
                dropTarget.HighlightDropTarget(true);
            }
        }

        #region QueryContinueDrag

        private static void QueryContinueDrag(DependencyObject sender, bool escapePressed, bool mouseReleased)
        {
            if (sender == null)
            {
                if (escapePressed || mouseReleased)
                {
                    DragDropManager.EndDrag(true);
                }

                return;
            }

            if (escapePressed)
            {
                DragDropManager.EndDrag(true);
            }
            else if (mouseReleased)
            {
                DragDropManager.EndDrag(false);
            }
            else
            {
                DragDropEventArgs args = GetDragDropEventArgs();

                // if operation type is changed by the user then we need to apply the related cursor
                if (args.OperationType != null)
                {
                    ApplyCursorTemplate(args.OperationType);
                }
                else
                {
                    // Apply relevant for the operation cursor template.
                    ApplyCursorTemplate();
                }
            }
        }

        #endregion // QueryContinueDrag

        #region RemoveTargetMarker

        private static void RemoveTargetMarker()
        {
            if (Instance._targetMarkerPopup != null)
            {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


                PlatformProxy.CloseHost(Instance._targetMarkerPopup, Instance._rootVisual, true, true);
                //Instance._targetMarkerPopup.Close();
                Instance._targetMarkerPopup = null;

            }
        }

        #endregion // RemoveTargetMarker

        #region SetCursorPopupPosition

        private static void SetCursorPopupPosition()
        {
            int k = -1;

            int p = 0;
            FrameworkElement trackedElement = (FrameworkElement)Instance._trackedElement;
            FrameworkElement rootElement = (FrameworkElement)Instance._rootVisual;
            bool isRightPositioned = false;
            bool isMenuRightDropRightAligned = SystemParameters.IsMenuDropRightAligned;
            bool isFlowRightToLeft = rootElement.FlowDirection == FlowDirection.RightToLeft;

            if ((isFlowRightToLeft && !isMenuRightDropRightAligned) ||
                (!isFlowRightToLeft && isMenuRightDropRightAligned))
            {
                isRightPositioned = true;
            }

            // for XBAP application drag-drop uses a Window as a host
            //if (Instance._rootVisual.GetType().Name != "RootBrowserWindow")
            UIElement hostChild = PlatformProxy.GetHostChild(Instance._cursorPopup, Instance._rootVisual);

            if (trackedElement.FlowDirection == FlowDirection.RightToLeft)
            {
                k = 1;
                p = -1;
            }

            if (Instance._cursorPopup != null)
            {





                double top = (Instance._mousePosition.Y - (hostChild.DesiredSize.Height / 2));
                double left = (Instance._mousePosition.X + k * (hostChild.DesiredSize.Width / 2) + p * hostChild.DesiredSize.Width);

                if (Instance._rootVisual.GetType().Name != "RootBrowserWindow" && isRightPositioned)
                {
                    left += hostChild.DesiredSize.Width;
                }

                PlatformProxy.SetHostPosition(Instance._cursorPopup, Instance._rootVisual, top, left);

            }
        }

        #endregion // SetCursorPopupPosition

        #region SetCustomCursorTemplate

        private static void SetCustomCursorTemplate(DataTemplate cursor, OperationType cursorType)
        {
            switch (cursorType)
            {
                case OperationType.Move:
                    Instance._customMoveCursorTemplate = cursor;
                    break;

                case OperationType.DropNotAllowed:
                    Instance._customDropNotAllowedCursorTemplate = cursor;
                    break;

                case OperationType.Copy:
                    Instance._customCopyCursorTemplate = cursor;
                    break;
            }
        }

        #endregion // SetCustomCursorTemplate

        #region SetCustomCursorTemplates

        private static void SetCustomCursorTemplates(DragDropEventArgs args)
        {
            SetCustomCursorTemplate(args.CopyCursorTemplate, OperationType.Copy);
            SetCustomCursorTemplate(args.MoveCursorTemplate, OperationType.Move);
            SetCustomCursorTemplate(args.DropNotAllowedCursorTemplate, OperationType.DropNotAllowed);
        }

        #endregion // SetCustomCursorTemplates

        #region SetDragPopupPosition

        private static void SetDragPopupPosition(Point draggedOffset)
        {
            DataTemplate draggedElementTemplate =
                Instance._dragSourceDragTemplate ??
                Instance._defaultDragTemplate;

            int k = -1;
            FrameworkElement trackedElement = (FrameworkElement)Instance._trackedElement;

            int p = 0;
            FrameworkElement rootElement = (FrameworkElement)Instance._rootVisual;
            bool isRightPositioned = false;
            bool isMenuRightDropRightAligned = SystemParameters.IsMenuDropRightAligned;
            bool isFlowRightToLeft = rootElement.FlowDirection == FlowDirection.RightToLeft;

            if ((isFlowRightToLeft && !isMenuRightDropRightAligned) ||
                (!isFlowRightToLeft && isMenuRightDropRightAligned))
            {
                isRightPositioned = true;
            }

            // for XBAP application drag-drop uses a Window as a host
            //if (Instance._rootVisual.GetType().Name != "RootBrowserWindow")
            UIElement hostChild = PlatformProxy.GetHostChild(Instance._dragPopup, Instance._rootVisual);

            if (trackedElement.FlowDirection == FlowDirection.RightToLeft)
            {
                k = 1;
                p = -1;
            }            

            // set cursor to center when:
                // 1. There is applied custom drag data template
                // 2. Dragged element is not the original source and is not the element marked as draggable either 
                if (draggedElementTemplate != Instance._defaultDragTemplate ||
                    (Instance._draggedElement != null && (!Instance._draggedElement.Equals(Instance._originalDragSourceElement) &&
                    !Instance._draggedElement.Equals(Instance._dragSourceElement))))
                {






                    double top = (Instance._mousePosition.Y - (hostChild.DesiredSize.Height / 2));
                    double left = (Instance._mousePosition.X + k * (hostChild.DesiredSize.Width / 2) + p * hostChild.DesiredSize.Width);

                    if (Instance._rootVisual.GetType().Name != "RootBrowserWindow" && isRightPositioned)
                    {
                        left += hostChild.DesiredSize.Width;
                    }

                    PlatformProxy.SetHostPosition(Instance._dragPopup, Instance._rootVisual, top, left);

                }
                else
                {
                    


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


                    double elementOffsetX = k * Instance._mouseOffset.X;
                    double top = (Instance._mousePosition.Y - Instance._mouseOffset.Y + draggedOffset.Y);
                    double left = (Instance._mousePosition.X + elementOffsetX + draggedOffset.X + p * hostChild.DesiredSize.Width);
                    if (Instance._rootVisual.GetType().Name != "RootBrowserWindow" && isRightPositioned)
                    {
                        left += hostChild.DesiredSize.Width;
                    }

                    PlatformProxy.SetHostPosition(Instance._dragPopup, Instance._rootVisual, top, left);

                }
        }

        #endregion // SetDragPopupPosition

        #endregion // Private Methods

        #region Protected Methods

        #region RegisterDragSource







        protected internal static void RegisterDragSource(UIElement element)
        {
            if (element != null)
            {
                if (!Instance._dragSources.Contains(element))
                {
                    Instance._dragSources.Add(element);

                    // these event handlers are removed just when drag source is unregistered
                    element.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
                }
            }
        }

        #endregion // RegisterDragSource

        #region RegisterDropTarget







        protected internal static void RegisterDropTarget(UIElement element)
        {
            if (!Instance._dropTargets.Contains(element))
            {
                Instance._dropTargets.Add(element);
            }
        }

        #endregion // RegisterDropTarget

        #region UnregisterDragSource







        protected internal static void UnregisterDragSource(UIElement element)
        {
            if (element != null)
            {
                if (Instance._dragSources.Contains(element))
                {
                    Instance._dragSources.Remove(element);
                }

                element.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));
            }
        }

        #endregion // UnregisterDragSource

        #region UnregisterDropTarget







        protected internal static void UnregisterDropTarget(UIElement element)
        {
            if (Instance._dropTargets.Contains(element))
            {
                Instance._dropTargets.Remove(element);
            }
        }

        #endregion // UnregisterDropTarget

        #endregion // Protected Methods

        #endregion // Methods

        #region Event Handlers

        #region CursorPopupKeyDown

        private static void CursorPopupKeyDown(object sender, KeyEventArgs e)
        {
            QueryContinueDrag(Instance._dropTargetElement, false, false);
        }

        #endregion // CursorPopupKeyDown

        #region CursorPopupKeyUp

        private static void CursorPopupKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                QueryContinueDrag(Instance._dropTargetElement, true, false);
            }
            else
            {
                QueryContinueDrag(Instance._dropTargetElement, false, false);
            }
        }

        #endregion // CursorPopupKeyUp

        #region CursorPopupSizeChanged

        private static void CursorPopupSizeChanged(object sender, SizeChangedEventArgs e)
        {

            UIElement hostChild = PlatformProxy.GetHostChild(Instance._cursorPopup, Instance._rootVisual);

            PlatformProxy.SetHostSize(Instance._cursorPopup, Instance._rootVisual, hostChild.DesiredSize.Width, hostChild.DesiredSize.Height);
            //Instance._cursorPopup.Width = hostChild.DesiredSize.Width;
            //Instance._cursorPopup.Height = hostChild.DesiredSize.Height;

            SetCursorPopupPosition();
        }

        #endregion // CursorPopupSizeChanged

        #region DragPopupSizeChanged

        private static void DragPopupSizeChanged(object sender, SizeChangedEventArgs e)
        {

            UIElement hostChild = PlatformProxy.GetHostChild(Instance._dragPopup, Instance._rootVisual);
            PlatformProxy.SetHostSize(Instance._dragPopup, Instance._rootVisual, hostChild.DesiredSize.Width, hostChild.DesiredSize.Height);

            //Instance._dragPopup.Width = hostChild.DesiredSize.Width;
            //Instance._dragPopup.Height = hostChild.DesiredSize.Height;

            SetDragPopupPosition(Instance._draggedOffset);
        }

        #endregion // DragPopupSizeChanged

        #region OnMouseLeftButtonDown

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Instance._endDrag = false;

            if (Instance._mouseCaptured)
            {
                return;
            }


            Instance._rootVisual = PlatformProxy.GetRootVisual((DependencyObject)sender);
            if (Instance._rootVisual == null)
            {
                return;
            }

            Point mousePosition = PlatformProxy.GetMousePosition(e, Instance._rootVisual);





            Instance._initialMousePosition.X = mousePosition.X;
            Instance._initialMousePosition.Y = mousePosition.Y;

            Instance._dragSourceElement = (UIElement)sender;
            Instance._originalDragSourceElement = e.OriginalSource as UIElement;
            Instance._dragSource = GetDragSource(Instance._dragSourceElement);

            // PP: Do not relay on e.OriginalSource for mouse tracking
            // we have to be sure about visibility state of trakced element
            UIElement capturedElement = Instance._dragSourceElement;

            // capture the mouse in order to receive all events
            Instance._mouseCaptured = CaptureMouse(capturedElement, true);
            if (!Instance._mouseCaptured)
            {
                capturedElement = GetElementWithMouseCapture();
                if (capturedElement != null)
                {
                    Instance._mouseCaptured = CaptureMouse(capturedElement, false);
                }
            }

            Instance._waitDragStart = Instance._mouseCaptured;
        }

        private static bool CaptureMouse(UIElement element, bool needMouseRelease)
        {

            // mouse is already captured
            if (Mouse.Captured != null)
            {
                return false;
            }

            // capture the mouse in order to receive all events            
            if (element.CaptureMouse())
            {
                // we capture the mouse and we are responsible to release it
                Instance._needReleaseMouse = needMouseRelease;

                Instance._trackedElement = element;

                // this handler is removed when drag ends
                Instance._trackedElement.MouseMove += DragDropManager.OnMouseMove;

                // this handler is removed when drag ends            
                Instance._trackedElement.AddHandler(
                    UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);

                // track when mouse capture is lost
                Instance._trackedElement.LostMouseCapture += LostMouseCapture;

                FrameworkElement trackedElement = Instance._trackedElement as FrameworkElement;
                if (trackedElement != null)
                {
                    trackedElement.Unloaded += TrackedElementUnloaded;
                }

                return true;
            }

            return false;
        }

        private static void TrackedElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (Instance._dragging || Instance._waitDragStart)
            {
                // no matter if the mouse is captured by our or external code
                // we are going to release it so we can find appropriate element to
                // capture it later
                if (Instance._mouseCaptured)
                {
                    Instance._trackedElement.ReleaseMouseCapture();
                }

                ResolveTrackedElement(sender);
            }
        }

        private static void LostMouseCapture(object sender, MouseEventArgs e)
        {
            Instance._needReleaseMouse = false;

            ResolveTrackedElement(sender);
        }

        private static void ResolveTrackedElement(object sender)
        {
            UIElement uiMouseHolder = sender as UIElement;
            if (uiMouseHolder == null)
            {
                return;
            }

            FrameworkElement trackedElement = Instance._trackedElement as FrameworkElement;
            if (trackedElement != null)
            {
                trackedElement.Unloaded -= TrackedElementUnloaded;
            }

            uiMouseHolder.LostMouseCapture -= LostMouseCapture;
            uiMouseHolder.MouseMove -= OnMouseMove;
            uiMouseHolder.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));

            if (!Instance._dragging && !Instance._waitDragStart)
            {
                return;
            }

            // FrameworkElement rootFrameworkElement = Instance._trackedElement as FrameworkElement;
            if (trackedElement != null && Instance._gotCursor)
            {
                // AS 5/23/11 TFS76472
                // Added if - do not set the cursor if it was changed after we set it to None.				
                if (trackedElement.Cursor == Cursors.None)
                {
                    trackedElement.Cursor = Instance._originalCursor;
                }

                Instance._originalCursor = null;
            }

            if (Instance._endDrag)
            {
                return;
            }

            // the original mouse holder loose capture before drag is actually started
            // or while drag is still in progress
            if (Instance._waitDragStart || Instance._dragging)
            {
                if (Instance._trackedElement != null)
                {
                    Instance._trackedElement.MouseMove -= DragDropManager.OnMouseMove;
                    Instance._trackedElement.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp));
                }

                Instance._trackedElement = GetElementWithMouseCapture();
                if (Instance._trackedElement == null)
                {
                    // there is no way to track mouse events
                    EndDrag(true);
                    return;
                }

                Instance._mouseCaptured = Instance._trackedElement.CaptureMouse();



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                // this handler is removed when drag ends
                Instance._trackedElement.MouseMove += OnMouseMove;

                // this handler is removed when drag ends            
                Instance._trackedElement.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);

                Instance._trackedElement.LostMouseCapture += LostMouseCapture;

                if (Instance._dragging)
                {
                    trackedElement = Instance._trackedElement as FrameworkElement;
                    if (trackedElement != null)
                    {
                        Instance._originalCursor = trackedElement.Cursor;
                        trackedElement.Cursor = Cursors.None;

                        Instance._gotCursor = true;
                    }
                }
            }
        }

        private static UIElement GetElementWithMouseCapture()
        {


#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

            if (Mouse.Captured != null)
            {
                UIElement mouseHolder = Mouse.Captured as UIElement;
                if (mouseHolder != null)
                {
                    return mouseHolder;
                }

                return null;
            }

            if (Instance._rootVisual.CaptureMouse())
            {
                Instance._needReleaseMouse = true;
                return Instance._rootVisual;
            }

            return null;

        }

        #endregion // OnMouseLeftButtonDown

        #region OnMouseLeftButtonUp

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Instance._mouseCaptured)
            {
                QueryContinueDrag(Instance._dropTargetElement, false, true);
            }
        }

        #endregion // OnMouseLeftButtonUp

        #region OnMouseMove

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Instance._mouseMoving)
            {
                return;
            }

            Instance._mouseMoving = true;
            if (Instance._mouseCaptured)
            {



                Point mousePosition = PlatformProxy.GetMousePosition(e, Instance._rootVisual);
                //mousePosition = Instance._rootVisual.PointToScreen(mousePosition);


                Instance._mousePosition = mousePosition;
                Instance._mouseEventArgs = e;

                UIElement target = FindTarget(e);

                if (!Instance._dragging)
                {
                    if (Math.Abs(mousePosition.X - Instance._initialMousePosition.X) > Instance._dragSource.DraggingOffset ||
                         Math.Abs(mousePosition.Y - Instance._initialMousePosition.Y) > Instance._dragSource.DraggingOffset)
                    {
                        Instance._dragging = true;
                        Instance._waitDragStart = false;

                        Instance._draggedOffset = new Point(mousePosition.X - Instance._initialMousePosition.X, mousePosition.Y - Instance._initialMousePosition.Y);







                        if (Instance._dragPopup != null)
                        {
                            PlatformProxy.CloseHost(Instance._dragPopup, Instance._rootVisual, true, true);
                        }

                        Instance._dragPopup = PlatformProxy.GetHost(Instance._rootVisual, Instance._rootVisual, new Grid());


                        // DragStart event is cancelable
                        // Also user can call EndDrag(bool) method in DragStart event handler
                        // that will produce that Instance._dragging will be set to false
                        if (!OnDragStart() || Instance.Error != null)
                        {
                            DragDropManager.EndDrag(true);
                            return;
                        }

                        // it is possible the dragged element to be not into the visual tree
                        // or to be not visible (collapsed or with opacity set to zero), or
                        // it might be into a popup that is closed. In this cases exception is
                        // thrown and we cannot get the current mouse offset so we are going
                        // to set position with offset (1, 1)
                        try
                        {
                            Instance._mouseOffset = Instance._mouseEventArgs.GetPosition(Instance._draggedElement);
                        }
                        catch (ArgumentException)
                        {
                            Instance._mouseOffset = new Point(1, 1);
                        }

                        // Initalize and open popup that contains dragged item representation.
                        ShowDragPopup();

                        // Initialize and open popup that contains applied cursor.
                        ShowCursorPopup();
                    }
                }

                if (Instance._dragging)
                {
                    SetDragPopupPosition(Instance._draggedOffset);

                    SetCursorPopupPosition();

                    if (target != null)
                    {
                        bool generateNewTargetMarker = false;

                        if (Instance._dropTargetElement != null)
                        {
                            if (Instance._dropTargetElement != target)
                            {
                                generateNewTargetMarker = true;

                                OnDragLeave();
                                if (Instance.Error != null)
                                {
                                    DragDropManager.EndDrag(true);
                                    return;
                                }

                                Instance._dropTargetElement = target;
                                Instance._dropTarget = GetDropTarget(Instance._dropTargetElement);

                                // DragEnter event is cancelable
                                if (!OnDragEnter() || Instance.Error != null)
                                {
                                    DragDropManager.EndDrag(true);
                                    return;
                                }
                            }
                            else
                            {
                                OnDragOver(e);
                                if (Instance.Error != null)
                                {
                                    DragDropManager.EndDrag(true);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            generateNewTargetMarker = true;
                            Instance._dropTargetElement = target;
                            Instance._dropTarget = GetDropTarget(Instance._dropTargetElement);

                            // DragEnter event is cancelable
                            if (!OnDragEnter() || Instance.Error != null)
                            {
                                DragDropManager.EndDrag(true);
                                return;
                            }
                        }

                        if (generateNewTargetMarker)
                        {
                            RemoveTargetMarker();

                            if (Instance._dropTarget.DropTargetStyle == null)
                            {
                                GenerateTargetMarker((FrameworkElement)Instance._dropTargetElement);
                            }
                        }
                    }
                    else
                    {
                        RemoveTargetMarker();

                        if (Instance._dropTargetElement != null)
                        {
                            OnDragLeave();
                            if (Instance.Error != null)
                            {
                                DragDropManager.EndDrag(true);
                                return;
                            }

                            Instance._dropTargetElement = null;
                        }
                    }

                    DragDropEventArgs args = GetDragDropEventArgs();

                    // if operation type is changed by the user then we need to apply the related cursor
                    if (args.OperationType != null)
                    {
                        ApplyCursorTemplate(args.OperationType);
                    }
                    else
                    {
                        // Apply relevant for the operation cursor template.
                        ApplyCursorTemplate();
                    }
                }
            }

            Instance._mouseMoving = false;
        }

        #endregion // OnMouseMove

        #endregion // Event Handlers
    }

    #endregion // DragDropManager class



    internal static class PlatformProxy
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("kernel32")]
        public static extern int GetCurrentThreadId();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

        public static UIElement GetRootVisual(DependencyObject dependencyObject)
        {
            UIElement rootPanel = Window.GetWindow(dependencyObject);
            if (rootPanel == null)
            {
                DependencyObject visualParent = VisualTreeHelper.GetParent(dependencyObject);

                while (visualParent != null)
                {
                    rootPanel = visualParent as UIElement;
                    visualParent = VisualTreeHelper.GetParent(visualParent);
                }
            }

            return rootPanel;
        }

        public static UIElement GetHostChild(UIElement host, UIElement rootSource)
        {
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                return ((Window)host).Content as UIElement;
            }

            return ((Popup)host).Child;

        }

        public static UIElement GetHost(UIElement hostRoot, UIElement appRoot, UIElement hostChild)
        {
            if (appRoot != null && appRoot.GetType().Name == "RootBrowserWindow")
            {
                Window window = new Window();

                window.IsHitTestVisible = false;
                window.WindowStyle = WindowStyle.None;
                window.ShowInTaskbar = false;
                window.AllowsTransparency = true;
                window.Topmost = true;
                window.Background = new SolidColorBrush(Colors.Transparent);
                window.Content = hostChild;

                return window;
            }

            Popup popup = new Popup
            {
                AllowsTransparency = true,
                Placement = PlacementMode.Relative,
                PlacementTarget = hostRoot,
                Child = hostChild
            };

            return popup;
        }

        public static void OpenHost(UIElement host, UIElement rootSource, UIElement child = null)
        {
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                Window wnd = (Window)host;
                if (child != null)
                {
                    wnd.Content = child;
                }

                wnd.Show();
                return;
            }

            Popup popup = (Popup)host;
            if (child != null)
            {
                popup.Child = child;
            }

            popup.IsOpen = true;
        }

        public static void CloseHost(UIElement host, UIElement rootSource, bool clearChild, bool close)
        {
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                Window wnd = (Window)host;
                if (!close)
                {
                    wnd.Hide();
                }
                else
                {
                    wnd.Close();
                }

                if (clearChild)
                {
                    wnd.Content = null;
                }

                return;
            }

            Popup popup = (Popup)host;
            if (popup.IsOpen)
            {
                popup.IsOpen = false;
            }

            if (clearChild)
            {
                popup.Child = null;
            }
        }

        public static void SetHostPosition(UIElement host, UIElement rootSource, double top, double left)
        {
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                Window wnd = (Window)host;
                wnd.Top = top;
                if (wnd.FlowDirection == FlowDirection.RightToLeft)
                {
                    left -= wnd.Width;
                }

                wnd.Left = left;
                return;
            }

            Popup popup = (Popup)host;
            popup.HorizontalOffset = left;
            popup.VerticalOffset = top;
        }

        public static void SetHostSize(UIElement host, UIElement rootSource, double width, double height)
        {
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                Window wnd = (Window)host;
                wnd.Width = width;
                wnd.Height = height;
            }
        }

        public static Point GetMousePosition(MouseEventArgs e, UIElement rootSource)
        {
            Point mousePosition = e.GetPosition(rootSource);
            if (rootSource != null && rootSource.GetType().Name == "RootBrowserWindow")
            {
                mousePosition = rootSource.PointToScreen(mousePosition);
            }

            return mousePosition;
        }

        public static void RefreshHostsLayout(UIElement rootSource, UIElement dragHost, UIElement cursorHost, UIElement targetMarkerHost)
        {
            if (targetMarkerHost != null)
            {
                PlatformProxy.CloseHost(targetMarkerHost, rootSource, false, false);
                PlatformProxy.OpenHost(targetMarkerHost, rootSource);
            }

            if (dragHost != null)
            {
                PlatformProxy.CloseHost(dragHost, rootSource, false, false);
                PlatformProxy.OpenHost(dragHost, rootSource);
            }

            if (cursorHost != null)
            {
                PlatformProxy.CloseHost(cursorHost, rootSource, false, false);
                PlatformProxy.OpenHost(cursorHost, rootSource);
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