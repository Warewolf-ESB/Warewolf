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

namespace Infragistics
{
	/// <summary>
	/// IEqualityComparer implementation used to compare DataKeys
	/// </summary>
	internal class DataKeyComparer : IEqualityComparer<object>
	{
		#region IEqualityComparer<object> Members

		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			if (x == null && y == null) 
				return true;

			if (x == null || y == null)
				return false;

			ISupportDataKeys xDataKey = x as ISupportDataKeys;
			ISupportDataKeys yDataKey = y as ISupportDataKeys;

			if (xDataKey != null && yDataKey != null)
			{
				object[] xDataKeys = xDataKey.GetDataKeys();
				object[] yDataKeys = yDataKey.GetDataKeys();
				if (xDataKeys == null && yDataKeys == null) 
					return true;
				if (xDataKeys == null || yDataKeys == null) 
					return false;
				if (xDataKeys.Length != yDataKeys.Length) 
					return false;
				for (int i = 0; i < xDataKeys.Length; i++)
				{
					if (xDataKeys[i] != yDataKeys[i]) 
						return false;
				}
				return true;
			}

            
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            return x.Equals(y);
            //return x.GetHashCode() == y.GetHashCode();
		}

		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			ISupportDataKeys dataKeySupport = obj as ISupportDataKeys;

			if (dataKeySupport != null )
			{
				object[] dataKeys = dataKeySupport.GetDataKeys();

				if (dataKeys != null && dataKeys.Length > 0)
				{

					return GenerateHashCodeFromDataKey(dataKeys, dataKeys.Length );
				}
			}

			return obj.GetHashCode();
		}

		#endregion

		#region Methods
		private static int GenerateHashCodeFromDataKey(object[] keys, int length)
		{
			if (length == 0)
				return 0;

			int i = length - 1;

			return (keys[i] == null ? 0 : keys[i].GetHashCode()) ^ GenerateHashCodeFromDataKey(keys, i);
		}
		#endregion // Methods


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