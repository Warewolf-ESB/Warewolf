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
using System.Globalization;

namespace Infragistics.Windows.Editors
{
	#region SectionsCollection Class

	/// <summary>
	/// Read-only collection that contains <see cref="SectionBase"/> derived class instances.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// XamMaskedEditor creates this collection when it parses its mask. XamMaskedEditor's
	/// <see cref="XamMaskedEditor.Sections"/> property returns this collection.
	/// See <see cref="XamMaskedEditor.Sections"/> for more information.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need for you to directly create or use this class as
	/// the XamMaskedEditor automatically creates and manages this when it parses the mask.
	/// </para>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// <seealso cref="SectionBase.DisplayChars"/>
	/// <seealso cref="XamMaskedEditor.DisplayChars"/>
	/// </remarks>
	// SSP 12/9/10 TFS53479
	// Apparently not implementing INotifyCollectionChanged causes memory leak in framework's ViewManager.
	// 
	//public class SectionsCollection : ReadOnlyCollection<SectionBase>
	public class SectionsCollection : ReadOnlyObservableCollection<SectionBase>
	{
		private MaskInfo	   maskInfo;

		// SSP 3/1/12 TFS92791
		// 
		internal bool _isCacheInvalid;






		internal SectionsCollection( )
			// SSP 12/9/10 TFS53479
			// Related to above.
			// 
			//: base( new List<SectionBase>( ) )
			: base( new ObservableCollection<SectionBase>( ) )
		{
		}

		internal void Initialize( MaskInfo maskInfo )
		{
			this.maskInfo = maskInfo;
		}

		internal MaskInfo MaskInfo
		{
			get
			{
				return this.maskInfo;
			}
		}

		internal XamMaskedEditor MaskedEditor
		{
			get
			{
				return this.maskInfo.MaskEditor;
			}
		}
		
		internal void Add( SectionBase section )
		{
			this.Items.Add( section );
			section.InitSectionsCollection( this );
		}

		private void CopyCharacterValues( DisplayCharsCollection dest, DisplayCharsCollection src )
		{
			for ( int i = 0; i < src.Count; i++ )
			{
				DisplayCharBase dc = dest[i];
				if ( dc.IsEditable )
					dc.Char = src[i].Char;
			}
		}

		internal SectionsCollection Clone( MaskInfo maskInfo, bool copyDisplayCharacterValues )
		{
			SectionsCollection clone = new SectionsCollection( );
			clone.Initialize( maskInfo );

			foreach ( SectionBase section in this )
			{
				SectionBase cloneSection = section.InternalClone( clone );
				clone.Add( cloneSection );

				if ( copyDisplayCharacterValues )
					this.CopyCharacterValues( cloneSection.DisplayChars, section.DisplayChars );
			}

			return clone;
		}

		internal IEnumerable AllDisplayCharacters
		{
			get
			{
				return new AllDisplayCharacterEnumerable( this );
			}
		}

        // AS 8/25/08 Support Calendar
        // Centralize the access of the sections to the formatprovider.
        //
        internal IFormatProvider FormatProvider
        {
            get
            {
                return null != this.maskInfo ? this.maskInfo.FormatProvider : System.Globalization.CultureInfo.CurrentCulture;
            }
        }

        // AS 8/25/08 Support Calendar
        internal System.Globalization.Calendar Calendar
        {
            get
            {
                return XamMaskedEditor.GetCultureCalendar(this.FormatProvider);
            }
        }

		private class AllDisplayCharacterEnumerable : IEnumerable
		{
			private SectionsCollection _sections;

			internal AllDisplayCharacterEnumerable( SectionsCollection sections )
			{
				_sections = sections;
			}

			public IEnumerator GetEnumerator( )
			{
				return new AllDisplayCharacterEnumerator( _sections );
			}
		}

		private class AllDisplayCharacterEnumerator : IEnumerator 
		{
			private SectionsCollection _sections;
			private DisplayCharBase _current;

			internal AllDisplayCharacterEnumerator( SectionsCollection sections )
			{
				_sections = sections;
			}

			public void Reset( )
			{
				_current = null;
			}

			public bool MoveNext( )
			{
				if ( null == _current )
				{
					_current = null != _sections && _sections.Count > 0 ? _sections[0].FirstDisplayChar : null;
				}
				else
				{
					_current = _current.NextDisplayChar;
				}

				return null != _current;
			}

			public object Current
			{
				get
				{
					return _current;
				}
			}
		}
	}

	#endregion // SectionsCollection Class

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