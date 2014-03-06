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

namespace Infragistics.Controls
{
	// AS 10/25/10 TFS58250
	// There is a bug in VS when you define an event where the event args is a generic class and the T 
	// you use for the concrete implementation is itself a generic type. In this case the event was a 
	// RoutedPropertyChangedEventArgs<DateRange?> which is just short hand for 
	// RoutedPropertyChangedEventArgs<Nullable<DateRange>>.
	//
	/// <summary>
	/// Custom event arguments for a property change event involving a Nullable&lt;T&gt;
	/// </summary>
	/// <typeparam name="T">The value type for the nullable property change</typeparam>
	public class NullableRoutedPropertyChangedEventArgs<T> : RoutedPropertyChangedEventArgs<T?>
		where T : struct
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="NullableRoutedPropertyChangedEventArgs&lt;T&gt;"/>
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		public NullableRoutedPropertyChangedEventArgs( T? oldValue, T? newValue )
			: base(oldValue, newValue)
		{
		}
		#endregion // Constructor
	}

	/// <summary>
	/// Delegate used to represent a method that will handle a property change involving a nullable property.
	/// </summary>
	/// <typeparam name="T">The underlying type of property value for the event</typeparam>
	/// <param name="sender">The object for which the event is being raised.</param>
	/// <param name="e">The event arguments that provides additional information about the property change.</param>
	public delegate void NullableRoutedPropertyChangedEventHandler<T>( object sender, NullableRoutedPropertyChangedEventArgs<T> e ) where T : struct;
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