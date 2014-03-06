using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Infragistics.Windows.Themes



{
	#region WashGroup class

	/// <summary>
	/// Defines the color to use to wash a group of resources
	/// </summary>
	/// <seealso cref="WashGroupCollection"/>
	/// <seealso cref="ResourceWasher"/>
	/// <seealso cref="ResourceWasher.WashGroupProperty"/>
	/// <seealso cref="ResourceWasher.WashGroups"/>
	public class WashGroup
	{
		#region Private Members

		private string _name;
		// AS 12/18/07 Changed to nullable
		//private Color _washColor = Colors.Transparent;
		private Color? _washColor = null;
		private float _hue;
		private float _saturation;
		// JJD 10/23/07
		// Added WashMode property 
		private WashMode? _washMode = null;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public WashGroup()
		{
		}

		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region Name

		/// <summary>
		/// Gets/sets the name of the group.
		/// </summary>
		/// <remarks>
		/// <para class="note">
		/// <b>Note:</b> Any brushes found in the source dictionary whose <see cref="ResourceWasher.WashGroupProperty"/> attached property mathes this name will be washed with the specified WashColor.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResourceWasher.SetWashGroup"/>
		/// <seealso cref="ResourceWasher.WashGroups"/>
		[DefaultValue(null)]
		public string Name
		{
			get { return this._name; }
			set { this._name = value; }
		}

		#endregion //Name

		#region WashColor

		/// <summary>
		/// Gets/sets the color to use to wash the resources in this group.
		/// </summary>
		/// <remarks>
		/// <para class="body">The color to use to wash any resources whose <see cref="ResourceWasher.WashGroupProperty"/> matches this group's <see cref="Name"/>.</para>
		/// <para class="note"><b>Note:</b> if this property is left to its default value of <b>Colors.Transparent</b> then the resources in this group will not be cloned and washed. Instead they will be copied over without cloning or modification.</para>
		/// </remarks>
		/// <seealso cref="ResourceWasher.SetWashGroup"/>
		/// <seealso cref="ResourceWasher.WashGroups"/>
		// AS 12/18/07 Changed to nullable
		//public Color WashColor

		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<Color>))] // AS 5/15/08 BR32816

		public Color? WashColor
		{
			get { return this._washColor; }
			set
			{
				this._washColor = value;

				// AS 12/18/07 Changed to nullable
				//this._hue = ResourceWasher.GetHue(value);
				//this._saturation = ResourceWasher.GetSaturation(value);
				Color hueSatColor = value ?? Colors.Transparent;
				this._hue = ResourceWasher.GetHue(hueSatColor);
				this._saturation = ResourceWasher.GetSaturation(hueSatColor);
			}
		}

		/// <summary>
		/// Determines if the <see cref="WashColor"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		// AS 12/18/07 Changed to nullable
		//public bool ShouldSerializeWashColor() { return this._washColor != Colors.Transparent; }
		public bool ShouldSerializeWashColor() { return this._washColor != null; }

		/// <summary>
		/// Resets the <see cref="WashColor"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashColor()
		{
			// AS 12/18/07 Changed to nullable
			//this._washColor = Colors.Transparent;
			this._washColor = null;
		}

		#endregion //WashColor

		// JJD 10/23/07
		// Added WashMode property 
		#region WashMode

		/// <summary>
		/// Gets/sets the method used to wash colors in the resources in the SourceDictionary in this group.
		/// </summary>
		/// <seealso cref="ResourceWasher.WashMode"/>

		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<WashMode>))] // AS 5/15/08 BR32816



		public WashMode? WashMode
		{
			get { return this._washMode; }
			set
			{
				this._washMode = value;
			}
		}

		/// <summary>
		/// Determines if the <see cref="WashMode"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWashMode() { return this._washMode != null; }

		/// <summary>
		/// Resets the <see cref="WashMode"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashMode()
		{
			this._washMode = null;
		}

		#endregion //WashMode

		#endregion //Public Properties

		#region Internal Properties

		internal float Hue { get { return this._hue; } }
		internal float Saturation { get { return this._saturation; } }

		#endregion //Internal Properties

		#endregion //Properties
	}

	#endregion //WashGroup class

	#region WashGroupCollection class

	/// <summary>
	/// A collection of groups used by the ResourceWasher for washing colors 
	/// </summary>
	/// <seealso cref="WashGroup"/>
	/// <seealso cref="ResourceWasher"/>
	/// <seealso cref="ResourceWasher.WashGroupProperty"/>
	/// <seealso cref="ResourceWasher.WashGroups"/>
	public class WashGroupCollection : ObservableCollection<WashGroup>
	{
	}

	#endregion //WashGroupCollection class	
    
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