using System;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Menus.Primitives;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Provides the system implementation for displaying a <see cref="XamContextMenu"/>.
    /// </summary>
    /// <seealso cref="XamContextMenu"/>
    /// <seealso cref="ContextMenuService"/>
    public class ContextMenuManager : DependencyObject
    {
        #region Constants

        private const OpenMode OpenModeDefaultValue = OpenMode.RightClick;
        private const ModifierKeys ModifierKeysDefaultValue = ModifierKeys.None;

        #endregion //Constants

        #region Members

        private WeakReference _targetElement; // a XamContextMenu is attached to this element

        #endregion //Members

        #region Properties

        #region Public Properties

        #region ContextMenu
        /// <summary>
        /// Identifies the ContextMenu dependency property.
        /// </summary>
        public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.Register("ContextMenu",
            typeof(XamContextMenu), typeof(ContextMenuManager), new PropertyMetadata(null, OnContextMenuChanged));

        private static void OnContextMenuChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContextMenuManager cmManager = target as ContextMenuManager;
            if (cmManager != null)
                cmManager.PrepareContextMenu(cmManager.ParentElement, e.OldValue as XamContextMenu, e.NewValue as XamContextMenu);
        }

        /// <summary>
        /// Gets/sets the value of the ContextMenu property.
        /// </summary>
        /// <seealso cref="ContextMenuProperty"/>
        /// <seealso cref="XamContextMenu"/>
        public XamContextMenu ContextMenu
        {
            get
            {
                return (XamContextMenu)this.GetValue(ContextMenuProperty);
            }

            set
            {
                this.SetValue(ContextMenuProperty, value);
            }
        }

        #endregion //ContextMenu

        #region ModifierKeys

        /// <summary>
        /// Identifies the <see cref="ModifierKeys"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register("ModifierKeys",
            typeof(ModifierKeys), typeof(ContextMenuManager), new PropertyMetadata(ModifierKeysDefaultValue));

        /// <summary>
        /// Gets/sets the ModifierKeys of ContextMenuManager.
        /// </summary>
        /// <remarks>The <see cref="XamContextMenu"/> opens with a combination of mouse click and modifier key such as Control key.</remarks>
        /// <seealso cref="ModifierKeysProperty"/>
        /// <seealso cref="OpenMode"/>
        public ModifierKeys ModifierKeys
        {
            get
            {
                return (ModifierKeys)this.GetValue(ModifierKeysProperty);
            }

            set
            {
                this.SetValue(ModifierKeysProperty, value);
            }
        }

        #endregion //ModifierKeys

        #region OpenMode

        /// <summary>
        /// Identifies the <see cref="OpenMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OpenModeProperty = DependencyProperty.Register("OpenMode",
                typeof(OpenMode), typeof(ContextMenuManager),
                new PropertyMetadata(OpenModeDefaultValue, new PropertyChangedCallback(OnOpenModeChanged)));

        private static void OnOpenModeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContextMenuManager manager = target as ContextMenuManager;

            if (manager != null)
            {
                manager.PrepareOpenMode(manager.ParentElement, (OpenMode)e.OldValue, (OpenMode)e.NewValue);
            }
        }

        /// <summary>
        /// Gets/sets the OpenMode of ContextMenuManager.
        /// </summary>
        /// <remarks>The <see cref="XamContextMenu"/> opens with a combination of mouse click and modifier key such as Control key.</remarks>
        /// <seealso cref="OpenModeProperty"/>
        /// <seealso cref="ModifierKeys"/>
        public OpenMode OpenMode
        {
            get
            {
                return (OpenMode)this.GetValue(OpenModeProperty);
            }

            set
            {
                this.SetValue(OpenModeProperty, value);
            }
        }

        #endregion //OpenMode

        #endregion //Public Properties

        #region Internal Properites

        #region ParentElement






        internal UIElement ParentElement
        {
            get
            {
                if (this._targetElement != null && this._targetElement.IsAlive)
                {
                    return this._targetElement.Target as UIElement;
                }

                return null;
            }

            set
            {
                this.OnParentElementChanging(this.ParentElement, value);
                this._targetElement = new WeakReference(value);
            }
        }

        #endregion //ParentElement

        #endregion //Internal Properites

        #endregion //Properties

        #region Methods

        #region Private Methods

        private void OnParentElementChanging(UIElement oldElement, UIElement newElement)
        {
            if (newElement == null)
            {
                this.PrepareOpenMode(oldElement, this.OpenMode, OpenMode.None); // remove old handlers
            }

            if (this.ContextMenu != null)
            {
                


                this.ContextMenu.CloseCurrentOpen();
                this.ContextMenu.ParentElement = newElement;
            }

            this.PrepareOpenMode(newElement, OpenMode.None, this.OpenMode);
        }

        private static bool OpenContextMenu(object sender, Point p)
        {
            DependencyObject dpo = sender as DependencyObject;
            XamContextMenu cm = null;
            if (dpo != null)
            {
                ContextMenuManager manager = ContextMenuService.GetManager(dpo);
                cm = manager.ContextMenu;
                if (cm != null && cm.Visibility == Visibility.Visible)
                {
                    ContextMenuService.PositionContextMenu(cm, p, sender as UIElement);
                }
            }

            return (cm != null) && (cm.IsOpen || cm.IsCancelled);
        }

        private void PrepareOpenMode(UIElement element, OpenMode oldOpenMode, OpenMode newOpenMode)
        {
            if (element != null)
            {
                switch (oldOpenMode)
                {
                    case OpenMode.RightClick:
                        this.RemoveRightMouseButtonUpHandler(element);
                        break;
                    case OpenMode.LeftClick:
                        this.RemoveLeftMouseButtonUpHandler(element);
                        break;
                }

                // add new eventhandlers
                switch (newOpenMode)
                {
                    case OpenMode.RightClick:
                        this.AddRightMouseButtonUpHandler(element);
                        break;
                    case OpenMode.LeftClick:
                        this.AddLeftMouseButtonUpHandler(element);
                        break;
                }
            }
        }

        private void PrepareContextMenu(UIElement element, XamContextMenu oldContextMenu, XamContextMenu newContextMenu)
        {
            if (element != null)
            {
                if (oldContextMenu != null)
                {
                    this.PrepareOpenMode(oldContextMenu.ParentElement, this.OpenMode, OpenMode.None); // this removes the old mouseeventhandler 
                }

                if (newContextMenu != null)
                {
                    this.PrepareOpenMode(element, OpenMode.None, this.OpenMode); // this adds the new mouseeventhandler 
                }

                if (newContextMenu != null)
                {
                    newContextMenu.ParentElement = element;
                }
            }

            CommandSourceManager.NotifyCanExecuteChanged(typeof(OpenCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CloseCommand));
        }

        private void AddRightMouseButtonUpHandler(UIElement element)
        {
            element.MouseRightButtonDown += new MouseButtonEventHandler(this.Element_MouseRightButtonDown);
            element.MouseRightButtonUp += new MouseButtonEventHandler(this.Element_MouseRightButtonUp);
        }

        private void AddLeftMouseButtonUpHandler(UIElement element)
        {
            element.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Element_MouseLeftButtonUp), true);
        }

        private void RemoveRightMouseButtonUpHandler(UIElement element)
        {
            element.MouseRightButtonDown -= this.Element_MouseRightButtonDown;
            element.MouseRightButtonUp -= this.Element_MouseRightButtonUp;
        }

        private void RemoveLeftMouseButtonUpHandler(UIElement element)
        {
            element.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.Element_MouseLeftButtonUp));
        }

        private bool CheckModifierKeys()
        {
            return (this.ModifierKeys == ModifierKeys.None) ||
                ((Keyboard.Modifiers & ModifierKeys) == ModifierKeys);
        }

        #endregion //Private Methods

        #endregion //Methods

        #region Eventhandlers

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.CheckModifierKeys())
            {
                return;
            }

            bool originalHasContextMenu = false;
            DependencyObject source = e.OriginalSource as DependencyObject;

            if (source != null)
            {
                originalHasContextMenu = ContextMenuService.GetManager(source) != null;
            }

            if (sender == e.OriginalSource || !e.Handled || !originalHasContextMenu)
            {
                e.Handled = OpenContextMenu(sender, e.GetPosition(PlatformProxy.GetRootVisual(source)));
            }
        }

        private void Element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.CheckModifierKeys())
            {
                return;
            }

            // try to close the current context menu first. 
            // keep only one context menu opened.
            if (ContextMenuService.CloseActiveContextMenu())
            {
                e.Handled = this.ContextMenu.ShouldRightClickBeHandled;

                // From what i can tell, we shouldn't have to set e.Handled = true, in doing so we actually stop anything else from listening.
                // I did not roll the changes back, as i'm unsure of any side effects this may cause. (Change made for 11.1 +)
                OpenContextMenu(sender, e.GetPosition(PlatformProxy.GetRootVisual(sender as DependencyObject)));
            }
            else
            {
                // this prevents default SL context menu
                e.Handled = true;
            }
        }

        private void Element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // The MouseRightButtonUp event will only be raised if a caller marks the preceding MouseRightButtonDown event as handled 
            e.Handled = true;
        }

        #endregion //Eventhandlers
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