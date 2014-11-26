
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2;
using Dev2.Activities.Designers2.Core.Controls;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Converters;
using Dev2.Interfaces;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Util.ExtensionMethods;
using Dev2.Utilities;
using Dev2.ViewModels.QuickVariableInput;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfBaseConvertActivityDesigner : IDisposable
    {

        #region Fields

        ModelItem activity;
        dynamic _convertCollection;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        Selection _workflowDesignerSelection;

        #endregion

        #region Ctor

        public DsfBaseConvertActivityDesigner()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public QuickVariableInputViewModel ViewModel { get; set; }

        public bool ShowAdorners
        {
            get { return (bool)GetValue(ShowAdornersProperty); }
            set { SetValue(ShowAdornersProperty, value); }
        }

        #endregion

        #region Dependancy Properties

        public bool IsAdornerOpen
        {
            get { return (bool)GetValue(IsAdornerOpenProperty); }
            set
            {
                SetValue(IsAdornerOpenProperty, value);
            }
        }

        public static readonly DependencyProperty IsAdornerOpenProperty =
            DependencyProperty.Register("IsAdornerOpen", typeof(bool), typeof(DsfBaseConvertActivityDesigner),
            new PropertyMetadata(false));


        // Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfBaseConvertActivityDesigner), new PropertyMetadata(false));



        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfBaseConvertActivityDesigner), new PropertyMetadata(false));

        public bool ShowRightClickOptions
        {
            get { return (bool)GetValue(ShowRightClickOptionsProperty); }
            set { SetValue(ShowRightClickOptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRightClickOptionsProperty =
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfBaseConvertActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            Context.Items.Subscribe<Selection>(SelectionChanged);

            _convertCollection = newItem;
            activity = newItem as ModelItem;

            if(_convertCollection.ConvertCollection == null || _convertCollection.ConvertCollection.Count <= 0)
            {
                _convertCollection.ConvertCollection.Add(new BaseConvertTO("", "Text", "Base 64", "", 1));
                _convertCollection.ConvertCollection.Add(new BaseConvertTO("", "Text", "Base 64", "", 2));
            }
            activity.Properties["DisplayName"].SetValue(createDisplayName());

            ModelItem parent = activity.Parent;

            while(parent != null)
            {
                if(parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            ICollectionActivity modelItemActivity = ModelItem.GetCurrentValue() as ICollectionActivity;

            QuickVariableInputModel model = new QuickVariableInputModel(ModelItem, modelItemActivity);

            ViewModel = new QuickVariableInputViewModel(model);
        }

        #region Dispose

        public void Dispose()
        {
            CleanUp();
        }

        #endregion

        #region Private Methods

        string createDisplayName()
        {
            string currentName = activity.Properties["DisplayName"].ComputedValue as string;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                if(currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (_convertCollection.ConvertCollection.Count - 1) + ")";
            return currentName;
        }

        #endregion

        #region Event Handlers

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfBaseConvertActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            List<BaseConvertTO> collection = ModelItem.Properties["ConvertCollection"].ComputedValue as List<BaseConvertTO>;
            if(collection != null)
            {
                int result = -1;
                BaseConvertTO lastItem = collection.LastOrDefault(c => c.FromExpression != string.Empty);
                if(lastItem != null)
                {
                    result = collection.IndexOf(lastItem) + 2;

                    if(result > -1)
                    {
                        while(collection.Count > result)
                        {
                            Resultsdg.RemoveRow(collection.Count - 1);
                        }
                    }
                }
            }

            Resultsdg.AddRow();
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
        }

        void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if(cbx != null)
            {
                if(cbx.Items.Count == 0)
                {
                    cbx.ItemsSource = Dev2EnumConverter.ConvertEnumsTypeToStringList<enDev2BaseConvertType>();
                }
            }
        }

        void DeleteRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
        }

        void InsertRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.InsertRow(Resultsdg.SelectedIndex);
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
        }

        void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if(e.Source.GetType() == typeof(Dev2DataGrid))
            {
                ShowRightClickOptions = true;
            }
            else
            {
                ShowRightClickOptions = false;
            }
        }

        void QuickVariableInputControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CloseAdornersRequested += delegate
            {
                ShowQuickVariableInput = false;
            };
        }

        void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

            if(_workflowDesignerSelection != null && _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
            {
                Selection.SelectOnly(Context, ModelItem);
            }

            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
            e.Handled = true;
        }

        void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            inputElement.ReleaseMouseCapture();
            Focus();
            BringToFront();
        }

        void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            Point tempPoint = e.GetPosition(sender as IInputElement);
            double xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
            double yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

            if(e.LeftButton == MouseButtonState.Pressed && _startManualDrag && Math.Max(xDelta, yDelta) >= 5)
            {

#pragma warning disable 618
                DragDropHelper.DoDragMove(this, e.GetPosition(this));
#pragma warning restore 618
                _startManualDrag = false;
                inputElement.ReleaseMouseCapture();
                Focus();
                BringToFront();
            }
        }

        void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
        }

        void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _startManualDrag = false;
        }

        void ShowAllAdorners()
        {
            //BringToFront();
            ShowAdorners = true;
        }

        private void BringToFront()
        {
            try
            {
                var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
                if(fElement != null)
                {
                    fElement.BringToFront();
                }
            }
            catch
            {
            }
        }

        void HideAdorners(bool forceHide = false)
        {
            if((!IsAdornerOpen || forceHide) && !IsSelected)
            {
                UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
                if(uiElement != null)
                {
                    Panel.SetZIndex(uiElement, int.MinValue);
                }

                ShowAdorners = false;
                IsAdornerOpen = false;
            }
        }

        void DsfBaseConvertActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                HideAdorners(true);
            }
            else
            {
                ShowAllAdorners();
            }
        }

        void SelectionChanged(Selection item)
        {
            _workflowDesignerSelection = item;

            if(_workflowDesignerSelection != null)
            {
                if(_workflowDesignerSelection.PrimarySelection == ModelItem)
                {
                    IsSelected = true;
                    BringToFront();
                    ShowAllAdorners();
                }
                else
                {
                    IsSelected = false;
                    HideAdorners();
                }
            }
        }

        protected bool IsSelected { get; set; }

        #endregion

        #region Clean Up

        void CleanUp()
        {
            Context.Items.Unsubscribe<Selection>(SelectionChanged);
        }

        #endregion

        private void DsfBaseConvertActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            HideAdorners();
        }

        private void DsfBaseConvertActivityDesigner_OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            HideAdorners(true);
        }
    }
}



