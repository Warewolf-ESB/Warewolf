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

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// Implements a selectable item inside a <see cref="XamComboEditor"/> class. 
    /// </summary>
    public class ComboEditorItem : ComboEditorItemBase<ComboEditorItemControl>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboEditorItem"/> class.
        /// </summary>
        /// <param name="data">The business object.</param>
        /// <param name="xamWebComboEditor">A reference to the ComboEditor.</param>
        internal ComboEditorItem(object data, XamComboEditor xamWebComboEditor) : base(data)
        {
            this.ComboEditor = xamWebComboEditor;
            this.ComboEditor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ComboEditor_PropertyChanged);
        }

        #endregion //Constructor

        #region Properties

        #region Public

        #region ComboEditor

        /// <summary>
        /// Gets the <see cref="XamComboEditor"/> that holds this <see cref="ComboEditorItem"/>
        /// </summary>
        public XamComboEditor ComboEditor
        {
            get;
            private set;
        }

        #endregion //ComboEditor

        #endregion // Public

        #endregion // Properties

        #region EventHandlers

        void ComboEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemContainerStyle")
                this.OnPropertyChanged("Style");
            else if (e.PropertyName == "ItemTemplate")
                this.OnPropertyChanged("Template");
        }

        #endregion // EventHandlers

        #region Overrides

        #region OnSelectionChanged

        /// <summary>
        /// Invoked when the IsSelected proprety changes.
        /// </summary>
        protected override void OnSelectionChanged()
        {
            if (this.IsSelected)
                this.ComboEditor.SelectItem(this, false);
            else
                this.ComboEditor.UnselectItem(this);
        }

        #endregion // OnSelectionChanged

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates an isntance of <see cref="ComboEditorItemControl"/> 
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override ComboEditorItemControl CreateInstanceOfRecyclingElement()
        {
            return new ComboEditorItemControl();
        }

        #endregion //CreateInstanceOfRecyclingElement

        #region OnElementAttached

        /// <summary>
        /// Called when the <see cref="ComboEditorItemControl"/> is attached to the <see cref="ComboEditorItem"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboEditorItemControl"/></param>
        protected override void OnElementAttached(ComboEditorItemControl element)
        {
            base.OnElementAttached(element);
            element.OnAttached(this);
        }

        #endregion // OnElementAttached

        #region OnElementReleased

        /// <summary>
        /// Called when the <see cref="ComboEditorItemControl"/> is removed from the <see cref="ComboEditorItem"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboEditorItemControl"/></param>
        protected override void OnElementReleased(ComboEditorItemControl element)
        {
            base.OnElementReleased(element);
            element.OnReleased(this);
        }

        #endregion // OnElementReleased

        #region EnsureVisualStates

        internal override void EnsureVisualStates()
        {
            if (this.Control != null)
                this.Control.EnsureVisualStates();
        }

        #endregion // EnsureVisualStates

        #region MeasureRaised

        /// <summary>
        /// Gets/Sets whether the measure method was raised when Measure was called. 
        /// </summary>
        protected internal override bool MeasureRaised
        {
            get
            {
                if (this.Control != null)
                    return this.Control.MeasureRaised;

                return true;
            }
            set
            {
                if (this.Control != null)
                    this.Control.MeasureRaised = value;
            }
        }

        #endregion // MeasureRaised

        #endregion // Overrides
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