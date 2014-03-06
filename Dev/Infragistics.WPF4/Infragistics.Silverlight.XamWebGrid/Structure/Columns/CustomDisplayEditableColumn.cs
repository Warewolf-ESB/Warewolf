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

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// The base class for all column object of the <see cref="XamGrid"/> templated to the control that will be used for editing and have the ability to change the editor control display.
    /// </summary>
    public abstract class CustomDisplayEditableColumn : EditableColumn
    {
        #region Properties

        #region EditorDisplayBehavior

        /// <summary>
        /// Identifies the <see cref="EditorDisplayBehavior"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EditorDisplayBehaviorProperty = DependencyProperty.Register("EditorDisplayBehavior", typeof(EditorDisplayBehaviors), typeof(CustomDisplayEditableColumn), new PropertyMetadata(EditorDisplayBehaviors.Default, new PropertyChangedCallback(EditorDisplayBehaviorChanged)));

        /// <summary>
        /// Gets / sets the value which will be set on the <see cref="Infragistics.Controls.Grids.EditableColumn"/>.EditorDisplayBehavior.
        /// </summary>
        public EditorDisplayBehaviors EditorDisplayBehavior
        {
            get { return (EditorDisplayBehaviors)this.GetValue(EditorDisplayBehaviorProperty); }
            set { this.SetValue(EditorDisplayBehaviorProperty, value); }
        }

        private static void EditorDisplayBehaviorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CustomDisplayEditableColumn ctrl = (CustomDisplayEditableColumn)obj;

            ctrl.OnPropertyChanged("EditorDisplayBehavior");
            ctrl.RedrawGrid();
        }

        #endregion 

        #endregion //Properties

        #region Methods

        #region RedrawGrid

        private void RedrawGrid()
        {
            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.IsLoaded)
                this.ColumnLayout.Grid.ResetPanelRows(true);
        }

        #endregion // RedrawGrid

        #endregion // Methods

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