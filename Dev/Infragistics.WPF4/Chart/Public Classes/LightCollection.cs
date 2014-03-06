
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Media3D;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Represents a collection of lights. Light is abstract class and 
    /// model3D object that represents lighting applied to a 3-D scene. 
    /// Light could be: Ambient, Directional, Point and Spot light.  
    /// </summary>
    /// <remarks>
    /// For 3D charts, by default, a light effect is created with default values (one DirectionalLight object, 
    /// and one PointLight object). However, if you want to change the Light effect for the 3D scene, you need 
    /// to create a Light effect and add it to the Lights collection. When you add a Light effect to the collection, 
    /// the default Light effect disappears, and only newly created Light effects can be used. 
    /// </remarks>
    public class LightCollection : ObservableCollection<Light>
    {
        #region Fields

        // Private fields
        private object _chartParent;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the LightCollection class. 
        /// </summary>
        public LightCollection()
        {
        }
        
        /// <summary>
        /// Raises the CollectionChanged event with the provided arguments. 
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Refresh chart
            XamChart control = XamChart.GetControl(this);
            if (control != null)
            {
                control.RefreshProperty();
            }
        }

        #endregion Methods
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