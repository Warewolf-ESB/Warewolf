using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Collections.Specialized;




namespace Infragistics.Collections

{
	#region TypedEnumerable<T> Class

	/// <summary>
	/// Typed enumerable that wraps a non-typed enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TypedEnumerable<T> : IEnumerable<T>
	{
		#region TypedEnumerator Class

		internal class Enumerator : IEnumerator<T>
		{
			private IEnumerator _e;

			public Enumerator( IEnumerator e )
			{
				_e = e;
			}

			public void Reset( )
			{
				_e.Reset( );
			}

			public bool MoveNext( )
			{
				return _e.MoveNext( );
			}

			public T Current
			{
				get
				{
					return (T)_e.Current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public void Dispose( )
			{
			}
		} 

		#endregion // TypedEnumerator Class

		private IEnumerable _e;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="e">Enumerable</param>
		public TypedEnumerable( IEnumerable e )
		{
			// AS 8/5/09 NA 2009.2 Field Sizing
			//_e = e;
			_e = e ?? new T[0];
		}

		// SSP 4/21/11 TFS73037
		// 
		internal void ResetSourceEnumerable( IEnumerable e )
		{
			_e = e ?? new T[0];
		}

		private IEnumerator<T> GetEnumerator( )
		{
			return new Enumerator( _e.GetEnumerator( ) );
		}

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}
	} 

	#endregion // TypedEnumerable<T> Class
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