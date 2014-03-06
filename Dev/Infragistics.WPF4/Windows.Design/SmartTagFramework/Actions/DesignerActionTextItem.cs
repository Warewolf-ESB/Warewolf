using System.Windows;
using System.Windows.Media;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Represents a static text item on a smart tag panel. 
    /// </summary>
    public class DesignerActionTextItem : DesignerActionItem
    {
        #region MemberVariables

        private FontWeight				_fontWeight;
		private Brush					_foreground = Brushes.Black;

        #endregion //MemberVariables

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionTextItem class. 
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>        
        public DesignerActionTextItem(string displayName)
            : base(displayName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionTextItem class. 
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>        
        public DesignerActionTextItem(string displayName, DesignerActionItemGroup designerActionItemGroup)
            : base(displayName, null, designerActionItemGroup)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionTextItem class. 
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>        
        /// <param name="fontWeight">FontWeight parameter.</param>
        public DesignerActionTextItem(string displayName, DesignerActionItemGroup designerActionItemGroup, FontWeight fontWeight)
            : base(displayName, null, designerActionItemGroup)
        {
            this._fontWeight = fontWeight;
        }

        /// <summary>
        /// Initializes a new instance of the DesignerActionTextItem class. 
        /// </summary>
        /// <param name="displayName">The panel text for this item.</param>
        /// <param name="designerActionItemGroup">An object that defines the groupings of panel entries.</param>        
        /// <param name="fontWeight">Specifies the FontWeight to use for the text.</param>
		/// <param name="foreground">Specifies the Foreground to use for the text.</param>
		/// <param name="orderNumber">Specifies the order in a group</param>
        public DesignerActionTextItem(string displayName, DesignerActionItemGroup designerActionItemGroup, FontWeight fontWeight, Brush foreground, int orderNumber)
            : base(displayName, null, designerActionItemGroup, orderNumber)
        {
            this._fontWeight	= fontWeight;
			this._foreground	= foreground;
        }

        #endregion //Constructors

        #region Properties

        #region Public Properties

        #region FontWeight

        /// <summary>
        /// Gets, sets a FontWeight property 
        /// </summary>
        public FontWeight FontWeight
        {
            get
            {
                return this._fontWeight;
            }

            set
            {
                this._fontWeight = value;
            }
        }

        #endregion //FontWeight

		#region Foreground

		/// <summary>
		/// Gets, sets a Foreground property 
		/// </summary>
		public Brush Foreground
		{
			get
			{
				return this._foreground;
			}

			set
			{
				this._foreground = value;
			}
		}

		#endregion //Foreground

		#endregion //Public Properties

		#endregion //Properties
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