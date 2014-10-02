
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	class DisplayOnlyTextBox : TextBox
	{
		private static readonly Color defaultBackColor = Color.Transparent;

		public DisplayOnlyTextBox() : base()
		{
			base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			base.BackColor = defaultBackColor;
			base.BorderStyle = System.Windows.Forms.BorderStyle.None;
			base.ReadOnly = true;
			base.TabStop = false;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			this.SetBounds(0, 0, TextRenderer.MeasureText(this.Text, this.Font).Width, 0, BoundsSpecified.Width);
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Drawing.Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		internal new void ResetBackColor()
		{
			base.BackColor = defaultBackColor;
		}

		internal bool ShouldSerializeBackColor()
		{
			return base.BackColor == defaultBackColor;
		}

		[DefaultValue(System.Windows.Forms.BorderStyle.None), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new System.Windows.Forms.BorderStyle BorderStyle
		{
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[DefaultValue(true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool ReadOnly
		{
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		[DefaultValue(false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}
	}
}
