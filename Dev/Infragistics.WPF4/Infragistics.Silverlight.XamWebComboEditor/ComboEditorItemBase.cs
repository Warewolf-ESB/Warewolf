
using System.Windows;
namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// Implements a selectable item inside a ComboEditorBase class. 
    /// </summary>
    public abstract class ComboEditorItemBase<T> : RecyclingContainer<T>, IBindableItem where T : FrameworkElement
    {
        #region Member

        private bool _isSelected;
        private bool _isFocused;
        private bool _isEnabled;
        private T _control;
        private Style _style;

        #endregion //Member

        #region Costructor

        /// <summary>
        /// Initializes a new instance of the ComboEditorItemBase class.
        /// </summary>
        /// <param name="data">The business object.</param>
        protected ComboEditorItemBase(object data)
        {
            this.Data = data;
            this.IsEnabled = true;
        }

        #endregion //Costructor

        #region Properties

        #region Public

        #region Data

        /// <summary>
        /// Gets the business object that is hold by this <see cref="ComboEditorItem"/>
        /// </summary>
        public object Data
        {
            get;
            private set;
        }

        #endregion //Data

        #region IsSelected

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ComboEditorItem"/> is selected. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (this._isSelected != value)
                {
                    this._isSelected = value;

                    this.OnSelectionChanged();

                    this.OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion // IsSelected

        #region IsFocused

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ComboEditorItem"/> is focused. 
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return this._isFocused;
            }
            set
            {
                if (this._isFocused != value)
                {
                    this._isFocused = value;
                    this.OnPropertyChanged("IsFocused");
                }
            }
        }
        #endregion // IsFocused

        #region Control

        /// <summary>
        /// Gets or sets a <see cref="ComboEditorItemControl"/> that is attached to the <see cref="ComboEditorItem"/>
        /// </summary>
        /// <remarks>A Control is only assoicated with a ComboEditorItem when it's in the viewport of the ItemsPanel.</remarks>
        public T Control
        {
            get
            {
                return this._control;
            }

            protected internal set
            {
                this._control = value;
            }
        }
        #endregion // Control

        #region IsEnabled

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ComboEditorItem"/> is Enabled. 
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                if (this._isEnabled != value)
                {
                    this._isEnabled = value;
                    this.OnPropertyChanged("IsEnabled");
                }
            }
        }

        #endregion //IsEnabled

        #region Style

        /// <summary>
        /// Gets/Sets the style that will be applied to the <see cref="ComboEditorItemControl"/> that represents this item. 
        /// Note: any style set on this item, will override any style set on the <see cref="XamComboEditor"/>
        /// </summary>
        public Style Style
        {
            get{return this._style;}
            set
            {
                if (this._style != value)
                {
                    this._style = value;
                    this.OnPropertyChanged("Style");
                }
            }
        }

        #endregion // Style 

        #endregion //Public

        #region Protected

        #region MeasureRaised

        /// <summary>
        /// Gets/sets whether the underlying control's Measure was raised when the Measure method was invoked. 
        /// </summary>
        protected internal virtual bool MeasureRaised
        {
            get { return true; }
            set{ }
        }

        #endregion // MeasureRaised


        #endregion // Protected

        #endregion //Properties

        #region Methods

        #region Protected

        #region OnSelectionChanged

        /// <summary>
        /// Invoked when the IsSelected proprety changes.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
          
        }

        #endregion // OnSelectionChanged

        #endregion // Protected

        #region Internal

        #region SetSelected

        /// <summary>
        /// Sets the selected state of an item. 
        /// </summary>
        /// <param name="isSelected">Specifies if the item will be selected.</param>
        internal void SetSelected(bool isSelected)
        {
            this._isSelected = isSelected;
            this.OnPropertyChanged("IsSelected");
        }

        #endregion //SetSelected

        #region EnsureVisualStates

        internal virtual void EnsureVisualStates()
        {
            
                
        }

        #endregion // EnsureVisualStates

        #endregion //Internal

        #endregion //Methods

        #region Overrides

        #region OnElementAttached

        /// <summary>
        /// Called when the <see cref="ComboEditorItemControl"/> is attached to the <see cref="ComboEditorItem"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboEditorItemControl"/></param>
        protected override void OnElementAttached(T element)
        {
            this.Control = element;
        }

        #endregion // OnElementAttached

        #region OnElementReleased

        /// <summary>
        /// Called when the <see cref="ComboEditorItemControl"/> is removed from the <see cref="ComboEditorItem"/>
        /// </summary>
        /// <param name="element">A <see cref="ComboEditorItemControl"/></param>
        protected override void OnElementReleased(T element)
        {
            this.Control = null;
        }

        #endregion // OnElementReleased

        #endregion // Overrides   

        #region IBindableItem Members

        /// <summary>
        /// Gets or sets a value indicating whether the object was created from a data source or adhoc. 
        /// </summary>
        protected bool IsDataBound
        {
            get;
            set;
        }

        bool IBindableItem.IsDataBound
        {
            get
            {
                return this.IsDataBound;
            }
            set
            {
                this.IsDataBound = value;
            }
        }

        #endregion
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