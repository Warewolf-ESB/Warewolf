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
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Persistence
{
	/// <summary>
	/// A base object that can be used to identify a Property.
	/// </summary>
	public abstract class PropertyPersistenceInfoBase
	{
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

		/// <summary>
		/// Gets/sets a string representation of the value for a property that will be used to store the property.
		/// </summary>
		public string SaveValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the a TypeConverter that can be used to Convert an object to and from a string.
		/// </summary>
		public TypeConverter TypeConverter
		{
			get;
			set;
		}

		/// <summary>
		/// When overridden on a derived class, this method should be used to determine if the specified property matches
		/// the criteria given.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		/// <returns></returns>
		public abstract bool DoesPropertyMeetCriteria(PropertyInfo pi, string propertyPath);

		/// <summary>
		/// A method that when overridden on a derived class, can be used to build a string version of the value that should be saved.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual string SaveValueToString(PropertyInfo pi, string propertyPath, object value)
		{
			return null;		
		}

		/// <summary>
		/// A method that when overridden on a derived class, can be used to load a object from a string.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="propertyPath"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual object LoadObjectFromString(PropertyInfo pi, string propertyPath, string value)
		{
			return null;
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