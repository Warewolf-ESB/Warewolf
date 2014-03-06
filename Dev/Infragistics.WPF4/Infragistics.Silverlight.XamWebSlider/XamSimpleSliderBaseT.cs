using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control that provides generic simple slider behavior. 
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public abstract class XamSimpleSliderBase<T> : XamSliderBase<T>
    {
        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Called when control template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Thumb == null)
            {
                this.Thumb = new XamSliderThumb<T> { Value = this.Value };
            }

            if (this.ThumbStyle != null)
            {
                this.Thumb.Style = this.ThumbStyle;
            }

            this.Thumb.IsActive = true;
        }
        #endregion //OnApplyTemplate

        #endregion Overrides

        #region Properties

        #region Thumb

        /// <summary>
        /// Identifies the <see cref="Thumb"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ThumbProperty = DependencyProperty.Register("Thumb", typeof(XamSliderThumb<T>), typeof(XamSimpleSliderBase<T>), new PropertyMetadata(new PropertyChangedCallback(ThumbChanged)));

        /// <summary>
        /// Gets or sets the thumb.
        /// </summary>
        /// <value>The thumb.</value>
        public XamSliderThumb<T> Thumb
        {
            get { return (XamSliderThumb<T>)this.GetValue(ThumbProperty); }
            set { this.SetValue(ThumbProperty, value); }
        }

        private static void ThumbChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSimpleSliderBase<T> slider = obj as XamSimpleSliderBase<T>;
            if (slider != null)
            {
                if (e.NewValue != e.OldValue && e.NewValue != null)
                {
                    XamSliderThumb<T> thumb = e.NewValue as XamSliderThumb<T>;
                    slider.OnThumbChanged(thumb);
                }
            }
        }

        #endregion // Thumb

        #region Value

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(XamSimpleSliderBase<T>), null);

        /// <summary>
        /// Gets or sets the value.
        /// It is the current value of the ActiveThumb , respectively 
        /// Thumb of the simple slider
        /// </summary>
        /// <value>The value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual T Value
        {
            get { return (T)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        #endregion // Value

        #endregion // Properties

        #region Methods
        #region Protected
        #region OnThumbChanged
        /// <summary>
        /// Called when the value of Thumb property is changed.
        /// </summary>
        /// <param name="thumb">The thumb.</param>
        protected virtual void OnThumbChanged(XamSliderThumb<T> thumb)
        {
            if (this.Thumbs.Count > 0)
            {
                this.Thumbs.Clear();
                if (this.HorizontalThumbsPanel != null)
                {
                    this.HorizontalThumbsPanel.Children.Clear();   
                }

                if (this.VerticalThumbsPanel != null)
                {
                    this.VerticalThumbsPanel.Children.Clear();
                }
            }

            if (!thumb.Value.Equals(this.Value))
            {
                this.Value = thumb.Value;
            }

            this.Thumbs.Add(thumb);

            Binding binding = new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            };

            this.Thumb.SetBinding(XamSliderThumb<T>.ValueProperty, binding);
        }
        #endregion //OnThumbChanged
        #endregion //Protected 
        #endregion //Methods
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