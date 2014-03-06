using System.ComponentModel;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A class which represents a single color in a <see cref="ColorStrip"/>.
    /// </summary>
    public class ColorItem : INotifyPropertyChanged
    {
        #region Members

        Color _color;
        bool _isSelected;        
        bool _isHover;
        bool _wasSelected;

        #endregion // Members

        #region Properties

        #region Parent

        /// <summary>
        /// Gets the <see cref="ColorStrip"/> which this <see cref="ColorItem"/> is part of.
        /// </summary>
        public ColorStrip Parent { get; protected internal set; }

        #endregion // Parent

        #region Color
        /// <summary>
        /// Gets / sets the <see cref="Color"/> which is represented by this <see cref="ColorItem"/>.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._color;
            }
            set
            {
                if (this._color != value)
                {
                    this._color = value;
                    this.NotifyPropertyChanged("Color");
                }
            }
        }
        #endregion // Color

        #region IsSelected

        /// <summary>
        /// Gets / sets whether this <see cref="ColorItem"/> is selected.  
        /// </summary>
        protected internal bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                //if (value != this.IsSelected)
                //{
                    if (this.Parent != null && this.Parent.ColorPalette != null)
                    {
                        if (this.Color != this.Parent.ColorPalette.DefaultColor)
                        {
                            if (value)
                            {
                                this.Parent.SelectedColorItem = this;
                            }
                            else
                            {
                                this.Parent.SelectedColorItem = null;
                            }

                            this._isSelected = value;
                        }
                    }
                    else
                    {
                        this._isSelected = value;
                    }
                //}
            }
        }

        #endregion // IsSelected

        #region WasSelected

        /// <summary>
        /// Gets / sets if this <see cref="ColorItem"/> was selected.
        /// </summary>
        protected internal bool WasSelected
        {
            get
            {
                return _wasSelected;
            }
            set
            {
                if (_wasSelected != value)
                {
                    _wasSelected = value;
                    if (this.ColorItemBox != null)
                    {
                        this.ColorItemBox.EnsureState();
                    }
                }
            }
        }

        #endregion // WasSelected

        #region IsHover
        /// <summary>
        /// Gets / sets if this <see cref="ColorItem"/> is marked as being hovered over.
        /// </summary>
        protected internal bool IsHover
        {
            get
            {
                return this._isHover;
            }
            set
            {
                if (value != this.IsHover)
                {
                    this._isHover = value;
                    if (this.Parent != null)
                    {
                        if (value)
                        {
                            this.Parent.HoverColorItem = this;
                        }
                        else
                        {
                            this.Parent.HoverColorItem = null;
                        }
                    }
                }
            }
        }
        #endregion // IsHover

        #region ColorItemBox

        /// <summary>
        /// Gets / sets the <see cref="ColorItemBox"/> which is currently displaying this <see cref="ColorItem"/>.
        /// </summary>
        protected internal ColorItemBox ColorItemBox
        {
            get;
            set;
        }

        #endregion // ColorItemBox

        #endregion // Properties

        #region Methods

        #region SetSelectedInternal
        /// <summary>
        /// Used by the control to set the internal member of the object without passing the information up.
        /// </summary>        
        /// <param name="selected"></param>
        protected internal void SetSelectedInternal(bool selected)
        {
            bool raiseEvent = selected != this._isSelected;
            if (raiseEvent)
            {
                this.NotifyPropertyChanged("IsSelected");
            }
        }
        #endregion // SetSelectedInternal

        #region SetWasSelected

        /// <summary>
        /// Used by the control to set the internal member of the object without passing the information up.
        /// </summary>        
        /// <param name="wasSelected"></param>
        protected internal void SetWasSelected(bool wasSelected)
        {
            if (this.WasSelected != wasSelected)
            {
                this.WasSelected = wasSelected;
                this.NotifyPropertyChanged("WasSelected");
            }
        }

        #endregion // SetWasSelected

        #region SetHoverInternal
        /// <summary>
        /// Used by the control to set the internal member of the object without passing the information up.
        /// </summary>        
        /// <param name="hover"></param>
        protected internal void SetHoverInternal(bool hover)
        {
            if (hover != this._isHover)
            {
                this._isHover = hover;

                this.NotifyPropertyChanged("IsHover");
            }
        }
        #endregion // SetHoverInternal

        #endregion // Methods

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="ColorItem"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="ColorItem"/> object.
        /// </summary>
        /// <param propertyName="propName">The propertyName of the property that has changed.</param>
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
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