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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Persistence.Primitives
{
	/// <summary>
	/// An object that acts as a serializable container for storing information of an object. 
	/// </summary>
	public class PropertyData
	{
        /// <summary>
        /// Initializes the members of the <see cref="PropertyData"/>
        /// </summary>
        public PropertyData()
        {
            this.LookUpKeys = new Collection<string>();
            this.Properties = new Collection<PropertyDataInfo>();
        }

		/// <summary>
		/// Gets/sets the AssemblyQualifiedName of a TypeConverter that can be used to Convert a string back to an object.
		/// </summary>
		public string ConverterTypeName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the lookup key for the type of property this object is. 
		/// </summary>
		public int AssemblyTypeKey
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the actual value of a property. 
		/// </summary>
		/// <remarks>
		/// This property is only used for properties that are Value types.
		/// </remarks>
		public object Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets a string representation of a property.
		/// </summary>
		public string StringValue
		{
			get;
			set;
		}


        /// <summary>
        /// Gets/sets a collection of string keys that should be used to repopulate a collection.
        /// </summary>
        public Collection<string> LookUpKeys
        {
            get;
            set;
        }

		/// <summary>
		/// Gets/sets a list of property information for all subproperties of an object.
		/// </summary>
		public Collection<PropertyDataInfo> Properties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets an identifier that can be used to identify an property being loaded. 
		/// </summary>
		/// <remarks>
		/// If specified, during loading the framework will attempt to load an object from the Application.Current.Resources using this Identifier. 
		/// Otherwise, it may be used by the end developer to identify a specific property in an a event. 
		/// </remarks>
		public string Identifier
		{
			get;
			set;
		}
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