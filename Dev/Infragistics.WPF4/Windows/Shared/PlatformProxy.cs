using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Infragistics
{
    internal static class PlatformProxy
    {
        #region GetFocusedElement
        /// <summary>
        /// Gets the Focused element
        /// </summary>
        /// <param name="scopedElement">
        /// The paramter is specific to wpf, where you would pass the element who you should be scoped to.
        /// </param>
        /// <returns></returns>
        internal static object GetFocusedElement(DependencyObject scopedElement)
        {


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


            DependencyObject owner = FocusManager.GetFocusScope(scopedElement);
            if (owner != null)
            {
                return FocusManager.GetFocusedElement(owner);
            }

            return null;


        }
        #endregion // GetFocusedElement

        #region GetRooVisual
        /// <summary>
        /// The primary use case of this method is to replace using Application.Current.RootVisual in SL.
        /// </summary>
        /// <param name="scopedElement"></param>
        /// <returns></returns>
        internal static UIElement GetRootVisual(DependencyObject scopedElement)
        {


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


            if (scopedElement == null)
            {
                return Application.Current.MainWindow;
            }

            UIElement uiElement = scopedElement as UIElement;
            DependencyObject element = scopedElement;

            while (element is ContentElement)
            {
                element = LogicalTreeHelper.GetParent(element);
            }

            while (element != null)
            {
                if (element is Visual || element is Visual3D)
                {
                    element = VisualTreeHelper.GetParent(element);
                }

                if (element is UIElement)
                {
                    uiElement = (UIElement)element;
                }
            }

            return uiElement;

        }
        #endregion // GetRooVisual

        #region GetParent

        private static DependencyObject GetVisualParent(DependencyObject child)
        {
            Visual visual = child as Visual;

            if (visual != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(visual);

                if (parent != null)
                    return parent;

                FrameworkElement fe = child as FrameworkElement;

                if (fe != null)
                    return fe.Parent;

                return null;
            }

            FrameworkContentElement fce = child as FrameworkContentElement;

            // since FrameworkContentElements are not Visuals calling VisualTreeHelper.GetParent will
            // cause an exception so we just return the logical parent in this case
            if (fce != null)
                return fce.Parent;

            Visual3D visual3D = child as Visual3D;

            if (visual3D != null)
                return VisualTreeHelper.GetParent(visual3D);

            return null;

        }

        #endregion // GetParent

        #region GetZoomFactor

        internal static double GetZoomFactor()
        {



            return 1;

        }

        #endregion // GetZoomFactor

        #region ResolveContainerSize

        internal static Size ResolveContainerSize()
        {



            // In WPF it doesn't matter what size the parent is for a drop down, since the WPF framework handles it
            return new Size();

        }
        #endregion // ResolveContainerSize

        #region GetElementsFromPoint
        internal static IEnumerable<UIElement> GetElementsFromPoint(Point point, UIElement subtree)
        {



            HitTestHelper hth = new HitTestHelper();
            VisualTreeHelper.HitTest(subtree, new HitTestFilterCallback(hth.FilterCallback), new HitTestResultCallback(hth.ResultCallback), new PointHitTestParameters(point));
            return hth.HitResults;

        }


        internal class HitTestHelper
        {
            List<UIElement> _hitElements = new List<UIElement>();

            public HitTestFilterBehavior FilterCallback(DependencyObject target)
            {
                UIElement elem = target as UIElement;
                if(elem != null)
                    this._hitElements.Add(elem);

                return HitTestFilterBehavior.Continue;
            }

            public HitTestResultBehavior ResultCallback(HitTestResult result)
            {
                return HitTestResultBehavior.Continue;
            }

            public List<UIElement> HitResults
            {
                get{return this._hitElements;}
            }


        }


        #endregion // GetElementsFromPoint

        #region IsVersionSupported
        internal static bool IsVersionSupported(string version)
        {



            return true;

        }
        #endregion // IsVersionSupported

        #region GetRootParent

        internal static DependencyObject GetRootParent(DependencyObject element)
        {



            DependencyObject root = null;
            DependencyObject parent = GetVisualParent(element);

            while (parent != null)
            {
                root = parent;
                parent = GetVisualParent(parent);
            }

            return root;

        }

        #endregion //GetRootParent

        #region TabNavigation

        public static KeyboardNavigationMode GetTabNavigation(System.Windows.Controls.Control element)
        {

            return KeyboardNavigation.GetTabNavigation(element);



        }

        public static void SetTabNavigation(System.Windows.Controls.Control element, KeyboardNavigationMode mode)
        {

            KeyboardNavigation.SetTabNavigation(element, mode);



        }

        #endregion TabNavigation

        #region GetParent

        public static DependencyObject GetParent(DependencyObject child)
        {
            return GetParent(child, true);
        }

        public static DependencyObject GetParent(DependencyObject child, bool walkDirectParents)
        {

            Visual v = child as Visual;

            if (v == null)
            {
                return LogicalTreeHelper.GetParent(child);
            }
            else
            {
                DependencyObject parent = VisualTreeHelper.GetParent(child);

                if (parent != null)
                {
                    return parent;
                }

                if (walkDirectParents)
                {
                    FrameworkElement fe = child as FrameworkElement;

                    if (fe != null && fe.Parent != null)
                    {
                        return fe.Parent;
                    }
                }

                return null;
            }



        }

        #endregion GetParent


        internal static System.Windows.Controls.Primitives.Popup GetPopupRootVisual(DependencyObject scopedElement)
        {
            DependencyObject child = scopedElement;

            DependencyObject parent;

            Type type = typeof(System.Windows.Controls.Primitives.Popup);

            while (child != null)
            {
                parent = GetVisualParent(child);

                if (parent != null)
                {
                    Type parentType = parent.GetType();

                    if (parentType == type)
                        return parent as System.Windows.Controls.Primitives.Popup;

                    if (type.IsAssignableFrom(parentType))
                        return parent as System.Windows.Controls.Primitives.Popup;
                }

                child = parent;
            }

            return null;
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