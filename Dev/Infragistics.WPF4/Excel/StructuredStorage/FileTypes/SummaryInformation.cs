using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.StructuredStorage.FileTypes

{
	internal class SummaryInformation : InformationBase
	{
		#region Member Variables

		private SummaryInformationProperties properties;

		#endregion Member Variables

		#region Base Class Overrides

		#region DefaultSectionId

		protected override string DefaultSectionId
		{
			get { return "f29f85e04ff91068ab9108002b27b3d9"; }
		}

		#endregion DefaultSectionId

		#endregion Base Class Overrides

		#region Properties

		public SummaryInformationProperties Properties
		{
			get
			{
				if ( this.properties == null )
					this.properties = new SummaryInformationProperties( this );

				return this.properties;
			}
		}

		#endregion Properties


		#region SummaryInformationProperties class

		public class SummaryInformationProperties
		{
			private SummaryInformation summaryInformation;

			#region Constructor

			public SummaryInformationProperties( SummaryInformation summaryInformation )
			{
				this.summaryInformation = summaryInformation;
			}

			#endregion Constructor

			#region VerifyPropertyType

			private static bool VerifyPropertyType( SummaryPropertyType propertyType, Type valueType )
			{
				// If this is the Code Page property, never allow it to be set
				if ( 1 == (int)propertyType )
					return false;

				switch ( propertyType )
				{
					case SummaryPropertyType.Title:
					case SummaryPropertyType.Subject:
					case SummaryPropertyType.Author:
					case SummaryPropertyType.Keywords:
					case SummaryPropertyType.Comments:
					case SummaryPropertyType.Template:
					case SummaryPropertyType.LastSavedBy:
					case SummaryPropertyType.RevisionNumber:
					case SummaryPropertyType.NameOfCreatingApplication:
						return valueType == typeof( string );

					case SummaryPropertyType.TotalEditingTime:
					case SummaryPropertyType.LastPrinted:
					case SummaryPropertyType.CreatedDateTime:
					case SummaryPropertyType.LastSavedDateTime:
						return valueType == typeof( DateTime );

					case SummaryPropertyType.NumberOfPages:
					case SummaryPropertyType.NumberOfWords:
					case SummaryPropertyType.NumberOfCharacters:
					case SummaryPropertyType.Security:
						return valueType == typeof( int );

					case SummaryPropertyType.Thumbnail:
						return false;

					default:
						return true;
				}
			}

			#endregion VerifyPropertyType

			#region Indexer [ SummaryPropertyType ]

			public object this[ SummaryPropertyType type ]
			{
				get 
				{
					int key = (int)type;

					if ( this.summaryInformation.DefaultProperties.ContainsKey( key ) == false )
						return null;

					return this.summaryInformation.DefaultProperties[ key ]; 
				}
				set
				{
					int key = (int)type;

					// If the value is nul or DBNull, remove the property from the info class
					if ( value == null || value == DBNull.Value )
					{
						if ( this.summaryInformation.DefaultProperties.ContainsKey( key ) )
							this.summaryInformation.DefaultProperties.Remove( key );
					}
					else
					{
						if ( VerifyPropertyType( type, value.GetType() ) == false )
						{
							Utilities.DebugFail( "Invaid type of value for the property." );
							return;
						}

						// Set the value of the property
						if ( this.summaryInformation.DefaultProperties.ContainsKey( key ) )
							this.summaryInformation.DefaultProperties[ key ] = value;
						else
							this.summaryInformation.DefaultProperties.Add( key, value );
					}
				}
			}

			#endregion Indexer [ SummaryPropertyType ]
		}

		#endregion SummaryInformationProperties class
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