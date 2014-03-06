using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Editors
{
	#region DisplayCharsCollection Class

	/// <summary>
	/// Read-only collection that contains <see cref="DisplayCharBase"/> derived class instances.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Each section exposes a <see cref="SectionBase.DisplayChars"/> property that returns the
	/// display characters of that section. The XamMaskedEditor's <see cref="XamMaskedEditor.DisplayChars"/>
	/// property also returns a DisplayCharsCollection that contains all the display characters of 
	/// all the mask sections in the editor.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use this class as
	/// the XamMaskedEditor automatically creates and manages this when it parses the mask.
	/// </para>
	/// <para class="body">
	/// See <see cref="XamMaskedEditor.Sections"/> for more information.
	/// </para>
	/// <seealso cref="SectionBase.DisplayChars"/>
	/// <seealso cref="XamMaskedEditor.DisplayChars"/>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// </remarks>
	// SSP 12/9/10 TFS53479
	// Apparently not implementing INotifyCollectionChanged causes memory leak in framework's ViewManager.
	// 
	//public class DisplayCharsCollection : ReadOnlyCollection<DisplayCharBase>
	public class DisplayCharsCollection : ReadOnlyObservableCollection<DisplayCharBase>
	{
		private SectionBase section;






		internal DisplayCharsCollection( ) : this( null )
		{
		}







		internal DisplayCharsCollection( SectionBase section )
			// SSP 12/9/10 TFS53479
			// Related to above.
			// 
			//: base( new List<DisplayCharBase>( ) )
			: base( new ObservableCollection<DisplayCharBase>( ) )
		{
			this.section = section;
		}

		/// <summary>
		/// Returns the associated section object.
		/// </summary>
		public SectionBase Section
		{
			get
			{
				return this.section;
			}
		}

		internal void Add( DisplayCharBase displayChar )
		{
			this.Items.Add( displayChar );
		}

		// SSP 2/29/12 TFS92791
		// 
		internal void Insert( int index, DisplayCharBase displayChar )
		{
			this.Items.Insert( index, displayChar );
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal DisplayCharsCollection InternalClone( SectionBase section )
		{
			DisplayCharsCollection clone = new DisplayCharsCollection( section );

			for ( int i = 0; i < this.Count; i++ )
			{
				clone.Add( this[i].InternalClone( section ) );
			}

			return clone;
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		internal void Clear( )
		{
			this.Items.Clear( );
		}

	}

	#endregion // DisplayCharsCollection Class

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