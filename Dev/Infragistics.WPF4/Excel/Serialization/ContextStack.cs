using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization
{






	internal class ContextStack
	{
		#region Member Variables

		private List<object> contexts;

		#endregion Member Variables

		#region Constructor

		public ContextStack()
		{
			this.contexts = new List<object>();
		}

		#endregion Constructor

		#region Methods

		// MD 6/20/08 - Excel 2007 Format
		#region ClearToCount

		public void ClearToCount( int newCount )
		{
			while ( this.Count > newCount )
				this.Pop();
		} 

		#endregion ClearToCount

		// MD 3/16/12 - 12.1 - Table Support
		#region Get

		public T Get<T>()
		{
			// MD 7/2/12 - TFS115692
			// If there is no item of the type and the type specified is a value type, test for null so we don't throw an exception.
			//return (T)this[typeof(T)];
			object value = this[typeof(T)];
			if (value == null)
				return default(T);

			return (T)value;
		}

		#endregion // Get

		// MD 7/15/11 - Shape support
		#region ReplaceItem

		public void ReplaceItem(object instance, object replacement)
		{
			int index = this.contexts.LastIndexOf(instance);

			if (index < 0)
			{
				Utilities.DebugFail("Cannot find the item to replace.");
				return;
			}

			this.contexts[index] = replacement;
		}

		#endregion // ReplaceItem

		#region Pop

		public object Pop()
		{
			object context = this.Current;

			this.contexts.RemoveAt( this.contexts.Count - 1 );

			return context;
		}

		#endregion Pop

		#region Push

		public void Push( object context )
		{
			this.contexts.Add( context );
		}

		#endregion Push

		#endregion Methods

		#region Properties

		// MD 6/20/08 - Excel 2007 Format
		#region Count

		public int Count
		{
			get { return this.contexts.Count; }
		} 

		#endregion Count

		#region Current

		public object Current
		{
			get
			{
				if ( this.contexts.Count == 0 )
					return null;

				return this.contexts[ this.contexts.Count - 1 ];
			}
		}

		#endregion Current

		#region Indexer[ Type ]

		public object this[ Type type ]
		{
			// MD 5/16/07 - BR23017
			// Moved all code to the new overload
			get { return this[ type, 0 ]; }
		}

		#endregion Indexer[ Type ]

		// MD 5/16/07 - BR23017
		#region Indexer[ Type, int ]

		public object this[ Type type, int level ]
		{
			get
			{
				if ( type == null )
				{
                    Utilities.DebugFail( "A null type is not allowed" );
					return null;
				}

				bool allowNulls = type == typeof( object );

				for ( int i = this.contexts.Count - 1; i >= 0; i-- )
				{
					object context = this.contexts[ i ];

					// MD 6/11/07 - BR23706
					// The wrong check was being made here
					//if ( allowNulls && contexts == null )
					if ( allowNulls && context == null )
						return null;

					if ( type.IsInstanceOfType( context ) )
					{
						// MD 5/16/07 - BR23017
						// We might not want to get the first one of a type anymore
						//return context;
						if ( level == 0 )
							return context;

						level--;
					}
				}

				return null;
			}
		}

		#endregion Indexer[ Type, int ]

		#endregion Properties
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