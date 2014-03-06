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
using System.ComponentModel;
using System.Windows.Data;
using Infragistics.Controls.Grids.Primitives;
using System.Globalization;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for all column objects in the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class ColumnBase : DependencyObjectNotifier, IProvidePropertyPersistenceSettings
	{
		#region Members

		DataTemplate _headerTemplate;
		Type _dataType;
		List<string> _propertiesThatShouldntBePersisted;
        Style _cellStyle, _headerStyle, _footerStyle;
        bool _isHideable = true;
        ColumnLayout _layout;

		// AS 1/20/12 XamGantt
		[ThreadStatic]
		static Dictionary<Type, TypeConverter> _typeConverterTable;

		#endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase"/> class.
        /// </summary>
        protected ColumnBase()
        {
            this.DataField = new DataField("", null);
        }

        #endregion // Constructor

		#region Properties

		#region Public

		#region Key

		/// <summary>
		/// Identifies the <see cref="Key"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(string), typeof(ColumnBase), new PropertyMetadata(new PropertyChangedCallback(KeyChanged)));

		/// <summary>
		/// Gets/sets a string that identifies the <see cref="ColumnBase"/>.
		/// </summary>
		public string Key
		{
			get { return (string)this.GetValue(KeyProperty); }
			set { this.SetValue(KeyProperty, value); }
		}

		private static void KeyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // Key

		#region Visibility

		/// <summary>
		/// Identifies the <see cref="Visibility"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ColumnBase), new PropertyMetadata(new PropertyChangedCallback(VisibilityChanged)));

		/// <summary>
		/// Gets/Sets the Visibility of the <see cref="ColumnBase"/>
		/// </summary>
		public Visibility Visibility
		{
			get { return (Visibility)this.GetValue(VisibilityProperty); }
			set { this.SetValue(VisibilityProperty, value); }
		}

		private static void VisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnBase col = (ColumnBase)obj;
			col.OnVisibilityChanged();

            // Ok, in a ParentColumn scenario, if a user tries to hide a child column, and its the last visible child column
            // Then don't hide the child column, make it visible, and instead hide the parent column.
            if ((Visibility)e.NewValue == Visibility.Collapsed)
            {
                Column column = col as Column;
                if (column != null && column.ParentColumn != null)
                {
                    column.ParentColumn.InvalidateVisibility(false);

                    if (column.ParentColumn.Visibility == Visibility.Collapsed)
                        col.Visibility = Visibility.Visible;
                }
            }
		}

		/// <summary>
		/// Raised when the Visiblity of a <see cref="ColumnBase"/> has changed.
		/// </summary>
		protected virtual void OnVisibilityChanged()
		{
			this.OnPropertyChanged("Visibility");
		}
		#endregion // Visibility

		#region ColumnLayout

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> thats owns this <see cref="ColumnBase"/>
		/// </summary>
		public ColumnLayout ColumnLayout
		{
            get { return this._layout; }
            protected internal set
            {
                if (this._layout != value)
                {
                    this._layout = value;
                    this.OnColumnLayoutChanged();
                }
            }
        }       

		#endregion // ColumnLayout

		#region CellStyle

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="CellControl"/> objects on this <see cref="ColumnBase"/>.
        /// </summary>
        public Style CellStyle
        {
            get
            {
                return this._cellStyle;
            }
            set
            {
                if (this._cellStyle != value)
                {
                    this._cellStyle = value;
                    ControlTemplate controlTemplate = null;
                    this.StrippedCellStyleForConditionalFormatting = XamGrid.CloneStyleWithoutControlTemplate(value, out controlTemplate);
                    this.ControlTemplateForConditionalFormatting = controlTemplate;
                    this.OnStyleChanged();
                    this.OnPropertyChanged("CellStyle");
                }
            }
        }

		#endregion // CellStyle

		#region CellStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="CellControl"/>
		/// </summary>
		public virtual Style CellStyleResolved
		{
			get
			{
				if (this.CellStyle == null && this.ColumnLayout != null)
					return this.ColumnLayout.CellStyleResolved;
				else
					return this.CellStyle;
			}
		}

		#endregion // CellStyleResolved

		#region HeaderStyle

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="HeaderCellControl"/> objects on this <see cref="ColumnBase"/>.
		/// </summary>
		public Style HeaderStyle
        {
            get
            {
                return this._headerStyle;
            }
            set
            {
                if (this._headerStyle != value)
                {
                    this._headerStyle = value;
                    this.OnStyleChanged();
                    this.OnPropertyChanged("HeaderStyle");
                }
            }
        }

		#endregion // HeaderStyle

		#region HeaderStyleResolved


		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="HeaderCellControl"/>
		/// </summary>
		public virtual Style HeaderStyleResolved
		{
			get
			{
				if (this.HeaderStyle == null && this.ColumnLayout != null)
					return this.ColumnLayout.HeaderStyleResolved;
				else
					return this.HeaderStyle;
			}
		}

		#endregion //HeaderStyleResolved

		#region FooterStyle

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be used for all <see cref="FooterCellControl"/> objects on this <see cref="ColumnBase"/>.
		/// </summary>
		public Style FooterStyle
        {
            get
            {
                return this._footerStyle;
            }
            set
            {
                if (this._footerStyle != value)
                {
                    this._footerStyle = value;
                    this.OnStyleChanged();
                    this.OnPropertyChanged("FooterStyle");
                }
            }
        }

		#endregion // FooterStyle

		#region FooterStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="FooterCellControl"/>
		/// </summary>
		public virtual Style FooterStyleResolved
		{
			get
			{
				if (this.FooterStyle == null && this.ColumnLayout != null)
					return this.ColumnLayout.FooterStyleResolved;
				else
					return this.FooterStyle;
			}
		}

		#endregion //FooterStyleResolved

		#region IsAutoGenerated

		/// <summary>
		/// Gets whether or not the <see cref="ColumnBase"/> was predefined, or generated by the <see cref="XamGrid"/> based on the 
		/// underlying data. 
		/// </summary>
		public bool IsAutoGenerated
		{
			get;
			protected internal set;
		}

		#endregion // IsAutoGenerated

		#region Tag

		/// <summary>
		/// Identifies the <see cref="Tag"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(ColumnBase), null);

		/// <summary>
		/// Allows additional information to be stored for a <see cref="ColumnBase"/>
		/// </summary>
		public object Tag
		{
			get { return (object)this.GetValue(TagProperty); }
			set { this.SetValue(TagProperty, value); }
		}

		#endregion // Tag

		#region DataType
		/// <summary>
		/// The DataType that the column's data is derived from.
		/// </summary>
		public virtual Type DataType
		{
			get
			{
				return this._dataType;
			}
			protected internal set
			{
				if (value != this._dataType)
				{
					this._dataType = value;
					this.OnDataTypeChanged();
				}
			}
		}
		#endregion

        #region HeaderText

        /// <summary>
        /// Identifies the <see cref="HeaderText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ColumnBase), new PropertyMetadata(new PropertyChangedCallback(HeaderTextChanged)));

        /// <summary>
        /// Gets/Sets the text that will be displayed in the Header.
        /// </summary>
        /// <remarks>If the <see cref="ColumnBase.HeaderTemplate"/> is set, this property will have no effect.</remarks>
        public string HeaderText
        {
            get { return (string)this.GetValue(HeaderTextProperty); }
            set { this.SetValue(HeaderTextProperty, value); }
        }

        private static void HeaderTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnBase col = (ColumnBase)obj;

            col.OnPropertyChanged("HeaderText");
        }

        #endregion // HeaderText 
				
		#region HeaderTemplate

		/// <summary>
		/// Gets/Sets the <see cref="DataTemplate"/> used to define the Content of the Header of the <see cref="ColumnBase"/>.
		/// </summary>
		public DataTemplate HeaderTemplate
		{
			get { return this._headerTemplate; }
			set
			{
				this._headerTemplate = value;
				this.OnPropertyChanged("HeaderTemplate");
			}
		}

		#endregion // HeaderTemplate

        #region DisplayNameResolved

        /// <summary>
        /// Gets a string that represents the Column, if the HeaderText property isn't used, it fallsback to the Column's Key.
        /// </summary>
        public string DisplayNameResolved
        {
            get
            {
                if (string.IsNullOrEmpty(this.HeaderText))
                    return this.Key;
                else
                    return this.HeaderText;
            }
        }

        #endregion // DisplayNameResolved

        #region IsHideable

        /// <summary>
        /// Gets/sets whether a <see cref="ColumnBase"/> can be hidden via the UI.
        /// </summary>
        public virtual bool IsHideable
        {
            get { return this._isHideable; }
            set 
            {
                if (this._isHideable != value)
                {
                    this._isHideable = value;
                    this.OnPropertyChanged("IsHideable");
                }
            }
        }

        #endregion // IsHideable

		#endregion // Public

		#region Protected

		#region RequiresBoundDataKey
		/// <summary>
		/// Gets whether an exception should be thrown if the key associated with the <see cref="ColumnBase"/> doesn't 
		/// correspond with a property in the data that this object represents.
		/// </summary>
		protected internal virtual bool RequiresBoundDataKey
		{
			get { return true; }
		}
		#endregion // RequiresBoundDataKey

		#region PropertiesToIgnore

		/// <summary>
		/// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
		/// </summary>
		protected virtual List<string> PropertiesToIgnore
		{
			get
			{
				if (this._propertiesThatShouldntBePersisted == null)
				{
					this._propertiesThatShouldntBePersisted = new List<string>()
					{
						"ColumnLayout"
					};
				}

				return this._propertiesThatShouldntBePersisted;
			}
		}

		#endregion // PropertiesToIgnore

		#region PriorityProperties

		/// <summary>
		/// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
		/// </summary>
		protected virtual List<String> PriorityProperties
		{
			get
			{
				return null;
			}
		}

		#endregion // PriorityProperties

        #region DataField

        /// <summary>
        /// Gets/Sets the <see cref="DataField"/> associated with this particular <see cref="ColumnBase"/>
        /// </summary>
        protected internal DataField DataField
        {
            get;
            set;
        }

        #endregion // DataField

		#endregion // Protected

        #region Internal

        internal bool IsMoving
        {
            get;
            set;
        }

        #region StrippedCellStyleForConditionalFormatting

        internal Style StrippedCellStyleForConditionalFormatting
        {
            get;
            set;
        }

        #endregion // StrippedCellStyleForConditionalFormatting

        #region StrippedCellStyleForConditionalFormattingResolved

        internal virtual Style StrippedCellStyleForConditionalFormattingResolved
        {
            get
            {
                if (this.StrippedCellStyleForConditionalFormatting == null && this.ColumnLayout != null)
                    return this.ColumnLayout.StrippedCellStyleForConditionalFormattingResolved;
                else
                    return this.StrippedCellStyleForConditionalFormatting;
            }
        }

        #endregion // StrippedCellStyleForConditionalFormattingResolved

        internal ControlTemplate ControlTemplateForConditionalFormatting
        {
            get;
            set;
        }

        internal virtual ControlTemplate ControlTemplateForConditionalFormattingResolved
        {
            get
            {
                if (this.ControlTemplateForConditionalFormatting == null && this.ColumnLayout != null)
                    return this.ColumnLayout.ControlTemplateForConditionalFormattingResolved;

                return this.ControlTemplateForConditionalFormatting;
            }
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region OnStyleChanged

        /// <summary>
		/// Raised when any of the Style properties of the <see cref="ColumnBase"/> have changed.
		/// </summary>
		protected internal virtual void OnStyleChanged()
		{
			if (this.ColumnLayout != null)
				this.ColumnLayout.Grid.ResetPanelRows();
		}

		#endregion // OnStyleChanged

		#region OnDataTypeChanged
		/// <summary>
		/// Raised when the DataType of the <see cref="ColumnBase" /> is changed.
		/// </summary>
		protected virtual void OnDataTypeChanged()
		{
		}
		#endregion // OnDataTypeChanged

		#region ResolveValue

		/// <summary>
		/// If the <see cref="DataType"/> is available, uses Convert to cast the inputted value.
		/// </summary>
		/// <param name="value"></param>
        /// <param name="culture"></param>
		/// <returns></returns>
        protected internal virtual object ResolveValue(object value, CultureInfo culture)
        {
            if (this.DataType != null)
            {
                Type conversionType = this.DataType;

                bool isGenericNullable = conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

                if (isGenericNullable)
                {
                    if (value != null)
                    {
                        Type newType = Nullable.GetUnderlyingType(conversionType);

                        conversionType = newType;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (value == null)
                        return value;
                }

				Type valueType = value.GetType();

				// AS 1/20/12 XamGantt
				// if the type represented by the DataType of the column has a TypeConverter
				// associated with it then we should try to use that. the Convert.ChangeType 
				// will just cast the value to an IConvertible and for "unknown" types (i.e. 
				// any type not specified by the various ToXXX methods on IConvertible such as 
				// ToDateTime) it will just call that object's ToType implementation. to 
				// restrict the scope of this change we will just do this for intrinsic/known 
				// types or if the value is not iconvertible (in which case Convert would 
				// have thrown an exception). if the value is a custom type that implements 
				// iconvertible then we'll let it's ToType handle it
				if (!(value is IConvertible) || CoreUtilities.IsKnownType(valueType))
				{
					if (valueType == conversionType)
						return value;

					var converter = GetConverter(conversionType);

					// if the typeconverter for the targettype can handle it then let 
					// it do the conversion
					if (null != converter && converter.CanConvertFrom(valueType))
					{
						return converter.ConvertFrom(null, culture, value);
					}
				}

                return Convert.ChangeType(value, conversionType, culture);
            }
            return value;
        }

        /// <summary>
        /// Resolves the value to the datatype of the column using the current culture.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected internal virtual object ResolveValue(object value)
        {
            return this.ResolveValue(value, CultureInfo.CurrentCulture);
        }

		#endregion // ResolveValue

		#region FinishedLoadingPersistence

		/// <summary>
		/// Allows an object to perform an operation, after it's been loaded.
		/// </summary>
		protected virtual void FinishedLoadingPersistence()
		{
		}

		#endregion // FinishedLoadingPersistence

        #region OnColumnLayoutChanged

        /// <summary>
        /// Raised when the a <see cref="ColumnLayout"/> is assigned or removed from this <see cref="ColumnBase"/>
        /// </summary>
        protected virtual void OnColumnLayoutChanged()
        {

        }

        #endregion // OnColumnLayoutChanged

        #endregion // Protected

		#region Internal

		#region GetConverter
		// AS 1/20/12 XamGantt
		internal static TypeConverter GetConverter(Type type)
		{
			if (type == null
				|| CoreUtilities.IsKnownType(type)
				|| type.IsEnum
				|| type == typeof(object))
			{
				return null;
			}

			if (_typeConverterTable == null)
				_typeConverterTable = new Dictionary<Type, TypeConverter>();

			TypeConverter converter;

			if (!_typeConverterTable.TryGetValue(type, out converter))
			{
				try
				{
					var attribs = type.GetCustomAttributes(typeof(TypeConverterAttribute), true);

					if (null != attribs && attribs.Length > 0)
					{
						var attrib = attribs[0] as TypeConverterAttribute;
						var converterType = Type.GetType(attrib.ConverterTypeName, false);

						if (null != converterType)
						{
							converter = (TypeConverter)Activator.CreateInstance(converterType);
						}
					}
				}
				catch
				{
				}

				_typeConverterTable[type] = converter;
			}

			return converter;
		}
		#endregion //GetConverter 

		#endregion //Internal

        #endregion // Methods

        #region IProvidePropertyPersistenceSettings Members

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
		{
			get
			{
				return this.PropertiesToIgnore;
			}
		}

		List<string> IProvidePropertyPersistenceSettings.PriorityProperties
		{
			get
			{
				return this.PriorityProperties;
			}
		}

		void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
		{
			this.FinishedLoadingPersistence();
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