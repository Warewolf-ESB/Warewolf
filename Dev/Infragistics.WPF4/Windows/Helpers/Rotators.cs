using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;

namespace Infragistics.Windows.Helpers
{
    #region RotationMode enumeration

    /// <summary>
    /// Determines how a <see cref="RotatorBase"/> derived class decides which value is returned for an item.
    /// </summary>
    public enum RotationMode
    {
        /// <summary>
        /// Cycle thru the rotator values based on the order of the request.
        /// </summary>
        Cycle = 0,
        /// <summary>
        /// Select a value from the list randomly
        /// </summary>
        Random = 1,
    }

    #endregion //RotationMode enumeration	
    
    #region RotatorBase abstract base class

    /// <summary>
    /// Abstract class used by rotators
    /// </summary>
    public abstract class RotatorBase : DependencyObject
    {
        #region Private Members

        private int _currentIndex = -1;

        #endregion //Private Members	

        #region Properties

            #region CurrentIndex

        /// <summary>
        /// Gets/sets the current index
        /// </summary>
        public virtual int CurrentIndex
        {
            get
            {
                return this._currentIndex;
            }
            set
            {
                if (value < -1 || value >= CountInternal)
                    throw new ArgumentOutOfRangeException();

                this._currentIndex = value;
            }
        }

            #endregion //CurrentIndex	
    
            #region CountInternal

        /// <summary>
        /// Returns the number of items (read-only)
        /// </summary>
        abstract protected int CountInternal { get; }

            #endregion //CountInternal	
    
            #region Mode

        /// <summary>
        /// Identifies the 'Mode' dependency property
        /// </summary>
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode",
                typeof(RotationMode), typeof(RotatorBase), new FrameworkPropertyMetadata(RotationMode.Cycle, new PropertyChangedCallback(OnModeChanged)));

        private static void OnModeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            RotatorBase control = target as RotatorBase;

            if (control != null)
            {
                control._cachedMode = (RotationMode)(e.NewValue);
            }
        }

        private RotationMode _cachedMode = RotationMode.Cycle;

        /// <summary>
        /// Determines how the rotator serves up its values
        /// </summary>
        //[Description("Determines how the rotator serves up its values")]
        //[Category("Behavior")]
        public RotationMode Mode
        {
            get
            {
                return this._cachedMode;
            }
            set
            {
                this.SetValue(RotatorBase.ModeProperty, value);
            }
        }

            #endregion //Mode

        #endregion //Properties	

    }

    #endregion //RotatorBase abstract base class	

    #region Rotator<T> abstract base class

    /// <summary>
    /// Abstract generic class used by rotators to specify a target that is strongly typed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Rotator<T> : RotatorBase, IAddChild
    {
        #region Private Members

        private List<T> _list = new List<T>();
        private Random _rnd;

        #endregion //Private Members

        #region Base class overrides

            #region CountInternal

        /// <summary>
        /// Returns the number of items (read-only)
        /// </summary>
        protected override int CountInternal { get { return this._list.Count; } }

            #endregion //CountInternal

        #endregion //Base class overrides

        #region Properties

            #region List

        internal List<T> List { get { return this._list; } }

            #endregion //List

        #endregion //Properties

        #region Methods

            #region GetValueForTarget

        internal object GetValueForTarget(DependencyObject target)
        {
            if (this._list.Count < 1)
                return null;

            int index = 0;
            int count = this._list.Count;

            switch (this.Mode)
            {
                case RotationMode.Cycle:
                {
                    if (target != null)
                    {
                        ItemsControl ic = null;
                        object item = target.GetValue(FrameworkElement.DataContextProperty);

                        if (item != null)
                        {
                            Panel panel = Utilities.GetItemsHostFromItem(target) as Panel;

                            if (panel != null)
                                ic = ItemsControl.GetItemsOwner(panel);

                        }

                        if (ic == null)
                            index = this.CurrentIndex + 1;
                        else
							// June 2006 CTP change
                            //index = ((IGeneratorHost)ic).View.IndexOf(item);
							index = ic.Items.IndexOf(item);

                    }
                    else
                    {
                        index = this.CurrentIndex + 1;
                    }
                }
                break;

                case RotationMode.Random:
                {
                    if (count > 1)
                    {
                        if (this._rnd == null)
                            this._rnd = new Random();

                        index = this._rnd.Next(0, count - 1);
                    }
                }
                break;
            }

            if (index < 0)
                index = 0;
            else
                if (index >= count)
                    index = index % count;

            this.CurrentIndex = index;

            return this._list[index];
        }

            #endregion //GetValueForTarget

        #endregion //Methods

        #region IAddChild Members

        void IAddChild.AddChild(object value)
        {
			if ( !( value is T ) )
				throw new Exception( SR.GetString( "LE_Exception_1", typeof( T ).Name ) );

            this._list.Add((T)value);
        }

        void IAddChild.AddText(string text)
        {
            throw new NotSupportedException(SR.GetString( "LE_NotSupportedException_2" ));
        }

        #endregion
    }

    #endregion //Rotator<T> abstract base class	
    
    #region BrushRotator

    /// <summary>
    /// Class used to maintain a list of Brushes that can be rotated.
    /// </summary>
    [TypeConverter(typeof(BrushRotator.BrushRotatorTypeConverter))]
    public class BrushRotator : Rotator<Brush>
    {
        #region Rotator

        /// <summary>
        /// Returns this instance (read-only)
        /// </summary>
        public BrushRotator Rotator { get { return this; } }

        #endregion //Rotator	
    
        #region BrushRotatorTypeConverter private class

        private class BrushRotatorTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(Brush))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                BrushRotator rotator = value as BrushRotator;

                if (rotator != null && destinationType == typeof(Brush))
                    return rotator.GetValueForTarget(null);

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion //BrushRotatorTypeConverter
    }

    #endregion //BrushRotator	

    #region StyleRotator

    /// <summary>
    /// Class used to maintain a list of Stylees that can be rotated.
    /// </summary>
    [TypeConverter(typeof(StyleRotator.StyleRotatorTypeConverter))]
    public class StyleRotator : Rotator<Style>
    {
        #region Rotator

        /// <summary>
        /// Returns this instance (read-only)
        /// </summary>
        public StyleRotator Rotator { get { return this; } }

        #endregion //Rotator	
    
        #region StyleSelector

        private StyleSelector _styleSelector;

        /// <summary>
        /// Gets the StyleSelector (read-only)
        /// </summary>
        public StyleSelector StyleSelector
        {
            get
            {
                if (this._styleSelector == null)
                    this._styleSelector = new InternalStyleSelector(this);

                return this._styleSelector;
            }
        }

        #endregion //StyleSelector

        #region InternalStyleSelector private class

        private class InternalStyleSelector : StyleSelector
        {
            private StyleRotator _rotator;

            internal InternalStyleSelector(StyleRotator rotator)
            {
                this._rotator = rotator;
            }

            public override Style SelectStyle(object item, DependencyObject container)
            {
                return this._rotator.GetValueForTarget(container) as Style;
            }
        }

        #endregion //InternalStyleSelector private class	

        #region StyleRotatorTypeConverter private class

        private class StyleRotatorTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(Style) || destinationType == typeof(StyleSelector))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                StyleRotator rotator = value as StyleRotator;

                if (rotator != null)
                {
                    if (destinationType == typeof(Style))
                        return rotator.GetValueForTarget(null);

                    if (destinationType == typeof(StyleSelector))
                        return rotator.StyleSelector;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion //StyleRotatorTypeConverter
    }

    #endregion //StyleRotator	

    #region DataTemplateRotator

    /// <summary>
    /// Class used to maintain a list of DataTemplatees that can be rotated.
    /// </summary>
    [TypeConverter(typeof(DataTemplateRotator.TemplateRotatorTypeConverter))]
    public class DataTemplateRotator : Rotator<DataTemplate>
    {
        #region Rotator

        /// <summary>
        /// Returns this instance (read-only)
        /// </summary>
        public DataTemplateRotator Rotator { get { return this; } }

        #endregion //Rotator

        #region TemplateSelector

        private DataTemplateSelector _templateSelector;

        /// <summary>
        /// Gets the StyleSelector (read-only)
        /// </summary>
        public DataTemplateSelector TemplateSelector
        {
            get
            {
                if (this._templateSelector == null)
                    this._templateSelector = new InternalDataTemplateSelector(this);

                return this._templateSelector;
            }
        }

        #endregion //TemplateSelector

        #region InternalDataTemplateSelector private class

        private class InternalDataTemplateSelector : DataTemplateSelector
        {
            private DataTemplateRotator _rotator;

            internal InternalDataTemplateSelector(DataTemplateRotator rotator)
            {
                this._rotator = rotator;
            }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                return this._rotator.GetValueForTarget(container) as DataTemplate;
            }
        }

        #endregion //InternalDataTemplateSelector private class	

        #region TemplateRotatorTypeConverter private class

        private class TemplateRotatorTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(DataTemplate) || destinationType == typeof(DataTemplateSelector))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                DataTemplateRotator rotator = value as DataTemplateRotator;

                if (rotator != null)
                {
                    if (destinationType == typeof(DataTemplate))
                        return rotator.GetValueForTarget(null);

                    if (destinationType == typeof(DataTemplateSelector))
                        return rotator.TemplateSelector;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        #endregion //TemplateRotatorTypeConverter
    }

    #endregion //DataTemplateRotator	

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