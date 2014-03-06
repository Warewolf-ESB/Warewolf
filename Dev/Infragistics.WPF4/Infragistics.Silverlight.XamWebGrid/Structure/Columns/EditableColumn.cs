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
using System.Windows.Data;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for all standard column object of the <see cref="XamGrid"/> templated to the control that will be used for editting.
	/// </summary>
    public abstract class EditableColumn : Column
    {
        #region Members

        private bool _cachedIsEditable = false;
        private int _cachedIsReadOnlyDirtyFlag = int.MinValue;
		HorizontalAlignment _editorHAlign = HorizontalAlignment.Stretch;
		VerticalAlignment _editorVAlign = VerticalAlignment.Stretch;
        Style _editorStyle;

        #endregion // Members

        #region Properties

        #region Public

        #region EditorStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that should be applied to the editor of this <see cref="Column"/>
        /// </summary>
        public Style EditorStyle
        {
            get
            {
                return this._editorStyle;
            }
            set
            {
                if (this._editorStyle != value)
                {
                    this._editorStyle = value;
                    this.OnPropertyChanged("EditorStyle");

                    if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                    {
                        this.ColumnLayout.Grid.ResetPanelRows(this.RequiresFullRedrawOnEditorStyleUpdate);
                    }
                }
            }
        }

        #endregion // EditorStyle

        #region IsReadOnly

        /// <summary>
        /// Identifies the <see cref="IsReadOnly"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(EditableColumn), new PropertyMetadata(false, new PropertyChangedCallback(IsReadOnlyChanged)));

        /// <summary>
        /// Gets / sets if the column should be editable.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        private static void IsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            EditableColumn col = (EditableColumn)obj;
            col.OnPropertyChanged("IsReadOnly");
            col.IsReadOnlyDirtyFlag++;
        }

        #endregion // IsReadOnly

        #region EditorValueConverter

        /// <summary>
        /// Identifies the <see cref="EditorValueConverter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EditorValueConverterProperty = DependencyProperty.Register("EditorValueConverter", typeof(IValueConverter), typeof(EditableColumn), new PropertyMetadata(new PropertyChangedCallback(EditorValueConverterChanged)));

        /// <summary>
        /// Gets/sets the <see cref="IValueConverter"/> that will be used for the genereated <see cref="Binding"/> when a <see cref="Cell"/> enters edit mode.
        /// </summary>
        public IValueConverter EditorValueConverter
        {
            get { return (IValueConverter)this.GetValue(EditorValueConverterProperty); }
            set { this.SetValue(EditorValueConverterProperty, value); }
        }

        private static void EditorValueConverterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            EditableColumn col = (EditableColumn)obj;
            col.OnPropertyChanged("EditorValueConverter");
        }

        #endregion // EditorValueConverter

        #region EditorValueConverterParameter

        /// <summary>
        /// Identifies the <see cref="EditorValueConverterParameter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty EditorValueConverterParameterProperty = DependencyProperty.Register("EditorValueConverterParameter", typeof(object), typeof(EditableColumn), new PropertyMetadata(new PropertyChangedCallback(EditorValueConverterParameterChanged)));

        /// <summary>
        /// Gets/sets the parameter that will be used with the <see cref="EditorValueConverter"/> property.
        /// </summary>
        public object EditorValueConverterParameter
        {
            get { return (object)this.GetValue(EditorValueConverterParameterProperty); }
            set { this.SetValue(EditorValueConverterParameterProperty, value); }
        }

        private static void EditorValueConverterParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            EditableColumn col = (EditableColumn)obj;
            col.OnPropertyChanged("EditorValueConverterParameter");
        }

        #endregion // EditorValueConverterParameter

        #region AllowEditingValidation

        /// <summary>
        /// Identifies the <see cref="AllowEditingValidation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowEditingValidationProperty = DependencyProperty.Register("AllowEditingValidation", typeof(bool), typeof(EditableColumn), new PropertyMetadata(true, new PropertyChangedCallback(AllowEditingValidationChanged)));

        /// <summary>
        /// Gets/sets whether Validation should be used when editing a cell. 
        /// </summary>
        public bool AllowEditingValidation
        {
            get { return (bool)this.GetValue(AllowEditingValidationProperty); }
            set { this.SetValue(AllowEditingValidationProperty, value); }
        }

        private static void AllowEditingValidationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // AllowEditingValidation

        #region EditorHorizontalContentAlignment

        /// <summary>
        /// Gets/Sets the <see cref="HorizontalAlignment"/> of the content of all cells in this <see cref="Column"/> while they're in edit mode;
        /// </summary>
        public HorizontalAlignment EditorHorizontalContentAlignment
        {
            get { return this._editorHAlign; }
            set
            {
                this._editorHAlign = value;
                this.OnPropertyChanged("EditorHorizontalContentAlignment");
            }
        }

        #endregion // EditorHorizontalContentAlignment

        #region EditorVerticalContentAlignment

        /// <summary>
        /// Gets/Sets the <see cref="VerticalAlignment"/> of the content of all cells in this <see cref="Column"/> while they're in edit mode;
        /// </summary>
        public VerticalAlignment EditorVerticalContentAlignment
        {
            get { return this._editorVAlign; }
            set
            {
                this._editorVAlign = value;
                this.OnPropertyChanged("EditorVerticalContentAlignment");
            }
        }

        #endregion // EditorVerticalContentAlignment

        #endregion // Public

        #region Protected

        #region IsEditable

        /// <summary>
        /// Resolves whether this <see cref="Column"/> supports editing.
        /// </summary>
        protected internal override bool IsEditable
        {
            get
            {
                if (this._cachedIsReadOnlyDirtyFlag != this.IsReadOnlyDirtyFlag)
                {
                    this._cachedIsEditable = !this.IsReadOnly;
                    this._cachedIsReadOnlyDirtyFlag = this.IsReadOnlyDirtyFlag;
                }

                return this._cachedIsEditable;
            }
        }

        #endregion // IsEditable

        #region RequiresFullRedrawOnEditorStyleUpdate

        /// <summary>
        /// Gets / sets if whether the <see cref="Infragistics.Controls.Grids.XamGrid.ResetPanelRows(bool)"/> should be called to do a full redraw when the <see cref="EditorStyle"/> is changed.
        /// </summary>
        protected internal virtual bool RequiresFullRedrawOnEditorStyleUpdate
        {
            get { return false; }
        }

        #endregion // RequiresFullRedrawOnEditorStyleUpdate

        #endregion // Protected

        #region Internal

        internal int IsReadOnlyDirtyFlag
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Properties        
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