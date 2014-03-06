using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel.Serialization

{
	internal abstract class BiffRecordBase<RecordTypeEnum, SerializationManagerType> 
		where SerializationManagerType : WorkbookSerializationManager
	{
		public abstract void Load( SerializationManagerType manager );
		public abstract void Save( SerializationManagerType manager );

		public abstract RecordTypeEnum Type { get; }

		// MD 4/18/08 - BR32154
		// This variable may be accessed by multiple threads. Since it is just used for caching, instead of using locks, we can just use
		// a different collection for each thread by putting the ThreadStatic attribute on it. However, if we do this, we cannot create
		// the collection at the definition, because the static constructor is only called on the first thread that accesses the class.
		// Additional threads would have a null collection. Therefore, we need to lazily create the collection in a property. All references
		// to the static field have been replaced with references to the static property.
		//private static Dictionary<BIFFType, BiffRecordBase> records = new Dictionary<BIFFType, BiffRecordBase>();
		[ThreadStatic]
		private static Dictionary<RecordTypeEnum, BiffRecordBase<RecordTypeEnum, SerializationManagerType>> records;
		private static Dictionary<RecordTypeEnum, BiffRecordBase<RecordTypeEnum, SerializationManagerType>> Records
		{
			get
			{
				if ( BiffRecordBase<RecordTypeEnum, SerializationManagerType>.records == null )
				{
					BiffRecordBase<RecordTypeEnum, SerializationManagerType>.records = 
						new Dictionary<RecordTypeEnum, BiffRecordBase<RecordTypeEnum, SerializationManagerType>>();
				}

				return BiffRecordBase<RecordTypeEnum, SerializationManagerType>.records;
			}
		}

		public delegate BiffRecordBase<RecordTypeEnum, SerializationManagerType> CreateBiffRecordDelegate( RecordTypeEnum type );
		public static BiffRecordBase<RecordTypeEnum, SerializationManagerType> GetBiffRecord( RecordTypeEnum type, CreateBiffRecordDelegate createBiffRecord )
		{
			BiffRecordBase<RecordTypeEnum, SerializationManagerType> record;

			// MD 4/18/08 - BR32154
			// Use property instead of field. See notes on field.
			//if ( records.TryGetValue( type, out record ) )
			if ( BiffRecordBase<RecordTypeEnum, SerializationManagerType>.Records.TryGetValue( type, out record ) )
				return record;

			record = createBiffRecord( type );

			// MD 4/18/08 - BR32154
			// Use property instead of field. See notes on field.
			//records.Add( type, record );
			BiffRecordBase<RecordTypeEnum, SerializationManagerType>.Records.Add( type, record );

			return record;
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