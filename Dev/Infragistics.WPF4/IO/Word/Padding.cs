using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections;


using Infragistics.Shared;
using System.ComponentModel.Design.Serialization;


namespace Infragistics.Documents.Word
{
    #region Padding struct
    /// <summary>
    /// Structure used to store padding.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]
    [TypeConverter(typeof(PaddingConverter))] // MRS 3/17/2011 - TFS68842

    public struct Padding
    {
        #region Members
        
        private float top;
        private float left;
        private float bottom;
        private float right;
        static private object empty;

        #endregion Members

        #region Constructor
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public Padding( float left, float top, float right, float bottom )
        {
            if ( top < 0f )
                throw new ArgumentOutOfRangeException( "top" );

            if ( left < 0f )
                throw new ArgumentOutOfRangeException( "left" );

            if ( bottom < 0f )
                throw new ArgumentOutOfRangeException( "bottom" );

            if ( right < 0f )
                throw new ArgumentOutOfRangeException( "right" );

            this.top = top;
            this.left = left;
            this.bottom = bottom;
            this.right = right;
        }
        #endregion Constructor

        #region Properties

        /// <summary>Returns the padding as relative to the top.</summary>
        public float Top { get { return this.top; } }

        /// <summary>Returns the padding as relative to the left.</summary>
        public float Left { get { return this.left; } }

        /// <summary>Returns the padding as relative to the bottom.</summary>
        public float Bottom { get { return this.bottom; } }

        /// <summary>Returns the padding as relative to the right.</summary>
        public float Right { get { return this.right; } }

        /// <summary>
        /// Returns a Padding instance whose Top, Left, Right, and Bottom
        /// are all set to zero.
        /// </summary>
        static public Padding Empty
        {
            get
            {
                if ( Padding.empty == null )
                    Padding.empty = new Padding( 0f, 0f, 0f, 0f );

                return (Padding)Padding.empty;
            }
        }

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values to their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.top = 0f;
            this.left = 0f;
            this.bottom = 0f;
            this.right = 0f;
        }
        #endregion Reset

        #region Pad methods

        /// <summary>
        /// Returns an instance with all properties set to the
        /// specified <paramref name="value"/>.
        /// </summary>
        static public Padding PadAll( float value )
        {
            return new Padding( value, value, value, value );
        }

        /// <summary>
        /// Returns an instance whose Left property is set to the
        /// specified <paramref name="value"/>, with all other
        /// properties set to zero.
        /// </summary>
        static public Padding PadLeft( float value )
        {
            return new Padding( value, 0f, 0f, 0f );
        }

        /// <summary>
        /// Returns an instance whose Right property is set to the
        /// specified <paramref name="value"/>, with all other
        /// properties set to zero.
        /// </summary>
        static public Padding PadRight( float value )
        {
            return new Padding( 0f, 0f, value, 0f );
        }

        /// <summary>
        /// Returns an instance whose Top property is set to the
        /// specified <paramref name="value"/>, with all other
        /// properties set to zero.
        /// </summary>
        static public Padding PadTop( float value )
        {
            return new Padding( 0f, value, 0f, 0f );
        }

        /// <summary>
        /// Returns an instance whose Bottom property is set to the
        /// specified <paramref name="value"/>, with all other
        /// properties set to zero.
        /// </summary>
        static public Padding PadBottom( float value )
        {
            return new Padding( 0f, 0f, 0f, value );
        }

        /// <summary>
        /// Returns an instance whose Left and Right properties are set to the
        /// specified <paramref name="value"/>, with all other properties set to zero.
        /// </summary>
        static public Padding PadHorizontal( float value )
        {
            return new Padding( value, 0f, value, 0f );
        }

        /// <summary>
        /// Returns an instance whose Top and Bottom properties are set to the
        /// specified <paramref name="value"/>, with all other properties set to zero.
        /// </summary>
        static public Padding PadVertical( float value )
        {
            return new Padding( 0f, value, 0f, value );
        }
        #endregion Pad methods

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            if ( this.top == 0 && this.left == 0 && this.right == 0 && this.bottom == 0 )
                return "{Empty}";
            else
                return string.Format("Top={0}, Left={1}, Bottom={2}, Right={3}", this.top, this.left, this.bottom, this.right);
        }
        #endregion ToString

        #endregion Methods

        #region Overrides

        #region Equals
         /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if ((obj is Padding) == false)
                return false;

            Padding objPadding = (Padding)obj;

            return this.Left == objPadding.Left &&
                this.Top == objPadding.Top &&
                this.Right == objPadding.Right &&
                this.Bottom == objPadding.Bottom;
        }
        #endregion // Equals

        #region GetHashCode
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Left.GetHashCode() |
                this.Top.GetHashCode() << 8 |
                this.Right.GetHashCode() << 16 |
                this.Bottom.GetHashCode() << 24;
        }
        #endregion // GetHashCode

        #endregion // Overrides

        
        // MRS 3/17/2011 - TFS68842
        // Added a converter class for the Padding struct. 
        //
        #region PaddingConverter class
        /// <summary>
        /// Provides a type converter to convert <see cref="Padding"/> values to and from various other representations.
        /// </summary>
        public class PaddingConverter : TypeConverter
        {
            /// <summary>
            /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.                        
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <param name="sourceType">A <see cref="System.Type"/> that represents the type you want to convert from.</param>
            /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {                
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            /// <summary>
            /// Returns whether this converter can convert the object to the specified type, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <param name="destinationType">A <see cref="System.Type"/> that represents the type you want to convert to.</param>
            /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
            }

            /// <summary>
            /// Converts the given object to the type of this converter, using the specified context and culture information.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> to use as the current culture.</param>
            /// <param name="value">The <see cref="System.Object"/> to convert.</param>
            /// <returns>An <see cref="System.Object"/> that represents the converted value.</returns>
            /// <exception cref="System.NotSupportedException">The conversion cannot be performed.</exception>
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string valueAsString = value as string;
                if (valueAsString == null)                
                    return base.ConvertFrom(context, culture, value);
                
                valueAsString = valueAsString.Trim();
                if (valueAsString.Length == 0)
                    return null;
                
                if (culture == null)
                    culture = CultureInfo.CurrentCulture;
                
                char ch = culture.TextInfo.ListSeparator[0];
                string[] strArray = valueAsString.Split(new char[] { ch });

                if (strArray.Length != 4)
                    throw new ArgumentException(SR.GetString("Exception_PaddingTextParseFailedFormat", valueAsString));

                float[] values = new float[strArray.Length];
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)converter.ConvertFromString(context, culture, strArray[i]);
                }
               
                return new Padding(values[0], values[1], values[2], values[3]);
            }

            /// <summary>
            /// Converts the given value object to the specified type, using the specified and culture information.
            /// </summary>
            /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
            /// <param name="culture"> A System.Globalization.CultureInfo. If null is passed, the current culture is assumed.</param>
            /// <param name="value">The System.Object to convert.</param>
            /// <param name="destinationType">The System.Type to convert the value parameter to.</param>
            /// <returns>An System.Object that represents the converted value.</returns>
            /// <exception cref="System.ArgumentNullException">The destinationType parameter is null.</exception>
            /// <exception cref="System.NotSupportedException">The conversion cannot be performed.</exception>
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                    throw new ArgumentNullException("destinationType");
                                
                if (value is Padding)
                {
                    Padding padding = (Padding)value;

                    if (destinationType == typeof(string))
                    {                        
                        if (culture == null)
                            culture = CultureInfo.CurrentCulture;
                        
                        string separator = culture.TextInfo.ListSeparator + " ";

                        TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
                        string[] strArray = new string[4];
                        int i = 0;
                        strArray[i++] = converter.ConvertToString(context, culture, padding.Left);
                        strArray[i++] = converter.ConvertToString(context, culture, padding.Top);
                        strArray[i++] = converter.ConvertToString(context, culture, padding.Right);
                        strArray[i++] = converter.ConvertToString(context, culture, padding.Bottom);
                        return string.Join(separator, strArray);
                    }
                    if (destinationType == typeof(InstanceDescriptor))
                    {
                        return new InstanceDescriptor(typeof(Padding).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }), new object[] { padding.Left, padding.Top, padding.Right, padding.Bottom });
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            /// <summary>
            /// Creates an instance of the type that this <see cref="System.ComponentModel.TypeConverter"/> is associated with, using the specified context, given a set of property values for the object.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <param name="propertyValues">An <see cref="System.Collections.IDictionary"/> of new property values.</param>
            /// <returns>An <see cref="System.Object"/> representing the given System.Collections.IDictionary, or null if the object cannot be created. This method always returns null.</returns>
            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                
                if (propertyValues == null)
                    throw new ArgumentNullException("propertyValues");
                
                Padding padding = (Padding)context.PropertyDescriptor.GetValue(context.Instance);
                return new Padding((float)propertyValues["Left"], (float)propertyValues["Top"], (float)propertyValues["Right"], (float)propertyValues["Bottom"]);
            }

            /// <summary>
            /// Returns whether changing a value on this object requires a call to <see cref="System.ComponentModel.TypeConverter.CreateInstance(System.Collections.IDictionary)"/> to create a new value, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <returns> true if changing a property on this object requires a call to <see cref="System.ComponentModel.TypeConverter.CreateInstance(System.Collections.IDictionary)"/> to create a new value; otherwise, false.</returns>
            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            /// <summary>
            /// Returns a collection of properties for the type of array specified by the value parameter, using the specified context and attributes.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <param name="value">An <see cref="System.Object"/> that specifies the type of array for which to get properties.</param>
            /// <param name="attributes">An array of type <see cref="System.Attribute"/> that is used as a filter.</param>
            /// <returns>A <see cref="System.ComponentModel.PropertyDescriptorCollection"/> with the properties that are exposed for this data type, or null if there are no properties.</returns>
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(Padding), attributes).Sort(new string[] { "All", "Left", "Top", "Right", "Bottom" });
            }

            /// <summary>
            /// Returns whether this object supports properties, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
            /// <returns>true if <see cref="System.ComponentModel.TypeConverter.GetProperties(System.Object)"/> should be called to find the properties of this object; otherwise, false.</returns>
            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
        #endregion // PaddingConverter class
        

    }
    #endregion Padding struct
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