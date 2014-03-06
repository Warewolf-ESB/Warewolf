using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.Themes
{
	/// <summary>
	/// Custom resource dictionary that loads based on a specified <see cref="ResourceSetLocator"/>
	/// </summary>
	public class ResourceSetLoader : ResourceDictionary, ISupportInitialize
	{
		#region Member Variables

		private bool _initializing;
		private ResourceSetLocator _locator;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ResourceSetLoader"/>
		/// </summary>
		public ResourceSetLoader()
		{
		}

		internal ResourceSetLoader(ResourceSetLocator locator)
		{
			this.Locator = locator;
		}
		#endregion //Constructor

		#region Properties

		#region Locator

		/// <summary>
		/// Gets/sets adictionary that contains the resources to clone. 
		/// </summary>
		//[Description("A 'ResourceSetLocator' that identifies the resources to load.")]
		//[Category("Data")]
		[DefaultValue(null)]
		public ResourceSetLocator Locator
		{
			get { return this._locator; }
			set
			{
				if (value != this._locator)
				{
					// AS 12/3/07
					Utilities.VerifyCanBeModified(this);

					this._locator = value;
					this.OnCriteriaChanged();
				}
			}
		}

		#endregion //Locator

		#endregion //Properties

		#region Methods

		#region OnCriteriaChanged

		private void OnCriteriaChanged()
		{
			if (this._initializing)
				return;

			this.Clear();
			this.MergedDictionaries.Clear();

			// if we don't have a source dictionary then just return
			if (this._locator == null)
				return;

			ResourceDictionary rd = Utilities.CreateResourceSetDictionary(this._locator.Assembly, this._locator.ResourcePath);
			this.MergedDictionaries.Add(rd);

			// make sure any resourcewashers with autowash set to false are washed
			ResourceWasher.ForceAutoWashResources(this);
		}
		#endregion //OnCriteriaChanged

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

			this.OnCriteriaChanged();

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