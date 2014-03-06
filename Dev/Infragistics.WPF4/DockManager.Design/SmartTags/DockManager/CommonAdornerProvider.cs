using System;
using System.Reflection;
using System.Windows;
using Infragistics.Windows.Design.SmartTagFramework;
using Infragistics.Windows.DockManager;


namespace Infragistics.Windows.Design.DockManager
{
    /// <summary>
    /// The CustomGenericAdornerProvider class implements an adorner provider for the 
    /// adorned control. The adorner is a edit control, which changes some properties of the adorned control.
    /// </summary>
    public class CommonAdornerProvider : GenericAdornerProvider
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of CustomGenericAdornerProvider
        /// </summary>
		public CommonAdornerProvider()
        {
            string assemblyName 	= Assembly.GetExecutingAssembly().GetName().Name;
            string resourceFileName = @"SmartTags/DockManager/CustomEditors/CustomPropertyEditors.xaml";
            string packUri 			= String.Format(@"/{0};component/{1}", assemblyName, resourceFileName);
            Uri uri 				= new Uri(packUri, UriKind.Relative);

            this.CustomPropertyEditors = Application.LoadComponent(uri) as ResourceDictionary;
        }

        #endregion //Constructors

        #region Base Class Overrides

			#region GetDesignerActionListTypeFromControlType

        /// <summary>
        /// A method wich is used for association between predefined DesignerActionList and an Infragistics control
        /// </summary>
        /// <param name="controlType">A control type</param>
        /// <returns>Predefined DesignerActionList type</returns>
        public override Type GetDesignerActionListTypeFromControlType(Type controlType)
        {
            return Association.GetCorrespondingType(controlType);
        }

			#endregion //GetDesignerActionListTypeFromControlType	
    
			#region SupportedItemTypes

		/// <summary>
		/// Returns an array of the item types that are supported by the Adorner.
		/// </summary>
		protected override Type[] SupportedItemTypes
		{
			get { return new Type [] { typeof(ContentPane),
										typeof(TabGroupPane),
										typeof(XamDockManager),
									};
			}
		}

			#endregion //SupportedItemTypes
    
        #endregion //Base Class Overrides
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