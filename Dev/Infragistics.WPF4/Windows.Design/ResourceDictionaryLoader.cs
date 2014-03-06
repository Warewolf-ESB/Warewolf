using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;


using Infragistics.Windows.Design.SmartTagFramework;


namespace Infragistics.Windows.Design
{
	/// <summary>
	/// ResourceDictionary that conditionally merges SmartTagFramework resource dictionaries for designer 4.0 projects only.
	/// </summary>
	public class ResourceDictionaryLoader : ResourceDictionary, ISupportInitialize
	{
		#region Member Variables


		private bool			_initializing;


		#endregion //Member Variables

		#region Methods

			#region MergeSelectedDictionaries

		private void MergeSelectedDictionaries()
		{

			if (this._initializing)
				return;

			this.Clear();
			this.MergedDictionaries.Clear();

			ResourceDictionary rd	= new ResourceDictionary();
			rd.Source				= ContentLocators.UriDesignerActionItemTemplatesXaml;
			this.MergedDictionaries.Add(rd);

			rd			= new ResourceDictionary();
			rd.Source	= ContentLocators.UriExpanderXaml;
			this.MergedDictionaries.Add(rd);

			rd			= new ResourceDictionary();
			rd.Source	= ContentLocators.UriPropertyEditorsXaml;
			this.MergedDictionaries.Add(rd);

		}

			#endregion //MergeSelectedDictionaries

		#endregion //Methods

		#region ISupportInitialize Members

		/// <summary>
		/// Starts the initialization of the dictionary.
		/// </summary>
		public new void BeginInit()
		{

			this._initializing = true;


			base.BeginInit();
		}

		/// <summary>
		/// Ends the initialization of the dictionary.
		/// </summary>
		public new void EndInit()
		{

			this._initializing = false;

			this.MergeSelectedDictionaries();


			base.EndInit();
		}

		#endregion //ISupportInitialize
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