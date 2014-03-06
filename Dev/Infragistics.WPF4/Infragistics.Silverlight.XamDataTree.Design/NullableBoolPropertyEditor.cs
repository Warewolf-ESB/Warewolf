


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;



namespace InfragisticsWPF4.Controls.Menus.XamDataTree.Design



{    
    public partial class NullableBoolPropertyEditor : PropertyValueEditor
    {
        #region Members

        private CheckBox _owner;        

        #endregion // Members

        #region Constructors

        public NullableBoolPropertyEditor(DataTemplate inlineEditorTemplate)
            : base(inlineEditorTemplate)
        {

        }

        public NullableBoolPropertyEditor()
        {
            FrameworkElementFactory checkBox = new FrameworkElementFactory(typeof(CheckBox));
            checkBox.AddHandler(CheckBox.LoadedEvent, new RoutedEventHandler(CheckBox_Loaded));
            checkBox.SetValue(CheckBox.IsThreeStateProperty, true);
            DataTemplate dt = new DataTemplate();
            dt.VisualTree = checkBox;
            InlineEditorTemplate = dt;
        }

        #endregion // Constructors

        #region EventHandlers

        void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            this._owner = (CheckBox)sender;

            PropertyValue pv = this._owner.DataContext as PropertyValue;
            if (pv != null)
            {
                PropertyEntry pe = pv.ParentProperty;
                if (pe != null)
                {
                    this._owner.IsChecked = (bool?)pv.Value;
                }
                this._owner.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(CheckBox_Checked));
                this._owner.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(CheckBox_Checked));
                this._owner.AddHandler(CheckBox.IndeterminateEvent, new RoutedEventHandler(CheckBox_Checked));
            }
        }

        void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PropertyValue pv = _owner.DataContext as PropertyValue;
            if (pv != null)
            {
                bool? newValue = (bool?)this._owner.IsChecked;
                if (newValue == null)
                    pv.Value = null;
                else
                    pv.Value = newValue.ToString();
            }
        }

        #endregion // EventHandlers
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