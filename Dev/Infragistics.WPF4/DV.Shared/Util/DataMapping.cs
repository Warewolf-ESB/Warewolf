using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Represents a correspondence between an internal and external attribute
    /// name.
    /// </summary>
    public sealed class DataMappingPair
    {
        /// <summary>
        /// Initializes a new DataMappingPair object
        /// </summary>
        public DataMappingPair()
        {
        }

        /// <summary>
        /// Initializes a new DataMappingPair object
        /// </summary>
        /// <param name="internalName"></param>
        /// <param name="externalName"></param>
        public DataMappingPair(string internalName, string externalName)
        {
            InternalName = internalName;
            ExternalName = externalName;
        }

        /// <summary>
        /// Gets or sets the internal name for the current DataMappingPair object.
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the external name for the current DataMappingPair object.
        /// </summary>
        public string ExternalName { get; set; }
    }

    /// <summary>
    /// Defines a mapping between database and internal property names.
    /// </summary>
    /// <remarks>
    /// Datamappings allow specification of one-to-one and one-to-many property name
    /// mappings.
    /// <para>
    /// Typically, unrefererenced internal properties will not be assigned to and
    /// unreferenced external properties will be ignored. It is often valid to
    /// assign to arbitrary internal property ("custom") names, although this
    /// is not functionality implemented by the DataMapping class, which simply
    /// provides the mapping mechanism.
    /// </para>
    /// </remarks>
    [TypeConverter(typeof(DataMapping.Converter))]
    public sealed class DataMapping : ObservableCollection<DataMappingPair>
    {
        #region Converter
        /// <summary>
        /// Converts instances of other types to and from a DataMapping. 
        /// </summary>
        /// <remarks>
        /// The only conversion type supported by DataMapping.Converter is to
        /// and from string.
        /// </remarks>
        public class Converter : TypeConverter
        {
            /// <summary>
            /// Returns whether the type converter can convert an object from the specified
            /// type to the type of this converter.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="sourceType"></param>
            /// <returns></returns>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            /// <summary>
            /// Converts from the specified value to the type of this converter. 
            /// </summary>
            /// <param name="context">An object that provides a format context.</param>
            /// <param name="culture">The CultureInfo to use as the current culture. </param>
            /// <param name="value">The value to convert to the type of this converter.</param>
            /// <returns>The converted value.</returns>
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                string stringValue = value as string;
                if (stringValue != null)
                {
                    DataMapping dataMapping = new DataMapping();

                    foreach (string pair in stringValue.Split(';'))
                    {
                        string[] p = pair.Split('=');


                        if (p.Length == 2)
                        {
                            p[0] = p[0].Trim();
                            p[1] = p[1].Trim();

                            if (string.IsNullOrEmpty(p[1]))
                            {
                                throw new InvalidOperationException("Cannot map from empty external name");
                            }

                            foreach (string name in p[0].Split(','))
                            {
                                string internalName = name.Trim();

                                if (internalName.Equals(""))
                                {
                                    throw new InvalidOperationException("Cannot map to an empty external name: " + pair);
                                }

                                dataMapping.Add(new DataMappingPair(internalName, p[1]));
                            }
                        }
                    }

                    return dataMapping;
                }
                else
                {
                    return base.ConvertFrom(context, culture, value);
                }
            }
            /// <summary>
            /// Returns whether the type converter can convert an object to the specified type.
            /// </summary>
            /// <param name="context">An object that provides a format context.</param>
            /// <param name="destinationType">The type you want to convert to.</param>
            /// <returns>
            /// true if this converter can perform the conversion; otherwise, false.
            /// </returns>
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
            }
            /// <summary>
            /// Converts the specified value object to the specified type.
            /// </summary>
            /// <param name="context">An object that provides a format context.</param>
            /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
            /// <param name="value">The object to convert.</param>
            /// <param name="destinationType">The type to convert the object to.</param>
            /// <returns>The converted object.</returns>
            /// <exception cref="T:System.NotImplementedException">
            /// 	<see cref="M:System.ComponentModel.TypeConverter.ConvertTo(System.ComponentModel.ITypeDescriptorContext,System.Globalization.CultureInfo,System.Object,System.Type)"/>  not implemented in base <see cref="T:System.ComponentModel.TypeConverter"/>.</exception>
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    string str = "";
                    DataMapping dataMapping = value as DataMapping;

                    if (dataMapping != null)
                    {
                        foreach (DataMappingPair pair in dataMapping)
                        {
                            str += pair.InternalName + "=" + pair.ExternalName + "; ";
                        }
                    }

                    return str;
                }
                else
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
            }
        }
        #endregion

        /// <summary>
        /// Attempts to safely add a new datamapping pair to the current datamapping object.
        /// </summary>
        /// <param name="internalName"></param>
        /// <param name="externalName"></param>
        /// <returns>True if the mapping pair was successfully added.</returns>
        /// <remarks>
        /// Datamappings cannot contain null internal or external names, or duplicate
        /// internal names.
        /// </remarks>
        public bool TryAdd(string internalName, string externalName)
        {
            return TryAdd(new DataMappingPair(internalName, externalName));
        }

        /// <summary>
        /// Attempts to safely add a new datamapping pair to the current datamapping object.
        /// </summary>
        /// <param name="pair"></param>
        /// <returns>True if the mapping pair was successfully added.</returns>
        /// <remarks>
        /// A Datamapping should not attempt to map multiple external names
        /// to a single internal name.
        /// </remarks>
        public bool TryAdd(DataMappingPair pair)
        {
            if (pair.InternalName != null && pair.ExternalName != null && !InternalToExternal.ContainsKey(pair.InternalName))
            {
                Add(pair);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all external names mapped by the current datamapping object.
        /// </summary>
        /// <returns>Enumerable (never null) of external names.</returns>
        public IEnumerable<string> GetExternalNames()
        {
            return ExternalToInternal.Keys;
        }

        /// <summary>
        /// Gets an enumerable of external names which map to the specified internal name
        /// </summary>
        /// <param name="internalName"></param>
        /// <returns>Enumerable (may be null) of external names.</returns>
        public string GetExternalName(string internalName)
        {
            string externalName = null;

            InternalToExternal.TryGetValue(internalName, out externalName);

            return externalName;
        }

        /// <summary>
        /// Gets all internal names mapped by the current datamapping object.
        /// </summary>
        /// <returns>Enumerable (never null) of internal names.</returns>
        public IEnumerable<string> GetInternalNames()
        {
            return InternalToExternal.Keys;
        }

        /// <summary>
        /// Gets an enumerable of internal names which are mapped to by the specified
        /// external name
        /// </summary>
        /// <param name="externalName"></param>
        /// <returns>Enumerable (may be null) of internal names.</returns>
        public IEnumerable<string> GetInternalNames(string externalName)
        {
            IList<string> internalNames = null;
            ExternalToInternal.TryGetValue(externalName, out internalNames);

            return internalNames;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided event data.
        /// </summary>
        /// <param name="e">The event data to report in the event.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            internalToExternal = null;
            externalToInternal = null;

            base.OnCollectionChanged(e);
        }

        private Dictionary<string, string> InternalToExternal
        {
            get
            {
                if (internalToExternal == null)
                {
                    internalToExternal = new Dictionary<string, string>();

                    foreach (DataMappingPair child in this)
                    {
                        internalToExternal[child.InternalName] = child.ExternalName;
                    }
                }

                return internalToExternal;
            }
        }
        private Dictionary<string, string> internalToExternal;

        private Dictionary<string, IList<string>> ExternalToInternal
        {
            get
            {
                IList<string> internalNames = null;

                if (externalToInternal == null)
                {
                    externalToInternal = new Dictionary<string, IList<string>>();

                    foreach (DataMappingPair child in this)
                    {
                        if (!externalToInternal.TryGetValue(child.ExternalName, out internalNames))
                        {
                            internalNames = new List<string>();
                            externalToInternal[child.ExternalName] = internalNames;
                        }

                        internalNames.Add(child.InternalName);
                    }
                }

                return externalToInternal;
            }
        }
        private Dictionary<string, IList<string>> externalToInternal;
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